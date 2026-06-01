#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Attriax.Unity
{
    /// <summary>
    /// Scene component wrapper for teams that prefer MonoBehaviour-driven SDK access.
    /// </summary>
    [AddComponentMenu("Attriax/Attriax Behaviour")]
    [DisallowMultipleComponent]
    public sealed class AttriaxBehaviour : MonoBehaviour
    {
        [Header("Attriax")]
        [FormerlySerializedAs("_appToken")]
        [SerializeField] private string _projectToken = string.Empty;
        [SerializeField] private string _apiBaseUrl = string.Empty;
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _enableDebugLogs;
        [SerializeField] private bool _gdprEnabled;
        [SerializeField] private bool _collectAdvertisingId = true;
        [SerializeField] private bool _automaticCrashReportingEnabled = true;
        [SerializeField] private bool _requestTrackingAuthorizationOnInit;
        [SerializeField] private int _trackingAuthorizationStatusTimeoutMs = 60000;
        [SerializeField] private bool _automaticSceneTracking = true;
        [SerializeField] private bool _captureInitialUrl = true;
        [SerializeField] private bool _persistAcrossScenes = true;

        private IDisposable? _deepLinkSubscription;
        private IDisposable? _synchronizationSubscription;
        private AttriaxConfig? _configuredRuntimeConfig;
        private AttriaxInitOptions? _configuredInitOptions;

        /// <summary>
        /// Creates an inactive host component, applies configuration, then activates it.
        /// Call <see cref="InitializeAsync"/> manually or use <see cref="CreateAndInitializeHostAsync"/>.
        /// </summary>
        public static AttriaxBehaviour CreateHost(
            AttriaxConfig config,
            AttriaxInitOptions? initOptions = null,
            bool persistAcrossScenes = true,
            string gameObjectName = "Attriax")
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var hostName = string.IsNullOrWhiteSpace(gameObjectName) ? "Attriax" : gameObjectName;
            var gameObject = new GameObject(hostName);
            gameObject.SetActive(false);

            var behaviour = gameObject.AddComponent<AttriaxBehaviour>();
            behaviour.Configure(config, initOptions, persistAcrossScenes);

            gameObject.SetActive(true);
            return behaviour;
        }

        /// <summary>
        /// Creates a host component, optionally marks it as persistent, and initializes it immediately.
        /// </summary>
        public static async Task<AttriaxBehaviour> CreateAndInitializeHostAsync(
            AttriaxConfig config,
            AttriaxInitOptions? initOptions = null,
            bool persistAcrossScenes = true,
            string gameObjectName = "Attriax")
        {
            var behaviour = CreateHost(config, initOptions, persistAcrossScenes, gameObjectName);
            await behaviour.InitializeAsync();
            return behaviour;
        }

        /// <summary>
        /// Gets the SDK instance owned by this component.
        /// </summary>
        public Attriax? Instance { get; private set; }

        /// <summary>
        /// Raised when a deep link is observed by this component instance.
        /// </summary>
        public event Action<AttriaxDeepLinkEvent>? DeepLinkReceived;

        /// <summary>
        /// Raised when the SDK synchronization state changes.
        /// </summary>
        public event Action<AttriaxSynchronizationState>? SynchronizationChanged;

        /// <summary>
        /// Returns <see langword="true"/> when the component instance is initialized.
        /// </summary>
        public bool IsReady
        {
            get { return Instance?.IsInitialized == true; }
        }

        /// <summary>
        /// Regulation-scoped consent helpers for the component-owned instance.
        /// </summary>
        public AttriaxConsent? Consent
        {
            get { return Instance?.Consent; }
        }

        /// <summary>
        /// Event tracking, revenue, identity, and handled-error helpers for the component-owned instance.
        /// </summary>
        public AttriaxTracking? Tracking
        {
            get { return Instance?.Tracking; }
        }

        /// <summary>
        /// Synchronization state and subscriptions for the component-owned instance.
        /// </summary>
        public AttriaxSynchronization? Synchronization
        {
            get { return Instance?.Synchronization; }
        }

        /// <summary>
        /// Deep-link state and subscriptions for the component-owned instance.
        /// </summary>
        public AttriaxDeepLinks? DeepLinks
        {
            get { return Instance?.DeepLinks; }
        }

        /// <summary>
        /// Startup referrer state retained for the component-owned instance.
        /// </summary>
        public AttriaxReferrer? Referrer
        {
            get { return Instance?.Referrer; }
        }

        /// <summary>
        /// Dashboard-managed SKAN state and manual update helpers for the component-owned instance.
        /// </summary>
        public AttriaxSkan? Skan
        {
            get { return Instance?.Skan; }
        }

        private async void Awake()
        {
            if (_persistAcrossScenes && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (!_initializeOnAwake)
            {
                return;
            }

            if (!CanCreateRuntimeConfig())
            {
                Debug.LogWarning(
                    "AttriaxBehaviour is set to initialize on Awake, but no project token is configured. Fill the inspector fields, use Tools/Attriax/Configuration for the configured singleton flow, or call Configure(...) from code before InitializeAsync().",
                    this);
                return;
            }

            try
            {
                await InitializeAsync();
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
        }

        /// <summary>
        /// Applies a full runtime configuration for code-created hosts.
        /// This path disables Awake auto-initialization so the caller can explicitly decide when to initialize.
        /// </summary>
        public void Configure(
            AttriaxConfig config,
            AttriaxInitOptions? initOptions = null,
            bool persistAcrossScenes = true)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (Instance != null)
            {
                throw new InvalidOperationException(
                    "AttriaxBehaviour cannot be reconfigured after the SDK instance has been created.");
            }

            _configuredRuntimeConfig = CloneRuntimeConfig(config);
            _configuredInitOptions = initOptions != null ? CloneInitOptions(initOptions) : null;

            _projectToken = config.ProjectToken;
            _apiBaseUrl = config.ApiBaseUrl ?? string.Empty;
            _enableDebugLogs = config.EnableDebugLogs;
            _gdprEnabled = config.GdprEnabled;
            _collectAdvertisingId = config.CollectAdvertisingId;
            _automaticCrashReportingEnabled = config.AutomaticCrashReportingEnabled;
            _requestTrackingAuthorizationOnInit = config.RequestTrackingAuthorizationOnInit;
            _trackingAuthorizationStatusTimeoutMs = config.TrackingAuthorizationStatusTimeoutMs;
            _automaticSceneTracking = config.AutomaticSceneTracking;
            _persistAcrossScenes = persistAcrossScenes;
            _initializeOnAwake = false;

            if (_configuredInitOptions != null)
            {
                _captureInitialUrl = _configuredInitOptions.CaptureInitialUrl;
            }

            if (_persistAcrossScenes && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Creates the component-owned SDK instance if needed and initializes it.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!CanCreateRuntimeConfig())
            {
                throw new InvalidOperationException(
                    "AttriaxBehaviour requires a project token before initialization. Configure the component in the inspector, call Configure(...) from code, or use the configured singleton flow from Tools/Attriax/Configuration.");
            }

            var instance = EnsureInstance();
            await instance.InitializeAsync(BuildInitOptions());
        }

        private void HandleDeepLinkReceived(AttriaxDeepLinkEvent deepLinkEvent)
        {
            DeepLinkReceived?.Invoke(deepLinkEvent);
        }

        private void HandleSynchronizationChanged(AttriaxSynchronizationState state)
        {
            SynchronizationChanged?.Invoke(state);
        }

        private Attriax EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var runtimeConfig = _configuredRuntimeConfig != null
                ? CloneRuntimeConfig(_configuredRuntimeConfig)
                : new AttriaxConfig
                {
                    ProjectToken = _projectToken,
                    ApiBaseUrl = string.IsNullOrWhiteSpace(_apiBaseUrl) ? null : _apiBaseUrl,
                    EnableDebugLogs = _enableDebugLogs,
                    GdprEnabled = _gdprEnabled,
                    CollectAdvertisingId = _collectAdvertisingId,
                    AutomaticCrashReportingEnabled = _automaticCrashReportingEnabled,
                    RequestTrackingAuthorizationOnInit = _requestTrackingAuthorizationOnInit,
                    TrackingAuthorizationStatusTimeoutMs = _trackingAuthorizationStatusTimeoutMs,
                    AutomaticSceneTracking = _automaticSceneTracking,
                };

            Instance = new Attriax(runtimeConfig);

            _deepLinkSubscription = Instance.DeepLinks.Stream.Subscribe(HandleDeepLinkReceived);
            _synchronizationSubscription = Instance.Synchronization.Subscribe(
                HandleSynchronizationChanged);
            return Instance;
        }

        private bool CanCreateRuntimeConfig()
        {
            var configuredToken = _configuredRuntimeConfig?.ProjectToken;
            return !string.IsNullOrWhiteSpace(configuredToken ?? _projectToken);
        }

        private AttriaxInitOptions BuildInitOptions()
        {
            if (_configuredInitOptions != null)
            {
                return CloneInitOptions(_configuredInitOptions);
            }

            return new AttriaxInitOptions
            {
                CaptureInitialUrl = _captureInitialUrl,
            };
        }

        private static AttriaxConfig CloneRuntimeConfig(AttriaxConfig config)
        {
            return new AttriaxConfig
            {
                ProjectToken = config.ProjectToken,
                ApiBaseUrl = config.ApiBaseUrl,
                AppVersion = config.AppVersion,
                AppBuildNumber = config.AppBuildNumber,
                AppPackageName = config.AppPackageName,
                SdkMetadata = config.SdkMetadata != null
                    ? new Dictionary<string, object>(config.SdkMetadata)
                    : new Dictionary<string, object>(),
                EnableDebugLogs = config.EnableDebugLogs,
                GdprEnabled = config.GdprEnabled,
                CollectAdvertisingId = config.CollectAdvertisingId,
                AutomaticCrashReportingEnabled = config.AutomaticCrashReportingEnabled,
                RequestTrackingAuthorizationOnInit = config.RequestTrackingAuthorizationOnInit,
                TrackingAuthorizationStatusTimeoutMs = config.TrackingAuthorizationStatusTimeoutMs,
                AutomaticSceneTracking = config.AutomaticSceneTracking,
                RequestTimeoutMs = config.RequestTimeoutMs,
                MaxQueueSize = config.MaxQueueSize,
                StorageKeyPrefix = config.StorageKeyPrefix,
                Skan = CloneSkanConfig(config.Skan),
            };
        }

        private static AttriaxSkanConfig? CloneSkanConfig(AttriaxSkanConfig? config)
        {
            if (config == null)
            {
                return null;
            }

            return new AttriaxSkanConfig
            {
                Enabled = config.Enabled,
                RegisterFirstLaunchValue = config.RegisterFirstLaunchValue,
            };
        }

        private static AttriaxInitOptions CloneInitOptions(AttriaxInitOptions options)
        {
            return new AttriaxInitOptions
            {
                Enabled = options.Enabled,
                EventsEnabled = options.EventsEnabled,
                CaptureInitialUrl = options.CaptureInitialUrl,
            };
        }

        private void OnDestroy()
        {
            _deepLinkSubscription?.Dispose();
            _synchronizationSubscription?.Dispose();
            if (Instance != null)
            {
                Instance.Dispose();
                Instance = null;
            }

            _configuredRuntimeConfig = null;
            _configuredInitOptions = null;
        }
    }
}