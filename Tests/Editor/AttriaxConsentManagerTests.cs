#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;
using NUnit.Framework;
using UnityEngine;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxConsentManagerTests
    {
        private readonly List<string> _storageKeys = new List<string>();

        [Test]
        public void PublicConsentFacadeExposesGdprHelpers()
        {
            using var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
            });

            Assert.That(attriax.Consent, Is.Not.Null);
            Assert.That(attriax.Consent.Gdpr, Is.Not.Null);
            Assert.That(attriax.Consent.Gdpr.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var storageKey in _storageKeys)
            {
                PlayerPrefs.DeleteKey(storageKey);
            }

            PlayerPrefs.Save();
            _storageKeys.Clear();
        }

        [Test]
        public void InitKeepsUnknownWithoutDeferringAnonymousTracking()
        {
            var gateway = new RecordingConsentGateway();
            var manager = CreateManager(
                gateway,
                timezone: "Europe/Berlin",
                gdprEnabled: true);

            manager.Init();

            var analyticsDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.Analytics);
            var sessionDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.Session);
            var attributionDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.Attribution);

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
            Assert.That(manager.IsWaitingForConsent, Is.True);
            Assert.That(manager.ShouldDeferNetworkDispatch, Is.False);
            Assert.That(analyticsDecision.Capture, Is.True);
            Assert.That(analyticsDecision.IdentityMode, Is.EqualTo(AttriaxTrackingIdentityMode.Anonymous));
            Assert.That(analyticsDecision.DeferNetwork, Is.False);
            Assert.That(analyticsDecision.SendNetworkDirectly, Is.True);
            Assert.That(sessionDecision.Capture, Is.True);
            Assert.That(sessionDecision.DeferNetwork, Is.False);
            Assert.That(attributionDecision.Capture, Is.False);
            Assert.That(attributionDecision.DeferNetwork, Is.False);
            Assert.That(gateway.CheckCalls, Is.EqualTo(0));
            Assert.That(gateway.UpsertCalls, Is.EqualTo(0));
        }

        [Test]
        public void InitDoesNotResolveTimezoneWithoutNetworkConsentCheck()
        {
            var gateway = new RecordingConsentGateway();
            var manager = CreateManager(
                gateway,
                timezone: "W. Europe Standard Time",
                gdprEnabled: true);

            manager.Init();

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
            Assert.That(manager.IsWaitingForConsent, Is.True);
            Assert.That(gateway.CheckCalls, Is.EqualTo(0));
        }

        [Test]
        public void UnknownConsentAllowsAnonymousCapableSignalsWithoutNetworkDeferral()
        {
            var gateway = new RecordingConsentGateway();
            var manager = CreateManager(
                gateway,
                timezone: "Attriax/Unknown",
                gdprEnabled: true);

            manager.Init();

            var analyticsDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.Analytics);
            var deepLinkDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.DeepLink);
            var uninstallDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.UninstallTracking);

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
            Assert.That(manager.IsWaitingForConsent, Is.True);
            Assert.That(manager.ShouldDeferNetworkDispatch, Is.False);
            Assert.That(analyticsDecision.Capture, Is.True);
            Assert.That(analyticsDecision.IdentityMode, Is.EqualTo(AttriaxTrackingIdentityMode.Anonymous));
            Assert.That(analyticsDecision.DeferNetwork, Is.False);
            Assert.That(analyticsDecision.SendNetworkDirectly, Is.True);
            Assert.That(deepLinkDecision.Capture, Is.True);
            Assert.That(deepLinkDecision.DeferNetwork, Is.False);
            Assert.That(deepLinkDecision.SendNetworkDirectly, Is.True);
            Assert.That(uninstallDecision.Capture, Is.False);
            Assert.That(uninstallDecision.DeferNetwork, Is.False);
            Assert.That(gateway.CheckCalls, Is.EqualTo(0));
            Assert.That(gateway.UpsertCalls, Is.EqualTo(0));
        }

        [Test]
        public void UnknownConsentDefersAnonymousCapableSignalsWhenAnonymousTrackingIsDisabled()
        {
            var gateway = new RecordingConsentGateway();
            var manager = CreateManager(
                gateway,
                timezone: "Attriax/Unknown",
                gdprEnabled: true,
                anonymousTracking: false);

            manager.Init();

            var analyticsDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.Analytics);
            var deepLinkDecision = manager.TrackingDecisionFor(AttriaxTrackingSignal.DeepLink);

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
            Assert.That(manager.IsWaitingForConsent, Is.True);
            Assert.That(manager.AnonymousTrackingEnabled, Is.False);
            Assert.That(manager.ShouldDeferNetworkDispatch, Is.True);
            Assert.That(analyticsDecision.Capture, Is.True);
            Assert.That(analyticsDecision.IdentityMode, Is.EqualTo(AttriaxTrackingIdentityMode.Anonymous));
            Assert.That(analyticsDecision.DeferNetwork, Is.True);
            Assert.That(analyticsDecision.SendNetworkDirectly, Is.False);
            Assert.That(deepLinkDecision.Capture, Is.True);
            Assert.That(deepLinkDecision.DeferNetwork, Is.True);
            Assert.That(gateway.CheckCalls, Is.EqualTo(0));
            Assert.That(gateway.UpsertCalls, Is.EqualTo(0));
        }

        [Test]
        public async Task NeedsConsentUsesRemoteCheckWhenGdprIsDisabled()
        {
            var gateway = new RecordingConsentGateway
            {
                NextCheckStatus = new SdkGdprConsentStatusDto(
                    checkedAt: DateTime.UtcNow,
                    countryCode: "US",
                    needsConsent: false,
                    regionSource: "ip",
                    state: AppUserGdprConsentState.NotRequired,
                    values: null),
            };
            var manager = CreateManager(
                gateway,
                timezone: "America/New_York",
                gdprEnabled: false);

            var needsConsent = await manager.NeedsConsentAsync().ConfigureAwait(false);

            Assert.That(needsConsent, Is.False);
            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.NotRequired));
            Assert.That(gateway.CheckCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task LocalOnlyConsentChecksRefreshUnresolvedPendingState()
        {
            var gateway = new RecordingConsentGateway();
            var timezone = "Europe/Berlin";
            var manager = CreateManager(
                gateway,
                resolveTimezone: () => timezone,
                gdprEnabled: true);

            var initialNeedsConsent = await manager.NeedsConsentAsync(localOnly: true).ConfigureAwait(false);

            Assert.That(initialNeedsConsent, Is.True);
            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Pending));

            timezone = "Europe/Kiev";

            var refreshedNeedsConsent = await manager.NeedsConsentAsync(localOnly: true).ConfigureAwait(false);

            Assert.That(refreshedNeedsConsent, Is.False);
            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.NotRequired));
            Assert.That(gateway.CheckCalls, Is.EqualTo(0));
        }

        [Test]
        public async Task RemoteConsentChecksRefreshUnresolvedPendingState()
        {
            var gateway = new RecordingConsentGateway
            {
                NextCheckStatusFactory = callCount => new SdkGdprConsentStatusDto(
                    checkedAt: DateTime.UtcNow,
                    countryCode: callCount == 1 ? "DE" : "UA",
                    needsConsent: callCount == 1,
                    regionSource: "ip_country",
                    state: callCount == 1
                        ? AppUserGdprConsentState.Pending
                        : AppUserGdprConsentState.NotRequired,
                    values: null),
            };
            var manager = CreateManager(
                gateway,
                timezone: "Europe/Kiev",
                gdprEnabled: true);

            var initialNeedsConsent = await manager.NeedsConsentAsync().ConfigureAwait(false);
            var refreshedNeedsConsent = await manager.NeedsConsentAsync().ConfigureAwait(false);

            Assert.That(initialNeedsConsent, Is.True);
            Assert.That(refreshedNeedsConsent, Is.False);
            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.NotRequired));
            Assert.That(gateway.CheckCalls, Is.EqualTo(2));
        }

        [Test]
        public async Task SetConsentPersistsAndSyncsGrantedValues()
        {
            var gateway = new RecordingConsentGateway
            {
                NextUpsertStatus = new SdkGdprConsentStatusDto(
                    checkedAt: DateTime.UtcNow,
                    countryCode: "DE",
                    needsConsent: false,
                    regionSource: "manual",
                    state: AppUserGdprConsentState.Granted,
                    values: new SdkGdprConsentValuesDto(
                        adEvents: true,
                        analytics: true,
                        attribution: false)),
            };
            var storageKey = NewStorageKey();
            var manager = CreateManager(
                gateway,
                storageKey: storageKey,
                timezone: "Europe/Berlin",
                gdprEnabled: true);

            manager.SetConsent(analytics: true, attribution: false, adEvents: true);
            await manager.FlushPendingSyncAsync().ConfigureAwait(false);

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Granted));
            Assert.That(manager.Values, Is.Not.Null);
            Assert.That(manager.Values!.Analytics, Is.True);
            Assert.That(manager.Values!.Attribution, Is.False);
            Assert.That(manager.Values!.AdEvents, Is.True);
            Assert.That(gateway.UpsertCalls, Is.EqualTo(1));

            var restoredManager = CreateManager(
                gateway,
                storageKey: storageKey,
                timezone: "Europe/Berlin",
                gdprEnabled: true);
            restoredManager.Init();

            Assert.That(restoredManager.State, Is.EqualTo(AttriaxGdprConsentState.Granted));
            Assert.That(restoredManager.Values, Is.Not.Null);
            Assert.That(restoredManager.Values!.Analytics, Is.True);
            Assert.That(restoredManager.Values!.Attribution, Is.False);
            Assert.That(restoredManager.Values!.AdEvents, Is.True);
        }

        [Test]
        public async Task ResetClearsPersistedConsentAndForcesRemoteRecheck()
        {
            var gateway = new RecordingConsentGateway
            {
                NextUpsertStatusFactory = _ => new SdkGdprConsentStatusDto(
                    checkedAt: DateTime.UtcNow,
                    countryCode: null,
                    needsConsent: true,
                    regionSource: null,
                    state: AppUserGdprConsentState.Unknown,
                    values: null),
                NextCheckStatus = new SdkGdprConsentStatusDto(
                    checkedAt: DateTime.UtcNow,
                    countryCode: "UA",
                    needsConsent: false,
                    regionSource: "ip_country",
                    state: AppUserGdprConsentState.NotRequired,
                    values: null),
            };
            var storageKey = NewStorageKey();
            var manager = CreateManager(
                gateway,
                storageKey: storageKey,
                timezone: "Europe/Kiev",
                gdprEnabled: true);

            manager.Reset();
            await manager.FlushPendingSyncAsync().ConfigureAwait(false);

            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.Unknown));
            Assert.That(PlayerPrefs.HasKey(storageKey), Is.False);
            Assert.That(gateway.UpsertCalls, Is.EqualTo(1));
            Assert.That(gateway.UpsertRequests[0].state, Is.EqualTo(AppUserGdprConsentState.Unknown));

            var needsConsent = await manager.NeedsConsentAsync().ConfigureAwait(false);

            Assert.That(needsConsent, Is.False);
            Assert.That(manager.State, Is.EqualTo(AttriaxGdprConsentState.NotRequired));
            Assert.That(gateway.CheckCalls, Is.EqualTo(1));
        }

        private AttriaxConsentManager CreateManager(
            RecordingConsentGateway gateway,
            string? storageKey = null,
            string? timezone = null,
            Func<string?>? resolveTimezone = null,
            bool gdprEnabled = true,
            bool anonymousTracking = true)
        {
            return new AttriaxConsentManager(
                new AttriaxPlayerPrefsConsentStore(
                    storageKey ?? NewStorageKey(),
                    (_, _) => { }),
                appToken: "ax_test",
                gdprEnabled: gdprEnabled,
                anonymousTracking: anonymousTracking,
                ensureConsentIdentity: () => new AttriaxConsentIdentity("consent-1"),
                resolveTimezone: resolveTimezone ?? (() => timezone),
                gateway: gateway,
                onStateChanged: null,
                debugLog: (_, _) => { });
        }

        private string NewStorageKey()
        {
            var storageKey = "attriax.tests.consent." + Guid.NewGuid().ToString("N");
            _storageKeys.Add(storageKey);
            return storageKey;
        }

        private sealed class RecordingConsentGateway : IAttriaxGdprConsentGateway
        {
            public int CheckCalls { get; private set; }

            public int UpsertCalls { get; private set; }

            public SdkGdprConsentStatusDto? NextCheckStatus { get; set; }

            public Func<int, SdkGdprConsentStatusDto>? NextCheckStatusFactory { get; set; }

            public SdkGdprConsentStatusDto? NextUpsertStatus { get; set; }

            public Func<int, SdkGdprConsentStatusDto>? NextUpsertStatusFactory { get; set; }

            public List<SdkV1GdprConsentWriteDto> UpsertRequests { get; } = new List<SdkV1GdprConsentWriteDto>();

            public Task<SdkGdprConsentStatusDto> CheckGdprConsentAsync(SdkV1GdprConsentCheckDto request)
            {
                CheckCalls += 1;
                return Task.FromResult(
                    NextCheckStatusFactory?.Invoke(CheckCalls) ??
                    NextCheckStatus ?? new SdkGdprConsentStatusDto(
                        checkedAt: DateTime.UtcNow,
                        countryCode: null,
                        needsConsent: true,
                        regionSource: "ip",
                        state: AppUserGdprConsentState.Pending,
                        values: null));
            }

            public Task<SdkGdprConsentStatusDto> UpsertGdprConsentAsync(SdkV1GdprConsentWriteDto request)
            {
                UpsertCalls += 1;
                UpsertRequests.Add(request);
                return Task.FromResult(
                    NextUpsertStatusFactory?.Invoke(UpsertCalls) ??
                    NextUpsertStatus ?? new SdkGdprConsentStatusDto(
                        checkedAt: DateTime.UtcNow,
                        countryCode: request.countryCode,
                        needsConsent: request.state != AppUserGdprConsentState.NotRequired,
                        regionSource: request.regionSource,
                        state: request.state,
                        values: request.values == null
                            ? null
                            : new SdkGdprConsentValuesDto(
                                adEvents: request.values.adEvents,
                                analytics: request.values.analytics,
                                attribution: request.values.attribution)));
            }
        }
    }
}