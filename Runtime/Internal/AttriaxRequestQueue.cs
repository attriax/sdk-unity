#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SdkEventDto = Attriax.Unity.Generated.Model.SdkEventDto;
using SdkRegisterUninstallTokenDto = Attriax.Unity.Generated.Model.SdkRegisterUninstallTokenDto;
using SdkSessionDto = Attriax.Unity.Generated.Model.SdkSessionDto;
using SdkUserDto = Attriax.Unity.Generated.Model.SdkUserDto;
using SdkV1DeepLinkResolveDto = Attriax.Unity.Generated.Model.SdkV1DeepLinkResolveDto;
using SdkV1OpenDto = Attriax.Unity.Generated.Model.SdkV1OpenDto;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxRequestQueue
    {
        private const int QueueSchemaVersion = 4;

        private readonly string _storageKey;
        private readonly int _maxQueueSize;
        private readonly object _gate = new object();
        private readonly List<AttriaxQueuedRequest> _entries;
        private readonly Dictionary<string, PendingRequest> _pendingRequests = new Dictionary<string, PendingRequest>();
        private string _pendingSerializedQueue = string.Empty;
        private int _pendingQueueWriteRequested;

        public AttriaxRequestQueue(string storageKey, int maxQueueSize)
        {
            _storageKey = storageKey;
            _maxQueueSize = maxQueueSize;
            _entries = ReadQueue();
        }

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _entries.Count;
                }
            }
        }

        public Task<object> Enqueue(AttriaxQueuedRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var pending = new PendingRequest();
            List<PendingRequest>? droppedPendings = null;
            lock (_gate)
            {
                _entries.Add(request);
                while (_entries.Count > _maxQueueSize)
                {
                    var dropped = _entries[0];
                    _entries.RemoveAt(0);
                    if (_pendingRequests.TryGetValue(dropped.Id, out var droppedPending))
                    {
                        _pendingRequests.Remove(dropped.Id);
                        droppedPendings ??= new List<PendingRequest>();
                        droppedPendings.Add(droppedPending);
                    }
                }

                WriteQueueUnderLock();
                _pendingRequests[request.Id] = pending;
            }

            if (droppedPendings != null)
            {
                foreach (var droppedPending in droppedPendings)
                {
                    droppedPending.Reject(new AttriaxApiError(
                        "Attriax queue entry was dropped because the queue reached capacity.",
                        null,
                        false,
                        true));
                }
            }

            return pending.Task;
        }

        public AttriaxQueuedRequest Peek()
        {
            lock (_gate)
            {
                if (_entries.Count == 0)
                {
                    throw new InvalidOperationException("The Attriax queue is empty.");
                }

                return _entries[0];
            }
        }

        public AttriaxQueuedRequest PeekAt(int index)
        {
            lock (_gate)
            {
                if (index < 0 || index >= _entries.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _entries[index];
            }
        }

        public bool HasPendingOpen()
        {
            lock (_gate)
            {
                return _entries.Exists(entry => entry.Kind == AttriaxQueuedRequestKind.Open);
            }
        }

        public void PrioritizeOpenRequests()
        {
            lock (_gate)
            {
                if (_entries.Count < 2)
                {
                    return;
                }

                var openEntries = new List<AttriaxQueuedRequest>();
                var otherEntries = new List<AttriaxQueuedRequest>();

                foreach (var entry in _entries)
                {
                    if (entry.Kind == AttriaxQueuedRequestKind.Open)
                    {
                        openEntries.Add(entry);
                    }
                    else
                    {
                        otherEntries.Add(entry);
                    }
                }

                if (openEntries.Count == 0 || otherEntries.Count == 0)
                {
                    return;
                }

                for (var index = 0; index < openEntries.Count; index += 1)
                {
                    if (!string.Equals(_entries[index].Id, openEntries[index].Id, StringComparison.Ordinal))
                    {
                        _entries.Clear();
                        _entries.AddRange(openEntries);
                        _entries.AddRange(otherEntries);
                        WriteQueueUnderLock();
                        return;
                    }
                }
            }
        }

        public List<AttriaxQueuedRequest> PeekBatchablePrefix()
        {
            return PeekBatchablePrefix(0);
        }

        public List<AttriaxQueuedRequest> PeekBatchablePrefix(int startIndex)
        {
            lock (_gate)
            {
                var entries = new List<AttriaxQueuedRequest>();
                AttriaxQueuedRequest? batchIdentityEntry = null;
                for (var index = startIndex; index < _entries.Count; index += 1)
                {
                    var entry = _entries[index];
                    if (!IsBatchable(entry))
                    {
                        break;
                    }

                    if (batchIdentityEntry != null && !CanShareBatchEnvelope(batchIdentityEntry, entry))
                    {
                        break;
                    }

                    batchIdentityEntry ??= entry;

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public void RemoveFirst()
        {
            RemoveFirst(1);
        }

        public void RemoveFirst(int count)
        {
            RemoveRange(0, count);
        }

        public void RemoveAt(int index)
        {
            RemoveRange(index, 1);
        }

        public void RemoveRange(int index, int count)
        {
            lock (_gate)
            {
                if (_entries.Count == 0 || count <= 0)
                {
                    return;
                }

                if (index < 0 || index >= _entries.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var removeCount = Math.Min(count, _entries.Count - index);
                _entries.RemoveRange(index, removeCount);
                WriteQueueUnderLock();
            }
        }

        public void ReplaceAt(int index, AttriaxQueuedRequest entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            lock (_gate)
            {
                if (index < 0 || index >= _entries.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                _entries[index] = entry;
                WriteQueueUnderLock();
            }
        }

        public DateTimeOffset? PeekEarliestRetryAt()
        {
            lock (_gate)
            {
                DateTimeOffset? earliestRetryAt = null;
                foreach (var entry in _entries)
                {
                    if (!entry.NextRetryAt.HasValue)
                    {
                        continue;
                    }

                    if (!earliestRetryAt.HasValue || entry.NextRetryAt.Value < earliestRetryAt.Value)
                    {
                        earliestRetryAt = entry.NextRetryAt.Value;
                    }
                }

                return earliestRetryAt;
            }
        }

        public void Complete(string id, object value)
        {
            PendingRequest? pending;
            lock (_gate)
            {
                if (!_pendingRequests.TryGetValue(id, out pending))
                {
                    return;
                }

                _pendingRequests.Remove(id);
            }

            pending.Resolve(value);
        }

        public void Reject(string id, Exception error)
        {
            PendingRequest? pending;
            lock (_gate)
            {
                if (!_pendingRequests.TryGetValue(id, out pending))
                {
                    return;
                }

                _pendingRequests.Remove(id);
            }

            pending.Reject(error);
        }

        public void RejectAll(Exception error)
        {
            List<PendingRequest> pendingRequests;
            lock (_gate)
            {
                pendingRequests = new List<PendingRequest>(_pendingRequests.Values);
                _pendingRequests.Clear();
            }

            foreach (var pending in pendingRequests)
            {
                pending.Reject(error);
            }
        }

        public void Clear(Exception error)
        {
            List<PendingRequest> pendingRequests;
            lock (_gate)
            {
                pendingRequests = new List<PendingRequest>(_pendingRequests.Values);
                _pendingRequests.Clear();
                _entries.Clear();
            }

            foreach (var pending in pendingRequests)
            {
                pending.Reject(error);
            }

            Interlocked.Exchange(ref _pendingSerializedQueue, null);
            Interlocked.Exchange(ref _pendingQueueWriteRequested, 0);
            AttriaxPlayerPrefs.DeleteKey(_storageKey);
            AttriaxPlayerPrefs.Save();
        }

        internal void FlushPendingWrite()
        {
            var serialized = Interlocked.Exchange(ref _pendingSerializedQueue, null);
            if (Interlocked.Exchange(ref _pendingQueueWriteRequested, 0) == 0 || serialized == null)
            {
                return;
            }

            AttriaxPlayerPrefs.SetString(_storageKey, serialized);
            AttriaxPlayerPrefs.Save();
        }

        public void DiscardWhere(Predicate<AttriaxQueuedRequest> predicate, Exception error)
        {
            List<PendingRequest>? rejectedPendings = null;
            var changed = false;
            lock (_gate)
            {
                for (var index = _entries.Count - 1; index >= 0; index -= 1)
                {
                    var entry = _entries[index];
                    if (!predicate(entry))
                    {
                        continue;
                    }

                    _entries.RemoveAt(index);
                    if (_pendingRequests.TryGetValue(entry.Id, out var pending))
                    {
                        _pendingRequests.Remove(entry.Id);
                        rejectedPendings ??= new List<PendingRequest>();
                        rejectedPendings.Add(pending);
                    }

                    changed = true;
                }

                if (changed)
                {
                    WriteQueueUnderLock();
                }
            }

            if (rejectedPendings == null)
            {
                return;
            }

            foreach (var pending in rejectedPendings)
            {
                pending.Reject(error);
            }
        }

        public int RewriteWhere(
            Predicate<AttriaxQueuedRequest> predicate,
            Action<AttriaxQueuedRequest> rewrite)
        {
            lock (_gate)
            {
                var changed = 0;
                foreach (var entry in _entries)
                {
                    if (!predicate(entry))
                    {
                        continue;
                    }

                    rewrite(entry);
                    changed += 1;
                }

                if (changed > 0)
                {
                    WriteQueueUnderLock();
                }

                return changed;
            }
        }

        private List<AttriaxQueuedRequest> ReadQueue()
        {
            var raw = AttriaxPlayerPrefs.GetString(_storageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<AttriaxQueuedRequest>();
            }

            try
            {
                var envelope = JsonConvert.DeserializeObject<QueueEnvelope>(raw);
                if (envelope == null || envelope.Version != QueueSchemaVersion || envelope.Entries == null)
                {
                    return new List<AttriaxQueuedRequest>();
                }

                return envelope.Entries;
            }
            catch (Exception error)
            {
                return new List<AttriaxQueuedRequest>();
            }
        }

        private void WriteQueueUnderLock()
        {
            var serialized = JsonConvert.SerializeObject(new QueueEnvelope
            {
                Version = QueueSchemaVersion,
                Entries = _entries,
            });
            Interlocked.Exchange(ref _pendingSerializedQueue, serialized);
            Interlocked.Exchange(ref _pendingQueueWriteRequested, 1);
        }

        [Serializable]
        private sealed class QueueEnvelope
        {
            public int Version { get; set; }

            public List<AttriaxQueuedRequest>? Entries { get; set; }
        }

        private sealed class PendingRequest
        {
            private readonly TaskCompletionSource<object> _source =
                new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            public Task<object> Task => _source.Task;

            public void Resolve(object value)
            {
                _source.TrySetResult(value);
            }

            public void Reject(Exception error)
            {
                _source.TrySetException(error);
            }
        }

        private static bool IsBatchable(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.User:
                    return true;
                case AttriaxQueuedRequestKind.Event:
                    return !string.IsNullOrWhiteSpace(entry.RequireEventRequest().deviceId);
                case AttriaxQueuedRequestKind.Session:
                    return !string.IsNullOrWhiteSpace(entry.RequireSessionRequest().deviceId);
                default:
                    return false;
            }
        }

        private static bool CanShareBatchEnvelope(AttriaxQueuedRequest left, AttriaxQueuedRequest right)
        {
            return string.Equals(GetBatchProjectToken(left), GetBatchProjectToken(right), StringComparison.Ordinal)
                && string.Equals(GetBatchDeviceId(left), GetBatchDeviceId(right), StringComparison.Ordinal)
                && string.Equals(GetBatchDeviceIdSource(left), GetBatchDeviceIdSource(right), StringComparison.Ordinal);
        }

        private static string GetBatchProjectToken(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return entry.RequireEventRequest().projectToken;
                case AttriaxQueuedRequestKind.Session:
                    return entry.RequireSessionRequest().projectToken;
                case AttriaxQueuedRequestKind.User:
                    return entry.RequireUserRequest().projectToken;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unsupported batch request kind.");
            }
        }

        private static string GetBatchDeviceId(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return entry.RequireEventRequest().deviceId;
                case AttriaxQueuedRequestKind.Session:
                    return entry.RequireSessionRequest().deviceId;
                case AttriaxQueuedRequestKind.User:
                    return entry.RequireUserRequest().deviceId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unsupported batch request kind.");
            }
        }

        private static string GetBatchDeviceIdSource(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return entry.RequireEventRequest().deviceIdSource;
                case AttriaxQueuedRequestKind.Session:
                    return entry.RequireSessionRequest().deviceIdSource;
                case AttriaxQueuedRequestKind.User:
                    return entry.RequireUserRequest().deviceIdSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unsupported batch request kind.");
            }
        }
    }

    internal enum AttriaxQueuedRequestKind
    {
        Open,
        Event,
        Crash,
        Session,
        User,
        DeepLinkResolve,
        UninstallToken,
    }

    [Serializable]
    internal sealed class AttriaxQueuedRequest
    {
        public string Id { get; set; } = string.Empty;

        public AttriaxQueuedRequestKind Kind { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int AttemptCount { get; set; }

        public DateTimeOffset? LastAttemptAt { get; set; }

        public string? LastErrorClass { get; set; }

        public int? LastHttpStatusCode { get; set; }

        public DateTimeOffset? NextRetryAt { get; set; }

        public SdkV1OpenDto? OpenRequest { get; set; }

        public SdkEventDto? EventRequest { get; set; }

        public AttriaxCrashRequest? CrashRequest { get; set; }

        public SdkSessionDto? SessionRequest { get; set; }

        [JsonProperty("identifyRequest")]
        public SdkUserDto? UserRequest { get; set; }

        public SdkV1DeepLinkResolveDto? DeepLinkResolveRequest { get; set; }

        public SdkRegisterUninstallTokenDto? UninstallTokenRequest { get; set; }

        public static AttriaxQueuedRequest CreateOpen(SdkV1OpenDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.Open,
                CreatedAt = DateTimeOffset.UtcNow,
                OpenRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateEvent(SdkEventDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.Event,
                CreatedAt = DateTimeOffset.UtcNow,
                EventRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateCrash(AttriaxCrashRequest request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.Crash,
                CreatedAt = DateTimeOffset.UtcNow,
                CrashRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateSession(SdkSessionDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.Session,
                CreatedAt = DateTimeOffset.UtcNow,
                SessionRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateUser(SdkUserDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.User,
                CreatedAt = DateTimeOffset.UtcNow,
                UserRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateDeepLinkResolve(SdkV1DeepLinkResolveDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.DeepLinkResolve,
                CreatedAt = DateTimeOffset.UtcNow,
                DeepLinkResolveRequest = request,
            };
        }

        public static AttriaxQueuedRequest CreateUninstallToken(SdkRegisterUninstallTokenDto request)
        {
            return new AttriaxQueuedRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = AttriaxQueuedRequestKind.UninstallToken,
                CreatedAt = DateTimeOffset.UtcNow,
                UninstallTokenRequest = request,
            };
        }

        public SdkV1OpenDto RequireOpenRequest()
        {
            return OpenRequest ?? throw MissingPayload("open");
        }

        public SdkEventDto RequireEventRequest()
        {
            return EventRequest ?? throw MissingPayload("event");
        }

        public AttriaxCrashRequest RequireCrashRequest()
        {
            return CrashRequest ?? throw MissingPayload("crash");
        }

        public SdkSessionDto RequireSessionRequest()
        {
            return SessionRequest ?? throw MissingPayload("session");
        }

        public SdkUserDto RequireUserRequest()
        {
            return UserRequest ?? throw MissingPayload("user");
        }

        public SdkV1DeepLinkResolveDto RequireDeepLinkResolveRequest()
        {
            return DeepLinkResolveRequest ?? throw MissingPayload("deep-link resolve");
        }

        public SdkRegisterUninstallTokenDto RequireUninstallTokenRequest()
        {
            return UninstallTokenRequest ?? throw MissingPayload("uninstall token");
        }

        private static AttriaxApiError MissingPayload(string label)
        {
            return new AttriaxApiError(
                string.Format("Queued {0} request payload is missing.", label),
                null,
                false,
                true);
        }
    }

    internal static class AttriaxQueueRetryPolicy
    {
        private const int MaxRetryAttempts = 8;
        private static readonly TimeSpan MaxRetryAge = TimeSpan.FromDays(7);

        public static bool IsWaitingForRetryWindow(AttriaxQueuedRequest request, DateTimeOffset now)
        {
            return request.NextRetryAt.HasValue && request.NextRetryAt.Value > now;
        }

        public static string? GetTerminalDropReason(AttriaxQueuedRequest request, DateTimeOffset now)
        {
            if (request.Kind == AttriaxQueuedRequestKind.DeepLinkResolve)
            {
                return null;
            }

            if (request.AttemptCount >= MaxRetryAttempts)
            {
                return "max_attempts_exceeded";
            }

            if (now - request.CreatedAt > MaxRetryAge)
            {
                return "max_age_exceeded";
            }

            return null;
        }

        public static AttriaxQueuedRequest MarkForRetry(
            AttriaxQueuedRequest request,
            AttriaxApiError error,
            DateTimeOffset attemptedAt,
            int defaultRetryDelayMs)
        {
            request.AttemptCount += 1;
            request.LastAttemptAt = attemptedAt;
            request.LastErrorClass = BuildRetryErrorClass(error);
            request.LastHttpStatusCode = error.StatusCode;
            request.NextRetryAt = ResolveRetryAt(error, attemptedAt, defaultRetryDelayMs);
            return request;
        }

        private static string BuildRetryErrorClass(AttriaxApiError error)
        {
            if (error.StatusCode.HasValue)
            {
                return "http_" + error.StatusCode.Value.ToString();
            }

            if (error.InnerException is TimeoutException)
            {
                return "timeout";
            }

            if (error.InnerException != null)
            {
                return error.InnerException.GetType().Name;
            }

            return error.GetType().Name;
        }

        private static DateTimeOffset ResolveRetryAt(
            AttriaxApiError error,
            DateTimeOffset attemptedAt,
            int defaultRetryDelayMs)
        {
            if (error.RetryAfterAt.HasValue && error.RetryAfterAt.Value > attemptedAt)
            {
                return error.RetryAfterAt.Value;
            }

            var retryDelayMs = defaultRetryDelayMs > 0
                ? defaultRetryDelayMs
                : 60000;
            return attemptedAt.AddMilliseconds(retryDelayMs);
        }
    }

    [Serializable]
    internal sealed class AttriaxCrashRequest
    {
        public string ProjectToken { get; set; } = string.Empty;

        public string? DeviceId { get; set; }

        public string? DeviceIdSource { get; set; }

        public string Platform { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public DateTimeOffset ClientOccurredAt { get; set; }

        public bool IsFatal { get; set; }

        public string ExceptionType { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? SessionId { get; set; }

        public decimal? SessionRelativeTimeMs { get; set; }

        public string? Locale { get; set; }

        public string? AppVersion { get; set; }

        public string? AppBuildNumber { get; set; }

        public string? AppPackageName { get; set; }

        public string SdkApiVersion { get; set; } = string.Empty;

        public string SdkPackageVersion { get; set; } = string.Empty;

        public bool IsFirstLaunch { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }
}