#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// The single seam every Attriax facade method routes through once the Unity
    /// SDK becomes a thin wrapper over a native engine (see
    /// <c>NATIVE_ENGINE_REWRAP.md</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Phase 1 (additive, non-destructive) DEFINES this contract and validates its
    /// shape against a <c>FakeEnginePlatform</c> in EditMode tests. The existing
    /// C# engine under <c>Runtime/Internal/</c> stays authoritative until a native
    /// binding (Phase 2+) and the facade rewire (Phase 5) are landed; nothing here
    /// changes the public C# API.
    /// </para>
    /// <para>
    /// It mirrors the freshly-reviewed Flutter platform interface
    /// (<c>AttriaxPlatform</c>) 1:1, translated to C# idioms — every command,
    /// getter, and event maps method-for-method. Per-platform implementations
    /// (Android <c>core-android</c> AAR, iOS XCFramework, desktop/Editor shared
    /// lib, WebGL <c>@attriax/js</c>) forward these calls to their native engine;
    /// the wire argument keys match the Flutter <c>MethodChannel</c> and the KMP
    /// C-ABI <c>route(...)</c> dispatch so a binding can forward without
    /// translation.
    /// </para>
    /// <para>
    /// C# idiom deviations from the Dart interface, all intentional:
    /// commands/getters use <see cref="Task"/>/<see cref="Task{TResult}"/> instead
    /// of <c>Future</c>; event streams become C# <see cref="Action{T}"/> events
    /// instead of <c>Stream</c>; <c>Duration?</c> timeouts become
    /// <c>int? timeoutMs</c>; the KMP CCPA reader getters
    /// (<see cref="GetDoNotSell"/> / <see cref="GetUsPrivacy"/>) are exposed
    /// (the Dart interface omits them — the Dart facade owns that state). The
    /// historical Flutter "signal-collection" methods (<c>collectNativeContext</c>,
    /// <c>collectInstallReferrer</c>, <c>readAttributionClipboard</c>,
    /// <c>collectWebViewUserAgent</c>, <c>setAutomaticCrashReportingEnabled</c>,
    /// <c>consumePendingCrashReport</c>, <c>openBrowserUrl</c>) are NOT mirrored
    /// here: in Unity those signals are collected C#-side by
    /// <c>AttriaxNativeBridge</c>, not routed through the engine platform.
    /// </para>
    /// </remarks>
    internal interface IAttriaxEnginePlatform
    {
        // ---------------------------------------------------------------------
        // Lifecycle (mirrors KMP Attriax.init / flush / reset / dispose).
        // ---------------------------------------------------------------------

        /// <summary>Bootstrap the native engine with <paramref name="config"/>.</summary>
        Task InitializeAsync(AttriaxConfig config);

        /// <summary>Best-effort queue flush (KMP <c>Attriax.flush</c>).</summary>
        Task Flush();

        /// <summary>Clear SDK state to pre-init (KMP <c>Attriax.reset</c>).</summary>
        Task Reset();

        /// <summary>Release listeners and dispose runtime resources (KMP <c>Attriax.dispose</c>).</summary>
        Task Dispose();

        // ---------------------------------------------------------------------
        // Tracking — events / page views.
        // ---------------------------------------------------------------------

        /// <summary>Record a custom event (KMP <c>AttriaxTracking.recordEvent</c>).</summary>
        Task RecordEvent(
            string name,
            IDictionary<string, object>? eventData = null,
            bool flushImmediately = false);

        /// <summary>Record a page/screen view (KMP <c>AttriaxTracking.recordPageView</c>).</summary>
        Task RecordPageView(
            string pageName,
            string? pageClass = null,
            string? pageTitle = null,
            string? previousPageName = null,
            IDictionary<string, object>? parameters = null,
            string source = "manual",
            bool flushImmediately = false);

        // ---------------------------------------------------------------------
        // Tracking — revenue / ad events. The native engine performs the
        // reserved-key lowering + currency normalization.
        // ---------------------------------------------------------------------

        /// <summary>Record a completed purchase (KMP <c>AttriaxTracking.recordPurchase</c>).</summary>
        Task RecordPurchase(
            double revenue,
            string currency = "USD",
            bool revenueInMicros = false,
            string? purchaseType = null,
            string? productId = null,
            string? transactionId = null,
            string? originalTransactionId = null,
            string? validationProvider = null,
            string? validationEnvironment = null,
            string? purchaseToken = null,
            string? receiptData = null,
            string? signedPayload = null,
            string? receiptSignature = null,
            bool? isRenewal = null,
            int quantity = 1,
            string? store = null,
            string? packageName = null,
            bool? voided = null,
            bool? test = null,
            string? validationId = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = true);

        /// <summary>Record a refund (KMP <c>AttriaxTracking.recordRefund</c>).</summary>
        Task RecordRefund(
            double revenue,
            string currency = "USD",
            bool revenueInMicros = false,
            string? purchaseType = null,
            string? productId = null,
            string? transactionId = null,
            string? originalTransactionId = null,
            int quantity = 1,
            string? store = null,
            string? packageName = null,
            bool? voided = null,
            bool? test = null,
            string? reason = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = true);

        /// <summary>Record realized ad revenue (KMP <c>AttriaxTracking.recordAdRevenue</c>).</summary>
        Task RecordAdRevenue(
            double revenue,
            string currency = "USD",
            bool revenueInMicros = false,
            string? adNetwork = null,
            string? adFormat = null,
            string? adType = null,
            string? adPlacement = null,
            bool? test = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = true);

        /// <summary>
        /// Record an ad-lifecycle event under its reserved <paramref name="eventName"/>
        /// (KMP <c>AttriaxTracking.recordAdEvent</c>, whose <c>AttriaxAdEventType</c>
        /// lowers to the reserved event name).
        /// </summary>
        Task RecordAdEvent(
            string eventName,
            string? adNetwork = null,
            string? mediationNetwork = null,
            string? adUnitId = null,
            string? adPlacement = null,
            string? adFormat = null,
            string? adType = null,
            string? failureReason = null,
            double? loadLatencyMs = null,
            string? rewardType = null,
            double? rewardAmount = null,
            bool? test = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = true);

        // ---------------------------------------------------------------------
        // Tracking — notifications / errors.
        // ---------------------------------------------------------------------

        /// <summary>
        /// Record a push-notification lifecycle event (KMP
        /// <c>AttriaxTracking.recordNotification</c>). <paramref name="type"/> and
        /// <paramref name="source"/> are the wire slugs the facade resolves from
        /// its notification enums.
        /// </summary>
        Task RecordNotification(
            string type,
            string notificationId,
            string? linkId = null,
            string? campaignId = null,
            string? title = null,
            string? source = null,
            IDictionary<string, object>? payload = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = false);

        /// <summary>
        /// Record an error/crash (KMP <c>AttriaxTracking.recordError</c>). The
        /// facade lowers a C# <see cref="Exception"/> to the primitive
        /// message/exceptionType/stackTrace wire fields.
        /// </summary>
        Task RecordError(
            string message,
            string exceptionType,
            string? stackTrace = null,
            bool fatal = false,
            string source = "manual",
            string? reason = null,
            IDictionary<string, object>? metadata = null);

        // ---------------------------------------------------------------------
        // Tracking — identify / user properties.
        // ---------------------------------------------------------------------

        /// <summary>
        /// Associate the current device with a user; a <c>null</c>
        /// <paramref name="userId"/> clears it (KMP <c>AttriaxTracking.setUser</c>).
        /// </summary>
        Task SetUser(string? userId = null, string? userName = null);

        /// <summary>
        /// Set a single user property; a <c>null</c> <paramref name="value"/>
        /// clears it (KMP <c>AttriaxTracking.setUserProperty</c>).
        /// </summary>
        Task SetUserProperty(string name, object? value);

        /// <summary>Merge user properties (KMP <c>AttriaxTracking.setUserProperties</c>).</summary>
        Task SetUserProperties(IDictionary<string, object> properties);

        /// <summary>
        /// Clear user properties; <c>null</c>/empty <paramref name="propertyNames"/>
        /// clears all (KMP <c>AttriaxTracking.clearUserProperties</c>).
        /// </summary>
        Task ClearUserProperties(IList<string>? propertyNames = null);

        /// <summary>
        /// Register (or, with a <c>null</c> <paramref name="token"/>, de-register)
        /// a push/uninstall token for <paramref name="provider"/> (KMP
        /// <c>registerFirebaseMessagingToken</c> / <c>registerApplePushToken</c>).
        /// </summary>
        Task RegisterPushToken(
            AttriaxPushTokenProvider provider,
            string? token,
            IDictionary<string, object>? metadata = null);

        // ---------------------------------------------------------------------
        // Deep links (mirrors KMP AttriaxDeepLinks).
        // ---------------------------------------------------------------------

        /// <summary>Feed a raw deep-link URI to the engine (KMP <c>AttriaxDeepLinks.handleUri</c>).</summary>
        Task HandleIncomingLink(string uri, bool isInitialLink = false);

        /// <summary>
        /// Mark the initial-link probe complete when the launch carried no deep
        /// link (KMP <c>AttriaxDeepLinks.completeInitialLinkIfAbsent</c>).
        /// </summary>
        Task CompleteInitialDeepLink();

        /// <summary>
        /// Record a deep link manually and return its resolved event (KMP
        /// <c>AttriaxDeepLinks.recordDeepLink</c>).
        /// </summary>
        Task<AttriaxDeepLinkEvent?> RecordDeepLink(
            Uri uri,
            IDictionary<string, object>? metadata = null,
            string source = "manual");

        /// <summary>
        /// Block until the initial-link probe settles (KMP
        /// <c>AttriaxDeepLinks.waitForInitialDeepLink</c>).
        /// </summary>
        Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink();

        /// <summary>
        /// Block until the resolution for <paramref name="rawEvent"/> completes
        /// (KMP <c>AttriaxDeepLinks.waitResolution</c>).
        /// </summary>
        Task<AttriaxDeepLinkEvent?> WaitForDeepLinkResolution(AttriaxRawDeepLinkEvent rawEvent);

        /// <summary>Create a short dynamic link (KMP <c>AttriaxDeepLinks.createDynamicLink</c>).</summary>
        Task<AttriaxCreateDynamicLinkResult> CreateDynamicLink(AttriaxCreateDynamicLinkOptions options);

        // ---------------------------------------------------------------------
        // Revenue receipt validation.
        // ---------------------------------------------------------------------

        /// <summary>Validate a purchase receipt directly (KMP <c>Attriax.validateReceipt</c>).</summary>
        Task<AttriaxRevenueReceiptValidationResult> ValidateReceipt(
            string receipt,
            bool test = false,
            string? provider = null,
            string? environment = null,
            string? productId = null,
            string? transactionId = null);

        // ---------------------------------------------------------------------
        // Consent — GDPR (mirrors KMP AttriaxGdprConsent).
        // ---------------------------------------------------------------------

        /// <summary>Store granted GDPR consent category values (KMP <c>AttriaxGdprConsent.setConsent</c>).</summary>
        Task SetGdprConsent(bool analytics, bool attribution, bool adEvents);

        /// <summary>Mark GDPR consent as not required (KMP <c>AttriaxGdprConsent.setNotRequired</c>).</summary>
        Task SetGdprConsentNotRequired();

        /// <summary>Clear the local GDPR decision (KMP <c>AttriaxGdprConsent.reset</c>).</summary>
        Task ResetGdprConsent();

        /// <summary>Request GDPR data erasure (KMP <c>AttriaxGdprConsent.requestDataErasure</c>).</summary>
        Task RequestGdprDataErasure();

        // ---------------------------------------------------------------------
        // Toggles (mirrors KMP Attriax.enabled / anonymousTrackingEnabled and the
        // facade's separate SDK-enabled vs events-enabled flags).
        // ---------------------------------------------------------------------

        /// <summary>Toggle GDPR-safe anonymous tracking (KMP <c>Attriax.anonymousTrackingEnabled</c>).</summary>
        Task SetAnonymousTracking(bool enabled);

        /// <summary>
        /// Set the CCPA "do not sell / share" election (KMP
        /// <c>consent.ccpa</c>). A non-null value is forwarded so the next
        /// app-open/identify carries it TOP-LEVEL as <c>doNotSell</c> /
        /// <c>usPrivacy</c>; an omitted (null) field is left unchanged. An explicit
        /// <c>doNotSell: false</c> may clear a prior server latch.
        /// </summary>
        Task SetCcpaConsent(bool? doNotSell, string? usPrivacy);

        /// <summary>Toggle the whole SDK runtime (KMP <c>Attriax.enabled</c>).</summary>
        Task SetSdkEnabled(bool enabled);

        /// <summary>Toggle event-style tracking (facade <c>tracking.enabled</c>).</summary>
        Task SetEventTrackingEnabled(bool enabled);

        // ---------------------------------------------------------------------
        // Apple seams (mirrors KMP Attriax.submitAsaToken, AttriaxSkan, ATT).
        // ---------------------------------------------------------------------

        /// <summary>
        /// Submit an Apple Search Ads (AdServices) attribution token (KMP
        /// <c>Attriax.submitAsaToken</c>).
        /// </summary>
        Task SubmitAsaToken(string token);

        /// <summary>
        /// Wrapper-supply the natively-obtained ATT status (KMP
        /// <c>AttriaxAttConsent.setStatus</c>).
        /// </summary>
        Task SetTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status);

        /// <summary>
        /// Request App Tracking Transparency authorization (KMP
        /// <c>AttriaxAttConsent.requestAuthorization</c>). <paramref name="timeoutMs"/>
        /// bounds the wait; <c>null</c> waits without a timeout.
        /// </summary>
        Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorization(int? timeoutMs = null);

        /// <summary>
        /// Manually push a SKAdNetwork conversion-value update (KMP
        /// <c>AttriaxSkan.updateConversionValue</c>).
        /// </summary>
        Task<AttriaxSkanUpdateResult> UpdateSkanConversionValue(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue = null,
            bool lockWindow = false);

        // ---------------------------------------------------------------------
        // Engine reads (mirrors KMP Attriax getters + the sub-surface getters).
        // ---------------------------------------------------------------------

        /// <summary>Stable Attriax device identifier (KMP <c>Attriax.deviceId</c>).</summary>
        Task<string?> GetDeviceId();

        /// <summary>Whether the current run is the first launch (KMP <c>Attriax.isFirstLaunch</c>).</summary>
        Task<bool> GetIsFirstLaunch();

        /// <summary>Whether the engine finished initialization (KMP <c>Attriax.isInitialized</c>).</summary>
        Task<bool> GetIsInitialized();

        /// <summary>SDK version + metadata snapshot (KMP <c>Attriax.sdkSnapshot</c>).</summary>
        Task<AttriaxSdkSnapshot?> GetSdkSnapshot();

        /// <summary>Whether the whole SDK runtime is enabled (KMP <c>Attriax.enabled</c>).</summary>
        Task<bool> GetSdkEnabled();

        /// <summary>Whether event-style tracking is enabled (facade <c>tracking.enabled</c>).</summary>
        Task<bool> GetEventTrackingEnabled();

        /// <summary>
        /// Whether GDPR-safe anonymous tracking is allowed (KMP
        /// <c>Attriax.anonymousTrackingEnabled</c>).
        /// </summary>
        Task<bool> GetAnonymousTracking();

        /// <summary>Current synchronization state (KMP <c>AttriaxSynchronization.state</c>).</summary>
        Task<AttriaxSynchronizationState> GetSynchronizationState();

        /// <summary>
        /// Whether every queued request has been delivered (KMP
        /// <c>AttriaxSynchronization.isSynchronized</c>).
        /// </summary>
        Task<bool> GetIsSynchronized();

        /// <summary>Original install referrer (KMP <c>AttriaxReferrer.getOriginalInstallReferrer</c>).</summary>
        Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrer(int? timeoutMs = null);

        /// <summary>Reinstall referrer (KMP <c>AttriaxReferrer.getReinstallReferrer</c>).</summary>
        Task<AttriaxInstallReferrerDetails?> GetReinstallReferrer(int? timeoutMs = null);

        /// <summary>Raw platform install-referrer string (KMP <c>AttriaxReferrer.getRawInstallReferrer</c>).</summary>
        Task<string?> GetRawInstallReferrer(int? timeoutMs = null);

        /// <summary>
        /// Deep-link referrer that opened the current session (KMP
        /// <c>AttriaxReferrer.getSessionReferrer</c>).
        /// </summary>
        Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrer(int? timeoutMs = null);

        /// <summary>
        /// Most recent deep-link referrer (KMP
        /// <c>AttriaxReferrer.getLatestDeepLinkReferrer</c>).
        /// </summary>
        Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrer(int? timeoutMs = null);

        /// <summary>Latest locally persisted SKAdNetwork state (KMP <c>AttriaxSkan.state</c>).</summary>
        Task<AttriaxSkanState?> GetSkanState();

        /// <summary>Most recent handled deep-link event (KMP <c>AttriaxDeepLinks.latestDeepLink</c>).</summary>
        Task<AttriaxDeepLinkEvent?> GetLatestDeepLink();

        /// <summary>Launch deep-link event, once resolved (KMP <c>AttriaxDeepLinks.initialDeepLink</c>).</summary>
        Task<AttriaxDeepLinkEvent?> GetInitialDeepLink();

        /// <summary>Launch raw deep-link event (KMP <c>AttriaxDeepLinks.rawInitialDeepLink</c>).</summary>
        Task<AttriaxRawDeepLinkEvent?> GetRawInitialDeepLink();

        /// <summary>
        /// Whether the initial-link probe has completed (KMP
        /// <c>AttriaxDeepLinks.initialDeepLinkResolved</c>).
        /// </summary>
        Task<bool> GetIsInitialDeepLinkResolved();

        /// <summary>
        /// Resolve whether this device needs a GDPR consent decision (KMP
        /// <c>AttriaxGdprConsent.needsConsent</c>).
        /// </summary>
        Task<bool> NeedsGdprConsent(bool localOnly = false);

        /// <summary>
        /// Whether the SDK is waiting for a GDPR decision (KMP
        /// <c>AttriaxGdprConsent.isWaitingForConsent</c>).
        /// </summary>
        Task<bool> GetIsWaitingForGdprConsent();

        /// <summary>
        /// Current App Tracking Transparency authorization status (KMP
        /// <c>AttriaxAttConsent.status</c>).
        /// </summary>
        Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatus();

        /// <summary>
        /// Current CCPA do-not-sell election, or <c>null</c> when unset (KMP
        /// C-ABI <c>getDoNotSell</c>).
        /// </summary>
        Task<bool?> GetDoNotSell();

        /// <summary>
        /// Current raw IAB US-Privacy string, or <c>null</c> when unset (KMP
        /// C-ABI <c>getUsPrivacy</c>).
        /// </summary>
        Task<string?> GetUsPrivacy();

        // ---------------------------------------------------------------------
        // Events (native -> C#). Mirror the KMP listener surfaces / the Flutter
        // EventChannel streams. Implementations MUST raise these on the Unity
        // main thread via AttriaxLifecycleDispatcher.PostToMainThread
        // (fire-and-forget) — never a blocking marshal (see the DebugLog
        // main-thread deadlock lesson).
        // ---------------------------------------------------------------------

        /// <summary>
        /// Synchronization-state transitions (KMP
        /// <c>AttriaxSynchronization.addStateListener</c>).
        /// </summary>
        event Action<AttriaxSynchronizationState> SynchronizationStateChanged;

        /// <summary>Resolved deep-link events (KMP <c>AttriaxDeepLinks.addListener</c>).</summary>
        event Action<AttriaxDeepLinkEvent> DeepLinkResolved;

        /// <summary>
        /// Raw (pre-resolution) deep-link inputs (KMP
        /// <c>AttriaxDeepLinks.addRawListener</c>).
        /// </summary>
        event Action<AttriaxRawDeepLinkEvent> RawDeepLinkReceived;

        /// <summary>
        /// Initial-link probe resolutions (KMP
        /// <c>AttriaxDeepLinks.waitForInitialDeepLink</c> completion).
        /// </summary>
        event Action<AttriaxInitialDeepLinkResolution> InitialDeepLinkResolved;
    }
}
