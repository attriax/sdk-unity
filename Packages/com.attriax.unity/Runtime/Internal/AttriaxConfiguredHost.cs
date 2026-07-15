#nullable enable
using System;
using System.Linq;
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
        private bool _instanceIsExternallyOwned;

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
            // DontDestroyOnLoad is only legal in play mode; Unity throws outside it (e.g. EditMode tests,
            // editor scripts), which would abort host creation before the singleton is registered below.
            if (settings.PersistConfiguredInstanceAcrossSceneLoads && Application.isPlaying)
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

            // DontDestroyOnLoad is only legal in play mode; Unity throws outside it, which would abort
            // Awake before the duplicate-behaviour diagnostic below runs.
            if (_settings != null && _settings.PersistConfiguredInstanceAcrossSceneLoads && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            WarnIfDuplicateBehaviourExists();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            if (Instance != null && !_instanceIsExternallyOwned)
            {
                Instance.Dispose();
            }

            Instance = null;
        }

        private void WarnIfDuplicateBehaviourExists()
        {
            if (_settings == null)
            {
                return;
            }

            var duplicate = FindObjectsOfType<AttriaxBehaviour>()
                .FirstOrDefault(b =>
                {
                    var token = b.Instance?.Config.ProjectToken;
                    return !string.IsNullOrWhiteSpace(token) &&
                           string.Equals(token, _settings.ProjectToken, StringComparison.Ordinal);
                });

            if (duplicate != null)
            {
                UnityEngine.Debug.LogWarning(
                    "[Attriax] A configured singleton and an AttriaxBehaviour both target the same project token. " +
                    "The AttriaxBehaviour will reuse the configured instance to avoid duplicate sessions. " +
                    "Consider removing one initialization path.",
                    duplicate);
            }
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

            var existing = Attriax.TryGetActiveInstance(_settings.ProjectToken);
            if (existing != null)
            {
                Instance = existing;
                _instanceIsExternallyOwned = true;
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

            await Instance.InitializeAsync(_settings.CreateInitOptions()).ConfigureAwait(false);
        }

        private async Task AwaitInitializationAsync(Task initializationTask)
        {
            try
            {
                await initializationTask.ConfigureAwait(false);
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Configured-runtime initialization failed. The SDK will not be available for this session. "
                    + "See the inner exception for details: " + error.Message);
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