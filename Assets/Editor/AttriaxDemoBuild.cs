#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Batchmode build for the INTERACTIVE demo (<see cref="AttriaxDemo"/>) — a real,
    /// tappable app used to exercise the SDK by hand on a device.
    ///
    /// Distinct from <see cref="AttriaxIosBuild"/>, which builds the headless smoke used
    /// as the compile/link gate: that scene is deliberately empty (no camera → a black
    /// screen) because it only ever needed to prove the binding links and logs. This one
    /// generates a camera + the demo component so there is something to look at.
    ///
    ///   Unity -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -buildTarget iOS -executeMethod Attriax.Unity.Editor.AttriaxDemoBuild.Build
    /// </summary>
    public static class AttriaxDemoBuild
    {
        private const string ScenePath = "Assets/AttriaxDemo.unity";
        private const string BundleId = "com.attriax.unitydemo";
        private const string OutputDir = "Build/iOSDemo";

        /// Apple REQUIRES this key for ATT: without it the prompt can never be shown
        /// (and a request may be silently refused). The Flutter example declares the same
        /// key in its Info.plist — the SDK deliberately does NOT inject it, since the
        /// copy is App-Store-reviewed and belongs to the app.
        private const string AttUsageDescription =
            "We use your data to measure advertising performance and improve your experience.";

        public static void Build()
        {
            EnsureScene();
            ConfigurePlayerSettings();

            var outPath = Path.GetFullPath(OutputDir);
            Directory.CreateDirectory(Directory.GetParent(outPath)!.FullName);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outPath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None,
            });

            var s = report.summary;
            Debug.Log($"[AttriaxDemoBuild] Result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} output={s.outputPath}");
            if (s.result != BuildResult.Succeeded)
            {
                throw new Exception($"[AttriaxDemoBuild] iOS demo build failed: {s.result} ({s.totalErrors} errors)");
            }

#if UNITY_IOS
            AddAttUsageDescription(outPath);
#endif
            Debug.Log("[AttriaxDemoBuild] iOS demo Xcode project generation SUCCEEDED.");
        }

        private static void EnsureScene()
        {
            // Always regenerate: the scene is a build artifact of this script, and a
            // camera-less leftover would render black.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.11f, 0.16f); // dark slate
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";

            var demo = new GameObject("AttriaxDemo");
            demo.AddComponent<AttriaxDemo>();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.productName = "Attriax Demo";
            PlayerSettings.companyName = "Attriax";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "14.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }

#if UNITY_IOS
        private static void AddAttUsageDescription(string pathToBuiltProject)
        {
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning("[AttriaxDemoBuild] Info.plist not found; skipping ATT usage description.");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetString("NSUserTrackingUsageDescription", AttUsageDescription);
            plist.WriteToFile(plistPath);
            Debug.Log("[AttriaxDemoBuild] Added NSUserTrackingUsageDescription to Info.plist.");
        }
#endif
    }
}
