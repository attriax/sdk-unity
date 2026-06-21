#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using GeneratedUninstallTokenProvider = Attriax.Unity.Generated.Model.AppUserUninstallTokenProvider;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;

namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime :
        IAttriaxContextRefreshProvider,
        IAttriaxDeepLinkConversionResolver,
        IAttriaxAppOpenPipeline,
        IAttriaxSessionLifecycleQueue
    {
        private static readonly Action<string, string?> NoopDebugLog = static (_, _) => { };

        private const string DefaultApiBaseUrl = "https://api.attriax.com";
        private const string DefaultStorageKeyPrefix = "attriax:unity";
        private const int DefaultRequestTimeoutMs = 12000;
        private const int DefaultMaxQueueSize = 500;
        private const int DefaultSessionHeartbeatIntervalMs = 300000;
        private const int DefaultFirstLaunchSessionHeartbeatIntervalMs = 30000;
        private const int DefaultEventFlushIntervalMs = 60000;
        private const int DefaultTrackingAuthorizationStatusTimeoutMs = 60000;
        private const string InstallReferrerStorageKey = "installReferrer";
        private const string OriginalInstallReferrerDetailsStorageKey = "originalInstallReferrerDetails";
        private const string ReinstallReferrerDetailsStorageKey = "reinstallReferrerDetails";
        private const string GdprConsentStorageKey = "gdprConsent";
        private const string GdprConsentIdStorageKey = "gdprConsentId";
        private const string SkanStateStorageKey = "skanState";
        private const string SessionStorageKey = "session";
        private const string PersistentStorageDeviceIdSource = "persistent_storage";
        private const string SdkApiVersion = "v1";
        private const string SdkPackageVersion = "0.5.0";

        private readonly object _initializationGate = new object();
        private readonly AttriaxAppOpenManager _appOpenManager;
        private readonly AttriaxIosAppOpenEnrichmentManager _iosAppOpenEnrichmentManager;
        private readonly AttriaxPlatformInstallReferrerManager _platformInstallReferrerManager;
        private readonly AttriaxConsentManager _consentManager;
        private AttriaxConsentQueuePolicy? _consentQueuePolicy;
        private AttriaxUninstallTokenRegistrar? _uninstallTokenRegistrar;
        private readonly AttriaxContextSnapshotBuilder _contextSnapshotBuilder;
        private readonly AttriaxContextManager _contextManager;
        private readonly AttriaxDeepLinkManager _deepLinkManager;
        private readonly AttriaxEventHub _eventHub;
        private readonly AttriaxGeneratedGateway _generatedGateway;
        private readonly Func<AttriaxPlatformType, string, AttriaxResolvedUrlOpenMode, Task<bool>> _openBrowserUrlAsync;
        private readonly AttriaxDeepLinkBrowserHandler _deepLinkBrowserHandler;
        private readonly AttriaxRequestManager _requestManager;
        private readonly AttriaxRequestQueue _requestQueue;
        private readonly string[] _runtimeScopedStorageKeys;
        private readonly AttriaxRuntimeState _runtimeState = new AttriaxRuntimeState();
        private readonly AttriaxRuntimeSettingsStore _runtimeSettingsStore;
        private readonly AttriaxRuntimeSettingsState _settingsState;
        private readonly AttriaxReferrerManager _referrerManager;
        private readonly AttriaxSdkRuntimeConfigCoordinator _sdkRuntimeConfigCoordinator;
        private readonly AttriaxSkanManager _skanManager;
        private readonly AttriaxSessionManager _sessionManager;
        private readonly AttriaxTrackingAuthorizationManager _trackingAuthorizationManager;
        private readonly AttriaxTrackingManager _trackingManager;
        private readonly AttriaxAppOpenLaunchCoordinator _appOpenLaunchCoordinator;
        private readonly AttriaxRuntimeActivationCoordinator _activationCoordinator;
        private readonly AttriaxCrashReportingCoordinator _crashReportingCoordinator;
        private readonly AttriaxInstallReferrerStore _installReferrerStore;

        private Task _initializationTask;
        private Task _flushTask;
        private readonly object _flushGate = new object();
        private DateTimeOffset? _deferredFlushDueAt;
        private NormalizedConfig _config;
        private string _storageNamespace;
        private bool _lifecycleAttached;
        private bool _disposed;
        private int _backgroundTaskGeneration;
        private bool _shouldGateRequestsOnSuccessfulAppOpen;
        private Task? _identifiedConsentTransitionTask;
        private AttriaxPlatformType _resolvedPlatform = AttriaxPlatformType.Unknown;
        private readonly AttriaxPlatformType _platform;

        private bool _initialized
        {
            get => _runtimeState.IsInitialized;
            set => _runtimeState.IsInitialized = value;
        }

        private bool _enabled
        {
            get => _runtimeState.IsEnabled;
            set => _runtimeState.IsEnabled = value;
        }

        private bool _eventsEnabled
        {
            get => _runtimeState.AreEventsEnabled;
            set => _runtimeState.AreEventsEnabled = value;
        }

        private bool _isFirstLaunch
        {
            get => _runtimeState.IsFirstLaunch;
            set => _runtimeState.IsFirstLaunch = value;
        }

        private string _deviceId
        {
            get => _runtimeState.DeviceId;
            set => _runtimeState.DeviceId = value;
        }

        private string _deviceIdSource
        {
            get => _runtimeState.DeviceIdSource;
            set => _runtimeState.DeviceIdSource = value;
        }

        public AttriaxRuntime(AttriaxConfig config)
            : this(config, null)
        {
        }

        internal AttriaxRuntime(
            AttriaxConfig config,
            Func<AttriaxPlatformType, string, AttriaxResolvedUrlOpenMode, Task<bool>>? openBrowserUrlAsync)
        {
            AttriaxLifecycleDispatcher.BindToCurrentThread();
            _config = NormalizeConfig(config);
            _platform = MapPlatform(Application.platform);
            _openBrowserUrlAsync = openBrowserUrlAsync ?? AttriaxNativeBridge.OpenBrowserUrlAsync;
            _deepLinkBrowserHandler = new AttriaxDeepLinkBrowserHandler(
                _config.AutomaticBrowserHandling,
                GetCurrentPlatform,
                _openBrowserUrlAsync);
            _contextSnapshotBuilder = new AttriaxContextSnapshotBuilder(
                _config.ToPublic(),
                SdkApiVersion,
                SdkPackageVersion,
                ResolveCurrentTimezone);
            _platformInstallReferrerManager = new AttriaxPlatformInstallReferrerManager(
                ReadPersistedInstallReferrer);
            _contextManager = new AttriaxContextManager(
                this);
            _eventHub = new AttriaxEventHub();
            _deepLinkManager = new AttriaxDeepLinkManager(
                _runtimeState,
                this,
                _eventHub);
            _generatedGateway = new AttriaxGeneratedGateway(_config.ApiBaseUrl, _config.RequestTimeoutMs);
            _storageNamespace = BuildStorageNamespace(_config.ProjectToken);
            _installReferrerStore = new AttriaxInstallReferrerStore(Key);
            _runtimeSettingsStore = new AttriaxRuntimeSettingsStore(
                Key("deviceId"),
                Key("deviceIdSource"),
                Key("enabled"),
                Key("eventsEnabled"),
                Key("hasLaunched"));
            _runtimeScopedStorageKeys = new[]
            {
                Key("deviceId"),
                Key("deviceIdSource"),
                Key("enabled"),
                Key("eventsEnabled"),
                Key(InstallReferrerStorageKey),
                Key(OriginalInstallReferrerDetailsStorageKey),
                Key(ReinstallReferrerDetailsStorageKey),
                Key(SkanStateStorageKey),
                Key(SessionStorageKey),
                Key("queue"),
            };
            AttriaxPlayerPrefs.SetRuntimePersistenceMode(
                _runtimeScopedStorageKeys,
                ResolveInitialRuntimePersistenceMode());
            _requestManager = new AttriaxRequestManager();
            _requestQueue = new AttriaxRequestQueue(Key("queue"), _config.MaxQueueSize);
            _requestManager.BindQueue(_requestQueue);
            _settingsState = new AttriaxRuntimeSettingsState(_runtimeSettingsStore, _runtimeState);
            _settingsState.RestoreFromStore();
            _consentManager = new AttriaxConsentManager(
                new AttriaxPlayerPrefsConsentStore(
                    Key(GdprConsentStorageKey),
                    NoopDebugLog),
                _config.ProjectToken,
                _config.GdprEnabled,
                _config.AnonymousTracking,
                EnsureConsentDeviceIdentity,
                ResolveCurrentTimezone,
                _generatedGateway,
                HandleConsentStateChanged,
                NoopDebugLog);
            _skanManager = new AttriaxSkanManager(
                _config.Skan ?? new AttriaxSkanConfig(),
                _platform,
                () => DateTimeOffset.UtcNow,
                () => AttriaxPlayerPrefs.HasKey(Key(SkanStateStorageKey))
                    ? AttriaxPlayerPrefs.GetString(Key(SkanStateStorageKey), string.Empty)
                    : null,
                value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        AttriaxPlayerPrefs.DeleteKey(Key(SkanStateStorageKey));
                    }
                    else
                    {
                        AttriaxPlayerPrefs.SetString(Key(SkanStateStorageKey), value);
                    }

                    AttriaxPlayerPrefs.Save();
                },
                NoopDebugLog,
                (amountMicros, currency, occurredAt) => _generatedGateway.ConvertRevenueToUsdMicrosAsync(
                    _config.ProjectToken,
                    amountMicros,
                    currency,
                    occurredAt));
            _trackingAuthorizationManager = new AttriaxTrackingAuthorizationManager(
                _config.ToPublic(),
                _platform,
                () => AttriaxNativeBridge.GetTrackingAuthorizationStatusAsync(_platform),
                () => AttriaxNativeBridge.RequestTrackingAuthorizationAsync(_platform));
            _sessionManager = new AttriaxSessionManager(
                _config.SessionTrackingEnabled,
                () => ShouldTrackSessionActivity,
                _runtimeState,
                _contextManager,
                _config.FirstLaunchSessionHeartbeatIntervalMs,
                _config.SessionHeartbeatIntervalMs,
                new AttriaxPlayerPrefsSessionStore(
                    Key(SessionStorageKey),
                    NoopDebugLog),
                this,
                NoopDebugLog);
            _appOpenManager = new AttriaxAppOpenManager(
                _runtimeState,
                this,
                _eventHub);
            _referrerManager = new AttriaxReferrerManager(
                _runtimeState,
                () => _deepLinkManager.InitialDeepLinkValue,
                () => _deepLinkManager.LatestDeepLink,
                () => _deepLinkManager.InitialDeepLink,
                _appOpenManager.WaitForPublicResultAsync,
                _eventHub.SubscribeToDeepLinks,
                () => _sessionManager.CurrentSession);
            _iosAppOpenEnrichmentManager = new AttriaxIosAppOpenEnrichmentManager(_platform);
            _sdkRuntimeConfigCoordinator = new AttriaxSdkRuntimeConfigCoordinator(
                LoadSdkRuntimeConfigAsync,
                HandleSdkRuntimeConfigLoadedAsync);
            _appOpenLaunchCoordinator = new AttriaxAppOpenLaunchCoordinator(
                () => _appOpenManager.DidSchedule,
                () => _consentManager.AllowsAttributionTracking,
                EnsureSdkRuntimeConfigLoadedAsync,
                allowsAttributionTracking => _iosAppOpenEnrichmentManager
                    .BuildDeviceMetadataOverridesForAppOpenAsync(allowsAttributionTracking),
                (clipboardAttributionEnabled, allowsAttributionTracking) => _iosAppOpenEnrichmentManager
                    .InstallReferrerOverrideForAppOpen(
                        clipboardAttributionEnabled,
                        allowsAttributionTracking),
                SchedulePreparedAppOpenAsync);
            _trackingManager = new AttriaxTrackingManager(
                _config.ProjectToken,
                _config.FlushEventsImmediatelyOnFirstLaunch,
                _config.SessionTrackingEnabled,
                _consentManager,
                _runtimeState,
                ShouldDispatchAnalyticsInCurrentMode,
                _sessionManager,
                _requestManager,
                _skanManager,
                RequestQueueFlush,
                NoopDebugLog);
            _crashReportingCoordinator = new AttriaxCrashReportingCoordinator(
                _config.ToPublic(),
                () => !_disposed && _initialized && _enabled,
                _trackingManager,
                ObserveBackgroundTask);
            _activationCoordinator = new AttriaxRuntimeActivationCoordinator(
                PersistEnabledState,
                RefreshAppOpenDispatchGate,
                ClearDeferredFlush,
                _sessionManager.HandleSdkDisabled,
                () => _sessionManager.HandleSdkEnabled(DateTimeOffset.UtcNow),
                EnsureReferrerTasksForEnabledState,
                ScheduleLaunchPreparationIfNeeded,
                SyncRuntimePersistenceMode,
                RewriteAndPurgeQueuedRequestsForConsent,
                ObserveDeniedAttributionStateResolution,
                () => _requestQueue.Count,
                RequestImmediateQueueFlush,
                SetSynchronizationState,
                () => _initialized);
            _shouldGateRequestsOnSuccessfulAppOpen = ShouldGateRequestsOnSuccessfulAppOpen;
        }

        public AttriaxConfig Config => _config.ToPublic();

        public bool IsInitialized => _initialized;

        public bool Enabled => _enabled;

        public bool EventsEnabled
        {
            get => _eventsEnabled;
            set => SetEventsEnabled(value);
        }

        public bool IsFirstLaunch => _isFirstLaunch;

        public string? DeviceId =>
            _initialized && !string.IsNullOrWhiteSpace(_deviceId)
                ? _deviceId
                : null;

        public bool AnonymousTrackingEnabled
        {
            get => _consentManager.AnonymousTrackingEnabled;
            set => SetAnonymousTrackingEnabled(value);
        }

        public AttriaxSdkSnapshot? SdkSnapshot => _initialized ? _contextManager.Snapshot.Sdk : null;

        public AttriaxAppOpen? LastAppOpenResult => _appOpenManager.LastPublicResult;

        public AttriaxGdprConsentState GdprConsentState => _consentManager.State;

        public AttriaxGdprConsentValues? GdprConsentValues => _consentManager.Values;

        public bool IsWaitingForGdprConsent => _consentManager.IsWaitingForConsent;

        public AttriaxSkanState? SkanState => _skanManager.State;

        public Task<AttriaxInstallReferrerDetails?> OriginalInstallReferrer =>
            _referrerManager.OriginalInstallReferrer;

        public Task<AttriaxInstallReferrerDetails?> ReinstallReferrer =>
            _referrerManager.ReinstallReferrer;

        public Task<AttriaxInstallReferrerDetails?> InstallReferrer =>
            GetLegacyInstallReferrerAsync();

        public AttriaxRawDeepLinkEvent? RawInitialDeepLink =>
            _deepLinkManager.RawInitialDeepLinkValue;

        public Task<AttriaxDeepLinkEvent?> InitialDeepLink =>
            _deepLinkManager.InitialDeepLink;

        public Task<AttriaxDeepLinkEvent> WaitForDeepLinkResolutionAsync(
            AttriaxRawDeepLinkEvent rawEvent)
        {
            return _deepLinkManager.WaitForResolutionAsync(rawEvent);
        }

        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync()
        {
            return await _referrerManager.GetSessionReferrerAsync().ConfigureAwait(false);
        }

        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync()
        {
            return _referrerManager.GetLatestDeepLinkReferrerAsync();
        }

        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(
            int? timeoutMs = null)
        {
            AssertNotDisposed();
            return _trackingAuthorizationManager.RequestTrackingAuthorizationAsync(timeoutMs);
        }

        public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync()
        {
            AssertNotDisposed();
            return _trackingAuthorizationManager.GetTrackingAuthorizationStatusAsync();
        }

        public Task<bool> NeedsGdprConsentAsync(bool localOnly = false)
        {
            AssertNotDisposed();
            return _consentManager.NeedsConsentAsync(localOnly);
        }

        public void SetGdprConsent(bool analytics, bool attribution, bool adEvents)
        {
            AssertNotDisposed();
            _consentManager.SetConsent(analytics, attribution, adEvents);
        }

        public void SetGdprConsentNotRequired()
        {
            AssertNotDisposed();
            _consentManager.SetNotRequired();
        }

        public void ResetGdprConsent()
        {
            AssertNotDisposed();
            _consentManager.Reset();
        }

        public async Task RequestGdprDataErasureAsync()
        {
            AssertInitialized();

            if (string.IsNullOrWhiteSpace(_deviceId))
            {
                throw new InvalidOperationException(
                    "Attriax SDK device identity is unavailable. Call InitializeAsync() first.");
            }

            await _generatedGateway.EraseGdprDataAsync(
                    new global::Attriax.Unity.Generated.Model.SdkV1GdprDataEraseDto(
                        _config.ProjectToken,
                        _deviceId))
                .ConfigureAwait(false);

            await ResetAsync().ConfigureAwait(false);
        }

        internal AttriaxRawDeepLinkEvent? RawInitialDeepLinkValue => RawInitialDeepLink;
        internal AttriaxDeepLinkEvent? InitialDeepLinkValue => _deepLinkManager.InitialDeepLinkValue;

        internal bool InitialDeepLinkResolved => _deepLinkManager.InitialDeepLinkResolved;

        internal Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink => InitialDeepLink;

        internal AttriaxDeepLinkEvent? LatestDeepLink => _deepLinkManager.LatestDeepLink;

        public AttriaxSynchronizationState SynchronizationState => _eventHub.SynchronizationState;

        public Task InitializeAsync(AttriaxInitOptions options)
        {
            AssertNotDisposed();

            if (_initialized)
            {
                return Task.CompletedTask;
            }

            lock (_initializationGate)
            {
                if (_initialized)
                {
                    return Task.CompletedTask;
                }

                if (_initializationTask == null)
                {
                    _initializationTask = RunInitializeAsync(options);
                }

                return AwaitInitializationAsync(_initializationTask);
            }
        }

        public Task TrackEventAsync(string eventName, AttriaxTrackEventOptions options)
        {
            AssertInitialized();
            return _trackingManager.TrackEventAsync(eventName, options);
        }

        public Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions options)
        {
            AssertInitialized();
            return _trackingManager.RecordErrorAsync(error, options);
        }

        public Task RecordNotificationAsync(
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxRecordNotificationOptions options)
        {
            AssertInitialized();
            return _trackingManager.RecordNotificationAsync(type, notificationId, options);
        }

        public Task TrackPageViewAsync(string pageName, AttriaxPageViewOptions options)
        {
            AssertInitialized();
            return _trackingManager.TrackPageViewAsync(pageName, options);
        }

        public Task RecordEventAsync(string eventName, AttriaxTrackEventOptions options)
        {
            AssertInitialized();
            return _trackingManager.TrackEventAsync(eventName, options);
        }

        public Task RecordPageViewAsync(string pageName, AttriaxPageViewOptions options)
        {
            AssertInitialized();
            return _trackingManager.TrackPageViewAsync(pageName, options);
        }

        public Task RecordPurchaseAsync(double revenue, AttriaxRecordPurchaseOptions options)
        {
            AssertInitialized();

            if (double.IsInfinity(revenue) || double.IsNaN(revenue))
            {
                throw new ArgumentException("revenue must be finite.", nameof(revenue));
            }

            if (options.Quantity <= 0)
            {
                throw new ArgumentException("quantity must be positive.", nameof(options.Quantity));
            }

            var eventData = BuildPurchaseRevenueEventData(
                AttriaxAnalyticsEventKeys.Purchase,
                revenue,
                options.Currency,
                options.RevenueInMicros,
                options.PurchaseType,
                options.ProductId,
                options.TransactionId,
                options.OriginalTransactionId,
                options.ValidationProvider,
                options.ValidationEnvironment,
                options.PurchaseToken,
                options.ReceiptData,
                options.SignedPayload,
                options.ReceiptSignature,
                options.IsRenewal,
                options.Quantity,
                options.Store,
                options.PackageName,
                options.Voided,
                options.Test,
                options.ValidationId,
                options.Metadata,
                isRefund: false);

            return TrackEventAsync(AttriaxAnalyticsEventKeys.Purchase, new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately,
            });
        }

        public Task RecordRefundAsync(double revenue, AttriaxRecordRefundOptions options)
        {
            AssertInitialized();

            if (double.IsInfinity(revenue) || double.IsNaN(revenue))
            {
                throw new ArgumentException("revenue must be finite.", nameof(revenue));
            }

            if (options.Quantity <= 0)
            {
                throw new ArgumentException("quantity must be positive.", nameof(options.Quantity));
            }

            var eventData = BuildRefundRevenueEventData(
                revenue,
                options.Currency,
                options.RevenueInMicros,
                options.PurchaseType,
                options.ProductId,
                options.TransactionId,
                options.OriginalTransactionId,
                options.Quantity,
                options.Store,
                options.PackageName,
                options.Voided,
                options.Test,
                options.Reason,
                options.Metadata);

            return TrackEventAsync(AttriaxAnalyticsEventKeys.Refund, new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately,
            });
        }

        public Task RecordAdRevenueAsync(double revenue, AttriaxRecordAdRevenueOptions options)
        {
            AssertInitialized();

            if (double.IsInfinity(revenue) || double.IsNaN(revenue))
            {
                throw new ArgumentException("revenue must be finite.", nameof(revenue));
            }

            var eventData = BuildAdRevenueEventData(
                revenue,
                options.Currency,
                options.RevenueInMicros,
                options.AdNetwork,
                options.AdFormat,
                options.AdType,
                options.AdPlacement,
                options.Test,
                options.Metadata);

            return TrackEventAsync(AttriaxAnalyticsEventKeys.AdRevenue, new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately,
            });
        }

        public Task RecordAdEventAsync(AttriaxAdEventType type, AttriaxRecordAdEventOptions options)
        {
            AssertInitialized();

            if (options.LoadLatencyMs.HasValue &&
                (double.IsInfinity(options.LoadLatencyMs.Value) || double.IsNaN(options.LoadLatencyMs.Value)))
            {
                throw new ArgumentException("loadLatencyMs must be finite.", nameof(options));
            }

            if (options.RewardAmount.HasValue &&
                (double.IsInfinity(options.RewardAmount.Value) || double.IsNaN(options.RewardAmount.Value)))
            {
                throw new ArgumentException("rewardAmount must be finite.", nameof(options));
            }

            var eventName = AdEventTypeToEventName(type);
            var eventData = BuildAdEventData(
                options.AdNetwork,
                options.MediationNetwork,
                options.AdUnitId,
                options.AdPlacement,
                options.AdFormat,
                options.AdType,
                options.FailureReason,
                options.LoadLatencyMs,
                options.RewardType,
                options.RewardAmount,
                options.Test,
                options.Metadata);

            return TrackEventAsync(eventName, new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately,
            });
        }

        private static string AdEventTypeToEventName(AttriaxAdEventType type)
        {
            switch (type)
            {
                case AttriaxAdEventType.Request: return "ad_request";
                case AttriaxAdEventType.Load: return "ad_load";
                case AttriaxAdEventType.LoadFailed: return "ad_load_failed";
                case AttriaxAdEventType.Show: return "ad_show";
                case AttriaxAdEventType.ShowFailed: return "ad_show_failed";
                case AttriaxAdEventType.Impression: return "ad_impression";
                case AttriaxAdEventType.Click: return "ad_click";
                case AttriaxAdEventType.Dismiss: return "ad_dismiss";
                case AttriaxAdEventType.Reward: return "ad_reward";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown AttriaxAdEventType.");
            }
        }

        public Task<AttriaxSkanUpdateResult> UpdateSkanConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
            AssertInitialized();
            return _skanManager.UpdateConversionValueAsync(fineValue, coarseValue, lockWindow);
        }

        public Task IdentifyAsync(string? userId, AttriaxIdentifyOptions options)
        {
            AssertInitialized();
            return _trackingManager.IdentifyAsync(userId, options);
        }

        public Task SetUserAsync(string? userId, AttriaxSetUserOptions options)
        {
            AssertInitialized();
            return _trackingManager.SetUserAsync(userId, options);
        }

        public Task SetUserPropertyAsync(string name, object? value)
        {
            AssertInitialized();
            return _trackingManager.SetUserPropertyAsync(name, value);
        }

        public Task SetUserPropertiesAsync(IDictionary<string, object?> properties)
        {
            AssertInitialized();
            return _trackingManager.SetUserPropertiesAsync(properties);
        }

        public Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null)
        {
            AssertInitialized();
            return _trackingManager.ClearUserPropertiesAsync(propertyNames);
        }

        public async Task<AttriaxCreateDynamicLinkResult> CreateDynamicLinkAsync(AttriaxCreateDynamicLinkOptions options)
        {
            AssertInitialized();

                return await _generatedGateway.SendCreateDynamicLinkAsync(
                    AttriaxGeneratedRequestFactory.BuildCreateDynamicLinkRequest(_config.ProjectToken, options))
                .ConfigureAwait(false);
        }

        public async Task<AttriaxRevenueReceiptValidationResult> ValidateReceiptAsync(
            AttriaxValidateReceiptOptions options)
        {
            AssertInitialized();

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.Receipt))
            {
                throw new ArgumentException(
                    "ValidateReceiptAsync() requires a non-empty receipt.",
                    nameof(options));
            }

            return await _generatedGateway.SendValidateReceiptAsync(
                    AttriaxGeneratedRequestFactory.BuildValidateReceiptRequest(
                        _config.ProjectToken,
                        string.IsNullOrWhiteSpace(_deviceId) ? null : _deviceId,
                        options,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
        }

        public Task<AttriaxDeepLinkEvent?> RecordDeepLinkConversionAsync(AttriaxDeepLinkConversionOptions options)
        {
            AssertInitialized();

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                throw new ArgumentException(
                    "RecordDeepLinkConversionAsync() requires a non-empty URI.",
                    nameof(options));
            }

            return _deepLinkManager.RecordConversionAsync(options);
        }

        public async Task RegisterFirebaseMessagingTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            await RegisterUninstallTokenAsync(
                    GeneratedUninstallTokenProvider.Fcm,
                    token,
                    metadata)
                .ConfigureAwait(false);
        }

        public async Task RegisterApplePushTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            await RegisterUninstallTokenAsync(
                    GeneratedUninstallTokenProvider.Apns,
                    token,
                    metadata)
                .ConfigureAwait(false);
        }

        // Builds the uninstall-token registrar on first use. The captured delegates
        // reference instance members, so it must only be accessed after init.
        private AttriaxUninstallTokenRegistrar UninstallTokenRegistrar =>
            _uninstallTokenRegistrar ??= new AttriaxUninstallTokenRegistrar(
                _config.ProjectToken,
                _consentManager,
                () => _deviceId,
                RequireDeviceIdSource,
                GetCurrentPlatform,
                request =>
                {
                    _ = _requestQueue.Enqueue(AttriaxQueuedRequest.CreateUninstallToken(request));
                    RequestQueueFlush(true);
                });

        private Task RegisterUninstallTokenAsync(
            GeneratedUninstallTokenProvider provider,
            string? token,
            IDictionary<string, object>? metadata)
        {
            AssertInitialized();

            if (!_enabled)
            {
                return Task.CompletedTask;
            }

            UninstallTokenRegistrar.Register(provider, token, metadata);
            return Task.CompletedTask;
        }

        private async Task<AttriaxDeepLinkEvent> ResolveDeepLinkConversionAsync(
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent? rawEvent,
            DateTimeOffset clickedAt)
        {
            if (ShouldTrackSessionActivity)
            {
                _sessionManager.PrepareTrackedActivity(DateTimeOffset.UtcNow);
            }

            var decision = TrackingDecisionFor(AttriaxTrackingSignal.DeepLink);
            var request = AttriaxGeneratedRequestFactory.BuildResolveDeepLinkRequest(
                _config.ProjectToken,
                decision.AttachDeviceIdentity ? _deviceId : null,
                decision.AttachDeviceIdentity ? RequireDeviceIdSource() : null,
                _isFirstLaunch,
                GetCurrentPlatform(),
                options);

            AttriaxDeepLinkResolutionResultInternal queuedResult;
            if (decision.SendNetworkDirectly && !decision.AttachDeviceIdentity)
            {
                queuedResult = await _generatedGateway.SendDeepLinkResolutionAsync(request)
                    .ConfigureAwait(false);
            }
            else
            {
                var queued = _requestQueue.Enqueue(AttriaxQueuedRequest.CreateDeepLinkResolve(request));
                try
                {
                    queuedResult = (AttriaxDeepLinkResolutionResultInternal)await queued.ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    throw error.InnerException ?? new Exception("Deep-link resolution failed.", error);
                }
            }

            return await MapResolutionToDeepLinkEventAsync(
                queuedResult,
                options,
                rawEvent,
                clickedAt,
                false).ConfigureAwait(false);
        }

        public Task<AttriaxAppOpen?> WaitForAppOpenTrackingAsync()
        {
            return _appOpenManager.WaitForPublicResultAsync();
        }

        public IDisposable SubscribeToRawDeepLinks(Action<AttriaxRawDeepLinkEvent> listener)
        {
            AssertNotDisposed();
            return _eventHub.SubscribeToRawDeepLinks(listener);
        }

        public IDisposable SubscribeToDeepLinks(Action<AttriaxDeepLinkEvent> listener)
        {
            AssertNotDisposed();
            return _eventHub.SubscribeToDeepLinks(listener);
        }

        public IDisposable SubscribeToSynchronization(Action<AttriaxSynchronizationState> listener)
        {
            AssertNotDisposed();
            return _eventHub.SubscribeToSynchronization(listener);
        }

        public Task FlushAsync()
        {
            _deferredFlushDueAt = null;

            lock (_flushGate)
            {
                if (_flushTask != null)
                {
                    return _flushTask;
                }

                _flushTask = FlushInternalAsync();
                return AwaitFlushAsync(_flushTask);
            }
        }

        public async Task ResetAsync()
        {
            AssertNotDisposed();

            AttriaxLifecycleDispatcher.InvokeOnMainThread(
                () => UnityEngine.Debug.LogWarning(
                    "[Attriax][WARNING] Resetting Attriax SDK state. Call InitializeAsync() again before reusing this instance."));

            var initializationTask = _initializationTask;
            if (initializationTask != null)
            {
                try
                {
                    await initializationTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }

            _backgroundTaskGeneration += 1;
            _deferredFlushDueAt = null;

            var flushTask = _flushTask;
            if (flushTask != null)
            {
                try
                {
                    await flushTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }

            var resetError = new AttriaxApiError(
                "Attriax SDK state was reset before queued work completed.",
                null,
                true,
                true);

            _requestManager.Clear(resetError);
            DetachLifecycle();
            _sessionManager.Reset();
            _appOpenManager.Reset();
            _appOpenLaunchCoordinator.Reset();
            _sdkRuntimeConfigCoordinator.Reset();
            _contextManager.Reset();
            _referrerManager.Reset();
            _deepLinkManager.Reset();
            _eventHub.Reset();
            await _skanManager.ResetAsync().ConfigureAwait(false);
            ClearPersistedState();
            _consentManager.ClearMemory();

            _installReferrerStore.ResetCache();
            _runtimeState.Reset();
            _initializationTask = null;
            _flushTask = null;
            _identifiedConsentTransitionTask = null;
            _resolvedPlatform = AttriaxPlatformType.Unknown;
        }

        public void SetEnabled(bool enabled)
        {
            _activationCoordinator.SetEnabled(enabled, RuntimeActivationState);
        }

        public void SetEventsEnabled(bool enabled)
        {
            _settingsState.SetEventsEnabled(enabled);
        }

        public void SetAnonymousTrackingEnabled(bool enabled)
        {
            _consentManager.SetAnonymousTrackingEnabled(enabled);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _backgroundTaskGeneration += 1;
            _deferredFlushDueAt = null;
            _identifiedConsentTransitionTask = null;
            _resolvedPlatform = AttriaxPlatformType.Unknown;
            _referrerManager.CompleteAllWithNull();
            CompleteInitialDeepLink(null);
            DetachLifecycle();
            _appOpenManager.Reset();
            _appOpenLaunchCoordinator.Reset();
            _sdkRuntimeConfigCoordinator.Reset();
            _generatedGateway.Dispose();
            AttriaxPlayerPrefs.ForgetRuntimeKeys(_runtimeScopedStorageKeys);
            _requestManager.RejectAll(new AttriaxApiError(
                "Attriax instance was disposed before queued work completed.",
                null,
                true,
                true));
            _eventHub.Clear();
        }

        private async Task RunInitializeAsync(AttriaxInitOptions options)
        {
            try
            {
                var bootstrapCoordinator = new AttriaxRuntimeBootstrapCoordinator(this);
                await bootstrapCoordinator.RunAsync(options).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] SDK initialization failed. The SDK will not be available for this session. "
                    + error);
                throw;
            }
        }

        private async Task AwaitInitializationAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            finally
            {
                lock (_initializationGate)
                {
                    if (ReferenceEquals(_initializationTask, task))
                    {
                        _initializationTask = null;
                    }
                }
            }
        }

        private async Task AwaitFlushAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            finally
            {
                if (ReferenceEquals(_flushTask, task))
                {
                    _flushTask = null;
                }
            }
        }

        Task<AttriaxPreparedContextRefresh> IAttriaxContextRefreshProvider.PrepareContextRefreshAsync(
            bool resolveInstallReferrer)
        {
            return PrepareContextRefreshAsync(resolveInstallReferrer);
        }

        Task<AttriaxDeepLinkEvent> IAttriaxDeepLinkConversionResolver.ResolveDeepLinkConversionAsync(
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent? rawEvent,
            DateTimeOffset clickedAt)
        {
            return ResolveDeepLinkConversionAsync(options, rawEvent, clickedAt);
        }

        Task<AttriaxAppOpenResult> IAttriaxAppOpenPipeline.EnqueueOpenAsync(
            string? installReferrerOverride,
            IDictionary<string, object>? deviceMetadataOverrides)
        {
            return EnqueueOpenAsync(installReferrerOverride, deviceMetadataOverrides);
        }

        Task IAttriaxAppOpenPipeline.ResolveInstallReferrerFromAppOpenAsync(
            Task<AttriaxAppOpenResult> openTrackingTask)
        {
            return ResolveInstallReferrerFromAppOpenAsync(openTrackingTask);
        }

        AttriaxDeepLinkEvent? IAttriaxAppOpenPipeline.BuildDeepLinkEventFromAppOpenResult(
            AttriaxAppOpenResult result)
        {
            return BuildDeepLinkEventFromAppOpenResult(result);
        }

        AttriaxAppOpen? IAttriaxAppOpenPipeline.ToPublicAppOpen(AttriaxAppOpenResult? result)
        {
            return ToPublicAppOpen(result);
        }

        void IAttriaxSessionLifecycleQueue.QueueSessionLifecycle(
            SdkSessionLifecycleKind kind,
            AttriaxSessionSnapshot session,
            DateTimeOffset occurredAt,
            IDictionary<string, object>? metadata)
        {
            QueueSessionLifecycle(kind, session, occurredAt, metadata);
        }

        private void ScheduleAppOpenIfNeeded()
        {
            ScheduleLaunchPreparationIfNeeded();
        }

        private void PrimeSdkRuntimeConfigForLaunch()
        {
            _sdkRuntimeConfigCoordinator.PrimeForLaunch(_initialized, _enabled);
        }

        private Task<AttriaxSdkRuntimeConfig> EnsureSdkRuntimeConfigLoadedAsync()
        {
            return _sdkRuntimeConfigCoordinator.EnsureLoadedAsync();
        }

        private void ScheduleLaunchAppOpenIfNeeded()
        {
            ObserveBackgroundTask(
                _appOpenLaunchCoordinator.ScheduleIfNeeded(_initialized, _enabled),
                "Attriax launch preparation failed.");
        }

        private Task SchedulePreparedAppOpenAsync(
            string? installReferrerOverride,
            IDictionary<string, object> deviceMetadataOverrides)
        {
            if (_disposed)
            {
                return Task.CompletedTask;
            }

            return _appOpenManager.ScheduleAsync(
                installReferrerOverride,
                deviceMetadataOverrides);
        }

        private Task HandleSdkRuntimeConfigLoadedAsync(AttriaxSdkRuntimeConfig runtimeConfig)
        {
            return _iosAppOpenEnrichmentManager.PrimeForConsentStateAsync(
                runtimeConfig.ClipboardAttributionEnabled,
                _consentManager.IsWaitingForConsent,
                _consentManager.AllowsAttributionTracking);
        }

        private Task<AttriaxSdkRuntimeConfig> LoadSdkRuntimeConfigAsync()
        {
            if (!_initialized)
            {
                return Task.FromResult(new AttriaxSdkRuntimeConfig());
            }

            return _generatedGateway.FetchSdkRuntimeConfigAsync(
                AttriaxSdkRuntimeConfigRequestBuilder.Build(
                    _config.ProjectToken,
                    _contextManager.Snapshot));
        }

        private async Task<AttriaxInstallReferrerDetails?> GetLegacyInstallReferrerAsync()
        {
            return await _referrerManager.GetLegacyInstallReferrerAsync().ConfigureAwait(false);
        }

        private void EnsureReferrerTasksForEnabledState()
        {
            _referrerManager.PrepareForEnabledState(
                ReadPersistedInstallReferrerDetails(OriginalInstallReferrerDetailsStorageKey),
                ReadPersistedInstallReferrerDetails(ReinstallReferrerDetailsStorageKey));
        }

        private Task<AttriaxAppOpenResult> EnqueueOpenAsync(
            string? installReferrerOverride = null,
            IDictionary<string, object>? deviceMetadataOverrides = null)
        {
            return EnqueueOpenWithResolvedContextAsync(
                installReferrerOverride,
                deviceMetadataOverrides);
        }

        private async Task<AttriaxAppOpenResult> EnqueueOpenWithResolvedContextAsync(
            string? installReferrerOverride,
            IDictionary<string, object>? deviceMetadataOverrides)
        {
            var snapshot = await _contextManager.EnsureResolvedForAppOpenAsync()
                .ConfigureAwait(false);
            var openRequest = AttriaxGeneratedRequestFactory.BuildOpenRequest(
                _config.ProjectToken,
                RequireDeviceIdSource(),
                snapshot,
                _sessionManager.CurrentSession,
                installReferrerOverride,
                deviceMetadataOverrides);
            var queuedRequest = AttriaxQueuedRequest.CreateOpen(openRequest);
            var queued = AttriaxLifecycleDispatcher.InvokeOnMainThread(
                () => _requestQueue.Enqueue(queuedRequest));
            RequestQueueFlush(true);
            return await queued.ContinueWith(
                task =>
                {
                    if (task.IsFaulted)
                    {
                        throw task.Exception?.InnerException ?? new Exception("App-open tracking failed.");
                    }

                    return (AttriaxAppOpenResult)task.Result;
                },
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);
        }

        private async Task CaptureInitialUrlAsync()
        {
            var generation = _backgroundTaskGeneration;
            await _deepLinkManager.CaptureInitialUrlAsync(AttriaxLifecycleDispatcher.InitialAbsoluteUrl)
                .ConfigureAwait(false);

            if (generation != _backgroundTaskGeneration)
            {
                return;
            }
        }

        private async Task ResolveInstallReferrerFromAppOpenAsync(Task<AttriaxAppOpenResult> openTrackingTask)
        {
            var generation = _backgroundTaskGeneration;
            try
            {
                var appOpenResult = await openTrackingTask.ConfigureAwait(false);
                if (generation != _backgroundTaskGeneration)
                {
                    return;
                }

                var originalInstallReferrerDetails = appOpenResult?.OriginalInstallReferrer;
                var reinstallReferrerDetails = appOpenResult?.ReinstallReferrer;
                var currentInstallReferrerDetails = reinstallReferrerDetails
                    ?? originalInstallReferrerDetails
                    ?? appOpenResult?.InstallReferrer;

                if (originalInstallReferrerDetails != null)
                {
                    PersistInstallReferrerDetails(
                        OriginalInstallReferrerDetailsStorageKey,
                        originalInstallReferrerDetails);
                }

                if (reinstallReferrerDetails != null)
                {
                    PersistInstallReferrerDetails(
                        ReinstallReferrerDetailsStorageKey,
                        reinstallReferrerDetails);
                }

                if (!string.IsNullOrWhiteSpace(currentInstallReferrerDetails?.RawPlatformInstallReferrer))
                {
                    PersistInstallReferrer(currentInstallReferrerDetails.RawPlatformInstallReferrer!);
                }

                _referrerManager.CompleteOriginal(originalInstallReferrerDetails);
                _referrerManager.CompleteReinstall(reinstallReferrerDetails);
            }
            catch (Exception)
            {
                if (generation != _backgroundTaskGeneration)
                {
                    return;
                }

                _deepLinkManager.MarkAppOpenUnavailable();
                _referrerManager.CompleteOriginal(null);
                _referrerManager.CompleteReinstall(null);
            }
        }

        private async Task ResolveLocalInstallReferrerForDeniedConsentAsync()
        {
            var generation = _backgroundTaskGeneration;
            var platform = GetCurrentPlatform();
            if (platform != AttriaxPlatformType.Android)
            {
                _referrerManager.CompleteOriginal(null);
                _referrerManager.CompleteReinstall(null);
                _deepLinkManager.MarkAppOpenUnavailable();
                return;
            }

            var context = await CollectInstallReferrerContextAsync(platform).ConfigureAwait(false);
            if (generation != _backgroundTaskGeneration)
            {
                return;
            }

            var details = _platformInstallReferrerManager.BuildLocalInstallReferrerDetails(context);
            if (!string.IsNullOrWhiteSpace(details?.RawPlatformInstallReferrer))
            {
                PersistInstallReferrer(details.RawPlatformInstallReferrer!);
                PersistInstallReferrerDetails(OriginalInstallReferrerDetailsStorageKey, details);
                PersistInstallReferrerDetails(ReinstallReferrerDetailsStorageKey, details);
            }
            _referrerManager.CompleteOriginal(details);
            _referrerManager.CompleteReinstall(details);
            _deepLinkManager.MarkAppOpenUnavailable();
        }

        private void AttachLifecycle()
        {
            if (_lifecycleAttached)
            {
                return;
            }

            AttriaxLifecycleDispatcher.EnsureCreated();
            AttriaxLifecycleDispatcher.DeepLinkActivated += HandleDeepLinkActivated;
            AttriaxLifecycleDispatcher.Tick += HandleTick;
            AttriaxLifecycleDispatcher.ApplicationPaused += HandleApplicationPaused;
            AttriaxLifecycleDispatcher.ApplicationFocusChanged += HandleApplicationFocusChanged;
            AttriaxLifecycleDispatcher.SceneChanged += HandleSceneChanged;
            AttriaxLifecycleDispatcher.Quitting += HandleApplicationQuitting;
            _crashReportingCoordinator.Activate();
            _lifecycleAttached = true;
        }

        private void DetachLifecycle()
        {
            if (!_lifecycleAttached)
            {
                return;
            }

            AttriaxLifecycleDispatcher.DeepLinkActivated -= HandleDeepLinkActivated;
            AttriaxLifecycleDispatcher.Tick -= HandleTick;
            AttriaxLifecycleDispatcher.ApplicationPaused -= HandleApplicationPaused;
            AttriaxLifecycleDispatcher.ApplicationFocusChanged -= HandleApplicationFocusChanged;
            AttriaxLifecycleDispatcher.SceneChanged -= HandleSceneChanged;
            AttriaxLifecycleDispatcher.Quitting -= HandleApplicationQuitting;
            _crashReportingCoordinator.Deactivate();
            _lifecycleAttached = false;
        }

        private void HandleDeepLinkActivated(string url)
        {
            if (_disposed || !_initialized || !_enabled || string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            _deepLinkManager.HandleDeepLinkActivated(url);
        }

        private void HandleTick(float deltaSeconds)
        {
            if (_disposed || !_initialized || !_enabled)
            {
                return;
            }

            try
            {
                _requestQueue.FlushPendingWrite();
                _sessionManager.HandleTick(deltaSeconds, DateTimeOffset.UtcNow);

                if (_requestQueue.Count == 0 || !_deferredFlushDueAt.HasValue)
                {
                    return;
                }

                if (DateTimeOffset.UtcNow >= _deferredFlushDueAt.Value)
                {
                    _deferredFlushDueAt = null;
                    _ = Task.Run(async () => await FlushAsync().ConfigureAwait(false));
                }
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Lifecycle tick handler threw an unexpected error: " + error);
            }
        }

        private void HandleApplicationPaused(bool paused)
        {
            if (_disposed || !_initialized)
            {
                return;
            }
            try
            {

                if (paused)
                {
                    if (_config.SessionTrackingEnabled)
                    {
                        _sessionManager.HandlePause(DateTimeOffset.UtcNow);
                    }

                    if (_enabled)
                    {
                        RequestQueueFlush(true);
                    }

                    return;
                }

                if (_config.SessionTrackingEnabled)
                {
                    _sessionManager.HandleFocus(DateTimeOffset.UtcNow);
                }

                if (_enabled)
                {
                    RequestQueueFlush(true);
                }
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Application-pause handler threw an unexpected error: " + error);
            }
        }

        private void HandleApplicationFocusChanged(bool hasFocus)
        {
            try
            {
                if (_disposed || !_initialized)
                {
                    return;
                }

                if (hasFocus)
                {
                    if (_config.SessionTrackingEnabled)
                    {
                        _sessionManager.HandleFocus(DateTimeOffset.UtcNow);
                    }

                    if (_enabled)
                    {
                        RequestQueueFlush(true);
                    }

                    return;
                }

                if (_config.SessionTrackingEnabled)
                {
                    _sessionManager.HandlePause(DateTimeOffset.UtcNow);
                }

                if (_enabled)
                {
                    RequestQueueFlush(true);
                }
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Application-focus handler threw an unexpected error: " + error);
            }
        }

        private void HandleApplicationQuitting()
        {
            try
            {
                if (_disposed || !_initialized)
                {
                    return;
                }

                if (_config.SessionTrackingEnabled)
                {
                    _sessionManager.HandleQuitting(DateTimeOffset.UtcNow);
                }

                if (_enabled)
                {
                    RequestQueueFlush(true);
                }
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Application-quitting handler threw an unexpected error: " + error);
            }
        }

        private void HandleSceneChanged(string previousScene, string nextScene)
        {
            if (!_initialized || !_config.AutomaticSceneTracking || !_enabled || !_eventsEnabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nextScene))
            {
                return;
            }

            ObserveBackgroundTask(
                TrackPageViewAsync(nextScene, new AttriaxPageViewOptions
                {
                    PreviousPageName = string.IsNullOrWhiteSpace(previousScene) ? null : previousScene,
                    PageClass = nextScene,
                    PageTitle = nextScene,
                    Source = "automatic_scene",
                }),
                "Automatic Unity scene page tracking failed.");
        }

        private void QueueSessionLifecycle(
            SdkSessionLifecycleKind kind,
            AttriaxSessionSnapshot session,
            DateTimeOffset occurredAt,
            IDictionary<string, object>? metadata = null)
        {
            if (!_config.SessionTrackingEnabled || !ShouldDispatchAnalyticsInCurrentMode())
            {
                return;
            }

            ObserveBackgroundTask(
                _requestQueue.Enqueue(
                    AttriaxQueuedRequest.CreateSession(
                        AttriaxGeneratedRequestFactory.BuildTrackSessionRequest(
                            _config.ProjectToken,
                            SessionTrackingDecision.AttachDeviceIdentity ? _deviceId : null,
                            SessionTrackingDecision.AttachDeviceIdentity ? RequireDeviceIdSource() : null,
                            session,
                            kind,
                            occurredAt,
                            metadata))),
                "Session lifecycle queueing failed.");
            RequestQueueFlush(true);
        }

        private void RequestQueueFlush(bool immediate)
        {
            if (_requestQueue.Count == 0)
            {
                return;
            }

            if (_consentManager.ShouldDeferNetworkDispatch)
            {
                _deferredFlushDueAt = null;
                SetSynchronizationState(AttriaxSynchronizationState.Deferred);
                return;
            }

            if (_shouldGateRequestsOnSuccessfulAppOpen && !_appOpenManager.HasSuccessfulResult)
            {
                ScheduleLaunchPreparationIfNeeded();
            }

            if (immediate || _config.EventFlushIntervalMs == 0)
            {
                _deferredFlushDueAt = null;
                _ = Task.Run(async () => await FlushAsync().ConfigureAwait(false));
                return;
            }

            if (!_deferredFlushDueAt.HasValue)
            {
                ScheduleFlushAt(DateTimeOffset.UtcNow.AddMilliseconds(_config.EventFlushIntervalMs));
            }
            else
            {
                ScheduleFlushAt(DateTimeOffset.UtcNow.AddMilliseconds(_config.EventFlushIntervalMs));
            }

            SetSynchronizationState(AttriaxSynchronizationState.Deferred);
        }

        private void ScheduleFlushAt(DateTimeOffset scheduledAt)
        {
            if (!_deferredFlushDueAt.HasValue || scheduledAt < _deferredFlushDueAt.Value)
            {
                _deferredFlushDueAt = scheduledAt;
            }
        }

        private void ScheduleRetryFlush(DateTimeOffset? retryAt = null)
        {
            if (_requestQueue.Count == 0)
            {
                return;
            }

            var scheduledAt = retryAt ?? DateTimeOffset.UtcNow.AddMilliseconds(ResolveRetryDelayMs());
            ScheduleFlushAt(scheduledAt);
        }

        private int ResolveRetryDelayMs()
        {
            return _config.EventFlushIntervalMs > 0
                ? _config.EventFlushIntervalMs
                : DefaultEventFlushIntervalMs;
        }

        private void ObserveBackgroundTask(Task task, string message)
        {
            var generation = _backgroundTaskGeneration;
            task.ContinueWith(
                continuation =>
                {
                    if (generation != _backgroundTaskGeneration)
                    {
                        return;
                    }

                },
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }

        private async Task FlushInternalAsync()
        {
            try
            {
            if (_disposed || !_enabled)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Disabled);
                return;
            }

            if (_consentManager.ShouldDeferNetworkDispatch)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Deferred);
                return;
            }

            if (!IsOnline())
            {
                SetSynchronizationState(AttriaxSynchronizationState.Offline);
                ScheduleRetryFlush();
                return;
            }

            if (_requestQueue.Count == 0)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Synchronized);
                return;
            }

            RewriteAndPurgeQueuedRequestsForConsent();

            if (_requestQueue.Count == 0)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Synchronized);
                return;
            }

            SetSynchronizationState(AttriaxSynchronizationState.Synchronizing);
            _requestQueue.PrioritizeOpenRequests();

            var queueIndex = 0;
            while (queueIndex < _requestQueue.Count)
            {
                var now = DateTimeOffset.UtcNow;
                var entry = _requestQueue.PeekAt(queueIndex);

                if (_config.GdprEnabled && !_consentManager.IsWaitingForConsent && !ConsentQueuePolicy.IsRequestAllowedByResolvedConsent(entry))
                {
                    _requestQueue.RemoveAt(queueIndex);
                    _requestQueue.Reject(entry.Id, new AttriaxApiError(
                        "Queued request was dropped because GDPR consent blocked this category.",
                        null,
                        false,
                        true));
                    continue;
                }

                var retryDropReason = AttriaxRequestRetryPolicy.GetTerminalDropReason(entry, now);
                if (retryDropReason != null)
                {
                    _requestQueue.RemoveAt(queueIndex);
                    _requestQueue.Reject(entry.Id, new AttriaxApiError(
                        "Attriax request dropped after exceeding the retry policy: " + retryDropReason + ".",
                        entry.LastHttpStatusCode,
                        false,
                        true));
                    continue;
                }

                if (AttriaxRequestRetryPolicy.IsWaitingForRetryWindow(entry, now))
                {
                    queueIndex += 1;
                    continue;
                }

                if (!CanDispatchRequest(entry))
                {
                    queueIndex += 1;
                    continue;
                }

                if (IsBatchableRequest(entry))
                {
                    var batchEntries = CollectSendableBatchEntries(queueIndex, now);
                    var batchResult = await FlushBatchEntriesAsync(batchEntries, queueIndex)
                        .ConfigureAwait(false);
                    if (batchResult.ShouldStop)
                    {
                        return;
                    }

                    queueIndex += batchResult.RemainingCount;
                    continue;
                }

                try
                {
                    var result = await PerformQueuedRequestAsync(entry).ConfigureAwait(false);
                    _requestQueue.RemoveAt(queueIndex);
                    _requestQueue.Complete(entry.Id, result);
                }
                catch (AttriaxApiError error)
                {
                    if (error.ShouldDrop)
                    {
                        _requestQueue.RemoveAt(queueIndex);
                        _requestQueue.Reject(entry.Id, error);
                        continue;
                    }

                    if (error.Retriable)
                    {
                        _requestQueue.ReplaceAt(
                            queueIndex,
                            AttriaxRequestRetryPolicy.MarkForRetry(
                                entry,
                                error,
                                now));
                        queueIndex += 1;
                        continue;
                    }

                    SetSynchronizationState(AttriaxSynchronizationState.Failed);
                    _requestQueue.Reject(entry.Id, error);
                    return;
                }
                catch (Exception error)
                {
                    SetSynchronizationState(AttriaxSynchronizationState.Failed);
                    _requestQueue.Reject(entry.Id, error);
                    return;
                }
            }

            if (_requestQueue.Count > 0)
            {
                var nextRetryAt = _requestQueue.PeekEarliestRetryAt();
                if (nextRetryAt.HasValue)
                {
                    SetSynchronizationState(IsOnline()
                        ? AttriaxSynchronizationState.Deferred
                        : AttriaxSynchronizationState.Offline);
                    ScheduleRetryFlush(nextRetryAt.Value > DateTimeOffset.UtcNow
                        ? nextRetryAt.Value
                        : DateTimeOffset.UtcNow);
                    return;
                }

                SetSynchronizationState(AttriaxSynchronizationState.Deferred);
                return;
            }

            SetSynchronizationState(AttriaxSynchronizationState.Synchronized);
            }
            catch (Exception error)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Failed);
                UnityEngine.Debug.LogError(
                    "[Attriax] Request-flush loop terminated by an unexpected error: " + error);
            }
        }

        private bool CanDispatchRequest(AttriaxQueuedRequest entry)
        {
            if (!_shouldGateRequestsOnSuccessfulAppOpen)
            {
                return true;
            }

            var canDispatch = entry.Kind == AttriaxQueuedRequestKind.Open
                || entry.Kind == AttriaxQueuedRequestKind.DeepLinkResolve
                || _appOpenManager.HasSuccessfulResult;

            return canDispatch;
        }

        private async Task<BatchFlushResult> FlushBatchEntriesAsync(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            int startIndex)
        {
            if (entries.Count == 0)
            {
                return new BatchFlushResult(false, 0);
            }

            var preparedBatch = PrepareBatchEntries(entries, DateTimeOffset.UtcNow);

            try
            {
                await _generatedGateway.SendBatchAsync(preparedBatch.TransportEntries, AttriaxBatchLimits.MaxItemCount, AttriaxBatchLimits.MaxBodyBytes)
                    .ConfigureAwait(false);
                _requestQueue.RemoveRange(startIndex, preparedBatch.QueuedEntries.Count);
                foreach (var entry in preparedBatch.QueuedEntries)
                {
                    _requestQueue.Complete(entry.Id, null);
                }

                if (preparedBatch.KeepAliveSessionId != null && preparedBatch.KeepAliveOccurredAt.HasValue)
                {
                    _sessionManager.HandleSuccessfulForegroundFlush(
                        preparedBatch.KeepAliveSessionId,
                        preparedBatch.KeepAliveOccurredAt.Value);
                }

                return new BatchFlushResult(false, 0);
            }
            catch (Exception error)
            {
                var normalizedError = NormalizeBatchError(error);

                if (normalizedError.Retriable)
                {
                    var attemptedAt = DateTimeOffset.UtcNow;
                    for (var index = 0; index < entries.Count; index += 1)
                    {
                        _requestQueue.ReplaceAt(
                            startIndex + index,
                            AttriaxRequestRetryPolicy.MarkForRetry(
                                entries[index],
                                normalizedError,
                                attemptedAt));
                    }

                    return new BatchFlushResult(false, entries.Count);
                }

                if (entries.Count > 1)
                {
                    var splitIndex = entries.Count / 2;

                    var firstResult = await FlushBatchEntriesAsync(entries.Take(splitIndex).ToList(), startIndex)
                        .ConfigureAwait(false);
                    if (firstResult.ShouldStop)
                    {
                        return firstResult;
                    }

                    var secondResult = await FlushBatchEntriesAsync(
                            entries.Skip(splitIndex).ToList(),
                            startIndex + firstResult.RemainingCount)
                        .ConfigureAwait(false);
                    if (secondResult.ShouldStop)
                    {
                        return secondResult;
                    }

                    return new BatchFlushResult(false, firstResult.RemainingCount + secondResult.RemainingCount);
                }

                var singleEntry = entries[0];
                if (normalizedError.ShouldDrop)
                {
                    _requestQueue.RemoveAt(startIndex);
                    _requestQueue.Reject(singleEntry.Id, normalizedError);
                    return new BatchFlushResult(false, 0);
                }

                SetSynchronizationState(AttriaxSynchronizationState.Failed);
                _requestQueue.Reject(singleEntry.Id, normalizedError);
                return new BatchFlushResult(true, 1);
            }
        }

        private List<AttriaxQueuedRequest> CollectSendableBatchEntries(int startIndex, DateTimeOffset now)
        {
            var entries = _requestQueue.PeekBatchablePrefix(startIndex);
            if (entries.Count <= 1)
            {
                return new List<AttriaxQueuedRequest>(entries);
            }

            var keepAliveOccurredAt = DateTimeOffset.UtcNow;
            var sendableEntries = new List<AttriaxQueuedRequest>(entries.Count);
            for (var index = 0; index < entries.Count; index += 1)
            {
                if (AttriaxRequestRetryPolicy.IsWaitingForRetryWindow(entries[index], now)
                    || AttriaxRequestRetryPolicy.GetTerminalDropReason(entries[index], now) != null)
                {
                    break;
                }

                sendableEntries.Add(entries[index]);
                var preparedBatch = PrepareBatchEntries(sendableEntries, keepAliveOccurredAt);
                if (!_generatedGateway.FitsBatch(
                        preparedBatch.TransportEntries,
                        AttriaxBatchLimits.MaxItemCount,
                        AttriaxBatchLimits.MaxBodyBytes))
                {
                    sendableEntries.RemoveAt(sendableEntries.Count - 1);
                    break;
                }
            }

            if (sendableEntries.Count > 0)
            {
                return sendableEntries;
            }

            return new List<AttriaxQueuedRequest>
            {
                entries[0],
            };
        }

        private PreparedBatchEntries PrepareBatchEntries(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            DateTimeOffset occurredAt)
        {
            var transportEntries = new List<AttriaxQueuedRequest>(entries.Count + 1);
            transportEntries.AddRange(entries);

            var keepAliveEntry = BuildSessionKeepAliveBatchEntry(entries, occurredAt);
            if (keepAliveEntry == null)
            {
                return new PreparedBatchEntries(entries, transportEntries, null, null);
            }

            transportEntries.Add(keepAliveEntry);
            var keepAliveRequest = keepAliveEntry.RequireSessionRequest();
            return new PreparedBatchEntries(
                entries,
                transportEntries,
                keepAliveRequest.sessionId,
                occurredAt);
        }

        private AttriaxQueuedRequest? BuildSessionKeepAliveBatchEntry(
            IReadOnlyList<AttriaxQueuedRequest> entries,
            DateTimeOffset occurredAt)
        {
            var currentSession = _sessionManager.CurrentSession;
            if (currentSession == null || _sessionManager.IsBackgrounded)
            {
                return null;
            }

            var includesCurrentSessionEvent = entries.Any(entry =>
                entry.Kind == AttriaxQueuedRequestKind.Event &&
                string.Equals(entry.RequireEventRequest().sessionId, currentSession.Id, StringComparison.Ordinal));
            if (!includesCurrentSessionEvent)
            {
                return null;
            }

            var keepAliveRequest = AttriaxGeneratedRequestFactory.BuildTrackSessionRequest(
                _config.ProjectToken,
                SessionTrackingDecision.AttachDeviceIdentity ? currentSession.DeviceId : null,
                SessionTrackingDecision.AttachDeviceIdentity ? RequireDeviceIdSource() : null,
                currentSession,
                SdkSessionLifecycleKind.Heartbeat,
                occurredAt,
                null);
            return AttriaxQueuedRequest.CreateSession(keepAliveRequest);
        }

        private async Task<object> PerformQueuedRequestAsync(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Open:
                {
                    var result = await _generatedGateway.SendOpenAsync(entry.RequireOpenRequest()).ConfigureAwait(false);
                    await _skanManager.ApplyAppOpenResultAsync(result).ConfigureAwait(false);
                    _appOpenManager.HandleResult(result);

                    return result;
                }
                case AttriaxQueuedRequestKind.Event:
                {
                    await _generatedGateway.SendTrackEventAsync(entry.RequireEventRequest()).ConfigureAwait(false);
                    return null;
                }
                case AttriaxQueuedRequestKind.Crash:
                {
                    await _generatedGateway.SendTrackCrashAsync(entry.RequireCrashRequest()).ConfigureAwait(false);
                    return null;
                }
                case AttriaxQueuedRequestKind.Notification:
                {
                    await _generatedGateway.SendTrackNotificationAsync(entry.RequireNotificationRequest()).ConfigureAwait(false);
                    return null;
                }
                case AttriaxQueuedRequestKind.Session:
                {
                    await _generatedGateway.SendTrackSessionAsync(entry.RequireSessionRequest()).ConfigureAwait(false);
                    return null;
                }
                case AttriaxQueuedRequestKind.User:
                {
                    await _generatedGateway.SendSetUserAsync(entry.RequireUserRequest()).ConfigureAwait(false);
                    return null;
                }
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                {
                    return await _generatedGateway.SendDeepLinkResolutionAsync(entry.RequireDeepLinkResolveRequest())
                        .ConfigureAwait(false);
                }
                case AttriaxQueuedRequestKind.UninstallToken:
                {
                    await _generatedGateway.SendRegisterUninstallTokenAsync(entry.RequireUninstallTokenRequest())
                        .ConfigureAwait(false);
                    return null;
                }
                default:
                    throw new AttriaxApiError(
                        string.Format(CultureInfo.InvariantCulture, "Unsupported queued request kind: {0}", entry.Kind),
                        null,
                        false,
                        true);
            }
        }

        private static bool IsBatchableRequest(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.User:
                    return true;
                case AttriaxQueuedRequestKind.Event:
                    return !string.IsNullOrWhiteSpace(entry.RequireEventRequest().deviceId);
                case AttriaxQueuedRequestKind.Session:
                    return !string.IsNullOrWhiteSpace(entry.RequireSessionRequest().deviceId);
                default:
                    return false;
            }
        }

        private static AttriaxApiError NormalizeBatchError(Exception error)
        {
            if (error is AttriaxApiError apiError)
            {
                return apiError;
            }

            return new AttriaxApiError(
                string.IsNullOrWhiteSpace(error.Message)
                    ? "Attriax batch request failed."
                    : error.Message,
                null,
                false,
                false,
                error);
        }

        private async Task<PreparedContext> PrepareContextAsync(
            ResolvedDeviceId resolvedDeviceId,
            bool isFirstLaunch,
            bool resolveInstallReferrer,
            AttriaxNativeContextPayload? nativeContextOverride = null)
        {
            var platform = GetCurrentPlatform();
            var nativeContext = nativeContextOverride
                ?? await CollectNativeContextAsync(platform).ConfigureAwait(false);
            var initialInstallReferrerContext = BuildInitialInstallReferrerContext(platform);
            var initialSnapshot = BuildContextSnapshot(
                platform,
                resolvedDeviceId.Value,
                isFirstLaunch,
                nativeContext,
                initialInstallReferrerContext,
                initialInstallReferrerContext.InstallReferrer);

            return new PreparedContext
            {
                InitialSnapshot = initialSnapshot,
                DeviceId = resolvedDeviceId,
                ResolvedSnapshotTask = resolveInstallReferrer
                    ? ResolveContextSnapshotAsync(
                        platform,
                        resolvedDeviceId.Value,
                        isFirstLaunch,
                        nativeContext,
                        initialInstallReferrerContext)
                    : Task.FromResult(initialSnapshot),
            };
        }

        private async Task<PreparedContext> PrepareIdentifiedContextAsync(
            bool isFirstLaunch,
            bool resolveInstallReferrer)
        {
            var storedDeviceId = EnsureDeviceId();
            _deviceId = storedDeviceId.Value;
            _deviceIdSource = ReadStoredDeviceIdSource();

            AttriaxNativeContextPayload? initialNativeContext = null;
            ResolvedDeviceId resolvedDeviceId;
            if (!string.IsNullOrWhiteSpace(_deviceIdSource))
            {
                resolvedDeviceId = new ResolvedDeviceId
                {
                    Value = _deviceId,
                    Source = _deviceIdSource,
                    IsFallback = string.Equals(
                        _deviceIdSource,
                        PersistentStorageDeviceIdSource,
                        StringComparison.Ordinal),
                };
            }
            else if (storedDeviceId.HasPersistedValue)
            {
                resolvedDeviceId = new ResolvedDeviceId
                {
                    Value = _deviceId,
                    Source = PersistentStorageDeviceIdSource,
                    IsFallback = true,
                };
            }
            else
            {
                var platform = GetCurrentPlatform();
                initialNativeContext = await CollectNativeContextAsync(platform).ConfigureAwait(false);
                resolvedDeviceId = ResolvePreferredDeviceId(platform, initialNativeContext, _deviceId);
            }

            if (!string.Equals(_deviceId, resolvedDeviceId.Value, StringComparison.Ordinal))
            {
                PersistDeviceId(resolvedDeviceId.Value);
            }

            _deviceId = resolvedDeviceId.Value;
            _deviceIdSource = resolvedDeviceId.Source;
            PersistDeviceIdSource(_deviceIdSource);

            var preparedContext = await PrepareContextAsync(
                    resolvedDeviceId,
                    isFirstLaunch,
                    resolveInstallReferrer,
                    initialNativeContext)
                .ConfigureAwait(false);
            return preparedContext;
        }

        private PreparedContext PrepareAnonymousContext(bool isFirstLaunch)
        {
            _deviceId = string.Empty;
            _deviceIdSource = string.Empty;

            var snapshot = BuildAnonymousContextSnapshot(GetCurrentPlatform(), isFirstLaunch);
            return new PreparedContext
            {
                InitialSnapshot = snapshot,
                DeviceId = new ResolvedDeviceId
                {
                    Value = string.Empty,
                    Source = string.Empty,
                    IsFallback = false,
                },
                ResolvedSnapshotTask = Task.FromResult(snapshot),
            };
        }

        private AttriaxContextSnapshot BuildAnonymousContextSnapshot(
            AttriaxPlatformType platform,
            bool isFirstLaunch)
        {
            return _contextSnapshotBuilder.BuildAnonymousContextSnapshot(platform, isFirstLaunch);
        }

        private async Task EnsureIdentifiedContextAsync()
        {
            if (!string.IsNullOrWhiteSpace(_deviceId))
            {
                _sessionManager.SyncCurrentSessionContext();
                return;
            }

            var includesInstallReferrer = _enabled && _consentManager.AllowsAttributionTracking;
            var preparedContext = await PrepareIdentifiedContextAsync(
                    _isFirstLaunch,
                    includesInstallReferrer)
                .ConfigureAwait(false);
            _resolvedPlatform = preparedContext.InitialSnapshot.Platform;
            _contextManager.SetPreparedContext(
                new AttriaxPreparedContextRefresh(
                    preparedContext.InitialSnapshot,
                    preparedContext.ResolvedSnapshotTask),
                includesInstallReferrer);
            _sessionManager.SyncCurrentSessionContext();
        }

        private async Task<AttriaxNativeContextPayload> CollectNativeContextAsync(
            AttriaxPlatformType platform)
        {
            return await AttriaxNativeBridge.CollectNativeContextAsync(
                    platform,
                    _config.CollectAdvertisingId)
                .ConfigureAwait(false);
        }

        private AttriaxInstallReferrerContextPayload BuildInitialInstallReferrerContext(
            AttriaxPlatformType platform)
        {
            return _platformInstallReferrerManager.BuildInitialInstallReferrerContext(platform);
        }

        private async Task<AttriaxContextSnapshot> ResolveContextSnapshotAsync(
            AttriaxPlatformType platform,
            string deviceId,
            bool isFirstLaunch,
            AttriaxNativeContextPayload nativeContext,
            AttriaxInstallReferrerContextPayload initialInstallReferrerContext)
        {
            var installReferrerContext = await CollectInstallReferrerContextAsync(platform)
                .ConfigureAwait(false);
            var installReferrer = FirstNonEmpty(
                FirstNonEmpty(installReferrerContext.InstallReferrer, nativeContext.InstallReferrer),
                initialInstallReferrerContext.InstallReferrer);

            if (!string.IsNullOrWhiteSpace(installReferrer))
            {
                PersistInstallReferrer(installReferrer);
            }

            return BuildContextSnapshot(
                platform,
                deviceId,
                isFirstLaunch,
                nativeContext,
                installReferrerContext,
                installReferrer);
        }

        private AttriaxContextSnapshot BuildContextSnapshot(
            AttriaxPlatformType platform,
            string deviceId,
            bool isFirstLaunch,
            AttriaxNativeContextPayload nativeContext,
            AttriaxInstallReferrerContextPayload installReferrerContext,
            string? rawPlatformInstallReferrer)
        {
            return _contextSnapshotBuilder.BuildContextSnapshot(
                platform,
                deviceId,
                isFirstLaunch,
                nativeContext,
                installReferrerContext,
                rawPlatformInstallReferrer);
        }

        private async Task<AttriaxPreparedContextRefresh> PrepareContextRefreshAsync(
            bool resolveInstallReferrer)
        {
            var preparedContext = await PrepareContextAsync(
                    CurrentResolvedDeviceId(),
                    _isFirstLaunch,
                    resolveInstallReferrer)
                .ConfigureAwait(false);
            return new AttriaxPreparedContextRefresh(
                preparedContext.InitialSnapshot,
                preparedContext.ResolvedSnapshotTask);
        }

        private async Task<AttriaxInstallReferrerContextPayload> CollectInstallReferrerContextAsync(
            AttriaxPlatformType platform)
        {
            return await _platformInstallReferrerManager
                .CollectInstallReferrerContextAsync(platform)
                .ConfigureAwait(false);
        }

        private string ReadPersistedInstallReferrer()
        {
            return _installReferrerStore.ReadPersistedInstallReferrer();
        }

        private AttriaxInstallReferrerDetails? ReadPersistedInstallReferrerDetails(string storageKey)
        {
            return _installReferrerStore.ReadPersistedInstallReferrerDetails(storageKey);
        }

        private void PersistInstallReferrer(string installReferrer)
        {
            _installReferrerStore.PersistInstallReferrer(installReferrer);
        }

        private void PersistInstallReferrerDetails(
            string storageKey,
            AttriaxInstallReferrerDetails installReferrerDetails)
        {
            _installReferrerStore.PersistInstallReferrerDetails(storageKey, installReferrerDetails);
        }

        private void ClearPersistedState()
        {
            _runtimeSettingsStore.Clear();
            AttriaxPlayerPrefs.DeleteKey(Key(InstallReferrerStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(OriginalInstallReferrerDetailsStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(ReinstallReferrerDetailsStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(GdprConsentStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(GdprConsentIdStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(SkanStateStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key(SessionStorageKey));
            AttriaxPlayerPrefs.DeleteKey(Key("queue"));
            AttriaxPlayerPrefs.Save();
        }

        private static string FirstNonEmpty(string first, string second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }

            return string.IsNullOrWhiteSpace(second) ? null : second;
        }

        private static string SanitizeOptionalString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value!.Trim();
        }

        private static IDictionary<string, object> BuildPurchaseRevenueEventData(
            string eventName,
            double revenue,
            string currency,
            bool revenueInMicros,
            string? purchaseType,
            string? productId,
            string? transactionId,
            string? originalTransactionId,
            string? validationProvider,
            string? validationEnvironment,
            string? purchaseToken,
            string? receiptData,
            string? signedPayload,
            string? receiptSignature,
            bool? isRenewal,
            int quantity,
            string? store,
            string? packageName,
            bool? voided,
            bool? test,
            string? validationId,
            IDictionary<string, object>? metadata,
            bool isRefund)
        {
            var normalizedCurrency = SanitizeOptionalString(currency)?.ToUpperInvariant();
            if (normalizedCurrency == null || normalizedCurrency.Length != 3)
            {
                normalizedCurrency = "USD";
            }

            var eventData = new Dictionary<string, object>
            {
                [AttriaxAnalyticsParamKeys.Revenue] = revenue,
                [AttriaxAnalyticsParamKeys.Currency] = normalizedCurrency,
            };
            if (revenueInMicros)
            {
                eventData[AttriaxAnalyticsParamKeys.RevenueInMicros] = true;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.PurchaseType, purchaseType);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ProductId, productId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.TransactionId, transactionId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.OriginalTransactionId, originalTransactionId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ValidationProvider, validationProvider);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ValidationEnvironment, validationEnvironment);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.PurchaseToken, purchaseToken);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ReceiptData, receiptData);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.SignedPayload, signedPayload);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ReceiptSignature, receiptSignature);

            if (isRenewal.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.IsRenewal] = isRenewal.Value;
            }

            if (quantity != 1)
            {
                eventData[AttriaxAnalyticsParamKeys.Quantity] = quantity;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.Store, store);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.PackageName, packageName);

            if (voided.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Voided] = voided.Value;
            }

            if (test.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Test] = test.Value;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ValidationId, validationId);

            if (isRefund)
            {
                eventData[AttriaxAnalyticsParamKeys.RevenueType] = AttriaxAnalyticsEventKeys.Refund;
            }

            if (metadata != null)
            {
                foreach (var entry in metadata)
                {
                    eventData[entry.Key] = entry.Value;
                }
            }

            return eventData;
        }

        private static IDictionary<string, object> BuildRefundRevenueEventData(
            double revenue,
            string currency,
            bool revenueInMicros,
            string? purchaseType,
            string? productId,
            string? transactionId,
            string? originalTransactionId,
            int quantity,
            string? store,
            string? packageName,
            bool? voided,
            bool? test,
            string? reason,
            IDictionary<string, object>? metadata)
        {
            var normalizedCurrency = SanitizeOptionalString(currency)?.ToUpperInvariant();
            if (normalizedCurrency == null || normalizedCurrency.Length != 3)
            {
                normalizedCurrency = "USD";
            }

            var refundRevenue = revenue == 0 ? 0 : -Math.Abs(revenue);

            var eventData = new Dictionary<string, object>
            {
                [AttriaxAnalyticsParamKeys.Revenue] = refundRevenue,
                [AttriaxAnalyticsParamKeys.Currency] = normalizedCurrency,
                [AttriaxAnalyticsParamKeys.RevenueType] = AttriaxAnalyticsEventKeys.Refund,
            };
            if (revenueInMicros)
            {
                eventData[AttriaxAnalyticsParamKeys.RevenueInMicros] = true;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.PurchaseType, purchaseType);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.ProductId, productId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.TransactionId, transactionId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.OriginalTransactionId, originalTransactionId);

            if (quantity != 1)
            {
                eventData[AttriaxAnalyticsParamKeys.Quantity] = quantity;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.Store, store);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.PackageName, packageName);

            if (voided.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Voided] = voided.Value;
            }

            if (test.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Test] = test.Value;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.Reason, reason);

            if (metadata != null)
            {
                foreach (var entry in metadata)
                {
                    eventData[entry.Key] = entry.Value;
                }
            }

            return eventData;
        }

        private static IDictionary<string, object> BuildAdRevenueEventData(
            double revenue,
            string currency,
            bool revenueInMicros,
            string? adNetwork,
            string? adFormat,
            string? adType,
            string? adPlacement,
            bool? test,
            IDictionary<string, object>? metadata)
        {
            var normalizedCurrency = SanitizeOptionalString(currency)?.ToUpperInvariant();
            if (normalizedCurrency == null || normalizedCurrency.Length != 3)
            {
                normalizedCurrency = "USD";
            }

            var eventData = new Dictionary<string, object>
            {
                [AttriaxAnalyticsParamKeys.Revenue] = revenue,
                [AttriaxAnalyticsParamKeys.Currency] = normalizedCurrency,
            };
            if (revenueInMicros)
            {
                eventData[AttriaxAnalyticsParamKeys.RevenueInMicros] = true;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdNetwork, adNetwork);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdFormat, adFormat);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdType, adType);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdPlacement, adPlacement);

            if (test.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Test] = test.Value;
            }

            if (metadata != null)
            {
                foreach (var entry in metadata)
                {
                    eventData[entry.Key] = entry.Value;
                }
            }

            return eventData;
        }

        private static IDictionary<string, object> BuildAdEventData(
            string? adNetwork,
            string? mediationNetwork,
            string? adUnitId,
            string? adPlacement,
            string? adFormat,
            string? adType,
            string? failureReason,
            double? loadLatencyMs,
            string? rewardType,
            double? rewardAmount,
            bool? test,
            IDictionary<string, object>? metadata)
        {
            var eventData = new Dictionary<string, object>();

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdNetwork, adNetwork);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.MediationNetwork, mediationNetwork);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdUnitId, adUnitId);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdPlacement, adPlacement);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdFormat, adFormat);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.AdType, adType);
            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.FailureReason, failureReason);

            if (loadLatencyMs.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.LoadLatencyMs] = loadLatencyMs.Value;
            }

            AddIfNotNullOrWhitespace(eventData, AttriaxAnalyticsParamKeys.RewardType, rewardType);

            if (rewardAmount.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.RewardAmount] = rewardAmount.Value;
            }

            if (test.HasValue)
            {
                eventData[AttriaxAnalyticsParamKeys.Test] = test.Value;
            }

            if (metadata != null)
            {
                foreach (var entry in metadata)
                {
                    eventData[entry.Key] = entry.Value;
                }
            }

            return eventData;
        }

        private static void AddIfNotNullOrWhitespace(IDictionary<string, object> eventData, string key, string? value)
        {
            var sanitized = SanitizeOptionalString(value);
            if (sanitized != null)
            {
                eventData[key] = sanitized;
            }
        }

        private static string ReadString(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<string>();
            }

            return rawValue.ToString();
        }

        private static long? ReadInt64(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is long longValue)
            {
                return longValue;
            }

            if (rawValue is int intValue)
            {
                return intValue;
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<long?>();
            }

            if (long.TryParse(rawValue.ToString(), out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static T? DeepClone<T>(T? value)
            where T : class
        {
            if (value == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }

        private StoredDeviceIdState EnsureDeviceId()
        {
            var existing = SanitizeDeviceIdCandidate(_runtimeSettingsStore.ReadDeviceId());
            if (!string.IsNullOrWhiteSpace(existing))
            {
                return new StoredDeviceIdState
                {
                    Value = existing,
                    HasPersistedValue = true,
                };
            }

            var generated = Guid.NewGuid().ToString("N");
            PersistDeviceId(generated);
            return new StoredDeviceIdState
            {
                Value = generated,
                HasPersistedValue = false,
            };
        }

        private void PersistDeviceId(string deviceId)
        {
            _runtimeSettingsStore.WriteDeviceId(deviceId);
        }

        private string ReadStoredDeviceIdSource()
        {
            return SanitizeDeviceIdCandidate(_runtimeSettingsStore.ReadDeviceIdSource());
        }

        private void PersistDeviceIdSource(string deviceIdSource)
        {
            _runtimeSettingsStore.WriteDeviceIdSource(deviceIdSource);
        }

        private AttriaxConsentIdentity EnsureConsentDeviceIdentity()
        {
            var consentId = AttriaxPlayerPrefs.GetString(Key(GdprConsentIdStorageKey), string.Empty);
            if (string.IsNullOrWhiteSpace(consentId))
            {
                consentId = Guid.NewGuid().ToString("N");
                AttriaxPlayerPrefs.SetString(Key(GdprConsentIdStorageKey), consentId);
                AttriaxPlayerPrefs.Save();
            }

            return new AttriaxConsentIdentity(consentId);
        }

        private string? ResolveCurrentTimezone()
        {
            try
            {
                if (_initialized)
                {
                    return SanitizeDeviceIdCandidate(_contextManager.Snapshot.Device.Timezone);
                }

                return SanitizeDeviceIdCandidate(TimeZoneInfo.Local.Id);
            }
            catch
            {
                return null;
            }
        }

        private ResolvedDeviceId CurrentResolvedDeviceId()
        {
            var source = RequireDeviceIdSource();
            return new ResolvedDeviceId
            {
                Value = _deviceId,
                Source = source,
                IsFallback = string.Equals(source, PersistentStorageDeviceIdSource, StringComparison.Ordinal),
            };
        }

        private ResolvedDeviceId ResolvePreferredDeviceId(
            AttriaxPlatformType platform,
            AttriaxNativeContextPayload nativeContext,
            string fallbackDeviceId)
        {
            ResolvedDeviceId? preferred = null;

            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    preferred = BuildResolvedDeviceId(nativeContext.AndroidId, "android_ssaid")
                        ?? (_config.CollectAdvertisingId
                            ? BuildResolvedDeviceId(nativeContext.AdvertisingId, "android_gaid")
                            : null);
                    break;
                case AttriaxPlatformType.IOS:
                    preferred = BuildResolvedDeviceId(
                        ReadString(nativeContext.Metadata, "keychainDeviceId"),
                        "ios_keychain")
                        ?? BuildResolvedDeviceId(
                            ReadString(nativeContext.Metadata, "vendorIdentifier"),
                            "ios_idfv");
                    break;
                case AttriaxPlatformType.Windows:
                    preferred = BuildResolvedDeviceId(
                        TryReadWindowsMachineGuid(),
                        "windows_machine_guid");
                    break;
                case AttriaxPlatformType.MacOS:
                    preferred = BuildResolvedDeviceId(
                        TryReadMacOSPlatformUuid(),
                        "macos_platform_uuid")
                        ?? BuildResolvedDeviceId(
                            TryReadMacOSKeychainDeviceId(),
                            "macos_keychain");
                    break;
                case AttriaxPlatformType.Linux:
                    preferred = BuildResolvedDeviceId(
                        TryReadLinuxMachineId(),
                        "linux_machine_id");
                    break;
                default:
                    break;
            }

            return preferred ?? new ResolvedDeviceId
            {
                Value = fallbackDeviceId,
                Source = PersistentStorageDeviceIdSource,
                IsFallback = true,
            };
        }

        private ResolvedDeviceId? BuildResolvedDeviceId(
            string rawValue,
            string source)
        {
            var candidate = SanitizeDeviceIdCandidate(rawValue);
            if (candidate == null)
            {
                return null;
            }

            return new ResolvedDeviceId
            {
                Value = candidate,
                Source = source,
            };
        }

        private string RequireDeviceIdSource()
        {
            return string.IsNullOrWhiteSpace(_deviceIdSource)
                ? PersistentStorageDeviceIdSource
                : _deviceIdSource;
        }

        private static string TryReadWindowsMachineGuid()
        {
#if UNITY_STANDALONE_WIN
            try
            {
                var registryType = Type.GetType("Microsoft.Win32.Registry, Microsoft.Win32.Registry");
                var getValueMethod = registryType?.GetMethod(
                    "GetValue",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(string), typeof(string), typeof(object) },
                    modifiers: null);
                var value = getValueMethod?.Invoke(
                    obj: null,
                    parameters: new object[]
                    {
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
                        "MachineGuid",
                        null,
                    });
                return SanitizeDeviceIdCandidate(value?.ToString());
            }
            catch (Exception)
            {
                return null;
            }
#else
            return null;
#endif
        }

        private static string TryReadLinuxMachineId()
        {
#if UNITY_STANDALONE_LINUX
            try
            {
                foreach (var path in new[] { "/etc/machine-id", "/var/lib/dbus/machine-id" })
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    var value = SanitizeDeviceIdCandidate(File.ReadAllText(path));
                    if (value != null)
                    {
                        return value;
                    }
                }
            }
            catch (Exception)
            {
            }
#endif
            return null;
        }

        private static string TryReadMacOSPlatformUuid()
        {
#if UNITY_STANDALONE_OSX
            var output = RunProcessAndCaptureOutput("/usr/sbin/ioreg", "-d2 -c IOPlatformExpertDevice");
            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            foreach (var rawLine in output.Split('\n'))
            {
                var line = rawLine.Trim();
                const string marker = "\"IOPlatformUUID\" = \"";
                var markerIndex = line.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0)
                {
                    continue;
                }

                var valueStart = markerIndex + marker.Length;
                var valueEnd = line.IndexOf('"', valueStart);
                if (valueEnd > valueStart)
                {
                    return SanitizeDeviceIdCandidate(line.Substring(valueStart, valueEnd - valueStart));
                }
            }
#endif
            return null;
        }

        private static string TryReadMacOSKeychainDeviceId()
        {
#if UNITY_STANDALONE_OSX
            var service = string.IsNullOrWhiteSpace(Application.identifier)
                ? "com.attriax.sdk"
                : Application.identifier;
            return SanitizeDeviceIdCandidate(
                RunProcessAndCaptureOutput(
                    "/usr/bin/security",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "find-generic-password -s {0} -a attriax.device_id -w",
                        service)));
#else
            return null;
#endif
        }

        private static string RunProcessAndCaptureOutput(string fileName, string arguments)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(1500);
                    if (!process.HasExited)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception)
                        {
                        }

                        return null;
                    }

                    if (process.ExitCode != 0)
                    {
                        return null;
                    }

                    return output?.Trim();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SanitizeDeviceIdCandidate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (string.Equals(normalized, "n/a", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "unknown", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "unsupported", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return normalized;
        }

        private bool ReadBoolean(string key, bool defaultValue)
        {
            if (!AttriaxPlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            return AttriaxPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        private void WriteBoolean(string key, bool value)
        {
            AttriaxPlayerPrefs.SetInt(key, value ? 1 : 0);
            AttriaxPlayerPrefs.Save();
        }

        private AttriaxPlayerPrefsPersistenceMode ResolveInitialRuntimePersistenceMode()
        {
            if (!_config.GdprEnabled)
            {
                return AttriaxPlayerPrefsPersistenceMode.FullRuntime;
            }

            var raw = AttriaxPlayerPrefs.GetString(Key(GdprConsentStorageKey), string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return AttriaxPlayerPrefsPersistenceMode.ConsentOnly;
            }

            try
            {
                var stored = JsonConvert.DeserializeObject<StoredRuntimeConsentState>(raw);
                if (stored == null)
                {
                    return AttriaxPlayerPrefsPersistenceMode.ConsentOnly;
                }

                if (string.Equals(stored.State, "not_required", StringComparison.Ordinal))
                {
                    return AttriaxPlayerPrefsPersistenceMode.FullRuntime;
                }

                if (string.Equals(stored.State, "granted", StringComparison.Ordinal) &&
                    stored.Values != null &&
                    (stored.Values.Analytics || stored.Values.Attribution || stored.Values.AdEvents))
                {
                    return AttriaxPlayerPrefsPersistenceMode.FullRuntime;
                }
            }
            catch (JsonException exception)
            {
            }

            return AttriaxPlayerPrefsPersistenceMode.ConsentOnly;
        }

        private void SyncRuntimePersistenceMode()
        {
            AttriaxPlayerPrefs.SetRuntimePersistenceMode(
                _runtimeScopedStorageKeys,
                ShouldMaterializeIdentifiedContext
                    ? AttriaxPlayerPrefsPersistenceMode.FullRuntime
                    : AttriaxPlayerPrefsPersistenceMode.ConsentOnly);
        }

        private string Key(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", _config.StorageKeyPrefix, _storageNamespace, name);
        }

        private static string BuildStorageNamespace(string projectToken)
        {
            using (var sha = SHA256.Create())
            {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(projectToken));
                var builder = new StringBuilder(16);
                for (var index = 0; index < 8; index += 1)
                {
                    builder.Append(bytes[index].ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        [Serializable]
        private sealed class StoredRuntimeConsentState
        {
            public string? State { get; set; }

            public StoredRuntimeConsentValues? Values { get; set; }
        }

        [Serializable]
        private sealed class StoredRuntimeConsentValues
        {
            public bool Analytics { get; set; }

            public bool Attribution { get; set; }

            public bool AdEvents { get; set; }
        }

        private static NormalizedConfig NormalizeConfig(AttriaxConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(config.ProjectToken))
            {
                throw new ArgumentException("AttriaxConfig.ProjectToken is required.", nameof(config));
            }

            var apiBaseUrl = string.IsNullOrWhiteSpace(config.ApiBaseUrl) ? DefaultApiBaseUrl : config.ApiBaseUrl.TrimEnd('/');
            if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
            {
                throw new ArgumentException("AttriaxConfig.ApiBaseUrl must be a valid absolute URL.", nameof(config));
            }

            var normalizedHost = NormalizeHost(apiUri.Host);
            var isLocalhost = normalizedHost == "localhost" || normalizedHost == "127.0.0.1" || normalizedHost == "::1";
            if (apiUri.Scheme != Uri.UriSchemeHttps && !(isLocalhost && apiUri.Scheme == Uri.UriSchemeHttp))
            {
                throw new ArgumentException("AttriaxConfig.ApiBaseUrl must use HTTPS unless it targets localhost.", nameof(config));
            }

            return new NormalizedConfig
            {
                ProjectToken = config.ProjectToken.Trim(),
                ApiBaseUrl = apiBaseUrl,
                SdkBaseUrl = BuildSdkBaseUrl(apiBaseUrl),
                AppVersion = config.AppVersion,
                AppBuildNumber = config.AppBuildNumber,
                AppPackageName = config.AppPackageName,
                SdkMetadata = AttriaxObjectNormalizer.NormalizeObjectMap(config.SdkMetadata) ?? new Dictionary<string, object>(),
                EnableDebugLogs = config.EnableDebugLogs,
                GdprEnabled = config.GdprEnabled,
                AnonymousTracking = config.AnonymousTracking,
                CollectAdvertisingId = config.CollectAdvertisingId,
                AutomaticCrashReportingEnabled = config.AutomaticCrashReportingEnabled,
                RequestTrackingAuthorizationOnInit = config.RequestTrackingAuthorizationOnInit,
                TrackingAuthorizationStatusTimeoutMs = config.TrackingAuthorizationStatusTimeoutMs <= 0
                    ? DefaultTrackingAuthorizationStatusTimeoutMs
                    : config.TrackingAuthorizationStatusTimeoutMs,
                SessionTrackingEnabled = config.SessionTrackingEnabled,
                AutomaticSceneTracking = config.AutomaticSceneTracking,
                AutomaticBrowserHandling = config.AutomaticBrowserHandling,
                SessionHeartbeatIntervalMs = config.SessionHeartbeatIntervalMs <= 0 ? DefaultSessionHeartbeatIntervalMs : config.SessionHeartbeatIntervalMs,
                FirstLaunchSessionHeartbeatIntervalMs = config.FirstLaunchSessionHeartbeatIntervalMs <= 0 ? DefaultFirstLaunchSessionHeartbeatIntervalMs : config.FirstLaunchSessionHeartbeatIntervalMs,
                EventFlushIntervalMs = config.EventFlushIntervalMs < 0 ? throw new ArgumentException("AttriaxConfig.EventFlushIntervalMs must not be negative.", nameof(config)) : config.EventFlushIntervalMs,
                FlushEventsImmediatelyOnFirstLaunch = config.FlushEventsImmediatelyOnFirstLaunch,
                RequestTimeoutMs = config.RequestTimeoutMs <= 0 ? DefaultRequestTimeoutMs : config.RequestTimeoutMs,
                MaxQueueSize = config.MaxQueueSize <= 0 ? DefaultMaxQueueSize : config.MaxQueueSize,
                StorageKeyPrefix = string.IsNullOrWhiteSpace(config.StorageKeyPrefix) ? DefaultStorageKeyPrefix : config.StorageKeyPrefix,
                Skan = DeepClone(config.Skan),
            };
        }

        private bool ShouldTrackSessionActivity =>
            ShouldDispatchAnalyticsInCurrentMode() &&
            _config.SessionTrackingEnabled &&
            SessionTrackingDecision.Capture;

        private bool ShouldMaterializeIdentifiedContext => _consentManager.AllowsRuntimePersistence;

        private AttriaxTrackingDecision SessionTrackingDecision =>
            TrackingDecisionFor(AttriaxTrackingSignal.Session);

        private AttriaxTrackingDecision TrackingDecisionFor(AttriaxTrackingSignal signal)
        {
            return _consentManager.TrackingDecisionFor(signal);
        }

        private bool ShouldGateRequestsOnSuccessfulAppOpen =>
            _enabled && _consentManager.AllowsAttributionTracking;

        private bool ShouldDispatchAnalyticsInCurrentMode()
        {
            return true;
        }

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

        private static string BuildSdkBaseUrl(string apiBaseUrl)
        {
            if (apiBaseUrl.EndsWith("/api/sdk", StringComparison.OrdinalIgnoreCase))
            {
                return apiBaseUrl;
            }

            if (apiBaseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                return apiBaseUrl + "/sdk";
            }

            return apiBaseUrl + "/api/sdk";
        }

        private static string NormalizeHost(string host)
        {
            if (host.StartsWith("[", StringComparison.Ordinal) && host.EndsWith("]", StringComparison.Ordinal))
            {
                return host.Substring(1, host.Length - 2);
            }

            return host;
        }

        private static AttriaxPlatformType MapPlatform(RuntimePlatform platform)
        {
            if (Application.isEditor)
            {
                return AttriaxPlatformType.UnityEditor;
            }

            switch (platform)
            {
                case RuntimePlatform.Android:
                    return AttriaxPlatformType.Android;
                case RuntimePlatform.IPhonePlayer:
                    return AttriaxPlatformType.IOS;
                case RuntimePlatform.WebGLPlayer:
                    return AttriaxPlatformType.Web;
                case RuntimePlatform.WindowsPlayer:
                    return AttriaxPlatformType.Windows;
                case RuntimePlatform.OSXPlayer:
                    return AttriaxPlatformType.MacOS;
                case RuntimePlatform.LinuxPlayer:
                    return AttriaxPlatformType.Linux;
                default:
                    return AttriaxPlatformType.Unknown;
            }
        }

        private async Task<AttriaxDeepLinkEvent> MapResolutionToDeepLinkEventAsync(
            AttriaxDeepLinkResolutionResultInternal resolution,
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent? rawEvent,
            DateTimeOffset clickedAt,
            bool isDeferred)
        {
            var handledBySdk = await HandleBrowserActionAsync(resolution.BrowserAction)
                .ConfigureAwait(false);
            return BuildResolvedDeepLinkEvent(
                deepLink: resolution.DeepLink,
                rawEvent: rawEvent,
                clickedAt: clickedAt,
                consumedAt: resolution.ConsumedAt ?? resolution.AcceptedAt ?? clickedAt,
                trigger: BuildDeepLinkTrigger(isDeferred, rawEvent),
                browserAction: resolution.BrowserAction,
                handledBySdk: handledBySdk,
                fallbackUri: BuildFallbackDeepLinkUri(
                    rawEvent,
                    options.Uri,
                    resolution.DeepLink),
                found: resolution.Matched && resolution.DeepLink != null);
        }

        private Task<bool> HandleBrowserActionAsync(AttriaxResolvedUrlAction? browserAction)
        {
            return _deepLinkBrowserHandler.HandleAsync(browserAction);
        }

        private static AttriaxDeepLinkEvent BuildResolvedDeepLinkEvent(
            AttriaxDeepLink? deepLink,
            AttriaxRawDeepLinkEvent? rawEvent,
            DateTimeOffset clickedAt,
            DateTimeOffset consumedAt,
            AttriaxDeepLinkTrigger trigger,
            AttriaxResolvedUrlAction? browserAction,
            bool handledBySdk,
            Uri fallbackUri,
            bool found)
        {
            return new AttriaxDeepLinkEvent
            {
                Uri = deepLink?.Uri ?? fallbackUri,
                ClickedAt = clickedAt,
                ConsumedAt = consumedAt,
                Found = found,
                Trigger = trigger,
                RawEvent = rawEvent,
                Data = deepLink?.Data,
                Utm = deepLink?.Utm,
                BrowserAction = browserAction,
                HandledBySdk = handledBySdk,
            };
        }

        private static AttriaxDeepLinkTrigger BuildDeepLinkTrigger(
            bool isDeferred,
            AttriaxRawDeepLinkEvent? rawEvent)
        {
            if (isDeferred)
            {
                return AttriaxDeepLinkTrigger.Deferred;
            }

            return rawEvent != null && rawEvent.IsInitial
                ? AttriaxDeepLinkTrigger.ColdStart
                : AttriaxDeepLinkTrigger.Foreground;
        }

        private static Uri BuildFallbackDeepLinkUri(
            AttriaxRawDeepLinkEvent? rawEvent,
            string? uri,
            AttriaxDeepLink? deepLink)
        {
            if (deepLink?.Uri != null)
            {
                return deepLink.Uri;
            }

            if (rawEvent != null)
            {
                return rawEvent.Uri;
            }

            if (!string.IsNullOrWhiteSpace(uri) &&
                Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri) &&
                parsedUri != null)
            {
                return parsedUri;
            }

            var normalizedPath = NormalizeDeepLinkPath(uri) ?? NormalizeDeepLinkPath(deepLink?.Path);
            return string.IsNullOrWhiteSpace(normalizedPath)
                ? new Uri("https://attriax.invalid/")
                : new Uri("https://attriax.invalid/" + normalizedPath);
        }

        private static string? NormalizeDeepLinkPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var trimmed = path.Trim().Trim('/');
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static AttriaxAppOpen? ToPublicAppOpen(AttriaxAppOpenResult? result)
        {
            if (result == null)
            {
                return null;
            }

            return new AttriaxAppOpen
            {
                IsNewUser = result.IsNewUser,
                IsFirstLaunch = result.IsFirstLaunch,
                InstallState = result.InstallState,
                DeepLink = result.DeepLink,
                DeepLinkClickedAt = result.DeepLinkClickedAt,
                DeepLinkConsumedAt = result.DeepLinkConsumedAt,
                OriginalInstallReferrer = result.OriginalInstallReferrer,
                ReinstallReferrer = result.ReinstallReferrer,
                Skan = DeepClone(result.Skan),
            };
        }

        private AttriaxDeepLinkEvent? BuildDeepLinkEventFromAppOpenResult(AttriaxAppOpenResult result)
        {
            if (result.DeepLink == null)
            {
                _deepLinkManager.MarkAppOpenUnavailable();
                return null;
            }

            var fallbackUri = BuildFallbackDeepLinkUri(
                null,
                result.DeepLink.Uri != null ? result.DeepLink.Uri.ToString() : null,
                result.DeepLink);
            var clickedAt = result.DeepLinkClickedAt ?? result.AcceptedAt ?? DateTimeOffset.UtcNow;
            var deepLinkEvent = BuildResolvedDeepLinkEvent(
                deepLink: result.DeepLink,
                rawEvent: null,
                clickedAt: clickedAt,
                consumedAt: result.DeepLinkConsumedAt ?? result.AcceptedAt ?? clickedAt,
                trigger: AttriaxDeepLinkTrigger.Deferred,
                browserAction: null,
                handledBySdk: false,
                fallbackUri: fallbackUri,
                found: true);
            _deepLinkManager.HandleAppOpenEvent(deepLinkEvent);
            return deepLinkEvent;
        }

        private void CompleteInitialDeepLink(AttriaxDeepLinkEvent? deepLinkEvent)
        {
            _deepLinkManager.CompleteInitial(deepLinkEvent);
        }

        private void HandleConsentStateChanged()
        {
            if (!_initialized)
            {
                SyncRuntimePersistenceMode();
                return;
            }

            if (ShouldMaterializeIdentifiedContext && string.IsNullOrWhiteSpace(_deviceId))
            {
                if (_identifiedConsentTransitionTask != null)
                {
                    return;
                }

                var transitionTask = HandleConsentStateChangedWithIdentifiedContextAsync();
                _identifiedConsentTransitionTask = transitionTask;
                ObserveBackgroundTask(
                    AwaitIdentifiedConsentTransitionAsync(transitionTask),
                    "Attriax failed to materialize identified context after GDPR consent changed.");
                return;
            }

            _activationCoordinator.HandleConsentStateChanged(_enabled, RuntimeActivationState);
        }

        private async Task AwaitIdentifiedConsentTransitionAsync(Task transitionTask)
        {
            try
            {
                await transitionTask.ConfigureAwait(false);
            }
            finally
            {
                if (ReferenceEquals(_identifiedConsentTransitionTask, transitionTask))
                {
                    _identifiedConsentTransitionTask = null;
                }
            }
        }

        private async Task HandleConsentStateChangedWithIdentifiedContextAsync()
        {
            SyncRuntimePersistenceMode();
            await EnsureIdentifiedContextAsync().ConfigureAwait(false);

            // Parity guard: ensure the current session context is synced with the now-identified
            // device identity so the same sessionId is preserved across the anonymous-to-identified
            // transition. This lets the backend match and merge anonymous activity by sessionId.
            _sessionManager.SyncCurrentSessionContext();

            var updatedSession = _sessionManager.CurrentSession;
            if (updatedSession != null && string.IsNullOrWhiteSpace(updatedSession.DeviceId) &&
                !string.IsNullOrWhiteSpace(_deviceId))
            {
                _sessionManager.HandleSdkEnabled(DateTimeOffset.UtcNow);
            }

            _activationCoordinator.HandleConsentStateChanged(_enabled, RuntimeActivationState);

            if (!_consentManager.IsWaitingForConsent && ShouldTrackSessionActivity)
            {
                var currentSession = _sessionManager.CurrentSession;
                if (currentSession != null)
                {
                    var decision = TrackingDecisionFor(AttriaxTrackingSignal.Session);
                    _ = _requestQueue.Enqueue(
                        AttriaxQueuedRequest.CreateSession(
                            AttriaxGeneratedRequestFactory.BuildTrackSessionRequest(
                                _config.ProjectToken,
                                decision.AttachDeviceIdentity ? _deviceId : null,
                                decision.AttachDeviceIdentity ? RequireDeviceIdSource() : null,
                                currentSession,
                                SdkSessionLifecycleKind.Heartbeat,
                                DateTimeOffset.UtcNow,
                                null)));
                    RequestQueueFlush(true);
                }
            }
        }

        // Pure queued-request consent decisions, lazily built. The captured
        // delegates reference instance members (TrackingDecisionFor,
        // ShouldMaterializeIdentifiedContext) so `this` must already be usable;
        // it is, because the policy is only accessed on the consent-resolution path.
        private AttriaxConsentQueuePolicy ConsentQueuePolicy =>
            _consentQueuePolicy ??= new AttriaxConsentQueuePolicy(
                TrackingDecisionFor,
                () => ShouldMaterializeIdentifiedContext,
                IsAdEventName);

        private void RewriteAndPurgeQueuedRequestsForConsent()
        {
            if (!_config.GdprEnabled || _consentManager.IsWaitingForConsent)
            {
                return;
            }

            var policy = ConsentQueuePolicy;

            _requestQueue.RewriteWhere(
                policy.ShouldIdentifyQueuedRequestForResolvedConsent,
                IdentifyQueuedRequestForResolvedConsent);

            _requestQueue.RewriteWhere(
                policy.ShouldAnonymizeQueuedRequest,
                AnonymizeQueuedRequest);

            _requestQueue.DiscardWhere(
                entry => !policy.IsRequestAllowedByResolvedConsent(entry),
                new AttriaxApiError(
                    "Queued request was dropped because GDPR consent blocked this category.",
                    null,
                    false,
                    true));
        }

        private void IdentifyQueuedRequestForResolvedConsent(AttriaxQueuedRequest entry)
        {
            if (string.IsNullOrWhiteSpace(_deviceId))
            {
                return;
            }

            var deviceIdSource = RequireDeviceIdSource();

            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    entry.RequireEventRequest().deviceId = _deviceId;
                    entry.RequireEventRequest().deviceIdSource = deviceIdSource;
                    break;
                case AttriaxQueuedRequestKind.Crash:
                    entry.RequireCrashRequest().DeviceId = _deviceId;
                    entry.RequireCrashRequest().DeviceIdSource = deviceIdSource;
                    break;
                case AttriaxQueuedRequestKind.Notification:
                    entry.RequireNotificationRequest().deviceId = _deviceId;
                    entry.RequireNotificationRequest().deviceIdSource = deviceIdSource;
                    break;
                case AttriaxQueuedRequestKind.Session:
                    entry.RequireSessionRequest().deviceId = _deviceId;
                    entry.RequireSessionRequest().deviceIdSource = deviceIdSource;
                    break;
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                    entry.RequireDeepLinkResolveRequest().deviceId = _deviceId;
                    entry.RequireDeepLinkResolveRequest().deviceIdSource = deviceIdSource;
                    break;
            }
        }

        private static void AnonymizeQueuedRequest(AttriaxQueuedRequest entry)
        {
            switch (entry.Kind)
            {
                case AttriaxQueuedRequestKind.Event:
                    entry.RequireEventRequest().deviceId = null;
                    entry.RequireEventRequest().deviceIdSource = null;
                    break;
                case AttriaxQueuedRequestKind.Crash:
                    entry.RequireCrashRequest().DeviceId = null;
                    entry.RequireCrashRequest().DeviceIdSource = null;
                    break;
                case AttriaxQueuedRequestKind.Notification:
                    entry.RequireNotificationRequest().deviceId = null;
                    entry.RequireNotificationRequest().deviceIdSource = null;
                    break;
                case AttriaxQueuedRequestKind.Session:
                    entry.RequireSessionRequest().deviceId = null;
                    entry.RequireSessionRequest().deviceIdSource = null;
                    break;
                case AttriaxQueuedRequestKind.DeepLinkResolve:
                    entry.RequireDeepLinkResolveRequest().deviceId = null;
                    entry.RequireDeepLinkResolveRequest().deviceIdSource = null;
                    break;
            }
        }

        private void FailInitialDeepLink(Exception exception)
        {
            _deepLinkManager.FailInitial(exception);
        }

        private void SetSynchronizationState(AttriaxSynchronizationState state)
        {
            _eventHub.SetSynchronizationState(state);
        }

        private bool _cachedOnlineFlag = true;
        private int _lastOnlineCheckTicks;

        private bool IsOnline()
        {
            var nowTicks = Environment.TickCount;
            var lastTicks = Volatile.Read(ref _lastOnlineCheckTicks);
            var elapsed = nowTicks - lastTicks;
            if (elapsed >= 0 && elapsed < 1000)
            {
                return _cachedOnlineFlag;
            }

            _cachedOnlineFlag = AttriaxLifecycleDispatcher.InvokeOnMainThread(
                () => Application.isEditor || Application.internetReachability != NetworkReachability.NotReachable);
            Volatile.Write(ref _lastOnlineCheckTicks, nowTicks);
            return _cachedOnlineFlag;
        }

        private AttriaxPlatformType GetCurrentPlatform()
        {
            return MapPlatform(Application.platform);
        }

        private void AssertInitialized()
        {
            AssertNotDisposed();
            if (!_initialized)
            {
                throw new InvalidOperationException("Attriax.InitializeAsync() must complete before using the SDK.");
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Attriax");
            }
        }

        private void DebugLog(string message, string? detail = null)
        {
            if (!_config.EnableDebugLogs)
            {
                return;
            }

            var prefix = "[Attriax] [" + DateTimeOffset.UtcNow.ToString("O") + "] [thread "
                + System.Threading.Thread.CurrentThread.ManagedThreadId + "] ";

            if (string.IsNullOrWhiteSpace(detail))
            {
                AttriaxLifecycleDispatcher.InvokeOnMainThread(
                    () => UnityEngine.Debug.Log(prefix + message));
                return;
            }

            AttriaxLifecycleDispatcher.InvokeOnMainThread(
                () => UnityEngine.Debug.Log(prefix + message + " " + detail));
        }

        private void DebugLog(string message, Exception exception)
        {
            DebugLog(message, exception != null ? exception.Message : null);
        }

        private static string DescribeTaskStatus(Task? task)
        {
            return task == null ? "null" : task.Status.ToString();
        }

        private static string DescribeQueuedRequest(AttriaxQueuedRequest entry, int queueIndex)
        {
            return "index=" + queueIndex
                + ", id=" + entry.Id
                + ", kind=" + entry.Kind
                + ", attempt=" + entry.AttemptCount
                + ", nextRetryAt=" + (entry.NextRetryAt.HasValue ? entry.NextRetryAt.Value.ToString("O") : "null")
                + ", lastHttpStatusCode=" + (entry.LastHttpStatusCode.HasValue ? entry.LastHttpStatusCode.Value.ToString() : "null");
        }

        private static string DescribeInstallReferrerContext(AttriaxInstallReferrerContextPayload context)
        {
            var metadata = context.Metadata;
            return "hasReferrer=" + (!string.IsNullOrWhiteSpace(context.InstallReferrer))
                + ", status=" + (ReadString(metadata, "installReferrerStatus") ?? "null")
                + ", attempts=" + (ReadInt64(metadata, "installReferrerAttempts")?.ToString() ?? "null")
                + ", clickTs=" + (context.ReferrerClickTimestampSeconds?.ToString() ?? "null")
                + ", installTs=" + (context.InstallBeginTimestampSeconds?.ToString() ?? "null");
        }

        private sealed class PreparedContext
        {
            public AttriaxContextSnapshot InitialSnapshot;
            public ResolvedDeviceId DeviceId;
            public Task<AttriaxContextSnapshot> ResolvedSnapshotTask;
        }

        private sealed class PreparedBatchEntries
        {
            public PreparedBatchEntries(
                IReadOnlyList<AttriaxQueuedRequest> queuedEntries,
                IReadOnlyList<AttriaxQueuedRequest> transportEntries,
                string? keepAliveSessionId,
                DateTimeOffset? keepAliveOccurredAt)
            {
                QueuedEntries = queuedEntries;
                TransportEntries = transportEntries;
                KeepAliveSessionId = keepAliveSessionId;
                KeepAliveOccurredAt = keepAliveOccurredAt;
            }

            public IReadOnlyList<AttriaxQueuedRequest> QueuedEntries { get; }

            public IReadOnlyList<AttriaxQueuedRequest> TransportEntries { get; }

            public string? KeepAliveSessionId { get; }

            public DateTimeOffset? KeepAliveOccurredAt { get; }
        }

        private readonly struct BatchFlushResult
        {
            public BatchFlushResult(bool shouldStop, int remainingCount)
            {
                ShouldStop = shouldStop;
                RemainingCount = remainingCount;
            }

            public bool ShouldStop { get; }

            public int RemainingCount { get; }
        }

        private sealed class ResolvedDeviceId
        {
            public string Value;
            public string Source;
            public bool IsFallback;
        }

        private sealed class StoredDeviceIdState
        {
            public string Value;
            public bool HasPersistedValue;
        }

        private sealed class NormalizedConfig
        {
            public string ProjectToken;
            public string ApiBaseUrl;
            public string SdkBaseUrl;
            public string AppVersion;
            public string AppBuildNumber;
            public string AppPackageName;
            public Dictionary<string, object> SdkMetadata;
            public bool EnableDebugLogs;
            public bool GdprEnabled;
            public bool AnonymousTracking;
            public bool CollectAdvertisingId;
            public bool AutomaticCrashReportingEnabled;
            public bool RequestTrackingAuthorizationOnInit;
            public int TrackingAuthorizationStatusTimeoutMs;
            public bool SessionTrackingEnabled;
            public bool AutomaticSceneTracking;
            public bool AutomaticBrowserHandling;
            public int SessionHeartbeatIntervalMs;
            public int FirstLaunchSessionHeartbeatIntervalMs;
            public int EventFlushIntervalMs;
            public bool FlushEventsImmediatelyOnFirstLaunch;
            public int RequestTimeoutMs;
            public int MaxQueueSize;
            public string StorageKeyPrefix;
            public AttriaxSkanConfig? Skan;

            public AttriaxConfig ToPublic()
            {
                return new AttriaxConfig
                {
                    ProjectToken = ProjectToken,
                    ApiBaseUrl = ApiBaseUrl,
                    AppVersion = AppVersion,
                    AppBuildNumber = AppBuildNumber,
                    AppPackageName = AppPackageName,
                    SdkMetadata = new Dictionary<string, object>(SdkMetadata),
                    EnableDebugLogs = EnableDebugLogs,
                    GdprEnabled = GdprEnabled,
                    AnonymousTracking = AnonymousTracking,
                    CollectAdvertisingId = CollectAdvertisingId,
                    AutomaticCrashReportingEnabled = AutomaticCrashReportingEnabled,
                    RequestTrackingAuthorizationOnInit = RequestTrackingAuthorizationOnInit,
                    TrackingAuthorizationStatusTimeoutMs = TrackingAuthorizationStatusTimeoutMs,
                    SessionTrackingEnabled = SessionTrackingEnabled,
                    AutomaticSceneTracking = AutomaticSceneTracking,
                    AutomaticBrowserHandling = AutomaticBrowserHandling,
                    SessionHeartbeatIntervalMs = SessionHeartbeatIntervalMs,
                    FirstLaunchSessionHeartbeatIntervalMs = FirstLaunchSessionHeartbeatIntervalMs,
                    EventFlushIntervalMs = EventFlushIntervalMs,
                    FlushEventsImmediatelyOnFirstLaunch = FlushEventsImmediatelyOnFirstLaunch,
                    RequestTimeoutMs = RequestTimeoutMs,
                    MaxQueueSize = MaxQueueSize,
                    StorageKeyPrefix = StorageKeyPrefix,
                    Skan = DeepClone(Skan),
                };
            }
        }

    }
}
