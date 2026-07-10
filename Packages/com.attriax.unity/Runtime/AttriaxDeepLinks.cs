#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Subscription handle for raw deep-link inputs captured by the SDK.
    /// </summary>
    public sealed class AttriaxRawDeepLinkStream
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxRawDeepLinkStream(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Subscribes to raw deep-link inputs captured directly by the SDK.
        /// </summary>
        public IDisposable Subscribe(Action<AttriaxRawDeepLinkEvent> listener)
        {
            return _runtime.SubscribeToRawDeepLinks(listener);
        }
    }

    /// <summary>
    /// Subscription handle for resolved deep-link events.
    /// </summary>
    public sealed class AttriaxDeepLinkStream
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxDeepLinkStream(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Subscribes to resolved deep-link events.
        /// </summary>
        public IDisposable Subscribe(Action<AttriaxDeepLinkEvent> listener)
        {
            return _runtime.SubscribeToDeepLinks(listener);
        }
    }

    /// <summary>
    /// Deep-link state and subscriptions exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxDeepLinks
    {
        private readonly Internal.AttriaxRuntime _runtime;

        internal AttriaxDeepLinks(Internal.AttriaxRuntime runtime)
        {
            _runtime = runtime;
            RawStream = new AttriaxRawDeepLinkStream(runtime);
            Stream = new AttriaxDeepLinkStream(runtime);
        }

        /// <summary>
        /// First raw deep-link event detected during startup, when one exists.
        /// </summary>
        public AttriaxRawDeepLinkEvent? RawInitialDeepLink
        {
            get { return _runtime.RawInitialDeepLinkValue; }
        }

        /// <summary>
        /// First deep-link event detected during startup, when one exists.
        /// </summary>
        /// <remarks>
        /// This stays <see langword="null"/> until the initial deep-link probe finishes.
        /// Use <see cref="InitialDeepLinkResolved"/> to distinguish "still pending"
        /// from "resolved and no initial deep link was found".
        /// </remarks>
        public AttriaxDeepLinkEvent? InitialDeepLink
        {
            get { return _runtime.InitialDeepLinkValue; }
        }

        /// <summary>
        /// Returns <see langword="true"/> once the initial deep-link probe has completed.
        /// </summary>
        public bool InitialDeepLinkResolved
        {
            get { return _runtime.InitialDeepLinkResolved; }
        }

        /// <summary>
        /// Task that waits for the initial deep-link probe to finish if it is still pending.
        /// </summary>
        public Task<AttriaxDeepLinkEvent?> WaitForInitialDeepLink
        {
            get { return _runtime.WaitForInitialDeepLink; }
        }

        /// <summary>
        /// Waits for the resolved deep-link event corresponding to a raw deep-link input.
        /// </summary>
        public Task<AttriaxDeepLinkEvent> WaitResolutionAsync(AttriaxRawDeepLinkEvent rawEvent)
        {
            return _runtime.WaitForDeepLinkResolutionAsync(rawEvent);
        }

        /// <summary>
        /// Creates a dynamic link through the Attriax backend and returns the final short URL.
        /// Optional iOS and Android redirect flags override project defaults for this link.
        /// </summary>
        public Task<AttriaxCreateDynamicLinkResult> CreateDynamicLinkAsync(
            AttriaxCreateDynamicLinkOptions? options = null)
        {
            return _runtime.CreateDynamicLinkAsync(options ?? new AttriaxCreateDynamicLinkOptions());
        }

        /// <summary>
        /// Resolves and records a deep-link conversion event for the provided URI.
        /// </summary>
        /// <remarks>
        /// Use this when your app receives a deep-link URI before the SDK captures
        /// it automatically. A non-empty <see cref="AttriaxDeepLinkConversionOptions.Uri"/>
        /// is required; calls with an empty URI throw <see cref="ArgumentException"/>.
        /// When Attriax does not recognize the link, the returned event still
        /// completes with its found flag set to <see langword="false"/>.
        /// </remarks>
        public Task<AttriaxDeepLinkEvent?> RecordDeepLinkConversionAsync(
            AttriaxDeepLinkConversionOptions options)
        {
            return _runtime.RecordDeepLinkConversionAsync(options);
        }

        /// <summary>
        /// Raw deep-link event subscription.
        /// </summary>
        public AttriaxRawDeepLinkStream RawStream { get; }

        /// <summary>
        /// Resolved deep-link event subscription.
        /// Deferred deep links resolved later by app-open tracking also flow here.
        /// </summary>
        public AttriaxDeepLinkStream Stream { get; }

        /// <summary>
        /// Most recent handled deep-link event seen by the SDK.
        /// </summary>
        public AttriaxDeepLinkEvent? LatestDeepLink
        {
            get { return _runtime.LatestDeepLink; }
        }
    }
}
