#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors Flutter's <c>attriax_session_continuation_policy.dart</c>: decides
    /// whether a persisted session can be continued for a new activity, and
    /// resolves the continuation window from the session's heartbeat interval.
    /// </summary>
    internal static class AttriaxSessionContinuationPolicy
    {
        public static bool ShouldContinue(
            AttriaxSessionSnapshot session,
            string? deviceId,
            AttriaxContextSnapshot context,
            DateTimeOffset occurredAt)
        {
            if (!string.Equals(session.DeviceId, deviceId, StringComparison.Ordinal) ||
                session.Platform != context.Platform)
            {
                return false;
            }

            if (!string.Equals(session.AppPackageName, context.App.PackageName, StringComparison.Ordinal) ||
                !string.Equals(session.AppVersion, context.App.Version, StringComparison.Ordinal) ||
                !string.Equals(session.AppBuildNumber, context.App.BuildNumber, StringComparison.Ordinal))
            {
                return false;
            }

            if (occurredAt < session.StartedAt)
            {
                return false;
            }

            var continuationWindowMs = ResolveContinuationWindowMs(session.HeartbeatIntervalMs);
            return (occurredAt - session.LastActivityAt).TotalMilliseconds <= continuationWindowMs;
        }

        // Continuation window = 2 x heartbeat, clamped to [60s, 30min]. The lower
        // bound stops a tiny or misconfigured heartbeat (e.g. the 30s first-launch
        // default) from collapsing attribution-sensitive sessions on a brief
        // background; the upper bound stops an unusually large heartbeat from
        // keeping a session continuable for an unbounded time.
        public static double ResolveContinuationWindowMs(long heartbeatIntervalMs)
        {
            const double minWindowMs = 60000d;
            const double maxWindowMs = 1800000d;
            var raw = heartbeatIntervalMs * 2d;
            if (raw < minWindowMs)
            {
                return minWindowMs;
            }

            if (raw > maxWindowMs)
            {
                return maxWindowMs;
            }

            return raw;
        }
    }
}
