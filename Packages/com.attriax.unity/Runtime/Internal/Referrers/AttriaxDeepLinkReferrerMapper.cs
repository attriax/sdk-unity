#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors Flutter's <c>attriax_deep_link_referrer_mapper.dart</c>: maps a
    /// deep-link event/observation to referrer details for the current session,
    /// applying the session-opening and observed-at-before-StartedAt staleness
    /// filtering.
    /// </summary>
    internal static class AttriaxDeepLinkReferrerMapper
    {
        public static AttriaxDeepLinkReferrerDetails? BuildForCurrentSession(
            AttriaxDeepLinkEvent? deepLinkEvent,
            AttriaxSessionSnapshot? currentSession,
            bool requireSessionOpeningEvent = false)
        {
            if (deepLinkEvent == null)
            {
                return null;
            }

            if (requireSessionOpeningEvent && !IsSessionOpeningEvent(deepLinkEvent))
            {
                return null;
            }

            if (currentSession != null && GetDeepLinkSessionObservedAt(deepLinkEvent) < currentSession.StartedAt)
            {
                return null;
            }

            return new AttriaxDeepLinkReferrerDetails
            {
                Uri = deepLinkEvent.Uri,
                ReceivedAt = deepLinkEvent.RawEvent?.ReceivedAt ?? deepLinkEvent.ClickedAt,
                ClickedAt = deepLinkEvent.ClickedAt,
                ConsumedAt = deepLinkEvent.ConsumedAt,
                Trigger = deepLinkEvent.Trigger,
                IsAttriaxDomain = deepLinkEvent.IsAttriaxSubDomain,
                Found = deepLinkEvent.Found,
                Data = deepLinkEvent.Data,
                Utm = deepLinkEvent.Utm,
                BrowserAction = deepLinkEvent.BrowserAction,
                HandledBySdk = deepLinkEvent.HandledBySdk,
            };
        }

        private static bool IsSessionOpeningEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            return deepLinkEvent.IsColdStart || deepLinkEvent.IsDeferred;
        }

        private static DateTimeOffset GetDeepLinkSessionObservedAt(AttriaxDeepLinkEvent deepLinkEvent)
        {
            return deepLinkEvent.RawEvent?.ReceivedAt ?? deepLinkEvent.ConsumedAt;
        }
    }
}
