#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Terminal-drop and backoff rules for queued request retries. Mirrors the
    /// Flutter <c>dispatch/request_retry_policy.dart</c> module.
    /// </summary>
    internal static class AttriaxRequestRetryPolicy
    {
        private const int MaxRetryAttempts = 8;
        private const int MaxBackoffExponent = 20;
        private static readonly TimeSpan MaxRetryAge = TimeSpan.FromDays(7);

        // Base delay for the first retry when the server does not send Retry-After.
        private static readonly TimeSpan RetryBaseBackoff = TimeSpan.FromSeconds(2);

        // Upper bound for a single backoff delay.
        private static readonly TimeSpan RetryMaxBackoff = TimeSpan.FromMinutes(5);

        public static bool IsWaitingForRetryWindow(AttriaxQueuedRequest request, DateTimeOffset now)
        {
            return request.NextRetryAt.HasValue && request.NextRetryAt.Value > now;
        }

        public static string? GetTerminalDropReason(AttriaxQueuedRequest request, DateTimeOffset now)
        {
            if (request.Kind == AttriaxQueuedRequestKind.DeepLinkResolve)
            {
                return null;
            }

            if (request.AttemptCount >= MaxRetryAttempts)
            {
                return "max_attempts_exceeded";
            }

            if (now - request.CreatedAt > MaxRetryAge)
            {
                return "max_age_exceeded";
            }

            return null;
        }

        public static AttriaxQueuedRequest MarkForRetry(
            AttriaxQueuedRequest request,
            AttriaxApiError error,
            DateTimeOffset attemptedAt)
        {
            request.AttemptCount += 1;
            request.LastAttemptAt = attemptedAt;
            request.LastErrorClass = BuildRetryErrorClass(error);
            request.LastHttpStatusCode = error.StatusCode;
            // A server-provided Retry-After always wins. Otherwise fall back to a
            // jittered exponential backoff keyed off the post-increment attempt
            // count so transient failures are re-attempted with growing spacing
            // instead of hammering a failing server at a fixed cadence.
            request.NextRetryAt = ResolveRetryAt(error, attemptedAt, request.AttemptCount);
            return request;
        }

        private static string BuildRetryErrorClass(AttriaxApiError error)
        {
            if (error.StatusCode.HasValue)
            {
                return "http_" + error.StatusCode.Value.ToString();
            }

            if (error.InnerException is TimeoutException)
            {
                return "timeout";
            }

            if (error.InnerException != null)
            {
                return error.InnerException.GetType().Name;
            }

            return error.GetType().Name;
        }

        private static DateTimeOffset ResolveRetryAt(
            AttriaxApiError error,
            DateTimeOffset attemptedAt,
            int attemptCount)
        {
            if (error.RetryAfterAt.HasValue && error.RetryAfterAt.Value > attemptedAt)
            {
                return error.RetryAfterAt.Value;
            }

            return BackoffRetryAt(attemptedAt, attemptCount);
        }

        // Capped exponential backoff. The delay doubles each attempt from
        // RetryBaseBackoff up to RetryMaxBackoff. A small deterministic jitter
        // derived from attemptedAt spreads retries across devices without
        // depending on a random source, keeping the result reproducible.
        private static DateTimeOffset BackoffRetryAt(DateTimeOffset attemptedAt, int attemptCount)
        {
            var exponent = Math.Min(Math.Max(attemptCount - 1, 0), MaxBackoffExponent);
            var scaledMs = RetryBaseBackoff.TotalMilliseconds * (1L << exponent);
            var cappedMs = Math.Min(RetryMaxBackoff.TotalMilliseconds, scaledMs);
            var jitterRange = (long)Math.Floor(cappedMs * 0.2);
            var jitterMs = jitterRange == 0
                ? 0L
                : Math.Abs(attemptedAt.ToUnixTimeMilliseconds() * 1000L) % (jitterRange + 1);
            return attemptedAt.AddMilliseconds(cappedMs + jitterMs);
        }
    }
}
