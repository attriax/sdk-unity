#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// The facade-facing engine surface every public wrapper
    /// (<c>AttriaxTracking</c>, <c>AttriaxConsent</c>, <c>AttriaxDeepLinks</c>,
    /// <c>AttriaxReferrer</c>, <c>AttriaxSkan</c>, <c>AttriaxSynchronization</c>,
    /// and the top-level <c>Attriax</c> entry point) depends on. It is the
    /// engine-selection seam introduced ahead of the native re-wrap
    /// (see <c>NATIVE_ENGINE_REWRAP.md</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is deliberately the <em>public-API-shaped</em> surface that the
    /// managed C# engine (<see cref="AttriaxRuntime"/>) already exposes today —
    /// rich option objects, synchronous property getters, <c>Task</c>-returning
    /// commands, and <c>Subscribe*</c> handles. Extracting it as an interface
    /// lets the facades bind to an abstraction that <see cref="AttriaxRuntime"/>
    /// satisfies with <b>zero signature changes</b>, so behavior is identical.
    /// </para>
    /// <para>
    /// It is intentionally distinct from the wire-shaped native contract
    /// <c>Engine.IAttriaxEnginePlatform</c> (decomposed primitive arguments,
    /// all-async getters, C# events) which mirrors the Flutter/KMP platform
    /// interface. A future adapter will bridge an
    /// <c>IAttriaxEnginePlatform</c> impl onto this facade surface, at which
    /// point <see cref="AttriaxEngineSelector"/> can return a native engine on a
    /// per-platform basis. Until each platform's native binding is verified live,
    /// the C# engine remains the default and fallback on every platform.
    /// </para>
    /// </remarks>
    internal interface IAttriaxEngine : IDisposable
    {
        // -- Lifecycle / instance state -------------------------------------
        AttriaxConfig Config { get; }
        bool IsInitialized { get; }
        bool Enabled { get; }
        bool IsFirstLaunch { get; }
        string? DeviceId { get; }
        AttriaxSdkSnapshot? SdkSnapshot { get; }

        void SetEnabled(bool enabled);
        Task InitializeAsync(AttriaxInitOptions options);
        Task ResetAsync();
        Task<AttriaxRevenueReceiptValidationResult> ValidateReceiptAsync(
            AttriaxValidateReceiptOptions options);

        // -- Tracking: toggles ----------------------------------------------
        bool EventsEnabled { get; set; }
        bool AnonymousTrackingEnabled { get; set; }

        // -- Tracking: events / revenue / notifications / errors ------------
        Task RecordEventAsync(string eventName, AttriaxTrackEventOptions options);
        Task RecordPageViewAsync(string pageName, AttriaxPageViewOptions options);
        Task RecordPurchaseAsync(double revenue, AttriaxRecordPurchaseOptions options);
        Task RecordRefundAsync(double revenue, AttriaxRecordRefundOptions options);
        Task RecordAdRevenueAsync(double revenue, AttriaxRecordAdRevenueOptions options);
        Task RecordAdEventAsync(AttriaxAdEventType type, AttriaxRecordAdEventOptions options);
        Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions options);
        Task RecordNotificationAsync(
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxRecordNotificationOptions options);

        // -- Tracking: identity / user properties / push tokens -------------
        Task SetUserAsync(string? userId, AttriaxSetUserOptions options);
        Task IdentifyAsync(string? userId, AttriaxIdentifyOptions options);
        Task SetUserPropertyAsync(string name, object? value);
        Task SetUserPropertiesAsync(IDictionary<string, object?> properties);
        Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null);
        Task RegisterFirebaseMessagingTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null);
        Task RegisterApplePushTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null);

        // -- Consent: ATT ----------------------------------------------------
        Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(
            int? timeoutMs = null);
        Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync();

        // -- Consent: GDPR ---------------------------------------------------
        AttriaxGdprConsentState GdprConsentState { get; }
        AttriaxGdprConsentValues? GdprConsentValues { get; }
        bool IsWaitingForGdprConsent { get; }
        Task<bool> NeedsGdprConsentAsync(bool localOnly = false);
        void SetGdprConsent(bool analytics, bool attribution, bool adEvents);
        void SetGdprConsentNotRequired();
        void ResetGdprConsent();
        Task RequestGdprDataErasureAsync();

        // -- Deep links ------------------------------------------------------
        AttriaxRawDeepLinkEvent? RawInitialDeepLinkValue { get; }
        AttriaxDeepLinkEvent? InitialDeepLinkValue { get; }
        bool InitialDeepLinkResolved { get; }
        Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink { get; }
        AttriaxDeepLinkEvent? LatestDeepLink { get; }
        IDisposable SubscribeToRawDeepLinks(Action<AttriaxRawDeepLinkEvent> listener);
        IDisposable SubscribeToDeepLinks(Action<AttriaxDeepLinkEvent> listener);
        Task<AttriaxDeepLinkEvent> WaitForDeepLinkResolutionAsync(AttriaxRawDeepLinkEvent rawEvent);
        Task<AttriaxCreateDynamicLinkResult> CreateDynamicLinkAsync(
            AttriaxCreateDynamicLinkOptions options);
        Task<AttriaxDeepLinkEvent?> RecordDeepLinkConversionAsync(
            AttriaxDeepLinkConversionOptions options);

        // -- Referrer --------------------------------------------------------
        Task<AttriaxInstallReferrerDetails?> OriginalInstallReferrer { get; }
        Task<AttriaxInstallReferrerDetails?> ReinstallReferrer { get; }
        Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync();
        Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync();

        // -- SKAN ------------------------------------------------------------
        AttriaxSkanState? SkanState { get; }
        Task<AttriaxSkanUpdateResult> UpdateSkanConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow);

        // -- Synchronization -------------------------------------------------
        AttriaxSynchronizationState SynchronizationState { get; }
        IDisposable SubscribeToSynchronization(Action<AttriaxSynchronizationState> listener);
    }
}
