#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxTrackingManager
    {
        private readonly string _projectToken;
        private readonly bool _flushEventsImmediatelyOnFirstLaunch;
        private readonly bool _sessionTrackingEnabled;
        private readonly IAttriaxConsentReadView _consent;
        private readonly AttriaxRuntimeState _runtimeState;
        private readonly Func<bool> _shouldDispatchAnalytics;
        private readonly AttriaxSessionManager _sessionManager;
        private readonly AttriaxRequestManager _requestManager;
        private readonly AttriaxSkanManager _skanManager;
        private readonly Action<bool> _requestFlush;
        private readonly Action<string, string?> _debugLog;

        public AttriaxTrackingManager(
            string projectToken,
            bool flushEventsImmediatelyOnFirstLaunch,
            bool sessionTrackingEnabled,
            IAttriaxConsentReadView consent,
            AttriaxRuntimeState runtimeState,
            Func<bool> shouldDispatchAnalytics,
            AttriaxSessionManager sessionManager,
            AttriaxRequestManager requestManager,
            AttriaxSkanManager skanManager,
            Action<bool> requestFlush,
            Action<string, string?> debugLog)
        {
            _projectToken = projectToken;
            _flushEventsImmediatelyOnFirstLaunch = flushEventsImmediatelyOnFirstLaunch;
            _sessionTrackingEnabled = sessionTrackingEnabled;
            _consent = consent;
            _runtimeState = runtimeState;
            _shouldDispatchAnalytics = shouldDispatchAnalytics;
            _sessionManager = sessionManager;
            _requestManager = requestManager;
            _skanManager = skanManager;
            _requestFlush = requestFlush;
            _debugLog = debugLog;
        }

        public async Task TrackEventAsync(string eventName, AttriaxTrackEventOptions options)
        {
            if (!_runtimeState.IsEnabled || !_runtimeState.AreEventsEnabled)
            {
                _debugLog("Skipping event because tracking is disabled.", eventName);
                return;
            }

            if (!ShouldDispatchAnalytics)
            {
                _debugLog(
                    "Queueing event locally because analytics dispatch is currently disabled.",
                    eventName);
            }

            var decision = TrackingDecisionFor(IsAdEventName(eventName)
                ? AttriaxTrackingSignal.AdEvents
                : AttriaxTrackingSignal.Analytics);
            if (!decision.Capture)
            {
                _debugLog("Skipping event because GDPR consent blocked capture.", eventName);
                return;
            }

            var occurredAt = DateTimeOffset.UtcNow;
            var session = ShouldTrackSessionActivity
                ? _sessionManager.PrepareTrackedActivity(occurredAt).CurrentSession
                : null;

            await _requestManager.Enqueue(
                    AttriaxQueuedRequest.CreateEvent(
                        AttriaxGeneratedRequestFactory.BuildTrackEventRequest(
                            _projectToken,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceId : null,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceIdSource : null,
                            eventName,
                            options,
                            session,
                            occurredAt)))
                .ConfigureAwait(false);
            await _skanManager.HandleTrackedEventAsync(eventName, options.EventData)
                .ConfigureAwait(false);
                        _requestFlush(ShouldFlushEventImmediately(options.FlushImmediately ?? false));
        }

        public Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions options)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            return RecordCrashAsync(
                string.IsNullOrWhiteSpace(error.GetType().Name) ? "Exception" : error.GetType().Name,
                string.IsNullOrWhiteSpace(error.Message) ? error.ToString() : error.Message,
                string.IsNullOrWhiteSpace(error.StackTrace) ? error.ToString() : error.StackTrace,
                options);
        }

        public async Task RecordCrashAsync(
            string exceptionType,
            string message,
            string stackTrace,
            AttriaxRecordErrorOptions options)
        {
            if (!_runtimeState.IsEnabled)
            {
                _debugLog("Skipping crash report because SDK is disabled.", exceptionType);
                return;
            }

            if (!ShouldDispatchAnalytics)
            {
                _debugLog(
                    "Skipping crash report because analytics dispatch is currently disabled.",
                    exceptionType);
                return;
            }

            var decision = TrackingDecisionFor(AttriaxTrackingSignal.Analytics);
            if (!decision.Capture)
            {
                _debugLog("Skipping crash report because GDPR consent blocked capture.", exceptionType);
                return;
            }

            var occurredAt = options.OccurredAt ?? DateTimeOffset.UtcNow;
            var session = ShouldTrackSessionActivity
                ? _sessionManager.PrepareTrackedActivity(occurredAt).CurrentSession
                : null;
            var snapshot = _sessionManager.ContextSnapshot;

            await _requestManager.Enqueue(
                    AttriaxQueuedRequest.CreateCrash(
                        AttriaxGeneratedRequestFactory.BuildTrackCrashRequest(
                            _projectToken,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceId : null,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceIdSource : null,
                            snapshot,
                            session,
                            string.IsNullOrWhiteSpace(options.Source) ? "manual" : options.Source,
                            options.IsFatal,
                            string.IsNullOrWhiteSpace(exceptionType) ? "Exception" : exceptionType,
                            string.IsNullOrWhiteSpace(message) ? "Unhandled error" : message,
                            string.IsNullOrWhiteSpace(stackTrace)
                                ? (string.IsNullOrWhiteSpace(message) ? "Unhandled error" : message)
                                : stackTrace,
                            options.Reason,
                            MergeCrashMetadata(snapshot.Device.Metadata, options.Metadata),
                            occurredAt)))
                .ConfigureAwait(false);
                        _requestFlush(true);
        }

        public async Task RecordNotificationAsync(
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxRecordNotificationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!_runtimeState.IsEnabled || !_runtimeState.AreEventsEnabled)
            {
                _debugLog("Skipping notification because tracking is disabled.", notificationId);
                return;
            }

            if (!ShouldDispatchAnalytics)
            {
                _debugLog(
                    "Queueing notification locally because analytics dispatch is currently disabled.",
                    notificationId);
            }

            var decision = TrackingDecisionFor(AttriaxTrackingSignal.Analytics);
            if (!decision.Capture)
            {
                _debugLog("Skipping notification because GDPR consent blocked capture.", notificationId);
                return;
            }

            var normalizedNotificationId = notificationId?.Trim();
            if (string.IsNullOrEmpty(normalizedNotificationId))
            {
                throw new ArgumentException("notificationId must not be empty.", nameof(notificationId));
            }

            var occurredAt = DateTimeOffset.UtcNow;
            var session = ShouldTrackSessionActivity
                ? _sessionManager.PrepareTrackedActivity(occurredAt).CurrentSession
                : null;
            var snapshot = _sessionManager.ContextSnapshot;

            var source = options.Source ?? InferNotificationSource(options.Payload);
            var metadata = MergeNotificationMetadata(options.Metadata, options.Payload);

            await _requestManager.Enqueue(
                    AttriaxQueuedRequest.CreateNotification(
                        AttriaxGeneratedRequestFactory.BuildTrackNotificationRequest(
                            _projectToken,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceId : null,
                            decision.AttachDeviceIdentity ? _runtimeState.DeviceIdSource : null,
                            type,
                            normalizedNotificationId,
                            snapshot.Platform,
                            options.LinkId,
                            options.CampaignId,
                            options.Title,
                            source,
                            session?.Id,
                            metadata,
                            occurredAt)))
                .ConfigureAwait(false);
            _requestFlush(ShouldFlushEventImmediately(options.FlushImmediately ?? false));
        }

        public Task TrackPageViewAsync(string pageName, AttriaxPageViewOptions options)
        {
            var eventData = new Dictionary<string, object>();
            eventData["pageName"] = pageName;

            if (!string.IsNullOrWhiteSpace(options.PageClass))
            {
                eventData["pageClass"] = options.PageClass;
            }

            if (!string.IsNullOrWhiteSpace(options.PageTitle))
            {
                eventData["pageTitle"] = options.PageTitle;
            }

            if (!string.IsNullOrWhiteSpace(options.PreviousPageName))
            {
                eventData["previousPageName"] = options.PreviousPageName;
            }

            if (options.Parameters != null && options.Parameters.Count > 0)
            {
                eventData["parameters"] = options.Parameters;
            }

            eventData["source"] = string.IsNullOrWhiteSpace(options.Source) ? "manual" : options.Source;

            return TrackEventAsync("page_view", new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately,
            });
        }

        public Task IdentifyAsync(string? userId, AttriaxIdentifyOptions options)
        {
            return SetUserAsync(userId, options);
        }

        public Task SetUserAsync(string? userId, AttriaxSetUserOptions options)
        {
            return QueueUserUpdateAsync(
                userId,
                AttriaxUserPropertySanitizer.SanitizeSetUserOptions(options),
                clearExternalUser: string.IsNullOrWhiteSpace(userId));
        }

        public Task SetUserPropertyAsync(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("User property name is required.", nameof(name));
            }

            if (value == null)
            {
                return ClearUserPropertiesAsync(new[] { name.Trim() });
            }

            return SetUserPropertiesAsync(new Dictionary<string, object?>
            {
                [name.Trim()] = value,
            });
        }

        public Task SetUserPropertiesAsync(IDictionary<string, object?> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var sanitizedUpdate = AttriaxUserPropertySanitizer.SanitizeUpdate(properties);
            if (sanitizedUpdate.IsEmpty)
            {
                return Task.CompletedTask;
            }

            return QueueUserUpdateAsync(
                null,
                new AttriaxSetUserOptions
                {
                    Properties = sanitizedUpdate.Properties,
                    ClearPropertyKeys = sanitizedUpdate.ClearPropertyKeys,
                },
                clearExternalUser: false);
        }

        public Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null)
        {
            var normalizedPropertyNames = AttriaxUserPropertySanitizer.NormalizePropertyKeys(propertyNames);
            return QueueUserUpdateAsync(
                null,
                new AttriaxSetUserOptions
                {
                    ClearAllProperties = normalizedPropertyNames == null || normalizedPropertyNames.Count == 0,
                    ClearPropertyKeys = normalizedPropertyNames,
                },
                clearExternalUser: false);
        }

        private async Task QueueUserUpdateAsync(
            string? userId,
            AttriaxSetUserOptions options,
            bool clearExternalUser)
        {
            if (!_runtimeState.IsEnabled)
            {
                _debugLog("Skipping user update because SDK is disabled.", null);
                return;
            }

            if (!ShouldDispatchAnalytics)
            {
                _debugLog(
                    "Skipping user update because analytics dispatch is currently disabled.",
                    userId);
                return;
            }

            if (!TrackingDecisionFor(AttriaxTrackingSignal.Attribution).Capture)
            {
                _debugLog("Skipping user update because GDPR consent blocked capture.", userId);
                return;
            }

            if (ShouldTrackSessionActivity)
            {
                _sessionManager.PrepareTrackedActivity(DateTimeOffset.UtcNow);
            }

            await _requestManager.Enqueue(
                    AttriaxQueuedRequest.CreateUser(
                        AttriaxGeneratedRequestFactory.BuildUserRequest(
                            _projectToken,
                            _runtimeState.DeviceId,
                            _runtimeState.DeviceIdSource,
                            userId,
                            options,
                            clearExternalUser)))
                .ConfigureAwait(false);
            _requestFlush(true);
        }

        private AttriaxTrackingDecision TrackingDecisionFor(AttriaxTrackingSignal signal)
        {
            return _consent.TrackingDecisionFor(signal);
        }

        private bool ShouldFlushEventImmediately(bool flushImmediately)
        {
            if (flushImmediately)
            {
                return true;
            }

            return _flushEventsImmediatelyOnFirstLaunch && _runtimeState.IsFirstLaunch;
        }

        private bool ShouldTrackSessionActivity =>
            ShouldDispatchAnalytics &&
            _sessionTrackingEnabled &&
            TrackingDecisionFor(AttriaxTrackingSignal.Session).Capture;

        private bool ShouldDispatchAnalytics => _shouldDispatchAnalytics();

        private static bool IsAdEventName(string eventName)
        {
            switch (eventName)
            {
                case "ad_request":
                case "ad_load":
                case "ad_load_failed":
                case "ad_show":
                case "ad_show_failed":
                case "ad_impression":
                case "ad_click":
                case "ad_dismiss":
                case "ad_reward":
                case "ad_revenue":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Preserves the raw FCM/APNs payload under a <c>payload</c> key inside the
        /// notification metadata so attribution context survives the trip to the
        /// server. Explicit metadata entries take precedence. Mirrors Flutter's
        /// <c>_mergeNotificationMetadata</c>.
        /// </summary>
        private static IDictionary<string, object>? MergeNotificationMetadata(
            IDictionary<string, object>? metadata,
            IDictionary<string, object>? payload)
        {
            var hasPayload = payload != null && payload.Count > 0;
            var hasMetadata = metadata != null && metadata.Count > 0;
            if (!hasPayload && !hasMetadata)
            {
                return metadata;
            }

            var merged = new Dictionary<string, object>();
            if (hasPayload)
            {
                merged["payload"] = new Dictionary<string, object>(payload);
            }

            if (hasMetadata)
            {
                foreach (var pair in metadata)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            return merged;
        }

        /// <summary>
        /// Best-effort inference of the delivery channel from a raw FCM/APNs payload.
        /// APNs payloads carry an <c>aps</c> envelope; FCM payloads carry a
        /// <c>google.message_id</c> / <c>gcm.message_id</c> (or any <c>google.</c> /
        /// <c>gcm.</c> prefixed key). Returns null when undecidable so the server can
        /// fall back to <c>other</c>. Mirrors Flutter's <c>_inferNotificationSource</c>.
        /// </summary>
        private static AttriaxNotificationEventSource? InferNotificationSource(
            IDictionary<string, object>? payload)
        {
            if (payload == null || payload.Count == 0)
            {
                return null;
            }

            if (payload.ContainsKey("aps"))
            {
                return AttriaxNotificationEventSource.Apns;
            }

            foreach (var key in payload.Keys)
            {
                if (key == "google.message_id" ||
                    key == "gcm.message_id" ||
                    key.StartsWith("google.", StringComparison.Ordinal) ||
                    key.StartsWith("gcm.", StringComparison.Ordinal))
                {
                    return AttriaxNotificationEventSource.Fcm;
                }
            }

            return null;
        }

        private static IDictionary<string, object>? MergeCrashMetadata(
            IDictionary<string, object>? baseMetadata,
            IDictionary<string, object>? extraMetadata)
        {
            if ((baseMetadata == null || baseMetadata.Count == 0) &&
                (extraMetadata == null || extraMetadata.Count == 0))
            {
                return null;
            }

            var merged = new Dictionary<string, object>();

            if (baseMetadata != null)
            {
                foreach (var pair in baseMetadata)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            if (extraMetadata != null)
            {
                foreach (var pair in extraMetadata)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            return merged;
        }
    }
}
