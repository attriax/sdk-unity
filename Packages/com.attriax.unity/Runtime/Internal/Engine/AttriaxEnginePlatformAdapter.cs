#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attriax.Unity;
using UnityEngine;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Bridges any native <see cref="IAttriaxEnginePlatform"/> engine onto the
    /// facade-facing <see cref="IAttriaxEngine"/> surface, so
    /// <see cref="AttriaxEngineSelector"/> can return a native engine on a
    /// per-platform basis without changing a single facade.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the C# twin of the Flutter <c>AttriaxNativeRuntime</c>
    /// (<c>attriax_native_runtime.dart</c>). The native engine owns the
    /// authoritative state (identity, queue, consent, sessions, sync); this class
    /// holds no engine logic — only the small caches the facade's <b>synchronous</b>
    /// getters need. It performs the four bridging jobs required by the impedance
    /// mismatch between the two surfaces:
    /// </para>
    /// <list type="number">
    /// <item><b>Lowering</b> — rich option objects
    /// (<see cref="AttriaxTrackEventOptions"/> and siblings) are decomposed into the
    /// platform's primitive arguments on every command; the native engine performs
    /// the reserved-key lowering + currency normalization.</item>
    /// <item><b>Sync-getter caching</b> — the platform exposes all-async getters, but
    /// <see cref="IAttriaxEngine"/> requires synchronous property getters
    /// (<see cref="DeviceId"/>, <see cref="GdprConsentState"/>, the deep-link
    /// snapshot, …). The cache is seeded once from the async getters at
    /// <see cref="InitializeAsync"/> and kept fresh from the platform's event
    /// streams and from the app's own consent/toggle calls.</item>
    /// <item><b>Event bridging</b> — the platform's C# <see cref="Action{T}"/> events
    /// are re-broadcast through the facade's <c>Subscribe*</c> handles, updating the
    /// caches as they fire.</item>
    /// <item><b>Lifecycle mapping</b> — init / reset / dispose map onto the platform,
    /// and the platform-event subscriptions are torn down on <see cref="Dispose"/>.</item>
    /// </list>
    /// <para>
    /// Reads that a binding may not implement degrade to the same benign default the
    /// managed C# engine would return rather than throwing into app code, mirroring
    /// the Flutter bridge's <c>_readOr</c> / <c>_fireAndForget</c> contract.
    /// </para>
    /// </remarks>
    internal sealed class AttriaxEnginePlatformAdapter : IAttriaxEngine
    {
        private readonly AttriaxConfig _config;
        private readonly IAttriaxEnginePlatform _platform;

        private readonly Broadcast<AttriaxSynchronizationState> _syncBroadcast = new Broadcast<AttriaxSynchronizationState>();
        private readonly Broadcast<AttriaxDeepLinkEvent> _deepLinkBroadcast = new Broadcast<AttriaxDeepLinkEvent>();
        private readonly Broadcast<AttriaxRawDeepLinkEvent> _rawDeepLinkBroadcast = new Broadcast<AttriaxRawDeepLinkEvent>();

        private readonly TaskCompletionSource<bool> _initialLinkProbe =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // --- cached synchronous state (seeded on init, refreshed from events) ---
        private bool _initialized;
        private bool _disposed;
        private bool _sdkEnabled = true;
        private bool _eventsEnabled = true;
        private bool _anonymousTracking = true;
        private bool _isFirstLaunch;
        private string? _deviceId;
        private AttriaxSdkSnapshot? _sdkSnapshot;
        private AttriaxSynchronizationState _synchronizationState = AttriaxSynchronizationState.Initializing;
        private AttriaxSkanState? _skanState;
        private AttriaxDeepLinkEvent? _latestDeepLink;
        private AttriaxDeepLinkEvent? _initialDeepLink;
        private AttriaxRawDeepLinkEvent? _rawInitialDeepLink;
        private bool _initialDeepLinkResolved;
        private AttriaxGdprConsentState _gdprConsentState = AttriaxGdprConsentState.Unknown;
        private AttriaxGdprConsentValues? _gdprConsentValues;
        private bool _isWaitingForGdprConsent;

        public AttriaxEnginePlatformAdapter(AttriaxConfig config, IAttriaxEnginePlatform platform)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _platform = platform ?? throw new ArgumentNullException(nameof(platform));
            _anonymousTracking = config.AnonymousTracking;

            _platform.SynchronizationStateChanged += OnPlatformSyncState;
            _platform.DeepLinkResolved += OnPlatformDeepLink;
            _platform.RawDeepLinkReceived += OnPlatformRawDeepLink;
            _platform.InitialDeepLinkResolved += OnPlatformInitialDeepLink;
        }

        // ------------------------------------------------------------------
        // Lifecycle / instance state
        // ------------------------------------------------------------------

        public AttriaxConfig Config => _config;

        public bool IsInitialized => _initialized;

        public bool Enabled => _sdkEnabled;

        public bool IsFirstLaunch => _isFirstLaunch;

        public string? DeviceId => _deviceId;

        public AttriaxSdkSnapshot? SdkSnapshot => _sdkSnapshot;

        public void SetEnabled(bool enabled)
        {
            _sdkEnabled = enabled;
            _ = FireAndForget(() => _platform.SetSdkEnabled(enabled), "setSdkEnabled");
        }

        public async Task InitializeAsync(AttriaxInitOptions options)
        {
            AssertNotDisposed();
            if (_initialized)
            {
                return;
            }

            await _platform.InitializeAsync(_config).ConfigureAwait(false);
            _initialized = true;
            await SeedCachedStateAsync().ConfigureAwait(false);
        }

        public async Task ResetAsync()
        {
            AssertNotDisposed();
            await _platform.Reset().ConfigureAwait(false);

            _initialized = false;
            _deviceId = null;
            _sdkSnapshot = null;
            _latestDeepLink = null;
            _initialDeepLink = null;
            _rawInitialDeepLink = null;
            _initialDeepLinkResolved = false;
            _synchronizationState = AttriaxSynchronizationState.Initializing;
            _gdprConsentState = AttriaxGdprConsentState.Unknown;
            _gdprConsentValues = null;
        }

        public async Task<AttriaxRevenueReceiptValidationResult> ValidateReceiptAsync(
            AttriaxValidateReceiptOptions options)
        {
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

            return await _platform.ValidateReceipt(
                    options.Receipt,
                    options.Test ?? false,
                    options.Provider,
                    options.Environment,
                    options.ProductId,
                    options.TransactionId)
                .ConfigureAwait(false);
        }

        // ------------------------------------------------------------------
        // Tracking: toggles
        // ------------------------------------------------------------------

        public bool EventsEnabled
        {
            get => _eventsEnabled;
            set
            {
                _eventsEnabled = value;
                _ = FireAndForget(() => _platform.SetEventTrackingEnabled(value), "setEventTrackingEnabled");
            }
        }

        public bool AnonymousTrackingEnabled
        {
            get => _anonymousTracking;
            set
            {
                _anonymousTracking = value;
                _ = FireAndForget(() => _platform.SetAnonymousTracking(value), "setAnonymousTracking");
            }
        }

        // ------------------------------------------------------------------
        // Tracking: events / revenue / notifications / errors
        // ------------------------------------------------------------------

        public Task RecordEventAsync(string eventName, AttriaxTrackEventOptions options)
        {
            return _platform.RecordEvent(
                eventName,
                options.EventData,
                options.FlushImmediately ?? false);
        }

        public Task RecordPageViewAsync(string pageName, AttriaxPageViewOptions options)
        {
            return _platform.RecordPageView(
                pageName,
                options.PageClass,
                options.PageTitle,
                options.PreviousPageName,
                options.Parameters,
                string.IsNullOrWhiteSpace(options.Source) ? "manual" : options.Source,
                options.FlushImmediately ?? false);
        }

        public Task RecordPurchaseAsync(double revenue, AttriaxRecordPurchaseOptions options)
        {
            AssertFiniteRevenue(revenue);
            if (options.Quantity <= 0)
            {
                throw new ArgumentException("quantity must be positive.", nameof(options));
            }

            return _platform.RecordPurchase(
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
                options.FlushImmediately ?? true);
        }

        public Task RecordRefundAsync(double revenue, AttriaxRecordRefundOptions options)
        {
            AssertFiniteRevenue(revenue);
            if (options.Quantity <= 0)
            {
                throw new ArgumentException("quantity must be positive.", nameof(options));
            }

            return _platform.RecordRefund(
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
                options.Metadata,
                options.FlushImmediately ?? true);
        }

        public Task RecordAdRevenueAsync(double revenue, AttriaxRecordAdRevenueOptions options)
        {
            AssertFiniteRevenue(revenue);

            return _platform.RecordAdRevenue(
                revenue,
                options.Currency,
                options.RevenueInMicros,
                options.AdNetwork,
                options.AdFormat,
                options.AdType,
                options.AdPlacement,
                options.Test,
                options.Metadata,
                options.FlushImmediately ?? true);
        }

        public Task RecordAdEventAsync(AttriaxAdEventType type, AttriaxRecordAdEventOptions options)
        {
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

            return _platform.RecordAdEvent(
                AdEventTypeToEventName(type),
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
                options.Metadata,
                options.FlushImmediately ?? true);
        }

        public Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions options)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var exceptionType = string.IsNullOrWhiteSpace(error.GetType().Name)
                ? "Exception"
                : error.GetType().Name;
            var message = string.IsNullOrWhiteSpace(error.Message) ? error.ToString() : error.Message;
            var stackTrace = string.IsNullOrWhiteSpace(error.StackTrace) ? error.ToString() : error.StackTrace;

            return _platform.RecordError(
                message,
                exceptionType,
                stackTrace,
                options.IsFatal,
                string.IsNullOrWhiteSpace(options.Source) ? "manual" : options.Source,
                options.Reason,
                options.Metadata);
        }

        public Task RecordNotificationAsync(
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxRecordNotificationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return _platform.RecordNotification(
                NotificationTypeToWire(type),
                notificationId,
                options.LinkId,
                options.CampaignId,
                options.Title,
                NotificationSourceToWire(options.Source),
                options.Payload,
                options.Metadata,
                options.FlushImmediately ?? false);
        }

        // ------------------------------------------------------------------
        // Tracking: identity / user properties / push tokens
        // ------------------------------------------------------------------

        public async Task SetUserAsync(string? userId, AttriaxSetUserOptions options)
        {
            await _platform.SetUser(userId, options.UserName).ConfigureAwait(false);

            if (options.ClearAllProperties)
            {
                await _platform.ClearUserProperties(null).ConfigureAwait(false);
            }
            else if (options.ClearPropertyKeys != null && options.ClearPropertyKeys.Count > 0)
            {
                await _platform.ClearUserProperties(options.ClearPropertyKeys.ToList()).ConfigureAwait(false);
            }

            if (options.Properties != null && options.Properties.Count > 0)
            {
                await _platform.SetUserProperties(options.Properties).ConfigureAwait(false);
            }
        }

        public Task IdentifyAsync(string? userId, AttriaxIdentifyOptions options)
        {
            return SetUserAsync(userId, options);
        }

        public Task SetUserPropertyAsync(string name, object? value)
        {
            return _platform.SetUserProperty(name, value);
        }

        public Task SetUserPropertiesAsync(IDictionary<string, object?> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            return _platform.SetUserProperties(ToNonNullValueMap(properties));
        }

        public Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null)
        {
            return _platform.ClearUserProperties(propertyNames?.ToList());
        }

        public Task RegisterFirebaseMessagingTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            return _platform.RegisterPushToken(AttriaxPushTokenProvider.Fcm, token, metadata);
        }

        public Task RegisterApplePushTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            return _platform.RegisterPushToken(AttriaxPushTokenProvider.Apns, token, metadata);
        }

        // ------------------------------------------------------------------
        // Consent: ATT
        // ------------------------------------------------------------------

        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(int? timeoutMs = null)
        {
            return ReadOrAsync(
                () => _platform.RequestTrackingAuthorization(timeoutMs),
                AttriaxTrackingAuthorizationStatus.NotSupported);
        }

        public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync()
        {
            return ReadOrAsync(
                () => _platform.GetTrackingAuthorizationStatus(),
                AttriaxTrackingAuthorizationStatus.NotSupported);
        }

        // ------------------------------------------------------------------
        // Consent: GDPR (reader state tracked locally, mirroring Flutter)
        // ------------------------------------------------------------------

        public AttriaxGdprConsentState GdprConsentState => _gdprConsentState;

        public AttriaxGdprConsentValues? GdprConsentValues => _gdprConsentValues;

        public bool IsWaitingForGdprConsent => _isWaitingForGdprConsent;

        public Task<bool> NeedsGdprConsentAsync(bool localOnly = false)
        {
            return ReadOrAsync(() => _platform.NeedsGdprConsent(localOnly), false);
        }

        public void SetGdprConsent(bool analytics, bool attribution, bool adEvents)
        {
            _gdprConsentState = AttriaxGdprConsentState.Granted;
            _gdprConsentValues = new AttriaxGdprConsentValues
            {
                Analytics = analytics,
                Attribution = attribution,
                AdEvents = adEvents,
            };
            _isWaitingForGdprConsent = false;
            _ = FireAndForget(() => _platform.SetGdprConsent(analytics, attribution, adEvents), "setGdprConsent");
        }

        public void SetGdprConsentNotRequired()
        {
            _gdprConsentState = AttriaxGdprConsentState.NotRequired;
            _isWaitingForGdprConsent = false;
            _ = FireAndForget(() => _platform.SetGdprConsentNotRequired(), "setGdprConsentNotRequired");
        }

        public void ResetGdprConsent()
        {
            _gdprConsentState = AttriaxGdprConsentState.Unknown;
            _gdprConsentValues = null;
            _ = FireAndForget(() => _platform.ResetGdprConsent(), "resetGdprConsent");
        }

        public async Task RequestGdprDataErasureAsync()
        {
            await _platform.RequestGdprDataErasure().ConfigureAwait(false);
            _gdprConsentState = AttriaxGdprConsentState.Unknown;
            _gdprConsentValues = null;
        }

        // ------------------------------------------------------------------
        // Deep links
        // ------------------------------------------------------------------

        public AttriaxRawDeepLinkEvent? RawInitialDeepLinkValue => _rawInitialDeepLink;

        public AttriaxDeepLinkEvent? InitialDeepLinkValue => _initialDeepLink;

        public bool InitialDeepLinkResolved => _initialDeepLinkResolved;

        public Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink => WaitForInitialDeepLinkAsync();

        public AttriaxDeepLinkEvent? LatestDeepLink => _latestDeepLink;

        public IDisposable SubscribeToRawDeepLinks(Action<AttriaxRawDeepLinkEvent> listener)
        {
            return _rawDeepLinkBroadcast.Subscribe(listener);
        }

        public IDisposable SubscribeToDeepLinks(Action<AttriaxDeepLinkEvent> listener)
        {
            return _deepLinkBroadcast.Subscribe(listener);
        }

        public async Task<AttriaxDeepLinkEvent> WaitForDeepLinkResolutionAsync(AttriaxRawDeepLinkEvent rawEvent)
        {
            var resolved = await ReadOrAsync(
                () => _platform.WaitForDeepLinkResolution(rawEvent),
                (AttriaxDeepLinkEvent?)null).ConfigureAwait(false);
            if (resolved != null)
            {
                return resolved;
            }

            // Bindings that resolve asynchronously surface the resolution on the
            // deep-link event stream; fall back to the next resolved event.
            return await NextDeepLinkAsync().ConfigureAwait(false);
        }

        public Task<AttriaxCreateDynamicLinkResult> CreateDynamicLinkAsync(AttriaxCreateDynamicLinkOptions options)
        {
            return _platform.CreateDynamicLink(options);
        }

        public Task<AttriaxDeepLinkEvent?> RecordDeepLinkConversionAsync(AttriaxDeepLinkConversionOptions options)
        {
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

            return _platform.RecordDeepLink(
                new Uri(options.Uri, UriKind.RelativeOrAbsolute),
                options.Metadata,
                string.IsNullOrWhiteSpace(options.Source) ? "manual" : options.Source!);
        }

        // ------------------------------------------------------------------
        // Referrer (null when unavailable; honors initialized/enabled)
        // ------------------------------------------------------------------

        public Task<AttriaxInstallReferrerDetails?> OriginalInstallReferrer =>
            ReadReferrerAsync(() => _platform.GetOriginalInstallReferrer());

        public Task<AttriaxInstallReferrerDetails?> ReinstallReferrer =>
            ReadReferrerAsync(() => _platform.GetReinstallReferrer());

        public Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync()
        {
            return ReadReferrerAsync(() => _platform.GetSessionReferrer());
        }

        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync()
        {
            return ReadReferrerAsync(() => _platform.GetLatestDeepLinkReferrer());
        }

        // ------------------------------------------------------------------
        // SKAN
        // ------------------------------------------------------------------

        public AttriaxSkanState? SkanState => _skanState;

        public async Task<AttriaxSkanUpdateResult> UpdateSkanConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
            var result = await _platform.UpdateSkanConversionValue(fineValue, coarseValue, lockWindow)
                .ConfigureAwait(false);
            _skanState = await ReadOrAsync(() => _platform.GetSkanState(), _skanState).ConfigureAwait(false);
            return result;
        }

        // ------------------------------------------------------------------
        // Synchronization
        // ------------------------------------------------------------------

        public AttriaxSynchronizationState SynchronizationState => _synchronizationState;

        public IDisposable SubscribeToSynchronization(Action<AttriaxSynchronizationState> listener)
        {
            return _syncBroadcast.Subscribe(listener);
        }

        // ------------------------------------------------------------------
        // Disposal
        // ------------------------------------------------------------------

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _platform.SynchronizationStateChanged -= OnPlatformSyncState;
            _platform.DeepLinkResolved -= OnPlatformDeepLink;
            _platform.RawDeepLinkReceived -= OnPlatformRawDeepLink;
            _platform.InitialDeepLinkResolved -= OnPlatformInitialDeepLink;
            _syncBroadcast.Clear();
            _deepLinkBroadcast.Clear();
            _rawDeepLinkBroadcast.Clear();
            if (!_initialLinkProbe.Task.IsCompleted)
            {
                _initialLinkProbe.TrySetResult(false);
            }

            _ = FireAndForget(() => _platform.Dispose(), "dispose");
        }

        // ------------------------------------------------------------------
        // Platform-event bridging (updates caches + re-broadcasts)
        // ------------------------------------------------------------------

        private void OnPlatformSyncState(AttriaxSynchronizationState state)
        {
            _synchronizationState = state;
            _syncBroadcast.Raise(state);
        }

        private void OnPlatformDeepLink(AttriaxDeepLinkEvent deepLink)
        {
            _latestDeepLink = deepLink;
            _deepLinkBroadcast.Raise(deepLink);
        }

        private void OnPlatformRawDeepLink(AttriaxRawDeepLinkEvent rawEvent)
        {
            if (rawEvent.IsInitial)
            {
                _rawInitialDeepLink = rawEvent;
            }

            _rawDeepLinkBroadcast.Raise(rawEvent);
        }

        private void OnPlatformInitialDeepLink(AttriaxInitialDeepLinkResolution resolution)
        {
            _initialDeepLinkResolved = resolution.Resolved;
            if (resolution.DeepLink != null)
            {
                _initialDeepLink = resolution.DeepLink;
            }

            _initialLinkProbe.TrySetResult(true);
        }

        // ------------------------------------------------------------------
        // Seeding + helpers
        // ------------------------------------------------------------------

        private async Task SeedCachedStateAsync()
        {
            _deviceId = await ReadOrAsync(() => _platform.GetDeviceId(), _deviceId).ConfigureAwait(false);
            _isFirstLaunch = await ReadOrAsync(() => _platform.GetIsFirstLaunch(), _isFirstLaunch).ConfigureAwait(false);
            _sdkSnapshot = await ReadOrAsync(() => _platform.GetSdkSnapshot(), _sdkSnapshot).ConfigureAwait(false);
            _sdkEnabled = await ReadOrAsync(() => _platform.GetSdkEnabled(), _sdkEnabled).ConfigureAwait(false);
            _eventsEnabled = await ReadOrAsync(() => _platform.GetEventTrackingEnabled(), _eventsEnabled).ConfigureAwait(false);
            _anonymousTracking = await ReadOrAsync(() => _platform.GetAnonymousTracking(), _anonymousTracking).ConfigureAwait(false);
            _skanState = await ReadOrAsync(() => _platform.GetSkanState(), _skanState).ConfigureAwait(false);
            _isWaitingForGdprConsent = await ReadOrAsync(
                () => _platform.GetIsWaitingForGdprConsent(),
                _isWaitingForGdprConsent).ConfigureAwait(false);
            _latestDeepLink = await ReadOrAsync(() => _platform.GetLatestDeepLink(), _latestDeepLink).ConfigureAwait(false);
            _initialDeepLink = await ReadOrAsync(() => _platform.GetInitialDeepLink(), _initialDeepLink).ConfigureAwait(false);
            _rawInitialDeepLink = await ReadOrAsync(() => _platform.GetRawInitialDeepLink(), _rawInitialDeepLink).ConfigureAwait(false);
            _initialDeepLinkResolved = await ReadOrAsync(
                () => _platform.GetIsInitialDeepLinkResolved(),
                _initialDeepLinkResolved).ConfigureAwait(false);
            _synchronizationState = await ReadOrAsync(
                () => _platform.GetSynchronizationState(),
                _synchronizationState).ConfigureAwait(false);
        }

        private async Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLinkAsync()
        {
            if (!_initialDeepLinkResolved && !_initialLinkProbe.Task.IsCompleted)
            {
                var completed = await Task.WhenAny(
                    _initialLinkProbe.Task,
                    Task.Delay(TimeSpan.FromSeconds(10))).ConfigureAwait(false);
                if (completed != _initialLinkProbe.Task)
                {
                    // Timed out: fall through with whatever has been cached so far.
                }
            }

            return _initialDeepLink;
        }

        private Task<AttriaxDeepLinkEvent> NextDeepLinkAsync()
        {
            var tcs = new TaskCompletionSource<AttriaxDeepLinkEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
            IDisposable? subscription = null;
            subscription = _deepLinkBroadcast.Subscribe(deepLink =>
            {
                subscription?.Dispose();
                tcs.TrySetResult(deepLink);
            });
            return tcs.Task;
        }

        /// <summary>
        /// Reads a value from the native engine, returning <paramref name="fallback"/>
        /// when the binding has not implemented that call (or it otherwise fails).
        /// </summary>
        private async Task<T> ReadOrAsync<T>(Func<Task<T>> read, T fallback)
        {
            try
            {
                return await read().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log($"native read fell back to default: {exception.Message}");
                return fallback;
            }
        }

        /// <summary>
        /// Mirrors the managed engine's referrer contract: <c>null</c> before init or
        /// while disabled; degrades any binding failure to <c>null</c>.
        /// </summary>
        private async Task<T?> ReadReferrerAsync<T>(Func<Task<T?>> reader)
            where T : class
        {
            if (!_initialized || !_sdkEnabled)
            {
                return null;
            }

            try
            {
                return await reader().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log($"native referrer read degraded to null: {exception.Message}");
                return null;
            }
        }

        /// <summary>
        /// Invokes a fire-and-forget native command, logging (never throwing) when the
        /// binding has not implemented it.
        /// </summary>
        private async Task FireAndForget(Func<Task> command, string label)
        {
            try
            {
                await command().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log($"native command \"{label}\" ignored: {exception.Message}");
            }
        }

        private void Log(string message)
        {
            if (_config.EnableDebugLogs)
            {
                Debug.Log($"[Attriax][Engine] {message}");
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AttriaxEnginePlatformAdapter));
            }
        }

        private static void AssertFiniteRevenue(double revenue)
        {
            if (double.IsInfinity(revenue) || double.IsNaN(revenue))
            {
                throw new ArgumentException("revenue must be finite.", nameof(revenue));
            }
        }

        private static IDictionary<string, object> ToNonNullValueMap(IDictionary<string, object?> properties)
        {
            var map = new Dictionary<string, object>(properties.Count);
            foreach (var pair in properties)
            {
                // The native engine interprets a null value as "clear this key";
                // Dictionary<string, object> holds nulls fine at runtime.
                map[pair.Key] = pair.Value!;
            }

            return map;
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

        private static string NotificationTypeToWire(AttriaxNotificationEventType type)
        {
            switch (type)
            {
                case AttriaxNotificationEventType.Received: return "received";
                case AttriaxNotificationEventType.Opened: return "opened";
                case AttriaxNotificationEventType.Dismissed: return "dismissed";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown AttriaxNotificationEventType.");
            }
        }

        private static string? NotificationSourceToWire(AttriaxNotificationEventSource? source)
        {
            switch (source)
            {
                case null: return null;
                case AttriaxNotificationEventSource.Fcm: return "fcm";
                case AttriaxNotificationEventSource.Apns: return "apns";
                case AttriaxNotificationEventSource.Other: return "other";
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown AttriaxNotificationEventSource.");
            }
        }

        /// <summary>
        /// A minimal multicast fan-out backing the facade <c>Subscribe*</c> handles.
        /// Each subscription returns an <see cref="IDisposable"/> that detaches the
        /// listener, mirroring the managed engine's <c>AttriaxEventHub</c>.
        /// </summary>
        private sealed class Broadcast<T>
        {
            private readonly object _gate = new object();
            private Action<T>? _handlers;

            public IDisposable Subscribe(Action<T> listener)
            {
                if (listener == null)
                {
                    throw new ArgumentNullException(nameof(listener));
                }

                lock (_gate)
                {
                    _handlers += listener;
                }

                return new Subscription(() =>
                {
                    lock (_gate)
                    {
                        _handlers -= listener;
                    }
                });
            }

            public void Raise(T value)
            {
                Action<T>? handlers;
                lock (_gate)
                {
                    handlers = _handlers;
                }

                handlers?.Invoke(value);
            }

            public void Clear()
            {
                lock (_gate)
                {
                    _handlers = null;
                }
            }

            private sealed class Subscription : IDisposable
            {
                private Action? _dispose;

                public Subscription(Action dispose)
                {
                    _dispose = dispose;
                }

                public void Dispose()
                {
                    var dispose = _dispose;
                    _dispose = null;
                    dispose?.Invoke();
                }
            }
        }
    }
}
