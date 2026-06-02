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
        private readonly AttriaxRequestQueue _requestQueue;
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
            AttriaxRequestQueue requestQueue,
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
            _requestQueue = requestQueue;
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
                    "Queueing event locally because Unity Editor validation mode does not dispatch analytics.",
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

            await _requestQueue.Enqueue(
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
                    "Skipping crash report because Unity Editor validation mode does not dispatch analytics.",
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

            await _requestQueue.Enqueue(
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
                    "Skipping user update because Unity Editor validation mode does not dispatch analytics.",
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

            await _requestQueue.Enqueue(
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
            (_consent.CanCaptureAnalytics || _consent.CanCaptureAdEvents);

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