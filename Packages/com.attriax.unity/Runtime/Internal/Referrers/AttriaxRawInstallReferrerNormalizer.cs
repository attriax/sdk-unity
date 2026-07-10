#nullable enable

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors Flutter's <c>attriax_raw_install_referrer_normalizer.dart</c>:
    /// trims the raw platform install-referrer string and collapses
    /// empty/whitespace values to null.
    /// </summary>
    internal static class AttriaxRawInstallReferrerNormalizer
    {
        public static string? Normalize(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
