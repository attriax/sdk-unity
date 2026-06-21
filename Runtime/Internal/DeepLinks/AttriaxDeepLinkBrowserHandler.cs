#nullable enable
using System;
using System.Threading.Tasks;
using Attriax.Unity;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Opens the resolved browser URL for a deep link when automatic browser
    /// handling is enabled. Mirrors the Flutter reference's
    /// <c>deep_links/attriax_deep_link_browser_handler.dart</c>.
    /// </summary>
    internal sealed class AttriaxDeepLinkBrowserHandler
    {
        private readonly bool _automaticBrowserHandling;
        private readonly Func<AttriaxPlatformType> _getCurrentPlatform;
        private readonly Func<AttriaxPlatformType, string, AttriaxResolvedUrlOpenMode, Task<bool>> _openBrowserUrlAsync;

        internal AttriaxDeepLinkBrowserHandler(
            bool automaticBrowserHandling,
            Func<AttriaxPlatformType> getCurrentPlatform,
            Func<AttriaxPlatformType, string, AttriaxResolvedUrlOpenMode, Task<bool>> openBrowserUrlAsync)
        {
            _automaticBrowserHandling = automaticBrowserHandling;
            _getCurrentPlatform = getCurrentPlatform;
            _openBrowserUrlAsync = openBrowserUrlAsync;
        }

        internal async Task<bool> HandleAsync(AttriaxResolvedUrlAction? browserAction)
        {
            if (!_automaticBrowserHandling ||
                browserAction == null ||
                string.IsNullOrWhiteSpace(browserAction.Url))
            {
                return false;
            }

            return await _openBrowserUrlAsync(
                    _getCurrentPlatform(),
                    browserAction.Url,
                    browserAction.OpenMode)
                .ConfigureAwait(false);
        }
    }
}
