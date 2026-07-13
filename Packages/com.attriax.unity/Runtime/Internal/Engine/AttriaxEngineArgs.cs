#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// The single, transport-independent source of the engine command <b>argument</b>
    /// wire shapes. Every binding (Android JNI, desktop/iOS C-ABI, WebGL jslib) is a
    /// string boundary — the argument map is <c>JsonConvert.SerializeObject</c>-d to
    /// JSON before it crosses — so a Newtonsoft-annotated DTO reproduces today's exact
    /// wire declaratively and is marshalled once here instead of hand-built per
    /// transport.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DTOs replace the byte-for-byte-replicated <c>Args(...)</c> / <c>Put(...)</c>
    /// blocks that used to live in every platform file. Two Newtonsoft behaviours
    /// reproduce the old hand marshalling exactly:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Null omission.</b> Fields the old code filled with <c>Put</c> (which
    /// skipped nulls) carry <see cref="NullValueHandling.Ignore"/>, so a <c>null</c>
    /// is omitted from the wire. Fields the old code assigned <i>directly</i> — which
    /// serialized an explicit <c>null</c> (e.g. <c>setUser</c>'s <c>userId</c>,
    /// <c>setUserProperty</c>'s <c>value</c>, both of which clear state by sending
    /// <c>null</c>) — deliberately keep the default include behaviour so the explicit
    /// <c>null</c> still reaches the engine.</item>
    /// <item><b>Default serializer settings.</b> The DTOs are serialized with
    /// <c>JsonConvert.SerializeObject(dto)</c> (default settings, compact, null
    /// included), matching the previous <c>SerializeObject(argsDictionary)</c> call.
    /// The DTOs carry no enums — every reserved slug / status / coarse value is still
    /// lowered to its wire string by the transport before it reaches the DTO — so no
    /// enum converter is involved and the output is identical.</item>
    /// </list>
    /// <para>
    /// Only the commands whose argument shape is <b>identical</b> across all bindings
    /// live here. The genuinely per-transport commands (push-token registration and
    /// CCPA, which some bindings split into separate setters; dynamic-link creation,
    /// which sdk-js nests; the deep-link resolution wait) stay hand-built in their
    /// platform file. <c>recordAdEvent</c> is shared: it differs only in the reserved
    /// event-name key (<c>type</c> on the C-ABI / jslib, <c>eventName</c> on the JNI
    /// bridge), so the DTO exposes both and the transport sets the one it needs.
    /// </para>
    /// </remarks>
    internal static class AttriaxEngineArgs
    {
        public static RecordEventArgs RecordEvent(
            string name,
            IDictionary<string, object>? eventData,
            bool flushImmediately) =>
            new RecordEventArgs
            {
                Name = name,
                EventData = eventData,
                FlushImmediately = flushImmediately,
            };

        public static RecordPageViewArgs RecordPageView(
            string pageName,
            string? pageClass,
            string? pageTitle,
            string? previousPageName,
            IDictionary<string, object>? parameters,
            string source,
            bool flushImmediately) =>
            new RecordPageViewArgs
            {
                PageName = pageName,
                Source = source,
                FlushImmediately = flushImmediately,
                PageClass = pageClass,
                PageTitle = pageTitle,
                PreviousPageName = previousPageName,
                Parameters = parameters,
            };

        public static RecordPurchaseArgs RecordPurchase(
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
            bool flushImmediately) =>
            new RecordPurchaseArgs
            {
                Revenue = revenue,
                Currency = currency,
                RevenueInMicros = revenueInMicros,
                Quantity = quantity,
                FlushImmediately = flushImmediately,
                PurchaseType = purchaseType,
                ProductId = productId,
                TransactionId = transactionId,
                OriginalTransactionId = originalTransactionId,
                ValidationProvider = validationProvider,
                ValidationEnvironment = validationEnvironment,
                PurchaseToken = purchaseToken,
                ReceiptData = receiptData,
                SignedPayload = signedPayload,
                ReceiptSignature = receiptSignature,
                IsRenewal = isRenewal,
                Store = store,
                PackageName = packageName,
                Voided = voided,
                Test = test,
                ValidationId = validationId,
                Metadata = metadata,
            };

        public static RecordRefundArgs RecordRefund(
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
            IDictionary<string, object>? metadata,
            bool flushImmediately) =>
            new RecordRefundArgs
            {
                Revenue = revenue,
                Currency = currency,
                RevenueInMicros = revenueInMicros,
                Quantity = quantity,
                FlushImmediately = flushImmediately,
                PurchaseType = purchaseType,
                ProductId = productId,
                TransactionId = transactionId,
                OriginalTransactionId = originalTransactionId,
                Store = store,
                PackageName = packageName,
                Voided = voided,
                Test = test,
                Reason = reason,
                Metadata = metadata,
            };

        public static RecordAdRevenueArgs RecordAdRevenue(
            double revenue,
            string currency,
            bool revenueInMicros,
            string? adNetwork,
            string? adFormat,
            string? adType,
            string? adPlacement,
            bool? test,
            IDictionary<string, object>? metadata,
            bool flushImmediately) =>
            new RecordAdRevenueArgs
            {
                Revenue = revenue,
                Currency = currency,
                RevenueInMicros = revenueInMicros,
                FlushImmediately = flushImmediately,
                AdNetwork = adNetwork,
                AdFormat = adFormat,
                AdType = adType,
                AdPlacement = adPlacement,
                Test = test,
                Metadata = metadata,
            };

        /// <summary>
        /// Builds the shared <c>recordAdEvent</c> arguments. The reserved event name
        /// crosses under <paramref name="type"/> on the C-ABI / jslib bindings and
        /// under <paramref name="eventName"/> on the JNI bridge; the caller supplies
        /// exactly one and leaves the other <c>null</c> (so it is omitted).
        /// </summary>
        public static RecordAdEventArgs RecordAdEvent(
            string? type,
            string? eventName,
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
            IDictionary<string, object>? metadata,
            bool flushImmediately) =>
            new RecordAdEventArgs
            {
                Type = type,
                EventName = eventName,
                FlushImmediately = flushImmediately,
                AdNetwork = adNetwork,
                MediationNetwork = mediationNetwork,
                AdUnitId = adUnitId,
                AdPlacement = adPlacement,
                AdFormat = adFormat,
                AdType = adType,
                FailureReason = failureReason,
                LoadLatencyMs = loadLatencyMs,
                RewardType = rewardType,
                RewardAmount = rewardAmount,
                Test = test,
                Metadata = metadata,
            };

        public static RecordNotificationArgs RecordNotification(
            string type,
            string notificationId,
            string? linkId,
            string? campaignId,
            string? title,
            string? source,
            IDictionary<string, object>? payload,
            IDictionary<string, object>? metadata,
            bool flushImmediately) =>
            new RecordNotificationArgs
            {
                Type = type,
                NotificationId = notificationId,
                FlushImmediately = flushImmediately,
                LinkId = linkId,
                CampaignId = campaignId,
                Title = title,
                Source = source,
                Payload = payload,
                Metadata = metadata,
            };

        public static RecordErrorArgs RecordError(
            string message,
            string exceptionType,
            string? stackTrace,
            bool fatal,
            string source,
            string? reason,
            IDictionary<string, object>? metadata) =>
            new RecordErrorArgs
            {
                Message = message,
                ExceptionType = exceptionType,
                Fatal = fatal,
                Source = source,
                StackTrace = stackTrace,
                Reason = reason,
                Metadata = metadata,
            };

        public static SetUserArgs SetUser(string? userId, string? userName) =>
            new SetUserArgs { UserId = userId, UserName = userName };

        public static SetUserPropertyArgs SetUserProperty(string name, object? value) =>
            new SetUserPropertyArgs { Name = name, Value = value };

        public static SetUserPropertiesArgs SetUserProperties(IDictionary<string, object> properties) =>
            new SetUserPropertiesArgs { Properties = properties };

        public static ClearUserPropertiesArgs ClearUserProperties(IList<string>? propertyNames) =>
            new ClearUserPropertiesArgs { PropertyNames = propertyNames };

        public static HandleIncomingLinkArgs HandleIncomingLink(string uri, bool isInitialLink) =>
            new HandleIncomingLinkArgs { Uri = uri, IsInitialLink = isInitialLink };

        public static RecordDeepLinkArgs RecordDeepLink(
            string uri,
            IDictionary<string, object>? metadata,
            string source) =>
            new RecordDeepLinkArgs { Uri = uri, Source = source, Metadata = metadata };

        public static ValidateReceiptArgs ValidateReceipt(
            string receipt,
            bool test,
            string? provider,
            string? environment,
            string? productId,
            string? transactionId) =>
            new ValidateReceiptArgs
            {
                Receipt = receipt,
                Test = test,
                Provider = provider,
                Environment = environment,
                ProductId = productId,
                TransactionId = transactionId,
            };

        public static SetGdprConsentArgs SetGdprConsent(bool analytics, bool attribution, bool adEvents) =>
            new SetGdprConsentArgs { Analytics = analytics, Attribution = attribution, AdEvents = adEvents };

        public static UpdateSkanConversionValueArgs UpdateSkanConversionValue(
            int fineValue,
            string? coarseValue,
            bool lockWindow) =>
            new UpdateSkanConversionValueArgs
            {
                FineValue = fineValue,
                LockWindow = lockWindow,
                CoarseValue = coarseValue,
            };
    }

    internal sealed class RecordEventArgs
    {
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; }

        [JsonProperty("eventData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? EventData { get; set; }
    }

    internal sealed class RecordPageViewArgs
    {
        [JsonProperty("pageName")] public string PageName { get; set; } = string.Empty;
        [JsonProperty("source")] public string Source { get; set; } = "manual";
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; }

        [JsonProperty("pageClass", NullValueHandling = NullValueHandling.Ignore)]
        public string? PageClass { get; set; }

        [JsonProperty("pageTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string? PageTitle { get; set; }

        [JsonProperty("previousPageName", NullValueHandling = NullValueHandling.Ignore)]
        public string? PreviousPageName { get; set; }

        [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Parameters { get; set; }
    }

    internal sealed class RecordPurchaseArgs
    {
        [JsonProperty("revenue")] public double Revenue { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; } = "USD";
        [JsonProperty("revenueInMicros")] public bool RevenueInMicros { get; set; }
        [JsonProperty("quantity")] public int Quantity { get; set; } = 1;
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; } = true;

        [JsonProperty("purchaseType", NullValueHandling = NullValueHandling.Ignore)]
        public string? PurchaseType { get; set; }

        [JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProductId { get; set; }

        [JsonProperty("transactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? TransactionId { get; set; }

        [JsonProperty("originalTransactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? OriginalTransactionId { get; set; }

        [JsonProperty("validationProvider", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValidationProvider { get; set; }

        [JsonProperty("validationEnvironment", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValidationEnvironment { get; set; }

        [JsonProperty("purchaseToken", NullValueHandling = NullValueHandling.Ignore)]
        public string? PurchaseToken { get; set; }

        [JsonProperty("receiptData", NullValueHandling = NullValueHandling.Ignore)]
        public string? ReceiptData { get; set; }

        [JsonProperty("signedPayload", NullValueHandling = NullValueHandling.Ignore)]
        public string? SignedPayload { get; set; }

        [JsonProperty("receiptSignature", NullValueHandling = NullValueHandling.Ignore)]
        public string? ReceiptSignature { get; set; }

        [JsonProperty("isRenewal", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRenewal { get; set; }

        [JsonProperty("store", NullValueHandling = NullValueHandling.Ignore)]
        public string? Store { get; set; }

        [JsonProperty("packageName", NullValueHandling = NullValueHandling.Ignore)]
        public string? PackageName { get; set; }

        [JsonProperty("voided", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Voided { get; set; }

        [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Test { get; set; }

        [JsonProperty("validationId", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValidationId { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class RecordRefundArgs
    {
        [JsonProperty("revenue")] public double Revenue { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; } = "USD";
        [JsonProperty("revenueInMicros")] public bool RevenueInMicros { get; set; }
        [JsonProperty("quantity")] public int Quantity { get; set; } = 1;
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; } = true;

        [JsonProperty("purchaseType", NullValueHandling = NullValueHandling.Ignore)]
        public string? PurchaseType { get; set; }

        [JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProductId { get; set; }

        [JsonProperty("transactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? TransactionId { get; set; }

        [JsonProperty("originalTransactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? OriginalTransactionId { get; set; }

        [JsonProperty("store", NullValueHandling = NullValueHandling.Ignore)]
        public string? Store { get; set; }

        [JsonProperty("packageName", NullValueHandling = NullValueHandling.Ignore)]
        public string? PackageName { get; set; }

        [JsonProperty("voided", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Voided { get; set; }

        [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Test { get; set; }

        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string? Reason { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class RecordAdRevenueArgs
    {
        [JsonProperty("revenue")] public double Revenue { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; } = "USD";
        [JsonProperty("revenueInMicros")] public bool RevenueInMicros { get; set; }
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; } = true;

        [JsonProperty("adNetwork", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdNetwork { get; set; }

        [JsonProperty("adFormat", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdFormat { get; set; }

        [JsonProperty("adType", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdType { get; set; }

        [JsonProperty("adPlacement", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdPlacement { get; set; }

        [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Test { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class RecordAdEventArgs
    {
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; } = true;

        // The reserved event name crosses under exactly one of these two keys,
        // depending on the binding; the unused one is left null and omitted.
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("eventName", NullValueHandling = NullValueHandling.Ignore)]
        public string? EventName { get; set; }

        [JsonProperty("adNetwork", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdNetwork { get; set; }

        [JsonProperty("mediationNetwork", NullValueHandling = NullValueHandling.Ignore)]
        public string? MediationNetwork { get; set; }

        [JsonProperty("adUnitId", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdUnitId { get; set; }

        [JsonProperty("adPlacement", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdPlacement { get; set; }

        [JsonProperty("adFormat", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdFormat { get; set; }

        [JsonProperty("adType", NullValueHandling = NullValueHandling.Ignore)]
        public string? AdType { get; set; }

        [JsonProperty("failureReason", NullValueHandling = NullValueHandling.Ignore)]
        public string? FailureReason { get; set; }

        [JsonProperty("loadLatencyMs", NullValueHandling = NullValueHandling.Ignore)]
        public double? LoadLatencyMs { get; set; }

        [JsonProperty("rewardType", NullValueHandling = NullValueHandling.Ignore)]
        public string? RewardType { get; set; }

        [JsonProperty("rewardAmount", NullValueHandling = NullValueHandling.Ignore)]
        public double? RewardAmount { get; set; }

        [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Test { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class RecordNotificationArgs
    {
        [JsonProperty("type")] public string Type { get; set; } = string.Empty;
        [JsonProperty("notificationId")] public string NotificationId { get; set; } = string.Empty;
        [JsonProperty("flushImmediately")] public bool FlushImmediately { get; set; }

        [JsonProperty("linkId", NullValueHandling = NullValueHandling.Ignore)]
        public string? LinkId { get; set; }

        [JsonProperty("campaignId", NullValueHandling = NullValueHandling.Ignore)]
        public string? CampaignId { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public string? Source { get; set; }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Payload { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class RecordErrorArgs
    {
        [JsonProperty("message")] public string Message { get; set; } = string.Empty;
        [JsonProperty("exceptionType")] public string ExceptionType { get; set; } = string.Empty;
        [JsonProperty("fatal")] public bool Fatal { get; set; }
        [JsonProperty("source")] public string Source { get; set; } = "manual";

        [JsonProperty("stackTrace", NullValueHandling = NullValueHandling.Ignore)]
        public string? StackTrace { get; set; }

        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string? Reason { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class SetUserArgs
    {
        // userId is assigned directly (not via Put) — an explicit null clears the
        // user, so it must stay on the wire.
        [JsonProperty("userId")] public string? UserId { get; set; }

        [JsonProperty("userName", NullValueHandling = NullValueHandling.Ignore)]
        public string? UserName { get; set; }
    }

    internal sealed class SetUserPropertyArgs
    {
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;

        // value is assigned directly — an explicit null clears the property, so it
        // must stay on the wire.
        [JsonProperty("value")] public object? Value { get; set; }
    }

    internal sealed class SetUserPropertiesArgs
    {
        [JsonProperty("properties")] public IDictionary<string, object> Properties { get; set; } =
            new Dictionary<string, object>();
    }

    internal sealed class ClearUserPropertiesArgs
    {
        [JsonProperty("propertyNames", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string>? PropertyNames { get; set; }
    }

    internal sealed class HandleIncomingLinkArgs
    {
        [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
        [JsonProperty("isInitialLink")] public bool IsInitialLink { get; set; }
    }

    internal sealed class RecordDeepLinkArgs
    {
        [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
        [JsonProperty("source")] public string Source { get; set; } = "manual";

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object>? Metadata { get; set; }
    }

    internal sealed class ValidateReceiptArgs
    {
        [JsonProperty("receipt")] public string Receipt { get; set; } = string.Empty;
        [JsonProperty("test")] public bool Test { get; set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public string? Provider { get; set; }

        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public string? Environment { get; set; }

        [JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProductId { get; set; }

        [JsonProperty("transactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? TransactionId { get; set; }
    }

    internal sealed class SetGdprConsentArgs
    {
        [JsonProperty("analytics")] public bool Analytics { get; set; }
        [JsonProperty("attribution")] public bool Attribution { get; set; }
        [JsonProperty("adEvents")] public bool AdEvents { get; set; }
    }

    internal sealed class UpdateSkanConversionValueArgs
    {
        [JsonProperty("fineValue")] public int FineValue { get; set; }
        [JsonProperty("lockWindow")] public bool LockWindow { get; set; }

        [JsonProperty("coarseValue", NullValueHandling = NullValueHandling.Ignore)]
        public string? CoarseValue { get; set; }
    }
}
