#nullable enable
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Attriax.Unity.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Android <see cref="IAttriaxEnginePlatform"/> implementation (Phase 2 re-wrap).
    /// Drives the KMP engine — shipped as the <c>com.attriax:core</c> AAR — through the
    /// <c>com.attriax.unity.AttriaxUnityBridge</c> Kotlin shim via
    /// <see cref="AndroidJavaObject"/>, and receives engine events over an
    /// <see cref="AndroidJavaProxy"/> implementing
    /// <c>com.attriax.unity.AttriaxUnityBridgeListener</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the JNI twin of the iOS C-ABI binding (<c>AttriaxIosEngineNative</c>) and
    /// mirrors the Flutter Android plugin (<c>AttriaxAndroidPlugin.kt</c>) 1:1: commands
    /// serialize their wire-keyed argument map to JSON and dispatch on a dedicated
    /// JNI-attached worker thread (never Unity's main thread); the bridge returns
    /// <c>null</c> for void commands or a <c>{"result": …}</c> JSON envelope for
    /// value-returning ones. Engine events fan in on the KMP engine's threads through the
    /// proxy and are re-raised on the Unity main thread via
    /// <see cref="AttriaxLifecycleDispatcher.PostToMainThread"/> (fire-and-forget — never
    /// the blocking marshal; see the DebugLog main-thread deadlock lesson).
    /// </para>
    /// <para>
    /// ⚠️ Compiled only into Android player builds (<c>UNITY_ANDROID &amp;&amp;
    /// !UNITY_EDITOR</c>); the Unity Editor on this project cannot open (PackageManager
    /// forbidden-folder crash), so this binding is contract-complete + reviewed but not
    /// Editor/device-verified here — the same honesty caveat the iOS binding carries.
    /// </para>
    /// </remarks>
    internal sealed class AttriaxAndroidEnginePlatform : IAttriaxEnginePlatform
    {
        private const string BridgeClassName = "com.attriax.unity.AttriaxUnityBridge";
        private const string ListenerInterface = "com.attriax.unity.AttriaxUnityBridgeListener";

        private readonly AttriaxAndroidEngineWorker _worker = new AttriaxAndroidEngineWorker();
        private readonly EngineEventListener _listener;
        private volatile AndroidJavaObject? _bridge;

        public AttriaxAndroidEnginePlatform()
        {
            _listener = new EngineEventListener(this);
        }

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
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var bridgeClass = new AndroidJavaClass(BridgeClassName);
                _bridge = bridgeClass.CallStatic<AndroidJavaObject>(
                    "create", activity, configJson, _listener);
            });
        }

        public Task Flush() => CallVoid("flush");

        public Task Reset() => CallVoid("reset");

        public Task Dispose()
        {
            var bridge = _bridge;
            _bridge = null;
            return _worker.Enqueue(() =>
            {
                if (bridge != null)
                {
                    try
                    {
                        bridge.Call("destroy");
                    }
                    finally
                    {
                        bridge.Dispose();
                    }
                }
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
            var args = Args(("eventName", eventName), ("flushImmediately", flushImmediately));
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
            var args = new Dictionary<string, object?>
            {
                ["provider"] = provider.ToWireValue(),
                ["token"] = token,
            };
            Put(args, "metadata", metadata);
            return CallVoid("registerPushToken", args);
        }

        // -----------------------------------------------------------------
        // Deep links.
        // -----------------------------------------------------------------

        public Task HandleIncomingLink(string uri, bool isInitialLink = false) =>
            CallVoid("handleIncomingLink", new Dictionary<string, object?> { ["uri"] = uri, ["isInitialLink"] = isInitialLink });

        // KMP's androidMain resolves the initial-link probe automatically
        // (AttriaxDeepLinks.completeInitialLinkIfAbsent is internal + auto-invoked), so
        // the wrapper has nothing to forward — the shared deferred item across bindings.
        public Task CompleteInitialDeepLink() => Task.CompletedTask;

        public async Task<AttriaxDeepLinkEvent?> RecordDeepLink(Uri uri, IDictionary<string, object>? metadata = null, string source = "manual")
        {
            var args = Args(("uri", uri.ToString()), ("source", source));
            Put(args, "metadata", metadata);
            var token = await CallResult("recordDeepLink", args).ConfigureAwait(false);
            return AttriaxAndroidEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        public async Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink()
        {
            var token = await CallResult("waitForInitialDeepLink").ConfigureAwait(false);
            return AttriaxAndroidEngineMapper.ToDeepLinkEvent(token as JObject);
        }

        public async Task<AttriaxDeepLinkEvent?> WaitForDeepLinkResolution(AttriaxRawDeepLinkEvent rawEvent)
        {
            var raw = new Dictionary<string, object?>
            {
                ["uri"] = rawEvent.Uri.ToString(),
                ["receivedAtMs"] = rawEvent.ReceivedAt.ToUnixTimeMilliseconds(),
                ["isInitial"] = rawEvent.IsInitial,
            };
            var args = new Dictionary<string, object?> { ["rawEvent"] = raw };
            var token = await CallResult("waitForDeepLinkResolution", args).ConfigureAwait(false);
            return AttriaxAndroidEngineMapper.ToDeepLinkEvent(token as JObject);
        }

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
            return AttriaxAndroidEngineMapper.ToCreateDynamicLinkResult(token as JObject);
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
            return AttriaxAndroidEngineMapper.ToReceiptResult(token as JObject);
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

        public Task SetCcpaConsent(bool? doNotSell, string? usPrivacy)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "doNotSell", doNotSell);
            Put(args, "usPrivacy", usPrivacy);
            return CallVoid("setCcpaConsent", args);
        }

        public Task SetSdkEnabled(bool enabled) =>
            CallVoid("setSdkEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });

        public Task SetEventTrackingEnabled(bool enabled) =>
            CallVoid("setEventTrackingEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });

        // -----------------------------------------------------------------
        // Apple seams.
        // -----------------------------------------------------------------

        public Task SubmitAsaToken(string token) =>
            CallVoid("submitAsaToken", new Dictionary<string, object?> { ["token"] = token });

        public Task SetTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status) =>
            CallVoid("setTrackingAuthorizationStatus", new Dictionary<string, object?> { ["status"] = AttToWire(status) });

        public async Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorization(int? timeoutMs = null)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "timeoutMs", timeoutMs);
            var token = await CallResult("requestTrackingAuthorization", args).ConfigureAwait(false);
            return AttriaxAndroidEngineMapper.ToTrackingAuthStatus(token?.Value<string>());
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
            return AttriaxAndroidEngineMapper.ToSkanUpdateResult(token as JObject);
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
            AttriaxAndroidEngineMapper.ToSdkSnapshot(await CallResult("getSdkSnapshot").ConfigureAwait(false) as JObject);

        public async Task<bool> GetSdkEnabled() =>
            (await CallResult("getSdkEnabled").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetEventTrackingEnabled() =>
            (await CallResult("getEventTrackingEnabled").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetAnonymousTracking() =>
            (await CallResult("getAnonymousTracking").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxSynchronizationState> GetSynchronizationState() =>
            AttriaxAndroidEngineMapper.ToSyncState((await CallResult("getSynchronizationState").ConfigureAwait(false))?.Value<string>());

        public async Task<bool> GetIsSynchronized() =>
            (await CallResult("getIsSynchronized").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrer(int? timeoutMs = null) =>
            AttriaxAndroidEngineMapper.ToInstallReferrer(await CallResult("getOriginalInstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxInstallReferrerDetails?> GetReinstallReferrer(int? timeoutMs = null) =>
            AttriaxAndroidEngineMapper.ToInstallReferrer(await CallResult("getReinstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<string?> GetRawInstallReferrer(int? timeoutMs = null) =>
            (await CallResult("getRawInstallReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false))?.Value<string>();

        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrer(int? timeoutMs = null) =>
            AttriaxAndroidEngineMapper.ToDeepLinkReferrer(await CallResult("getSessionReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrer(int? timeoutMs = null) =>
            AttriaxAndroidEngineMapper.ToDeepLinkReferrer(await CallResult("getLatestDeepLinkReferrer", TimeoutArgs(timeoutMs)).ConfigureAwait(false) as JObject);

        public async Task<AttriaxSkanState?> GetSkanState() =>
            AttriaxAndroidEngineMapper.ToSkanState(await CallResult("getSkanState").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkEvent?> GetLatestDeepLink() =>
            AttriaxAndroidEngineMapper.ToDeepLinkEvent(await CallResult("getLatestDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxDeepLinkEvent?> GetInitialDeepLink() =>
            AttriaxAndroidEngineMapper.ToDeepLinkEvent(await CallResult("getInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<AttriaxRawDeepLinkEvent?> GetRawInitialDeepLink() =>
            AttriaxAndroidEngineMapper.ToRawDeepLinkEvent(await CallResult("getRawInitialDeepLink").ConfigureAwait(false) as JObject);

        public async Task<bool> GetIsInitialDeepLinkResolved() =>
            (await CallResult("getIsInitialDeepLinkResolved").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> NeedsGdprConsent(bool localOnly = false) =>
            (await CallResult("needsGdprConsent", new Dictionary<string, object?> { ["localOnly"] = localOnly }).ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<bool> GetIsWaitingForGdprConsent() =>
            (await CallResult("getIsWaitingForGdprConsent").ConfigureAwait(false))?.Value<bool>() ?? false;

        public async Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatus() =>
            AttriaxAndroidEngineMapper.ToTrackingAuthStatus((await CallResult("getTrackingAuthorizationStatus").ConfigureAwait(false))?.Value<string>());

        public async Task<bool?> GetDoNotSell()
        {
            var token = await CallResult("getDoNotSell").ConfigureAwait(false);
            return token == null ? (bool?)null : token.Value<bool>();
        }

        public async Task<string?> GetUsPrivacy() =>
            (await CallResult("getUsPrivacy").ConfigureAwait(false))?.Value<string>();

        // -----------------------------------------------------------------
        // Engine-event delivery (from the AndroidJavaProxy, off the main thread).
        // -----------------------------------------------------------------

        private void OnSyncState(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var state = AttriaxAndroidEngineMapper.ToSyncState(name);
            AttriaxLifecycleDispatcher.PostToMainThread(() => SynchronizationStateChanged?.Invoke(state));
        }

        private void OnDeepLink(string? json)
        {
            var evt = ParseObject(json, AttriaxAndroidEngineMapper.ToDeepLinkEvent);
            if (evt == null)
            {
                return;
            }

            AttriaxLifecycleDispatcher.PostToMainThread(() => DeepLinkResolved?.Invoke(evt));
        }

        private void OnRawDeepLink(string? json)
        {
            var evt = ParseObject(json, AttriaxAndroidEngineMapper.ToRawDeepLinkEvent);
            if (evt == null)
            {
                return;
            }

            AttriaxLifecycleDispatcher.PostToMainThread(() => RawDeepLinkReceived?.Invoke(evt));
        }

        private static T? ParseObject<T>(string? json, Func<JObject?, T?> map)
            where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return map(JObject.Parse(json));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // -----------------------------------------------------------------
        // Dispatch plumbing.
        // -----------------------------------------------------------------

        private Task CallVoid(string method, IDictionary<string, object?>? args = null) =>
            _worker.Enqueue(() => Dispatch(method, args));

        private async Task<JToken?> CallResult(string method, IDictionary<string, object?>? args = null)
        {
            var json = await _worker.Enqueue(() => Dispatch(method, args)).ConfigureAwait(false);
            return UnwrapResult(json);
        }

        private string? Dispatch(string method, IDictionary<string, object?>? args)
        {
            var bridge = _bridge
                ?? throw new InvalidOperationException("Attriax Android engine is not initialized.");
            var argsJson = args == null ? null : JsonConvert.SerializeObject(args);
            return bridge.Call<string>("dispatch", method, argsJson);
        }

        private static JToken? UnwrapResult(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var envelope = JObject.Parse(json);
            var token = envelope["result"];
            return token == null || token.Type == JTokenType.Null ? null : token;
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

        /// <summary>
        /// <see cref="AndroidJavaProxy"/> for <c>AttriaxUnityBridgeListener</c>. Engine
        /// callbacks arrive on the KMP engine's threads; each carries a single Java
        /// <c>String</c> which is forwarded to the owning platform (which re-marshals to
        /// the Unity main thread).
        /// </summary>
        private sealed class EngineEventListener : AndroidJavaProxy
        {
            private readonly AttriaxAndroidEnginePlatform _owner;

            public EngineEventListener(AttriaxAndroidEnginePlatform owner)
                : base(ListenerInterface)
            {
                _owner = owner;
            }

            public override AndroidJavaObject Invoke(string methodName, AndroidJavaObject[] javaArgs)
            {
                var payload = javaArgs != null && javaArgs.Length > 0 && javaArgs[0] != null
                    ? javaArgs[0].Call<string>("toString")
                    : null;

                switch (methodName)
                {
                    case "onSyncState":
                        _owner.OnSyncState(payload);
                        break;
                    case "onDeepLink":
                        _owner.OnDeepLink(payload);
                        break;
                    case "onRawDeepLink":
                        _owner.OnRawDeepLink(payload);
                        break;
                }

                // A void Java interface method; returning null is the AndroidJavaProxy
                // convention for "no return value".
                return null!;
            }
        }

        /// <summary>
        /// A single JNI-attached worker thread. All engine interaction runs here (never
        /// Unity's main thread), mirroring the Flutter Android plugin's single
        /// <c>Executors.newSingleThreadExecutor</c>; the thread stays attached to the JVM
        /// for its lifetime so repeated attach/detach is avoided.
        /// </summary>
        private sealed class AttriaxAndroidEngineWorker : IDisposable
        {
            private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
            private readonly Thread _thread;

            public AttriaxAndroidEngineWorker()
            {
                _thread = new Thread(Run) { IsBackground = true, Name = "attriax-engine" };
                _thread.Start();
            }

            public Task<T> Enqueue<T>(Func<T> func)
            {
                var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (_queue.IsAddingCompleted)
                {
                    tcs.SetException(new ObjectDisposedException(nameof(AttriaxAndroidEngineWorker)));
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
                AndroidJNI.AttachCurrentThread();
                try
                {
                    foreach (var action in _queue.GetConsumingEnumerable())
                    {
                        action();
                    }
                }
                finally
                {
                    AndroidJNI.DetachCurrentThread();
                }
            }
        }
    }
}
#endif
