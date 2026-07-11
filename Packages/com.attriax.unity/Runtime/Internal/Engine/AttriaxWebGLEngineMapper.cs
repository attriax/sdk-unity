#nullable enable
#if UNITY_WEBGL && !UNITY_EDITOR
#define ATTRIAX_WEBGL_ENGINE
#endif
#if ATTRIAX_WEBGL_ENGINE
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Deserializes the JSON payloads produced by the <c>@attriax/js</c> engine
    /// (sdk-js) into the public Unity SDK value types for
    /// <see cref="AttriaxWebGLEnginePlatform"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the WebGL twin of <c>AttriaxDesktopEngineMapper</c> /
    /// <c>AttriaxAndroidEngineMapper</c>, but it parses a <b>different wire shape</b>:
    /// the C-ABI / JNI mappers read the KMP engine's serialization (millisecond
    /// epoch <c>*Ms</c> timestamps and <c>name.lowercase()</c> / UPPER_SNAKE enum
    /// slugs), whereas sdk-js hands back its own public object model
    /// <c>JSON.stringify</c>-d by the <c>.jslib</c> bridge. That means:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Dates</b> are ISO-8601 strings (a JS <c>Date</c> serializes via
    /// <c>toJSON()</c>), not <c>*Ms</c> integers.</item>
    /// <item><b>URIs</b> are already <c>href</c> strings (a JS <c>URL</c> serializes
    /// via <c>toJSON()</c>).</item>
    /// <item><b>Enums</b> carry sdk-js's own string values — camelCase for the
    /// deep-link trigger (<c>coldStart</c>) and resolved-URL open mode
    /// (<c>inApp</c>), snake for the receipt status (<c>provider_error</c>).</item>
    /// </list>
    /// <para>
    /// The value-type field readers otherwise line up with the sibling mappers, so
    /// the produced Unity types are identical regardless of which native engine the
    /// binding drives. Members that sdk-js does not surface on the web (ATT / SKAN)
    /// degrade to defaults on the platform itself and are never routed here, so this
    /// mapper omits their parsers.
    /// </para>
    /// </remarks>
    internal static class AttriaxWebGLEngineMapper
    {
        private static readonly Uri PlaceholderUri = new Uri("https://attriax.invalid/");

        // -----------------------------------------------------------------
        // Enums (wire = sdk-js public string values).
        // -----------------------------------------------------------------

        public static AttriaxSynchronizationState ToSyncState(string? wire)
        {
            switch (Norm(wire))
            {
                case "initializing":
                    return AttriaxSynchronizationState.Initializing;
                case "synchronizing":
                    return AttriaxSynchronizationState.Synchronizing;
                case "deferred":
                    return AttriaxSynchronizationState.Deferred;
                case "synchronized":
                    return AttriaxSynchronizationState.Synchronized;
                case "offline":
                    return AttriaxSynchronizationState.Offline;
                case "failed":
                    return AttriaxSynchronizationState.Failed;
                case "disabled":
                    return AttriaxSynchronizationState.Disabled;
                default:
                    return AttriaxSynchronizationState.Initializing;
            }
        }

        private static AttriaxDeepLinkTrigger ToTrigger(string? wire)
        {
            // sdk-js emits camelCase: coldStart / foreground / deferred.
            switch (Norm(wire))
            {
                case "coldstart":
                    return AttriaxDeepLinkTrigger.ColdStart;
                case "foreground":
                    return AttriaxDeepLinkTrigger.Foreground;
                case "deferred":
                    return AttriaxDeepLinkTrigger.Deferred;
                default:
                    return AttriaxDeepLinkTrigger.ColdStart;
            }
        }

        private static AttriaxResolvedUrlOpenMode ToOpenMode(string? wire)
        {
            // sdk-js emits camelCase: inApp / external / unknown.
            switch (Norm(wire))
            {
                case "inapp":
                    return AttriaxResolvedUrlOpenMode.InApp;
                case "external":
                    return AttriaxResolvedUrlOpenMode.External;
                default:
                    return AttriaxResolvedUrlOpenMode.Unknown;
            }
        }

        private static AttributionType ToAttributionType(string? wire)
        {
            switch (Norm(wire))
            {
                case "referrer":
                    return AttributionType.Referrer;
                case "fingerprint":
                    return AttributionType.Fingerprint;
                case "external":
                    return AttributionType.External;
                default:
                    return AttributionType.Organic;
            }
        }

        private static AttriaxRevenueReceiptValidationStatus ToReceiptStatus(string? wire)
        {
            switch (Norm(wire))
            {
                case "verified":
                    return AttriaxRevenueReceiptValidationStatus.Verified;
                case "rejected":
                    return AttriaxRevenueReceiptValidationStatus.Rejected;
                case "pending":
                    return AttriaxRevenueReceiptValidationStatus.Pending;
                case "unconfigured":
                    return AttriaxRevenueReceiptValidationStatus.Unconfigured;
                case "provider_error":
                    return AttriaxRevenueReceiptValidationStatus.ProviderError;
                case "passthrough":
                    return AttriaxRevenueReceiptValidationStatus.Passthrough;
                default:
                    return AttriaxRevenueReceiptValidationStatus.Rejected;
            }
        }

        // -----------------------------------------------------------------
        // Value types.
        // -----------------------------------------------------------------

        public static AttriaxDeepLinkEvent? ToDeepLinkEvent(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxDeepLinkEvent
            {
                Uri = ToUri(Str(o, "uri")),
                ClickedAt = ToDate(o, "clickedAt"),
                ConsumedAt = ToDate(o, "consumedAt"),
                Found = Bool(o, "found"),
                Trigger = ToTrigger(Str(o, "trigger")),
                RawEvent = ToRawDeepLinkEvent(o["rawEvent"] as JObject),
                Data = ToDict(o["data"]),
                Utm = ToUtm(o["utm"] as JObject),
                BrowserAction = ToBrowserAction(o["browserAction"] as JObject),
                HandledBySdk = Bool(o, "handledBySdk"),
            };
        }

        public static AttriaxRawDeepLinkEvent? ToRawDeepLinkEvent(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxRawDeepLinkEvent
            {
                Uri = ToUri(Str(o, "uri")),
                ReceivedAt = ToDate(o, "receivedAt"),
                IsInitial = Bool(o, "isInitial"),
            };
        }

        public static AttriaxSdkSnapshot? ToSdkSnapshot(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxSdkSnapshot
            {
                ApiVersion = Str(o, "apiVersion") ?? string.Empty,
                PackageVersion = Str(o, "packageVersion") ?? string.Empty,
                Metadata = ToDict(o["metadata"]) ?? new Dictionary<string, object>(),
            };
        }

        public static AttriaxInstallReferrerDetails? ToInstallReferrer(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxInstallReferrerDetails
            {
                RawPlatformInstallReferrer = Str(o, "rawPlatformInstallReferrer"),
                Source = Str(o, "source"),
                Medium = Str(o, "medium"),
                Campaign = Str(o, "campaign"),
                Term = Str(o, "term"),
                Content = Str(o, "content"),
                AdNetwork = Str(o, "adNetwork"),
                AdClickId = Str(o, "adClickId"),
                AttributionType = ToAttributionType(Str(o, "attributionType")),
                DeepLinkUri = Str(o, "deepLinkUrl") ?? Str(o, "deepLinkUri"),
                DeepLinkData = ToDict(o["deepLinkData"]),
                RegisteredAt = ToDateFromIso(Str(o, "registeredAt")),
                InstallBeginTimestampSeconds = LongOrNull(o, "installBeginTimestampSeconds"),
                ReferrerClickTimestampSeconds = LongOrNull(o, "referrerClickTimestampSeconds"),
                GooglePlayInstantParam = BoolOrNull(o, "googlePlayInstantParam"),
                Precision = Double(o, "precision"),
            };
        }

        public static AttriaxDeepLinkReferrerDetails? ToDeepLinkReferrer(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxDeepLinkReferrerDetails
            {
                Uri = ToUri(Str(o, "uri")),
                ReceivedAt = ToDate(o, "receivedAt"),
                ClickedAt = ToDate(o, "clickedAt"),
                ConsumedAt = ToDate(o, "consumedAt"),
                Trigger = ToTrigger(Str(o, "trigger")),
                IsAttriaxDomain = Bool(o, "isAttriaxDomain"),
                Found = Bool(o, "found"),
                Data = ToDict(o["data"]),
                Utm = ToUtm(o["utm"] as JObject),
                BrowserAction = ToBrowserAction(o["browserAction"] as JObject),
            };
        }

        public static AttriaxRevenueReceiptValidationResult ToReceiptResult(JObject? o)
        {
            if (o == null)
            {
                return new AttriaxRevenueReceiptValidationResult();
            }

            return new AttriaxRevenueReceiptValidationResult
            {
                ValidationId = Str(o, "validationId") ?? string.Empty,
                Status = ToReceiptStatus(Str(o, "status")),
                RequestVersion = Str(o, "requestVersion"),
                AcceptedAt = ToDateOrNull(o, "acceptedAt"),
                Provider = Str(o, "provider"),
                Environment = Str(o, "environment"),
                TransactionId = Str(o, "transactionId"),
                OriginalTransactionId = Str(o, "originalTransactionId"),
                ProductId = Str(o, "productId"),
                FailureReason = Str(o, "failureReason"),
                ExpiresAt = ToDateOrNull(o, "expiresAt"),
                ProviderResult = ToDict(o["providerResult"]),
                PublicReceipt = ToDict(o["publicReceipt"]) ?? new Dictionary<string, object>(),
            };
        }

        public static AttriaxCreateDynamicLinkResult ToCreateDynamicLinkResult(JObject? o)
        {
            var result = new AttriaxCreateDynamicLinkResult();
            if (o == null)
            {
                return result;
            }

            result.RequestVersion = Str(o, "requestVersion");
            result.AcceptedAt = ToDateOrNull(o, "acceptedAt");

            // sdk-js returns the created link under `link`.
            var record = o["link"] as JObject;
            if (record == null)
            {
                return result;
            }

            result.Link = new AttriaxDynamicLinkRecord
            {
                Id = Str(record, "id") ?? string.Empty,
                Path = Str(record, "path") ?? string.Empty,
                ShortUrl = Str(record, "shortUrl") ?? string.Empty,
                Name = Str(record, "name"),
                DestinationUrl = Str(record, "destinationUrl"),
                Group = Str(record, "group"),
                Prefix = Str(record, "prefix"),
                Data = ToDict(record["data"]),
                PreviewTitle = Str(record, "previewTitle"),
                PreviewDescription = Str(record, "previewDescription"),
                PreviewImagePath = Str(record, "previewImagePath"),
                IOSRedirect = BoolOrNull(record, "iosRedirect"),
                AndroidRedirect = BoolOrNull(record, "androidRedirect"),
                UtmSource = Str(record, "utmSource"),
                UtmMedium = Str(record, "utmMedium"),
                UtmCampaign = Str(record, "utmCampaign"),
                UtmTerm = Str(record, "utmTerm"),
                UtmContent = Str(record, "utmContent"),
                CreatedAt = ToDateOrNull(record, "createdAt"),
            };
            return result;
        }

        private static AttriaxUtmParameters? ToUtm(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxUtmParameters
            {
                Source = Str(o, "source"),
                Medium = Str(o, "medium"),
                Campaign = Str(o, "campaign"),
                Term = Str(o, "term"),
                Content = Str(o, "content"),
            };
        }

        private static AttriaxResolvedUrlAction? ToBrowserAction(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxResolvedUrlAction
            {
                Url = Str(o, "url") ?? string.Empty,
                OpenMode = ToOpenMode(Str(o, "openMode")),
            };
        }

        // -----------------------------------------------------------------
        // Primitive readers.
        // -----------------------------------------------------------------

        private static string? Norm(string? wire) => wire?.Trim().ToLowerInvariant();

        private static string? Str(JObject? o, string key)
        {
            var token = o?[key];
            return token == null || token.Type == JTokenType.Null ? null : token.Value<string>();
        }

        private static bool Bool(JObject? o, string key)
        {
            var token = o?[key];
            return token != null && token.Type == JTokenType.Boolean && token.Value<bool>();
        }

        private static bool? BoolOrNull(JObject? o, string key)
        {
            var token = o?[key];
            return token == null || token.Type == JTokenType.Null ? (bool?)null : token.Value<bool>();
        }

        private static long? LongOrNull(JObject? o, string key)
        {
            var token = o?[key];
            return token == null || token.Type == JTokenType.Null ? (long?)null : token.Value<long>();
        }

        private static double Double(JObject? o, string key)
        {
            var token = o?[key];
            return token == null || token.Type == JTokenType.Null ? 0.0 : token.Value<double>();
        }

        private static Uri ToUri(string? raw)
        {
            if (!string.IsNullOrEmpty(raw) && Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            {
                return uri;
            }

            return PlaceholderUri;
        }

        // sdk-js dates cross the bridge as ISO-8601 strings (JS Date.toJSON()).
        private static DateTimeOffset ToDate(JObject? o, string key) =>
            ToDateFromIso(Str(o, key)) ?? DateTimeOffset.MinValue;

        private static DateTimeOffset? ToDateOrNull(JObject? o, string key) =>
            ToDateFromIso(Str(o, key));

        private static DateTimeOffset? ToDateFromIso(string? iso)
        {
            if (string.IsNullOrEmpty(iso))
            {
                return null;
            }

            return DateTimeOffset.TryParse(
                iso,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed)
                ? parsed
                : (DateTimeOffset?)null;
        }

        private static IDictionary<string, object>? ToDict(JToken? token)
        {
            if (token is not JObject obj)
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            foreach (var property in obj.Properties())
            {
                var value = ToPlain(property.Value);
                if (value != null)
                {
                    dict[property.Name] = value;
                }
            }

            return dict;
        }

        private static object? ToPlain(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ToDict(token);
                case JTokenType.Array:
                    var list = new List<object>();
                    foreach (var item in (JArray)token)
                    {
                        var value = ToPlain(item);
                        if (value != null)
                        {
                            list.Add(value);
                        }
                    }

                    return list;
                case JTokenType.Integer:
                    return token.Value<long>();
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Null:
                    return null;
                case JTokenType.String:
                    return token.Value<string>();
                default:
                    return token.ToString();
            }
        }
    }
}
#endif
