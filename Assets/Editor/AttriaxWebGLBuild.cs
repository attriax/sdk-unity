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
    /// Batchmode WebGL build entry point for the browser smoke test. Reuses the
    /// smoke scene (creating it if missing) so the <see cref="AttriaxSmoke"/>
    /// MonoBehaviour runs on load, switches the active build target to WebGL, and
    /// builds a player that a plain static file server can host (compression
    /// disabled + decompression fallback, so no special Content-Encoding headers
    /// are required). Invoke with:
    ///
    ///   Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -executeMethod Attriax.Unity.Editor.AttriaxWebGLBuild.Build
    ///
    /// The smoke scene targets <c>http://localhost:33000</c> with the dev token and
    /// self-reports on an OnGUI readout.
    /// </summary>
    public static class AttriaxWebGLBuild
    {
        private const string ScenePath = "Assets/AttriaxSmoke.unity";
        private const string OutputRelativePath = "Build/WebGL";

        public static void Build()
        {
            SwitchToWebGL();
            EnsureScene();
            ConfigurePlayerSettings();

            var outputPath = Path.GetFullPath(OutputRelativePath);
            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            Debug.Log($"[AttriaxWebGLBuild] Building WebGL -> {outputPath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            Debug.Log(
                $"[AttriaxWebGLBuild] Result={summary.result} " +
                $"errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"size={summary.totalSize} bytes output={summary.outputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[AttriaxWebGLBuild] step='{step.name}' {msg.type}: {msg.content}");
                        }
                    }
                }

                throw new Exception(
                    $"[AttriaxWebGLBuild] WebGL build failed: {summary.result} " +
                    $"({summary.totalErrors} errors)");
            }

            Debug.Log("[AttriaxWebGLBuild] WebGL build SUCCEEDED.");
        }

        private static void SwitchToWebGL()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.Log("[AttriaxWebGLBuild] Switching active build target to WebGL");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log($"[AttriaxWebGLBuild] Reusing existing scene {ScenePath}");
                return;
            }

            Debug.Log($"[AttriaxWebGLBuild] Creating scene {ScenePath}");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("AttriaxSmoke");
            go.AddComponent<AttriaxSmoke>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "AttriaxSmoke";
            PlayerSettings.companyName = "Attriax";

            // Serve the build from a plain static file server without requiring
            // Content-Encoding headers: disable compression and enable the JS
            // decompression fallback.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
        }
    }
}
