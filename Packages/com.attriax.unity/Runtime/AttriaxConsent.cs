#nullable enable
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Regulation-scoped consent helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxConsent
    {
        internal AttriaxConsent(Internal.AttriaxRuntime runtime)
        {
            Gdpr = new AttriaxGdprConsent(runtime);
            Att = new AttriaxAttConsent(runtime);
        }

        /// <summary>
        /// GDPR-specific consent state and helpers.
        /// </summary>
        public AttriaxGdprConsent Gdpr { get; }

        /// <summary>
        /// Apple App Tracking Transparency helpers.
        /// </summary>
        public AttriaxAttConsent Att { get; }
    }

    /// <summary>
    /// Apple App Tracking Transparency helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxAttConsent
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxAttConsent(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Requests App Tracking Transparency authorization on supported Apple platforms.
        /// </summary>
        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(
            int? timeoutMs = null)
        {
            return _runtime.RequestTrackingAuthorizationAsync(timeoutMs);
        }

        /// <summary>
        /// Reads the current App Tracking Transparency authorization status.
        /// </summary>
        public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync()
        {
            return _runtime.GetTrackingAuthorizationStatusAsync();
        }
    }

    /// <summary>
    /// GDPR consent state and helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxGdprConsent
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxGdprConsent(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Current GDPR consent state retained for this device.
        /// </summary>
        public AttriaxGdprConsentState State
        {
            get { return _runtime.GdprConsentState; }
        }

        /// <summary>
        /// Current granted consent values, when one exists.
        /// </summary>
        public AttriaxGdprConsentValues? Values
        {
            get { return _runtime.GdprConsentValues; }
        }

        /// <summary>
        /// Returns <see langword="true"/> while the SDK is still waiting for an explicit GDPR decision.
        /// </summary>
        public bool IsWaitingForConsent
        {
            get { return _runtime.IsWaitingForGdprConsent; }
        }

        /// <summary>
        /// Checks whether this device needs an explicit GDPR decision.
        /// </summary>
        public Task<bool> NeedsConsentAsync(bool localOnly = false)
        {
            return _runtime.NeedsGdprConsentAsync(localOnly);
        }

        /// <summary>
        /// Grants GDPR consent with explicit category values.
        /// </summary>
        public void SetConsent(bool analytics, bool attribution, bool adEvents)
        {
            _runtime.SetGdprConsent(analytics, attribution, adEvents);
        }

        /// <summary>
        /// Marks GDPR consent as not required for this device.
        /// </summary>
        public void SetNotRequired()
        {
            _runtime.SetGdprConsentNotRequired();
        }

        /// <summary>
        /// Resets GDPR consent for this device back to a pending decision state.
        /// </summary>
        public void Reset()
        {
            _runtime.ResetGdprConsent();
        }

        /// <summary>
        /// Requests deletion of device-linked GDPR data on the Attriax backend.
        /// </summary>
        /// <remarks>
        /// On success, this also clears the local SDK state and returns this instance
        /// to the pre-initialization state.
        /// </remarks>
        public Task RequestDataErasureAsync()
        {
            return _runtime.RequestGdprDataErasureAsync();
        }
    }
}