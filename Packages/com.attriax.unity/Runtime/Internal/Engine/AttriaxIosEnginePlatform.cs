#nullable enable
#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Attriax.Unity.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// iOS <see cref="IAttriaxEnginePlatform"/> implementation. Drives the shared Kotlin
    /// Multiplatform core — Attriax's reference engine — through the exact same <b>C-ABI</b>
    /// the desktop binding uses (<see cref="AttriaxDesktopEnginePlatform"/>), only the
    /// linkage differs: iOS is IL2CPP + static linking, so the five C entrypoints
    /// (<c>attriax_create</c>, <c>attriax_dispatch</c>,
    /// <c>attriax_register_event_callback</c>, <c>attriax_free_string</c>,
    /// <c>attriax_destroy</c>) are P/Invoked via <c>[DllImport("__Internal")]</c> against the
    /// statically-linked <c>libattriax_core.a</c> (the KMP <c>staticLib("attriax_core")</c>
    /// output) rather than dlopen'd — iOS forbids dlopen of an app dylib.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The wire contract is identical to the desktop C-ABI, so this binding reuses
    /// <see cref="AttriaxDesktopEngineMapper"/> verbatim for every JSON→typed conversion.
    /// Each command serializes its wire-keyed argument map to JSON and calls
    /// <c>attriax_dispatch</c> on a dedicated worker thread (never Unity's main thread —
    /// dispatch is <c>runBlocking</c>-bridged and some calls do a network round-trip). The
    /// result is the <c>{"ok":true,"value":…}</c> / <c>{"ok":false,"error":…}</c> envelope;
    /// every returned <c>char*</c> — and every event <c>char*</c> — is heap-allocated by the
    /// engine and freed exactly once via <c>attriax_free_string</c> (caller-frees UAF
    /// contract, carried over from <c>AttriaxCApi.kt</c>).
    /// </para>
    /// <para>
    /// The engine event callback is a static <see cref="AOT.MonoPInvokeCallbackAttribute"/>
    /// trampoline (required under IL2CPP) kept alive in a static field; it decodes the
    /// event, frees the string, and re-raises on the Unity main thread via
    /// <see cref="AttriaxLifecycleDispatcher.PostToMainThread"/> (fire-and-forget — never a
    /// blocking marshal). The owning instance is recovered from a <see cref="GCHandle"/>
    /// passed as the C-ABI <c>userData</c>.
    /// </para>
    /// <para>
    /// Members the C-ABI does not route degrade to a benign no-op / default, matching the
    /// desktop binding: <see cref="SetEventTrackingEnabled"/> /
    /// <see cref="GetEventTrackingEnabled"/> (the C-ABI exposes only the whole-SDK
    /// <c>enabled</c> toggle), <see cref="CompleteInitialDeepLink"/> /
    /// <see cref="WaitForInitialDeepLink"/> / <see cref="WaitForDeepLinkResolution"/>, and
    /// the raw / initial-resolution deep-link event streams (only the resolved-deep-link and
    /// synchronization-state listeners are wired).
    /// </para>
    /// </remarks>
    internal sealed class AttriaxIosEnginePlatform : IAttriaxEnginePlatform
    {
        // -----------------------------------------------------------------
        // C-ABI — statically linked libattriax_core.a via __Internal.
        // -----------------------------------------------------------------

        [DllImport("__Internal")]
        private static extern IntPtr attriax_create(IntPtr configJson, IntPtr dataDir);

        [DllImport("__Internal")]
        private static extern IntPtr attriax_dispatch(IntPtr handle, IntPtr method, IntPtr argsJson);

        [DllImport("__Internal")]
        private static extern void attriax_register_event_callback(IntPtr handle, IntPtr callback, IntPtr userData);

        [DllImport("__Internal")]
        private static extern void attriax_free_string(IntPtr ptr);

        [DllImport("__Internal")]
        private static extern void attriax_destroy(IntPtr handle);

        private readonly AttriaxIosEngineWorker _worker = new AttriaxIosEngineWorker();
        private IntPtr _handle = IntPtr.Zero;
        private GCHandle _selfHandle;

        // -----------------------------------------------------------------
        // Static event-callback trampoline (IL2CPP-safe: static + kept alive).
        // -----------------------------------------------------------------

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallbackNative(IntPtr eventJson, IntPtr userData);

        private static readonly EventCallbackNative EventCallbackDelegate = OnNativeEventStatic;
        private static readonly IntPtr EventCallbackPtr =
            Marshal.GetFunctionPointerForDelegate(EventCallbackDelegate);

        public event Action<AttriaxSynchronizationState> SynchronizationStateChanged = delegate { };
        public event Action<AttriaxDeepLinkEvent> DeepLinkResolved = delegate { };
        public event Action<AttriaxRawDeepLinkEvent> RawDeepLinkReceived = delegate { };
        public event Action<AttriaxInitialDeepLinkResolution> InitialDeepLinkResolved = delegate { };

        // -----------------------------------------------------------------
        // Lifecycle.
        // -----------------------------------------------------------------

        public Task InitializeAsync(AttriaxConfig config)
        {
            var configJson = JsonConvert.SerializeObject(config.ToEngineArguments());
            return _worker.Enqueue(() =>
            {
                IntPtr configPtr = Utf8ToNative(configJson);
                try
                {
                    // dataDir null → the Apple engine persists via NSUserDefaults.
                    var handle = attriax_create(configPtr, IntPtr.Zero);
                    if (handle == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("attriax_create returned a null handle.");
                    }

                    _handle = handle;
                    _selfHandle = GCHandle.Alloc(this);
                    attriax_register_event_callback(handle, EventCallbackPtr, GCHandle.ToIntPtr(_selfHandle));

                    // Surface a genuine init failure (create succeeded but init errored).
                    UnwrapResult(DispatchRaw("init", null));
                }
                finally
                {
                    Marshal.FreeHGlobal(configPtr);
                }
            });
        }

        public Task Flush() => CallVoid("flush");

        public Task Reset() => CallVoid("reset");

        public Task Dispose()
        {
            return _worker.Enqueue(() =>
            {
                var handle = _handle;
                if (handle != IntPtr.Zero)
                {
                    try
                    {
                        attriax_register_event_callback(handle, IntPtr.Zero, IntPtr.Zero);
                        DispatchRaw("dispose", null);
                    }
                    catch
                    {
                        // Best-effort dispose; still destroy below.
                    }

                    attriax_destroy(handle);
                }

                _handle = IntPtr.Zero;
                if (_selfHandle.IsAllocated)
                {
                    _selfHandle.Free();
                }
            }).ContinueWith(_ => _worker.Dispose(), TaskScheduler.Default);
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
            // The C-ABI router resolves the reserved event name (e.g. "ad_show_failed")
            // back to the AttriaxAdEventType whose name/eventName matches, under arg
            // key `type` (see AttriaxCApi.route recordAdEvent).
            CallVoid("recordAdEvent", AttriaxEngineArgs.RecordAdEvent(
                type: eventName, eventName: null, adNetwork, mediationNetwork, adUnitId, adPlacement,
                adFormat, adType, failureReason, loadLatencyMs, rewardType, rewardAmount, test,
                metadata, flushImmediately));

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

        public Task RegisterPushToken(AttriaxPushTokenProvider provider, string? token, IDictionary<string, object>? metadata = null)
        {
            // The C-ABI splits push registration by provider.
            var method = provider == AttriaxPushTokenProvider.Apns
                ? "registerApplePushToken"
                : "registerFirebaseMessagingToken";
            var args = new Dictionary<string, object?> { ["token"] = token };
            Put(args, "metadata", metadata);
            return CallVoid(method, args);
        }

        // -----------------------------------------------------------------
        // Deep links.
        // -----------------------------------------------------------------

        public Task HandleIncomingLink(string uri, bool isInitialLink = false) =>
            CallVoid("handleIncomingLink", new Dictionary<string, object?> { ["uri"] = uri, ["isInitialLink"] = isInitialLink });

        // The engine resolves the launch link itself, so there is no absent initial-link
        // probe to complete — the shared deferred item across bindings.
        public Task CompleteInitialDeepLink() => Task.CompletedTask;

        public async Task<AttriaxDeepLinkEvent?> RecordDeepLink(Uri uri, IDictionary<string, object>? metadata = null, string source = "manual")
        {
            var token = await CallResult(
                "recordDeepLink",
                AttriaxEngineArgs.RecordDeepLink(uri.ToString(), metadata, source)).ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        // Degraded: the C-ABI routes no blocking initial-link wait. Returns the cached
        // launch link if the engine already resolved one.
        public Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink() => GetInitialDeepLink();

        // Degraded: the C-ABI routes no blocking resolution wait; the adapter falls back
        // to the next resolved event on the deep-link stream.
        public Task<AttriaxDeepLinkEvent?> WaitForDeepLinkResolution(AttriaxRawDeepLinkEvent rawEvent) =>
            Task.FromResult<AttriaxDeepLinkEvent?>(null);

        public async Task<AttriaxCreateDynamicLinkResult> CreateDynamicLink(AttriaxCreateDynamicLinkOptions options)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "name", options.Name);
            Put(args, "destinationUrl", options.DestinationUrl);
            Put(args, "group", options.Group);
            Put(args, "prefix", options.Prefix);
            Put(args, "previewTitle", options.PreviewTitle);
            Put(args, "previewDescription", options.PreviewDescription);
            Put(args, "utmSource", options.UtmSource);
            Put(args, "utmMedium", options.UtmMedium);
            Put(args, "utmCampaign", options.UtmCampaign);
            Put(args, "utmTerm", options.UtmTerm);
            Put(args, "utmContent", options.UtmContent);
            Put(args, "iosRedirect", options.IOSRedirect);
            Put(args, "androidRedirect", options.AndroidRedirect);
            Put(args, "data", options.Data);
            var token = await CallResult("createDynamicLink", args).ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToCreateDynamicLinkResult(token as JObject);
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
            return AttriaxDesktopEngineMapper.ToReceiptResult(token as JObject);
        }

        // -----------------------------------------------------------------
        // Consent / toggles.
        // -----------------------------------------------------------------

        public Task SetGdprConsent(bool analytics, bool attribution, bool adEvents) =>
            CallVoid("setGdprConsent", AttriaxEngineArgs.SetGdprConsent(analytics, attribution, adEvents));

        public Task SetGdprConsentNotRequired() => CallVoid("setGdprConsentNotRequired");

        public Task ResetGdprConsent() => CallVoid("resetGdprConsent");

        public Task RequestGdprDataErasure() => CallVoid("requestGdprDataErasure");

        public Task SetAnonymousTracking(bool enabled) =>
            CallVoid("setAnonymousTracking", new Dictionary<string, object?> { ["enabled"] = enabled });

        public async Task SetCcpaConsent(bool? doNotSell, string? usPrivacy)
        {
            // The C-ABI splits the CCPA election into two setters; forward each field
            // that was supplied.
            if (doNotSell.HasValue)
            {
                await CallVoid("setDoNotSell", new Dictionary<string, object?> { ["doNotSell"] = doNotSell.Value })
                    .ConfigureAwait(false);
            }

            if (usPrivacy != null)
            {
                await CallVoid("setUsPrivacy", new Dictionary<string, object?> { ["usPrivacy"] = usPrivacy })
                    .ConfigureAwait(false);
            }
        }

        public Task SetSdkEnabled(bool enabled) =>
            CallVoid("setEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });

        // Degraded: the C-ABI exposes only the whole-SDK `enabled` toggle, not the
        // separate tracking-enabled flag. No-op.
        public Task SetEventTrackingEnabled(bool enabled) => Task.CompletedTask;

        // -----------------------------------------------------------------
        // Apple seams (native on iOS — forwarded through the C-ABI to the engine's
        // real ATT/IDFA/SKAN/ASA adapters).
        // -----------------------------------------------------------------

        public Task SubmitAsaToken(string token) =>
            CallVoid("submitAsaToken", new Dictionary<string, object?> { ["token"] = token });

        public Task SetTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status) =>
            CallVoid("setAttStatus", new Dictionary<string, object?> { ["status"] = AttToWire(status) });

        public async Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorization(int? timeoutMs = null)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "timeoutMs", timeoutMs);
            var token = await CallResult("requestAttAuthorization", args).ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToTrackingAuthStatus(token?.Value<string>());
        }

        public async Task<AttriaxSkanUpdateResult> UpdateSkanConversionValue(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue = null,
            bool lockWindow = false)
        {
            var token = await CallResult(
                "updateSkanConversionValue",
                AttriaxEngineArgs.UpdateSkanConversionValue(
                    fineValue, coarseValue.HasValue ? CoarseToWire(coarseValue.Value) : null, lockWindow))
                .ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToSkanUpdateResult(token as JObject);
        }

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
            AttriaxDesktopEngineMapper.ToSdkSnapshot(await CallResult("getSdkSnapshot").ConfigureAwait(false) as JObject);

        public async Task<bool> GetSdkEnabled() =>
            (await CallResult("getEnabled").ConfigureAwait(false))?.Value<bool>() ?? false;

        // Degraded: the C-ABI exposes only the whole-SDK `enabled` flag. Reports the
        // base default (enabled).
        public Task<bool> GetEventTrackingEnabled() => Task.FromResult(true);

        public async Task<bool> GetAnonymousTracking() =>
            (await CallResult("getAnonymousTracking").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxSynchronizationState> GetSynchronizationState() =>
            AttriaxDesktopEngineMapper.ToSyncState((await CallResult("getSynchronizationState").ConfigureAwait(false))?.Value<string>());

        public async Task<bool> GetIsSynchronized() =>
            (await CallResult("getIsSynchronized").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrer(int? timeoutMs = null) =>
            AttriaxDesktopEngineMapper.ToInstallReferrer(await CallResult("getOriginalInstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxInstallReferrerDetails?> GetReinstallReferrer(int? timeoutMs = null) =>
            AttriaxDesktopEngineMapper.ToInstallReferrer(await CallResult("getReinstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<string?> GetRawInstallReferrer(int? timeoutMs = null) =>
            (await CallResult("getRawInstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false))?.Value<string>();

        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrer(int? timeoutMs = null) =>
            AttriaxDesktopEngineMapper.ToDeepLinkReferrer(await CallResult("getSessionReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrer(int? timeoutMs = null) =>
            AttriaxDesktopEngineMapper.ToDeepLinkReferrer(await CallResult("getLatestDeepLinkReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxSkanState?> GetSkanState() =>
            AttriaxDesktopEngineMapper.ToSkanState(await CallResult("getSkanState").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkEvent?> GetLatestDeepLink() =>
            AttriaxDesktopEngineMapper.ToDeepLinkEvent(await CallResult("getLatestDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkEvent?> GetInitialDeepLink() =>
            AttriaxDesktopEngineMapper.ToDeepLinkEvent(await CallResult("getInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxRawDeepLinkEvent?> GetRawInitialDeepLink() =>
            AttriaxDesktopEngineMapper.ToRawDeepLinkEvent(await CallResult("getRawInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<bool> GetIsInitialDeepLinkResolved() =>
            (await CallResult("getInitialDeepLinkResolved").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> NeedsGdprConsent(bool localOnly = false) =>
            (await CallResult("needsGdprConsent", new Dictionary<string, object?> { ["localOnly"] = localOnly }).ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetIsWaitingForGdprConsent() =>
            (await CallResult("getIsWaitingForGdprConsent").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatus() =>
            AttriaxDesktopEngineMapper.ToTrackingAuthStatus((await CallResult("getAttStatus").ConfigureAwait(false))?.Value<string>());

        public async Task<bool?> GetDoNotSell()
        {
            var token = await CallResult("getDoNotSell").ConfigureAwait(false);
            return token == null ? (bool?)null : token.Value<bool>();
        }

        public async Task<string?> GetUsPrivacy() =>
            (await CallResult("getUsPrivacy").ConfigureAwait(false))?.Value<string>();

        // -----------------------------------------------------------------
        // Engine-event delivery (static trampoline -> instance, off the main thread).
        // -----------------------------------------------------------------

        [AOT.MonoPInvokeCallback(typeof(EventCallbackNative))]
        private static void OnNativeEventStatic(IntPtr eventJson, IntPtr userData)
        {
            if (eventJson == IntPtr.Zero)
            {
                return;
            }

            AttriaxIosEnginePlatform? self = null;
            try
            {
                if (userData != IntPtr.Zero)
                {
                    var gch = GCHandle.FromIntPtr(userData);
                    if (gch.IsAllocated)
                    {
                        self = gch.Target as AttriaxIosEnginePlatform;
                    }
                }

                var json = Utf8FromNative(eventJson);
                self?.HandleNativeEvent(json);
            }
            catch
            {
                // A misbehaving callback must never crash a background engine thread.
            }
            finally
            {
                // Ownership of eventJson transferred to us; free it exactly once.
                attriax_free_string(eventJson);
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
            if (type == "synchronizationState")
            {
                var state = AttriaxDesktopEngineMapper.ToSyncState(envelope["state"]?.Value<string>());
                AttriaxLifecycleDispatcher.PostToMainThread(() => SynchronizationStateChanged?.Invoke(state));
            }
            else if (type == "deepLink")
            {
                var evt = AttriaxDesktopEngineMapper.ToDeepLinkEvent(envelope["event"] as JObject);
                if (evt != null)
                {
                    AttriaxLifecycleDispatcher.PostToMainThread(() => DeepLinkResolved?.Invoke(evt));
                }
            }
        }

        // -----------------------------------------------------------------
        // Dispatch plumbing.
        // -----------------------------------------------------------------

        private Task CallVoid(string method, object? args = null) =>
            _worker.Enqueue(() => DispatchRaw(method, args == null ? null : JsonConvert.SerializeObject(args)));

        private async Task<JToken?> CallResult(string method, object? args = null)
        {
            var json = await _worker
                .Enqueue(() => DispatchRaw(method, args == null ? null : JsonConvert.SerializeObject(args)))
                .ConfigureAwait(false);
            return UnwrapResult(json);
        }

        /// <summary>
        /// Marshals <paramref name="method"/> + <paramref name="argsJson"/> to the C-ABI,
        /// returns the decoded envelope string, and frees every native string (args in
        /// <c>finally</c>, the result via <c>attriax_free_string</c>).
        /// </summary>
        private string? DispatchRaw(string method, string? argsJson)
        {
            var handle = _handle;
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Attriax iOS engine is not initialized.");
            }

            IntPtr methodPtr = Utf8ToNative(method);
            IntPtr argsPtr = Utf8ToNative(argsJson ?? "{}");
            IntPtr resultPtr = IntPtr.Zero;
            try
            {
                resultPtr = attriax_dispatch(handle, methodPtr, argsPtr);
                return Utf8FromNative(resultPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(methodPtr);
                Marshal.FreeHGlobal(argsPtr);
                if (resultPtr != IntPtr.Zero)
                {
                    attriax_free_string(resultPtr);
                }
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
                throw new AttriaxIosDispatchException(error);
            }

            var value = envelope["value"];
            return value == null || value.Type == JTokenType.Null ? null : value;
        }

        private static IDictionary<string, object?>? TimeoutArgs(int? timeoutMs)
        {
            if (!timeoutMs.HasValue)
            {
                return null;
            }

            return new Dictionary<string, object?> { ["timeoutMs"] = timeoutMs.Value };
        }

        private static void Put(IDictionary<string, object?> args, string key, object? value)
        {
            if (value != null)
            {
                args[key] = value;
            }
        }

        private static string AttToWire(AttriaxTrackingAuthorizationStatus status)
        {
            // The C-ABI setAttStatus parses the AttriaxAttStatus wireValues.
            switch (status)
            {
                case AttriaxTrackingAuthorizationStatus.Authorized:
                    return "authorized";
                case AttriaxTrackingAuthorizationStatus.Denied:
                    return "denied";
                case AttriaxTrackingAuthorizationStatus.Restricted:
                    return "restricted";
                case AttriaxTrackingAuthorizationStatus.NotDetermined:
                    return "notDetermined";
                default:
                    return "unknown";
            }
        }

        private static string CoarseToWire(AttriaxSkanCoarseValue coarse)
        {
            switch (coarse)
            {
                case AttriaxSkanCoarseValue.Medium:
                    return "medium";
                case AttriaxSkanCoarseValue.High:
                    return "high";
                default:
                    return "low";
            }
        }

        // -----------------------------------------------------------------
        // UTF-8 marshaling (portable; avoids netstandard PtrToStringUTF8 dependency).
        // -----------------------------------------------------------------

        private static IntPtr Utf8ToNative(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        private static string? Utf8FromNative(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }

            if (length == 0)
            {
                return string.Empty;
            }

            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// A single background worker thread. All native engine interaction runs here
        /// (never Unity's main thread), mirroring the desktop binding's single-thread
        /// executor and the Android binding's single-thread dispatch discipline.
        /// </summary>
        private sealed class AttriaxIosEngineWorker : IDisposable
        {
            private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
            private readonly Thread _thread;

            public AttriaxIosEngineWorker()
            {
                _thread = new Thread(Run) { IsBackground = true, Name = "attriax-ios-engine" };
                _thread.Start();
            }

            public Task<T> Enqueue<T>(Func<T> func)
            {
                var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (_queue.IsAddingCompleted)
                {
                    tcs.SetException(new ObjectDisposedException(nameof(AttriaxIosEngineWorker)));
                    return tcs.Task;
                }

                _queue.Add(() =>
                {
                    try
                    {
                        tcs.SetResult(func());
                    }
                    catch (Exception exception)
                    {
                        tcs.SetException(exception);
                    }
                });
                return tcs.Task;
            }

            public Task Enqueue(Action action) =>
                Enqueue<object?>(() =>
                {
                    action();
                    return null;
                });

            public void Dispose() => _queue.CompleteAdding();

            private void Run()
            {
                foreach (var action in _queue.GetConsumingEnumerable())
                {
                    action();
                }
            }
        }
    }

    /// <summary>Raised when the C-ABI returns an <c>{"ok":false,"error":…}</c> envelope.</summary>
    internal sealed class AttriaxIosDispatchException : Exception
    {
        public AttriaxIosDispatchException(string error)
            : base($"Attriax iOS engine dispatch failed: {error}")
        {
        }
    }
}
#endif
