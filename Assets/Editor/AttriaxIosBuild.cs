#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Batchmode iOS Xcode-project generation for the native-binding build check.
    /// Generating the player IL2CPP-converts every script — including
    /// Engine.AttriaxIosEnginePlatform (#if UNITY_IOS &amp;&amp; !UNITY_EDITOR) — so a
    /// green build proves the iOS C-ABI binding compiles, and the
    /// AttriaxIosBuildPostprocessor runs against the generated project. Invoke with:
    ///
    ///   Unity -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -buildTarget iOS -executeMethod Attriax.Unity.Editor.AttriaxIosBuild.Build
    /// </summary>
    public static class AttriaxIosBuild
    {
        private const string ScenePath = "Assets/AttriaxSmoke.unity";
        private const string BundleId = "com.attriax.unitysmoke";
        private const string OutputDir = "Build/iOS";

        public static void Build()
        {
            EnsureScene();
            ConfigurePlayerSettings();

            var outPath = Path.GetFullPath(OutputDir);
            Directory.CreateDirectory(Directory.GetParent(outPath)!.FullName);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outPath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None,
            };

            Debug.Log($"[AttriaxIosBuild] Generating Xcode project -> {outPath}");
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[AttriaxIosBuild] Result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} output={s.outputPath}");
            if (s.result != BuildResult.Succeeded)
            {
                throw new Exception($"[AttriaxIosBuild] iOS build failed: {s.result} ({s.totalErrors} errors)");
            }

            Debug.Log("[AttriaxIosBuild] iOS Xcode project generation SUCCEEDED.");
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("AttriaxSmoke");
            go.AddComponent<AttriaxSmoke>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.productName = "AttriaxSmoke";
            PlayerSettings.companyName = "Attriax";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "14.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        }
    }
}
