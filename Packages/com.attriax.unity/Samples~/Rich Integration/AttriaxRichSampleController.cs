#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using AttriaxSdk = Attriax.Unity.Attriax;

namespace Attriax.Unity.Samples
{
    /// <summary>
    /// Rich-sample controller: the single owner of every Attriax SDK call in the
    /// sample. The uGUI view (<see cref="AttriaxRichSampleView"/>) never touches the
    /// SDK directly — it only invokes the public methods here and renders the
    /// snapshot + recent-activity log this controller exposes.
    ///
    /// This mirrors the Flutter rich example's <c>ExampleAppController</c>: one
    /// global instance, an awaited init, a consent gate, and a scrollable
    /// recent-activity feed. Because Unity is the API SUPERSET, this sample also
    /// exercises ATT, SKAN, data-erasure, and push-token registration — features
    /// the JS / React-Native SDKs do not expose. iOS-only surfaces (ATT, SKAN) are
    /// gated and clearly no-op with a logged reason on desktop / Editor / Android.
    ///
    /// Threading: SDK subscription callbacks (synchronization + deep links) can
    /// arrive on a background thread, so this controller only ever mutates plain
    /// data and flips a volatile dirty flag. The view polls <see cref="IsDirty"/>
    /// on the Unity main thread and re-renders — no cross-thread UI mutation.
    /// </summary>
    [AddComponentMenu("Attriax/Samples/Attriax Rich Sample Controller")]
    public sealed class AttriaxRichSampleController : MonoBehaviour
    {
        /// <summary>
        /// Which of the three public integration paths this sample uses to obtain
        /// the runtime. All three are demonstrated by the Basic Integration sample;
        /// the Rich sample lets you pick one and then drives the full API on top.
        /// </summary>
        public enum InitPath
        {
            /// <summary>A fully manual <c>new Attriax(config)</c> flow (default).</summary>
            ManualRuntime,

            /// <summary>The configured-singleton flow (Tools &gt; Attriax &gt; Configuration).</summary>
            ConfiguredSingleton,

            /// <summary>A scene <see cref="AttriaxBehaviour"/> host assigned in the inspector.</summary>
            BehaviourHost,
        }

        [Header("Integration path")]
        [SerializeField] private InitPath _initPath = InitPath.ManualRuntime;
        [SerializeField] private AttriaxBehaviour? _behaviourHost;

        [Header("Manual runtime configuration")]
        [Tooltip("Project token used by the ManualRuntime path. Replace with your own project token.")]
        [SerializeField] private string _projectToken = "ax_your_project_token";
        [Tooltip("Optional API base URL override for local / staging environments.")]
        [SerializeField] private string _apiBaseUrl = string.Empty;
        [SerializeField] private bool _enableDebugLogs = true;
        [Tooltip("Enables GDPR gating so the consent surfaces can be demonstrated end to end.")]
        [SerializeField] private bool _gdprEnabled = true;

        [Header("Lifecycle")]
        [Tooltip("Start the SDK automatically when the scene enters play mode.")]
        [SerializeField] private bool _autoStart = true;

        private const int MaxActivityEntries = 30;

        private readonly object _activityLock = new object();
        private readonly List<ActivityEntry> _recentActivity = new List<ActivityEntry>();

        private AttriaxSdk? _sdk;
        private IDisposable? _syncSubscription;
        private IDisposable? _deepLinkSubscription;
        private bool _started;
        private volatile bool _isDirty = true;
        private bool _ownsSdk;

        /// <summary>Human-readable status line rendered above the action buttons.</summary>
        public string StatusMessage { get; private set; } = "Not started. Press Start to initialize Attriax.";

        /// <summary>Latest short summary of the most recent deep-link resolution.</summary>
        public string DeepLinkSummary { get; private set; } = "none";

        /// <summary>Latest referrer summary refreshed by the referrer buttons.</summary>
        public string ReferrerSummary { get; private set; } = "not refreshed";

        /// <summary>Latest SKAN / ATT / receipt / push summary rendered in the snapshot.</summary>
        public string ExtrasSummary { get; private set; } = "n/a";

        /// <summary>True once the view should re-render from the current snapshot.</summary>
        public bool IsDirty => _isDirty;

        /// <summary>Underlying SDK instance, or null before <see cref="StartSampleAsync"/> completes.</summary>
        public AttriaxSdk? Sdk => _sdk;

        /// <summary>Whether iOS-only surfaces (ATT, SKAN) can actually run on this platform.</summary>
        public bool IosFeaturesAvailable => Application.platform == RuntimePlatform.IPhonePlayer;

        /// <summary>True while the SDK is still waiting for an explicit GDPR decision.</summary>
        public bool IsWaitingForConsent => _sdk?.Consent.Gdpr.IsWaitingForConsent ?? false;

        private void Start()
        {
            if (_autoStart && Application.isPlaying)
            {
                _ = StartSampleAsync();
            }
        }

        private void OnDestroy()
        {
            Teardown();
        }

        /// <summary>
        /// Marks the current snapshot as consumed. The view calls this after it
        /// re-renders so it can poll <see cref="IsDirty"/> cheaply each frame.
        /// </summary>
        public void ClearDirty()
        {
            _isDirty = false;
        }

        /// <summary>
        /// Configures the manual-runtime path from code (used by the Editor smoke
        /// harness and by hosts that construct the controller programmatically).
        /// Must be called before <see cref="StartSampleAsync"/>.
        /// </summary>
        public void ConfigureManual(string projectToken, string? apiBaseUrl, bool enableDebugLogs, bool gdprEnabled)
        {
            if (_started)
            {
                throw new InvalidOperationException("Configure the controller before starting the sample.");
            }

            _initPath = InitPath.ManualRuntime;
            _projectToken = projectToken;
            _apiBaseUrl = apiBaseUrl ?? string.Empty;
            _enableDebugLogs = enableDebugLogs;
            _gdprEnabled = gdprEnabled;
        }

        // ---------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------

        /// <summary>
        /// Resolves the runtime by the selected init path, wires the synchronization
        /// and deep-link subscriptions, and refreshes startup diagnostics.
        /// </summary>
        public async Task StartSampleAsync()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            SetStatus("Initializing Attriax...");

            try
            {
                _sdk = await ResolveRuntimeAsync();

                _syncSubscription = _sdk.Synchronization.Subscribe(_ =>
                {
                    // Background thread safe: data + dirty flag only.
                    MarkDirty();
                });
                _deepLinkSubscription = _sdk.DeepLinks.Stream.Subscribe(HandleDeepLinkEvent);

                Log("Initialized",
                    detail: "firstLaunch=" + _sdk.IsFirstLaunch + " deviceId=" + (_sdk.DeviceId ?? "pending"));

                await RefreshDiagnosticsAsync();
            }
            catch (Exception error)
            {
                _started = false;
                Log("Initialization failed", detail: error.Message, isError: true);
                SetStatus("Initialization failed: " + error.Message);
            }
        }

        private async Task<AttriaxSdk> ResolveRuntimeAsync()
        {
            switch (_initPath)
            {
                case InitPath.ConfiguredSingleton:
                    _ownsSdk = false;
                    return await AttriaxSdk.InitializeConfiguredAsync();

                case InitPath.BehaviourHost:
                    if (_behaviourHost == null)
                    {
                        throw new InvalidOperationException(
                            "InitPath.BehaviourHost requires an AttriaxBehaviour reference in the inspector.");
                    }

                    if (!_behaviourHost.IsReady)
                    {
                        await _behaviourHost.InitializeAsync();
                    }

                    _ownsSdk = false;
                    return _behaviourHost.Instance
                        ?? throw new InvalidOperationException("AttriaxBehaviour did not produce an instance.");

                case InitPath.ManualRuntime:
                default:
                    var sdk = new AttriaxSdk(new AttriaxConfig
                    {
                        ProjectToken = _projectToken,
                        ApiBaseUrl = string.IsNullOrWhiteSpace(_apiBaseUrl) ? null : _apiBaseUrl,
                        EnableDebugLogs = _enableDebugLogs,
                        GdprEnabled = _gdprEnabled,
                    });
                    _ownsSdk = true;
                    await sdk.InitializeAsync(new AttriaxInitOptions { CaptureInitialUrl = true });
                    return sdk;
            }
        }

        /// <summary>
        /// Refreshes the diagnostic snapshot: initial deep-link probe, referrers,
        /// and identity. Honors the consent gate the same way the Flutter example
        /// does — no attribution work is forced while consent is still pending.
        /// </summary>
        public async Task RefreshDiagnosticsAsync()
        {
            var sdk = RequireSdk();

            try
            {
                if (sdk.Consent.Gdpr.IsWaitingForConsent)
                {
                    SetStatus("GDPR consent pending. Resolve consent before attribution starts.");
                    Log("GDPR consent pending", detail: sdk.Consent.Gdpr.State.ToString());
                    return;
                }

                var initial = await sdk.DeepLinks.WaitForInitialDeepLink;
                DeepLinkSummary = DescribeDeepLink(initial ?? sdk.DeepLinks.LatestDeepLink);

                var original = await sdk.Referrer.GetOriginalInstallReferrerAsync();
                var reinstall = await sdk.Referrer.GetReinstallReferrerAsync();
                ReferrerSummary = "original=" + DescribeReferrer(original) + " reinstall=" + DescribeReferrer(reinstall);

                SetStatus("SDK ready. Drive the buttons to exercise the full Attriax surface.");
                Log("Diagnostics refreshed", detail: sdk.Synchronization.State.ToString());
            }
            catch (Exception error)
            {
                Log("Diagnostics refresh failed", detail: error.Message, isError: true);
                SetStatus("Loaded with partial diagnostics: " + error.Message);
            }
        }

        /// <summary>Clears SDK-owned persisted state and returns to pre-init state.</summary>
        public async Task ResetAsync()
        {
            var sdk = RequireSdk();
            try
            {
                await sdk.ResetAsync();
                Log("ResetAsync", detail: "SDK returned to pre-init state");
                SetStatus("SDK reset. Call Start again to re-initialize.");
                _started = false;
            }
            catch (Exception error)
            {
                Log("ResetAsync failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Enable toggles
        // ---------------------------------------------------------------------

        public void ToggleSdkEnabled()
        {
            var sdk = RequireSdk();
            sdk.Enabled = !sdk.Enabled;
            Log("Attriax.Enabled", detail: sdk.Enabled.ToString());
            SetStatus("SDK " + (sdk.Enabled ? "enabled" : "disabled") + ".");
        }

        public void ToggleEventsEnabled()
        {
            var sdk = RequireSdk();
            sdk.Tracking.Enabled = !sdk.Tracking.Enabled;
            Log("Tracking.Enabled", detail: sdk.Tracking.Enabled.ToString());
        }

        public void ToggleAnonymousTracking()
        {
            var sdk = RequireSdk();
            sdk.Tracking.AnonymousTrackingEnabled = !sdk.Tracking.AnonymousTrackingEnabled;
            Log("Tracking.AnonymousTrackingEnabled", detail: sdk.Tracking.AnonymousTrackingEnabled.ToString());
        }

        // ---------------------------------------------------------------------
        // Events / revenue
        // ---------------------------------------------------------------------

        public void RecordCustomEvent()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordEvent("unity_rich_custom_event", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object>
                {
                    ["scene"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    ["surface"] = "rich_sample",
                    ["at"] = DateTimeOffset.UtcNow.ToString("o"),
                },
                FlushImmediately = true,
            });
            Log("recordEvent", detail: "unity_rich_custom_event");
        }

        public void RecordPageView()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordPageView("rich_sample_home", new AttriaxPageViewOptions
            {
                PageClass = "AttriaxRichSample",
                PageTitle = "Rich Sample Home",
                PreviousPageName = "launcher",
                Source = "rich_sample_button",
                Parameters = new Dictionary<string, object> { ["tab"] = "events" },
            });
            Log("recordPageView", detail: "rich_sample_home");
        }

        public void RecordPurchase()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordPurchase(9.99, new AttriaxRecordPurchaseOptions
            {
                Currency = "USD",
                PurchaseType = "subscription_initial",
                ProductId = "pro_monthly",
                TransactionId = "txn_unity_demo_1001",
                OriginalTransactionId = "txn_unity_demo_root_1001",
                Store = "google_play",
                PackageName = "com.attriax.unity.sample",
                Test = true,
                Metadata = new Dictionary<string, object> { ["paywall"] = "starter_offer" },
            });
            Log("recordPurchase", detail: "pro_monthly / 9.99 USD");
        }

        public void RecordRefund()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordRefund(9.99, new AttriaxRecordRefundOptions
            {
                Currency = "USD",
                PurchaseType = "subscription_initial",
                ProductId = "pro_monthly",
                TransactionId = "refund_unity_demo_1001",
                OriginalTransactionId = "txn_unity_demo_root_1001",
                Store = "google_play",
                Reason = "customer_support_demo",
                Test = true,
            });
            Log("recordRefund", detail: "pro_monthly / 9.99 USD");
        }

        public void RecordAdRevenue()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordAdRevenue(1200, new AttriaxRecordAdRevenueOptions
            {
                Currency = "USD",
                RevenueInMicros = true,
                AdNetwork = "admob",
                AdFormat = "rewarded",
                AdType = "paid_event",
                AdPlacement = "rewarded_end_of_level",
                Test = true,
            });
            Log("recordAdRevenue", detail: "0.0012 USD (rewarded)");
        }

        public void RecordAdEvent()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordAdEvent(AttriaxAdEventType.Reward, new AttriaxRecordAdEventOptions
            {
                AdNetwork = "admob",
                MediationNetwork = "applovin_max",
                AdUnitId = "demo_unit_01",
                AdPlacement = "rewarded_end_of_level",
                AdFormat = "rewarded",
                RewardType = "coins",
                RewardAmount = 25,
                Test = true,
            });
            Log("recordAdEvent", detail: "reward / rewarded_end_of_level");
        }

        public void RecordError()
        {
            var sdk = RequireSdk();
            var error = new InvalidOperationException("Rich sample demo handled error");
            sdk.Tracking.RecordError(error, new AttriaxRecordErrorOptions
            {
                Source = "rich_sample",
                IsFatal = false,
                Reason = "demo_button",
                Metadata = new Dictionary<string, object> { ["surface"] = "events_page" },
            });
            Log("recordError", detail: "handled InvalidOperationException");
        }

        public void RecordNotificationOpened()
        {
            var sdk = RequireSdk();
            sdk.Tracking.RecordNotificationOpened("ntf_rich_demo_1", new AttriaxRecordNotificationOptions
            {
                LinkId = "lnk_promo_winback",
                CampaignId = "cmp_summer_sale",
                Title = "Your boost is waiting",
                Source = AttriaxNotificationEventSource.Fcm,
                Metadata = new Dictionary<string, object> { ["surface"] = "rich_sample" },
                FlushImmediately = true,
            });
            Log("recordNotificationOpened", detail: "ntf_rich_demo_1");
        }

        // ---------------------------------------------------------------------
        // Identity
        // ---------------------------------------------------------------------

        public void SetExampleUser()
        {
            var sdk = RequireSdk();
            sdk.Tracking.SetUser("demo-user-1", new AttriaxSetUserOptions
            {
                UserName = "Unity Rich Sample User",
                Properties = new Dictionary<string, object> { ["plan"] = "growth" },
            });
            Log("setUser", detail: "demo-user-1 (Unity Rich Sample User)");
        }

        public void ClearExampleUser()
        {
            var sdk = RequireSdk();
            sdk.Tracking.SetUser(null);
            Log("setUser", detail: "cleared");
        }

        public void IdentifyExampleUser()
        {
            var sdk = RequireSdk();
            // Tracking.Identify is the obsolete alias for SetUser, kept for
            // backward compatibility. Demonstrated here so the sample surfaces the
            // full identity API; new code should call SetUser instead.
#pragma warning disable 618
            sdk.Tracking.Identify("demo-user-2", new AttriaxIdentifyOptions
            {
                UserName = "Identify Alias User",
            });
#pragma warning restore 618
            Log("identify (obsolete alias)", detail: "demo-user-2");
        }

        public void SetUserProperty()
        {
            var sdk = RequireSdk();
            sdk.Tracking.SetUserProperty("cohort", "spring_launch");
            Log("setUserProperty", detail: "cohort=spring_launch");
        }

        public void SetUserProperties()
        {
            var sdk = RequireSdk();
            sdk.Tracking.SetUserProperties(new Dictionary<string, object?>
            {
                ["plan"] = "growth",
                ["cohort"] = "spring_launch",
                ["notifications_enabled"] = true,
            });
            Log("setUserProperties", detail: "plan, cohort, notifications_enabled");
        }

        public void ClearUserProperties()
        {
            var sdk = RequireSdk();
            sdk.Tracking.ClearUserProperties();
            Log("clearUserProperties");
        }

        // ---------------------------------------------------------------------
        // GDPR consent (gating UX)
        // ---------------------------------------------------------------------

        public void AcceptConsent()
        {
            var sdk = RequireSdk();
            sdk.Consent.Gdpr.SetConsent(analytics: true, attribution: true, adEvents: true);
            Log("consent.gdpr.setConsent", detail: "analytics+attribution+adEvents");
            SetStatus("GDPR consent granted. Attribution can now start.");
            _ = RefreshDiagnosticsAsync();
        }

        public void RejectConsent()
        {
            var sdk = RequireSdk();
            sdk.Consent.Gdpr.SetConsent(analytics: false, attribution: false, adEvents: false);
            Log("consent.gdpr.setConsent", detail: "all denied");
            SetStatus("GDPR consent denied.");
        }

        public void MarkConsentNotRequired()
        {
            var sdk = RequireSdk();
            sdk.Consent.Gdpr.SetNotRequired();
            Log("consent.gdpr.setNotRequired", detail: sdk.Consent.Gdpr.State.ToString());
        }

        public void ResetConsent()
        {
            var sdk = RequireSdk();
            sdk.Consent.Gdpr.Reset();
            Log("consent.gdpr.reset", detail: sdk.Consent.Gdpr.State.ToString());
        }

        public async Task CheckNeedsConsentAsync(bool localOnly)
        {
            var sdk = RequireSdk();
            try
            {
                var needs = await sdk.Consent.Gdpr.NeedsConsentAsync(localOnly);
                Log("consent.gdpr.needsConsent",
                    detail: (localOnly ? "local" : "remote") + " -> " + needs);
            }
            catch (Exception error)
            {
                Log("consent.gdpr.needsConsent failed", detail: error.Message, isError: true);
            }
        }

        public async Task RequestDataErasureAsync()
        {
            var sdk = RequireSdk();
            try
            {
                await sdk.Consent.Gdpr.RequestDataErasureAsync();
                Log("consent.gdpr.requestDataErasure", detail: "requested; local state cleared");
                SetStatus("Data erasure requested. SDK returned to pre-init state.");
                _started = false;
            }
            catch (Exception error)
            {
                Log("requestDataErasure failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // ATT (iOS only)
        // ---------------------------------------------------------------------

        public async Task RequestAttAsync()
        {
            var sdk = RequireSdk();
            if (!IosFeaturesAvailable)
            {
                Log("ATT request", detail: "iOS only — no-op on this platform");
                return;
            }

            try
            {
                var status = await sdk.Consent.Att.RequestTrackingAuthorizationAsync();
                ExtrasSummary = "ATT=" + status;
                Log("att.requestTrackingAuthorization", detail: status.ToString());
            }
            catch (Exception error)
            {
                Log("ATT request failed", detail: error.Message, isError: true);
            }
        }

        public async Task RefreshAttStatusAsync()
        {
            var sdk = RequireSdk();
            if (!IosFeaturesAvailable)
            {
                Log("ATT status", detail: "iOS only — no-op on this platform");
                return;
            }

            try
            {
                var status = await sdk.Consent.Att.GetTrackingAuthorizationStatusAsync();
                ExtrasSummary = "ATT=" + status;
                Log("att.getTrackingAuthorizationStatus", detail: status.ToString());
            }
            catch (Exception error)
            {
                Log("ATT status failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // SKAN (iOS only)
        // ---------------------------------------------------------------------

        public void RefreshSkanState()
        {
            var sdk = RequireSdk();
            if (!IosFeaturesAvailable)
            {
                Log("SKAN state", detail: "iOS only — no-op on this platform");
                return;
            }

            var state = sdk.Skan.State;
            ExtrasSummary = "SKAN fine=" + (state?.FineValue?.ToString() ?? "unset")
                + " coarse=" + (state?.CoarseValue?.ToString() ?? "unset");
            Log("skan.state", detail: ExtrasSummary);
        }

        public async Task UpdateSkanConversionValueAsync()
        {
            var sdk = RequireSdk();
            if (!IosFeaturesAvailable)
            {
                Log("SKAN update", detail: "iOS only — no-op on this platform");
                return;
            }

            try
            {
                var result = await sdk.Skan.UpdateConversionValueAsync(
                    fineValue: 32,
                    coarseValue: AttriaxSkanCoarseValue.Medium,
                    lockWindow: false);
                ExtrasSummary = "SKAN update=" + result.Status;
                Log("skan.updateConversionValue",
                    detail: result.Status + (result.Message == null ? string.Empty : ": " + result.Message),
                    isError: result.Status == AttriaxSkanUpdateStatus.Error);
            }
            catch (Exception error)
            {
                Log("SKAN update failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Deep links
        // ---------------------------------------------------------------------

        public async Task CreateDynamicLinkAsync()
        {
            var sdk = RequireSdk();
            try
            {
                var result = await sdk.DeepLinks.CreateDynamicLinkAsync(new AttriaxCreateDynamicLinkOptions
                {
                    Name = "Unity rich sample deep-link demo",
                    DestinationUrl = "https://example-test.attriax.com/example/deep-link-success",
                    Group = "unity_rich_sample",
                    PreviewTitle = "Attriax Unity Example",
                    PreviewDescription = "Open the example and inspect deep-link state.",
                    UtmSource = "unity_rich_sample",
                    UtmMedium = "share",
                    UtmCampaign = "deeplink_demo",
                    IOSRedirect = true,
                    AndroidRedirect = true,
                    Data = new Dictionary<string, object> { ["screen"] = "deep_link_result" },
                });
                DeepLinkSummary = "created " + result.Link.ShortUrl;
                Log("createDynamicLink", detail: result.Link.ShortUrl);
            }
            catch (Exception error)
            {
                Log("createDynamicLink failed", detail: error.Message, isError: true);
            }
        }

        public async Task RecordManualDeepLinkAsync()
        {
            var sdk = RequireSdk();
            try
            {
                var resolution = await sdk.DeepLinks.RecordDeepLinkConversionAsync(new AttriaxDeepLinkConversionOptions
                {
                    Uri = "https://example-test.attriax.com/example/deep-link-success",
                    Source = "rich_sample_manual",
                    Metadata = new Dictionary<string, object> { ["source"] = "rich_sample" },
                });
                DeepLinkSummary = resolution == null
                    ? "manual: no result"
                    : (resolution.Found ? "manual matched " : "manual unmatched ") + resolution.Uri.AbsolutePath;
                Log("recordDeepLinkConversion", detail: DeepLinkSummary);
            }
            catch (Exception error)
            {
                Log("recordDeepLinkConversion failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Referrer (Android-meaningful)
        // ---------------------------------------------------------------------

        public async Task RefreshReferrersAsync()
        {
            var sdk = RequireSdk();
            try
            {
                var original = await sdk.Referrer.GetOriginalInstallReferrerAsync();
                var reinstall = await sdk.Referrer.GetReinstallReferrerAsync();
                var session = await sdk.Referrer.GetSessionReferrerAsync();
                var latest = await sdk.Referrer.GetLatestDeepLinkReferrerAsync();
                ReferrerSummary =
                    "original=" + DescribeReferrer(original)
                    + " reinstall=" + DescribeReferrer(reinstall)
                    + " session=" + DescribeDeepLinkReferrer(session)
                    + " latest=" + DescribeDeepLinkReferrer(latest);
                Log("referrer.* (Android-meaningful)", detail: ReferrerSummary);
            }
            catch (Exception error)
            {
                Log("referrer refresh failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Receipt validation
        // ---------------------------------------------------------------------

        public async Task ValidateReceiptAsync()
        {
            var sdk = RequireSdk();
            try
            {
                var result = await sdk.ValidateReceiptAsync(new AttriaxValidateReceiptOptions
                {
                    Receipt = "unity_demo_receipt_payload",
                    Provider = "unity",
                    Environment = "sandbox",
                    TransactionId = "txn_unity_demo_1001",
                    ProductId = "pro_monthly",
                    Test = true,
                });
                ExtrasSummary = "receipt=" + result.Status;
                Log("validateReceipt", detail: result.Status + " (" + result.ValidationId + ")");
            }
            catch (Exception error)
            {
                Log("validateReceipt failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Push-token registration (may no-op without Firebase/APNs configured)
        // ---------------------------------------------------------------------

        public async Task RegisterFcmTokenAsync()
        {
            var sdk = RequireSdk();
            try
            {
                await sdk.Tracking.RegisterFirebaseMessagingTokenAsync(
                    "demo-fcm-token-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    new Dictionary<string, object> { ["source"] = "rich_sample" });
                Log("registerFirebaseMessagingToken", detail: "demo token registered (needs real Firebase for delivery)");
            }
            catch (Exception error)
            {
                Log("registerFirebaseMessagingToken failed", detail: error.Message, isError: true);
            }
        }

        public async Task RegisterApnsTokenAsync()
        {
            var sdk = RequireSdk();
            try
            {
                await sdk.Tracking.RegisterApplePushTokenAsync(
                    "demo-apns-token-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    new Dictionary<string, object> { ["source"] = "rich_sample" });
                Log("registerApplePushToken", detail: "demo token registered (Apple platforms only)");
            }
            catch (Exception error)
            {
                Log("registerApplePushToken failed", detail: error.Message, isError: true);
            }
        }

        // ---------------------------------------------------------------------
        // Snapshot for the view
        // ---------------------------------------------------------------------

        /// <summary>Builds the multi-line status snapshot rendered above the log.</summary>
        public string BuildSnapshot()
        {
            var sdk = _sdk;
            if (sdk == null)
            {
                return "SDK: not started";
            }

            var snapshot = sdk.SdkSnapshot;
            return
                "initialized=" + sdk.IsInitialized
                + "  enabled=" + sdk.Enabled
                + "  events=" + sdk.Tracking.Enabled
                + "  anon=" + sdk.Tracking.AnonymousTrackingEnabled + "\n"
                + "firstLaunch=" + sdk.IsFirstLaunch
                + "  deviceId=" + (sdk.DeviceId ?? "pending") + "\n"
                + "sync=" + sdk.Synchronization.State
                + " (synchronized=" + sdk.Synchronization.IsSynchronized + ")\n"
                + "consent=" + sdk.Consent.Gdpr.State
                + "  values=" + DescribeConsentValues(sdk) + "\n"
                + "sdk=" + (snapshot == null ? "n/a" : snapshot.PackageVersion + " / api " + snapshot.ApiVersion) + "\n"
                + "deepLink: " + DeepLinkSummary + "\n"
                + "referrer: " + ReferrerSummary + "\n"
                + "extras: " + ExtrasSummary
                + (IosFeaturesAvailable ? string.Empty : "  (ATT/SKAN iOS-only)");
        }

        /// <summary>Returns a copy of the recent-activity feed, newest first.</summary>
        public IReadOnlyList<ActivityEntry> SnapshotActivity()
        {
            lock (_activityLock)
            {
                return new List<ActivityEntry>(_recentActivity);
            }
        }

        // ---------------------------------------------------------------------
        // Internals
        // ---------------------------------------------------------------------

        private void HandleDeepLinkEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            DeepLinkSummary = DescribeDeepLink(deepLinkEvent);
            Log("deepLinks.stream", detail: DeepLinkSummary, isError: !deepLinkEvent.Found);
        }

        private AttriaxSdk RequireSdk()
        {
            return _sdk ?? throw new InvalidOperationException("Start the sample before invoking SDK actions.");
        }

        private void SetStatus(string message)
        {
            StatusMessage = message;
            MarkDirty();
        }

        private void Log(string title, string? detail = null, bool isError = false)
        {
            lock (_activityLock)
            {
                _recentActivity.Insert(0, new ActivityEntry(title, detail, DateTime.Now, isError));
                if (_recentActivity.Count > MaxActivityEntries)
                {
                    _recentActivity.RemoveAt(_recentActivity.Count - 1);
                }
            }

            if (isError)
            {
                Debug.LogWarning("[AttriaxRichSample] " + title + (detail == null ? string.Empty : ": " + detail), this);
            }

            MarkDirty();
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }

        private void Teardown()
        {
            _syncSubscription?.Dispose();
            _deepLinkSubscription?.Dispose();
            _syncSubscription = null;
            _deepLinkSubscription = null;

            if (_ownsSdk)
            {
                _sdk?.Dispose();
            }

            _sdk = null;
            _started = false;
        }

        private static string DescribeConsentValues(AttriaxSdk sdk)
        {
            var values = sdk.Consent.Gdpr.Values;
            if (values == null)
            {
                return sdk.Consent.Gdpr.State == AttriaxGdprConsentState.NotRequired ? "not required" : "unset";
            }

            return "analytics=" + values.Analytics + " attribution=" + values.Attribution + " adEvents=" + values.AdEvents;
        }

        private static string DescribeDeepLink(AttriaxDeepLinkEvent? deepLinkEvent)
        {
            if (deepLinkEvent == null)
            {
                return "none";
            }

            return (deepLinkEvent.Found ? "matched " : "unmatched ") + deepLinkEvent.Uri.AbsolutePath;
        }

        private static string DescribeReferrer(AttriaxInstallReferrerDetails? referrer)
        {
            if (referrer == null)
            {
                return "none";
            }

            return !string.IsNullOrWhiteSpace(referrer.Campaign)
                ? referrer.Campaign!
                : referrer.RawPlatformInstallReferrer ?? "available";
        }

        private static string DescribeDeepLinkReferrer(AttriaxDeepLinkReferrerDetails? referrer)
        {
            return referrer == null ? "none" : referrer.Uri.AbsolutePath;
        }

        /// <summary>One entry in the scrollable recent-activity feed.</summary>
        public sealed class ActivityEntry
        {
            public ActivityEntry(string title, string? detail, DateTime at, bool isError)
            {
                Title = title;
                Detail = detail;
                At = at;
                IsError = isError;
            }

            public string Title { get; }

            public string? Detail { get; }

            public DateTime At { get; }

            public bool IsError { get; }
        }
    }
}
