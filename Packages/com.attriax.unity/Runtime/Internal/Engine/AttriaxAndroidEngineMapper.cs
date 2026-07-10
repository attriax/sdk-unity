#nullable enable
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Deserializes the JSON result envelopes returned by
    /// <c>AttriaxUnityBridge.dispatch</c> (and the engine-event payloads) into the
    /// public Unity SDK value types. Field mappings mirror the Kotlin bridge
    /// serializers and the KMP public surface; unmapped/nested fields the C# types do
    /// not expose are left at their defaults (best-effort, matching the Flutter
    /// binding's serializers).
    /// </summary>
    internal static class AttriaxAndroidEngineMapper
    {
        private static readonly Uri PlaceholderUri = new Uri("https://attriax.invalid/");

        // -----------------------------------------------------------------
        // Enums.
        // -----------------------------------------------------------------

        public static AttriaxSynchronizationState ToSyncState(string? wire)
        {
            switch (wire)
            {
                case "INITIALIZING":
                    return AttriaxSynchronizationState.Initializing;
                case "SYNCHRONIZING":
                    return AttriaxSynchronizationState.Synchronizing;
                case "DEFERRED":
                    return AttriaxSynchronizationState.Deferred;
                case "SYNCHRONIZED":
                    return AttriaxSynchronizationState.Synchronized;
                case "OFFLINE":
                    return AttriaxSynchronizationState.Offline;
                case "FAILED":
                    return AttriaxSynchronizationState.Failed;
                case "DISABLED":
                    return AttriaxSynchronizationState.Disabled;
                default:
                    return AttriaxSynchronizationState.Initializing;
            }
        }

        public static AttriaxTrackingAuthorizationStatus ToTrackingAuthStatus(string? wire)
        {
            switch (wire)
            {
                case "authorized":
                    return AttriaxTrackingAuthorizationStatus.Authorized;
                case "denied":
                    return AttriaxTrackingAuthorizationStatus.Denied;
                case "restricted":
                    return AttriaxTrackingAuthorizationStatus.Restricted;
                case "notDetermined":
                case "not_determined":
                    return AttriaxTrackingAuthorizationStatus.NotDetermined;
                default:
                    return AttriaxTrackingAuthorizationStatus.Unknown;
            }
        }

        private static AttriaxDeepLinkTrigger ToTrigger(string? wire)
        {
            switch (wire)
            {
                case "COLD_START":
                    return AttriaxDeepLinkTrigger.ColdStart;
                case "FOREGROUND":
                    return AttriaxDeepLinkTrigger.Foreground;
                case "DEFERRED":
                    return AttriaxDeepLinkTrigger.Deferred;
                default:
                    return AttriaxDeepLinkTrigger.ColdStart;
            }
        }

        private static AttriaxResolvedUrlOpenMode ToOpenMode(string? wire)
        {
            switch (wire)
            {
                case "IN_APP":
                    return AttriaxResolvedUrlOpenMode.InApp;
                case "EXTERNAL":
                    return AttriaxResolvedUrlOpenMode.External;
                default:
                    return AttriaxResolvedUrlOpenMode.Unknown;
            }
        }

        private static AttributionType ToAttributionType(string? wire)
        {
            switch (wire)
            {
                case "REFERRER":
                    return AttributionType.Referrer;
                case "FINGERPRINT":
                    return AttributionType.Fingerprint;
                case "EXTERNAL":
                    return AttributionType.External;
                default:
                    return AttributionType.Organic;
            }
        }

        private static AttriaxRevenueReceiptValidationStatus ToReceiptStatus(string? wire)
        {
            switch (wire)
            {
                case "VERIFIED":
                    return AttriaxRevenueReceiptValidationStatus.Verified;
                case "REJECTED":
                    return AttriaxRevenueReceiptValidationStatus.Rejected;
                case "PENDING":
                    return AttriaxRevenueReceiptValidationStatus.Pending;
                case "UNCONFIGURED":
                    return AttriaxRevenueReceiptValidationStatus.Unconfigured;
                case "PROVIDER_ERROR":
                    return AttriaxRevenueReceiptValidationStatus.ProviderError;
                case "PASSTHROUGH":
                    return AttriaxRevenueReceiptValidationStatus.Passthrough;
                default:
                    return AttriaxRevenueReceiptValidationStatus.Rejected;
            }
        }

        private static AttriaxSkanUpdateStatus ToSkanUpdateStatus(string? wire)
        {
            switch (wire)
            {
                case "updated":
                    return AttriaxSkanUpdateStatus.Updated;
                case "skipped":
                    return AttriaxSkanUpdateStatus.Skipped;
                case "already_at_or_above_value":
                    return AttriaxSkanUpdateStatus.AlreadyAtOrAboveValue;
                case "invalid_value":
                    return AttriaxSkanUpdateStatus.InvalidValue;
                case "disabled":
                    return AttriaxSkanUpdateStatus.Disabled;
                case "error":
                    return AttriaxSkanUpdateStatus.Error;
                default:
                    return AttriaxSkanUpdateStatus.NotSupported;
            }
        }

        private static AttriaxSkanCoarseValue? ToCoarse(string? wire)
        {
            switch (wire)
            {
                case "low":
                    return AttriaxSkanCoarseValue.Low;
                case "medium":
                    return AttriaxSkanCoarseValue.Medium;
                case "high":
                    return AttriaxSkanCoarseValue.High;
                default:
                    return null;
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
                ClickedAt = ToDate(o, "clickedAtMs"),
                ConsumedAt = ToDate(o, "consumedAtMs"),
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
                ReceivedAt = ToDate(o, "receivedAtMs"),
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

        public static AttriaxSkanState? ToSkanState(JObject? o)
        {
            if (o == null)
            {
                return null;
            }

            return new AttriaxSkanState
            {
                Enabled = Bool(o, "enabled"),
                FineValue = IntOrNull(o, "fineValue"),
                CoarseValue = ToCoarse(Str(o, "coarseValue")),
                LockWindow = Bool(o, "lockWindow"),
            };
        }

        public static AttriaxSkanUpdateResult ToSkanUpdateResult(JObject? o)
        {
            if (o == null)
            {
                return new AttriaxSkanUpdateResult { Status = AttriaxSkanUpdateStatus.NotSupported };
            }

            return new AttriaxSkanUpdateResult
            {
                Status = ToSkanUpdateStatus(Str(o, "status")),
                Message = Str(o, "message"),
                FineValue = IntOrNull(o, "fineValue"),
                CoarseValue = ToCoarse(Str(o, "coarseValue")),
                LockWindow = Bool(o, "lockWindow"),
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
                DeepLinkUri = Str(o, "deepLinkUri"),
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
                ReceivedAt = ToDate(o, "receivedAtMs"),
                ClickedAt = ToDate(o, "clickedAtMs"),
                ConsumedAt = ToDate(o, "consumedAtMs"),
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
                AcceptedAt = ToDateOrNull(o, "acceptedAtMs"),
                Provider = Str(o, "provider"),
                Environment = Str(o, "environment"),
                TransactionId = Str(o, "transactionId"),
                OriginalTransactionId = Str(o, "originalTransactionId"),
                ProductId = Str(o, "productId"),
                FailureReason = Str(o, "failureReason"),
                ExpiresAt = ToDateOrNull(o, "expiresAtMs"),
                ProviderResult = ToDict(o["providerResult"]),
                PublicReceipt = ToDict(o["publicReceipt"]) ?? new Dictionary<string, object>(),
            };
        }

        public static AttriaxCreateDynamicLinkResult ToCreateDynamicLinkResult(JObject? o)
        {
            var result = new AttriaxCreateDynamicLinkResult();
            var record = o?["record"] as JObject;
            if (record == null)
            {
                return result;
            }

            result.Link = new AttriaxDynamicLinkRecord
            {
                Id = Str(record, "id") ?? string.Empty,
                Path = Str(record, "path") ?? string.Empty,
                ShortUrl = Str(record, "shortUrl") ?? Str(o, "shortUrl") ?? string.Empty,
                Name = Str(record, "name"),
                DestinationUrl = Str(record, "destinationUrl"),
                Group = Str(record, "group"),
                Prefix = Str(record, "prefix"),
                Data = ToDict(record["data"]),
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

        private static int? IntOrNull(JObject? o, string key)
        {
            var token = o?[key];
            return token == null || token.Type == JTokenType.Null ? (int?)null : token.Value<int>();
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

        private static DateTimeOffset ToDate(JObject? o, string key)
        {
            var ms = LongOrNull(o, key);
            return ms.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(ms.Value) : DateTimeOffset.MinValue;
        }

        private static DateTimeOffset? ToDateOrNull(JObject? o, string key)
        {
            var ms = LongOrNull(o, key);
            return ms.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(ms.Value) : (DateTimeOffset?)null;
        }

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
