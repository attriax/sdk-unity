#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    [DefaultExecutionOrder(-10000)]
    internal sealed class AttriaxConfiguredHost : MonoBehaviour
    {
        private static AttriaxConfiguredHost? _instance;

        private AttriaxProjectSettings? _settings;
        private Task? _initializationTask;

        internal Attriax? Instance { get; private set; }

        internal static AttriaxConfiguredHost EnsureCreated(AttriaxProjectSettings settings)
        {
            if (_instance != null)
            {
                _instance.ApplySettings(settings);
                return _instance;
            }

            var existing = FindFirstObjectByType<AttriaxConfiguredHost>();
            if (existing != null)
            {
                existing.ApplySettings(settings);
                _instance = existing;
                return existing;
            }

            var gameObject = new GameObject("AttriaxConfiguredHost");
            var host = gameObject.AddComponent<AttriaxConfiguredHost>();
            host.ApplySettings(settings);
            if (settings.PersistConfiguredInstanceAcrossSceneLoads)
            {
                DontDestroyOnLoad(gameObject);
            }

            _instance = host;
            return host;
        }

        internal Task InitializeIfNeededAsync()
        {
            EnsureInstanceCreated();
            if (Instance == null)
            {
                throw new InvalidOperationException("Configured Attriax instance could not be created.");
            }

            if (Instance.IsInitialized)
            {
                return Task.CompletedTask;
            }

            if (_initializationTask == null)
            {
                _initializationTask = RunInitializationAsync();
            }

            return AwaitInitializationAsync(_initializationTask);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            EnsureInstanceCreated();

            if (_settings != null && _settings.PersistConfiguredInstanceAcrossSceneLoads)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            Instance?.Dispose();
            Instance = null;
        }

        internal void ApplySettings(AttriaxProjectSettings settings)
        {
            _settings = settings;
            EnsureInstanceCreated();
        }

        private void EnsureInstanceCreated()
        {
            if (Instance != null)
            {
                return;
            }

            if (_settings == null)
            {
                return;
            }

            Instance = new Attriax(_settings.CreateRuntimeConfig());
        }

        private async Task RunInitializationAsync()
        {
            if (_settings == null || Instance == null)
            {
                throw new InvalidOperationException(
                    "Attriax configured runtime is missing project settings.");
            }

            await Instance.InitializeAsync(_settings.CreateInitOptions());
        }

        private async Task AwaitInitializationAsync(Task initializationTask)
        {
            try
            {
                await initializationTask;
            }
            finally
            {
                if (ReferenceEquals(_initializationTask, initializationTask))
                {
                    _initializationTask = null;
                }
            }
        }
    }
}