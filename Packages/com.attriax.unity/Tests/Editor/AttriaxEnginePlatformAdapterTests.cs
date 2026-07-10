#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity.Internal;
using Attriax.Unity.Internal.Engine;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    /// <summary>
    /// Exercises <see cref="AttriaxEnginePlatformAdapter"/> — the generic bridge from a
    /// native <see cref="IAttriaxEnginePlatform"/> onto the facade-facing
    /// <see cref="IAttriaxEngine"/> surface — against a <see cref="FakeEnginePlatform"/>.
    /// Covers the four bridging jobs: option→primitive lowering, sync-getter caching,
    /// event bridging to the Subscribe handles, and disposal teardown.
    /// </summary>
    public sealed class AttriaxEnginePlatformAdapterTests
    {
        private FakeEnginePlatform _platform = null!;
        private AttriaxEnginePlatformAdapter _adapter = null!;
        private IAttriaxEngine Engine => _adapter;

        [SetUp]
        public void SetUp()
        {
            _platform = new FakeEnginePlatform();
            _adapter = new AttriaxEnginePlatformAdapter(
                new AttriaxConfig { ProjectToken = "ax_test" },
                _platform);
        }

        [TearDown]
        public void TearDown()
        {
            _adapter.Dispose();
        }

        // -- (a) command lowering: option -> primitive ------------------------

        [Test]
        public async Task RecordEventLowersOptionToPrimitiveArgs()
        {
            var data = new Dictionary<string, object> { ["coins"] = 10 };

            await Engine.RecordEventAsync("level_up", new AttriaxTrackEventOptions
            {
                EventData = data,
                FlushImmediately = true,
            });

            var command = _platform.CommandFor("recordEvent");
            Assert.That(command!.Arg<string>("name"), Is.EqualTo("level_up"));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.True);
            Assert.That(command.Arg<IDictionary<string, object>>("eventData"), Is.SameAs(data));
        }

        [Test]
        public async Task RecordPurchaseDefaultsFlushImmediatelyToTrueAndOmitsNulls()
        {
            await Engine.RecordPurchaseAsync(4.99, new AttriaxRecordPurchaseOptions
            {
                Currency = "EUR",
                ProductId = "coins_100",
            });

            var command = _platform.CommandFor("recordPurchase");
            Assert.That(command!.Arg<double>("revenue"), Is.EqualTo(4.99));
            Assert.That(command.Arg<string>("currency"), Is.EqualTo("EUR"));
            Assert.That(command.Arg<string>("productId"), Is.EqualTo("coins_100"));
            Assert.That(command.Arg<int>("quantity"), Is.EqualTo(1));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.True, "purchase defaults to immediate flush");
            Assert.That(command.HasArg("transactionId"), Is.False);
        }

        [Test]
        public async Task RecordAdEventLowersTypeToReservedEventName()
        {
            await Engine.RecordAdEventAsync(AttriaxAdEventType.LoadFailed, new AttriaxRecordAdEventOptions
            {
                AdNetwork = "admob",
                FailureReason = "no_fill",
            });

            var command = _platform.CommandFor("recordAdEvent");
            Assert.That(command!.Arg<string>("eventName"), Is.EqualTo("ad_load_failed"));
            Assert.That(command.Arg<string>("adNetwork"), Is.EqualTo("admob"));
            Assert.That(command.Arg<string>("failureReason"), Is.EqualTo("no_fill"));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.True);
        }

        [Test]
        public async Task RecordNotificationLowersTypeAndSourceToWireSlugs()
        {
            await Engine.RecordNotificationAsync(
                AttriaxNotificationEventType.Opened,
                "notif-1",
                new AttriaxRecordNotificationOptions
                {
                    Source = AttriaxNotificationEventSource.Apns,
                    CampaignId = "camp-9",
                });

            var command = _platform.CommandFor("recordNotification");
            Assert.That(command!.Arg<string>("type"), Is.EqualTo("opened"));
            Assert.That(command.Arg<string>("notificationId"), Is.EqualTo("notif-1"));
            Assert.That(command.Arg<string>("source"), Is.EqualTo("apns"));
            Assert.That(command.Arg<string>("campaignId"), Is.EqualTo("camp-9"));
        }

        [Test]
        public async Task RecordErrorLowersExceptionToPrimitiveWireFields()
        {
            var error = new InvalidOperationException("boom");

            await Engine.RecordErrorAsync(error, new AttriaxRecordErrorOptions
            {
                IsFatal = true,
                Source = "unhandled",
                Reason = "test",
            });

            var command = _platform.CommandFor("recordError");
            Assert.That(command!.Arg<string>("message"), Is.EqualTo("boom"));
            Assert.That(command.Arg<string>("exceptionType"), Is.EqualTo("InvalidOperationException"));
            Assert.That(command.Arg<bool>("fatal"), Is.True);
            Assert.That(command.Arg<string>("source"), Is.EqualTo("unhandled"));
            Assert.That(command.Arg<string>("reason"), Is.EqualTo("test"));
        }

        [Test]
        public async Task SetUserDecomposesOptionsAcrossPrimitiveCommands()
        {
            await Engine.SetUserAsync("user-7", new AttriaxSetUserOptions
            {
                UserName = "Ada",
                Properties = new Dictionary<string, object> { ["tier"] = "gold" },
                ClearPropertyKeys = new List<string> { "legacy" },
            });

            var setUser = _platform.CommandFor("setUser");
            Assert.That(setUser!.Arg<string>("userId"), Is.EqualTo("user-7"));
            Assert.That(setUser.Arg<string>("userName"), Is.EqualTo("Ada"));

            Assert.That(_platform.CommandFor("clearUserProperties"), Is.Not.Null, "ClearPropertyKeys must forward");
            var setProps = _platform.CommandFor("setUserProperties");
            Assert.That(setProps, Is.Not.Null, "Properties must forward");
        }

        [Test]
        public async Task RegisterFirebaseMessagingTokenLowersToFcmProvider()
        {
            await Engine.RegisterFirebaseMessagingTokenAsync("token-abc");

            var command = _platform.CommandFor("registerPushToken");
            Assert.That(command!.Arg<string>("provider"), Is.EqualTo("fcm"));
            Assert.That(command.Arg<string>("token"), Is.EqualTo("token-abc"));
        }

        // -- (b) sync getters served from cache -------------------------------

        [Test]
        public async Task SyncGettersReturnCachedValuesAfterInit()
        {
            _platform.DeviceIdValue = "device-42";
            _platform.IsFirstLaunchValue = true;
            _platform.SdkSnapshotValue = new AttriaxSdkSnapshot { PackageVersion = "9.9.9" };
            _platform.SynchronizationStateValue = AttriaxSynchronizationState.Synchronized;

            Assert.That(Engine.IsInitialized, Is.False, "not initialized before init");
            Assert.That(Engine.DeviceId, Is.Null);

            await Engine.InitializeAsync(new AttriaxInitOptions());

            Assert.That(Engine.IsInitialized, Is.True);
            Assert.That(Engine.DeviceId, Is.EqualTo("device-42"));
            Assert.That(Engine.IsFirstLaunch, Is.True);
            Assert.That(Engine.SdkSnapshot!.PackageVersion, Is.EqualTo("9.9.9"));
            Assert.That(Engine.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Synchronized));
        }

        [Test]
        public async Task SyncGetterUpdatesAfterPlatformEvent()
        {
            await Engine.InitializeAsync(new AttriaxInitOptions());
            Assert.That(Engine.SynchronizationState, Is.Not.EqualTo(AttriaxSynchronizationState.Offline));

            _platform.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Offline);

            Assert.That(Engine.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Offline));
        }

        [Test]
        public async Task LatestDeepLinkCacheUpdatesFromPlatformEvent()
        {
            await Engine.InitializeAsync(new AttriaxInitOptions());
            var resolved = new AttriaxDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/x"), Found = true };

            _platform.RaiseDeepLinkResolved(resolved);

            Assert.That(Engine.LatestDeepLink, Is.SameAs(resolved));
        }

        [Test]
        public async Task UpdateSkanConversionValueRefreshesCachedSkanState()
        {
            await Engine.InitializeAsync(new AttriaxInitOptions());
            Assert.That(Engine.SkanState, Is.Null);

            var newState = new AttriaxSkanState();
            _platform.SkanStateValue = newState;
            _platform.UpdateSkanConversionValueResult =
                new AttriaxSkanUpdateResult { Status = AttriaxSkanUpdateStatus.Updated };

            var result = await Engine.UpdateSkanConversionValueAsync(3, AttriaxSkanCoarseValue.High, false);

            Assert.That(result.Status, Is.EqualTo(AttriaxSkanUpdateStatus.Updated));
            Assert.That(Engine.SkanState, Is.SameAs(newState), "cache refreshed from GetSkanState after update");
        }

        [Test]
        public void SetGdprConsentCachesGrantedStateAndValues()
        {
            Assert.That(Engine.GdprConsentState, Is.EqualTo(AttriaxGdprConsentState.Unknown));

            Engine.SetGdprConsent(analytics: true, attribution: false, adEvents: true);

            Assert.That(Engine.GdprConsentState, Is.EqualTo(AttriaxGdprConsentState.Granted));
            Assert.That(Engine.GdprConsentValues!.Analytics, Is.True);
            Assert.That(Engine.GdprConsentValues.Attribution, Is.False);
            Assert.That(Engine.GdprConsentValues.AdEvents, Is.True);
            Assert.That(Engine.IsWaitingForGdprConsent, Is.False);
            Assert.That(_platform.CommandFor("setGdprConsent"), Is.Not.Null);
        }

        [Test]
        public void SetEnabledFlipsCacheAndForwardsToPlatform()
        {
            Engine.SetEnabled(false);

            Assert.That(Engine.Enabled, Is.False);
            var command = _platform.CommandFor("setSdkEnabled");
            Assert.That(command!.Arg<bool>("enabled"), Is.False);
        }

        // -- (c) platform events bridge to the Subscribe handles --------------

        [Test]
        public void SubscribeHandlesReceiveBridgedPlatformEvents()
        {
            AttriaxSynchronizationState? sync = null;
            AttriaxDeepLinkEvent? deepLink = null;
            AttriaxRawDeepLinkEvent? raw = null;

            Engine.SubscribeToSynchronization(state => sync = state);
            Engine.SubscribeToDeepLinks(evt => deepLink = evt);
            Engine.SubscribeToRawDeepLinks(evt => raw = evt);

            var resolved = new AttriaxDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/y"), Found = true };
            var rawEvent = new AttriaxRawDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/y"), IsInitial = true };

            _platform.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Synchronizing);
            _platform.RaiseDeepLinkResolved(resolved);
            _platform.RaiseRawDeepLinkReceived(rawEvent);

            Assert.That(sync, Is.EqualTo(AttriaxSynchronizationState.Synchronizing));
            Assert.That(deepLink, Is.SameAs(resolved));
            Assert.That(raw, Is.SameAs(rawEvent));
        }

        [Test]
        public void UnsubscribingStopsDelivery()
        {
            var count = 0;
            var subscription = Engine.SubscribeToSynchronization(_ => count += 1);

            _platform.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Synchronizing);
            subscription.Dispose();
            _platform.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Offline);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task InitialDeepLinkResolvesFromPlatformResolutionEvent()
        {
            await Engine.InitializeAsync(new AttriaxInitOptions());
            var resolved = new AttriaxDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/launch"), Found = true };

            _platform.RaiseInitialDeepLinkResolved(new AttriaxInitialDeepLinkResolution(true, resolved));

            Assert.That(Engine.InitialDeepLinkResolved, Is.True);
            Assert.That(Engine.InitialDeepLinkValue, Is.SameAs(resolved));
            Assert.That(await Engine.WaitForInitialDeepLink, Is.SameAs(resolved));
        }

        // -- (d) disposal teardown --------------------------------------------

        [Test]
        public void DisposeStopsEventDeliveryAndForwardsToPlatform()
        {
            var count = 0;
            Engine.SubscribeToSynchronization(_ => count += 1);

            _adapter.Dispose();

            _platform.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Offline);

            Assert.That(count, Is.EqualTo(0), "no delivery after dispose");
            Assert.That(_platform.CommandFor("dispose"), Is.Not.Null, "platform.Dispose forwarded");
        }
    }
}
