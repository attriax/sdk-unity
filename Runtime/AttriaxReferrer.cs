#nullable enable
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Startup referrer state exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxReferrer
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxReferrer(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Returns the original install-referrer details retained for this device, if any.
        /// </summary>
        public Task<AttriaxInstallReferrerDetails?> GetOriginalInstallReferrerAsync()
        {
            return _runtime.OriginalInstallReferrer;
        }

        /// <summary>
        /// Returns the latest reinstall-referrer details retained for this device, if any.
        /// </summary>
        public Task<AttriaxInstallReferrerDetails?> GetReinstallReferrerAsync()
        {
            return _runtime.ReinstallReferrer;
        }

        /// <summary>
        /// Deep-link referrer that opened the current session.
        /// </summary>
        /// <remarks>
        /// This waits for the startup deep-link flow to settle. It resolves to a
        /// cold-start or deferred deep-link referrer, or null when the current
        /// session started without one.
        /// </remarks>
        public Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync()
        {
            return _runtime.GetSessionReferrerAsync();
        }

        /// <summary>
        /// Most recent deep-link referrer observed in the current session.
        /// </summary>
        /// <remarks>
        /// If no deep link has been received yet, this waits for the next handled
        /// deep-link event.
        /// </remarks>
        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync()
        {
            return _runtime.GetLatestDeepLinkReferrerAsync();
        }
    }
}