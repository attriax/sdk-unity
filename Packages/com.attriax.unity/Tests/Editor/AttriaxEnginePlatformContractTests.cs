#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity.Internal.Engine;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    /// <summary>
    /// Phase 1 contract tests: exercise <see cref="IAttriaxEnginePlatform"/> through
    /// <see cref="FakeEnginePlatform"/> to validate the command/getter/event surface
    /// and its wire mapping (mirrors the Flutter fake-platform tests).
    /// </summary>
    public sealed class AttriaxEnginePlatformContractTests
    {
        private FakeEnginePlatform _engine = null!;
        private IAttriaxEnginePlatform Engine => _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new FakeEnginePlatform();
        }

        // -- Commands ---------------------------------------------------------

        [Test]
        public async Task RecordEventForwardsNameAndFlushFlagWithOmittedNullData()
        {
            await Engine.RecordEvent("level_up", flushImmediately: true);

            var command = _engine.CommandFor("recordEvent");
            Assert.That(command, Is.Not.Null);
            Assert.That(command!.Arg<string>("name"), Is.EqualTo("level_up"));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.True);
            Assert.That(command.HasArg("eventData"), Is.False, "null eventData must be omitted from the wire map");
        }

        [Test]
        public async Task RecordEventForwardsProvidedEventData()
        {
            var data = new Dictionary<string, object> { ["coins"] = 10 };

            await Engine.RecordEvent("purchase_prompt", data);

            var command = _engine.CommandFor("recordEvent");
            Assert.That(command!.HasArg("eventData"), Is.True);
            Assert.That(command.Arg<IDictionary<string, object>>("eventData"), Is.SameAs(data));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.False);
        }

        [Test]
        public async Task RecordPurchaseForwardsRequiredFieldsAndOmitsNullOptionals()
        {
            await Engine.RecordPurchase(4.99, currency: "EUR", productId: "coins_100");

            var command = _engine.CommandFor("recordPurchase");
            Assert.That(command!.Arg<double>("revenue"), Is.EqualTo(4.99));
            Assert.That(command.Arg<string>("currency"), Is.EqualTo("EUR"));
            Assert.That(command.Arg<string>("productId"), Is.EqualTo("coins_100"));
            Assert.That(command.Arg<int>("quantity"), Is.EqualTo(1));
            Assert.That(command.Arg<bool>("flushImmediately"), Is.True, "purchase defaults to immediate flush");
            Assert.That(command.HasArg("transactionId"), Is.False);
            Assert.That(command.HasArg("metadata"), Is.False);
        }

        [Test]
        public async Task RegisterPushTokenLowersProviderToWireSlug()
        {
            await Engine.RegisterPushToken(AttriaxPushTokenProvider.Apns, "token-123");

            var command = _engine.CommandFor("registerPushToken");
            Assert.That(command!.Arg<string>("provider"), Is.EqualTo("apns"));
            Assert.That(command.Arg<string>("token"), Is.EqualTo("token-123"));

            await Engine.RegisterPushToken(AttriaxPushTokenProvider.Fcm, null);
            Assert.That(_engine.CommandFor("registerPushToken")!.Arg<string>("provider"), Is.EqualTo("fcm"));
        }

        // -- CCPA command semantics ------------------------------------------

        [Test]
        public async Task SetCcpaConsentIncludesOnlyProvidedFields()
        {
            await Engine.SetCcpaConsent(doNotSell: null, usPrivacy: "1YNN");

            var command = _engine.CommandFor("setCcpaConsent");
            Assert.That(command, Is.Not.Null);
            Assert.That(command!.HasArg("doNotSell"), Is.False, "null doNotSell must be omitted");
            Assert.That(command.Arg<string>("usPrivacy"), Is.EqualTo("1YNN"));
        }

        [Test]
        public async Task SetCcpaConsentForwardsExplicitFalseToClearLatch()
        {
            await Engine.SetCcpaConsent(doNotSell: false, usPrivacy: null);

            var command = _engine.CommandFor("setCcpaConsent");
            Assert.That(command!.HasArg("doNotSell"), Is.True, "explicit doNotSell:false must be forwarded");
            Assert.That(command.Arg<bool>("doNotSell"), Is.False);
            Assert.That(command.HasArg("usPrivacy"), Is.False);
        }

        // -- CCPA config serialization ---------------------------------------

        [Test]
        public async Task InitializeSerializesCcpaTopLevelWhenSet()
        {
            var config = new AttriaxConfig
            {
                ProjectToken = "ax_test",
                DoNotSell = true,
                UsPrivacy = "1YYN",
            };

            await Engine.InitializeAsync(config);

            var command = _engine.CommandFor("initialize");
            var serialized = command!.Arg<IDictionary<string, object?>>("config");
            Assert.That(serialized, Is.Not.Null);
            Assert.That(serialized!["projectToken"], Is.EqualTo("ax_test"));
            Assert.That(serialized["doNotSell"], Is.EqualTo(true));
            Assert.That(serialized["usPrivacy"], Is.EqualTo("1YYN"));
        }

        [Test]
        public void ConfigSerializationOmitsCcpaWhenUnset()
        {
            var serialized = new AttriaxConfig { ProjectToken = "ax_test" }.ToEngineArguments();

            Assert.That(serialized.ContainsKey("doNotSell"), Is.False);
            Assert.That(serialized.ContainsKey("usPrivacy"), Is.False);
            // Interval fields carry through in milliseconds under the KMP wire keys.
            Assert.That(serialized["eventFlushIntervalMs"], Is.EqualTo(60000));
            Assert.That(serialized["sessionHeartbeatIntervalMs"], Is.EqualTo(300000));
            Assert.That(serialized["apiBaseUrl"], Is.EqualTo(AttriaxEngineConfigArguments.DefaultApiBaseUrl));
        }

        // -- Getters ----------------------------------------------------------

        [Test]
        public async Task GettersReturnConfiguredValues()
        {
            _engine.DeviceIdValue = "device-42";
            _engine.IsInitializedValue = true;
            _engine.SynchronizationStateValue = AttriaxSynchronizationState.Synchronized;
            _engine.DoNotSellValue = true;
            _engine.UsPrivacyValue = "1YYN";

            Assert.That(await Engine.GetDeviceId(), Is.EqualTo("device-42"));
            Assert.That(await Engine.GetIsInitialized(), Is.True);
            Assert.That(await Engine.GetSynchronizationState(), Is.EqualTo(AttriaxSynchronizationState.Synchronized));
            Assert.That(await Engine.GetDoNotSell(), Is.True);
            Assert.That(await Engine.GetUsPrivacy(), Is.EqualTo("1YYN"));
        }

        [Test]
        public async Task NeedsGdprConsentRecordsLocalOnlyArgAndReturnsConfiguredValue()
        {
            _engine.NeedsGdprConsentValue = true;

            var result = await Engine.NeedsGdprConsent(localOnly: true);

            Assert.That(result, Is.True);
            Assert.That(_engine.CommandFor("needsGdprConsent")!.Arg<bool>("localOnly"), Is.True);
        }

        // -- Events -----------------------------------------------------------

        [Test]
        public void SynchronizationStateEventRaisesToSubscriber()
        {
            AttriaxSynchronizationState? observed = null;
            Engine.SynchronizationStateChanged += state => observed = state;

            _engine.RaiseSynchronizationStateChanged(AttriaxSynchronizationState.Offline);

            Assert.That(observed, Is.EqualTo(AttriaxSynchronizationState.Offline));
        }

        [Test]
        public void DeepLinkEventsRaiseToSubscribers()
        {
            AttriaxRawDeepLinkEvent? rawObserved = null;
            AttriaxDeepLinkEvent? resolvedObserved = null;
            AttriaxInitialDeepLinkResolution? initialObserved = null;

            Engine.RawDeepLinkReceived += raw => rawObserved = raw;
            Engine.DeepLinkResolved += resolved => resolvedObserved = resolved;
            Engine.InitialDeepLinkResolved += resolution => initialObserved = resolution;

            var raw = new AttriaxRawDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/promo"), IsInitial = true };
            var resolvedEvent = new AttriaxDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/promo"), Found = true };

            _engine.RaiseRawDeepLinkReceived(raw);
            _engine.RaiseDeepLinkResolved(resolvedEvent);
            _engine.RaiseInitialDeepLinkResolved(new AttriaxInitialDeepLinkResolution(true, resolvedEvent));

            Assert.That(rawObserved, Is.SameAs(raw));
            Assert.That(resolvedObserved, Is.SameAs(resolvedEvent));
            Assert.That(initialObserved, Is.Not.Null);
            Assert.That(initialObserved!.Resolved, Is.True);
            Assert.That(initialObserved.DeepLink, Is.SameAs(resolvedEvent));
        }

        // -- Command return values -------------------------------------------

        [Test]
        public async Task RecordDeepLinkReturnsConfiguredResolvedEvent()
        {
            var resolved = new AttriaxDeepLinkEvent { Uri = new Uri("https://demo.attriax.com/x"), Found = true };
            _engine.RecordDeepLinkResult = resolved;

            var result = await Engine.RecordDeepLink(new Uri("https://demo.attriax.com/x"));

            Assert.That(result, Is.SameAs(resolved));
            Assert.That(_engine.CommandFor("recordDeepLink")!.Arg<string>("uri"), Is.EqualTo("https://demo.attriax.com/x"));
        }
    }
}
