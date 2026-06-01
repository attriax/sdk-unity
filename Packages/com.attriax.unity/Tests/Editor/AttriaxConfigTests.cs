#nullable enable
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxConfigTests
    {
        [Test]
        public void ConstructorRejectsMissingAppToken()
        {
            Assert.Throws<System.ArgumentException>(() => new Attriax(new AttriaxConfig()));
        }

        [Test]
        public void ConstructorAllowsLocalhostHttpForDevelopment()
        {
            var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
                ApiBaseUrl = "http://localhost:3000",
            });

            Assert.That(attriax.Config.ApiBaseUrl, Is.EqualTo("http://localhost:3000"));
            attriax.Dispose();
        }

        [Test]
        public void ConstructorAllowsIpv6LoopbackHttpForDevelopment()
        {
            var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
                ApiBaseUrl = "http://[::1]:3000",
            });

            Assert.That(attriax.Config.ApiBaseUrl, Is.EqualTo("http://[::1]:3000"));
            attriax.Dispose();
        }

        [Test]
        public void ConstructorRejectsInsecureRemoteApiBaseUrl()
        {
            Assert.Throws<System.ArgumentException>(() => new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
                ApiBaseUrl = "http://example.com",
            }));
        }

        [Test]
        public void ConstructorEnablesAutomaticBrowserHandlingByDefault()
        {
            var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
            });

            Assert.That(attriax.Config.AutomaticBrowserHandling, Is.True);
            Assert.That(attriax.Config.CollectAdvertisingId, Is.True);
            Assert.That(attriax.Config.AutomaticCrashReportingEnabled, Is.True);
            Assert.That(attriax.Config.RequestTrackingAuthorizationOnInit, Is.False);
            Assert.That(attriax.Config.TrackingAuthorizationStatusTimeoutMs, Is.EqualTo(60000));
            attriax.Dispose();
        }

        [Test]
        public void ConstructorAllowsDisablingAutomaticCrashReporting()
        {
            var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
                AutomaticCrashReportingEnabled = false,
            });

            Assert.That(attriax.Config.AutomaticCrashReportingEnabled, Is.False);
            attriax.Dispose();
        }

        [Test]
        public void ConstructorUsesForegroundHeartbeatAndQueueDefaults()
        {
            var attriax = new Attriax(new AttriaxConfig
            {
                AppToken = "ax_test",
            });

            Assert.That(attriax.Config.EventFlushIntervalMs, Is.EqualTo(60000));
            Assert.That(attriax.Config.SessionHeartbeatIntervalMs, Is.EqualTo(300000));
            Assert.That(attriax.Config.FirstLaunchSessionHeartbeatIntervalMs, Is.EqualTo(30000));
            Assert.That(attriax.Config.MaxQueueSize, Is.EqualTo(500));
            attriax.Dispose();
        }
    }
}