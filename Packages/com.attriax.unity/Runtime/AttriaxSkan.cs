#nullable enable
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Dashboard-managed SKAN state and manual update helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxSkan
    {
        private readonly Internal.IAttriaxEngine _runtime;

        internal AttriaxSkan(Internal.IAttriaxEngine runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Latest locally persisted SKAN runtime state tracked by the SDK.
        /// </summary>
        public AttriaxSkanState? State
        {
            get { return _runtime.SkanState; }
        }

        /// <summary>
        /// Manually requests a SKAdNetwork conversion-value update.
        /// Use this when the app wants to resolve SKAN values itself instead of relying
        /// on Attriax dashboard-managed event rules.
        /// </summary>
        public Task<AttriaxSkanUpdateResult> UpdateConversionValueAsync(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue = null,
            bool lockWindow = false)
        {
            return _runtime.UpdateSkanConversionValueAsync(fineValue, coarseValue, lockWindow);
        }
    }
}
