#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Attriax.Unity.Internal;
using UnityEngine;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRuntimeBootstrapCoordinatorTests
    {
        [Test]
        public async Task InitializeAsyncWithDisabledOptionCompletesBootstrapInDisabledState()
        {
            var runtime = CreateRuntime();

            try
            {
                await runtime.InitializeAsync(new AttriaxInitOptions
                {
                    Enabled = false,
                    CaptureInitialUrl = false,
                });

                var runtimeState = GetRuntimeState(runtime);
                Assert.That(runtimeState.IsInitialized, Is.True);
                Assert.That(runtimeState.IsEnabled, Is.False);
                Assert.That(runtime.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Disabled));
                Assert.That(runtime.InitialDeepLinkResolved, Is.True);
                Assert.That(await runtime.OriginalInstallReferrer, Is.Null);
                Assert.That(await runtime.ReinstallReferrer, Is.Null);
                Assert.That(await runtime.WaitForInitialDeepLink, Is.Null);
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public async Task InitializeAsyncHonorsEventsEnabledOverrideDuringBootstrap()
        {
            var runtime = CreateRuntime();

            try
            {
                await runtime.InitializeAsync(new AttriaxInitOptions
                {
                    EventsEnabled = false,
                    CaptureInitialUrl = false,
                });

                var runtimeState = GetRuntimeState(runtime);
                Assert.That(runtimeState.IsInitialized, Is.True);
                Assert.That(runtimeState.AreEventsEnabled, Is.False);
                Assert.That(runtime.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Synchronized));
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public async Task InitializeAsyncWithoutInitialUrlCaptureMarksInitialDeepLinkUnavailable()
        {
            var runtime = CreateRuntime();

            try
            {
                await runtime.InitializeAsync(new AttriaxInitOptions
                {
                    CaptureInitialUrl = false,
                });

                var deepLinkManager = GetDeepLinkManager(runtime);
                Assert.That(runtime.RawInitialDeepLinkValue, Is.Null);
                Assert.That(runtime.InitialDeepLinkValue, Is.Null);
                Assert.That(GetPrivateBool(deepLinkManager, "_initialUrlPending"), Is.False);
                Assert.That(GetPrivateBool(deepLinkManager, "_appOpenPending"), Is.True);
                Assert.That(runtime.InitialDeepLinkResolved, Is.False);
                Assert.That(runtime.SynchronizationState, Is.EqualTo(AttriaxSynchronizationState.Synchronized));
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public async Task PendingConsentBootstrapKeepsFirstLaunchMarkerAcrossRestartsWithoutDeviceIdentity()
        {
            var appToken = "ax_test_" + Guid.NewGuid().ToString("N");
            var firstRuntime = CreateRuntime(appToken, gdprEnabled: true);

            try
            {
                await firstRuntime.InitializeAsync(new AttriaxInitOptions
                {
                    CaptureInitialUrl = false,
                });

                Assert.That(firstRuntime.IsFirstLaunch, Is.True);
                Assert.That(firstRuntime.DeviceId, Is.Null);
                Assert.That(PlayerPrefs.HasKey(GetStorageKey(firstRuntime, "hasLaunched")), Is.True);
                Assert.That(PlayerPrefs.HasKey(GetStorageKey(firstRuntime, "deviceId")), Is.False);
                Assert.That(GetContextSnapshot(firstRuntime).DeviceId, Is.Empty);
            }
            finally
            {
                firstRuntime.Dispose();
            }

            var restartedRuntime = CreateRuntime(appToken, gdprEnabled: true);
            try
            {
                await restartedRuntime.InitializeAsync(new AttriaxInitOptions
                {
                    CaptureInitialUrl = false,
                });

                Assert.That(restartedRuntime.IsFirstLaunch, Is.False);
                Assert.That(restartedRuntime.DeviceId, Is.Null);
                Assert.That(GetContextSnapshot(restartedRuntime).DeviceId, Is.Empty);
            }
            finally
            {
                await restartedRuntime.ResetAsync();
                restartedRuntime.Dispose();
            }
        }

        private static AttriaxRuntime CreateRuntime(string? appToken = null, bool gdprEnabled = false)
        {
            var runtime = new AttriaxRuntime(new AttriaxConfig
            {
                AppToken = appToken ?? "ax_test_" + Guid.NewGuid().ToString("N"),
                GdprEnabled = gdprEnabled,
            });

            var lifecycleAttachedField = typeof(AttriaxRuntime).GetField(
                "_lifecycleAttached",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(lifecycleAttachedField, Is.Not.Null);
            lifecycleAttachedField!.SetValue(runtime, true);

            return runtime;
        }

        private static AttriaxRuntimeState GetRuntimeState(AttriaxRuntime runtime)
        {
            var runtimeStateField = typeof(AttriaxRuntime).GetField(
                "_runtimeState",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(runtimeStateField, Is.Not.Null);

            var runtimeState = runtimeStateField!.GetValue(runtime) as AttriaxRuntimeState;
            Assert.That(runtimeState, Is.Not.Null);
            return runtimeState!;
        }

        private static object GetDeepLinkManager(AttriaxRuntime runtime)
        {
            var deepLinkManagerField = typeof(AttriaxRuntime).GetField(
                "_deepLinkManager",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(deepLinkManagerField, Is.Not.Null);

            var deepLinkManager = deepLinkManagerField!.GetValue(runtime);
            Assert.That(deepLinkManager, Is.Not.Null);
            return deepLinkManager!;
        }

        private static AttriaxContextSnapshot GetContextSnapshot(AttriaxRuntime runtime)
        {
            var contextManagerField = typeof(AttriaxRuntime).GetField(
                "_contextManager",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(contextManagerField, Is.Not.Null);

            var contextManager = contextManagerField!.GetValue(runtime);
            Assert.That(contextManager, Is.Not.Null);

            var snapshotProperty = contextManager!.GetType().GetProperty(
                "Snapshot",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(snapshotProperty, Is.Not.Null);

            var snapshot = snapshotProperty!.GetValue(contextManager) as AttriaxContextSnapshot;
            Assert.That(snapshot, Is.Not.Null);
            return snapshot!;
        }

        private static string GetStorageKey(AttriaxRuntime runtime, string suffix)
        {
            var keyMethod = typeof(AttriaxRuntime).GetMethod(
                "Key",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(keyMethod, Is.Not.Null);

            return (string)keyMethod!.Invoke(runtime, new object[] { suffix })!;
        }

        private static bool GetPrivateBool(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (bool)field!.GetValue(instance)!;
        }
    }
}