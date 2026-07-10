#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Immutable snapshot of the GDPR consent gate. Mirrors the Flutter
    /// <c>consent/attriax_consent_policy.dart</c> module: it answers the capture,
    /// identity, network-deferral, and runtime-persistence questions from a fixed
    /// (gdprEnabled, state, values, anonymousTrackingEnabled) tuple.
    /// </summary>
    internal readonly struct AttriaxConsentPolicy
    {
        private readonly bool _gdprEnabled;
        private readonly AttriaxGdprConsentState _state;
        private readonly AttriaxGdprConsentValues? _values;
        private readonly bool _anonymousTrackingEnabled;

        public AttriaxConsentPolicy(
            bool gdprEnabled,
            AttriaxGdprConsentState state,
            AttriaxGdprConsentValues? values,
            bool anonymousTrackingEnabled)
        {
            _gdprEnabled = gdprEnabled;
            _state = state;
            _values = values;
            _anonymousTrackingEnabled = anonymousTrackingEnabled;
        }

        public bool IsWaitingForGdprConsent =>
            _state == AttriaxGdprConsentState.Pending ||
            _state == AttriaxGdprConsentState.Unknown;

        public bool ShouldDeferNetworkDispatch =>
            _gdprEnabled && IsWaitingForGdprConsent && !_anonymousTrackingEnabled;

        /// <summary>
        /// Whether runtime-scoped data may be persisted to disk under the current
        /// consent. Allowed when GDPR is off, the region does not require consent,
        /// or the user granted at least one tracking category. Anything else keeps
        /// the store in consent-only (memory-backed) mode until a category is granted.
        /// </summary>
        public bool AllowsRuntimePersistence
        {
            get
            {
                if (!_gdprEnabled)
                {
                    return true;
                }

                if (_state == AttriaxGdprConsentState.NotRequired)
                {
                    return true;
                }

                return _state == AttriaxGdprConsentState.Granted &&
                    _values != null &&
                    (_values.Analytics || _values.Attribution || _values.AdEvents);
            }
        }

        /// <summary>
        /// Strict identity gate: may this category be tracked with the device
        /// identity? Anonymous tracking does not relax a category the user
        /// explicitly declined under granted consent.
        /// </summary>
        public bool AllowsCategory(Func<AttriaxGdprConsentValues, bool> selector)
        {
            if (!_gdprEnabled)
            {
                return true;
            }

            switch (_state)
            {
                case AttriaxGdprConsentState.NotRequired:
                    return true;
                case AttriaxGdprConsentState.Granted:
                    return _values != null && selector(_values);
                case AttriaxGdprConsentState.Pending:
                case AttriaxGdprConsentState.Unknown:
                    return false;
                default:
                    return false;
            }
        }

        public AttriaxTrackingDecision TrackingDecisionFor(AttriaxTrackingSignal signal)
        {
            if (!_gdprEnabled)
            {
                return new AttriaxTrackingDecision(true, AttriaxTrackingIdentityMode.Identified, false);
            }

            if (IsWaitingForGdprConsent)
            {
                return new AttriaxTrackingDecision(
                    IsAnonymousCapableSignal(signal),
                    AttriaxTrackingIdentityMode.Anonymous,
                    !_anonymousTrackingEnabled);
            }

            if (_state == AttriaxGdprConsentState.NotRequired)
            {
                return new AttriaxTrackingDecision(true, AttriaxTrackingIdentityMode.Identified, false);
            }

            if (_state != AttriaxGdprConsentState.Granted || _values == null)
            {
                return new AttriaxTrackingDecision(false, AttriaxTrackingIdentityMode.Withheld, false);
            }

            if (IsSignalGranted(signal, _values))
            {
                return new AttriaxTrackingDecision(true, AttriaxTrackingIdentityMode.Identified, false);
            }

            if (_anonymousTrackingEnabled && IsAnonymousCapableSignal(signal))
            {
                return new AttriaxTrackingDecision(true, AttriaxTrackingIdentityMode.Anonymous, false);
            }

            return new AttriaxTrackingDecision(false, AttriaxTrackingIdentityMode.Withheld, false);
        }

        public static bool IsAnonymousCapableSignal(AttriaxTrackingSignal signal)
        {
            switch (signal)
            {
                case AttriaxTrackingSignal.Analytics:
                case AttriaxTrackingSignal.AdEvents:
                case AttriaxTrackingSignal.Session:
                case AttriaxTrackingSignal.DeepLink:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSignalGranted(AttriaxTrackingSignal signal, AttriaxGdprConsentValues values)
        {
            switch (signal)
            {
                case AttriaxTrackingSignal.Analytics:
                    return values.Analytics;
                case AttriaxTrackingSignal.AdEvents:
                    return values.AdEvents;
                case AttriaxTrackingSignal.Session:
                    return values.Analytics || values.AdEvents;
                case AttriaxTrackingSignal.DeepLink:
                case AttriaxTrackingSignal.Attribution:
                case AttriaxTrackingSignal.UninstallTracking:
                    return values.Attribution;
                default:
                    return false;
            }
        }
    }
}
