#nullable enable
using System;

namespace Attriax.Unity
{
    /// <summary>
    /// Error raised for Attriax API and transport failures.
    /// </summary>
    public sealed class AttriaxApiError : Exception
    {
        /// <summary>
        /// Creates a new API error instance.
        /// </summary>
        public AttriaxApiError(
            string message,
            int? statusCode = null,
            bool retriable = false,
            bool shouldDrop = false,
            Exception? innerException = null,
            DateTimeOffset? retryAfterAt = null) : base(message, innerException)
        {
            StatusCode = statusCode;
            Retriable = retriable;
            ShouldDrop = shouldDrop;
            RetryAfterAt = retryAfterAt;
        }

        /// <summary>
        /// HTTP status code when the error originated from an HTTP response.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Indicates whether retrying the request may succeed later.
        /// </summary>
        public bool Retriable { get; }

        /// <summary>
        /// Indicates whether the queued request should be discarded permanently.
        /// </summary>
        public bool ShouldDrop { get; }

        /// <summary>
        /// Absolute retry time derived from HTTP retry metadata when available.
        /// </summary>
        public DateTimeOffset? RetryAfterAt { get; }
    }
}