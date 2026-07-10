#nullable enable
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxInitialDeepLinkStateTests
    {
        [Test]
        public async Task WaitForInitialDeepLinkStartsPendingBeforeInitialization()
        {
            var runtime = new AttriaxRuntime(new AttriaxConfig
            {
                ProjectToken = "ax_test",
            });

            var waitForInitialDeepLink = runtime.WaitForInitialDeepLink;

            Assert.That(waitForInitialDeepLink.IsCompleted, Is.False);

            runtime.Dispose();

            Assert.That(await waitForInitialDeepLink, Is.Null);
        }
    }
}