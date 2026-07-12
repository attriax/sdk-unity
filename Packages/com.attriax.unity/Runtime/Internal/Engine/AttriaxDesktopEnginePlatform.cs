#nullable enable
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX))
#define ATTRIAX_DESKTOP_ENGINE
#endif
#if ATTRIAX_DESKTOP_ENGINE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
    /// Desktop + Unity-Editor <see cref="IAttriaxEnginePlatform"/> implementation.
    /// Drives the shared Kotlin Multiplatform core — Attriax's reference engine —
    /// through its <b>C-ABI</b> shared library (<c>attriax_core.dll</c> /
    /// <c>libattriax_core.so</c>), the exact boundary the Flutter Windows/Linux
    /// bindings use (<c>AttriaxWindows</c> over <c>dart:ffi</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The library is loaded <b>dynamically</b> (Windows <c>LoadLibraryEx</c> /
    /// <c>GetProcAddress</c>, Unix <c>dlopen</c> / <c>dlsym</c>) rather than through a
    /// by-name <c>[DllImport("attriax_core")]</c>, so the Editor can load — and a
    /// future domain-reload path could unload — the native lib without a hard link
    /// that the loader would lock. The five exported functions
    /// (<c>attriax_create</c>, <c>attriax_dispatch</c>,
    /// <c>attriax_register_event_callback</c>, <c>attriax_free_string</c>,
    /// <c>attriax_destroy</c>) are bound to Cdecl delegates.
    /// </para>
    /// <para>
    /// Every command serializes its wire-keyed argument map to JSON and calls
    /// <c>attriax_dispatch</c> on a dedicated worker thread (never Unity's main
    /// thread — <c>attriax_dispatch</c> is <c>runBlocking</c>-bridged and
    /// <see cref="ValidateReceipt"/> does a network round-trip). The result is the
    /// <c>{"ok":true,"value":…}</c> / <c>{"ok":false,"error":…}</c> envelope; every
    /// returned <c>char*</c> — and every event <c>char*</c> — is heap-allocated by the
    /// engine and freed exactly once via <c>attriax_free_string</c> (the caller-frees
    /// UAF contract, carried over from <c>AttriaxCApi.kt</c>).
    /// </para>
    /// <para>
    /// The engine event callback is a static <see cref="AOT.MonoPInvokeCallbackAttribute"/>
    /// trampoline (required under IL2CPP) kept alive in a static field; it decodes the
    /// event, frees the string, and re-raises on the Unity main thread via
    /// <see cref="AttriaxLifecycleDispatcher.PostToMainThread"/> (fire-and-forget —
    /// never a blocking marshal; see the DebugLog main-thread deadlock lesson). The
    /// owning instance is recovered from a <see cref="GCHandle"/> passed as the C-ABI
    /// <c>userData</c>.
    /// </para>
    /// <para>
    /// Members the C-ABI does not route degrade to a benign no-op / default, matching
    /// the Flutter desktop binding: <see cref="SetEventTrackingEnabled"/> /
    /// <see cref="GetEventTrackingEnabled"/> (the C-ABI exposes only the whole-SDK
    /// <c>enabled</c> toggle), <see cref="CompleteInitialDeepLink"/> /
    /// <see cref="WaitForInitialDeepLink"/> / <see cref="WaitForDeepLinkResolution"/>
    /// (the engine resolves the launch link itself), and the raw / initial-resolution
    /// deep-link event streams (only the resolved-deep-link and synchronization-state
    /// listeners are wired).
    /// </para>
    /// </remarks>
    internal sealed class AttriaxDesktopEnginePlatform : IAttriaxEnginePlatform
    {
        private const string WindowsLibraryName = "attriax_core.dll";
        private const string LinuxLibraryName = "libattriax_core.so";
        private const string MacLibraryName = "libattriax_core.dylib";

        private readonly AttriaxDesktopEngineWorker _worker = new AttriaxDesktopEngineWorker();

        private NativeLibrary? _library;
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
                var library = NativeLibrary.Load();
                _library = library;

                IntPtr configPtr = Utf8ToNative(configJson);
                try
                {
                    // dataDir left null → the engine uses AttriaxDesktopNative.defaultDataDir.
                    var handle = library.Create(configPtr, IntPtr.Zero);
                    if (handle == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("attriax_create returned a null handle.");
                    }

                    _handle = handle;
                    _selfHandle = GCHandle.Alloc(this);
                    library.RegisterEventCallback(handle, EventCallbackPtr, GCHandle.ToIntPtr(_selfHandle));

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
                var library = _library;
                var handle = _handle;
                if (library != null && handle != IntPtr.Zero)
                {
                    try
                    {
                        library.RegisterEventCallback(handle, IntPtr.Zero, IntPtr.Zero);
                        DispatchRaw("dispose", null);
                    }
                    catch
                    {
                        // Best-effort dispose; still destroy + free below.
                    }

                    library.Destroy(handle);
                }

                _handle = IntPtr.Zero;
                if (_selfHandle.IsAllocated)
                {
                    _selfHandle.Free();
                }

                library?.Dispose();
                _library = null;
            }).ContinueWith(_ => _worker.Dispose(), TaskScheduler.Default);
        }

        // -----------------------------------------------------------------
        // Tracking.
        // -----------------------------------------------------------------

        public Task RecordEvent(string name, IDictionary<string, object>? eventData = null, bool flushImmediately = false)
        {
            var args = Args(("name", name), ("flushImmediately", flushImmediately));
            Put(args, "eventData", eventData);
            return CallVoid("recordEvent", args);
        }

        public Task RecordPageView(
            string pageName,
            string? pageClass = null,
            string? pageTitle = null,
            string? previousPageName = null,
            IDictionary<string, object>? parameters = null,
            string source = "manual",
            bool flushImmediately = false)
        {
            var args = Args(("pageName", pageName), ("source", source), ("flushImmediately", flushImmediately));
            Put(args, "pageClass", pageClass);
            Put(args, "pageTitle", pageTitle);
            Put(args, "previousPageName", previousPageName);
            Put(args, "parameters", parameters);
            return CallVoid("recordPageView", args);
        }

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
            bool flushImmediately = true)
        {
            var args = Args(
                ("revenue", revenue),
                ("currency", currency),
                ("revenueInMicros", revenueInMicros),
                ("quantity", quantity),
                ("flushImmediately", flushImmediately));
            Put(args, "purchaseType", purchaseType);
            Put(args, "productId", productId);
            Put(args, "transactionId", transactionId);
            Put(args, "originalTransactionId", originalTransactionId);
            Put(args, "validationProvider", validationProvider);
            Put(args, "validationEnvironment", validationEnvironment);
            Put(args, "purchaseToken", purchaseToken);
            Put(args, "receiptData", receiptData);
            Put(args, "signedPayload", signedPayload);
            Put(args, "receiptSignature", receiptSignature);
            Put(args, "isRenewal", isRenewal);
            Put(args, "store", store);
            Put(args, "packageName", packageName);
            Put(args, "voided", voided);
            Put(args, "test", test);
            Put(args, "validationId", validationId);
            Put(args, "metadata", metadata);
            return CallVoid("recordPurchase", args);
        }

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
            bool flushImmediately = true)
        {
            var args = Args(
                ("revenue", revenue),
                ("currency", currency),
                ("revenueInMicros", revenueInMicros),
                ("quantity", quantity),
                ("flushImmediately", flushImmediately));
            Put(args, "purchaseType", purchaseType);
            Put(args, "productId", productId);
            Put(args, "transactionId", transactionId);
            Put(args, "originalTransactionId", originalTransactionId);
            Put(args, "store", store);
            Put(args, "packageName", packageName);
            Put(args, "voided", voided);
            Put(args, "test", test);
            Put(args, "reason", reason);
            Put(args, "metadata", metadata);
            return CallVoid("recordRefund", args);
        }

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
            bool flushImmediately = true)
        {
            var args = Args(
                ("revenue", revenue),
                ("currency", currency),
                ("revenueInMicros", revenueInMicros),
                ("flushImmediately", flushImmediately));
            Put(args, "adNetwork", adNetwork);
            Put(args, "adFormat", adFormat);
            Put(args, "adType", adType);
            Put(args, "adPlacement", adPlacement);
            Put(args, "test", test);
            Put(args, "metadata", metadata);
            return CallVoid("recordAdRevenue", args);
        }

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
            bool flushImmediately = true)
        {
            // The C-ABI router resolves the reserved event name (e.g. "ad_show_failed")
            // back to the AttriaxAdEventType whose name/eventName matches, under arg
            // key `type` (see AttriaxCApi.route recordAdEvent).
            var args = Args(("type", eventName), ("flushImmediately", flushImmediately));
            Put(args, "adNetwork", adNetwork);
            Put(args, "mediationNetwork", mediationNetwork);
            Put(args, "adUnitId", adUnitId);
            Put(args, "adPlacement", adPlacement);
            Put(args, "adFormat", adFormat);
            Put(args, "adType", adType);
            Put(args, "failureReason", failureReason);
            Put(args, "loadLatencyMs", loadLatencyMs);
            Put(args, "rewardType", rewardType);
            Put(args, "rewardAmount", rewardAmount);
            Put(args, "test", test);
            Put(args, "metadata", metadata);
            return CallVoid("recordAdEvent", args);
        }

        public Task RecordNotification(
            string type,
            string notificationId,
            string? linkId = null,
            string? campaignId = null,
            string? title = null,
            string? source = null,
            IDictionary<string, object>? payload = null,
            IDictionary<string, object>? metadata = null,
            bool flushImmediately = false)
        {
            var args = Args(("type", type), ("notificationId", notificationId), ("flushImmediately", flushImmediately));
            Put(args, "linkId", linkId);
            Put(args, "campaignId", campaignId);
            Put(args, "title", title);
            Put(args, "source", source);
            Put(args, "payload", payload);
            Put(args, "metadata", metadata);
            return CallVoid("recordNotification", args);
        }

        public Task RecordError(
            string message,
            string exceptionType,
            string? stackTrace = null,
            bool fatal = false,
            string source = "manual",
            string? reason = null,
            IDictionary<string, object>? metadata = null)
        {
            var args = Args(
                ("message", message),
                ("exceptionType", exceptionType),
                ("fatal", fatal),
                ("source", source));
            Put(args, "stackTrace", stackTrace);
            Put(args, "reason", reason);
            Put(args, "metadata", metadata);
            return CallVoid("recordError", args);
        }

        public Task SetUser(string? userId = null, string? userName = null)
        {
            var args = new Dictionary<string, object?> { ["userId"] = userId };
            Put(args, "userName", userName);
            return CallVoid("setUser", args);
        }

        public Task SetUserProperty(string name, object? value)
        {
            var args = new Dictionary<string, object?> { ["name"] = name, ["value"] = value };
            return CallVoid("setUserProperty", args);
        }

        public Task SetUserProperties(IDictionary<string, object> properties) =>
            CallVoid("setUserProperties", new Dictionary<string, object?> { ["properties"] = properties });

        public Task ClearUserProperties(IList<string>? propertyNames = null)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "propertyNames", propertyNames);
            return CallVoid("clearUserProperties", args);
        }

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

        // The desktop engine resolves the launch link itself, so there is no absent
        // initial-link probe to complete — the shared deferred item across bindings.
        public Task CompleteInitialDeepLink() => Task.CompletedTask;

        public async Task<AttriaxDeepLinkEvent?> RecordDeepLink(Uri uri, IDictionary<string, object>? metadata = null, string source = "manual")
        {
            var args = Args(("uri", uri.ToString()), ("source", source));
            Put(args, "metadata", metadata);
            var token = await CallResult("recordDeepLink", args).ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        // Degraded: the C-ABI routes no blocking initial-link wait. Returns the cached
        // launch link if the engine already resolved one.
        public Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink() => GetInitialDeepLink();

        // Degraded: the C-ABI routes no blocking resolution wait; the adapter falls
        // back to the next resolved event on the deep-link stream.
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
            var args = Args(("receipt", receipt), ("test", test));
            Put(args, "provider", provider);
            Put(args, "environment", environment);
            Put(args, "productId", productId);
            Put(args, "transactionId", transactionId);
            var token = await CallResult("validateReceipt", args).ConfigureAwait(false);
            return AttriaxDesktopEngineMapper.ToReceiptResult(token as JObject);
        }

        // -----------------------------------------------------------------
        // Consent / toggles.
        // -----------------------------------------------------------------

        public Task SetGdprConsent(bool analytics, bool attribution, bool adEvents) =>
            CallVoid("setGdprConsent", new Dictionary<string, object?>
            {
                ["analytics"] = analytics,
                ["attribution"] = attribution,
                ["adEvents"] = adEvents,
            });

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
        // Apple seams — forwarded for fidelity but inert on the desktop engine.
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
            var args = Args(("fineValue", fineValue), ("lockWindow", lockWindow));
            if (coarseValue.HasValue)
            {
                args["coarseValue"] = CoarseToWire(coarseValue.Value);
            }

            var token = await CallResult("updateSkanConversionValue", args).ConfigureAwait(false);
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

            AttriaxDesktopEnginePlatform? self = null;
            try
            {
                if (userData != IntPtr.Zero)
                {
                    var gch = GCHandle.FromIntPtr(userData);
                    if (gch.IsAllocated)
                    {
                        self = gch.Target as AttriaxDesktopEnginePlatform;
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
                // Ownership of eventJson transferred to us; free it exactly once. If the
                // owner is gone (disposed) we cannot reach the free function — a benign
                // teardown-only leak (the process is tearing down anyway).
                self?._library?.FreeString(eventJson);
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

        private Task CallVoid(string method, IDictionary<string, object?>? args = null) =>
            _worker.Enqueue(() => DispatchRaw(method, args == null ? null : JsonConvert.SerializeObject(args)));

        private async Task<JToken?> CallResult(string method, IDictionary<string, object?>? args = null)
        {
            var json = await _worker
                .Enqueue(() => DispatchRaw(method, args == null ? null : JsonConvert.SerializeObject(args)))
                .ConfigureAwait(false);
            return UnwrapResult(json);
        }

        /// <summary>
        /// Marshals <paramref name="method"/> + <paramref name="argsJson"/> to the
        /// C-ABI, returns the decoded envelope string, and frees every native string
        /// (args in <c>finally</c>, the result via <c>attriax_free_string</c>).
        /// </summary>
        private string? DispatchRaw(string method, string? argsJson)
        {
            var library = _library ?? throw new InvalidOperationException("Attriax desktop engine is not initialized.");
            var handle = _handle;
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Attriax desktop engine is not initialized.");
            }

            IntPtr methodPtr = Utf8ToNative(method);
            IntPtr argsPtr = Utf8ToNative(argsJson ?? "{}");
            IntPtr resultPtr = IntPtr.Zero;
            try
            {
                resultPtr = library.Dispatch(handle, methodPtr, argsPtr);
                return Utf8FromNative(resultPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(methodPtr);
                Marshal.FreeHGlobal(argsPtr);
                if (resultPtr != IntPtr.Zero)
                {
                    library.FreeString(resultPtr);
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
                throw new AttriaxDesktopDispatchException(error);
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

        private static Dictionary<string, object?> Args(params (string Key, object? Value)[] entries)
        {
            var map = new Dictionary<string, object?>();
            foreach (var (key, value) in entries)
            {
                map[key] = value;
            }

            return map;
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

        // -----------------------------------------------------------------
        // Dynamically-loaded native library (Windows LoadLibraryEx / Unix dlopen).
        // -----------------------------------------------------------------

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateDelegate(IntPtr configJson, IntPtr dataDir);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DispatchDelegate(IntPtr handle, IntPtr method, IntPtr argsJson);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void RegisterCallbackDelegate(IntPtr handle, IntPtr callback, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeStringDelegate(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DestroyDelegate(IntPtr handle);

        private sealed class NativeLibrary : IDisposable
        {
            private readonly CreateDelegate _create;
            private readonly DispatchDelegate _dispatch;
            private readonly RegisterCallbackDelegate _registerCallback;
            private readonly FreeStringDelegate _freeString;
            private readonly DestroyDelegate _destroy;
            private IntPtr _module;
            private readonly bool _isWindows;

            private NativeLibrary(IntPtr module, bool isWindows)
            {
                _module = module;
                _isWindows = isWindows;
                _create = GetFunction<CreateDelegate>("attriax_create");
                _dispatch = GetFunction<DispatchDelegate>("attriax_dispatch");
                _registerCallback = GetFunction<RegisterCallbackDelegate>("attriax_register_event_callback");
                _freeString = GetFunction<FreeStringDelegate>("attriax_free_string");
                _destroy = GetFunction<DestroyDelegate>("attriax_destroy");
            }

            public IntPtr Create(IntPtr configJson, IntPtr dataDir) => _create(configJson, dataDir);

            public IntPtr Dispatch(IntPtr handle, IntPtr method, IntPtr argsJson) => _dispatch(handle, method, argsJson);

            public void RegisterEventCallback(IntPtr handle, IntPtr callback, IntPtr userData) =>
                _registerCallback(handle, callback, userData);

            public void FreeString(IntPtr ptr) => _freeString(ptr);

            public void Destroy(IntPtr handle) => _destroy(handle);

            public static NativeLibrary Load()
            {
                var platform = Application.platform;
                var isWindows = platform == RuntimePlatform.WindowsPlayer ||
                                platform == RuntimePlatform.WindowsEditor;
                var isMac = platform == RuntimePlatform.OSXPlayer ||
                            platform == RuntimePlatform.OSXEditor;
                // macOS takes the Unix (dlopen) code path, just like Linux.
                var fileName = isWindows ? WindowsLibraryName : isMac ? MacLibraryName : LinuxLibraryName;

                IntPtr module = IntPtr.Zero;
                string? loadedFrom = null;
                foreach (var candidate in CandidatePaths(fileName, isWindows, isMac))
                {
                    if (!File.Exists(candidate))
                    {
                        continue;
                    }

                    module = OpenLibrary(candidate, isWindows);
                    if (module != IntPtr.Zero)
                    {
                        loadedFrom = candidate;
                        break;
                    }
                }

                if (module == IntPtr.Zero)
                {
                    // Last resort: let the OS loader resolve the bare name from its
                    // default search path (Unity's Plugins dir on standalone).
                    module = OpenLibrary(fileName, isWindows);
                    loadedFrom = fileName;
                }

                if (module == IntPtr.Zero)
                {
                    throw new DllNotFoundException(
                        $"Attriax could not load the native engine library '{fileName}'. " +
                        "Ensure it is bundled under Runtime/Plugins/x86_64 and enabled for this platform.");
                }

                Debug.Log($"[Attriax][Engine] Loaded native core from '{loadedFrom}'.");
                return new NativeLibrary(module, isWindows);
            }

            private static IEnumerable<string> CandidatePaths(string fileName, bool isWindows, bool isMac)
            {
                var dataPath = Application.dataPath;

                if (isMac)
                {
                    // Editor: the package-embedded universal (arm64 + x86_64) dylib.
                    yield return Path.GetFullPath(Path.Combine(
                        "Packages", "com.attriax.unity", "Runtime", "Plugins", "macOS", fileName));
                    // Standalone .app: Application.dataPath is
                    // <App>.app/Contents/Resources/Data, so Unity bundles native plugins
                    // into <App>.app/Contents/PlugIns/.
                    var contents = Path.GetDirectoryName(Path.GetDirectoryName(dataPath));
                    if (!string.IsNullOrEmpty(contents))
                    {
                        yield return Path.Combine(contents, "PlugIns", fileName);
                    }

                    yield return Path.Combine(dataPath, "Plugins", fileName);
                    yield break;
                }

                // Standalone player: <App>_Data/Plugins/x86_64/<lib>.
                yield return Path.Combine(dataPath, "Plugins", "x86_64", fileName);
                yield return Path.Combine(dataPath, "Plugins", fileName);
                // Next to the executable (belt-and-suspenders post-build copy target).
                var exeDir = Path.GetDirectoryName(dataPath);
                if (!string.IsNullOrEmpty(exeDir))
                {
                    yield return Path.Combine(exeDir, fileName);
                }

                // Editor: the package-embedded plugin.
                var platformDir = isWindows ? "Windows" : "Linux";
                yield return Path.GetFullPath(Path.Combine(
                    "Packages", "com.attriax.unity", "Runtime", "Plugins", "x86_64", platformDir, fileName));
                yield return Path.GetFullPath(Path.Combine(
                    "Packages", "com.attriax.unity", "Runtime", "Plugins", "x86_64", fileName));
            }

            private static IntPtr OpenLibrary(string path, bool isWindows)
            {
                try
                {
                    if (isWindows)
                    {
                        // LOAD_WITH_ALTERED_SEARCH_PATH so sibling DLLs (if any) resolve
                        // relative to the loaded module rather than the process dir.
                        return LoadLibraryExW(path, IntPtr.Zero, LoadWithAlteredSearchPath);
                    }

                    return UnixDlopen(path);
                }
                catch (DllNotFoundException)
                {
                    return IntPtr.Zero;
                }
            }

            private T GetFunction<T>(string name)
                where T : Delegate
            {
                var symbol = _isWindows ? GetProcAddress(_module, name) : UnixDlsym(_module, name);
                if (symbol == IntPtr.Zero)
                {
                    throw new EntryPointNotFoundException(
                        $"Attriax native engine is missing the export '{name}'.");
                }

                return Marshal.GetDelegateForFunctionPointer<T>(symbol);
            }

            public void Dispose()
            {
                var module = _module;
                _module = IntPtr.Zero;
                if (module == IntPtr.Zero)
                {
                    return;
                }

                try
                {
                    if (_isWindows)
                    {
                        FreeLibrary(module);
                    }
                    else
                    {
                        UnixDlclose(module);
                    }
                }
                catch
                {
                    // Unloading is best-effort.
                }
            }

            // -- Windows (kernel32) --

            private const uint LoadWithAlteredSearchPath = 0x00000008;

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr LoadLibraryExW(string lpLibFileName, IntPtr hFile, uint dwFlags);

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true)]
            private static extern bool FreeLibrary(IntPtr hModule);

            // -- Unix (libdl); try libdl.so.2 first, then libdl --

            private const int RtldNow = 2;

            // Three tiers: Linux resolves via libdl.so.2 (glibc) or libdl; macOS has no
            // physical libdl, so it falls through to the process-internal dl* symbols
            // (dlopen/dlsym/dlclose live in libSystem, reachable via __Internal in both
            // the Mono Editor and an IL2CPP standalone player).
            private static IntPtr UnixDlopen(string path)
            {
                try
                {
                    return dlopen_2(path, RtldNow);
                }
                catch (DllNotFoundException)
                {
                    try
                    {
                        return dlopen_1(path, RtldNow);
                    }
                    catch (DllNotFoundException)
                    {
                        return dlopen_sys(path, RtldNow);
                    }
                }
            }

            private static IntPtr UnixDlsym(IntPtr handle, string symbol)
            {
                try
                {
                    return dlsym_2(handle, symbol);
                }
                catch (DllNotFoundException)
                {
                    try
                    {
                        return dlsym_1(handle, symbol);
                    }
                    catch (DllNotFoundException)
                    {
                        return dlsym_sys(handle, symbol);
                    }
                }
            }

            private static int UnixDlclose(IntPtr handle)
            {
                try
                {
                    return dlclose_2(handle);
                }
                catch (DllNotFoundException)
                {
                    try
                    {
                        return dlclose_1(handle);
                    }
                    catch (DllNotFoundException)
                    {
                        return dlclose_sys(handle);
                    }
                }
            }

            [DllImport("libdl.so.2", EntryPoint = "dlopen", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlopen_2(string fileName, int flags);

            [DllImport("libdl.so.2", EntryPoint = "dlsym", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlsym_2(IntPtr handle, string symbol);

            [DllImport("libdl.so.2", EntryPoint = "dlclose")]
            private static extern int dlclose_2(IntPtr handle);

            [DllImport("libdl", EntryPoint = "dlopen", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlopen_1(string fileName, int flags);

            [DllImport("libdl", EntryPoint = "dlsym", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlsym_1(IntPtr handle, string symbol);

            [DllImport("libdl", EntryPoint = "dlclose")]
            private static extern int dlclose_1(IntPtr handle);

            // Process-internal (macOS: libSystem's dl* symbols; reachable via __Internal).
            [DllImport("__Internal", EntryPoint = "dlopen", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlopen_sys(string fileName, int flags);

            [DllImport("__Internal", EntryPoint = "dlsym", CharSet = CharSet.Ansi, BestFitMapping = false)]
            private static extern IntPtr dlsym_sys(IntPtr handle, string symbol);

            [DllImport("__Internal", EntryPoint = "dlclose")]
            private static extern int dlclose_sys(IntPtr handle);
        }

        /// <summary>
        /// A single background worker thread. All native engine interaction runs here
        /// (never Unity's main thread), mirroring the Android binding's single-thread
        /// executor and the Flutter desktop isolate dispatch discipline.
        /// </summary>
        private sealed class AttriaxDesktopEngineWorker : IDisposable
        {
            private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
            private readonly Thread _thread;

            public AttriaxDesktopEngineWorker()
            {
                _thread = new Thread(Run) { IsBackground = true, Name = "attriax-desktop-engine" };
                _thread.Start();
            }

            public Task<T> Enqueue<T>(Func<T> func)
            {
                var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (_queue.IsAddingCompleted)
                {
                    tcs.SetException(new ObjectDisposedException(nameof(AttriaxDesktopEngineWorker)));
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
    internal sealed class AttriaxDesktopDispatchException : Exception
    {
        public AttriaxDesktopDispatchException(string error)
            : base($"Attriax desktop engine dispatch failed: {error}")
        {
        }
    }
}
#endif
