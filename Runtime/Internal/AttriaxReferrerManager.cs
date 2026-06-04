#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Mirrors Flutter's referrer manager responsibilities for install-referrer
    /// waiters and deep-link referrer lookup.
    /// </summary>
    internal sealed class AttriaxReferrerManager
    {
        private readonly AttriaxRuntimeState _runtimeState;
        private readonly Func<AttriaxDeepLinkEvent?> _initialDeepLinkProvider;
        private readonly Func<AttriaxDeepLinkEvent?> _latestDeepLinkProvider;
        private readonly Func<Task<AttriaxDeepLinkEvent?>> _waitForInitialDeepLink;
        private readonly Func<Task<AttriaxAppOpen?>> _waitForAppOpenTracking;
        private readonly Func<Action<AttriaxDeepLinkEvent>, IDisposable> _subscribeToDeepLinks;
        private readonly Func<AttriaxSessionSnapshot?> _currentSessionProvider;

        private readonly AttriaxInstallReferrerState _originalInstallReferrerState =
            new AttriaxInstallReferrerState();
        private readonly AttriaxInstallReferrerState _reinstallInstallReferrerState =
            new AttriaxInstallReferrerState();

        public AttriaxReferrerManager(
            AttriaxRuntimeState runtimeState,
            Func<AttriaxDeepLinkEvent?> initialDeepLinkProvider,
            Func<AttriaxDeepLinkEvent?> latestDeepLinkProvider,
            Func<Task<AttriaxDeepLinkEvent?>> waitForInitialDeepLink,
            Func<Task<AttriaxAppOpen?>> waitForAppOpenTracking,
            Func<Action<AttriaxDeepLinkEvent>, IDisposable> subscribeToDeepLinks,
            Func<AttriaxSessionSnapshot?> currentSessionProvider)
        {
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            _initialDeepLinkProvider = initialDeepLinkProvider ?? throw new ArgumentNullException(nameof(initialDeepLinkProvider));
            _latestDeepLinkProvider = latestDeepLinkProvider ?? throw new ArgumentNullException(nameof(latestDeepLinkProvider));
            _waitForInitialDeepLink = waitForInitialDeepLink ?? throw new ArgumentNullException(nameof(waitForInitialDeepLink));
            _waitForAppOpenTracking = waitForAppOpenTracking ?? throw new ArgumentNullException(nameof(waitForAppOpenTracking));
            _subscribeToDeepLinks = subscribeToDeepLinks ?? throw new ArgumentNullException(nameof(subscribeToDeepLinks));
            _currentSessionProvider = currentSessionProvider ?? throw new ArgumentNullException(nameof(currentSessionProvider));
        }

        public Task<AttriaxInstallReferrerDetails?> OriginalInstallReferrer => _originalInstallReferrerState.Task;

        public Task<AttriaxInstallReferrerDetails?> ReinstallReferrer => _reinstallInstallReferrerState.Task;

        public void EnsureTaskSources()
        {
            _originalInstallReferrerState.EnsureTaskSource();
            _reinstallInstallReferrerState.EnsureTaskSource();
        }

        public void PrepareForEnabledState(
            AttriaxInstallReferrerDetails? originalInstallReferrer,
            AttriaxInstallReferrerDetails? reinstallReferrer)
        {
            _originalInstallReferrerState.PrepareForEnabledState(originalInstallReferrer);
            _reinstallInstallReferrerState.PrepareForEnabledState(reinstallReferrer);
        }

        public void CompleteOriginal(
            AttriaxInstallReferrerDetails? installReferrerDetails,
            bool disabledResult = false)
        {
            _originalInstallReferrerState.Complete(installReferrerDetails, disabledResult);
        }

        public void CompleteReinstall(
            AttriaxInstallReferrerDetails? installReferrerDetails,
            bool disabledResult = false)
        {
            _reinstallInstallReferrerState.Complete(installReferrerDetails, disabledResult);
        }

        public void Reset()
        {
            _originalInstallReferrerState.Reset();
            _reinstallInstallReferrerState.Reset();
        }

        public void CompleteAllWithNull()
        {
            _originalInstallReferrerState.Complete(null);
            _reinstallInstallReferrerState.Complete(null);
        }

        public async Task<AttriaxInstallReferrerDetails?> GetLegacyInstallReferrerAsync()
        {
            var reinstallReferrer = await ReinstallReferrer.ConfigureAwait(false);
            if (reinstallReferrer != null)
            {
                return reinstallReferrer;
            }

            return await OriginalInstallReferrer.ConfigureAwait(false);
        }

        public async Task<AttriaxDeepLinkReferrerDetails?> GetSessionReferrerAsync()
        {
            if (!_runtimeState.IsInitialized)
            {
                return await Task.FromException<AttriaxDeepLinkReferrerDetails?>(
                    new InvalidOperationException(
                        "Attriax.InitializeAsync() must complete before reading deep-link referrer details."));
            }

            if (!_runtimeState.IsEnabled)
            {
                return null;
            }

            var currentReferrer = ResolveSessionOpeningReferrer();
            if (currentReferrer != null)
            {
                return currentReferrer;
            }

            await _waitForInitialDeepLink().ConfigureAwait(false);
            var initialReferrer = ResolveSessionOpeningReferrer();
            if (initialReferrer != null)
            {
                return initialReferrer;
            }

            await _waitForAppOpenTracking().ConfigureAwait(false);
            return ResolveSessionOpeningReferrer();
        }

        public Task<AttriaxDeepLinkReferrerDetails?> GetLatestDeepLinkReferrerAsync()
        {
            if (!_runtimeState.IsInitialized)
            {
                return Task.FromException<AttriaxDeepLinkReferrerDetails?>(
                    new InvalidOperationException(
                        "Attriax.InitializeAsync() must complete before reading deep-link referrer details."));
            }

            if (!_runtimeState.IsEnabled)
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
            subscription = _subscribeToDeepLinks(deepLinkEvent =>
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
            return BuildDeepLinkReferrerDetailsForCurrentSession(_initialDeepLinkProvider(), true)
                ?? BuildDeepLinkReferrerDetailsForCurrentSession(_latestDeepLinkProvider(), true);
        }

        private AttriaxDeepLinkReferrerDetails? ResolveLatestDeepLinkReferrer()
        {
            return BuildDeepLinkReferrerDetailsForCurrentSession(_latestDeepLinkProvider());
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

            var currentSession = _currentSessionProvider();
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
