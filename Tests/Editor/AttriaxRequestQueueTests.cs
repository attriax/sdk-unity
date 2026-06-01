#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Attriax.Unity.Generated.Model;
using Attriax.Unity.Internal;
using Platform = Attriax.Unity.Generated.Model.Platform;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRequestQueueTests
    {
        private readonly List<string> _storageKeys = new List<string>();

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
        public void ConstructorDropsInvalidPersistedPayload()
        {
            var storageKey = NewStorageKey();
            PlayerPrefs.SetString(storageKey, "{ definitely-not-json }");
            PlayerPrefs.Save();

            var queue = new AttriaxRequestQueue(storageKey, 8, (_, _) => { });

            Assert.That(queue.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ClearRejectsPendingRequestsAndRemovesPersistedQueue()
        {
            var storageKey = NewStorageKey();
            var queue = new AttriaxRequestQueue(storageKey, 8, (_, _) => { });
            var pendingTask = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("first")));

            queue.Clear(new InvalidOperationException("cleared"));

            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(PlayerPrefs.HasKey(storageKey), Is.False);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pendingTask);
        }

        [Test]
        public void PrioritizeOpenRequestsMovesOpenEntriesToTheFront()
        {
            var queue = CreateQueue();

            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-before-open")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateOpen(CreateOpenRequest("device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateSession(CreateSessionRequest("session-1", "device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateOpen(CreateOpenRequest("device-1")));

            queue.PrioritizeOpenRequests();

            Assert.That(ReadKinds(queue), Is.EqualTo(new[]
            {
                AttriaxQueuedRequestKind.Open,
                AttriaxQueuedRequestKind.Open,
                AttriaxQueuedRequestKind.Event,
                AttriaxQueuedRequestKind.Session,
            }));
        }

        [Test]
        public void PeekBatchablePrefixStopsAtEnvelopeBoundaries()
        {
            var queue = CreateQueue();

            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-1", deviceId: "device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateSession(CreateSessionRequest("session-1", deviceId: "device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateUser(CreateUserRequest("device-2")));

            var batchablePrefix = queue.PeekBatchablePrefix();

            Assert.That(batchablePrefix.Count, Is.EqualTo(2));
            Assert.That(batchablePrefix[0].Kind, Is.EqualTo(AttriaxQueuedRequestKind.Event));
            Assert.That(batchablePrefix[1].Kind, Is.EqualTo(AttriaxQueuedRequestKind.Session));
        }

        [Test]
        public void PeekBatchablePrefixCanStartAfterEarlierDeferredEntries()
        {
            var queue = CreateQueue();

            _ = queue.Enqueue(AttriaxQueuedRequest.CreateOpen(CreateOpenRequest("device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-1", deviceId: "device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateSession(CreateSessionRequest("session-1", deviceId: "device-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateUser(CreateUserRequest("device-1")));

            var batchablePrefix = queue.PeekBatchablePrefix(1);

            Assert.That(batchablePrefix.Count, Is.EqualTo(3));
            Assert.That(batchablePrefix[0].Kind, Is.EqualTo(AttriaxQueuedRequestKind.Event));
            Assert.That(batchablePrefix[1].Kind, Is.EqualTo(AttriaxQueuedRequestKind.Session));
            Assert.That(batchablePrefix[2].Kind, Is.EqualTo(AttriaxQueuedRequestKind.User));
        }

        [Test]
        public async Task DiscardWhereRemovesMatchingEntriesAndRejectsPendingRequests()
        {
            var queue = CreateQueue();

            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-1")));
            var pendingUninstallToken = queue.Enqueue(
                AttriaxQueuedRequest.CreateUninstallToken(CreateUninstallTokenRequest()));

            queue.DiscardWhere(
                entry => entry.Kind == AttriaxQueuedRequestKind.UninstallToken,
                new InvalidOperationException("blocked"));

            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.Peek().Kind, Is.EqualTo(AttriaxQueuedRequestKind.Event));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pendingUninstallToken);
        }

        [Test]
        public void RetryMetadataPersistsAndReportsEarliestRetryAt()
        {
            var storageKey = NewStorageKey();
            var queue = new AttriaxRequestQueue(storageKey, 8, (_, _) => { });
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-1")));
            _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-2")));

            var attemptedAt = new DateTimeOffset(2026, 5, 25, 5, 0, 0, TimeSpan.Zero);
            var retryAfterAt = attemptedAt.AddSeconds(45);
            queue.ReplaceAt(
                0,
                AttriaxQueueRetryPolicy.MarkForRetry(
                    queue.PeekAt(0),
                    new AttriaxApiError("rate limited", 429, true, false, retryAfterAt: retryAfterAt),
                    attemptedAt,
                    30000));
            queue.ReplaceAt(
                1,
                AttriaxQueueRetryPolicy.MarkForRetry(
                    queue.PeekAt(1),
                    new AttriaxApiError("server unavailable", 503, true, false),
                    attemptedAt,
                    60000));

            var reloaded = new AttriaxRequestQueue(storageKey, 8, (_, _) => { });
            var first = reloaded.PeekAt(0);
            var second = reloaded.PeekAt(1);

            Assert.That(first.AttemptCount, Is.EqualTo(1));
            Assert.That(first.LastAttemptAt, Is.EqualTo(attemptedAt));
            Assert.That(first.LastErrorClass, Is.EqualTo("http_429"));
            Assert.That(first.LastHttpStatusCode, Is.EqualTo(429));
            Assert.That(first.NextRetryAt, Is.EqualTo(retryAfterAt));
            Assert.That(second.AttemptCount, Is.EqualTo(1));
            Assert.That(second.LastErrorClass, Is.EqualTo("http_503"));
            Assert.That(second.LastHttpStatusCode, Is.EqualTo(503));
            Assert.That(second.NextRetryAt, Is.EqualTo(attemptedAt.AddSeconds(60)));
            Assert.That(reloaded.PeekEarliestRetryAt(), Is.EqualTo(retryAfterAt));
        }

        [Test]
        public void RetryPolicyAppliesTerminalDropRulesButExemptsDeepLinkResolution()
        {
            var now = new DateTimeOffset(2026, 5, 25, 5, 0, 0, TimeSpan.Zero);
            var eventRequest = AttriaxQueuedRequest.CreateEvent(CreateEventRequest("event-1"));
            eventRequest.AttemptCount = 8;

            var deepLinkRequest = AttriaxQueuedRequest.CreateDeepLinkResolve(new SdkV1DeepLinkResolveDto(
                projectToken: "ax_test",
                deviceId: "device-1",
                deviceIdSource: "sdk",
                isFirstLaunch: false,
                linkPath: "/promo",
                metadata: null,
                platform: Platform.UnityEditor,
                rawUrl: "https://attriax.test/promo",
                source: "manual"));
            deepLinkRequest.AttemptCount = 8;

            Assert.That(
                AttriaxQueueRetryPolicy.GetTerminalDropReason(eventRequest, now),
                Is.EqualTo("max_attempts_exceeded"));
            Assert.That(
                AttriaxQueueRetryPolicy.GetTerminalDropReason(deepLinkRequest, now),
                Is.Null);

            eventRequest.AttemptCount = 0;
            eventRequest.CreatedAt = now.AddDays(-8);
            Assert.That(
                AttriaxQueueRetryPolicy.GetTerminalDropReason(eventRequest, now),
                Is.EqualTo("max_age_exceeded"));
        }

        [Test]
        public void ConfiguredRuntimeQueueStaysInMemoryUntilPersistenceReturns()
        {
            var storageKey = NewStorageKey();
            AttriaxPlayerPrefs.SetRuntimePersistenceMode(
                new[] { storageKey },
                AttriaxPlayerPrefsPersistenceMode.ConsentOnly);

            try
            {
                var queue = new AttriaxRequestQueue(storageKey, 8, (_, _) => { });

                _ = queue.Enqueue(AttriaxQueuedRequest.CreateEvent(CreateEventRequest("memory_only")));

                Assert.That(queue.Count, Is.EqualTo(1));
                Assert.That(PlayerPrefs.HasKey(storageKey), Is.False);

                AttriaxPlayerPrefs.SetRuntimePersistenceMode(
                    new[] { storageKey },
                    AttriaxPlayerPrefsPersistenceMode.FullRuntime);

                Assert.That(PlayerPrefs.HasKey(storageKey), Is.True);
                Assert.That(PlayerPrefs.GetString(storageKey, string.Empty), Does.Contain("memory_only"));
            }
            finally
            {
                AttriaxPlayerPrefs.ForgetRuntimeKeys(new[] { storageKey });
                AttriaxPlayerPrefs.DeleteKey(storageKey);
                AttriaxPlayerPrefs.Save();
            }
        }

        private AttriaxRequestQueue CreateQueue()
        {
            return new AttriaxRequestQueue(NewStorageKey(), 8, (_, _) => { });
        }

        private string NewStorageKey()
        {
            var storageKey = "attriax.tests.queue." + Guid.NewGuid().ToString("N");
            _storageKeys.Add(storageKey);
            return storageKey;
        }

        private static IReadOnlyList<AttriaxQueuedRequestKind> ReadKinds(AttriaxRequestQueue queue)
        {
            var kinds = new List<AttriaxQueuedRequestKind>();
            while (queue.Count > 0)
            {
                kinds.Add(queue.Peek().Kind);
                queue.RemoveFirst();
            }

            return kinds;
        }

        private static SdkEventDto CreateEventRequest(string eventName, string deviceId = "device-1")
        {
            return new SdkEventDto(
                projectToken: "ax_test",
                clientOccurredAt: DateTime.UtcNow,
                deviceId: deviceId,
                deviceIdSource: "sdk",
                eventName: eventName,
                eventData: new Dictionary<string, object>
                {
                    ["source"] = "editor-test",
                });
        }

        private static SdkSessionDto CreateSessionRequest(string sessionId, string deviceId)
        {
            return new SdkSessionDto(
                projectToken: "ax_test",
                clientOccurredAt: DateTime.UtcNow,
                deviceId: deviceId,
                deviceIdSource: "sdk",
                kind: SdkSessionLifecycleKind.Start,
                sessionId: sessionId);
        }

        private static SdkUserDto CreateUserRequest(string deviceId)
        {
            return new SdkUserDto(
                projectToken: "ax_test",
                deviceId: deviceId,
                deviceIdSource: "sdk",
                properties: new Dictionary<string, object>
                {
                    ["segment"] = "test",
                });
        }

        private static SdkV1OpenDto CreateOpenRequest(string deviceId)
        {
            return new SdkV1OpenDto(
                app: new AppVersionContextDto(version: "1.0.0", buildNumber: "100"),
                projectToken: "ax_test",
                device: new DeviceContextDto(model: "Editor"),
                deviceId: deviceId,
                deviceIdSource: "sdk",
                platform: Platform.UnityEditor,
                sdk: new SdkVersionContextDto(apiVersion: "1", packageVersion: "0.0.1"),
                sessionId: "session-open");
        }

        private static SdkRegisterUninstallTokenDto CreateUninstallTokenRequest()
        {
            return new SdkRegisterUninstallTokenDto(
                projectToken: "ax_test",
                deviceId: "device-1",
                deviceIdSource: "sdk",
                platform: Platform.UnityEditor,
                provider: AppUserUninstallTokenProvider.Fcm,
                token: "fcm-token");
        }
    }
}