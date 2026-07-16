#nullable enable
#if UNITY_WEBGL && !UNITY_EDITOR
#define ATTRIAX_WEBGL_ENGINE
#endif
#if ATTRIAX_WEBGL_ENGINE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using Attriax.Unity.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Unity <b>WebGL</b> <see cref="IAttriaxEnginePlatform"/> implementation. Drives
    /// the <c>@attriax/js</c> engine (sdk-js) — Attriax's reference browser identity
    /// implementation — through a <c>.jslib</c> bridge
    /// (<c>Runtime/Plugins/WebGL/AttriaxWebGL.jslib</c>), the exact engine the Flutter
    /// web binding drives (<c>AttriaxWeb</c> over <c>dart:js_interop</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the WebGL twin of <see cref="AttriaxDesktopEnginePlatform"/>. Both
    /// speak the same <c>{"ok":true,"value":…}</c> / <c>{"ok":false,"error":…}</c>
    /// dispatch envelope and re-raise engine events on the Unity main thread via
    /// <see cref="AttriaxLifecycleDispatcher.PostToMainThread"/>; only the transport
    /// differs. The desktop binding blocks a worker thread on a <c>runBlocking</c>
    /// C-ABI; the browser is single-threaded and every sdk-js command is a
    /// <c>Promise</c>, so this binding is <b>fully asynchronous</b>: each command
    /// tags an outbound request with a monotonically increasing id, parks a
    /// <see cref="TaskCompletionSource{TResult}"/> in <see cref="PendingResults"/>,
    /// and completes it when the <c>.jslib</c> resolves the promise and calls back
    /// through the registered result trampoline. Engine events (synchronization
    /// state, resolved / raw / initial deep links) arrive on a second registered
    /// trampoline and are routed to the owning instance by its JS handle.
    /// </para>
    /// <para>
    /// The two static trampolines are IL2CPP-safe (<see cref="MonoPInvokeCallbackAttribute"/>
    /// statics kept alive in static fields) and are registered once, process-wide;
    /// result routing is keyed by the global request id and event routing by the
    /// per-instance JS handle, so multiple <c>Attriax</c> instances coexist.
    /// </para>
    /// <para>
    /// Members sdk-js genuinely does not surface on the web degrade to the same
    /// benign no-op / default the Flutter web binding chose (<c>attriax_web.dart</c>),
    /// rather than throwing into app code: <see cref="SetCcpaConsent"/> /
    /// <see cref="GetDoNotSell"/> / <see cref="GetUsPrivacy"/> (no CCPA surface),
    /// <see cref="RegisterPushToken"/> (mobile-only), <see cref="RequestGdprDataErasure"/>,
    /// <see cref="CompleteInitialDeepLink"/> (sdk-js resolves the launch URL itself),
    /// <see cref="GetRawInstallReferrer"/> (no platform install-referrer on the web),
    /// and the Apple seams (<see cref="SubmitAsaToken"/> /
    /// <see cref="SetTrackingAuthorizationStatus"/> / ATT / SKAN).
    /// </para>
    /// </remarks>
    internal sealed class AttriaxWebGLEnginePlatform : IAttriaxEnginePlatform
    {
        // -----------------------------------------------------------------
        // .jslib bridge (Runtime/Plugins/WebGL/AttriaxWebGL.jslib).
        // -----------------------------------------------------------------

        [DllImport("__Internal")]
        private static extern int AttriaxWebGL_Create(string configJson);

        [DllImport("__Internal")]
        private static extern void AttriaxWebGL_RegisterCallbacks(IntPtr resultCallback, IntPtr eventCallback);

        [DllImport("__Internal")]
        private static extern void AttriaxWebGL_Dispatch(int handle, int requestId, string method, string argsJson);

        [DllImport("__Internal")]
        private static extern void AttriaxWebGL_Destroy(int handle);

        // -----------------------------------------------------------------
        // Static callback trampolines (IL2CPP-safe: static + kept alive) and the
        // routing registries shared across every instance.
        // -----------------------------------------------------------------

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ResultCallbackNative(int requestId, IntPtr resultJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallbackNative(int handle, IntPtr eventJson);

        private static readonly ResultCallbackNative ResultCallbackDelegate = OnDispatchResultStatic;
        private static readonly EventCallbackNative EventCallbackDelegate = OnNativeEventStatic;

        private static readonly ConcurrentDictionary<int, TaskCompletionSource<string?>> PendingResults =
            new ConcurrentDictionary<int, TaskCompletionSource<string?>>();
        private static readonly Dictionary<int, AttriaxWebGLEnginePlatform> Instances =
            new Dictionary<int, AttriaxWebGLEnginePlatform>();
        private static readonly object InstancesGate = new object();
        private static readonly object RegistrationGate = new object();

        private static int _nextRequestId;
        private static bool _callbacksRegistered;

        private int _handle;

        public event Action<AttriaxSynchronizationState> SynchronizationStateChanged = delegate { };
        public event Action<AttriaxDeepLinkEvent> DeepLinkResolved = delegate { };
        public event Action<AttriaxRawDeepLinkEvent> RawDeepLinkReceived = delegate { };
        public event Action<AttriaxInitialDeepLinkResolution> InitialDeepLinkResolved = delegate { };

        // -----------------------------------------------------------------
        // Lifecycle.
        // -----------------------------------------------------------------

        public Task InitializeAsync(AttriaxConfig config)
        {
            EnsureCallbacksRegistered();

            var engineArguments = config.ToEngineArguments();
            // sdk-js (unlike the KMP engines) ships its own URL-based automatic page
            // tracking and defaults it to ON. Page tracking is a Unity wrapper concern
            // (AttriaxSceneTracker emits `automatic_scene` page views), so the engine's
            // tracker must be disabled here or every WebGL session double-tracks: one
            // `automatic_page_tracking` page_view for the hosting URL plus the scene
            // ones. WebGL-only injection — the shared ToEngineArguments stays 1:1 with
            // the KMP config surface, which has no such key.
            engineArguments["automaticPageTracking"] = false;
            var configJson = JsonConvert.SerializeObject(engineArguments);
            var handle = AttriaxWebGL_Create(configJson);
            if (handle == 0)
            {
                throw new InvalidOperationException(
                    "AttriaxWebGL_Create returned a null handle. The @attriax/js bundle " +
                    "(globalThis.AttriaxJs) may not be loaded — ensure AttriaxJsBundle.jspre " +
                    "ships with the WebGL build.");
            }

            _handle = handle;
            lock (InstancesGate)
            {
                Instances[handle] = this;
            }

            // The engine wires its synchronization / deep-link streams and settles the
            // launch-link probe inside the `init` dispatch (see AttriaxWebGL.jslib).
            return CallVoid("init");
        }

        public Task Flush() => CallVoid("flush");

        public Task Reset() => CallVoid("reset");

        public async Task Dispose()
        {
            var handle = _handle;
            _handle = 0;
            if (handle == 0)
            {
                return;
            }

            try
            {
                await CallVoid("dispose", handleOverride: handle).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort dispose; still destroy + unregister below.
            }

            try
            {
                AttriaxWebGL_Destroy(handle);
            }
            catch
            {
                // Teardown is best-effort.
            }

            lock (InstancesGate)
            {
                Instances.Remove(handle);
            }
        }

        // -----------------------------------------------------------------
        // Tracking.
        // -----------------------------------------------------------------

        public Task RecordEvent(string name, IDictionary<string, object>? eventData = null, bool flushImmediately = false) =>
            CallVoid("recordEvent", AttriaxEngineArgs.RecordEvent(name, eventData, flushImmediately));

        public Task RecordPageView(
            string pageName,
            string? pageClass = null,
            string? pageTitle = null,
            string? previousPageName = null,
            IDictionary<string, object>? parameters = null,
            string source = "manual",
            bool flushImmediately = false) =>
            CallVoid("recordPageView", AttriaxEngineArgs.RecordPageView(
                pageName, pageClass, pageTitle, previousPageName, parameters, source, flushImmediately));

        public Task RecordPurchase(
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
            bool flushImmediately = true) =>
            CallVoid("recordPurchase", AttriaxEngineArgs.RecordPurchase(
                revenue, currency, revenueInMicros, purchaseType, productId, transactionId,
                originalTransactionId, validationProvider, validationEnvironment, purchaseToken,
                receiptData, signedPayload, receiptSignature, isRenewal, quantity, store, packageName,
                voided, test, validationId, metadata, flushImmediately));

        public Task RecordRefund(
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
            bool flushImmediately = true) =>
            CallVoid("recordRefund", AttriaxEngineArgs.RecordRefund(
                revenue, currency, revenueInMicros, purchaseType, productId, transactionId,
                originalTransactionId, quantity, store, packageName, voided, test, reason, metadata,
                flushImmediately));

        public Task RecordAdRevenue(
            double revenue,
            string currency = "USD",
            bool revenueInMicros = false,
            string? adNetwork = null,
            string? adFormat = null,
            string? adType = null,
            string? adPlacement = null,
            bool? test = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = true) =>
            CallVoid("recordAdRevenue", AttriaxEngineArgs.RecordAdRevenue(
                revenue, currency, revenueInMicros, adNetwork, adFormat, adType, adPlacement, test,
                metadata, flushImmediately));

        public Task RecordAdEvent(
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
            bool flushImmediately = true) =>
            // sdk-js's recordAdEvent re-derives the reserved wire name (`ad_show_failed`)
            // from its ad-type slug (`show_failed`), so translate back to the slug it
            // expects — mirroring the Flutter web binding's _adEventTypeSlug — and cross
            // it under `type`.
            CallVoid("recordAdEvent", AttriaxEngineArgs.RecordAdEvent(
                type: AdEventTypeSlug(eventName), eventName: null, adNetwork, mediationNetwork, adUnitId,
                adPlacement, adFormat, adType, failureReason, loadLatencyMs, rewardType, rewardAmount,
                test, metadata, flushImmediately));

        public Task RecordNotification(
            string type,
            string notificationId,
            string? linkId = null,
            string? campaignId = null,
            string? title = null,
            string? source = null,
            IDictionary<string, object>? payload = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = false) =>
            CallVoid("recordNotification", AttriaxEngineArgs.RecordNotification(
                type, notificationId, linkId, campaignId, title, source, payload, metadata,
                flushImmediately));

        public Task RecordError(
            string message,
            string exceptionType,
            string? stackTrace = null,
            bool fatal = false,
            string source = "manual",
            string? reason = null,
            IDictionary<string, object>? metadata = null) =>
            // The .jslib reconstructs a real `Error` (name = exceptionType, stack =
            // stackTrace) so sdk-js's `error instanceof Error` fast-path emits clean
            // exceptionType/message/stackTrace fields; `fatal` maps to sdk-js `isFatal`.
            CallVoid("recordError", AttriaxEngineArgs.RecordError(
                message, exceptionType, stackTrace, fatal, source, reason, metadata));

        public Task SetUser(string? userId = null, string? userName = null) =>
            CallVoid("setUser", AttriaxEngineArgs.SetUser(userId, userName));

        public Task SetUserProperty(string name, object? value) =>
            CallVoid("setUserProperty", AttriaxEngineArgs.SetUserProperty(name, value));

        public Task SetUserProperties(IDictionary<string, object> properties) =>
            CallVoid("setUserProperties", AttriaxEngineArgs.SetUserProperties(properties));

        public Task ClearUserProperties(IList<string>? propertyNames = null) =>
            CallVoid("clearUserProperties", AttriaxEngineArgs.ClearUserProperties(propertyNames));

        // Degraded: push/uninstall tokens are a mobile concept; the public sdk-js
        // surface exposes no token registration. No-op (matches attriax_web.dart).
        public Task RegisterPushToken(AttriaxPushTokenProvider provider, string? token, IDictionary<string, object>? metadata = null) =>
            Task.CompletedTask;

        // -----------------------------------------------------------------
        // Deep links.
        // -----------------------------------------------------------------

        public Task HandleIncomingLink(string uri, bool isInitialLink = false) =>
            CallVoid("handleIncomingLink", new Dictionary<string, object?> { ["uri"] = uri, ["isInitialLink"] = isInitialLink });

        // Degraded: sdk-js resolves the launch URL itself, so there is no absent
        // initial-link probe to complete. No-op.
        public Task CompleteInitialDeepLink() => Task.CompletedTask;

        public async Task<AttriaxDeepLinkEvent?> RecordDeepLink(Uri uri, IDictionary<string, object>? metadata = null, string source = "manual")
        {
            var token = await CallResult(
                "recordDeepLink",
                AttriaxEngineArgs.RecordDeepLink(uri.ToString(), metadata, source)).ConfigureAwait(false);
            return AttriaxWebGLEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        public async Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink()
        {
            var token = await CallResult("waitForInitialDeepLink").ConfigureAwait(false);
            return AttriaxWebGLEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        public async Task<AttriaxDeepLinkEvent?> WaitForDeepLinkResolution(AttriaxRawDeepLinkEvent rawEvent)
        {
            var args = new Dictionary<string, object?>
            {
                // sdk-js correlates the resolution by the raw event's URL; receivedAt is
                // forwarded as an ISO-8601 string to match its Date-shaped fields.
                ["uri"] = rawEvent.Uri.ToString(),
                ["receivedAt"] = rawEvent.ReceivedAt.ToString("o", CultureInfo.InvariantCulture),
                ["isInitial"] = rawEvent.IsInitial,
            };
            var token = await CallResult("waitResolution", args).ConfigureAwait(false);
            return AttriaxWebGLEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        public async Task<AttriaxCreateDynamicLinkResult> CreateDynamicLink(AttriaxCreateDynamicLinkOptions options)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "name", options.Name);
            Put(args, "destinationUrl", options.DestinationUrl);
            Put(args, "group", options.Group);
            Put(args, "prefix", options.Prefix);
            Put(args, "data", options.Data);

            var socialPreview = new Dictionary<string, object?>();
            Put(socialPreview, "title", options.PreviewTitle);
            Put(socialPreview, "description", options.PreviewDescription);
            if (socialPreview.Count > 0)
            {
                args["socialPreview"] = socialPreview;
            }

            var utms = new Dictionary<string, object?>();
            Put(utms, "source", options.UtmSource);
            Put(utms, "medium", options.UtmMedium);
            Put(utms, "campaign", options.UtmCampaign);
            Put(utms, "term", options.UtmTerm);
            Put(utms, "content", options.UtmContent);
            if (utms.Count > 0)
            {
                args["utms"] = utms;
            }

            var redirects = new Dictionary<string, object?>();
            Put(redirects, "ios", options.IOSRedirect);
            Put(redirects, "android", options.AndroidRedirect);
            if (redirects.Count > 0)
            {
                args["redirects"] = redirects;
            }

            var token = await CallResult("createDynamicLink", args).ConfigureAwait(false);
            return AttriaxWebGLEngineMapper.ToCreateDynamicLinkResult(token as JObject);
        }

        // -----------------------------------------------------------------
        // Receipt validation.
        // -----------------------------------------------------------------

        public async Task<AttriaxRevenueReceiptValidationResult> ValidateReceipt(
            string receipt,
            bool test = false,
            string? provider = null,
            string? environment = null,
            string? productId = null,
            string? transactionId = null)
        {
            var token = await CallResult(
                "validateReceipt",
                AttriaxEngineArgs.ValidateReceipt(receipt, test, provider, environment, productId, transactionId))
                .ConfigureAwait(false);
            return AttriaxWebGLEngineMapper.ToReceiptResult(token as JObject);
        }

        // -----------------------------------------------------------------
        // Consent / toggles.
        // -----------------------------------------------------------------

        public Task SetGdprConsent(bool analytics, bool attribution, bool adEvents) =>
            CallVoid("setGdprConsent", AttriaxEngineArgs.SetGdprConsent(analytics, attribution, adEvents));

        public Task SetGdprConsentNotRequired() => CallVoid("setGdprConsentNotRequired");

        public Task ResetGdprConsent() => CallVoid("resetGdprConsent");

        // Degraded: GDPR data erasure is not exposed on the public sdk-js surface.
        public Task RequestGdprDataErasure() => Task.CompletedTask;

        public Task SetAnonymousTracking(bool enabled) =>
            CallVoid("setAnonymousTracking", new Dictionary<string, object?> { ["enabled"] = enabled });

        // Degraded: sdk-js has no CCPA (doNotSell/usPrivacy) surface. The election is
        // carried on the config at construction (ignored by sdk-js); a runtime change
        // cannot be forwarded. No-op.
        public Task SetCcpaConsent(bool? doNotSell, string? usPrivacy) => Task.CompletedTask;

        public Task SetSdkEnabled(bool enabled) =>
            CallVoid("setEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });

        public Task SetEventTrackingEnabled(bool enabled) =>
            CallVoid("setEventTrackingEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });

        // -----------------------------------------------------------------
        // Apple seams — not applicable on the web (base defaults / no-ops).
        // -----------------------------------------------------------------

        public Task SubmitAsaToken(string token) => Task.CompletedTask;

        public Task SetTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status) => Task.CompletedTask;

        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorization(int? timeoutMs = null) =>
            Task.FromResult(AttriaxTrackingAuthorizationStatus.NotSupported);

        public Task<AttriaxSkanUpdateResult> UpdateSkanConversionValue(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue = null,
            bool lockWindow = false) =>
            Task.FromResult(new AttriaxSkanUpdateResult { Status = AttriaxSkanUpdateStatus.NotSupported });

        // -----------------------------------------------------------------
        // Getters.
        // -----------------------------------------------------------------

        public async Task<string?> GetDeviceId() =>
            (await CallResult("getDeviceId").ConfigureAwait(false))?.Value<string>();

        public async Task<bool> GetIsFirstLaunch() =>
            (await CallResult("getIsFirstLaunch").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetIsInitialized() =>
            (await CallResult("getIsInitialized").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxSdkSnapshot?> GetSdkSnapshot() =>
            AttriaxWebGLEngineMapper.ToSdkSnapshot(await CallResult("getSdkSnapshot").ConfigureAwait(false) as JObject);

        public async Task<bool> GetSdkEnabled() =>
            (await CallResult("getEnabled").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetEventTrackingEnabled() =>
            (await CallResult("getEventTrackingEnabled").ConfigureAwait(false))?.Value<bool>() ?? true;

        public async Task<bool> GetAnonymousTracking() =>
            (await CallResult("getAnonymousTracking").ConfigureAwait(false))?.Value<bool>() ?? true;

        public async Task<AttriaxSynchronizationState> GetSynchronizationState() =>
            AttriaxWebGLEngineMapper.ToSyncState((await CallResult("getSynchronizationState").ConfigureAwait(false))?.Value<string>());

        public async Task<bool> GetIsSynchronized() =>
            (await CallResult("getIsSynchronized").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrer(int? timeoutMs = null) =>
            AttriaxWebGLEngineMapper.ToInstallReferrer(await CallResult("getOriginalInstallReferrer").ConfigureAwait(false) as JObject);

        public async Task<AttriaxInstallReferrerDetails?> GetReinstallReferrer(int? timeoutMs = null) =>
            AttriaxWebGLEngineMapper.ToInstallReferrer(await CallResult("getReinstallReferrer").ConfigureAwait(false) as JObject);

        // Degraded: there is no platform install-referrer string on the web.
        public Task<string?> GetRawInstallReferrer(int? timeoutMs = null) => Task.FromResult<string?>(null);

        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrer(int? timeoutMs = null) =>
            AttriaxWebGLEngineMapper.ToDeepLinkReferrer(await CallResult("getSessionReferrer").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrer(int? timeoutMs = null) =>
            AttriaxWebGLEngineMapper.ToDeepLinkReferrer(await CallResult("getLatestDeepLinkReferrer").ConfigureAwait(false) as JObject);

        // Degraded: SKAdNetwork is iOS-only.
        public Task<AttriaxSkanState?> GetSkanState() => Task.FromResult<AttriaxSkanState?>(null);

        public async Task<AttriaxDeepLinkEvent?> GetLatestDeepLink() =>
            AttriaxWebGLEngineMapper.ToDeepLinkEvent(await CallResult("getLatestDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkEvent?> GetInitialDeepLink() =>
            AttriaxWebGLEngineMapper.ToDeepLinkEvent(await CallResult("getInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxRawDeepLinkEvent?> GetRawInitialDeepLink() =>
            AttriaxWebGLEngineMapper.ToRawDeepLinkEvent(await CallResult("getRawInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<bool> GetIsInitialDeepLinkResolved() =>
            (await CallResult("getInitialDeepLinkResolved").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> NeedsGdprConsent(bool localOnly = false) =>
            (await CallResult("needsGdprConsent", new Dictionary<string, object?> { ["localOnly"] = localOnly }).ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetIsWaitingForGdprConsent() =>
            (await CallResult("getIsWaitingForGdprConsent").ConfigureAwait(false))?.Value<bool>() ?? false;

        // Degraded: App Tracking Transparency is iOS-only.
        public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatus() =>
            Task.FromResult(AttriaxTrackingAuthorizationStatus.NotSupported);

        // Degraded: sdk-js has no CCPA surface.
        public Task<bool?> GetDoNotSell() => Task.FromResult<bool?>(null);

        public Task<string?> GetUsPrivacy() => Task.FromResult<string?>(null);

        // -----------------------------------------------------------------
        // Engine-event delivery (static trampoline -> instance, main thread).
        // -----------------------------------------------------------------

        [MonoPInvokeCallback(typeof(EventCallbackNative))]
        private static void OnNativeEventStatic(int handle, IntPtr eventJson)
        {
            if (eventJson == IntPtr.Zero)
            {
                return;
            }

            AttriaxWebGLEnginePlatform? self;
            lock (InstancesGate)
            {
                Instances.TryGetValue(handle, out self);
            }

            if (self == null)
            {
                return;
            }

            try
            {
                var json = Marshal.PtrToStringUTF8(eventJson);
                self.HandleNativeEvent(json);
            }
            catch
            {
                // A misbehaving callback must never crash the browser event loop.
            }
        }

        private void HandleNativeEvent(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            JObject envelope;
            try
            {
                envelope = JObject.Parse(json);
            }
            catch (JsonException)
            {
                return;
            }

            var type = envelope["type"]?.Value<string>();
            switch (type)
            {
                case "synchronizationState":
                {
                    var state = AttriaxWebGLEngineMapper.ToSyncState(envelope["state"]?.Value<string>());
                    AttriaxLifecycleDispatcher.PostToMainThread(() => SynchronizationStateChanged?.Invoke(state));
                    break;
                }

                case "deepLink":
                {
                    var evt = AttriaxWebGLEngineMapper.ToDeepLinkEvent(envelope["event"] as JObject);
                    if (evt != null)
                    {
                        AttriaxLifecycleDispatcher.PostToMainThread(() => DeepLinkResolved?.Invoke(evt));
                    }

                    break;
                }

                case "rawDeepLink":
                {
                    var raw = AttriaxWebGLEngineMapper.ToRawDeepLinkEvent(envelope["event"] as JObject);
                    if (raw != null)
                    {
                        AttriaxLifecycleDispatcher.PostToMainThread(() => RawDeepLinkReceived?.Invoke(raw));
                    }

                    break;
                }

                case "initialDeepLink":
                {
                    var resolved = envelope["resolved"]?.Value<bool>() ?? true;
                    var evt = AttriaxWebGLEngineMapper.ToDeepLinkEvent(envelope["event"] as JObject);
                    var resolution = new AttriaxInitialDeepLinkResolution(resolved, evt);
                    AttriaxLifecycleDispatcher.PostToMainThread(() => InitialDeepLinkResolved?.Invoke(resolution));
                    break;
                }
            }
        }

        // -----------------------------------------------------------------
        // Dispatch plumbing (request-id parked TCS -> result trampoline).
        // -----------------------------------------------------------------

        private async Task CallVoid(string method, object? args = null, int? handleOverride = null)
        {
            var json = await Dispatch(method, args, handleOverride).ConfigureAwait(false);
            UnwrapResult(json);
        }

        private async Task<JToken?> CallResult(string method, object? args = null)
        {
            var json = await Dispatch(method, args, null).ConfigureAwait(false);
            return UnwrapResult(json);
        }

        private Task<string?> Dispatch(string method, object? args, int? handleOverride)
        {
            var handle = handleOverride ?? _handle;
            if (handle == 0)
            {
                throw new InvalidOperationException("Attriax WebGL engine is not initialized.");
            }

            var requestId = Interlocked.Increment(ref _nextRequestId);
            // Deliberately NOT TaskCreationOptions.RunContinuationsAsynchronously (the
            // desktop twin's pattern): that flag queues await-continuations to the
            // ThreadPool, and Unity WebGL is single-threaded — ThreadPool work never
            // executes — so every engine await (starting with init) would hang forever.
            // Inline continuations are safe here: the result trampoline always runs on
            // the one main thread, and it copies the result out of the JS-owned buffer
            // into a managed string BEFORE completing the task, so continuations never
            // touch memory the .jslib is about to free.
            var tcs = new TaskCompletionSource<string?>();
            PendingResults[requestId] = tcs;

            try
            {
                AttriaxWebGL_Dispatch(handle, requestId, method, args == null ? "{}" : JsonConvert.SerializeObject(args));
            }
            catch (Exception exception)
            {
                PendingResults.TryRemove(requestId, out _);
                CompleteDetached(() => tcs.TrySetException(exception));
            }

            return tcs.Task;
        }

        [MonoPInvokeCallback(typeof(ResultCallbackNative))]
        private static void OnDispatchResultStatic(int requestId, IntPtr resultJson)
        {
            if (!PendingResults.TryRemove(requestId, out var tcs))
            {
                return;
            }

            // The .jslib owns the result buffer and frees it once this trampoline
            // returns, so copy synchronously into a managed string here.
            string? json;
            try
            {
                json = resultJson == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(resultJson);
            }
            catch (Exception exception)
            {
                CompleteDetached(() => tcs.TrySetException(exception));
                return;
            }

            CompleteDetached(() => tcs.TrySetResult(json));
        }

        /// <summary>
        /// Completes a parked <see cref="TaskCompletionSource{TResult}"/> with no ambient
        /// <see cref="SynchronizationContext"/>, so the waiting <c>ConfigureAwait(false)</c>
        /// continuations run INLINE on this stack — the same way they run on the
        /// desktop/Android twins, whose completions happen on worker threads.
        /// </summary>
        /// <remarks>
        /// On WebGL every completion happens on the single Unity main thread, where
        /// <see cref="SynchronizationContext.Current"/> is Unity's synchronization
        /// context. The .NET awaiter machinery refuses to inline a
        /// <c>ConfigureAwait(false)</c> continuation while a non-default context is
        /// current (<c>AwaitTaskContinuation.IsValidLocationForInlining</c>) and queues
        /// it to the ThreadPool instead — which never runs on the single-threaded web
        /// player, so every engine await would hang forever (this shipped as a
        /// permanently-stuck "Initializing Attriax SDK..."; task #71 found it via the
        /// scene-tracker verification). Clearing the context for the duration of the
        /// completion makes inlining valid again; continuations that captured the Unity
        /// context (plain <c>await</c>s in app code) still post to the player loop as
        /// usual.
        /// </remarks>
        private static void CompleteDetached(Action complete)
        {
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                complete();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static JToken? UnwrapResult(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var envelope = JObject.Parse(json);
            var ok = envelope["ok"];
            if (ok == null || ok.Type != JTokenType.Boolean || !ok.Value<bool>())
            {
                var error = envelope["error"]?.Value<string>() ?? "unknown_error";
                throw new AttriaxWebGLDispatchException(error);
            }

            var value = envelope["value"];
            return value == null || value.Type == JTokenType.Null ? null : value;
        }

        private static void EnsureCallbacksRegistered()
        {
            if (_callbacksRegistered)
            {
                return;
            }

            lock (RegistrationGate)
            {
                if (_callbacksRegistered)
                {
                    return;
                }

                AttriaxWebGL_RegisterCallbacks(
                    Marshal.GetFunctionPointerForDelegate(ResultCallbackDelegate),
                    Marshal.GetFunctionPointerForDelegate(EventCallbackDelegate));
                _callbacksRegistered = true;
            }
        }

        // -----------------------------------------------------------------
        // Small helpers (twins of the desktop binding).
        // -----------------------------------------------------------------

        private static void Put(IDictionary<string, object?> args, string key, object? value)
        {
            if (value != null)
            {
                args[key] = value;
            }
        }

        private static string AdEventTypeSlug(string eventName)
        {
            switch (eventName)
            {
                case "ad_request": return "request";
                case "ad_load": return "load";
                case "ad_load_failed": return "load_failed";
                case "ad_show": return "show";
                case "ad_show_failed": return "show_failed";
                case "ad_impression": return "impression";
                case "ad_click": return "click";
                case "ad_dismiss": return "dismiss";
                case "ad_reward": return "reward";
                default:
                    return eventName.StartsWith("ad_", StringComparison.Ordinal)
                        ? eventName.Substring(3)
                        : eventName;
            }
        }
    }

    /// <summary>Raised when the <c>.jslib</c> returns an <c>{"ok":false,"error":…}</c> envelope.</summary>
    internal sealed class AttriaxWebGLDispatchException : Exception
    {
        public AttriaxWebGLDispatchException(string error)
            : base($"Attriax WebGL engine dispatch failed: {error}")
        {
        }
    }
}
#endif
