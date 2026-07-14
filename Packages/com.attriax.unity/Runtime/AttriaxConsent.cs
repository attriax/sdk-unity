#nullable enable
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Regulation-scoped consent helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxConsent
    {
        internal AttriaxConsent(Internal.IAttriaxEngine runtime)
        {
            Gdpr = new AttriaxGdprConsent(runtime);
            Att = new AttriaxAttConsent(runtime);
            Ccpa = new AttriaxCcpaConsent(runtime);
        }

        /// <summary>
        /// GDPR-specific consent state and helpers.
        /// </summary>
        public AttriaxGdprConsent Gdpr { get; }

        /// <summary>
        /// Apple App Tracking Transparency helpers.
        /// </summary>
        public AttriaxAttConsent Att { get; }

        /// <summary>
        /// CCPA "do not sell / share" election and US-Privacy string helpers.
        /// </summary>
        public AttriaxCcpaConsent Ccpa { get; }
    }

    /// <summary>
    /// CCPA "do not sell / share" election and raw IAB US-Privacy string helpers exposed
    /// by the Unity SDK.
    /// </summary>
    /// <remarks>
    /// The election is seeded from <c>AttriaxConfig.DoNotSell</c> / <c>AttriaxConfig.UsPrivacy</c>
    /// and overridable at runtime here; a runtime change is reflected on the next
    /// app-open / identify request. A <see langword="null"/> do-not-sell and a
    /// <see langword="null"/>/blank US-Privacy string are omitted from the wire entirely;
    /// an explicit <see langword="false"/> do-not-sell is still emitted (it may clear a
    /// prior server-side latch). Mirrors the KMP <c>AttriaxCcpaConsent</c> and the Flutter
    /// <c>AttriaxCcpaConsent</c> wrapper. Getters/setters are asynchronous because the
    /// native engine owns the authoritative state behind the platform boundary.
    /// </remarks>
    public sealed class AttriaxCcpaConsent
    {
        private readonly Internal.IAttriaxEngine _runtime;

        internal AttriaxCcpaConsent(Internal.IAttriaxEngine runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Reads the current CCPA do-not-sell election, or <see langword="null"/> when
        /// unset (omitted from the wire).
        /// </summary>
        public Task<bool?> GetDoNotSellAsync()
        {
            return _runtime.GetCcpaDoNotSellAsync();
        }

        /// <summary>
        /// Reads the current raw IAB US-Privacy string, or <see langword="null"/> when
        /// unset/blank (omitted from the wire).
        /// </summary>
        public Task<string?> GetUsPrivacyAsync()
        {
            return _runtime.GetCcpaUsPrivacyAsync();
        }

        /// <summary>
        /// Sets both CCPA fields at once. An omitted (<see langword="null"/>) field
        /// returns that field to the unset (wire-omitted) state; an explicit
        /// <see langword="false"/> do-not-sell is still emitted.
        /// </summary>
        public Task SetAsync(bool? doNotSell, string? usPrivacy)
        {
            return _runtime.SetCcpaConsentAsync(doNotSell, usPrivacy);
        }

        /// <summary>
        /// Sets only the CCPA do-not-sell election, preserving the current US-Privacy
        /// string (read-modify-write over the platform's combined election primitive).
        /// </summary>
        public async Task SetDoNotSellAsync(bool? doNotSell)
        {
            var usPrivacy = await _runtime.GetCcpaUsPrivacyAsync().ConfigureAwait(false);
            await _runtime.SetCcpaConsentAsync(doNotSell, usPrivacy).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets only the raw IAB US-Privacy string, preserving the current do-not-sell
        /// election (read-modify-write over the platform's combined election primitive).
        /// </summary>
        public async Task SetUsPrivacyAsync(string? usPrivacy)
        {
            var doNotSell = await _runtime.GetCcpaDoNotSellAsync().ConfigureAwait(false);
            await _runtime.SetCcpaConsentAsync(doNotSell, usPrivacy).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Apple App Tracking Transparency helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxAttConsent
    {
        private readonly Internal.IAttriaxEngine _runtime;

        internal AttriaxAttConsent(Internal.IAttriaxEngine runtime)
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
        private readonly Internal.IAttriaxEngine _runtime;

        internal AttriaxGdprConsent(Internal.IAttriaxEngine runtime)
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