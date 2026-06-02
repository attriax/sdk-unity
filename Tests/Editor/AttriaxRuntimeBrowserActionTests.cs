#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRuntimeBrowserActionTests
    {
        [Test]
        public async Task BrowserActionsUseInjectedOpenerWhenAutomaticHandlingEnabled()
        {
            AttriaxPlatformType? observedPlatform = null;
            string? observedUrl = null;
            AttriaxResolvedUrlOpenMode? observedOpenMode = null;
            var runtime = new AttriaxRuntime(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    AutomaticBrowserHandling = true,
                },
                (platform, url, openMode) =>
                {
                    observedPlatform = platform;
                    observedUrl = url;
                    observedOpenMode = openMode;
                    return Task.FromResult(true);
                });

            try
            {
                var result = await InvokeMapResolutionToDeepLinkEventAsync(
                    runtime,
                    new AttriaxDeepLinkResolutionResultInternal
                    {
                        Matched = true,
                        DeepLink = new AttriaxDeepLink
                        {
                            Uri = new Uri("https://app.attriax.test/promo"),
                        },
                        BrowserAction = new AttriaxResolvedUrlAction
                        {
                            Url = "https://www.example.com",
                            OpenMode = AttriaxResolvedUrlOpenMode.External,
                        },
                    },
                    new AttriaxDeepLinkConversionOptions
                    {
                        Uri = "https://app.attriax.test/promo",
                    },
                    new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

                Assert.That(observedPlatform, Is.EqualTo(AttriaxPlatformType.UnityEditor));
                Assert.That(observedUrl, Is.EqualTo("https://www.example.com"));
                Assert.That(observedOpenMode, Is.EqualTo(AttriaxResolvedUrlOpenMode.External));
                Assert.That(result.BrowserAction, Is.Not.Null);
                Assert.That(result.HandledBySdk, Is.True);
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public async Task BrowserActionsSkipOpenerWhenAutomaticHandlingDisabled()
        {
            var openerCalls = 0;
            var runtime = new AttriaxRuntime(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    AutomaticBrowserHandling = false,
                },
                (_, _, _) =>
                {
                    openerCalls += 1;
                    return Task.FromResult(true);
                });

            try
            {
                var result = await InvokeMapResolutionToDeepLinkEventAsync(
                    runtime,
                    new AttriaxDeepLinkResolutionResultInternal
                    {
                        Matched = true,
                        DeepLink = new AttriaxDeepLink
                        {
                            Uri = new Uri("https://app.attriax.test/promo"),
                        },
                        BrowserAction = new AttriaxResolvedUrlAction
                        {
                            Url = "https://www.example.com",
                            OpenMode = AttriaxResolvedUrlOpenMode.InApp,
                        },
                    },
                    new AttriaxDeepLinkConversionOptions
                    {
                        Uri = "https://app.attriax.test/promo",
                    },
                    new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

                Assert.That(openerCalls, Is.EqualTo(0));
                Assert.That(result.BrowserAction, Is.Not.Null);
                Assert.That(result.HandledBySdk, Is.False);
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public void NotRequiredConsentReidentifiesAnonymousQueuedEvents()
        {
            var runtime = new AttriaxRuntime(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                });

            try
            {
                var runtimeStateField = typeof(AttriaxRuntime).GetField(
                    "_runtimeState",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(runtimeStateField, Is.Not.Null);

                var runtimeState = runtimeStateField!.GetValue(runtime) as AttriaxRuntimeState;
                Assert.That(runtimeState, Is.Not.Null);
                runtimeState!.DeviceId = "device-123";
                runtimeState.DeviceIdSource = "persistent_storage";

                var entry = AttriaxQueuedRequest.CreateEvent(new SdkEventDto(
                    projectToken: "ax_test",
                    clientOccurredAt: DateTime.UtcNow,
                    deviceId: null,
                    deviceIdSource: null,
                    eventName: "queued_event"));

                var method = typeof(AttriaxRuntime).GetMethod(
                    "IdentifyQueuedRequestForConsentNotRequired",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);

                method!.Invoke(runtime, new object?[] { entry });

                Assert.That(entry.RequireEventRequest().deviceId, Is.EqualTo("device-123"));
                Assert.That(entry.RequireEventRequest().deviceIdSource, Is.EqualTo("persistent_storage"));
            }
            finally
            {
                runtime.Dispose();
            }
        }

        private static async Task<AttriaxDeepLinkEvent> InvokeMapResolutionToDeepLinkEventAsync(
            AttriaxRuntime runtime,
            AttriaxDeepLinkResolutionResultInternal resolution,
            AttriaxDeepLinkConversionOptions options,
            DateTimeOffset clickedAt)
        {
            var method = typeof(AttriaxRuntime).GetMethod(
                "MapResolutionToDeepLinkEventAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);

            var task = method!.Invoke(
                runtime,
                new object?[]
                {
                    resolution,
                    options,
                    null,
                    clickedAt,
                    false,
                }) as Task<AttriaxDeepLinkEvent>;

            Assert.That(task, Is.Not.Null);
            return await task!;
        }
    }
}