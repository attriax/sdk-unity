#nullable enable
using System;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// Push-notification token provider slugs registered through the engine
    /// <c>registerPushToken</c> command.
    /// </summary>
    /// <remarks>
    /// Mirrors the Flutter <c>AttriaxPushTokenProvider</c> and the KMP
    /// <c>AttriaxTracking.registerFirebaseMessagingToken</c> /
    /// <c>registerApplePushToken</c> split, which lower to a single
    /// uninstall-token wire keyed by the provider slug (<c>fcm</c> / <c>apns</c>).
    /// Internal Phase 1 surface — the public facade keeps its
    /// <c>RegisterFirebaseMessagingTokenAsync</c> / <c>RegisterApplePushTokenAsync</c>
    /// methods and lowers them onto this provider when it routes through the
    /// engine platform in a later phase.
    /// </remarks>
    internal enum AttriaxPushTokenProvider
    {
        /// <summary>
        /// Firebase Cloud Messaging (Android / cross-platform).
        /// </summary>
        Fcm,

        /// <summary>
        /// Apple Push Notification service (iOS / macOS).
        /// </summary>
        Apns,
    }

    /// <summary>
    /// Wire-slug mapping for <see cref="AttriaxPushTokenProvider"/>, matching the
    /// Flutter <c>AttriaxPushTokenProviderWire</c> and the KMP
    /// <c>UNINSTALL_TOKEN_PROVIDER_*</c> constants.
    /// </summary>
    internal static class AttriaxPushTokenProviderWire
    {
        public static string ToWireValue(this AttriaxPushTokenProvider provider)
        {
            switch (provider)
            {
                case AttriaxPushTokenProvider.Fcm:
                    return "fcm";
                case AttriaxPushTokenProvider.Apns:
                    return "apns";
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }
        }
    }

    /// <summary>
    /// Resolution outcome for the startup initial-link probe, delivered on the
    /// <see cref="IAttriaxEnginePlatform.InitialDeepLinkResolved"/> event.
    /// </summary>
    /// <remarks>
    /// Mirrors the Flutter <c>AttriaxInitialDeepLinkResolution</c> and the KMP
    /// <c>deepLinks.waitForInitialDeepLink()</c> completion: once the probe
    /// settles, <see cref="Resolved"/> becomes <c>true</c> and
    /// <see cref="DeepLink"/> carries the launch deep-link event, or <c>null</c>
    /// when the launch carried no deep link.
    /// </remarks>
    internal sealed class AttriaxInitialDeepLinkResolution
    {
        public AttriaxInitialDeepLinkResolution(bool resolved, AttriaxDeepLinkEvent? deepLink = null)
        {
            Resolved = resolved;
            DeepLink = deepLink;
        }

        /// <summary>
        /// Whether the initial-link probe has completed for this app session.
        /// </summary>
        public bool Resolved { get; }

        /// <summary>
        /// The launch deep-link event, when one was present.
        /// </summary>
        public AttriaxDeepLinkEvent? DeepLink { get; }
    }
}
