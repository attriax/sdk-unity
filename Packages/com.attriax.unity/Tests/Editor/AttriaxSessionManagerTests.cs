#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Attriax.Unity.Internal;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxSessionManagerTests
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
        public void HandleSuccessfulForegroundFlushUpdatesActivityAndResetsHeartbeatAccumulator()
        {
            var occurredAt = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);
            var lifecycleQueue = new RecordingSessionLifecycleQueue();
            var sessionManager = CreateSessionManager(lifecycleQueue);
            sessionManager.Initialize(occurredAt);

            var currentSession = sessionManager.CurrentSession;
            Assert.That(currentSession, Is.Not.Null);

            sessionManager.HandleTick(200f, occurredAt.AddSeconds(200));
            sessionManager.HandleSuccessfulForegroundFlush(currentSession!.Id, occurredAt.AddSeconds(210));
            sessionManager.HandleTick(120f, occurredAt.AddSeconds(330));

            Assert.That(sessionManager.CurrentSession!.LastActivityAt, Is.EqualTo(occurredAt.AddSeconds(210)));
            Assert.That(lifecycleQueue.LifecycleKinds, Is.Empty);
        }

        [Test]
        public void HandleSuccessfulForegroundFlushIgnoresStaleSessionIds()
        {
            var occurredAt = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);
            var sessionManager = CreateSessionManager(new RecordingSessionLifecycleQueue());
            sessionManager.Initialize(occurredAt);

            var originalLastActivityAt = sessionManager.CurrentSession!.LastActivityAt;
            sessionManager.HandleSuccessfulForegroundFlush("stale_session", occurredAt.AddSeconds(30));

            Assert.That(sessionManager.CurrentSession!.LastActivityAt, Is.EqualTo(originalLastActivityAt));
        }

        [Test]
        public void InitializeSkipsSessionCreationWhenConsentBlocksSessionTracking()
        {
            var occurredAt = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);
            var sessionManager = CreateSessionManager(
                new RecordingSessionLifecycleQueue(),
                () => false);

            sessionManager.Initialize(occurredAt);

            Assert.That(sessionManager.CurrentSession, Is.Null);
        }

        [Test]
        public void AnonymousSessionSnapshotIsRestoredWithEmptyDeviceId()
        {
            var storageKey = "attriax.tests.session.anonymous." + Guid.NewGuid().ToString("N");
            _storageKeys.Add(storageKey);

            var occurredAt = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);
            var store = new AttriaxPlayerPrefsSessionStore(storageKey, (_, _) => { });

            var anonymousSession = new AttriaxSessionSnapshot
            {
                Id = Guid.NewGuid().ToString("N"),
                DeviceId = string.Empty,
                Platform = AttriaxPlatformType.UnityEditor,
                Locale = "en-US",
                IsFirstLaunch = true,
                StartedAt = occurredAt,
                LastActivityAt = occurredAt,
                HeartbeatIntervalMs = 30000,
                AppVersion = "1.0.0",
                AppBuildNumber = "1",
                AppPackageName = "com.attriax.test",
                SdkPackageVersion = "0.4.0",
            };

            store.WriteSessionSnapshot(anonymousSession);
            var restored = store.ReadSessionSnapshot();

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.DeviceId, Is.EqualTo(string.Empty));
            Assert.That(restored.Id, Is.EqualTo(anonymousSession.Id));
        }

        private AttriaxSessionManager CreateSessionManager(
            IAttriaxSessionLifecycleQueue lifecycleQueue,
            Func<bool>? canTrackSessions = null)
        {
            var storageKey = "attriax.tests.session." + Guid.NewGuid().ToString("N");
            _storageKeys.Add(storageKey);

            var runtimeState = new AttriaxRuntimeState
            {
                IsEnabled = true,
                IsFirstLaunch = false,
                DeviceId = "device_1",
                DeviceIdSource = "persistent_storage",
            };
            var contextSnapshot = new AttriaxContextSnapshot
            {
                Platform = AttriaxPlatformType.Android,
                DeviceId = "device_1",
                App = new AttriaxAppSnapshot
                {
                    Version = "1.0.0",
                    BuildNumber = "1",
                    PackageName = "com.attriax.test",
                },
                Device = new AttriaxDeviceSnapshot
                {
                    Language = "en-US",
                    Metadata = new Dictionary<string, object>(),
                    SupportedAbis = new List<string>(),
                },
                Sdk = new AttriaxSdkSnapshot
                {
                    ApiVersion = "v1",
                    PackageVersion = "0.4.0",
                    Metadata = new Dictionary<string, object>(),
                },
            };
            var contextManager = new AttriaxContextManager(new StaticContextRefreshProvider(contextSnapshot), (_, _) => { });
            contextManager.SetPreparedContext(
                new AttriaxPreparedContextRefresh(
                    contextSnapshot,
                    Task.FromResult(contextSnapshot)),
                includesInstallReferrer: false);

            return new AttriaxSessionManager(
                sessionTrackingEnabled: true,
                canTrackSessions: canTrackSessions ?? (() => true),
                runtimeState: runtimeState,
                contextManager: contextManager,
                firstLaunchHeartbeatIntervalMs: 30000,
                sessionHeartbeatIntervalMs: 300000,
                sessionStore: new AttriaxPlayerPrefsSessionStore(
                    storageKey,
                    (_, _) => { }),
                sessionLifecycleQueue: lifecycleQueue,
                debugLog: (_, _) => { });
        }

        private sealed class StaticContextRefreshProvider : IAttriaxContextRefreshProvider
        {
            private readonly AttriaxContextSnapshot _snapshot;

            public StaticContextRefreshProvider(AttriaxContextSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public Task<AttriaxPreparedContextRefresh> PrepareContextRefreshAsync(bool resolveInstallReferrer)
            {
                return Task.FromResult(new AttriaxPreparedContextRefresh(
                    _snapshot,
                    Task.FromResult(_snapshot)));
            }
        }

        private sealed class RecordingSessionLifecycleQueue : IAttriaxSessionLifecycleQueue
        {
            public List<SdkSessionLifecycleKind> LifecycleKinds { get; } = new List<SdkSessionLifecycleKind>();

            public void QueueSessionLifecycle(
                SdkSessionLifecycleKind kind,
                AttriaxSessionSnapshot session,
                DateTimeOffset occurredAt,
                IDictionary<string, object>? metadata = null)
            {
                LifecycleKinds.Add(kind);
            }
        }
    }
}