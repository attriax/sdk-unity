#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attriax.Unity.Internal.Engine;

namespace Attriax.Unity.Tests
{
    /// <summary>
    /// A single command forwarded to <see cref="FakeEnginePlatform"/>, captured as
    /// its wire method name plus the wire-keyed argument map. The keys mirror the
    /// Flutter <c>MethodChannel</c> / KMP C-ABI contract so assertions double as a
    /// wire-shape check.
    /// </summary>
    internal sealed class RecordedEngineCommand
    {
        public RecordedEngineCommand(string method, IReadOnlyDictionary<string, object?> args)
        {
            Method = method;
            Args = args;
        }

        public string Method { get; }

        public IReadOnlyDictionary<string, object?> Args { get; }

        public T? Arg<T>(string key)
        {
            return Args.TryGetValue(key, out var value) && value is T typed ? typed : default;
        }

        public bool HasArg(string key) => Args.ContainsKey(key);
    }

    /// <summary>
    /// In-memory <see cref="IAttriaxEnginePlatform"/> test double.
    /// </summary>
    /// <remarks>
    /// Records every command (method name + wire-keyed args) for assertion,
    /// returns configurable getter values, and lets tests raise the engine events.
    /// Optional/nullable arguments are omitted from the recorded map when null —
    /// mirroring the Flutter <c>MethodChannel</c> <c>?key</c> null-omission — so the
    /// CCPA non-null/omit semantics can be asserted directly.
    /// </remarks>
    internal sealed class FakeEnginePlatform : IAttriaxEnginePlatform
    {
        private readonly List<RecordedEngineCommand> _commands = new List<RecordedEngineCommand>();

        public IReadOnlyList<RecordedEngineCommand> Commands => _commands;

        public RecordedEngineCommand? LastCommand => _commands.Count == 0 ? null : _commands[_commands.Count - 1];

        public RecordedEngineCommand? CommandFor(string method) =>
            _commands.LastOrDefault(command => command.Method == method);

        public IReadOnlyList<RecordedEngineCommand> CommandsFor(string method) =>
            _commands.Where(command => command.Method == method).ToList();

        public void ClearRecordedCommands() => _commands.Clear();

        // -- Configurable getter values ---------------------------------------

        public string? DeviceIdValue { get; set; }
        public bool IsFirstLaunchValue { get; set; }
        public bool IsInitializedValue { get; set; }
        public AttriaxSdkSnapshot? SdkSnapshotValue { get; set; }
        public bool SdkEnabledValue { get; set; } = true;
        public bool EventTrackingEnabledValue { get; set; } = true;
        public bool AnonymousTrackingValue { get; set; } = true;
        public AttriaxSynchronizationState SynchronizationStateValue { get; set; } = AttriaxSynchronizationState.Initializing;
        public bool IsSynchronizedValue { get; set; }
        public AttriaxInstallReferrerDetails? OriginalInstallReferrerValue { get; set; }
        public AttriaxInstallReferrerDetails? ReinstallReferrerValue { get; set; }
        public string? RawInstallReferrerValue { get; set; }
        public AttriaxDeepLinkReferrerDetails? SessionReferrerValue { get; set; }
        public AttriaxDeepLinkReferrerDetails? LatestDeepLinkReferrerValue { get; set; }
        public AttriaxSkanState? SkanStateValue { get; set; }
        public AttriaxDeepLinkEvent? LatestDeepLinkValue { get; set; }
        public AttriaxDeepLinkEvent? InitialDeepLinkValue { get; set; }
        public AttriaxRawDeepLinkEvent? RawInitialDeepLinkValue { get; set; }
        public bool IsInitialDeepLinkResolvedValue { get; set; }
        public bool NeedsGdprConsentValue { get; set; }
        public bool IsWaitingForGdprConsentValue { get; set; }
        public AttriaxTrackingAuthorizationStatus TrackingAuthorizationStatusValue { get; set; } = AttriaxTrackingAuthorizationStatus.NotSupported;
        public bool? DoNotSellValue { get; set; }
        public string? UsPrivacyValue { get; set; }

        // -- Configurable command return values -------------------------------

        public AttriaxDeepLinkEvent? RecordDeepLinkResult { get; set; }
        public AttriaxDeepLinkEvent? WaitForInitialDeepLinkResult { get; set; }
        public AttriaxDeepLinkEvent? WaitForDeepLinkResolutionResult { get; set; }
        public AttriaxCreateDynamicLinkResult CreateDynamicLinkResult { get; set; } = new AttriaxCreateDynamicLinkResult();
        public AttriaxRevenueReceiptValidationResult ValidateReceiptResult { get; set; } = new AttriaxRevenueReceiptValidationResult();
        public AttriaxTrackingAuthorizationStatus RequestTrackingAuthorizationResult { get; set; } = AttriaxTrackingAuthorizationStatus.NotSupported;
        public AttriaxSkanUpdateResult UpdateSkanConversionValueResult { get; set; } =
            new AttriaxSkanUpdateResult { Status = AttriaxSkanUpdateStatus.NotSupported };

        // -- Lifecycle --------------------------------------------------------

        public Task InitializeAsync(AttriaxConfig config)
        {
            Record("initialize", new Dictionary<string, object?> { ["config"] = config.ToEngineArguments() });
            return Task.CompletedTask;
        }

        public Task Flush()
        {
            Record("flush");
            return Task.CompletedTask;
        }

        public Task Reset()
        {
            Record("reset");
            return Task.CompletedTask;
        }

        public Task Dispose()
        {
            Record("dispose");
            return Task.CompletedTask;
        }

        // -- Tracking ---------------------------------------------------------

        public Task RecordEvent(string name, IDictionary<string, object>? eventData = null, bool flushImmediately = false)
        {
            var args = Args(("name", name), ("flushImmediately", flushImmediately));
            Put(args, "eventData", eventData);
            Record("recordEvent", args);
            return Task.CompletedTask;
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
            Record("recordPageView", args);
            return Task.CompletedTask;
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
            Record("recordPurchase", args);
            return Task.CompletedTask;
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
            Record("recordRefund", args);
            return Task.CompletedTask;
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
            Record("recordAdRevenue", args);
            return Task.CompletedTask;
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
            Record("recordAdEvent", args);
            return Task.CompletedTask;
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
            Record("recordNotification", args);
            return Task.CompletedTask;
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
            Record("recordError", args);
            return Task.CompletedTask;
        }

        public Task SetUser(string? userId = null, string? userName = null)
        {
            var args = new Dictionary<string, object?> { ["userId"] = userId };
            Put(args, "userName", userName);
            Record("setUser", args);
            return Task.CompletedTask;
        }

        public Task SetUserProperty(string name, object? value)
        {
            Record("setUserProperty", new Dictionary<string, object?> { ["name"] = name, ["value"] = value });
            return Task.CompletedTask;
        }

        public Task SetUserProperties(IDictionary<string, object> properties)
        {
            Record("setUserProperties", new Dictionary<string, object?> { ["properties"] = properties });
            return Task.CompletedTask;
        }

        public Task ClearUserProperties(IList<string>? propertyNames = null)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "propertyNames", propertyNames);
            Record("clearUserProperties", args);
            return Task.CompletedTask;
        }

        public Task RegisterPushToken(AttriaxPushTokenProvider provider, string? token, IDictionary<string, object>? metadata = null)
        {
            var args = new Dictionary<string, object?>
            {
                ["provider"] = provider.ToWireValue(),
                ["token"] = token,
            };
            Put(args, "metadata", metadata);
            Record("registerPushToken", args);
            return Task.CompletedTask;
        }

        // -- Deep links -------------------------------------------------------

        public Task HandleIncomingLink(string uri, bool isInitialLink = false)
        {
            Record("handleIncomingLink", new Dictionary<string, object?> { ["uri"] = uri, ["isInitialLink"] = isInitialLink });
            return Task.CompletedTask;
        }

        public Task CompleteInitialDeepLink()
        {
            Record("completeInitialDeepLink");
            return Task.CompletedTask;
        }

        public Task<AttriaxDeepLinkEvent?> RecordDeepLink(Uri uri, IDictionary<string, object>? metadata = null, string source = "manual")
        {
            var args = Args(("uri", uri.ToString()), ("source", source));
            Put(args, "metadata", metadata);
            Record("recordDeepLink", args);
            return Task.FromResult(RecordDeepLinkResult);
        }

        public Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink()
        {
            Record("waitForInitialDeepLink");
            return Task.FromResult(WaitForInitialDeepLinkResult);
        }

        public Task<AttriaxDeepLinkEvent?> WaitForDeepLinkResolution(AttriaxRawDeepLinkEvent rawEvent)
        {
            Record("waitForDeepLinkResolution", new Dictionary<string, object?> { ["rawEvent"] = rawEvent });
            return Task.FromResult(WaitForDeepLinkResolutionResult);
        }

        public Task<AttriaxCreateDynamicLinkResult> CreateDynamicLink(AttriaxCreateDynamicLinkOptions options)
        {
            Record("createDynamicLink", new Dictionary<string, object?> { ["options"] = options });
            return Task.FromResult(CreateDynamicLinkResult);
        }

        // -- Receipt validation ----------------------------------------------

        public Task<AttriaxRevenueReceiptValidationResult> ValidateReceipt(
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
            Record("validateReceipt", args);
            return Task.FromResult(ValidateReceiptResult);
        }

        // -- Consent / toggles ------------------------------------------------

        public Task SetGdprConsent(bool analytics, bool attribution, bool adEvents)
        {
            Record("setGdprConsent", new Dictionary<string, object?>
            {
                ["analytics"] = analytics,
                ["attribution"] = attribution,
                ["adEvents"] = adEvents,
            });
            return Task.CompletedTask;
        }

        public Task SetGdprConsentNotRequired()
        {
            Record("setGdprConsentNotRequired");
            return Task.CompletedTask;
        }

        public Task ResetGdprConsent()
        {
            Record("resetGdprConsent");
            return Task.CompletedTask;
        }

        public Task RequestGdprDataErasure()
        {
            Record("requestGdprDataErasure");
            return Task.CompletedTask;
        }

        public Task SetAnonymousTracking(bool enabled)
        {
            Record("setAnonymousTracking", new Dictionary<string, object?> { ["enabled"] = enabled });
            return Task.CompletedTask;
        }

        public Task SetCcpaConsent(bool? doNotSell, string? usPrivacy)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "doNotSell", doNotSell);
            Put(args, "usPrivacy", usPrivacy);
            Record("setCcpaConsent", args);
            return Task.CompletedTask;
        }

        public Task SetSdkEnabled(bool enabled)
        {
            Record("setSdkEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });
            return Task.CompletedTask;
        }

        public Task SetEventTrackingEnabled(bool enabled)
        {
            Record("setEventTrackingEnabled", new Dictionary<string, object?> { ["enabled"] = enabled });
            return Task.CompletedTask;
        }

        // -- Apple seams ------------------------------------------------------

        public Task SubmitAsaToken(string token)
        {
            Record("submitAsaToken", new Dictionary<string, object?> { ["token"] = token });
            return Task.CompletedTask;
        }

        public Task SetTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status)
        {
            Record("setTrackingAuthorizationStatus", new Dictionary<string, object?> { ["status"] = status });
            return Task.CompletedTask;
        }

        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorization(int? timeoutMs = null)
        {
            var args = new Dictionary<string, object?>();
            Put(args, "timeoutMs", timeoutMs);
            Record("requestTrackingAuthorization", args);
            return Task.FromResult(RequestTrackingAuthorizationResult);
        }

        public Task<AttriaxSkanUpdateResult> UpdateSkanConversionValue(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue = null,
            bool lockWindow = false)
        {
            var args = Args(("fineValue", fineValue), ("lockWindow", lockWindow));
            if (coarseValue.HasValue)
            {
                args["coarseValue"] = coarseValue.Value;
            }

            Record("updateSkanConversionValue", args);
            return Task.FromResult(UpdateSkanConversionValueResult);
        }

        // -- Getters ----------------------------------------------------------

        public Task<string?> GetDeviceId() => Task.FromResult(DeviceIdValue);

        public Task<bool> GetIsFirstLaunch() => Task.FromResult(IsFirstLaunchValue);

        public Task<bool> GetIsInitialized() => Task.FromResult(IsInitializedValue);

        public Task<AttriaxSdkSnapshot?> GetSdkSnapshot() => Task.FromResult(SdkSnapshotValue);

        public Task<bool> GetSdkEnabled() => Task.FromResult(SdkEnabledValue);

        public Task<bool> GetEventTrackingEnabled() => Task.FromResult(EventTrackingEnabledValue);

        public Task<bool> GetAnonymousTracking() => Task.FromResult(AnonymousTrackingValue);

        public Task<AttriaxSynchronizationState> GetSynchronizationState() => Task.FromResult(SynchronizationStateValue);

        public Task<bool> GetIsSynchronized() => Task.FromResult(IsSynchronizedValue);

        public Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrer(int? timeoutMs = null) =>
            Task.FromResult(OriginalInstallReferrerValue);

        public Task<AttriaxInstallReferrerDetails?> GetReinstallReferrer(int? timeoutMs = null) =>
            Task.FromResult(ReinstallReferrerValue);

        public Task<string?> GetRawInstallReferrer(int? timeoutMs = null) => Task.FromResult(RawInstallReferrerValue);

        public Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrer(int? timeoutMs = null) =>
            Task.FromResult(SessionReferrerValue);

        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrer(int? timeoutMs = null) =>
            Task.FromResult(LatestDeepLinkReferrerValue);

        public Task<AttriaxSkanState?> GetSkanState() => Task.FromResult(SkanStateValue);

        public Task<AttriaxDeepLinkEvent?> GetLatestDeepLink() => Task.FromResult(LatestDeepLinkValue);

        public Task<AttriaxDeepLinkEvent?> GetInitialDeepLink() => Task.FromResult(InitialDeepLinkValue);

        public Task<AttriaxRawDeepLinkEvent?> GetRawInitialDeepLink() => Task.FromResult(RawInitialDeepLinkValue);

        public Task<bool> GetIsInitialDeepLinkResolved() => Task.FromResult(IsInitialDeepLinkResolvedValue);

        public Task<bool> NeedsGdprConsent(bool localOnly = false)
        {
            Record("needsGdprConsent", new Dictionary<string, object?> { ["localOnly"] = localOnly });
            return Task.FromResult(NeedsGdprConsentValue);
        }

        public Task<bool> GetIsWaitingForGdprConsent() => Task.FromResult(IsWaitingForGdprConsentValue);

        public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatus() =>
            Task.FromResult(TrackingAuthorizationStatusValue);

        public Task<bool?> GetDoNotSell() => Task.FromResult(DoNotSellValue);

        public Task<string?> GetUsPrivacy() => Task.FromResult(UsPrivacyValue);

        // -- Events -----------------------------------------------------------

        public event Action<AttriaxSynchronizationState> SynchronizationStateChanged = delegate { };
        public event Action<AttriaxDeepLinkEvent> DeepLinkResolved = delegate { };
        public event Action<AttriaxRawDeepLinkEvent> RawDeepLinkReceived = delegate { };
        public event Action<AttriaxInitialDeepLinkResolution> InitialDeepLinkResolved = delegate { };

        public void RaiseSynchronizationStateChanged(AttriaxSynchronizationState state) =>
            SynchronizationStateChanged?.Invoke(state);

        public void RaiseDeepLinkResolved(AttriaxDeepLinkEvent deepLink) => DeepLinkResolved?.Invoke(deepLink);

        public void RaiseRawDeepLinkReceived(AttriaxRawDeepLinkEvent rawEvent) => RawDeepLinkReceived?.Invoke(rawEvent);

        public void RaiseInitialDeepLinkResolved(AttriaxInitialDeepLinkResolution resolution) =>
            InitialDeepLinkResolved?.Invoke(resolution);

        // -- Helpers ----------------------------------------------------------

        private void Record(string method) => Record(method, new Dictionary<string, object?>());

        private void Record(string method, IReadOnlyDictionary<string, object?> args) =>
            _commands.Add(new RecordedEngineCommand(method, args));

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
    }
}
