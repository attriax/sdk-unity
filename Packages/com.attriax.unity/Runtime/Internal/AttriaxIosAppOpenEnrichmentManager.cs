#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxIosAppOpenEnrichmentManager
    {
        internal const string WkWebViewUserAgentMetadataKey = "wkWebViewUserAgent";

        private const string ClipboardClickIdParam = "attriax_click_id";

        private readonly AttriaxPlatformType _platformType;
        private readonly Func<Task<string?>> _readAttributionClipboardAsync;
        private readonly Func<Task<string?>> _collectWebViewUserAgentAsync;

        private bool _didAttemptClipboardCapture;
        private string? _capturedInstallReferrer;

        public AttriaxIosAppOpenEnrichmentManager(
            AttriaxPlatformType platformType,
            Func<Task<string?>>? readAttributionClipboardAsync = null,
            Func<Task<string?>>? collectWebViewUserAgentAsync = null)
        {
            _platformType = platformType;
            _readAttributionClipboardAsync = readAttributionClipboardAsync
                ?? (() => AttriaxNativeBridge.ReadAttributionClipboardAsync(platformType));
            _collectWebViewUserAgentAsync = collectWebViewUserAgentAsync
                ?? (() => AttriaxNativeBridge.CollectWebViewUserAgentAsync(platformType));
        }

        public async Task PrimeForConsentStateAsync(
            bool clipboardAttributionEnabled,
            bool isWaitingForGdprConsent,
            bool allowsAttributionTracking)
        {
            if (_platformType != AttriaxPlatformType.IOS ||
                !clipboardAttributionEnabled ||
                (!isWaitingForGdprConsent && !allowsAttributionTracking) ||
                _didAttemptClipboardCapture)
            {
                return;
            }

            _didAttemptClipboardCapture = true;
            _capturedInstallReferrer = NormalizeInstallReferrer(
                await _readAttributionClipboardAsync().ConfigureAwait(false));
        }

        public string? InstallReferrerOverrideForAppOpen(
            bool clipboardAttributionEnabled,
            bool allowsAttributionTracking)
        {
            return clipboardAttributionEnabled && allowsAttributionTracking
                ? _capturedInstallReferrer
                : null;
        }

        public async Task<Dictionary<string, object>> BuildDeviceMetadataOverridesForAppOpenAsync(
            bool allowsAttributionTracking)
        {
            if (_platformType != AttriaxPlatformType.IOS || !allowsAttributionTracking)
            {
                return new Dictionary<string, object>();
            }

            var webViewUserAgent = TrimOrNull(
                await _collectWebViewUserAgentAsync().ConfigureAwait(false));
            if (webViewUserAgent == null)
            {
                return new Dictionary<string, object>();
            }

            return new Dictionary<string, object>
            {
                [WkWebViewUserAgentMetadataKey] = webViewUserAgent,
            };
        }

        public void Reset()
        {
            _didAttemptClipboardCapture = false;
            _capturedInstallReferrer = null;
        }

        private static string? NormalizeInstallReferrer(string? clipboardText)
        {
            var trimmed = TrimOrNull(clipboardText);
            if (trimmed == null)
            {
                return null;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                var query = TrimOrNull(uri.Query?.TrimStart('?'));
                if (query != null)
                {
                    return query;
                }
            }

            var normalized = trimmed.StartsWith("?", StringComparison.Ordinal)
                ? trimmed.Substring(1)
                : trimmed;
            if (normalized.Contains("=", StringComparison.Ordinal) ||
                normalized.Contains("&", StringComparison.Ordinal))
            {
                return normalized;
            }

            return ClipboardClickIdParam + "=" + Uri.EscapeDataString(trimmed);
        }

        private static string? TrimOrNull(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}