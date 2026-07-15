// Guarded like AttriaxIosBuildPostprocessor's `#if UNITY_IOS`: `UnityEditor.Android` only
// exists when the Android build-support module is installed, and this callback only ever
// runs for an Android build anyway.
#if UNITY_ANDROID
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Forces <c>android.builtInKotlin=true</c> in the generated Gradle project's
    /// <c>gradle.properties</c> on Unity exports that use AGP 9 or newer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>AttriaxUnity.androidlib/build.gradle</c> feature-detects Kotlin support: if AGP
    /// already registers the <c>kotlin</c> extension it uses AGP's built-in Kotlin,
    /// otherwise it applies the Kotlin Gradle Plugin (KGP). That detection is correct, but
    /// Unity's generated <c>gradle.properties</c> ships <c>android.builtInKotlin=false</c>
    /// alongside <c>unity.agpVersion=9.x</c> — explicitly disabling AGP 9's built-in
    /// Kotlin. No <c>kotlin</c> extension is then registered, the detection falls through
    /// to the KGP branch, and KGP is incompatible with AGP 9 at every version, so Gradle
    /// configuration dies with:
    /// </para>
    /// <code>
    /// Failed to apply plugin 'org.jetbrains.kotlin.android'.
    ///   &gt; class ...LibraryExtensionImpl$AgpDecorated_Decorated cannot be cast to
    ///     class com.android.build.gradle.BaseExtension
    /// </code>
    /// <para>
    /// Re-enabling built-in Kotlin lets the feature-detect take its working branch and the
    /// androidlib's Kotlin compiles. AGP 8 exports (Unity 6000.2 and earlier Unity 6) have
    /// no built-in Kotlin at all — they omit <c>android.builtInKotlin</c> entirely and
    /// genuinely need the KGP branch — so this pass is gated on the exported AGP major
    /// version and no-ops there.
    /// </para>
    /// </remarks>
    public sealed class AttriaxAndroidBuiltInKotlinPatch : IPostGenerateGradleAndroidProject
    {
        private const string AgpVersionKey = "unity.agpVersion";
        private const string BuiltInKotlinKey = "android.builtInKotlin";
        private const int MinAgpMajorRequiringPatch = 9;
        private const string LogPrefix = "[AttriaxAndroidBuiltInKotlinPatch]";

        /// <summary>
        /// Independent of every other post-generate callback (it only touches
        /// <c>gradle.properties</c>), so the default ordering is fine.
        /// </summary>
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            try
            {
                // `path` is the unityLibrary module; the gradle root (which owns
                // gradle.properties) is its parent.
                var root = Directory.GetParent(path)?.FullName ?? path;
                var propertiesPath = Path.Combine(root, "gradle.properties");

                if (!File.Exists(propertiesPath))
                {
                    Debug.LogWarning(
                        $"{LogPrefix} No gradle.properties at {propertiesPath}; skipping. " +
                        "If this is an AGP 9 export, the Attriax androidlib may fail to configure.");
                    return;
                }

                var lines = new List<string>(File.ReadAllLines(propertiesPath));

                var agpVersion = FindValue(lines, AgpVersionKey);
                if (!TryGetMajorVersion(agpVersion, out var agpMajor))
                {
                    // Unity 6 exports all write `unity.agpVersion`; if some export ever omits
                    // it, assume the pre-AGP-9 world and let the androidlib apply KGP.
                    Debug.Log(
                        $"{LogPrefix} No parsable {AgpVersionKey} (found '{agpVersion ?? "<absent>"}'); " +
                        "assuming AGP 8 or earlier and leaving gradle.properties untouched.");
                    return;
                }

                if (agpMajor < MinAgpMajorRequiringPatch)
                {
                    Debug.Log(
                        $"{LogPrefix} AGP {agpVersion} (major {agpMajor}) has no built-in Kotlin; " +
                        "the androidlib applies KGP itself. Leaving gradle.properties untouched.");
                    return;
                }

                if (ApplyBuiltInKotlin(lines, out var previous))
                {
                    File.WriteAllLines(propertiesPath, lines);
                    Debug.Log(
                        $"{LogPrefix} AGP {agpVersion}: set {BuiltInKotlinKey}=true " +
                        $"(was {previous ?? "<absent>"}) in {propertiesPath} so the Attriax androidlib " +
                        "uses AGP's built-in Kotlin instead of the AGP 9-incompatible KGP.");
                }
                else
                {
                    Debug.Log(
                        $"{LogPrefix} AGP {agpVersion}: {BuiltInKotlinKey} is already true; nothing to do.");
                }
            }
            catch (Exception e)
            {
                // Never break a customer's build over this: warn loudly and let Gradle run.
                Debug.LogWarning(
                    $"{LogPrefix} Failed to patch gradle.properties ({e.GetType().Name}: {e.Message}). " +
                    "If this is an AGP 9 export, set `android.builtInKotlin=true` in your Gradle " +
                    "properties template to build the Attriax androidlib.");
            }
        }

        /// <summary>
        /// Sets <c>android.builtInKotlin=true</c>, replacing an existing entry (whatever its
        /// value) or appending one when absent. Returns false when it was already true.
        /// </summary>
        private static bool ApplyBuiltInKotlin(List<string> lines, out string? previousValue)
        {
            previousValue = null;

            for (var i = 0; i < lines.Count; i++)
            {
                if (!TryParseKey(lines[i], out var key, out var value) || key != BuiltInKotlinKey)
                {
                    continue;
                }

                previousValue = value;
                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                lines[i] = $"{BuiltInKotlinKey}=true";
                return true;
            }

            lines.Add($"{BuiltInKotlinKey}=true");
            return true;
        }

        private static string? FindValue(IEnumerable<string> lines, string wantedKey)
        {
            foreach (var line in lines)
            {
                if (TryParseKey(line, out var key, out var value) && key == wantedKey)
                {
                    return value;
                }
            }

            return null;
        }

        private static bool TryParseKey(string line, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal)
                                    || trimmed.StartsWith("!", StringComparison.Ordinal))
            {
                return false;
            }

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
            {
                return false;
            }

            key = trimmed.Substring(0, separator).Trim();
            value = trimmed.Substring(separator + 1).Trim();
            return key.Length > 0;
        }

        private static bool TryGetMajorVersion(string? version, out int major)
        {
            major = 0;
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            var dot = version!.IndexOf('.');
            var head = dot < 0 ? version : version.Substring(0, dot);
            return int.TryParse(head.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out major);
        }
    }
}
#endif
