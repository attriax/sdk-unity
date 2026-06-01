#nullable enable
using System;
using System.Collections.Generic;

namespace Attriax.Unity.Internal
{
    internal static class AttriaxGdprRegion
    {
        private static readonly HashSet<string> ExplicitGdprTimezones = new HashSet<string>(StringComparer.Ordinal)
        {
            "Arctic/Longyearbyen",
            "Asia/Famagusta",
            "Asia/Nicosia",
            "Atlantic/Azores",
            "Atlantic/Canary",
            "Atlantic/Faroe",
            "Atlantic/Madeira",
            "Atlantic/Reykjavik",
        };

        private static readonly HashSet<string> ExcludedEuropeTimezones = new HashSet<string>(StringComparer.Ordinal)
        {
            "Europe/Andorra",
            "Europe/Belgrade",
            "Europe/Chisinau",
            "Europe/Istanbul",
            "Europe/Kaliningrad",
            "Europe/Kiev",
            "Europe/Kirov",
            "Europe/Kyiv",
            "Europe/Minsk",
            "Europe/Moscow",
            "Europe/Podgorica",
            "Europe/Pristina",
            "Europe/Samara",
            "Europe/Sarajevo",
            "Europe/Simferopol",
            "Europe/Skopje",
            "Europe/Tirane",
            "Europe/Uzhgorod",
            "Europe/Volgograd",
            "Europe/Zaporozhye",
        };

        private static readonly Dictionary<string, string> TimezoneAliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Belarus Standard Time", "Europe/Minsk" },
            { "Central Europe Standard Time", "Europe/Budapest" },
            { "Central European Standard Time", "Europe/Warsaw" },
            { "E. Europe Standard Time", "Europe/Chisinau" },
            { "FLE Standard Time", "Europe/Helsinki" },
            { "GMT Standard Time", "Europe/London" },
            { "GTB Standard Time", "Europe/Bucharest" },
            { "Greenwich Standard Time", "Atlantic/Reykjavik" },
            { "Kaliningrad Standard Time", "Europe/Kaliningrad" },
            { "Romance Standard Time", "Europe/Paris" },
            { "Russia Time Zone 3", "Europe/Samara" },
            { "Russian Standard Time", "Europe/Moscow" },
            { "Turkey Standard Time", "Europe/Istanbul" },
            { "Volgograd Standard Time", "Europe/Volgograd" },
            { "W. Europe Standard Time", "Europe/Berlin" },
        };

        public static AttriaxGdprConsentState? ResolveStateForTimezone(string? timezone)
        {
            var normalized = CanonicalizeTimezone(timezone);
            if (string.IsNullOrWhiteSpace(normalized) || normalized.IndexOf('/') < 0)
            {
                return null;
            }

            if (ExplicitGdprTimezones.Contains(normalized))
            {
                return AttriaxGdprConsentState.Pending;
            }

            if (normalized.StartsWith("Europe/", StringComparison.Ordinal))
            {
                return ExcludedEuropeTimezones.Contains(normalized)
                    ? AttriaxGdprConsentState.NotRequired
                    : AttriaxGdprConsentState.Pending;
            }

            return AttriaxGdprConsentState.NotRequired;
        }

        private static string? CanonicalizeTimezone(string? timezone)
        {
            if (string.IsNullOrWhiteSpace(timezone))
            {
                return null;
            }

            var normalized = timezone.Trim();
            string aliased;
            return TimezoneAliases.TryGetValue(normalized, out aliased) ? aliased : normalized;
        }
    }
}