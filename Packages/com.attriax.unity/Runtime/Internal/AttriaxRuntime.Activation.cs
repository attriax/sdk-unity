#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

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
            _enabled = enabled;
            _runtimeSettingsStore.WriteEnabled(enabled);
        }

        private void RefreshAppOpenDispatchGate()
        {
            _shouldGateRequestsOnSuccessfulAppOpen = ShouldGateRequestsOnSuccessfulAppOpen;
        }

        private void ClearDeferredFlush()
        {
            _deferredFlushDueAt = null;
        }

        private void ScheduleLaunchPreparationIfNeeded()
        {
            if (IsUnityEditorValidationMode)
            {
                ScheduleUnityEditorValidationIfNeeded();
                return;
            }

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

        private void ScheduleUnityEditorValidationIfNeeded()
        {
            if (_unityEditorValidationTask != null)
            {
                return;
            }

            var validationTask = ValidateUnityEditorAsync();
            _unityEditorValidationTask = validationTask;
            ObserveBackgroundTask(
                AwaitUnityEditorValidationAsync(validationTask),
                "Unity Editor validation failed.");
        }

        private async Task AwaitUnityEditorValidationAsync(Task validationTask)
        {
            try
            {
                await validationTask.ConfigureAwait(false);
            }
            finally
            {
                if (ReferenceEquals(_unityEditorValidationTask, validationTask))
                {
                    _unityEditorValidationTask = null;
                }
            }
        }

        private Task ValidateUnityEditorAsync()
        {
            return _generatedGateway.SendValidateUnityEditorAsync(
                AttriaxGeneratedRequestFactory.BuildUnityEditorValidateRequest(
                    _config.AppToken,
                    SdkPackageVersion,
                    Application.unityVersion,
                    DetectEditorHostPlatform()));
        }
    }
}
