#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxGeneratedRequestFactoryTests
    {
        [Test]
        public void BuildValidateReceiptRequestReturnsGeneratedDtoWithNormalizedFields()
        {
            var occurredAt = new DateTimeOffset(2026, 5, 8, 13, 15, 0, TimeSpan.Zero);

            var request = AttriaxGeneratedRequestFactory.BuildValidateReceiptRequest(
                projectToken: "ax_test",
                deviceId: "device_1",
                config: new AttriaxConfig
                {
                    AppPackageName = "com.attriax.game",
                },
                options: new AttriaxValidateReceiptOptions
                {
                    Provider = " unity ",
                    ProductId = " coins_500 ",
                    PurchaseToken = " purchase-token-123 ",
                },
                occurredAt: occurredAt);

            Assert.That(request, Is.TypeOf<SdkV1RevenueReceiptValidateDto>());
            Assert.That(request.projectToken, Is.EqualTo("ax_test"));
            Assert.That(request.deviceId, Is.EqualTo("device_1"));
            Assert.That(request.clientOccurredAt, Is.EqualTo("2026-05-08T13:15:00.0000000+00:00"));
            Assert.That(request.provider, Is.EqualTo("unity"));
            Assert.That(request.productId, Is.EqualTo("coins_500"));
            Assert.That(request.purchaseToken, Is.EqualTo("purchase-token-123"));
            Assert.That(request.packageName, Is.EqualTo("com.attriax.game"));
        }

        [Test]
        public void BuildValidateReceiptRequestAllowsMissingDeviceIdentity()
        {
            var request = AttriaxGeneratedRequestFactory.BuildValidateReceiptRequest(
                projectToken: "ax_test",
                deviceId: null,
                config: new AttriaxConfig(),
                options: new AttriaxValidateReceiptOptions(),
                occurredAt: DateTimeOffset.UtcNow);

            Assert.That(request.deviceId, Is.Null);
        }

        [Test]
        public void BuildRegisterUninstallTokenRequestReturnsGeneratedDtoWithEnumProvider()
        {
            var request = AttriaxGeneratedRequestFactory.BuildRegisterUninstallTokenRequest(
                projectToken: "ax_test",
                deviceId: "device_1",
                deviceIdSource: "persistent_storage",
                platform: AttriaxPlatformType.IOS,
                provider: AppUserUninstallTokenProvider.Fcm,
                token: " fcm-token ",
                metadata: new Dictionary<string, object>
                {
                    ["origin"] = "qa",
                });

            Assert.That(request, Is.TypeOf<SdkRegisterUninstallTokenDto>());
            Assert.That(request.projectToken, Is.EqualTo("ax_test"));
            Assert.That(request.deviceId, Is.EqualTo("device_1"));
            Assert.That(request.deviceIdSource, Is.EqualTo("persistent_storage"));
            Assert.That(request.platform, Is.EqualTo(Platform.Ios));
            Assert.That(request.provider, Is.EqualTo(AppUserUninstallTokenProvider.Fcm));
            Assert.That(request.token, Is.EqualTo("fcm-token"));
            Assert.That(request.metadata, Is.Not.Null);
            Assert.That(request.metadata!["origin"], Is.EqualTo("qa"));
        }

        [Test]
        public void BuildRegisterUninstallTokenRequestSupportsApnsProvider()
        {
            var request = AttriaxGeneratedRequestFactory.BuildRegisterUninstallTokenRequest(
                projectToken: "ax_test",
                deviceId: "device_1",
                deviceIdSource: "persistent_storage",
                platform: AttriaxPlatformType.IOS,
                provider: AppUserUninstallTokenProvider.Apns,
                token: " apns-token ",
                metadata: null);

            Assert.That(request.provider, Is.EqualTo(AppUserUninstallTokenProvider.Apns));
            Assert.That(request.token, Is.EqualTo("apns-token"));
        }

        [Test]
        public void BuildOpenRequestIncludesTypedScreenMetrics()
        {
            var request = AttriaxGeneratedRequestFactory.BuildOpenRequest(
                projectToken: "ax_test",
                deviceIdSource: "windows_machine_guid",
                snapshot: new AttriaxContextSnapshot
                {
                    Platform = AttriaxPlatformType.Windows,
                    DeviceId = "device_1",
                    IsFirstLaunch = false,
                    Sdk = new AttriaxSdkSnapshot
                    {
                        ApiVersion = "v1",
                        PackageVersion = "0.0.1",
                        Metadata = new Dictionary<string, object>(),
                    },
                    App = new AttriaxAppSnapshot
                    {
                        Version = "1.0.0",
                        BuildNumber = "42",
                        PackageName = "com.attriax.test",
                    },
                    Device = new AttriaxDeviceSnapshot
                    {
                        ScreenResolution = "1920x1080",
                        ScreenWidth = 1920,
                        ScreenHeight = 1080,
                        DevicePixelRatio = 1.5,
                        ColorDepth = 32,
                        Metadata = new Dictionary<string, object>(),
                        SupportedAbis = new List<string>(),
                    },
                },
                session: null);

            Assert.That(request.device.screenResolution, Is.EqualTo("1920x1080"));
            Assert.That(request.device.screenWidth, Is.EqualTo(1920m));
            Assert.That(request.device.screenHeight, Is.EqualTo(1080m));
            Assert.That(request.device.devicePixelRatio, Is.EqualTo(1.5m));
            Assert.That(request.device.colorDepth, Is.EqualTo(32m));
        }

        [Test]
        public void BuildOpenRequestIncludesInstallReferrerContextFields()
        {
            var request = AttriaxGeneratedRequestFactory.BuildOpenRequest(
                projectToken: "ax_test",
                deviceIdSource: "android_ad_id",
                snapshot: new AttriaxContextSnapshot
                {
                    Platform = AttriaxPlatformType.Android,
                    DeviceId = "device_1",
                    IsFirstLaunch = true,
                    RawPlatformInstallReferrer = "utm_source=play_store",
                    GooglePlayInstantParam = true,
                    InstallBeginTimestampSeconds = 1715346000,
                    ReferrerClickTimestampSeconds = 1715345900,
                    Sdk = new AttriaxSdkSnapshot
                    {
                        ApiVersion = "v1",
                        PackageVersion = "0.4.0",
                        Metadata = new Dictionary<string, object>(),
                    },
                    App = new AttriaxAppSnapshot
                    {
                        Version = "1.0.0",
                        BuildNumber = "42",
                        PackageName = "com.attriax.test",
                    },
                    Device = new AttriaxDeviceSnapshot
                    {
                        Metadata = new Dictionary<string, object>(),
                        SupportedAbis = new List<string>(),
                    },
                },
                session: null);

            Assert.That(request.googlePlayInstantParam, Is.True);
            Assert.That(request.installBeginTimestampSeconds, Is.EqualTo(1715346000m));
            Assert.That(request.referrerClickTimestampSeconds, Is.EqualTo(1715345900m));
            Assert.That(request.installReferrer, Is.EqualTo("utm_source=play_store"));
        }

        [Test]
        public void BuildOpenRequestAppliesInstallReferrerAndDeviceMetadataOverrides()
        {
            var request = AttriaxGeneratedRequestFactory.BuildOpenRequest(
                projectToken: "ax_test",
                deviceIdSource: "ios_keychain",
                snapshot: new AttriaxContextSnapshot
                {
                    Platform = AttriaxPlatformType.IOS,
                    DeviceId = "device_1",
                    IsFirstLaunch = true,
                    RawPlatformInstallReferrer = "utm_source=stale",
                    Sdk = new AttriaxSdkSnapshot
                    {
                        ApiVersion = "v1",
                        PackageVersion = "0.4.0",
                        Metadata = new Dictionary<string, object>(),
                    },
                    App = new AttriaxAppSnapshot
                    {
                        Version = "1.0.0",
                        BuildNumber = "42",
                        PackageName = "com.attriax.test",
                    },
                    Device = new AttriaxDeviceSnapshot
                    {
                        Metadata = new Dictionary<string, object>
                        {
                            ["nativeContext"] = new Dictionary<string, object>
                            {
                                ["source"] = "ios_native",
                            },
                        },
                        SupportedAbis = new List<string>(),
                    },
                },
                session: null,
                installReferrerOverride: "attriax_click_id=click-123",
                deviceMetadataOverrides: new Dictionary<string, object>
                {
                    [AttriaxIosAppOpenEnrichmentManager.WkWebViewUserAgentMetadataKey] = "ua",
                });

            Assert.That(request.installReferrer, Is.EqualTo("attriax_click_id=click-123"));
            Assert.That(request.device.metadata, Is.Not.Null);
            Assert.That(request.device.metadata!["nativeContext"], Is.Not.Null);
            Assert.That(
                request.device.metadata![AttriaxIosAppOpenEnrichmentManager.WkWebViewUserAgentMetadataKey],
                Is.EqualTo("ua"));
        }

        [Test]
        public void BuildOpenRequestPreservesUnityEditorPlatform()
        {
            var request = AttriaxGeneratedRequestFactory.BuildOpenRequest(
                projectToken: "ax_test",
                deviceIdSource: "persistent_storage",
                snapshot: new AttriaxContextSnapshot
                {
                    Platform = AttriaxPlatformType.UnityEditor,
                    DeviceId = "device_1",
                    IsFirstLaunch = false,
                    Sdk = new AttriaxSdkSnapshot
                    {
                        ApiVersion = "v1",
                        PackageVersion = "0.4.0",
                        Metadata = new Dictionary<string, object>(),
                    },
                    App = new AttriaxAppSnapshot
                    {
                        Version = "1.0.0",
                        BuildNumber = "42",
                        PackageName = "com.attriax.test",
                    },
                    Device = new AttriaxDeviceSnapshot
                    {
                        Metadata = new Dictionary<string, object>(),
                        SupportedAbis = new List<string>(),
                    },
                },
                session: null);

            Assert.That(request.platform, Is.EqualTo(Platform.UnityEditor));
        }

        [Test]
        public void BuildUnityEditorValidateRequestUsesEditorMetadata()
        {
            var request = AttriaxGeneratedRequestFactory.BuildUnityEditorValidateRequest(
                projectToken: "ax_test",
                packageVersion: " 0.4.0 ",
                unityVersion: " 6000.4.6f1 ",
                editorHostPlatform: " WindowsEditor ");

            Assert.That(request.projectToken, Is.EqualTo("ax_test"));
            Assert.That(request.packageVersion, Is.EqualTo("0.4.0"));
            Assert.That(request.unityVersion, Is.EqualTo("6000.4.6f1"));
            Assert.That(request.editorHostPlatform, Is.EqualTo("WindowsEditor"));
        }

        [Test]
        public void BuildTrackSessionRequestRoundsSessionRelativeTimeDownToWholeMilliseconds()
        {
            var startedAt = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero).AddTicks(1234);
            var occurredAt = startedAt.AddMilliseconds(1500).AddTicks(9876);

            var request = AttriaxGeneratedRequestFactory.BuildTrackSessionRequest(
                projectToken: "ax_test",
                deviceId: "device_1",
                deviceIdSource: "persistent_storage",
                session: new AttriaxSessionSnapshot
                {
                    Id = "session_1",
                    DeviceId = "device_1",
                    Platform = AttriaxPlatformType.Android,
                    Locale = "en-US",
                    IsFirstLaunch = false,
                    StartedAt = startedAt,
                    LastActivityAt = occurredAt,
                    HeartbeatIntervalMs = 5000,
                    AppVersion = "1.0.0",
                    AppBuildNumber = "42",
                    AppPackageName = "com.attriax.test",
                    SdkPackageVersion = "0.0.1",
                },
                kind: SdkSessionLifecycleKind.Heartbeat,
                occurredAt: occurredAt,
                metadata: null);

            Assert.That(request.sessionRelativeTimeMs, Is.EqualTo(1500m));
        }
    }
}