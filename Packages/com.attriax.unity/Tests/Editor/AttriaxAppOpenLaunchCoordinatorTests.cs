#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Generated.Client;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxAppOpenLaunchCoordinatorTests
    {
        [Test]
        public void GeneratedLaunchResponseEnvelopesDeserializeWithUnityClientSerializerSettings()
        {
            var codec = new CustomJsonCodec(new Configuration());
            var configEnvelope = (SdkV1ConfigResponseEnvelopeDto)codec.DeserializeJson(
                "{\"success\":true,\"timestamp\":\"2026-06-02T12:24:26.680Z\",\"data\":{\"clipboardAttributionEnabled\":true}}",
                typeof(SdkV1ConfigResponseEnvelopeDto));
            var openEnvelope = (SdkV1OpenResponseEnvelopeDto)codec.DeserializeJson(
                "{\"success\":true,\"timestamp\":\"2026-06-02T12:24:26.680Z\",\"data\":{\"attributionType\":\"organic\"}}",
                typeof(SdkV1OpenResponseEnvelopeDto));

            Assert.That(configEnvelope, Is.Not.Null);
            Assert.That(configEnvelope!.data.clipboardAttributionEnabled, Is.True);
            Assert.That(openEnvelope, Is.Not.Null);
            Assert.That(openEnvelope!.success, Is.True);
        }

        [Test]
        public void GeneratedLaunchResponseModelsExposePublicJsonConstructors()
        {
            Assert.That(typeof(SdkV1ConfigResponseEnvelopeDto).IsPublic, Is.True);
            Assert.That(typeof(SdkV1ConfigResponseDto).IsPublic, Is.True);
            Assert.That(typeof(SdkV1OpenResponseEnvelopeDto).IsPublic, Is.True);
            Assert.That(typeof(SdkV1OpenResponseDto).IsPublic, Is.True);
            Assert.That(HasPublicParameterlessConstructor(typeof(SdkV1ConfigResponseEnvelopeDto)), Is.True);
            Assert.That(HasPublicParameterlessConstructor(typeof(SdkV1ConfigResponseDto)), Is.True);
            Assert.That(HasPublicParameterlessConstructor(typeof(SdkV1OpenResponseEnvelopeDto)), Is.True);
            Assert.That(HasPublicParameterlessConstructor(typeof(SdkV1OpenResponseDto)), Is.True);
        }

        [Test]
        public async Task WaitsForRuntimeConfigBeforeSchedulingAppOpen()
        {
            var runtimeConfigSource = new TaskCompletionSource<AttriaxSdkRuntimeConfig>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var scheduleCount = 0;
            var coordinator = new AttriaxAppOpenLaunchCoordinator(
                didSchedule: () => scheduleCount > 0,
                allowsAttributionTracking: () => true,
                ensureRuntimeConfigLoadedAsync: () => runtimeConfigSource.Task,
                buildDeviceMetadataOverridesAsync: _ => Task.FromResult(new Dictionary<string, object>()),
                installReferrerOverrideForAppOpen: (_, __) => null,
                scheduleAppOpenAsync: (_, __) =>
                {
                    scheduleCount += 1;
                    return Task.CompletedTask;
                });

            var scheduling = coordinator.ScheduleIfNeeded(isInitialized: true, isEnabled: true);

            Assert.That(scheduleCount, Is.Zero);

            runtimeConfigSource.SetResult(new AttriaxSdkRuntimeConfig(clipboardAttributionEnabled: true));
            await scheduling;

            Assert.That(scheduleCount, Is.EqualTo(1));
        }

        private static bool HasPublicParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null) != null;
        }

        [Test]
        public async Task CoalescesConcurrentSchedulingWhileLaunchPreparationIsInFlight()
        {
            var runtimeConfigSource = new TaskCompletionSource<AttriaxSdkRuntimeConfig>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var loadCount = 0;
            var scheduleCount = 0;
            var coordinator = new AttriaxAppOpenLaunchCoordinator(
                didSchedule: () => scheduleCount > 0,
                allowsAttributionTracking: () => true,
                ensureRuntimeConfigLoadedAsync: () =>
                {
                    loadCount += 1;
                    return runtimeConfigSource.Task;
                },
                buildDeviceMetadataOverridesAsync: _ => Task.FromResult(new Dictionary<string, object>()),
                installReferrerOverrideForAppOpen: (_, __) => null,
                scheduleAppOpenAsync: (_, __) =>
                {
                    scheduleCount += 1;
                    return Task.CompletedTask;
                });

            var first = coordinator.ScheduleIfNeeded(isInitialized: true, isEnabled: true);
            var second = coordinator.ScheduleIfNeeded(isInitialized: true, isEnabled: true);

            Assert.That(loadCount, Is.EqualTo(1));

            runtimeConfigSource.SetResult(new AttriaxSdkRuntimeConfig());
            await Task.WhenAll(first, second);

            Assert.That(scheduleCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RuntimeConfigCoordinatorCachesResolvedLaunchDefaults()
        {
            var loadCount = 0;
            var coordinator = new AttriaxSdkRuntimeConfigCoordinator(
                loadRuntimeConfigAsync: () =>
                {
                    loadCount += 1;
                    return Task.FromResult(new AttriaxSdkRuntimeConfig(clipboardAttributionEnabled: true));
                });

            var first = await coordinator.EnsureLoadedAsync();
            var second = await coordinator.EnsureLoadedAsync();

            Assert.That(loadCount, Is.EqualTo(1));
            Assert.That(first.ClipboardAttributionEnabled, Is.True);
            Assert.That(ReferenceEquals(first, second), Is.True);
        }

        [Test]
        public async Task RuntimeConfigCoordinatorInvokesOnLoadedCallbackWithResolvedConfig()
        {
            AttriaxSdkRuntimeConfig? loaded = null;
            var coordinator = new AttriaxSdkRuntimeConfigCoordinator(
                loadRuntimeConfigAsync: () => Task.FromResult(new AttriaxSdkRuntimeConfig(clipboardAttributionEnabled: true)),
                onLoadedAsync: runtimeConfig =>
                {
                    loaded = runtimeConfig;
                    return Task.CompletedTask;
                });

            await coordinator.EnsureLoadedAsync();

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.ClipboardAttributionEnabled, Is.True);
        }

        [Test]
        public void RuntimeConfigRequestBuilderUsesPackageNameAndAndroidSigningFingerprints()
        {
            var request = AttriaxSdkRuntimeConfigRequestBuilder.Build(
                "ax_test",
                new AttriaxContextSnapshot
                {
                    Platform = AttriaxPlatformType.Android,
                    App = new AttriaxAppSnapshot
                    {
                        PackageName = " com.attriax.test ",
                    },
                    Device = new AttriaxDeviceSnapshot
                    {
                        Metadata = new Dictionary<string, object>
                        {
                            ["signingSha256Fingerprints"] = new object[]
                            {
                                " sha256:abc ",
                                null,
                                "",
                                "sha256:def",
                            },
                        },
                    },
                });

            Assert.That(request.projectToken, Is.EqualTo("ax_test"));
            Assert.That(request.packageName, Is.EqualTo("com.attriax.test"));
            Assert.That(request.platform, Is.EqualTo(Platform.Android));
            Assert.That(request.signatureHashes, Is.EqualTo(new[] { "sha256:abc", "sha256:def" }));
        }

        [Test]
        public async Task ScheduleIfNeededPassesRuntimeConfigDerivedLaunchOverrides()
        {
            var buildDeviceMetadataCalls = new List<bool>();
            bool? sawClipboardAttributionEnabled = null;
            bool? sawAllowsAttributionTracking = null;
            string? capturedInstallReferrerOverride = null;
            IDictionary<string, object>? capturedDeviceMetadataOverrides = null;

            var coordinator = new AttriaxAppOpenLaunchCoordinator(
                didSchedule: () => false,
                allowsAttributionTracking: () => true,
                ensureRuntimeConfigLoadedAsync: () => Task.FromResult(new AttriaxSdkRuntimeConfig(clipboardAttributionEnabled: true)),
                buildDeviceMetadataOverridesAsync: allowsAttributionTracking =>
                {
                    buildDeviceMetadataCalls.Add(allowsAttributionTracking);
                    return Task.FromResult(new Dictionary<string, object>
                    {
                        [AttriaxIosAppOpenEnrichmentManager.WkWebViewUserAgentMetadataKey] = "ua",
                    });
                },
                installReferrerOverrideForAppOpen: (clipboardAttributionEnabled, allowsAttributionTracking) =>
                {
                    sawClipboardAttributionEnabled = clipboardAttributionEnabled;
                    sawAllowsAttributionTracking = allowsAttributionTracking;
                    return "attriax_click_id=click-123";
                },
                scheduleAppOpenAsync: (installReferrerOverride, deviceMetadataOverrides) =>
                {
                    capturedInstallReferrerOverride = installReferrerOverride;
                    capturedDeviceMetadataOverrides = new Dictionary<string, object>(deviceMetadataOverrides);
                    return Task.CompletedTask;
                });

            await coordinator.ScheduleIfNeeded(isInitialized: true, isEnabled: true);

            Assert.That(buildDeviceMetadataCalls, Is.EqualTo(new[] { true }));
            Assert.That(sawClipboardAttributionEnabled, Is.True);
            Assert.That(sawAllowsAttributionTracking, Is.True);
            Assert.That(capturedInstallReferrerOverride, Is.EqualTo("attriax_click_id=click-123"));
            Assert.That(capturedDeviceMetadataOverrides, Is.EqualTo(new Dictionary<string, object>
            {
                [AttriaxIosAppOpenEnrichmentManager.WkWebViewUserAgentMetadataKey] = "ua",
            }));
        }

        [Test]
        public async Task IosAppOpenEnrichmentManagerCapturesClipboardOnceAndNormalizesClickIds()
        {
            var clipboardReads = 0;
            var manager = new AttriaxIosAppOpenEnrichmentManager(
                AttriaxPlatformType.IOS,
                readAttributionClipboardAsync: () =>
                {
                    clipboardReads += 1;
                    return Task.FromResult<string?>(" click-123 ");
                },
                collectWebViewUserAgentAsync: () => Task.FromResult<string?>(null));

            await manager.PrimeForConsentStateAsync(
                clipboardAttributionEnabled: true,
                isWaitingForGdprConsent: true,
                allowsAttributionTracking: false);
            await manager.PrimeForConsentStateAsync(
                clipboardAttributionEnabled: true,
                isWaitingForGdprConsent: true,
                allowsAttributionTracking: false);

            Assert.That(clipboardReads, Is.EqualTo(1));
            Assert.That(
                manager.InstallReferrerOverrideForAppOpen(
                    clipboardAttributionEnabled: true,
                    allowsAttributionTracking: true),
                Is.EqualTo("attriax_click_id=click-123"));
        }

        [Test]
        public async Task IosAppOpenEnrichmentManagerBuildsWkWebViewUserAgentMetadata()
        {
            var userAgentReads = 0;
            var manager = new AttriaxIosAppOpenEnrichmentManager(
                AttriaxPlatformType.IOS,
                readAttributionClipboardAsync: () => Task.FromResult<string?>(null),
                collectWebViewUserAgentAsync: () =>
                {
                    userAgentReads += 1;
                    return Task.FromResult<string?>(" Mozilla/5.0 ");
                });

            var metadata = await manager.BuildDeviceMetadataOverridesForAppOpenAsync(
                allowsAttributionTracking: true);

            Assert.That(userAgentReads, Is.EqualTo(1));
            Assert.That(metadata, Is.EqualTo(new Dictionary<string, object>
            {
                [AttriaxIosAppOpenEnrichmentManager.WkWebViewUserAgentMetadataKey] = "Mozilla/5.0",
            }));
        }
    }
}