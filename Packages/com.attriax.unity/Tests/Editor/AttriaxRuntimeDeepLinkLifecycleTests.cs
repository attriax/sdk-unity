#nullable enable
using System;
using System.Reflection;
using NUnit.Framework;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRuntimeDeepLinkLifecycleTests
    {
        [Test]
        public void DisabledRuntimeIgnoresDeepLinkActivation()
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

                runtimeState!.IsInitialized = true;
                runtimeState.IsEnabled = false;

                AttriaxRawDeepLinkEvent? observed = null;
                using var subscription = runtime.SubscribeToRawDeepLinks(value => observed = value);

                var handleDeepLinkActivated = typeof(AttriaxRuntime).GetMethod(
                    "HandleDeepLinkActivated",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(handleDeepLinkActivated, Is.Not.Null);

                handleDeepLinkActivated!.Invoke(runtime, new object?[] { "https://attriax.test/path" });

                Assert.That(observed, Is.Null);
            }
            finally
            {
                runtime.Dispose();
            }
        }
    }
}