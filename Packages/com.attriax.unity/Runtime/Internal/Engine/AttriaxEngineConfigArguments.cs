#nullable enable
using System.Collections.Generic;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Serializes the JSON-representable <see cref="AttriaxConfig"/> surface into
    /// the engine <c>initialize</c> command arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The wire keys match the Flutter <c>AttriaxConfig.toJson</c> and the KMP
    /// <c>AttriaxConfig</c> fields 1:1, so a native binding forwards the map to the
    /// engine without translation. Interval fields are already stored in
    /// milliseconds on the Unity config (the Flutter equivalents are
    /// <c>Duration</c>s emitted as <c>*Ms</c> integers), so they map straight
    /// across.
    /// </para>
    /// <para>
    /// CCPA <c>doNotSell</c> / <c>usPrivacy</c> are emitted TOP-LEVEL (mirrors the
    /// ATT status), NOT nested under device context, and are omitted entirely when
    /// unset. Unity-only wrapper concerns (<c>AutomaticSceneTracking</c>,
    /// <c>StorageKeyPrefix</c>) are intentionally NOT forwarded — the native engine
    /// owns its own store and page-view tracking is a wrapper responsibility.
    /// </para>
    /// </remarks>
    internal static class AttriaxEngineConfigArguments
    {
        public const string DefaultApiBaseUrl = "https://api.attriax.com";

        public static IDictionary<string, object?> ToEngineArguments(this AttriaxConfig config)
        {
            var args = new Dictionary<string, object?>
            {
                ["projectToken"] = config.ProjectToken,
                ["apiBaseUrl"] = string.IsNullOrWhiteSpace(config.ApiBaseUrl)
                    ? DefaultApiBaseUrl
                    : config.ApiBaseUrl,
                ["enableDebugLogs"] = config.EnableDebugLogs,
                ["requestTimeoutMs"] = config.RequestTimeoutMs,
                ["maxQueueSize"] = config.MaxQueueSize,
                ["eventFlushIntervalMs"] = config.EventFlushIntervalMs,
                ["flushEventsImmediatelyOnFirstLaunch"] = config.FlushEventsImmediatelyOnFirstLaunch,
                ["collectAdvertisingId"] = config.CollectAdvertisingId,
                ["automaticCrashReportingEnabled"] = config.AutomaticCrashReportingEnabled,
                ["requestTrackingAuthorizationOnInit"] = config.RequestTrackingAuthorizationOnInit,
                ["trackingAuthorizationStatusTimeoutMs"] = config.TrackingAuthorizationStatusTimeoutMs,
                ["automaticBrowserHandling"] = config.AutomaticBrowserHandling,
                ["gdprEnabled"] = config.GdprEnabled,
                ["anonymousTracking"] = config.AnonymousTracking,
                ["sessionTrackingEnabled"] = config.SessionTrackingEnabled,
                ["sessionHeartbeatIntervalMs"] = config.SessionHeartbeatIntervalMs,
                ["firstLaunchSessionHeartbeatIntervalMs"] = config.FirstLaunchSessionHeartbeatIntervalMs,
            };

            if (!string.IsNullOrWhiteSpace(config.AppVersion))
            {
                args["appVersion"] = config.AppVersion;
            }

            if (!string.IsNullOrWhiteSpace(config.AppBuildNumber))
            {
                args["appBuildNumber"] = config.AppBuildNumber;
            }

            if (!string.IsNullOrWhiteSpace(config.AppPackageName))
            {
                args["appPackageName"] = config.AppPackageName;
            }

            if (config.SdkMetadata != null && config.SdkMetadata.Count > 0)
            {
                args["sdkMetadata"] = new Dictionary<string, object>(config.SdkMetadata);
            }

            if (config.Skan != null)
            {
                args["skan"] = new Dictionary<string, object?>
                {
                    ["enabled"] = config.Skan.Enabled,
                    ["registerFirstLaunchValue"] = config.Skan.RegisterFirstLaunchValue,
                };
            }

            // CCPA — top-level, omitted when unset.
            if (config.DoNotSell.HasValue)
            {
                args["doNotSell"] = config.DoNotSell.Value;
            }

            if (!string.IsNullOrEmpty(config.UsPrivacy))
            {
                args["usPrivacy"] = config.UsPrivacy;
            }

            return args;
        }
    }
}
