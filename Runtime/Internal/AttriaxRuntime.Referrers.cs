#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime
    {
        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync()
        {
            if (!_initialized)
            {
                return await Task.FromException<AttriaxDeepLinkReferrerDetails?>(
                    new InvalidOperationException(
                        "Attriax.InitializeAsync() must complete before reading deep-link referrer details."));
            }

            if (!_enabled)
            {
                return null;
            }

            var currentReferrer = ResolveSessionOpeningReferrer();
            if (currentReferrer != null)
            {
                return currentReferrer;
            }

            await WaitForInitialDeepLink.ConfigureAwait(false);
            var initialReferrer = ResolveSessionOpeningReferrer();
            if (initialReferrer != null)
            {
                return initialReferrer;
            }

            await _appOpenManager.WaitForPublicResultAsync().ConfigureAwait(false);
            return ResolveSessionOpeningReferrer();
        }

        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync()
        {
            if (!_initialized)
            {
                return Task.FromException<AttriaxDeepLinkReferrerDetails?>(
                    new InvalidOperationException(
                        "Attriax.InitializeAsync() must complete before reading deep-link referrer details."));
            }

            if (!_enabled)
            {
                return Task.FromResult<AttriaxDeepLinkReferrerDetails?>(null);
            }

            var currentReferrer = ResolveLatestDeepLinkReferrer();
            if (currentReferrer != null)
            {
                return Task.FromResult<AttriaxDeepLinkReferrerDetails?>(currentReferrer);
            }

            var taskSource = new TaskCompletionSource<AttriaxDeepLinkReferrerDetails?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            IDisposable? subscription = null;
            subscription = SubscribeToDeepLinks(deepLinkEvent =>
            {
                var nextReferrer = BuildDeepLinkReferrerDetailsForCurrentSession(deepLinkEvent);
                if (nextReferrer == null)
                {
                    return;
                }

                subscription?.Dispose();
                taskSource.TrySetResult(nextReferrer);
            });
            return taskSource.Task;
        }

        private AttriaxDeepLinkReferrerDetails? ResolveSessionOpeningReferrer()
        {
            return BuildDeepLinkReferrerDetailsForCurrentSession(_deepLinkManager.InitialDeepLinkValue, true)
                ?? BuildDeepLinkReferrerDetailsForCurrentSession(LatestDeepLink, true);
        }

        private AttriaxDeepLinkReferrerDetails? ResolveLatestDeepLinkReferrer()
        {
            return BuildDeepLinkReferrerDetailsForCurrentSession(LatestDeepLink);
        }

        private AttriaxDeepLinkReferrerDetails? BuildDeepLinkReferrerDetailsForCurrentSession(
            AttriaxDeepLinkEvent? deepLinkEvent,
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

            var currentSession = _sessionManager.CurrentSession;
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