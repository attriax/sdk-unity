#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxPlatformInstallReferrerManager
    {
        private readonly Func<string?> _readPersistedInstallReferrer;

        public AttriaxPlatformInstallReferrerManager(Func<string?> readPersistedInstallReferrer)
        {
            _readPersistedInstallReferrer = readPersistedInstallReferrer
                ?? throw new ArgumentNullException(nameof(readPersistedInstallReferrer));
        }

        public AttriaxInstallReferrerContextPayload BuildInitialInstallReferrerContext(
            AttriaxPlatformType platform)
        {
            var cachedInstallReferrer = platform == AttriaxPlatformType.Android
                ? _readPersistedInstallReferrer()
                : null;

            return new AttriaxInstallReferrerContextPayload
            {
                InstallReferrer = cachedInstallReferrer,
                Metadata = new Dictionary<string, object>
                {
                    ["source"] = !string.IsNullOrWhiteSpace(cachedInstallReferrer)
                        ? "unity_cached_install_referrer"
                        : "unity_initial_context",
                },
            };
        }

        public async Task<AttriaxInstallReferrerContextPayload> CollectInstallReferrerContextAsync(
            AttriaxPlatformType platform)
        {
            if (platform == AttriaxPlatformType.Android)
            {
                var cachedReferrer = _readPersistedInstallReferrer();
                if (!string.IsNullOrWhiteSpace(cachedReferrer))
                {
                    return new AttriaxInstallReferrerContextPayload
                    {
                        InstallReferrer = cachedReferrer,
                        Metadata = new Dictionary<string, object>
                        {
                            ["source"] = "unity_cached_install_referrer",
                        },
                    };
                }

                var firstAttempt = await AttriaxNativeBridge.CollectInstallReferrerAsync(platform);
                if (!string.IsNullOrWhiteSpace(firstAttempt.InstallReferrer))
                {
                    return firstAttempt;
                }

                var secondAttempt = await AttriaxNativeBridge.CollectInstallReferrerAsync(platform);
                if (!string.IsNullOrWhiteSpace(secondAttempt.InstallReferrer))
                {
                    secondAttempt.Metadata["installReferrerAttempts"] = 2;
                    return secondAttempt;
                }

                return new AttriaxInstallReferrerContextPayload
                {
                    Metadata = MergeMetadata(
                        firstAttempt.Metadata,
                        secondAttempt.Metadata,
                        new Dictionary<string, object>
                        {
                            ["installReferrerAttempts"] = 2,
                            ["installReferrerStatus"] =
                                ReadString(secondAttempt.Metadata, "installReferrerStatus")
                                ?? ReadString(firstAttempt.Metadata, "installReferrerStatus")
                                ?? "empty",
                        }),
                };
            }

            return await AttriaxNativeBridge.CollectInstallReferrerAsync(platform);
        }

        public AttriaxInstallReferrerDetails? BuildLocalInstallReferrerDetails(
            AttriaxInstallReferrerContextPayload context)
        {
            if (string.IsNullOrWhiteSpace(context.InstallReferrer))
            {
                return null;
            }

            var query = ParseQueryString(context.InstallReferrer!);
            var deepLinkUrl = FirstQueryValue(
                query,
                "deep_link_uri",
                "deep_link_url",
                "deep_link",
                "af_dp");

            return new AttriaxInstallReferrerDetails
            {
                RawPlatformInstallReferrer = context.InstallReferrer,
                Source = FirstQueryValue(query, "utm_source", "source"),
                Medium = FirstQueryValue(query, "utm_medium", "medium"),
                Campaign = FirstQueryValue(query, "utm_campaign", "campaign"),
                Term = FirstQueryValue(query, "utm_term", "term"),
                Content = FirstQueryValue(query, "utm_content", "content"),
                AdClickId = FirstQueryValue(query, "gclid", "gbraid", "wbraid", "fbclid"),
                AttributionType = AttributionType.Referrer,
                DeepLinkUri = deepLinkUrl,
                InstallBeginTimestampSeconds = context.InstallBeginTimestampSeconds,
                ReferrerClickTimestampSeconds = context.ReferrerClickTimestampSeconds,
                GooglePlayInstantParam = context.GooglePlayInstantParam,
                Precision = 0.5,
            };
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in query.Split('&'))
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var separatorIndex = part.IndexOf('=');
                var rawKey = separatorIndex >= 0 ? part.Substring(0, separatorIndex) : part;
                var rawValue = separatorIndex >= 0 ? part.Substring(separatorIndex + 1) : string.Empty;
                var key = DecodeQueryComponent(rawKey);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                parsed[key] = DecodeQueryComponent(rawValue);
            }

            return parsed;
        }

        private static string? FirstQueryValue(IDictionary<string, string> query, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string DecodeQueryComponent(string value)
        {
            return Uri.UnescapeDataString(value.Replace("+", " "));
        }

        private static Dictionary<string, object> MergeMetadata(
            params Dictionary<string, object>[] dictionaries)
        {
            var merged = new Dictionary<string, object>();
            foreach (var dictionary in dictionaries)
            {
                if (dictionary == null)
                {
                    continue;
                }

                foreach (var pair in dictionary)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            return merged;
        }

        private static string? ReadString(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            return rawValue.ToString();
        }
    }
}
