#nullable enable
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace Attriax.Unity.Editor
{
    internal sealed class AttriaxConfigurationWindow : EditorWindow
    {
        private const string SettingsAssetPath = "Assets/Resources/Attriax/AttriaxSettings.asset";

        private AttriaxProjectSettings? _settings;
        private SerializedObject? _serializedSettings;
        private string? _validationMessage;
        private MessageType _validationMessageType = MessageType.Info;
        private bool _isValidating;

        [MenuItem("Tools/Attriax/Configuration")]
        private static void OpenWindow()
        {
            var window = GetWindow<AttriaxConfigurationWindow>("Attriax");
            window.minSize = new Vector2(460f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawUsageGuidance();

            if (_settings == null)
            {
                DrawMissingSettingsState();
                return;
            }

            _serializedSettings ??= new SerializedObject(_settings);
            _serializedSettings.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
            DrawPropertyField("_projectToken", "Project Token");
            DrawPropertyField("_apiBaseUrl", "API Base URL");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            DrawPropertyField(
                "_autoInitializeOnLaunch",
                "Initialize configured instance automatically on startup");
            DrawPropertyField(
                "_persistConfiguredInstanceAcrossSceneLoads",
                "Persist configured instance across scene loads");
            DrawPropertyField("_captureInitialUrl", "Capture initial deep-link URL");
            DrawPropertyField("_collectAdvertisingId", "Collect advertising identifiers when available");
            DrawPropertyField(
                "_requestTrackingAuthorizationOnInit",
                "Request tracking authorization during initialization");
            DrawPropertyField(
                "_trackingAuthorizationStatusTimeoutMs",
                "Tracking authorization startup timeout (ms)");
            DrawPropertyField("_automaticSceneTracking", "Track scene changes automatically");
            DrawPropertyField("_automaticCrashReportingEnabled", "Capture unhandled Unity exceptions automatically");
            DrawPropertyField("_enableDebugLogs", "Enable debug logs");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional Overrides", EditorStyles.boldLabel);
            DrawPropertyField("_appVersion", "App Version");
            DrawPropertyField("_appBuildNumber", "App Build Number");
            DrawPropertyField("_appPackageName", "Package / Bundle Identifier");

            if (_serializedSettings.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settings);
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save", GUILayout.Height(28f)))
                {
                    SaveSettings();
                }

                if (GUILayout.Button("Select Asset", GUILayout.Height(28f)))
                {
                    Selection.activeObject = _settings;
                    EditorGUIUtility.PingObject(_settings);
                }
            }

            using (new EditorGUI.DisabledScope(_isValidating))
            {
                var label = _isValidating ? "Checking configuration..." : "Check backend configuration";
                if (GUILayout.Button(label, GUILayout.Height(32f)))
                {
                    ValidateConfigurationAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(_validationMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(_validationMessage, _validationMessageType);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attriax Unity SDK", EditorStyles.largeLabel);
            EditorGUILayout.LabelField(
                "Configure the shared SDK asset used by Attriax.Configured and optional automatic startup initialization.",
                EditorStyles.wordWrappedLabel);
        }

        private void DrawUsageGuidance()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Recommended integration modes:\n• Configured singleton: create the Attriax settings asset here and let Attriax.InitializeConfiguredAsync() or automatic startup own the SDK lifecycle.\n• Scene component host: add AttriaxBehaviour to a bootstrap GameObject, enable Initialize On Awake, and keep Persist Across Scenes on if you want a DontDestroyOnLoad host.\n• Full manual: create a host from code with AttriaxBehaviour.CreateAndInitializeHostAsync(...) or instantiate new Attriax(config) yourself when you want full lifecycle control.",
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "Obfuscation and stripping: the Attriax.Runtime assembly now ships with Obfuscation(Exclude=true) and AlwaysLinkAssembly. If your obfuscator ignores assembly-level attributes, exclude Attriax.Runtime or at least Attriax, AttriaxBehaviour, AttriaxProjectSettings, and AttriaxConfiguredHost from renaming or stripping.",
                MessageType.Info);
        }

        private void DrawMissingSettingsState()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "No Attriax settings asset was found. Create one to enable the configured singleton instance and the startup bootstrap flow.",
                MessageType.Info);

            if (GUILayout.Button("Create Settings Asset", GUILayout.Height(32f)))
            {
                CreateSettingsAsset();
            }
        }

        private void DrawPropertyField(string propertyName, string label)
        {
            var property = _serializedSettings?.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        private void LoadSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<AttriaxProjectSettings>(SettingsAssetPath);
            _serializedSettings = _settings != null ? new SerializedObject(_settings) : null;
        }

        private void CreateSettingsAsset()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Attriax");

            _settings = CreateInstance<AttriaxProjectSettings>();
            AssetDatabase.CreateAsset(_settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _serializedSettings = new SerializedObject(_settings);
            Selection.activeObject = _settings;
            EditorGUIUtility.PingObject(_settings);
        }

        private static void EnsureFolder(string parentFolder, string childFolderName)
        {
            var folderPath = parentFolder + "/" + childFolderName;
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolder, childFolderName);
        }

        private void SaveSettings()
        {
            if (_settings == null)
            {
                return;
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private async void ValidateConfigurationAsync()
        {
            if (_settings == null)
            {
                return;
            }

            SaveSettings();
            _isValidating = true;
            _validationMessage = "Checking Attriax backend configuration...";
            _validationMessageType = MessageType.Info;
            Repaint();

            try
            {
                var result = await ValidateAgainstBackendAsync(_settings);
                _validationMessageType = result.WarningCount > 0 ? MessageType.Warning : MessageType.Info;
                _validationMessage = result.Message;
            }
            catch (Exception error)
            {
                _validationMessageType = MessageType.Error;
                _validationMessage = error.Message;
            }
            finally
            {
                _isValidating = false;
                Repaint();
            }
        }

        private static async Task<ValidationResult> ValidateAgainstBackendAsync(
            AttriaxProjectSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ProjectToken))
            {
                throw new InvalidOperationException("Project Token is required before the configuration can be checked.");
            }

            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
                "Packages/com.attriax.unity");
            var endpoint = BuildSdkBaseUrl(settings.ApiBaseUrl) + "/v1/unity-editor/validate";
            var body = new JObject
            {
                ["projectToken"] = settings.ProjectToken,
                ["unityVersion"] = Application.unityVersion,
                ["packageVersion"] = packageInfo?.version ?? string.Empty,
                ["editorHostPlatform"] = Application.platform.ToString(),
            };

            using (var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                var json = Encoding.UTF8.GetBytes(body.ToString());
                request.uploadHandler = new UploadHandlerRaw(json);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 15;

                await SendRequestAsync(request);

                if (request.responseCode < 200 || request.responseCode >= 300)
                {
                    throw new InvalidOperationException(
                        "Backend validation failed: " + request.downloadHandler.text);
                }

                var payload = ParseEnvelope(request.downloadHandler.text);
                var warnings = payload["warnings"] as JArray;
                var warningMessages = warnings != null
                    ? warnings.Values<string>().Where(value => !string.IsNullOrWhiteSpace(value)).ToArray()
                    : Array.Empty<string>();
                var app = payload["app"] as JObject;

                var builder = new StringBuilder();
                builder.AppendLine("Project token accepted by Attriax.");
                if (!string.IsNullOrWhiteSpace(app?.Value<string>("publicHost")))
                {
                    builder.AppendLine("Public host: " + app.Value<string>("publicHost"));
                }

                if (warningMessages.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine("Warnings:");
                    foreach (var warning in warningMessages)
                    {
                        builder.AppendLine("- " + warning);
                    }
                }

                return new ValidationResult(builder.ToString().Trim(), warningMessages.Length);
            }
        }

        private static string BuildSdkBaseUrl(string apiBaseUrl)
        {
            var trimmed = string.IsNullOrWhiteSpace(apiBaseUrl)
                ? "https://api.attriax.com"
                : apiBaseUrl.Trim().TrimEnd('/');

            if (trimmed.EndsWith("/api/sdk", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (trimmed.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed + "/sdk";
            }

            return trimmed + "/api/sdk";
        }

        private static async Task SendRequestAsync(UnityWebRequest request)
        {
            var completionSource = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => completionSource.TrySetResult(true);
            await completionSource.Task;
        }

        private static JObject ParseEnvelope(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new JObject();
            }

            var parsed = JObject.Parse(payload);
            return parsed["data"] as JObject ?? parsed;
        }

        private readonly struct ValidationResult
        {
            internal ValidationResult(string message, int warningCount)
            {
                Message = message;
                WarningCount = warningCount;
            }

            internal string Message { get; }

            internal int WarningCount { get; }
        }
    }
}