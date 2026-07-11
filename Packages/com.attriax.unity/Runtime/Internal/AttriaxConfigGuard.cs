#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Single source of truth for the fail-fast <see cref="AttriaxConfig"/> validation
    /// contract the Unity public API promises at construction time: a project token is
    /// required, and an explicitly-set API base URL must use HTTPS (localhost may use
    /// HTTP for development). Both engines call this so <c>new Attriax(config)</c> throws
    /// identically regardless of the backing engine — the managed C# engine
    /// (<see cref="AttriaxRuntime"/>, whose <c>NormalizeConfig</c> mirrors these exact
    /// checks) and the native engine adapter
    /// (<see cref="Engine.AttriaxEnginePlatformAdapter"/>), which lowers config to the
    /// native core and would otherwise skip this fail-fast validation entirely.
    /// </summary>
    internal static class AttriaxConfigGuard
    {
        /// <summary>
        /// Throws when <paramref name="config"/> is missing a project token or specifies
        /// an insecure/invalid remote API base URL. Mirrors <c>AttriaxRuntime.NormalizeConfig</c>.
        /// </summary>
        public static void Validate(AttriaxConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(config.ProjectToken))
            {
                throw new ArgumentException("AttriaxConfig.ProjectToken is required.", nameof(config));
            }

            // A blank ApiBaseUrl is valid: the engine falls back to its default HTTPS
            // production endpoint. Only an explicitly-provided URL is validated.
            if (string.IsNullOrWhiteSpace(config.ApiBaseUrl))
            {
                return;
            }

            var apiBaseUrl = config.ApiBaseUrl!.TrimEnd('/');
            if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
            {
                throw new ArgumentException("AttriaxConfig.ApiBaseUrl must be a valid absolute URL.", nameof(config));
            }

            var host = apiUri.Host;
            if (host.StartsWith("[", StringComparison.Ordinal) && host.EndsWith("]", StringComparison.Ordinal))
            {
                host = host.Substring(1, host.Length - 2);
            }

            var isLocalhost = host == "localhost" || host == "127.0.0.1" || host == "::1";
            if (apiUri.Scheme != Uri.UriSchemeHttps && !(isLocalhost && apiUri.Scheme == Uri.UriSchemeHttp))
            {
                throw new ArgumentException("AttriaxConfig.ApiBaseUrl must use HTTPS unless it targets localhost.", nameof(config));
            }
        }
    }
}
