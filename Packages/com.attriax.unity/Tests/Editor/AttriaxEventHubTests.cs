#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxEventHubTests
    {
        [Test]
        public void SynchronizationSubscribersReceiveStateChangesInOrder()
        {
            var hub = new AttriaxEventHub();
            var states = new List<AttriaxSynchronizationState>();

            using var subscription = hub.SubscribeToSynchronization(states.Add);

            hub.SetSynchronizationState(AttriaxSynchronizationState.Synchronizing);
            hub.SetSynchronizationState(AttriaxSynchronizationState.Synchronized);

            Assert.That(states, Is.EqualTo(new[]
            {
                AttriaxSynchronizationState.Synchronizing,
                AttriaxSynchronizationState.Synchronized,
            }));
        }

        [Test]
        public void DisposedSynchronizationSubscriptionStopsFutureNotifications()
        {
            var hub = new AttriaxEventHub();
            var states = new List<AttriaxSynchronizationState>();

            var subscription = hub.SubscribeToSynchronization(states.Add);
            hub.SetSynchronizationState(AttriaxSynchronizationState.Synchronizing);

            subscription.Dispose();
            hub.SetSynchronizationState(AttriaxSynchronizationState.Failed);

            Assert.That(states, Is.EqualTo(new[]
            {
                AttriaxSynchronizationState.Synchronizing,
            }));
        }

        [Test]
        public void ResetRestoresTheInitializingSynchronizationState()
        {
            var hub = new AttriaxEventHub();

            hub.SetSynchronizationState(AttriaxSynchronizationState.Offline);
            hub.Reset();

            Assert.That(hub.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Initializing));
        }

        [Test]
        public void RawDeepLinkSubscribersReceiveEmittedEvents()
        {
            var hub = new AttriaxEventHub();
            AttriaxRawDeepLinkEvent? observed = null;
            var deepLinkEvent = new AttriaxRawDeepLinkEvent
            {
                Uri = new Uri("https://demo.attriax.com/promo/spring-launch"),
                ReceivedAt = DateTimeOffset.UtcNow,
                IsInitial = false,
            };

            using var subscription = hub.SubscribeToRawDeepLinks(value => observed = value);

            hub.EmitRawDeepLinkEvent(deepLinkEvent);

            Assert.That(observed, Is.SameAs(deepLinkEvent));
        }

        [Test]
        public void DeepLinkSubscribersReceiveEmittedEvents()
        {
            var hub = new AttriaxEventHub();
            AttriaxDeepLinkEvent? observed = null;
            var deepLinkEvent = new AttriaxDeepLinkEvent
            {
                Uri = new Uri("https://demo.attriax.com/promo/spring-launch"),
                ClickedAt = DateTimeOffset.UtcNow,
                ConsumedAt = DateTimeOffset.UtcNow,
                Found = true,
                Trigger = AttriaxDeepLinkTrigger.Foreground,
            };

            using var subscription = hub.SubscribeToDeepLinks(value => observed = value);

            hub.EmitDeepLinkEvent(deepLinkEvent);

            Assert.That(observed, Is.SameAs(deepLinkEvent));
        }

        [Test]
        public void DeepLinkEventMarksAttriaxManagedSubdomains()
        {
            var attriaxEvent = new AttriaxDeepLinkEvent
            {
                Uri = new Uri("https://demo.attriax.com/promo/spring-launch"),
            };
            var customDomainEvent = new AttriaxDeepLinkEvent
            {
                Uri = new Uri("https://app.example.com/promo/spring-launch"),
            };

            Assert.That(attriaxEvent.IsAttriaxSubDomain, Is.True);
            Assert.That(customDomainEvent.IsAttriaxSubDomain, Is.False);
        }
    }
}