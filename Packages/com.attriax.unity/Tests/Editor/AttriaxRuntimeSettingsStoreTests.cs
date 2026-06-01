#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxRuntimeSettingsStoreTests
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
        public void ReadsAndWritesRuntimeFlagsAndDeviceIdentity()
        {
            var store = CreateStore();

            store.WriteEnabled(false);
            store.WriteEventsEnabled(false);
            store.WriteHasLaunched(true);
            store.WriteDeviceId("device-123");
            store.WriteDeviceIdSource("persistent_storage");

            Assert.That(store.ReadEnabled(true), Is.False);
            Assert.That(store.ReadEventsEnabled(true), Is.False);
            Assert.That(store.ReadHasLaunched(false), Is.True);
            Assert.That(store.ReadDeviceId(), Is.EqualTo("device-123"));
            Assert.That(store.ReadDeviceIdSource(), Is.EqualTo("persistent_storage"));
        }

        [Test]
        public void ClearRemovesManagedKeys()
        {
            var store = CreateStore();
            store.WriteEnabled(false);
            store.WriteEventsEnabled(false);
            store.WriteHasLaunched(true);
            store.WriteDeviceId("device-456");
            store.WriteDeviceIdSource("source-1");

            store.Clear();

            foreach (var storageKey in _storageKeys)
            {
                Assert.That(PlayerPrefs.HasKey(storageKey), Is.False, storageKey);
            }
        }

        private AttriaxRuntimeSettingsStore CreateStore()
        {
            return new AttriaxRuntimeSettingsStore(
                NewStorageKey("deviceId"),
                NewStorageKey("deviceIdSource"),
                NewStorageKey("enabled"),
                NewStorageKey("eventsEnabled"),
                NewStorageKey("hasLaunched"));
        }

        private string NewStorageKey(string suffix)
        {
            var storageKey = "attriax.test.runtime-settings." + suffix + "." + Guid.NewGuid().ToString("N");
            _storageKeys.Add(storageKey);
            return storageKey;
        }
    }
}