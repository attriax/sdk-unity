#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Batchmode Android build entry point for the device smoke test. Creates the
    /// smoke scene (if missing), configures the Android player for an installable
    /// APK, and builds it. Invoke with:
    ///
    ///   Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -buildTarget Android -executeMethod Attriax.Unity.Editor.AttriaxAndroidBuild.Build
    ///
    /// Cleartext HTTP to localhost (needed to reach the dev API over
    /// `adb reverse tcp:33000`) is enabled by <see cref="AttriaxCleartextManifestPatch"/>,
    /// which patches the generated Gradle manifests after export.
    /// </summary>
    public static class AttriaxAndroidBuild
    {
        private const string ScenePath = "Assets/AttriaxSmoke.unity";
        private const string ApplicationId = "com.attriax.unitysmoke";
        private const string ApkRelativePath = "Build/AttriaxSmoke.apk";

        public static void Build()
        {
            EnsureScene();
            ConfigurePlayerSettings();

            var apkPath = Path.GetFullPath(ApkRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(apkPath)!);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            Debug.Log($"[AttriaxAndroidBuild] Building APK -> {apkPath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log(
                $"[AttriaxAndroidBuild] Result={summary.result} " +
                $"errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"size={summary.totalSize} bytes output={summary.outputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    $"[AttriaxAndroidBuild] Android build failed: {summary.result} " +
                    $"({summary.totalErrors} errors)");
            }

            Debug.Log("[AttriaxAndroidBuild] Android build SUCCEEDED.");
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log($"[AttriaxAndroidBuild] Reusing existing scene {ScenePath}");
                return;
            }

            Debug.Log($"[AttriaxAndroidBuild] Creating scene {ScenePath}");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("AttriaxSmoke");
            go.AddComponent<AttriaxSmoke>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
            PlayerSettings.productName = "AttriaxSmoke";
            PlayerSettings.companyName = "Attriax";

            // Native KMP AAR is pure JVM (no .so); ARM64 + IL2CPP is the representative
            // Google-Play configuration and runs on the Pixel 5 test device.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // androidlib requires minSdk 21; Unity 6 supports >= 23. targetSdk 34 matches
            // the androidlib's compile/target SDK.
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;

            // Belt-and-suspenders cleartext toggle (also patched into the manifest).
            PlayerSettings.Android.startInFullscreen = true;
        }
    }

    /// <summary>
    /// Enables cleartext HTTP in every generated Gradle manifest so the smoke build can
    /// reach the dev API at <c>http://localhost:33000</c> (Android blocks cleartext by
    /// default on API 28+). Runs after Unity exports the Gradle project, before Gradle
    /// builds the APK.
    /// </summary>
    public sealed class AttriaxCleartextManifestPatch : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // `path` is the unityLibrary module; the gradle root is its parent.
            var root = Directory.GetParent(path)?.FullName ?? path;

            foreach (var manifest in Directory.GetFiles(root, "AndroidManifest.xml", SearchOption.AllDirectories))
            {
                PatchManifest(manifest);
            }
        }

        private static void PatchManifest(string manifestPath)
        {
            var xml = File.ReadAllText(manifestPath);
            var idx = xml.IndexOf("<application", StringComparison.Ordinal);
            if (idx < 0 || xml.Contains("usesCleartextTraffic"))
            {
                return;
            }

            var insertAt = idx + "<application".Length;
            xml = xml.Insert(insertAt, " android:usesCleartextTraffic=\"true\"");
            File.WriteAllText(manifestPath, xml);
            Debug.Log($"[AttriaxCleartextManifestPatch] Enabled cleartext in {manifestPath}");
        }
    }
}
