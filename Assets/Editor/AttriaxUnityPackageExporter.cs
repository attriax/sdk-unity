#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Attriax.Unity.Editor
{
    [InitializeOnLoad]
    public static class AttriaxUnityBatchExportBootstrap
    {
        static AttriaxUnityBatchExportBootstrap()
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var index = 0; index < arguments.Length; index += 1)
            {
                if (!string.Equals(arguments[index], "-attriaxExportPackage", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                EditorApplication.delayCall += RunBatchExport;
                break;
            }
        }

        private static void RunBatchExport()
        {
            AttriaxUnityPackageExporter.ResetBatchTrace();
            AttriaxUnityPackageExporter.WriteBatchTrace("Starting batch export.");

            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var outputPath = Path.Combine(projectRoot, "dist", "Attriax.Unity.unitypackage");
                AttriaxUnityPackageExporter.Export(outputPath);
                AttriaxUnityPackageExporter.WriteBatchTrace("Batch export completed successfully.");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                AttriaxUnityPackageExporter.WriteBatchTrace(error.ToString());
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }
    }

    /// <summary>
    /// Exports the embedded package as a traditional unitypackage artifact.
    /// </summary>
    public static class AttriaxUnityPackageExporter
    {
        private const string PackageRoot = "Packages/com.attriax.unity";
        private static readonly string[] PackageExportPaths =
        {
            PackageRoot + "/Runtime",
            PackageRoot + "/README.md",
            PackageRoot + "/CHANGELOG.md",
            PackageRoot + "/Samples~",
        };

        internal static void ResetBatchTrace()
        {
            var tracePath = GetBatchTracePath();
            if (File.Exists(tracePath))
            {
                File.Delete(tracePath);
            }
        }

        internal static void WriteBatchTrace(string message)
        {
            var tracePath = GetBatchTracePath();
            Directory.CreateDirectory(Path.GetDirectoryName(tracePath) ?? ".");
            File.AppendAllText(
                tracePath,
                $"[{DateTimeOffset.UtcNow:O}] {message}{Environment.NewLine}");
        }

        /// <summary>
        /// Batch-friendly export entry point used by CI and the PowerShell wrapper.
        /// </summary>
        public static void ExportFromCommandLine()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputPath = Path.Combine(projectRoot, "dist", "Attriax.Unity.unitypackage");
            Export(outputPath);
        }

        [MenuItem("Attriax/Export unitypackage")]
        public static void ExportFromMenu()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputPath = EditorUtility.SaveFilePanel(
                "Export Attriax Unity Package",
                Path.Combine(projectRoot, "dist"),
                "Attriax.Unity",
                "unitypackage");

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return;
            }

            Export(outputPath);
        }

        public static void Export(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("Output path is required.", nameof(outputPath));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
            WriteBatchTrace("Running AssetDatabase.ExportPackage.");
            AssetDatabase.ExportPackage(
                PackageExportPaths,
                outputPath,
                ExportPackageOptions.Recurse);

            WriteBatchTrace($"Export finished: {outputPath}");
            Debug.Log($"Attriax Unity package exported to {outputPath}");
        }

        private static string GetBatchTracePath()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, "Temp", "unity-export.trace.log");
        }
    }
}