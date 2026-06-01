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
    }
}