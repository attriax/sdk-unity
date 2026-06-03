#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime
    {
        private sealed class AttriaxRuntimeBootstrapCoordinator
        {
            private readonly AttriaxRuntime _runtime;

            public AttriaxRuntimeBootstrapCoordinator(AttriaxRuntime runtime)
            {
                _runtime = runtime;
            }

            public async Task RunAsync(AttriaxInitOptions options)
            {
                _runtime.SetSynchronizationState(AttriaxSynchronizationState.Initializing);
                _runtime._consentManager.Init();
                _runtime.SyncRuntimePersistenceMode();

                if (options.Enabled.HasValue)
                {
                    _runtime._enabled = options.Enabled.Value;
                    _runtime.WriteBoolean(_runtime.Key("enabled"), _runtime._enabled);
                }

                if (options.EventsEnabled.HasValue)
                {
                    _runtime._eventsEnabled = options.EventsEnabled.Value;
                    _runtime.WriteBoolean(_runtime.Key("eventsEnabled"), _runtime._eventsEnabled);
                }

                _runtime._isFirstLaunch = !_runtime._runtimeSettingsStore.ReadHasLaunched(false);
                _runtime._originalInstallReferrerState.EnsureTaskSource();
                _runtime._reinstallInstallReferrerState.EnsureTaskSource();
                _runtime._deepLinkManager.Reset();
                await _runtime._skanManager.InitializeAsync(_runtime._isFirstLaunch).ConfigureAwait(false);

                if (_runtime._enabled)
                {
                    _runtime.EnsureReferrerTasksForEnabledState();
                }

                var preparedContext = _runtime.ShouldMaterializeIdentifiedContext
                    ? await _runtime.PrepareIdentifiedContextAsync(
                            _runtime._isFirstLaunch,
                            _runtime._enabled && _runtime._consentManager.AllowsAttributionTracking)
                        .ConfigureAwait(false)
                    : _runtime.PrepareAnonymousContext(_runtime._isFirstLaunch);
                _runtime._resolvedPlatform = preparedContext.InitialSnapshot.Platform;
                _runtime._contextManager.SetPreparedContext(
                    new AttriaxPreparedContextRefresh(
                        preparedContext.InitialSnapshot,
                        preparedContext.ResolvedSnapshotTask),
                    _runtime._enabled && _runtime._consentManager.AllowsAttributionTracking);
                _runtime._sessionManager.Initialize(DateTimeOffset.UtcNow);
                _runtime._initialized = true;
                _runtime._shouldGateRequestsOnSuccessfulAppOpen = _runtime.ShouldGateRequestsOnSuccessfulAppOpen;
                _runtime._deepLinkManager.BeginInitialProbe(options.CaptureInitialUrl);
                _runtime._runtimeSettingsStore.WriteHasLaunched(true);
                _runtime.AttachLifecycle();

                if (!_runtime._enabled)
                {
                    _runtime.CompleteOriginalInstallReferrer(null, true);
                    _runtime.CompleteReinstallInstallReferrer(null, true);
                    _runtime.CompleteInitialDeepLink(null);
                    _runtime.SetSynchronizationState(AttriaxSynchronizationState.Disabled);
                    return;
                }

                _runtime.ScheduleAppOpenIfNeeded();

                if (!_runtime._consentManager.IsWaitingForConsent && !_runtime._consentManager.AllowsAttributionTracking)
                {
                    _runtime.ObserveBackgroundTask(
                        _runtime.ResolveLocalInstallReferrerForDeniedConsentAsync(),
                        "Local install-referrer resolution after GDPR denial failed.");
                }

                if (options.CaptureInitialUrl)
                {
                    _ = _runtime.CaptureInitialUrlAsync();
                }
                else
                {
                    _runtime._deepLinkManager.MarkInitialUrlUnavailable();
                }

                if (_runtime._config.AutomaticSceneTracking)
                {
                    var activeScene = SceneManager.GetActiveScene();
                    if (!string.IsNullOrWhiteSpace(activeScene.name))
                    {
                        _runtime.ObserveBackgroundTask(
                            _runtime.TrackPageViewAsync(activeScene.name, new AttriaxPageViewOptions
                            {
                                PageClass = activeScene.path,
                                PageTitle = activeScene.name,
                                Source = "automatic_scene",
                            }),
                            "Automatic Unity scene page tracking failed.");
                    }
                }

                _runtime.RequestQueueFlush(true);

                if (_runtime._requestQueue.Count == 0)
                {
                    _runtime.SetSynchronizationState(AttriaxSynchronizationState.Synchronized);
                }
            }
        }
    }
}
