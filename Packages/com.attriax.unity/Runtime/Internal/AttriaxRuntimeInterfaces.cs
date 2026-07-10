#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;

namespace Attriax.Unity.Internal
{
    internal interface IAttriaxContextRefreshProvider
    {
        Task<AttriaxPreparedContextRefresh> PrepareContextRefreshAsync(bool resolveInstallReferrer);
    }

    internal interface IAttriaxDeepLinkConversionResolver
    {
        Task<AttriaxDeepLinkEvent> ResolveDeepLinkConversionAsync(
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent? rawEvent,
            System.DateTimeOffset clickedAt);
    }

    internal interface IAttriaxAppOpenPipeline
    {
        Task<AttriaxAppOpenResult> EnqueueOpenAsync(
            string? installReferrerOverride,
            IDictionary<string, object>? deviceMetadataOverrides);

        Task ResolveInstallReferrerFromAppOpenAsync(Task<AttriaxAppOpenResult> openTrackingTask);

        AttriaxDeepLinkEvent? BuildDeepLinkEventFromAppOpenResult(AttriaxAppOpenResult result);

        AttriaxAppOpen? ToPublicAppOpen(AttriaxAppOpenResult? result);
    }

    internal interface IAttriaxSessionLifecycleQueue
    {
        void QueueSessionLifecycle(
            SdkSessionLifecycleKind kind,
            AttriaxSessionSnapshot session,
            System.DateTimeOffset occurredAt,
            System.Collections.Generic.IDictionary<string, object>? metadata = null);
    }
}