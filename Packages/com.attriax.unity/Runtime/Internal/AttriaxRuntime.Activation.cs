#nullable enable
using System;
namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime
    {
        private AttriaxRuntimeActivationState RuntimeActivationState
        {
            get
            {
                return new AttriaxRuntimeActivationState(
                    _consentManager.ShouldDeferNetworkDispatch,
                    _consentManager.AllowsAttributionTracking,
                    _consentManager.CanCaptureAttribution
                        || _consentManager.CanCaptureAnalytics
                        || _consentManager.CanCaptureAdEvents
                        || _consentManager.CanCaptureUninstallTracking);
            }
        }

        private void PersistEnabledState(bool enabled)
        {
            _settingsState.SetEnabled(enabled);
        }

        private void RefreshAppOpenLaunchPrioritization()
        {
            _shouldPrioritizeAppOpenLaunch = ShouldPrioritizeAppOpenLaunch;
        }

        private void ClearDeferredFlush()
        {
            _deferredFlushDueAt = null;
        }

        private void ScheduleLaunchPreparationIfNeeded()
        {
            if (_consentManager.ShouldDeferNetworkDispatch || !_consentManager.AllowsAttributionTracking)
            {
                return;
            }

            PrimeSdkRuntimeConfigForLaunch();
            ScheduleLaunchAppOpenIfNeeded();
        }

        private void ObserveDeniedAttributionStateResolution()
        {
            if (_consentManager.IsWaitingForConsent || _consentManager.AllowsAttributionTracking)
            {
                return;
            }

            ObserveBackgroundTask(
                ResolveLocalInstallReferrerForDeniedConsentAsync(),
                "Local install-referrer resolution after GDPR denial failed.");
        }

        private void RequestImmediateQueueFlush()
        {
            RequestQueueFlush(true);
        }
    }
}
