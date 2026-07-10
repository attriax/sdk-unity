#nullable enable
using System.Collections.Generic;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRuntimeActivationCoordinatorTests
    {
        [Test]
        public void EntersDeferredModeWhenEnabledWhenDispatchIsExplicitlyDeferred()
        {
            var harness = new ActivationHarness();

            harness.Coordinator.SetEnabled(
                true,
                new AttriaxRuntimeActivationState(
                    shouldDeferNetworkDispatch: true,
                    allowsAttributionTracking: false,
                    shouldTrackAnything: true));

            Assert.That(harness.Calls, Is.EqualTo(new[]
            {
                "persistEnabled:True",
                "refreshLaunchPrioritization",
                "handleSdkEnabled",
                "prepareReferrers",
                "scheduleLaunchPreparation",
                "clearDeferredFlush",
                "setSynchronizationState:Deferred",
            }));
        }

        [Test]
        public void KeepsDeferredStateOnConsentChangeWhenDispatchIsExplicitlyDeferred()
        {
            var harness = new ActivationHarness();

            harness.Coordinator.HandleConsentStateChanged(
                enabled: true,
                state: new AttriaxRuntimeActivationState(
                    shouldDeferNetworkDispatch: true,
                    allowsAttributionTracking: false,
                    shouldTrackAnything: true));

            Assert.That(harness.Calls, Is.EqualTo(new[]
            {
                "refreshLaunchPrioritization",
                "syncRuntimePersistence",
                "rewriteQueueForConsent",
                "scheduleLaunchPreparation",
                "clearDeferredFlush",
                "setSynchronizationState:Deferred",
            }));
        }

        [Test]
        public void RequestsImmediateQueueFlushOnConsentChangeWhenAnonymousTrackingCanProceed()
        {
            var harness = new ActivationHarness
            {
                QueueCount = 1,
            };

            harness.Coordinator.HandleConsentStateChanged(
                enabled: true,
                state: new AttriaxRuntimeActivationState(
                    shouldDeferNetworkDispatch: false,
                    allowsAttributionTracking: false,
                    shouldTrackAnything: true));

            Assert.That(harness.Calls, Is.EqualTo(new[]
            {
                "refreshLaunchPrioritization",
                "syncRuntimePersistence",
                "rewriteQueueForConsent",
                "scheduleLaunchPreparation",
                "resolveDeniedAttribution",
                "requestImmediateQueueFlush",
            }));
        }

        [Test]
        public void DisablesSynchronizationWhenConsentBlocksEveryTrackingCategory()
        {
            var harness = new ActivationHarness();

            harness.Coordinator.HandleConsentStateChanged(
                enabled: true,
                state: new AttriaxRuntimeActivationState(
                    shouldDeferNetworkDispatch: false,
                    allowsAttributionTracking: false,
                    shouldTrackAnything: false));

            Assert.That(harness.Calls, Is.EqualTo(new[]
            {
                "refreshLaunchPrioritization",
                "syncRuntimePersistence",
                "rewriteQueueForConsent",
                "scheduleLaunchPreparation",
                "resolveDeniedAttribution",
                "clearDeferredFlush",
                "setSynchronizationState:Disabled",
            }));
        }

        [Test]
        public void SynchronizesWhenSomeTrackingRemainsWithoutAttribution()
        {
            var harness = new ActivationHarness();

            harness.Coordinator.SetEnabled(
                true,
                new AttriaxRuntimeActivationState(
                    shouldDeferNetworkDispatch: false,
                    allowsAttributionTracking: false,
                    shouldTrackAnything: true));

            Assert.That(harness.Calls, Is.EqualTo(new[]
            {
                "persistEnabled:True",
                "refreshLaunchPrioritization",
                "handleSdkEnabled",
                "prepareReferrers",
                "scheduleLaunchPreparation",
                "resolveDeniedAttribution",
                "requestImmediateQueueFlush",
                "setSynchronizationState:Synchronized",
            }));
        }

        private sealed class ActivationHarness
        {
            public ActivationHarness()
            {
                Coordinator = new AttriaxRuntimeActivationCoordinator(
                    persistEnabled: enabled => Calls.Add("persistEnabled:" + enabled),
                    refreshAppOpenLaunchPrioritization: () => Calls.Add("refreshLaunchPrioritization"),
                    clearDeferredFlush: () => Calls.Add("clearDeferredFlush"),
                    handleSdkDisabled: () => Calls.Add("handleSdkDisabled"),
                    handleSdkEnabled: () => Calls.Add("handleSdkEnabled"),
                    prepareReferrerTasksForEnabledState: () => Calls.Add("prepareReferrers"),
                    scheduleLaunchPreparationIfNeeded: () => Calls.Add("scheduleLaunchPreparation"),
                    syncRuntimePersistenceMode: () => Calls.Add("syncRuntimePersistence"),
                    rewriteAndPurgeQueuedRequestsForConsent: () => Calls.Add("rewriteQueueForConsent"),
                    resolveDeniedAttributionState: () => Calls.Add("resolveDeniedAttribution"),
                    queueCount: () => QueueCount,
                    requestImmediateQueueFlush: () => Calls.Add("requestImmediateQueueFlush"),
                    setSynchronizationState: state => Calls.Add("setSynchronizationState:" + state),
                        isInitialized: () => IsInitialized);
            }

            public List<string> Calls { get; } = new List<string>();

            public AttriaxRuntimeActivationCoordinator Coordinator { get; }

            public int QueueCount { get; set; }

            public bool IsInitialized { get; set; } = true;
        }
    }
}