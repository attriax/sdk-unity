#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxAppOpenManagerTests
    {
        [Test]
        public void TracksSuccessfulResultsOnlyAfterAppOpenCompletes()
        {
            var runtimeState = new AttriaxRuntimeState
            {
                IsInitialized = true,
                IsEnabled = true,
            };
            var pipeline = new FakeAppOpenPipeline();
            var manager = new AttriaxAppOpenManager(runtimeState, pipeline, new AttriaxEventHub());

            manager.ScheduleIfNeeded();

            Assert.That(manager.HasSuccessfulResult, Is.False);

            manager.HandleResult(new AttriaxAppOpenResult
            {
                UserId = "user_1",
                IsFirstLaunch = true,
                IsNewUser = true,
            });

            Assert.That(manager.HasSuccessfulResult, Is.True);

            manager.Reset();

            Assert.That(manager.HasSuccessfulResult, Is.False);
        }

        private sealed class FakeAppOpenPipeline : IAttriaxAppOpenPipeline
        {
            public Task<AttriaxAppOpenResult> EnqueueOpenAsync(
                string? installReferrerOverride,
                IDictionary<string, object>? deviceMetadataOverrides)
            {
                return new TaskCompletionSource<AttriaxAppOpenResult>().Task;
            }

            public Task ResolveInstallReferrerFromAppOpenAsync(Task<AttriaxAppOpenResult> openTrackingTask)
            {
                return Task.CompletedTask;
            }

            public AttriaxDeepLinkEvent? BuildDeepLinkEventFromAppOpenResult(AttriaxAppOpenResult result)
            {
                return null;
            }

            public AttriaxAppOpen? ToPublicAppOpen(AttriaxAppOpenResult? result)
            {
                return null;
            }
        }
    }
}