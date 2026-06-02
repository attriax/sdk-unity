#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxTrackingAuthorizationManagerTests
    {
        [Test]
        public async Task GetTrackingAuthorizationStatusReadsPlatformWhenAdIdsDisabled()
        {
            var bridge = new FakeTrackingAuthorizationBridge
            {
                TrackingAuthorizationStatus = AttriaxTrackingAuthorizationStatus.Authorized,
            };
            var manager = CreateManager(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    CollectAdvertisingId = false,
                },
                bridge);

            var status = await manager.GetTrackingAuthorizationStatusAsync();

            Assert.That(status, Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
            Assert.That(bridge.GetTrackingAuthorizationStatusCalls, Is.EqualTo(1));
            Assert.That(
                await manager.WaitForTrackingAuthorizationIfNeededAsync(),
                Is.EqualTo(AttriaxTrackingAuthorizationStatus.Disabled));
        }

        [Test]
        public async Task RequestTrackingAuthorizationForwardsRealRequestWhenAdIdsDisabled()
        {
            var bridge = new FakeTrackingAuthorizationBridge
            {
                RequestTrackingAuthorizationStatus = AttriaxTrackingAuthorizationStatus.Authorized,
            };
            var manager = CreateManager(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    CollectAdvertisingId = false,
                },
                bridge);

            var status = await manager.RequestTrackingAuthorizationAsync();

            Assert.That(status, Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task RequestTrackingAuthorizationOnInitRequestsOnceBeforeStartupCollection()
        {
            var bridge = new FakeTrackingAuthorizationBridge();
            var manager = CreateManager(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    RequestTrackingAuthorizationOnInit = true,
                },
                bridge);

            Assert.That(
                await manager.WaitForTrackingAuthorizationIfNeededAsync(),
                Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
            Assert.That(
                await manager.WaitForTrackingAuthorizationIfNeededAsync(),
                Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));

            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(1));
            Assert.That(bridge.GetTrackingAuthorizationStatusCalls, Is.EqualTo(0));
        }

        [Test]
        public async Task RequestTrackingAuthorizationWaitsForResolvedStatusAfterPrematureNotDetermined()
        {
            var bridge = new FakeTrackingAuthorizationBridge
            {
                RequestTrackingAuthorizationStatus = AttriaxTrackingAuthorizationStatus.NotDetermined,
                TrackingAuthorizationStatusResponses = new Queue<AttriaxTrackingAuthorizationStatus>(
                    new[]
                    {
                        AttriaxTrackingAuthorizationStatus.NotDetermined,
                        AttriaxTrackingAuthorizationStatus.Authorized,
                    }),
            };
            var manager = CreateManager(new AttriaxConfig { ProjectToken = "ax_test" }, bridge);

            var status = await manager.RequestTrackingAuthorizationAsync();

            Assert.That(status, Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(1));
            Assert.That(bridge.GetTrackingAuthorizationStatusCalls, Is.EqualTo(2));
        }

        [Test]
        public async Task RequestTrackingAuthorizationTimesOutWhenStatusNeverResolves()
        {
            var bridge = new FakeTrackingAuthorizationBridge
            {
                RequestTrackingAuthorizationStatus = AttriaxTrackingAuthorizationStatus.NotDetermined,
                TrackingAuthorizationStatus = AttriaxTrackingAuthorizationStatus.NotDetermined,
            };
            var manager = CreateManager(new AttriaxConfig { ProjectToken = "ax_test" }, bridge);

            var status = await manager.RequestTrackingAuthorizationAsync(timeoutMs: 35);

            Assert.That(status, Is.EqualTo(AttriaxTrackingAuthorizationStatus.TimedOut));
            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(1));
            Assert.That(bridge.GetTrackingAuthorizationStatusCalls, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task StartupPollingJoinsExplicitTrackingAuthorizationRequest()
        {
            var requestCompletion = new TaskCompletionSource<AttriaxTrackingAuthorizationStatus>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var bridge = new FakeTrackingAuthorizationBridge
            {
                TrackingAuthorizationStatusResponses = new Queue<AttriaxTrackingAuthorizationStatus>(
                    new[] { AttriaxTrackingAuthorizationStatus.NotDetermined }),
                RequestTrackingAuthorizationTask = requestCompletion.Task,
            };
            var manager = CreateManager(
                new AttriaxConfig
                {
                    ProjectToken = "ax_test",
                    TrackingAuthorizationStatusTimeoutMs = 200,
                },
                bridge,
                startupTrackingAuthorizationPollInterval: System.TimeSpan.FromSeconds(1));

            var startupWait = manager.WaitForTrackingAuthorizationIfNeededAsync();
            await Task.Delay(5);

            Assert.That(bridge.GetTrackingAuthorizationStatusCalls, Is.EqualTo(1));
            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(0));

            var manualRequest = manager.RequestTrackingAuthorizationAsync();
            await Task.Delay(5);

            Assert.That(bridge.RequestTrackingAuthorizationCalls, Is.EqualTo(1));

            requestCompletion.SetResult(AttriaxTrackingAuthorizationStatus.Authorized);

            Assert.That(
                await manualRequest,
                Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
            Assert.That(
                await startupWait,
                Is.EqualTo(AttriaxTrackingAuthorizationStatus.Authorized));
        }

        private static AttriaxTrackingAuthorizationManager CreateManager(
            AttriaxConfig config,
            FakeTrackingAuthorizationBridge bridge,
            System.TimeSpan? pendingTrackingAuthorizationPollInterval = null,
            System.TimeSpan? startupTrackingAuthorizationPollInterval = null)
        {
            return new AttriaxTrackingAuthorizationManager(
                config,
                AttriaxPlatformType.IOS,
                bridge.GetTrackingAuthorizationStatusAsync,
                bridge.RequestTrackingAuthorizationAsync,
                pendingTrackingAuthorizationPollInterval:
                    pendingTrackingAuthorizationPollInterval ?? System.TimeSpan.FromMilliseconds(10),
                startupTrackingAuthorizationPollInterval:
                    startupTrackingAuthorizationPollInterval ?? System.TimeSpan.FromMilliseconds(10));
        }

        private sealed class FakeTrackingAuthorizationBridge
        {
            public int RequestTrackingAuthorizationCalls { get; private set; }

            public int GetTrackingAuthorizationStatusCalls { get; private set; }

            public Queue<AttriaxTrackingAuthorizationStatus> TrackingAuthorizationStatusResponses { get; set; }
                = new Queue<AttriaxTrackingAuthorizationStatus>();

            public AttriaxTrackingAuthorizationStatus TrackingAuthorizationStatus { get; set; }
                = AttriaxTrackingAuthorizationStatus.NotDetermined;

            public AttriaxTrackingAuthorizationStatus RequestTrackingAuthorizationStatus { get; set; }
                = AttriaxTrackingAuthorizationStatus.Authorized;

            public Task<AttriaxTrackingAuthorizationStatus>? RequestTrackingAuthorizationTask { get; set; }

            public Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync()
            {
                GetTrackingAuthorizationStatusCalls += 1;

                if (TrackingAuthorizationStatusResponses.Count > 0)
                {
                    return Task.FromResult(TrackingAuthorizationStatusResponses.Dequeue());
                }

                return Task.FromResult(TrackingAuthorizationStatus);
            }

            public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync()
            {
                RequestTrackingAuthorizationCalls += 1;

                if (RequestTrackingAuthorizationTask != null)
                {
                    return RequestTrackingAuthorizationTask;
                }

                return Task.FromResult(RequestTrackingAuthorizationStatus);
            }
        }
    }
}