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
        private readonly Action _refreshAppOpenLaunchPrioritization;
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

        public AttriaxRuntimeActivationCoordinator(
            Action<bool> persistEnabled,
            Action refreshAppOpenLaunchPrioritization,
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
            Func<bool> isInitialized)
        {
            _persistEnabled = persistEnabled ?? throw new ArgumentNullException(nameof(persistEnabled));
            _refreshAppOpenLaunchPrioritization = refreshAppOpenLaunchPrioritization ?? throw new ArgumentNullException(nameof(refreshAppOpenLaunchPrioritization));
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
        }

        public void Apply(bool enabled, AttriaxRuntimeActivationState state)
        {
            _persistEnabled(enabled);
            _refreshAppOpenLaunchPrioritization();

            if (!enabled)
            {
                ApplyDisabledState(callHandleSdkDisabled: true);
                return;
            }

            _handleSdkEnabled();
            _prepareReferrerTasksForEnabledState();
            _scheduleLaunchPreparationIfNeeded();
            ApplyTrackingMode(
                state,
                requestFlushOnlyWhenQueuePending: false,
                synchronizeWhenQueueEmpty: true);
        }

        public void SetEnabled(bool enabled, AttriaxRuntimeActivationState state)
        {
            Apply(enabled, state);
        }

        public void HandleConsentStateChanged(bool enabled, AttriaxRuntimeActivationState state)
        {
            _refreshAppOpenLaunchPrioritization();
            _syncRuntimePersistenceMode();

            if (!_isInitialized())
            {
                return;
            }

            _rewriteAndPurgeQueuedRequestsForConsent();

            if (!enabled)
            {
                ApplyDisabledState(callHandleSdkDisabled: false);
                return;
            }

            _scheduleLaunchPreparationIfNeeded();

            ApplyTrackingMode(
                state,
                requestFlushOnlyWhenQueuePending: true,
                synchronizeWhenQueueEmpty: true);
        }

        private void ApplyTrackingMode(
            AttriaxRuntimeActivationState state,
            bool requestFlushOnlyWhenQueuePending,
            bool synchronizeWhenQueueEmpty)
        {
            if (state.ShouldDeferNetworkDispatch)
            {
                ApplyDeferredState();
                return;
            }

            if (!state.AllowsAttributionTracking)
            {
                _resolveDeniedAttributionState();
            }

            if (!state.ShouldTrackAnything)
            {
                ApplyNoTrackingState();
                return;
            }

            ApplyActiveState(
                requestFlushOnlyWhenQueuePending: requestFlushOnlyWhenQueuePending,
                synchronizeWhenQueueEmpty: synchronizeWhenQueueEmpty);
        }

        private void ApplyDisabledState(bool callHandleSdkDisabled)
        {
            _clearDeferredFlush();
            if (callHandleSdkDisabled)
            {
                _handleSdkDisabled();
            }

            _setSynchronizationState(AttriaxSynchronizationState.Disabled);
        }

        private void ApplyDeferredState()
        {
            _clearDeferredFlush();
            _setSynchronizationState(AttriaxSynchronizationState.Deferred);
        }

        private void ApplyNoTrackingState()
        {
            _clearDeferredFlush();
            _setSynchronizationState(AttriaxSynchronizationState.Disabled);
        }

        private void ApplyActiveState(
            bool requestFlushOnlyWhenQueuePending,
            bool synchronizeWhenQueueEmpty)
        {
            if (!requestFlushOnlyWhenQueuePending || _queueCount() > 0)
            {
                _requestImmediateQueueFlush();
            }

            if (synchronizeWhenQueueEmpty && _queueCount() == 0)
            {
                _setSynchronizationState(AttriaxSynchronizationState.Synchronized);
            }
        }
    }
}
