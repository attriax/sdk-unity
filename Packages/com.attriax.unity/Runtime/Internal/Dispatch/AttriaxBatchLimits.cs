#nullable enable

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Upper bounds applied when packing queued items into a single batch
    /// request. Mirrors the Flutter <c>dispatch/batch_limits.dart</c> module.
    /// </summary>
    internal static class AttriaxBatchLimits
    {
        /// <summary>Maximum number of items packed into one batch.</summary>
        public const int MaxItemCount = 100;

        /// <summary>Maximum encoded body size for one batch, in bytes (256 KiB, matching the API limit).</summary>
        public const int MaxBodyBytes = 256 * 1024;
    }
}
