#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    [Serializable]
    internal sealed class AttriaxSessionSnapshot
    {
        public string Id { get; set; } = string.Empty;

        public string DeviceId { get; set; } = string.Empty;

        public AttriaxPlatformType Platform { get; set; }

        public string? Locale { get; set; }

        public bool IsFirstLaunch { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public DateTimeOffset LastActivityAt { get; set; }

        public int HeartbeatIntervalMs { get; set; }

        public string? AppVersion { get; set; }

        public string? AppBuildNumber { get; set; }

        public string? AppPackageName { get; set; }

        public string SdkPackageVersion { get; set; } = string.Empty;
    }

    internal readonly struct AttriaxSessionRestoreResult
    {
        public AttriaxSessionRestoreResult(
            AttriaxSessionSnapshot currentSession,
            bool startedNewSession,
            AttriaxSessionSnapshot? replacedSession)
        {
            CurrentSession = currentSession;
            StartedNewSession = startedNewSession;
            ReplacedSession = replacedSession;
        }

        public AttriaxSessionSnapshot CurrentSession { get; }

        public bool StartedNewSession { get; }

        public AttriaxSessionSnapshot? ReplacedSession { get; }
    }
}