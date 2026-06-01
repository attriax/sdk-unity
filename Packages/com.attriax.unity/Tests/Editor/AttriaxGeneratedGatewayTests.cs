#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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