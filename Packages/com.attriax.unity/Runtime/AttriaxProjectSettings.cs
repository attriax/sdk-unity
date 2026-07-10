#nullable enable
using UnityEngine;

namespace Attriax.Unity
{
    /// <summary>
    /// Saved project configuration for the Attriax Unity SDK.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AttriaxSettings",
        menuName = "Attriax/Settings",
        order = 1000)]
    public sealed class AttriaxProjectSettings : ScriptableObject
    {
        /// <summary>
        /// Resource path used by the configured singleton loader.
        /// </summary>
        public const string ResourcesPath = "Attriax/AttriaxSettings";

        [SerializeField] private string _projectToken = string.Empty;
        [SerializeField] private string _apiBaseUrl = "https://api.attriax.com";
        [SerializeField] private string _appVersion = string.Empty;
        [SerializeField] private string _appBuildNumber = string.Empty;
        [SerializeField] private string _appPackageName = string.Empty;
        [SerializeField] private bool _autoInitializeOnLaunch = true;
        [SerializeField] private bool _enableDebugLogs;
        [SerializeField] private bool _gdprEnabled;
        [SerializeField] private bool _collectAdvertisingId = true;
        [SerializeField] private bool _automaticCrashReportingEnabled = true;
        [SerializeField] private bool _requestTrackingAuthorizationOnInit;
        [SerializeField] private int _trackingAuthorizationStatusTimeoutMs = 60000;
        [SerializeField] private bool _automaticSceneTracking = true;
        [SerializeField] private bool _captureInitialUrl = true;
        [SerializeField] private bool _persistConfiguredInstanceAcrossSceneLoads = true;

        /// <summary>
        /// Project token that authenticates public SDK requests.
        /// </summary>
        public string ProjectToken => _projectToken;

        /// <summary>
        /// Optional API base URL. Leave blank to use the hosted production API.
        /// </summary>
        public string ApiBaseUrl => _apiBaseUrl;

        /// <summary>
        /// Optional version override for the host application.
        /// </summary>
        public string AppVersion => _appVersion;

        /// <summary>
        /// Optional build number override for the host application. When left blank in a built player, Attriax falls back to Application.buildGUID.
        /// </summary>
        public string AppBuildNumber => _appBuildNumber;

        /// <summary>
        /// Optional package or bundle identifier override.
        /// </summary>
        public string AppPackageName => _appPackageName;

        /// <summary>
        /// Automatically initializes the configured instance during startup.
        /// </summary>
        public bool AutoInitializeOnLaunch => _autoInitializeOnLaunch;

        /// <summary>
        /// Enables verbose SDK logging.
        /// </summary>
        public bool EnableDebugLogs => _enableDebugLogs;

        /// <summary>
        /// Enables GDPR-aware capture and dispatch behavior.
        /// </summary>
        public bool GdprEnabled => _gdprEnabled;

        /// <summary>
        /// Whether native platform collectors may include advertising identifiers.
        /// </summary>
        public bool CollectAdvertisingId => _collectAdvertisingId;

        /// <summary>
        /// Enables automatic capture of unhandled Unity exceptions as crash reports.
        /// </summary>
        public bool AutomaticCrashReportingEnabled => _automaticCrashReportingEnabled;

        /// <summary>
        /// Whether initialization should request App Tracking Transparency authorization on iOS.
        /// </summary>
        public bool RequestTrackingAuthorizationOnInit => _requestTrackingAuthorizationOnInit;

        /// <summary>
        /// Maximum time startup waits for App Tracking Transparency status, in milliseconds.
        /// </summary>
        public int TrackingAuthorizationStatusTimeoutMs => _trackingAuthorizationStatusTimeoutMs;

        /// <summary>
        /// Enables automatic scene-based page tracking.
        /// </summary>
        public bool AutomaticSceneTracking => _automaticSceneTracking;

        /// <summary>
        /// Captures the initial deep-link URL if one exists.
        /// </summary>
        public bool CaptureInitialUrl => _captureInitialUrl;

        /// <summary>
        /// Keeps the configured runtime alive across scene changes.
        /// </summary>
        public bool PersistConfiguredInstanceAcrossSceneLoads =>
            _persistConfiguredInstanceAcrossSceneLoads;

        /// <summary>
        /// Creates a runtime configuration object from this asset.
        /// </summary>
        public AttriaxConfig CreateRuntimeConfig()
        {
            return new AttriaxConfig
            {
                ProjectToken = _projectToken,
                ApiBaseUrl = string.IsNullOrWhiteSpace(_apiBaseUrl) ? null : _apiBaseUrl,
                AppVersion = string.IsNullOrWhiteSpace(_appVersion) ? null : _appVersion,
                AppBuildNumber = string.IsNullOrWhiteSpace(_appBuildNumber) ? null : _appBuildNumber,
                AppPackageName = string.IsNullOrWhiteSpace(_appPackageName) ? null : _appPackageName,
                GdprEnabled = _gdprEnabled,
                EnableDebugLogs = _enableDebugLogs,
                CollectAdvertisingId = _collectAdvertisingId,
                AutomaticCrashReportingEnabled = _automaticCrashReportingEnabled,
                RequestTrackingAuthorizationOnInit = _requestTrackingAuthorizationOnInit,
                TrackingAuthorizationStatusTimeoutMs = _trackingAuthorizationStatusTimeoutMs,
                AutomaticSceneTracking = _automaticSceneTracking,
            };
        }

        /// <summary>
        /// Creates initialization options for the configured runtime.
        /// </summary>
        public AttriaxInitOptions CreateInitOptions()
        {
            return new AttriaxInitOptions
            {
                CaptureInitialUrl = _captureInitialUrl,
            };
        }
    }
}