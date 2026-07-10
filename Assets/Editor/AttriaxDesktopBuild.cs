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
    /// Batchmode Windows-standalone build entry point for the desktop native-engine
    /// smoke test. Reuses the shared smoke scene (<c>Assets/AttriaxSmoke.unity</c> +
    /// <see cref="AttriaxSmoke"/>), configures the native-core plugin importers, builds
    /// a Win64 player, and copies the C-ABI DLL next to the executable so the dynamic
    /// loader resolves it deterministically. Invoke with:
    ///
    ///   Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -buildTarget Win64 -executeMethod Attriax.Unity.Editor.AttriaxDesktopBuild.Build
    ///
    /// A Win64 standalone build compiles the Runtime asmdef with
    /// <c>UNITY_STANDALONE_WIN &amp;&amp; !UNITY_EDITOR</c>, so the produced player runs the
    /// C-ABI engine (<c>AttriaxDesktopEnginePlatform</c>), not the managed C# engine.
    /// Desktop reaches the dev API directly at <c>http://localhost:33000</c> (no adb).
    /// </summary>
    public static class AttriaxDesktopBuild
    {
        private const string ScenePath = "Assets/AttriaxSmoke.unity";
        private const string ProductName = "AttriaxSmokeDesktop";
        private const string ExeRelativePath = "Build/Desktop/AttriaxSmokeDesktop.exe";

        private const string WindowsLibAsset =
            "Packages/com.attriax.unity/Runtime/Plugins/x86_64/Windows/attriax_core.dll";
        private const string LinuxLibAsset =
            "Packages/com.attriax.unity/Runtime/Plugins/x86_64/Linux/libattriax_core.so";

        public static void Build()
        {
            ConfigurePluginImporters();
            EnsureScene();
            ConfigurePlayerSettings();

            var exePath = Path.GetFullPath(ExeRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(exePath)!);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development,
            };

            Debug.Log($"[AttriaxDesktopBuild] Building Win64 player -> {exePath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log(
                $"[AttriaxDesktopBuild] Result={summary.result} " +
                $"errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"size={summary.totalSize} bytes output={summary.outputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    $"[AttriaxDesktopBuild] Win64 build failed: {summary.result} " +
                    $"({summary.totalErrors} errors)");
            }

            CopyNativeCoreNextToPlayer(exePath);
            Debug.Log("[AttriaxDesktopBuild] Win64 build SUCCEEDED.");
        }

        private static void ConfigurePluginImporters()
        {
            ConfigureNativeImporter(WindowsLibAsset, BuildTarget.StandaloneWindows64);
            ConfigureNativeImporter(LinuxLibAsset, BuildTarget.StandaloneLinux64);
        }

        private static void ConfigureNativeImporter(string assetPath, BuildTarget target)
        {
            if (AssetImporter.GetAtPath(assetPath) is not PluginImporter importer)
            {
                Debug.LogWarning($"[AttriaxDesktopBuild] Plugin importer not found for {assetPath} (skipping).");
                return;
            }

            importer.SetCompatibleWithAnyPlatform(false);
            // The Editor loads the lib via AttriaxDesktopEnginePlatform's own path-probe
            // (LoadLibraryEx / dlopen), so leave the Editor plugin slot DISABLED — that
            // way Unity does not auto-load and lock the file (the dynamic-load rationale).
            importer.SetCompatibleWithEditor(false);
            // Exactly one standalone target owns each OS-specific binary; disable the rest
            // so a cross-OS build never bundles the wrong native file.
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, target == BuildTarget.StandaloneWindows64);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, target == BuildTarget.StandaloneLinux64);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
            importer.SetPlatformData(target, "CPU", "x86_64");
            importer.SaveAndReimport();
            Debug.Log($"[AttriaxDesktopBuild] Configured native importer {assetPath} for {target}.");
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log($"[AttriaxDesktopBuild] Reusing existing scene {ScenePath}");
                return;
            }

            Debug.Log($"[AttriaxDesktopBuild] Creating scene {ScenePath}");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("AttriaxSmoke");
            go.AddComponent<AttriaxSmoke>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = "Attriax";
            // Mono standalone: no IL2CPP C++ toolchain needed for the desktop smoke.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        }

        private static void CopyNativeCoreNextToPlayer(string exePath)
        {
            var srcPath = Path.GetFullPath(WindowsLibAsset);
            if (!File.Exists(srcPath))
            {
                Debug.LogWarning($"[AttriaxDesktopBuild] Native core not found at {srcPath}; skipping copy.");
                return;
            }

            var exeDir = Path.GetDirectoryName(exePath)!;
            var dataDir = Path.Combine(exeDir, ProductName + "_Data");
            var pluginDir = Path.Combine(dataDir, "Plugins", "x86_64");
            Directory.CreateDirectory(pluginDir);

            var fileName = Path.GetFileName(srcPath);
            File.Copy(srcPath, Path.Combine(pluginDir, fileName), overwrite: true);
            File.Copy(srcPath, Path.Combine(exeDir, fileName), overwrite: true);
            Debug.Log($"[AttriaxDesktopBuild] Copied {fileName} into player Plugins + exe dir.");
        }
    }
}
