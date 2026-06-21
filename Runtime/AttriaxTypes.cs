#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Attribution resolution type returned by app-open tracking.
    /// </summary>
    public enum AttributionType
    {
        /// <summary>
        /// Attribution derived from platform install-referrer data.
        /// </summary>
        Referrer,

        /// <summary>
        /// Attribution derived from probabilistic fingerprint matching.
        /// </summary>
        Fingerprint,

        /// <summary>
        /// Attribution reserved for future external provider integrations.
        /// </summary>
        External,

        /// <summary>
        /// Attribution assigned when no attributable source was found.
        /// </summary>
        Organic,
    }

    /// <summary>
    /// Install-state classification returned by the Attriax backend.
    /// </summary>
    public enum AttriaxInstallState
    {
        Existing,
        NewInstall,
        Reinstall,
        AppDataClear,
    }

    /// <summary>
    /// Deep-link resolution state returned by the Attriax API.
    /// </summary>
    public enum AttriaxDeepLinkResolutionStatus
    {
        Matched,
        Unmatched,
        Invalid,
    }

    /// <summary>
    /// How the SDK should open a resolved browser URL.
    /// </summary>
    public enum AttriaxResolvedUrlOpenMode
    {
        InApp,
        External,
        Unknown,
    }

    /// <summary>
    /// How a deep-link event entered the SDK runtime.
    /// </summary>
    public enum AttriaxDeepLinkTrigger
    {
        ColdStart,
        Foreground,
        Deferred,
    }

    /// <summary>
    /// Platform identifiers supported by the Unity SDK.
    /// </summary>
    public enum AttriaxPlatformType
    {
        IOS,
        Android,
        Web,
        UnityEditor,
        Windows,
        MacOS,
        Linux,
        Unknown,
    }

    /// <summary>
    /// Queue synchronization state for a running SDK instance.
    /// </summary>
    public enum AttriaxSynchronizationState
    {
        Initializing,
        Synchronizing,
        Deferred,
        Synchronized,
        Offline,
        Failed,
        Disabled,
    }

    /// <summary>
    /// Current App Tracking Transparency authorization state.
    /// </summary>
    public enum AttriaxTrackingAuthorizationStatus
    {
        NotSupported,
        Disabled,
        NotDetermined,
        Restricted,
        Denied,
        Authorized,
        TimedOut,
        Unknown,
    }

    /// <summary>
    /// Public GDPR consent states exposed by the SDK.
    /// </summary>
    public enum AttriaxGdprConsentState
    {
        Unknown,
        NotRequired,
        Pending,
        Granted,
    }

    /// <summary>
    /// Category-level GDPR consent values returned when consent has been granted.
    /// </summary>
    public sealed class AttriaxGdprConsentValues
    {
        public bool Analytics { get; set; }

        public bool Attribution { get; set; }

        public bool AdEvents { get; set; }
    }

    /// <summary>
    /// Public receipt-validation states returned by the Attriax API.
    /// </summary>
    public enum AttriaxRevenueReceiptValidationStatus
    {
        Verified,
        Rejected,
        Pending,
        Unconfigured,
        ProviderError,
        Passthrough,
    }

    /// <summary>
    /// Canonical ad lifecycle events tracked by the Attriax SDKs.
    /// </summary>
    public enum AttriaxAdEventType
    {
        Request,
        Load,
        LoadFailed,
        Show,
        ShowFailed,
        Impression,
        Click,
        Dismiss,
        Reward,
    }

    /// <summary>
    /// Coarse SKAdNetwork conversion buckets for postback windows that only expose low, medium, or high values.
    /// </summary>
    public enum AttriaxSkanCoarseValue
    {
        Low,
        Medium,
        High,
    }

    /// <summary>
    /// Operators supported by dashboard-managed SKAN rule conditions.
    /// </summary>
    public enum AttriaxSkanRuleOperator
    {
        Exists,
        Eq,
        NotEq,
        Gt,
        Gte,
        Lt,
        Lte,
        Contains,
    }

    /// <summary>
    /// Public outcome states returned from SKAN conversion-value updates.
    /// </summary>
    public enum AttriaxSkanUpdateStatus
    {
        Updated,
        Skipped,
        AlreadyAtOrAboveValue,
        InvalidValue,
        Disabled,
        NotSupported,
        Error,
    }

    /// <summary>
    /// Optional local SKAN runtime configuration.
    /// Leave this null in most apps and let Attriax load the schema from the dashboard during app open.
    /// </summary>
    public sealed class AttriaxSkanConfig
    {
        /// <summary>
        /// Enables or disables SKAN updates for this SDK instance.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Registers the first-launch value automatically when possible.
        /// </summary>
        public bool RegisterFirstLaunchValue { get; set; } = true;
    }

    /// <summary>
    /// Single condition attached to a SKAN rule.
    /// </summary>
    public sealed class AttriaxSkanCondition
    {
        public string Id { get; set; } = string.Empty;

        public string ParamKey { get; set; } = string.Empty;

        public AttriaxSkanRuleOperator Operator { get; set; } = AttriaxSkanRuleOperator.Eq;

        public object? Value { get; set; }
    }

    /// <summary>
    /// Dashboard-managed SKAN event matched by event name and optional parameter conditions.
    /// </summary>
    public sealed class AttriaxSkanEvent
    {
        public string Id { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public AttriaxSkanCoarseValue? CoarseValue { get; set; }

        public bool LockWindow { get; set; }

        public IList<AttriaxSkanCondition> Conditions { get; set; } = new List<AttriaxSkanCondition>();
    }

    /// <summary>
    /// Bit-range group for SKAN window 1.
    /// </summary>
    public sealed class AttriaxSkanWindow1Group
    {
        public string Id { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public int StartBit { get; set; }

        public int BitCount { get; set; }

        public IList<AttriaxSkanEvent> Events { get; set; } = new List<AttriaxSkanEvent>();
    }

    /// <summary>
    /// Coarse-only event for SKAN windows 2 and 3.
    /// </summary>
    public sealed class AttriaxSkanCoarseWindowEvent
    {
        public string Id { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public bool LockWindow { get; set; }

        public IList<AttriaxSkanCondition> Conditions { get; set; } = new List<AttriaxSkanCondition>();

        public AttriaxSkanCoarseValue CoarseValue { get; set; }
    }

    /// <summary>
    /// Fine-value bit-range configuration for the first postback window.
    /// </summary>
    public sealed class AttriaxSkanWindow1
    {
        public IList<AttriaxSkanWindow1Group> Groups { get; set; } = new List<AttriaxSkanWindow1Group>();
    }

    /// <summary>
    /// Coarse-only event list for the later postback windows.
    /// </summary>
    public sealed class AttriaxSkanCoarseWindow
    {
        public IList<AttriaxSkanCoarseWindowEvent> Events { get; set; } = new List<AttriaxSkanCoarseWindowEvent>();
    }

    /// <summary>
    /// Full dashboard-managed SKAN schema delivered during app open.
    /// </summary>
    public sealed class AttriaxSkanSchema
    {
        public int Version { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public AttriaxSkanWindow1 Window1 { get; set; } = new AttriaxSkanWindow1();

        public AttriaxSkanCoarseWindow Window2 { get; set; } = new AttriaxSkanCoarseWindow();

        public AttriaxSkanCoarseWindow Window3 { get; set; } = new AttriaxSkanCoarseWindow();
    }

    /// <summary>
    /// Runtime SKAN configuration returned by the backend on app open.
    /// </summary>
    public sealed class AttriaxSkanRuntimeConfiguration
    {
        public bool Enabled { get; set; } = true;

        public AttriaxSkanSchema? Schema { get; set; }
    }

    /// <summary>
    /// Latest locally persisted SKAN runtime state tracked by the Unity SDK.
    /// </summary>
    public sealed class AttriaxSkanState
    {
        public bool Enabled { get; set; }

        public int? FineValue { get; set; }

        public AttriaxSkanCoarseValue? CoarseValue { get; set; }

        public bool LockWindow { get; set; }

        public bool FirstLaunchValueRegistered { get; set; }

        public int? SchemaVersion { get; set; }

        public AttriaxSkanSchema? Schema { get; set; }

        public DateTimeOffset? InstallAnchorAt { get; set; }

        public IList<int> CompletedRetentionDays { get; set; } = new List<int>();

        public long PurchaseRevenueUsdMicros { get; set; }

        public int PurchaseCount { get; set; }

        public int AdShowCount { get; set; }

        public DateTimeOffset? LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// Result returned after a manual or automatic SKAN conversion-value update attempt.
    /// </summary>
    public sealed class AttriaxSkanUpdateResult
    {
        public AttriaxSkanUpdateStatus Status { get; set; }

        public string? Message { get; set; }

        public int? FineValue { get; set; }

        public AttriaxSkanCoarseValue? CoarseValue { get; set; }

        public bool LockWindow { get; set; }

        public AttriaxSkanState? State { get; set; }
    }

    /// <summary>
    /// Runtime configuration used to construct an SDK instance programmatically.
    /// </summary>
    public sealed class AttriaxConfig
    {
        /// <summary>
        /// Project token issued by Attriax for the current project.
        /// </summary>
        public string ProjectToken { get; set; } = string.Empty;

        /// <summary>
        /// Enables GDPR-aware dispatch gating and local persistence for this SDK instance.
        /// When enabled, anonymous-capable analytics activity still sends while consent is
        /// <c>Unknown</c> or <c>Pending</c>, but attribution-only work stays withheld until
        /// the consent state allows it.
        /// When disabled, consent APIs remain available but tracking is not blocked by consent state.
        /// </summary>
        public bool GdprEnabled { get; set; }

        /// <summary>
        /// Allows anonymous-capable traffic to keep flowing while GDPR consent is
        /// still <c>Unknown</c> or <c>Pending</c>.
        /// When enabled, the SDK avoids materializing device identity before consent
        /// resolves, but it can still send anonymous analytics/session/deep-link
        /// traffic immediately. When disabled, that traffic stays queued in memory
        /// until consent later allows identified delivery.
        /// </summary>
        public bool AnonymousTracking { get; set; } = true;

        /// <summary>
        /// Optional base URL override for local, staging, or self-hosted Attriax environments.
        /// </summary>
        public string? ApiBaseUrl { get; set; }

        /// <summary>
        /// Optional app version string attached to SDK requests.
        /// </summary>
        public string? AppVersion { get; set; }

        /// <summary>
        /// Optional app build number attached to SDK requests. When left blank in a built player, Attriax falls back to Application.buildGUID.
        /// </summary>
        public string? AppBuildNumber { get; set; }

        /// <summary>
        /// Optional application package identifier attached to SDK requests.
        /// </summary>
        public string? AppPackageName { get; set; }

        /// <summary>
        /// Extra SDK metadata sent with requests as a normalized JSON object.
        /// </summary>
        public IDictionary<string, object> SdkMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Enables verbose debug logging for SDK activity.
        /// </summary>
        public bool EnableDebugLogs { get; set; }

        /// <summary>
        /// Whether native platform collectors may include advertising identifiers.
        /// Android uses this to control GAID collection. Apple platforms use it with
        /// App Tracking Transparency authorization to control SDK-managed IDFA collection.
        /// Host apps may still call the public tracking-authorization APIs when this is false.
        /// </summary>
        public bool CollectAdvertisingId { get; set; } = true;

        /// <summary>
        /// Enables automatic capture of unhandled Unity exceptions as crash reports.
        /// </summary>
        public bool AutomaticCrashReportingEnabled { get; set; } = true;

        /// <summary>
        /// Whether initialization should request App Tracking Transparency authorization on iOS.
        /// When this is false, startup polls the current authorization status instead.
        /// </summary>
        public bool RequestTrackingAuthorizationOnInit { get; set; }

        /// <summary>
        /// Maximum time startup waits for App Tracking Transparency status, in milliseconds.
        /// </summary>
        public int TrackingAuthorizationStatusTimeoutMs { get; set; } = 60000;

        /// <summary>
        /// Enables automatic session lifecycle tracking and session enrichment.
        /// </summary>
        public bool SessionTrackingEnabled { get; set; } = true;

        /// <summary>
        /// Tracks the active Unity scene automatically as standardized page-view events.
        /// </summary>
        public bool AutomaticSceneTracking { get; set; } = true;

        /// <summary>
        /// Opens backend-provided browser actions automatically when they are returned.
        /// </summary>
        public bool AutomaticBrowserHandling { get; set; } = true;

        /// <summary>
        /// Heartbeat interval used for established sessions after first launch, in milliseconds.
        /// </summary>
        public int SessionHeartbeatIntervalMs { get; set; } = 300000;

        /// <summary>
        /// Heartbeat interval used during the installation's first launch session, in milliseconds.
        /// </summary>
        public int FirstLaunchSessionHeartbeatIntervalMs { get; set; } = 30000;

        /// <summary>
        /// Minimum delay between automatic flushes of regular queued events, in milliseconds.
        /// </summary>
        public int EventFlushIntervalMs { get; set; } = 60000;

        /// <summary>
        /// Keeps regular events on the installation's first launch session on the immediate flush path.
        /// </summary>
        public bool FlushEventsImmediatelyOnFirstLaunch { get; set; } = true;

        /// <summary>
        /// Per-request timeout for outbound Attriax API calls, in milliseconds.
        /// </summary>
        public int RequestTimeoutMs { get; set; } = 12000;

        /// <summary>
        /// Maximum number of queued operations persisted locally for retry.
        /// </summary>
        public int MaxQueueSize { get; set; } = 500;

        /// <summary>
        /// Prefix used for PlayerPrefs keys owned by this SDK instance.
        /// </summary>
        public string StorageKeyPrefix { get; set; } = "attriax:unity";

        /// <summary>
        /// Optional local SKAN runtime configuration.
        /// Leave null to use the default dashboard-driven SKAN behavior.
        /// </summary>
        public AttriaxSkanConfig? Skan { get; set; }
    }

    /// <summary>
    /// Initialization options applied when the SDK starts.
    /// </summary>
    public sealed class AttriaxInitOptions
    {
        /// <summary>
        /// Controls whether startup should inspect the current URL for an initial deep link.
        /// </summary>
        public bool CaptureInitialUrl { get; set; } = true;
    }

    /// <summary>
    /// Options for a custom event payload.
    /// </summary>
    public sealed class AttriaxTrackEventOptions
    {
        public IDictionary<string, object>? EventData { get; set; }

        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for a standardized purchase revenue event.
    /// </summary>
    public sealed class AttriaxRecordPurchaseOptions
    {
        /// <summary>
        /// ISO 4217 currency code for the revenue value. Defaults to USD.
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Set to true when Revenue is already expressed in micros.
        /// </summary>
        public bool RevenueInMicros { get; set; }

        /// <summary>
        /// Stable purchase subtype such as one_time or subscription_renewal.
        /// </summary>
        public string? PurchaseType { get; set; }

        /// <summary>
        /// Product or SKU identifier from the originating store or catalog.
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// Unique transaction or order identifier used for idempotency.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Root transaction identifier for renewals, restores, or upgrades.
        /// </summary>
        public string? OriginalTransactionId { get; set; }

        /// <summary>
        /// Validation source such as google_play, app_store, or a custom provider.
        /// </summary>
        public string? ValidationProvider { get; set; }

        /// <summary>
        /// Validation environment such as production, sandbox, or test.
        /// </summary>
        public string? ValidationEnvironment { get; set; }

        /// <summary>
        /// Raw Google Play purchase token or equivalent platform token.
        /// </summary>
        public string? PurchaseToken { get; set; }

        /// <summary>
        /// Raw App Store receipt payload or equivalent receipt blob.
        /// </summary>
        public string? ReceiptData { get; set; }

        /// <summary>
        /// Raw signed platform payload when the store exposes one separately.
        /// </summary>
        public string? SignedPayload { get; set; }

        /// <summary>
        /// Signature paired with a receipt or signed payload when required.
        /// </summary>
        public string? ReceiptSignature { get; set; }

        /// <summary>
        /// Marks this revenue row as a subscription renewal when applicable.
        /// </summary>
        public bool? IsRenewal { get; set; }

        /// <summary>
        /// Number of purchased units. Must stay positive and defaults to 1.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Store identifier such as google_play, app_store, or stripe.
        /// </summary>
        public string? Store { get; set; }

        /// <summary>
        /// Bundle id or package name associated with the purchase payload.
        /// </summary>
        public string? PackageName { get; set; }

        /// <summary>
        /// Marks a purchase that was already voided in the upstream store.
        /// </summary>
        public bool? Voided { get; set; }

        /// <summary>
        /// Marks sandbox, QA, or other non-production purchase traffic.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// Links this event to an already-created validation record.
        /// </summary>
        public string? ValidationId { get; set; }

        /// <summary>
        /// Extra JSON fields merged before the typed purchase fields are applied.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Flushes the queued purchase event immediately. Defaults to true.
        /// </summary>
        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for a standardized refund revenue event.
    /// </summary>
    public sealed class AttriaxRecordRefundOptions
    {
        /// <summary>
        /// ISO 4217 currency code for the revenue value. Defaults to USD.
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Set to true when Revenue is already expressed in micros.
        /// </summary>
        public bool RevenueInMicros { get; set; }

        /// <summary>
        /// Stable refund subtype such as chargeback or subscription_revoked.
        /// </summary>
        public string? PurchaseType { get; set; }

        /// <summary>
        /// Product or SKU identifier from the originating store or catalog.
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// Unique transaction or order identifier for the refund event itself.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Root transaction identifier for the original purchase or subscription.
        /// </summary>
        public string? OriginalTransactionId { get; set; }

        /// <summary>
        /// Number of refunded units. Must stay positive and defaults to 1.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Store identifier such as google_play, app_store, or stripe.
        /// </summary>
        public string? Store { get; set; }

        /// <summary>
        /// Bundle id or package name associated with the refunded purchase.
        /// </summary>
        public string? PackageName { get; set; }

        /// <summary>
        /// Marks refund data that was already voided in the upstream store.
        /// </summary>
        public bool? Voided { get; set; }

        /// <summary>
        /// Marks sandbox, QA, or other non-production refund traffic.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// Optional machine-readable refund reason such as chargeback.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Extra JSON fields merged before the typed refund fields are applied.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Flushes the queued refund event immediately. Defaults to true.
        /// </summary>
        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for synchronous receipt validation.
    /// </summary>
    public sealed class AttriaxValidateReceiptOptions
    {
        /// <summary>
        /// Raw receipt payload submitted by the host app.
        /// </summary>
        public string Receipt { get; set; } = string.Empty;

        /// <summary>
        /// Backend-recognized receipt provider such as <c>unity</c> or a store adapter name.
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// Validation environment such as <c>production</c>, <c>sandbox</c>, or a provider-specific label.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Transaction identifier reported by the mobile store or purchase wrapper.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Purchased product identifier or SKU.
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// Marks sandbox, QA, or other non-production validation traffic.
        /// </summary>
        public bool? Test { get; set; }
    }

    /// <summary>
    /// Result returned by synchronous receipt validation.
    /// </summary>
    public sealed class AttriaxRevenueReceiptValidationResult
    {
        public string ValidationId { get; set; } = string.Empty;

        public AttriaxRevenueReceiptValidationStatus Status { get; set; }

        public string? RequestVersion { get; set; }

        public DateTimeOffset? AcceptedAt { get; set; }

        public string? Provider { get; set; }

        public string? Environment { get; set; }

        public string? TransactionId { get; set; }

        public string? OriginalTransactionId { get; set; }

        public string? ProductId { get; set; }

        public string? FailureReason { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public IDictionary<string, object>? ProviderResult { get; set; }

        public IDictionary<string, object> PublicReceipt { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Options for a standardized ad revenue event.
    /// </summary>
    public sealed class AttriaxRecordAdRevenueOptions
    {
        /// <summary>
        /// ISO 4217 currency code for the revenue value. Defaults to USD.
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Set to true when Revenue is already expressed in micros.
        /// </summary>
        public bool RevenueInMicros { get; set; }

        /// <summary>
        /// Ad network or mediation source such as admob or applovin_max.
        /// </summary>
        public string? AdNetwork { get; set; }

        /// <summary>
        /// Ad format such as banner, interstitial, rewarded, or native.
        /// </summary>
        public string? AdFormat { get; set; }

        /// <summary>
        /// Optional app-defined subtype such as impression or paid_event.
        /// </summary>
        public string? AdType { get; set; }

        /// <summary>
        /// In-app placement or slot identifier for the monetized impression.
        /// </summary>
        public string? AdPlacement { get; set; }

        /// <summary>
        /// Marks sandbox, QA, or test monetization callbacks.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// Extra JSON fields merged before the typed ad fields are applied.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Flushes the queued ad revenue event immediately. Defaults to true.
        /// </summary>
        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for a canonical ad lifecycle event.
    /// </summary>
    public sealed class AttriaxRecordAdEventOptions
    {
        /// <summary>
        /// Ad network or mediation source such as admob or applovin_max.
        /// </summary>
        public string? AdNetwork { get; set; }

        /// <summary>
        /// Mediation layer when it differs from the serving network.
        /// </summary>
        public string? MediationNetwork { get; set; }

        /// <summary>
        /// Provider-side ad unit id or slot identifier.
        /// </summary>
        public string? AdUnitId { get; set; }

        /// <summary>
        /// In-app placement or surface label for the ad.
        /// </summary>
        public string? AdPlacement { get; set; }

        /// <summary>
        /// Ad format such as banner, interstitial, rewarded, or native.
        /// </summary>
        public string? AdFormat { get; set; }

        /// <summary>
        /// Optional app-defined subtype such as rewarded_interstitial.
        /// </summary>
        public string? AdType { get; set; }

        /// <summary>
        /// Error or provider failure reason for load/show failures.
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// Load latency in milliseconds when the provider exposes it.
        /// </summary>
        public double? LoadLatencyMs { get; set; }

        /// <summary>
        /// Reward currency or reward label when the ad grants one.
        /// </summary>
        public string? RewardType { get; set; }

        /// <summary>
        /// Reward quantity granted by the ad event.
        /// </summary>
        public double? RewardAmount { get; set; }

        /// <summary>
        /// Marks sandbox, QA, or test ad lifecycle callbacks.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// Extra JSON fields merged before the typed ad fields are applied.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Flushes the queued ad event immediately. Defaults to true.
        /// </summary>
        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for manual crash and error reporting.
    /// </summary>
    public sealed class AttriaxRecordErrorOptions
    {
        public string Source { get; set; } = "manual";

        public bool IsFatal { get; set; }

        public string? Reason { get; set; }

        public DateTimeOffset? OccurredAt { get; set; }

        public IDictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Push-notification lifecycle stages attributed by the Attriax SDKs.
    /// Attriax never sends pushes itself — the host application's own FCM/APNs
    /// handler reports these events and passes through any Attriax link/campaign
    /// reference embedded in the notification payload.
    /// </summary>
    public enum AttriaxNotificationEventType
    {
        /// <summary>
        /// The notification was delivered to / displayed on the device.
        /// </summary>
        Received,

        /// <summary>
        /// The user opened (tapped) the notification.
        /// </summary>
        Opened,

        /// <summary>
        /// The user dismissed the notification without opening it.
        /// </summary>
        Dismissed,
    }

    /// <summary>
    /// Delivery channel a push notification arrived through.
    /// </summary>
    public enum AttriaxNotificationEventSource
    {
        /// <summary>
        /// Firebase Cloud Messaging (Android and cross-platform).
        /// </summary>
        Fcm,

        /// <summary>
        /// Apple Push Notification service (iOS / macOS).
        /// </summary>
        Apns,

        /// <summary>
        /// Any other / unknown delivery channel.
        /// </summary>
        Other,
    }

    /// <summary>
    /// Options for a push-notification lifecycle attribution event.
    /// </summary>
    public sealed class AttriaxRecordNotificationOptions
    {
        /// <summary>
        /// Optional reference to an existing Attriax tracked link embedded in the
        /// notification payload.
        /// </summary>
        public string? LinkId { get; set; }

        /// <summary>
        /// Optional reference to an existing Attriax campaign this notification relates to.
        /// </summary>
        public string? CampaignId { get; set; }

        /// <summary>
        /// Optional human-readable notification title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Delivery channel the notification arrived through. When left null and a
        /// <see cref="Payload"/> is supplied, the source is inferred from the payload
        /// shape (an <c>aps</c> envelope means APNs, a <c>google.*</c>/<c>gcm.*</c> key
        /// means FCM); otherwise the server falls back to <c>other</c>.
        /// </summary>
        public AttriaxNotificationEventSource? Source { get; set; }

        /// <summary>
        /// Raw FCM/APNs data map. Preserved under a <c>payload</c> key inside the
        /// notification metadata so attribution context survives the trip to the server.
        /// Explicit <see cref="Metadata"/> entries take precedence.
        /// </summary>
        public IDictionary<string, object>? Payload { get; set; }

        /// <summary>
        /// Additional metadata to attach to the notification event.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Forces an immediate flush of the offline queue after enqueueing.
        /// </summary>
        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for a standardized page-view event.
    /// </summary>
    public sealed class AttriaxPageViewOptions
    {
        public string? PageClass { get; set; }

        public string? PageTitle { get; set; }

        public string? PreviousPageName { get; set; }

        public IDictionary<string, object>? Parameters { get; set; }

        public string Source { get; set; } = "manual";

        public bool? FlushImmediately { get; set; }
    }

    /// <summary>
    /// Options for user updates that apply to future tracked events.
    /// </summary>
    public class AttriaxSetUserOptions
    {
        public string? UserName { get; set; }

        public IDictionary<string, object>? Properties { get; set; }

        public ICollection<string>? ClearPropertyKeys { get; set; }

        public bool ClearAllProperties { get; set; }
    }

    /// <summary>
    /// Backward-compatible alias for <see cref="AttriaxSetUserOptions"/>.
    /// </summary>
    public sealed class AttriaxIdentifyOptions : AttriaxSetUserOptions
    {
    }

    /// <summary>
    /// Options for server-created dynamic links.
    /// </summary>
    public sealed class AttriaxCreateDynamicLinkOptions
    {
        /// <summary>
        /// Optional display name retained in the Attriax dashboard.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Destination URL opened when the dynamic link resolves outside the app.
        /// </summary>
        public string? DestinationUrl { get; set; }

        /// <summary>
        /// Optional grouping key for dashboard organization and reporting.
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Optional path prefix for the generated short link, for example <c>/promo</c>.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Controls whether iOS visitors should be redirected according to project settings.
        /// Leave null to use the backend default for the project.
        /// </summary>
        public bool? IOSRedirect { get; set; }

        /// <summary>
        /// Controls whether Android visitors should be redirected according to project settings.
        /// Leave null to use the backend default for the project.
        /// </summary>
        public bool? AndroidRedirect { get; set; }

        public string? PreviewTitle { get; set; }

        public string? PreviewDescription { get; set; }

        public string? PreviewImagePath { get; set; }

        public string? UtmSource { get; set; }

        public string? UtmMedium { get; set; }

        public string? UtmCampaign { get; set; }

        public string? UtmTerm { get; set; }

        public string? UtmContent { get; set; }

        public IDictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// Options for manual deep-link resolution.
    /// </summary>
    public sealed class AttriaxDeepLinkConversionOptions
    {
        /// <summary>
        /// Full deep-link URL or path-like URI that should be resolved and recorded.
        /// </summary>
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// Extra JSON metadata attached to the manual deep-link resolution request.
        /// </summary>
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Optional source label. Defaults to <c>manual</c> or <c>initial_url</c>.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Marks this conversion as the startup URL for the current session.
        /// </summary>
        public bool IsInitialLink { get; set; }
    }

    /// <summary>
    /// SDK version and metadata snapshot sent with app-open tracking.
    /// </summary>
    public sealed class AttriaxSdkSnapshot
    {
        public string ApiVersion { get; set; } = string.Empty;

        public string PackageVersion { get; set; } = string.Empty;

        public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Application version information sent with app-open tracking.
    /// </summary>
    internal sealed class AttriaxAppSnapshot
    {
        public string? Version { get; set; }

        public string? BuildNumber { get; set; }

        public string? PackageName { get; set; }
    }

    /// <summary>
    /// Device context captured for the current runtime.
    /// </summary>
    internal sealed class AttriaxDeviceSnapshot
    {
        public string? Model { get; set; }

        public string? Name { get; set; }

        public string? Brand { get; set; }

        public string? Manufacturer { get; set; }

        public string? Hardware { get; set; }

        public string? OsVersion { get; set; }

        public string? Language { get; set; }

        public string? Timezone { get; set; }

        public string? ScreenResolution { get; set; }

        public int? ScreenWidth { get; set; }

        public int? ScreenHeight { get; set; }

        public double? DevicePixelRatio { get; set; }

        public int? ColorDepth { get; set; }

        public string? AdvertisingId { get; set; }

        public string? AndroidId { get; set; }

        public bool? IsPhysicalDevice { get; set; }

        public IList<string> SupportedAbis { get; set; } = new List<string>();

        public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Full runtime context collected for an initialization request.
    /// </summary>
    internal sealed class AttriaxContextSnapshot
    {
        public AttriaxPlatformType Platform { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public bool IsFirstLaunch { get; set; }

        public string? RawPlatformInstallReferrer { get; set; }

        public long? InstallBeginTimestampSeconds { get; set; }

        public long? ReferrerClickTimestampSeconds { get; set; }

        public bool? GooglePlayInstantParam { get; set; }

        [Obsolete("Use RawPlatformInstallReferrer instead.")]
        public string? InstallReferrer
        {
            get { return RawPlatformInstallReferrer; }
            set { RawPlatformInstallReferrer = value; }
        }

        public AttriaxSdkSnapshot Sdk { get; set; } = new AttriaxSdkSnapshot();

        public AttriaxAppSnapshot App { get; set; } = new AttriaxAppSnapshot();

        public AttriaxDeviceSnapshot Device { get; set; } = new AttriaxDeviceSnapshot();
    }

    /// <summary>
    /// Deep-link record returned by the Attriax API.
    /// </summary>
    public sealed class AttriaxDeepLink
    {
        public string Path { get; set; } = string.Empty;

        public IDictionary<string, object>? Data { get; set; }

        public Uri? Uri { get; set; }

        public AttriaxUtmParameters? Utm { get; set; }
    }

    /// <summary>
    /// UTM parameters resolved for a deep-link payload.
    /// </summary>
    public sealed class AttriaxUtmParameters
    {
        public string? Source { get; set; }

        public string? Medium { get; set; }

        public string? Campaign { get; set; }

        public string? Term { get; set; }

        public string? Content { get; set; }
    }

    /// <summary>
    /// Structured install-referrer details returned by the Attriax API.
    /// </summary>
    public sealed class AttriaxInstallReferrerDetails
    {
        /// <summary>
        /// Raw platform install-referrer string cached by the SDK.
        /// </summary>
        public string? RawPlatformInstallReferrer { get; set; }

        /// <summary>
        /// Resolved UTM source extracted from the install referrer.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Resolved UTM medium extracted from the install referrer.
        /// </summary>
        public string? Medium { get; set; }

        /// <summary>
        /// Resolved UTM campaign extracted from the install referrer.
        /// </summary>
        public string? Campaign { get; set; }

        /// <summary>
        /// Resolved UTM term extracted from the install referrer.
        /// </summary>
        public string? Term { get; set; }

        /// <summary>
        /// Resolved UTM content extracted from the install referrer.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Detected ad-network identifier inferred from the referrer.
        /// </summary>
        public string? AdNetwork { get; set; }

        /// <summary>
        /// Detected ad click identifier such as gclid or fbclid.
        /// </summary>
        public string? AdClickId { get; set; }

        /// <summary>
        /// Attribution classification for the install-referrer payload.
        /// Current platform install-referrer parsing reports Referrer.
        /// External is reserved for future provider-based payloads.
        /// </summary>
        public AttributionType AttributionType { get; set; }

        /// <summary>
        /// Full tracked short-link URI associated with the resolved deep link.
        /// </summary>
        public string? DeepLinkUri { get; set; }

        /// <summary>
        /// Deprecated alias for DeepLinkUri kept for API compatibility.
        /// </summary>
        [Obsolete("Use DeepLinkUri instead.")]
        public string? DeepLinkUrl
        {
            get { return DeepLinkUri; }
            set { DeepLinkUri = value; }
        }

        /// <summary>
        /// Resolved deep-link payload data associated with the install referrer.
        /// </summary>
        public IDictionary<string, object>? DeepLinkData { get; set; }

        /// <summary>
        /// Server timestamp for when Attriax first registered this startup referrer.
        /// </summary>
        public DateTimeOffset? RegisteredAt { get; set; }

        /// <summary>
        /// Platform-reported install-begin timestamp in seconds, when available.
        /// </summary>
        public long? InstallBeginTimestampSeconds { get; set; }

        /// <summary>
        /// Platform-reported referrer-click timestamp in seconds, when available.
        /// </summary>
        public long? ReferrerClickTimestampSeconds { get; set; }

        /// <summary>
        /// Whether Google Play reported this launch as an instant experience, when available.
        /// </summary>
        public bool? GooglePlayInstantParam { get; set; }

        /// <summary>
        /// Confidence score from 0.0 to 1.0 for the returned interpretation.
        /// </summary>
        public double Precision { get; set; }
    }

    /// <summary>
    /// Dynamic-link definition returned by the API.
    /// </summary>
    public sealed class AttriaxDynamicLinkRecord
    {
        public string Id { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string ShortUrl { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? DestinationUrl { get; set; }

        public string? Group { get; set; }

        public string? Prefix { get; set; }

        public IDictionary<string, object>? Data { get; set; }

        public string? PreviewTitle { get; set; }

        public string? PreviewDescription { get; set; }

        public string? PreviewImagePath { get; set; }

        public bool? IOSRedirect { get; set; }

        public bool? AndroidRedirect { get; set; }

        public string? UtmSource { get; set; }

        public string? UtmMedium { get; set; }

        public string? UtmCampaign { get; set; }

        public string? UtmTerm { get; set; }

        public string? UtmContent { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }
    }

    /// <summary>
    /// Response for a successful dynamic-link creation request.
    /// </summary>
    public sealed class AttriaxCreateDynamicLinkResult
    {
        public string? RequestVersion { get; set; }

        public DateTimeOffset? AcceptedAt { get; set; }

        public AttriaxDynamicLinkRecord Link { get; set; } = new AttriaxDynamicLinkRecord();
    }

    /// <summary>
    /// Raw deep-link activation observed directly by the SDK.
    /// </summary>
    public sealed class AttriaxRawDeepLinkEvent
    {
        /// <summary>
        /// Fully normalized URL captured by the SDK.
        /// </summary>
        public Uri Uri { get; set; } = new Uri("https://attriax.invalid/");

        /// <summary>
        /// Timestamp for when the SDK observed the raw deep-link input.
        /// </summary>
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Whether this event came from the initial URL captured during initialization.
        /// </summary>
        public bool IsInitial { get; set; }
    }

    /// <summary>
    /// Session-scoped deep-link referrer details derived from handled deep-link events.
    /// </summary>
    public sealed class AttriaxDeepLinkReferrerDetails
    {
        /// <summary>
        /// Canonical deep-link URL returned by Attriax, or the incoming URL.
        /// </summary>
        public Uri Uri { get; set; } = new Uri("https://attriax.invalid/");

        /// <summary>
        /// Local timestamp when the SDK first observed the deep-link input.
        /// </summary>
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Best-known timestamp for when the originating click happened.
        /// </summary>
        public DateTimeOffset ClickedAt { get; set; }

        /// <summary>
        /// Timestamp recorded when Attriax processed the deep-link event.
        /// </summary>
        public DateTimeOffset ConsumedAt { get; set; }

        /// <summary>
        /// Describes how this deep-link event entered the SDK runtime.
        /// </summary>
        public AttriaxDeepLinkTrigger Trigger { get; set; }

        /// <summary>
        /// Whether the resolved URL belongs to an Attriax-managed subdomain.
        /// </summary>
        public bool IsAttriaxDomain { get; set; }

        /// <summary>
        /// Whether Attriax matched this event to a registered link.
        /// </summary>
        public bool Found { get; set; }

        /// <summary>
        /// Dashboard-configured payload returned for matched links.
        /// </summary>
        public IDictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Resolved UTM parameters associated with the matched link.
        /// </summary>
        public AttriaxUtmParameters? Utm { get; set; }

        /// <summary>
        /// Browser destination returned by Attriax for SDK-managed handling.
        /// </summary>
        public AttriaxResolvedUrlAction? BrowserAction { get; set; }

        /// <summary>
        /// Whether the SDK already handled the link by opening a browser.
        /// </summary>
        public bool HandledBySdk { get; set; }
    }

    /// <summary>
    /// Browser destination returned by Attriax for SDK-managed handling.
    /// </summary>
    public sealed class AttriaxResolvedUrlAction
    {
        /// <summary>
        /// Absolute URL that should be opened by the SDK.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Preferred browser-opening mode returned by the backend.
        /// </summary>
        public AttriaxResolvedUrlOpenMode OpenMode { get; set; }
    }

    /// <summary>
    /// Public app-open snapshot exposed after the first app-open request completes.
    /// </summary>
    public sealed class AttriaxAppOpen
    {
        /// <summary>
        /// Whether the Attriax backend created a new user for this app-open request.
        /// </summary>
        public bool IsNewUser { get; set; }

        /// <summary>
        /// Whether the tracked app-open still belongs to the first launch flow.
        /// </summary>
        public bool IsFirstLaunch { get; set; }

        /// <summary>
        /// Install classification returned for the current app-open request.
        /// </summary>
        public AttriaxInstallState InstallState { get; set; }

        /// <summary>
        /// Deferred deep-link payload returned for the app-open request, if any.
        /// </summary>
        public AttriaxDeepLink? DeepLink { get; set; }

        /// <summary>
        /// Timestamp for the matched deferred click, when the app-open returned one.
        /// </summary>
        public DateTimeOffset? DeepLinkClickedAt { get; set; }

        /// <summary>
        /// Timestamp for when Attriax consumed the deferred deep link, when available.
        /// </summary>
        public DateTimeOffset? DeepLinkConsumedAt { get; set; }

        /// <summary>
        /// Original install-referrer details retained for this device, if any.
        /// </summary>
        public AttriaxInstallReferrerDetails? OriginalInstallReferrer { get; set; }

        /// <summary>
        /// Latest reinstall-referrer details retained for this device, if any.
        /// </summary>
        public AttriaxInstallReferrerDetails? ReinstallReferrer { get; set; }

        /// <summary>
        /// Latest dashboard-managed SKAN runtime configuration returned for this app-open request.
        /// </summary>
        public AttriaxSkanRuntimeConfiguration? Skan { get; set; }

        /// <summary>
        /// Deprecated alias for the current startup referrer.
        /// </summary>
        [Obsolete("Use OriginalInstallReferrer or ReinstallReferrer instead.")]
        public AttriaxInstallReferrerDetails? InstallReferrer
        {
            get { return ReinstallReferrer ?? OriginalInstallReferrer; }
        }
    }

    /// <summary>
    /// Internal response payload for the initial app-open tracking request.
    /// </summary>
    internal sealed class AttriaxAppOpenResult
    {
        public string UserId { get; set; } = string.Empty;

        public bool IsNewUser { get; set; }

        public bool IsFirstLaunch { get; set; }

        public AttriaxInstallState InstallState { get; set; }

        public string? RequestVersion { get; set; }

        public DateTimeOffset? AcceptedAt { get; set; }

        public AttriaxDeepLink? DeepLink { get; set; }

        public DateTimeOffset? DeepLinkClickedAt { get; set; }

        public DateTimeOffset? DeepLinkConsumedAt { get; set; }

        public AttriaxInstallReferrerDetails? OriginalInstallReferrer { get; set; }

        public AttriaxInstallReferrerDetails? ReinstallReferrer { get; set; }

        public AttriaxSkanRuntimeConfiguration? Skan { get; set; }

        public AttriaxInstallReferrerDetails? InstallReferrer =>
            ReinstallReferrer ?? OriginalInstallReferrer;
    }

    /// <summary>
    /// Resolved deep-link event emitted after Attriax processing completes.
    /// </summary>
    public sealed class AttriaxDeepLinkEvent
    {
        public Uri Uri { get; set; } = new Uri("https://attriax.invalid/");

        public DateTimeOffset ClickedAt { get; set; }

        public DateTimeOffset ConsumedAt { get; set; }

        public bool Found { get; set; }

        public AttriaxDeepLinkTrigger Trigger { get; set; }

        public AttriaxRawDeepLinkEvent? RawEvent { get; set; }

        public IDictionary<string, object>? Data { get; set; }

        public AttriaxUtmParameters? Utm { get; set; }

        public AttriaxResolvedUrlAction? BrowserAction { get; set; }

        public bool HandledBySdk { get; set; }

        public bool IsDeferred
        {
            get { return Trigger == AttriaxDeepLinkTrigger.Deferred; }
        }

        public bool IsColdStart
        {
            get { return Trigger == AttriaxDeepLinkTrigger.ColdStart; }
        }

        public bool IsForeground
        {
            get { return Trigger == AttriaxDeepLinkTrigger.Foreground; }
        }

        public bool IsAttriaxSubDomain
        {
            get
            {
                var host = Uri.Host;
                return !string.IsNullOrWhiteSpace(host)
                    && host.EndsWith(".attriax.com", StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    internal sealed class AttriaxSubscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public AttriaxSubscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _unsubscribe();
        }
    }
}
