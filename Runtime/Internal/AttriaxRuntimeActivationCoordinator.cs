#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxRuntimeActivationState
    {
        public AttriaxRuntimeActivationState(
            bool shouldDeferNetworkDispatch,
            bool allowsAttributionTracking,
            bool shouldTrackAnything)
        {
            ShouldDeferNetworkDispatch = shouldDeferNetworkDispatch;
            AllowsAttributionTracking = allowsAttributionTracking;
            ShouldTrackAnything = shouldTrackAnything;
        }

        public bool ShouldDeferNetworkDispatch { get; }

        public bool AllowsAttributionTracking { get; }

        public bool ShouldTrackAnything { get; }
    }

    internal sealed class AttriaxRuntimeActivationCoordinator
    {
        private readonly Action<bool> _persistEnabled;
        private readonly Action _refreshAppOpenDispatchGate;
        private readonly Action _clearDeferredFlush;
        private readonly Action _handleSdkDisabled;
        private readonly Action _handleSdkEnabled;
        private readonly Action _prepareReferrerTasksForEnabledState;
        private readonly Action _scheduleLaunchPreparationIfNeeded;
        private readonly Action _syncRuntimePersistenceMode;
        private readonly Action _rewriteAndPurgeQueuedRequestsForConsent;
        private readonly Action _resolveDeniedAttributionState;
        private readonly Func<int> _queueCount;
        private readonly Action _requestImmediateQueueFlush;
        private readonly Action<AttriaxSynchronizationState> _setSynchronizationState;
        private readonly Func<bool> _isInitialized;
        private readonly Func<bool> _isEditorValidationMode;

        public AttriaxRuntimeActivationCoordinator(
            Action<bool> persistEnabled,
            Action refreshAppOpenDispatchGate,
            Action clearDeferredFlush,
            Action handleSdkDisabled,
            Action handleSdkEnabled,
            Action prepareReferrerTasksForEnabledState,
            Action scheduleLaunchPreparationIfNeeded,
            Action syncRuntimePersistenceMode,
            Action rewriteAndPurgeQueuedRequestsForConsent,
            Action resolveDeniedAttributionState,
            Func<int> queueCount,
            Action requestImmediateQueueFlush,
            Action<AttriaxSynchronizationState> setSynchronizationState,
            Func<bool> isInitialized,
            Func<bool> isEditorValidationMode)
        {
            _persistEnabled = persistEnabled ?? throw new ArgumentNullException(nameof(persistEnabled));
            _refreshAppOpenDispatchGate = refreshAppOpenDispatchGate ?? throw new ArgumentNullException(nameof(refreshAppOpenDispatchGate));
            _clearDeferredFlush = clearDeferredFlush ?? throw new ArgumentNullException(nameof(clearDeferredFlush));
            _handleSdkDisabled = handleSdkDisabled ?? throw new ArgumentNullException(nameof(handleSdkDisabled));
            _handleSdkEnabled = handleSdkEnabled ?? throw new ArgumentNullException(nameof(handleSdkEnabled));
            _prepareReferrerTasksForEnabledState = prepareReferrerTasksForEnabledState ?? throw new ArgumentNullException(nameof(prepareReferrerTasksForEnabledState));
            _scheduleLaunchPreparationIfNeeded = scheduleLaunchPreparationIfNeeded ?? throw new ArgumentNullException(nameof(scheduleLaunchPreparationIfNeeded));
            _syncRuntimePersistenceMode = syncRuntimePersistenceMode ?? throw new ArgumentNullException(nameof(syncRuntimePersistenceMode));
            _rewriteAndPurgeQueuedRequestsForConsent = rewriteAndPurgeQueuedRequestsForConsent ?? throw new ArgumentNullException(nameof(rewriteAndPurgeQueuedRequestsForConsent));
            _resolveDeniedAttributionState = resolveDeniedAttributionState ?? throw new ArgumentNullException(nameof(resolveDeniedAttributionState));
            _queueCount = queueCount ?? throw new ArgumentNullException(nameof(queueCount));
            _requestImmediateQueueFlush = requestImmediateQueueFlush ?? throw new ArgumentNullException(nameof(requestImmediateQueueFlush));
            _setSynchronizationState = setSynchronizationState ?? throw new ArgumentNullException(nameof(setSynchronizationState));
            _isInitialized = isInitialized ?? throw new ArgumentNullException(nameof(isInitialized));
            _isEditorValidationMode = isEditorValidationMode ?? throw new ArgumentNullException(nameof(isEditorValidationMode));
        }

        public void SetEnabled(bool enabled, AttriaxRuntimeActivationState state)
        {
            _persistEnabled(enabled);
            _refreshAppOpenDispatchGate();

            if (!enabled)
            {
                _clearDeferredFlush();
                _handleSdkDisabled();
                _setSynchronizationState(AttriaxSynchronizationState.Disabled);
                return;
            }

            _handleSdkEnabled();
            _prepareReferrerTasksForEnabledState();
            _scheduleLaunchPreparationIfNeeded();

            if (_isEditorValidationMode())
            {
                _setSynchronizationState(AttriaxSynchronizationState.Synchronized);
                return;
            }

            if (state.ShouldDeferNetworkDispatch)
            {
                _clearDeferredFlush();
                _setSynchronizationState(AttriaxSynchronizationState.Deferred);
                return;
            }

            if (!state.AllowsAttributionTracking)
            {
                _resolveDeniedAttributionState();
            }

            if (!state.ShouldTrackAnything)
            {
                _clearDeferredFlush();
                _setSynchronizationState(AttriaxSynchronizationState.Disabled);
                return;
            }

            _requestImmediateQueueFlush();
            if (_queueCount() == 0)
            {
                _setSynchronizationState(AttriaxSynchronizationState.Synchronized);
            }
        }

        public void HandleConsentStateChanged(bool enabled, AttriaxRuntimeActivationState state)
        {
            _refreshAppOpenDispatchGate();
            _syncRuntimePersistenceMode();

            if (!_isInitialized())
            {
                return;
            }

            _rewriteAndPurgeQueuedRequestsForConsent();

            if (!enabled)
            {
                _clearDeferredFlush();
                _setSynchronizationState(AttriaxSynchronizationState.Disabled);
                return;
            }

            _scheduleLaunchPreparationIfNeeded();

            if (_isEditorValidationMode())
            {
                _setSynchronizationState(AttriaxSynchronizationState.Synchronized);
                return;
            }

            if (state.ShouldDeferNetworkDispatch)
            {
                _clearDeferredFlush();
                _setSynchronizationState(AttriaxSynchronizationState.Deferred);
                return;
            }

            if (!state.AllowsAttributionTracking)
            {
                _resolveDeniedAttributionState();
            }

            if (!state.ShouldTrackAnything)
            {
                _clearDeferredFlush();
                _setSynchronizationState(AttriaxSynchronizationState.Disabled);
                return;
            }

            if (_queueCount() > 0)
            {
                _requestImmediateQueueFlush();
                return;
            }

            _setSynchronizationState(AttriaxSynchronizationState.Synchronized);
        }
    }
}