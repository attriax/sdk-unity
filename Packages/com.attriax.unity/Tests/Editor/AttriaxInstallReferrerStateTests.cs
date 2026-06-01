#nullable enable
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxInstallReferrerStateTests
    {
        [Test]
        public void TaskThrowsBeforeInitialization()
        {
            var state = new AttriaxInstallReferrerState();

            Assert.ThrowsAsync<System.InvalidOperationException>(async () => await state.Task);
        }

        [Test]
        public async Task PrepareForEnabledStateCompletesWithCachedDetails()
        {
            var state = new AttriaxInstallReferrerState();
            var details = new AttriaxInstallReferrerDetails
            {
                RawPlatformInstallReferrer = "utm_source=play_store",
                Campaign = "spring_launch",
            };

            state.PrepareForEnabledState(details);

            var resolved = await state.Task;
            Assert.That(resolved, Is.SameAs(details));
            Assert.That(state.HasPendingCompletion, Is.False);
        }

        [Test]
        public async Task PrepareForEnabledStateReopensAfterDisabledCompletion()
        {
            var state = new AttriaxInstallReferrerState();

            state.EnsureTaskSource();
            state.Complete(null, disabledResult: true);
            Assert.That(await state.Task, Is.Null);

            state.PrepareForEnabledState(null);

            Assert.That(state.HasPendingCompletion, Is.True);
            Assert.That(state.Task.IsCompleted, Is.False);
        }

        [Test]
        public void ResetRestoresThePreInitializationContract()
        {
            var state = new AttriaxInstallReferrerState();

            state.EnsureTaskSource();
            state.Reset();

            Assert.That(state.HasPendingCompletion, Is.False);
            Assert.ThrowsAsync<System.InvalidOperationException>(async () => await state.Task);
        }
    }
}