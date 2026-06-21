#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime
    {
        internal void BootstrapInitializeState(AttriaxInitOptions options)
        {
            SetSynchronizationState(AttriaxSynchronizationState.Initializing);
            _consentManager.Init();
            SyncRuntimePersistenceMode();

            _isFirstLaunch = !_settingsState.ReadHasLaunched(false);
            _referrerManager.EnsureTaskSources();
            _deepLinkManager.Reset();
        }

        internal async Task BootstrapPrepareContextAndSessionAsync(AttriaxInitOptions options)
        {
            await _skanManager.InitializeAsync(_isFirstLaunch).ConfigureAwait(false);

            if (_enabled)
            {
                EnsureReferrerTasksForEnabledState();
            }

            var preparedContext = ShouldMaterializeIdentifiedContext
                ? await PrepareIdentifiedContextAsync(
                        _isFirstLaunch,
                        _enabled && _consentManager.AllowsAttributionTracking)
                    .ConfigureAwait(false)
                : PrepareAnonymousContext(_isFirstLaunch);
            _resolvedPlatform = preparedContext.InitialSnapshot.Platform;
            _contextManager.SetPreparedContext(
                new AttriaxPreparedContextRefresh(
                    preparedContext.InitialSnapshot,
                    preparedContext.ResolvedSnapshotTask),
                _enabled && _consentManager.AllowsAttributionTracking);
            _sessionManager.Initialize(DateTimeOffset.UtcNow);
            _initialized = true;
            _shouldGateRequestsOnSuccessfulAppOpen = ShouldGateRequestsOnSuccessfulAppOpen;
            _deepLinkManager.BeginInitialProbe(options.CaptureInitialUrl);
            _settingsState.WriteHasLaunched(true);
            AttachLifecycle();
            DebugLog(
                "Initialization complete",
                "platform=" + _resolvedPlatform
                    + " enabled=" + _enabled
                    + " isFirstLaunch=" + _isFirstLaunch
                    + " identifiedContext=" + ShouldMaterializeIdentifiedContext
                    + " waitingForConsent=" + _consentManager.IsWaitingForConsent
                    + " hasDeviceId=" + !string.IsNullOrWhiteSpace(_deviceId));
        }

        internal void BootstrapCompleteDisabledState()
        {
            _referrerManager.CompleteOriginal(null, true);
            _referrerManager.CompleteReinstall(null, true);
            CompleteInitialDeepLink(null);
            SetSynchronizationState(AttriaxSynchronizationState.Disabled);
        }

        internal void BootstrapCompleteEnabledState(AttriaxInitOptions options)
        {
            ScheduleAppOpenIfNeeded();

            if (!_consentManager.IsWaitingForConsent && !_consentManager.AllowsAttributionTracking)
            {
                ObserveBackgroundTask(
                    ResolveLocalInstallReferrerForDeniedConsentAsync(),
                    "Local install-referrer resolution after GDPR denial failed.");
            }

            if (options.CaptureInitialUrl)
            {
                _ = CaptureInitialUrlAsync();
            }
            else
            {
                _deepLinkManager.MarkInitialUrlUnavailable();
            }

            if (_config.AutomaticSceneTracking)
            {
                var activeScene = SceneManager.GetActiveScene();
                if (!string.IsNullOrWhiteSpace(activeScene.name))
                {
                    ObserveBackgroundTask(
                        TrackPageViewAsync(activeScene.name, new AttriaxPageViewOptions
                        {
                            PageClass = activeScene.path,
                            PageTitle = activeScene.name,
                            Source = "automatic_scene",
                        }),
                        "Automatic Unity scene page tracking failed.");
                }
            }

            RequestQueueFlush(true);

            if (_requestQueue.Count == 0)
            {
                SetSynchronizationState(AttriaxSynchronizationState.Synchronized);
            }
        }
    }
}
