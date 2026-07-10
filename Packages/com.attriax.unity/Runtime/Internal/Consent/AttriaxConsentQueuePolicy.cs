#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Pure consent-gating decisions applied to queued requests once GDPR consent
    /// resolves. Mirrors the Flutter <c>consent/attriax_consent_queue_policy.dart</c>
    /// module: it answers which queued requests should be identified, anonymized,
    /// or dropped. The runtime applies the resulting mutations.
    /// </summary>
    internal sealed class AttriaxConsentQueuePolicy
    {
        private readonly Func<AttriaxTrackingSignal, AttriaxTrackingDecision> _trackingDecisionFor;
        private readonly Func<bool> _shouldMaterializeIdentifiedContext;
        private readonly Func<string, bool> _isAdEventName;

        public AttriaxConsentQueuePolicy(
            Func<AttriaxTrackingSignal, AttriaxTrackingDecision> trackingDecisionFor,
            Func<bool> shouldMaterializeIdentifiedContext,
            Func<string, bool> isAdEventName)
        {
            _trackingDecisionFor = trackingDecisionFor;
            _shouldMaterializeIdentifiedContext = shouldMaterializeIdentifiedContext;
            _isAdEventName = isAdEventName;
        }

        public AttriaxTrackingDecision TrackingDecisionForQueuedRequest(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return _trackingDecisionFor(
                        _isAdEventName(entry.RequireEventRequest().eventName)
                            ? AttriaxTrackingSignal.AdEvents
                            : AttriaxTrackingSignal.Analytics);
                case AttriaxQueuedRequestKind.Crash:
                    return _trackingDecisionFor(AttriaxTrackingSignal.Analytics);
                case AttriaxQueuedRequestKind.Notification:
                    return _trackingDecisionFor(AttriaxTrackingSignal.Analytics);
                case AttriaxQueuedRequestKind.Session:
                    return _trackingDecisionFor(AttriaxTrackingSignal.Session);
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                    return _trackingDecisionFor(AttriaxTrackingSignal.DeepLink);
                case AttriaxQueuedRequestKind.UninstallToken:
                    return _trackingDecisionFor(AttriaxTrackingSignal.UninstallTracking);
                case AttriaxQueuedRequestKind.User:
                case AttriaxQueuedRequestKind.Open:
                    return _trackingDecisionFor(AttriaxTrackingSignal.Attribution);
                default:
                    return new AttriaxTrackingDecision(false, AttriaxTrackingIdentityMode.Withheld, false);
            }
        }

        public bool IsRequestAllowedByResolvedConsent(AttriaxQueuedRequest entry)
        {
            return TrackingDecisionForQueuedRequest(entry).Capture;
        }

        public bool ShouldAnonymizeQueuedRequest(AttriaxQueuedRequest entry)
        {
            var decision = TrackingDecisionForQueuedRequest(entry);
            return decision.Capture && !decision.AttachDeviceIdentity && HasQueuedRequestDeviceId(entry);
        }

        public bool ShouldIdentifyQueuedRequestForResolvedConsent(AttriaxQueuedRequest entry)
        {
            if (!_shouldMaterializeIdentifiedContext())
            {
                return false;
            }

            var decision = TrackingDecisionForQueuedRequest(entry);
            if (!decision.Capture || !decision.AttachDeviceIdentity)
            {
                return false;
            }

            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return string.IsNullOrWhiteSpace(entry.RequireEventRequest().deviceId);
                case AttriaxQueuedRequestKind.Crash:
                    return string.IsNullOrWhiteSpace(entry.RequireCrashRequest().DeviceId);
                case AttriaxQueuedRequestKind.Notification:
                    return string.IsNullOrWhiteSpace(entry.RequireNotificationRequest().deviceId);
                case AttriaxQueuedRequestKind.Session:
                    return string.IsNullOrWhiteSpace(entry.RequireSessionRequest().deviceId);
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                    return string.IsNullOrWhiteSpace(entry.RequireDeepLinkResolveRequest().deviceId);
                default:
                    return false;
            }
        }

        private static bool HasQueuedRequestDeviceId(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    return !string.IsNullOrWhiteSpace(entry.RequireEventRequest().deviceId);
                case AttriaxQueuedRequestKind.Crash:
                    return !string.IsNullOrWhiteSpace(entry.RequireCrashRequest().DeviceId);
                case AttriaxQueuedRequestKind.Notification:
                    return !string.IsNullOrWhiteSpace(entry.RequireNotificationRequest().deviceId);
                case AttriaxQueuedRequestKind.Session:
                    return !string.IsNullOrWhiteSpace(entry.RequireSessionRequest().deviceId);
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                    return !string.IsNullOrWhiteSpace(entry.RequireDeepLinkResolveRequest().deviceId);
                default:
                    return false;
            }
        }
    }
}
