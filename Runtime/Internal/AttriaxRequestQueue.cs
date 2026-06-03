#nullable enable
using System;
using System.Collections.Generic;
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
        private readonly Action<string, string?> _debugLog;
        private readonly List<AttriaxQueuedRequest> _entries;
        private readonly Dictionary<string, PendingRequest> _pendingRequests = new Dictionary<string, PendingRequest>();

        public AttriaxRequestQueue(string storageKey, int maxQueueSize, Action<string, string?> debugLog)
        {
            _storageKey = storageKey;
            _maxQueueSize = maxQueueSize;
            _debugLog = debugLog;
            _entries = ReadQueue();
        }

        public int Count => _entries.Count;

        public Task<object> Enqueue(AttriaxQueuedRequest request)
        {
            _debugLog(
                "Enqueuing request.",
                DescribeRequest(request) + ", queueCountBefore=" + _entries.Count);
            _entries.Add(request);
            while (_entries.Count > _maxQueueSize)
            {
                var dropped = _entries[0];
                _entries.RemoveAt(0);
                Reject(dropped.Id, new AttriaxApiError(
                    "Attriax queue entry was dropped because the queue reached capacity.",
                    null,
                    false,
                    true));
            }

            WriteQueue();

            var pending = new PendingRequest();
            _pendingRequests[request.Id] = pending;
            _debugLog(
                "Enqueued request.",
                DescribeRequest(request) + ", queueCountAfter=" + _entries.Count);
            return pending.Task;
        }

        public AttriaxQueuedRequest Peek()
        {
            if (_entries.Count == 0)
            {
                throw new InvalidOperationException("The Attriax queue is empty.");
            }

            return _entries[0];
        }

        public AttriaxQueuedRequest PeekAt(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _entries[index];
        }

        public bool HasPendingOpen()
        {
            return _entries.Exists(entry => entry.Kind == AttriaxQueuedRequestKind.Open);
        }

        public void PrioritizeOpenRequests()
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

            var changed = false;
            for (var index = 0; index < openEntries.Count; index += 1)
            {
                if (!string.Equals(_entries[index].Id, openEntries[index].Id, StringComparison.Ordinal))
                {
                    changed = true;
                    break;
                }
            }

            if (!changed)
            {
                return;
            }

            _entries.Clear();
            _entries.AddRange(openEntries);
            _entries.AddRange(otherEntries);
            WriteQueue();
            _debugLog(
                "Reordered queue to prioritize open requests.",
                "openCount=" + openEntries.Count + ", otherCount=" + otherEntries.Count);
        }

        public List<AttriaxQueuedRequest> PeekBatchablePrefix()
        {
            return PeekBatchablePrefix(0);
        }

        public List<AttriaxQueuedRequest> PeekBatchablePrefix(int startIndex)
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
            if (_entries.Count == 0 || count <= 0)
            {
                return;
            }

            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var removeCount = Math.Min(count, _entries.Count - index);
            _debugLog(
                "Removing queued request range.",
                "index=" + index + ", count=" + removeCount + ", queueCountBefore=" + _entries.Count);
            _entries.RemoveRange(index, removeCount);
            WriteQueue();
        }

        public void ReplaceAt(int index, AttriaxQueuedRequest entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _debugLog(
                "Replacing queued request entry.",
                "index=" + index + ", request=" + DescribeRequest(entry));
            _entries[index] = entry;
            WriteQueue();
        }

        public DateTimeOffset? PeekEarliestRetryAt()
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

        public void Complete(string id, object value)
        {
            if (!_pendingRequests.TryGetValue(id, out var pending))
            {
                _debugLog("Skipping queue completion because no pending request was found.", "id=" + id);
                return;
            }

            _pendingRequests.Remove(id);
            _debugLog("Completing queued request.", "id=" + id);
            pending.Resolve(value);
        }

        public void Reject(string id, Exception error)
        {
            if (!_pendingRequests.TryGetValue(id, out var pending))
            {
                _debugLog("Skipping queue rejection because no pending request was found.", "id=" + id);
                return;
            }

            _pendingRequests.Remove(id);
            _debugLog(
                "Rejecting queued request.",
                "id=" + id + ", error=" + error.Message);
            pending.Reject(error);
        }

        public void RejectAll(Exception error)
        {
            foreach (var pending in _pendingRequests.Values)
            {
                pending.Reject(error);
            }

            _pendingRequests.Clear();
        }

        public void Clear(Exception error)
        {
            RejectAll(error);
            _entries.Clear();
            AttriaxPlayerPrefs.DeleteKey(_storageKey);
            AttriaxPlayerPrefs.Save();
        }

        public void DiscardWhere(Predicate<AttriaxQueuedRequest> predicate, Exception error)
        {
            var changed = false;
            for (var index = _entries.Count - 1; index >= 0; index -= 1)
            {
                var entry = _entries[index];
                if (!predicate(entry))
                {
                    continue;
                }

                _entries.RemoveAt(index);
                Reject(entry.Id, error);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            WriteQueue();
        }

        public int RewriteWhere(
            Predicate<AttriaxQueuedRequest> predicate,
            Action<AttriaxQueuedRequest> rewrite)
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
                WriteQueue();
            }

            return changed;
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
                    _debugLog("Discarding queue payload with unsupported schema version.", null);
                    return new List<AttriaxQueuedRequest>();
                }

                return envelope.Entries;
            }
            catch (Exception error)
            {
                _debugLog("Failed to parse queue from storage. Resetting the persisted queue.", error.Message);
                return new List<AttriaxQueuedRequest>();
            }
        }

        private void WriteQueue()
        {
            var serialized = JsonConvert.SerializeObject(new QueueEnvelope
            {
                Version = QueueSchemaVersion,
                Entries = _entries,
            });
            AttriaxPlayerPrefs.SetString(_storageKey, serialized);
            AttriaxPlayerPrefs.Save();
        }

        private static string DescribeRequest(AttriaxQueuedRequest request)
        {
            return "id=" + request.Id
                + ", kind=" + request.Kind
                + ", attempt=" + request.AttemptCount
                + ", nextRetryAt=" + (request.NextRetryAt.HasValue ? request.NextRetryAt.Value.ToString("O") : "null");
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