#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Attriax.Unity.Internal;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxBehaviourDualInitTests
    {
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _gameObjects)
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            _gameObjects.Clear();

            // Reset static singleton references so tests do not leak state.
            var hostInstanceField = typeof(AttriaxConfiguredHost).GetField(
                "_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            hostInstanceField?.SetValue(null, null);

            var dispatcherInstanceField = typeof(AttriaxLifecycleDispatcher).GetField(
                "_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            dispatcherInstanceField?.SetValue(null, null);
        }

        [Test]
        public async Task BehaviourReusesConfiguredSingletonWhenTokensMatch()
        {
            var token = "ax_test_token_" + Guid.NewGuid().ToString("N");

            // 1. Create configured singleton host.
            var settings = CreateTestProjectSettings(token);
            var host = AttriaxConfiguredHost.EnsureCreated(settings);
            _gameObjects.Add(host.gameObject);

            // 2. Create an AttriaxBehaviour with the same token.
            var behaviourGo = new GameObject("TestBehaviour");
            _gameObjects.Add(behaviourGo);
            var behaviour = behaviourGo.AddComponent<AttriaxBehaviour>();

            SetPrivateField(behaviour, "_projectToken", token);
            SetPrivateField(behaviour, "_initializeOnAwake", false);

            // 3. Initialize the behaviour; it should reuse the configured instance.
            await behaviour.InitializeAsync();

            // 4. Verify the behaviour uses the same Attriax instance as the host.
            Assert.That(behaviour.Instance, Is.Not.Null);
            Assert.That(host.Instance, Is.Not.Null);
            Assert.That(behaviour.Instance, Is.SameAs(host.Instance));
        }

        [Test]
        public async Task BehaviourCreatesOwnInstanceWhenTokensDiffer()
        {
            var configuredToken = "ax_test_token_a_" + Guid.NewGuid().ToString("N");
            var behaviourToken = "ax_test_token_b_" + Guid.NewGuid().ToString("N");

            // 1. Create configured singleton host.
            var settings = CreateTestProjectSettings(configuredToken);
            var host = AttriaxConfiguredHost.EnsureCreated(settings);
            _gameObjects.Add(host.gameObject);

            // 2. Create an AttriaxBehaviour with a different token.
            var behaviourGo = new GameObject("TestBehaviour");
            _gameObjects.Add(behaviourGo);
            var behaviour = behaviourGo.AddComponent<AttriaxBehaviour>();

            SetPrivateField(behaviour, "_projectToken", behaviourToken);
            SetPrivateField(behaviour, "_apiBaseUrl", "https://localhost:9999");
            SetPrivateField(behaviour, "_initializeOnAwake", false);

            // 3. Initialize the behaviour; it should create its own instance.
            await behaviour.InitializeAsync();

            // 4. Verify the behaviour created a separate instance.
            Assert.That(behaviour.Instance, Is.Not.Null);
            Assert.That(host.Instance, Is.Not.Null);
            Assert.That(behaviour.Instance, Is.Not.SameAs(host.Instance));
        }

        private static AttriaxProjectSettings CreateTestProjectSettings(string projectToken)
        {
            var settings = ScriptableObject.CreateInstance<AttriaxProjectSettings>();
            SetPrivateField(settings, "_projectToken", projectToken);
            SetPrivateField(settings, "_apiBaseUrl", "https://localhost:9999");
            SetPrivateField(settings, "_autoInitializeOnLaunch", false);
            return settings;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on {target.GetType().Name}.");
            }

            field.SetValue(target, value);
        }
    }
}
