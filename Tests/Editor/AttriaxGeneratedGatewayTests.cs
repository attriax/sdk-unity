#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;
using UnityEngine.TestTools;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxGeneratedGatewayTests
    {
        [Test]
        public void FitsBatchRejectsOneHundredEventsPlusKeepAlive()
        {
            using var gateway = new AttriaxGeneratedGateway("https://api.attriax.com", 12000);
            var entries = CreateEventEntries(100);
            entries.Add(CreateHeartbeatEntry());

            var fits = gateway.FitsBatch(entries, 100, 48 * 1024);

            Assert.That(fits, Is.False);
        }

        [Test]
        public void FitsBatchAllowsNinetyNineEventsPlusKeepAlive()
        {
            using var gateway = new AttriaxGeneratedGateway("https://api.attriax.com", 12000);
            var entries = CreateEventEntries(99);
            entries.Add(CreateHeartbeatEntry());

            var fits = gateway.FitsBatch(entries, 100, 48 * 1024);

            Assert.That(fits, Is.True);
        }

        [UnityTest]
        public IEnumerator ExecuteAsyncStartsGeneratedRequestsOnTheUnityMainThread()
        {
            AttriaxLifecycleDispatcher.BindToCurrentThread();
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var executeAsyncMethod = typeof(AttriaxGeneratedGateway).GetMethod(
                "ExecuteAsync",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(executeAsyncMethod, Is.Not.Null);

            Task<int>? executeTask = null;
            var backgroundInvocation = Task.Run(() =>
            {
                var execute = new Func<Task<int>>(() => Task.FromResult(Thread.CurrentThread.ManagedThreadId));
                executeTask = executeAsyncMethod!
                    .MakeGenericMethod(typeof(int))
                    .Invoke(null, new object?[] { execute }) as Task<int>;
            });

            while (!backgroundInvocation.IsCompleted)
            {
                yield return null;
            }

            Assert.That(backgroundInvocation.Exception, Is.Null);
            Assert.That(executeTask, Is.Not.Null);

            while (!executeTask!.IsCompleted)
            {
                yield return null;
            }

            Assert.That(executeTask.Exception, Is.Null);
            Assert.That(executeTask.Result, Is.EqualTo(mainThreadId));
        }

                [Test]
                public void MapRuntimeConfigEnvelopeParsesRawJsonWithoutGeneratedEnvelopeTypes()
                {
                        var config = AttriaxGeneratedGateway.MapRuntimeConfigEnvelope(JObject.Parse(@"""
                                {
                                    "success": true,
                                    "timestamp": "2026-06-03T08:20:00.000Z",
                                    "data": {
                                        "clipboardAttributionEnabled": true
                                    }
                                }
                                """));

                        Assert.That(config.ClipboardAttributionEnabled, Is.True);
                }

                [Test]
                public void MapAppOpenResultEnvelopeParsesRawJsonWithoutGeneratedEnvelopeTypes()
                {
                        var result = AttriaxGeneratedGateway.MapAppOpenResultEnvelope(JObject.Parse(@"""
                                {
                                    "success": true,
                                    "timestamp": "2026-06-03T08:20:00.000Z",
                                    "data": {
                                        "acceptedAt": "2026-06-03T08:20:00.000Z",
                                        "installState": "new_install",
                                        "isFirstLaunch": true,
                                        "isNewUser": true,
                                        "requestVersion": "v1",
                                        "userId": "user_123",
                                        "deepLink": {
                                            "path": "/promo",
                                            "uri": "https://example.com/promo",
                                            "data": {
                                                "code": "summer"
                                            },
                                            "utm": {
                                                "source": "newsletter"
                                            }
                                        },
                                        "originalInstallReferrer": {
                                            "attributionType": "referrer",
                                            "precision": 0.9,
                                            "rawPlatformInstallReferrer": "utm_source=newsletter",
                                            "source": "newsletter"
                                        }
                                    }
                                }
                                """));

                        Assert.That(result.UserId, Is.EqualTo("user_123"));
                        Assert.That(result.IsFirstLaunch, Is.True);
                        Assert.That(result.IsNewUser, Is.True);
                        Assert.That(result.InstallState, Is.EqualTo(AttriaxInstallState.NewInstall));
                        Assert.That(result.DeepLink?.Path, Is.EqualTo("/promo"));
                        Assert.That(result.DeepLink?.Utm?.Source, Is.EqualTo("newsletter"));
                        Assert.That(result.OriginalInstallReferrer?.Source, Is.EqualTo("newsletter"));
                        Assert.That(result.OriginalInstallReferrer?.AttributionType, Is.EqualTo(AttributionType.Referrer));
                }

        private static List<AttriaxQueuedRequest> CreateEventEntries(int count)
        {
            var entries = new List<AttriaxQueuedRequest>(count);
            for (var index = 0; index < count; index += 1)
            {
                entries.Add(AttriaxQueuedRequest.CreateEvent(new SdkEventDto(
                    projectToken: "ax_test",
                    clientOccurredAt: DateTime.UtcNow,
                    deviceId: "device_1",
                    deviceIdSource: "persistent_storage",
                    eventName: "event_" + index,
                    eventData: new Dictionary<string, object>
                    {
                        ["index"] = index,
                    },
                    sessionId: "session_1")));
            }

            return entries;
        }

        private static AttriaxQueuedRequest CreateHeartbeatEntry()
        {
            return AttriaxQueuedRequest.CreateSession(new SdkSessionDto(
                projectToken: "ax_test",
                clientOccurredAt: DateTime.UtcNow,
                deviceId: "device_1",
                deviceIdSource: "persistent_storage",
                kind: SdkSessionLifecycleKind.Heartbeat,
                sessionId: "session_1"));
        }
    }
}