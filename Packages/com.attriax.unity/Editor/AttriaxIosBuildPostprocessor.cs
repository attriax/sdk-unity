#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Wires the Attriax KMP C-ABI static library (<c>AttriaxCoreCApi.xcframework</c> →
    /// <c>libattriax_core.a</c>) into the generated iOS Xcode project so the Unity iOS
    /// binding (<c>AttriaxIosEnginePlatform</c>) links + resolves.
    /// </summary>
    /// <remarks>
    /// Unity already adds the imported xcframework to "Link Binary With Libraries", so
    /// this pass only adds (1) the Apple system frameworks the static KMP core
    /// references — mirroring the Flutter iOS plugin's podspec — and (2) <c>-u</c> linker
    /// flags that keep the flat <c>@CName</c> C entry points from being dead-stripped, so
    /// the IL2CPP <c>[DllImport("__Internal")]</c> P/Invokes resolve them.
    /// </remarks>
    internal static class AttriaxIosBuildPostprocessor
    {
        // The system frameworks the static KMP core references (ATT/IDFA, ASA via
        // AdServices, App Attest via DeviceCheck, SKAN via StoreKit, the WKWebView UA
        // probe via WebKit, networking via Network/SystemConfiguration, and the CSPRNG
        // via Security). Matches attriax_flutter_ios.podspec.
        private static readonly string[] RequiredFrameworks =
        {
            "AdSupport",
            "AppTrackingTransparency",
            "AdServices",
            "DeviceCheck",
            "StoreKit",
            "WebKit",
            "SafariServices",
            "Network",
            "SystemConfiguration",
            "Security",
        };

        // The flat C-ABI entry points that must survive dead-stripping so IL2CPP's
        // __Internal P/Invokes bind to them.
        private static readonly string[] CAbiSymbols =
        {
            "_attriax_create",
            "_attriax_dispatch",
            "_attriax_register_event_callback",
            "_attriax_free_string",
            "_attriax_destroy",
        };

        [PostProcessBuild(1000)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            // Apply to both the UnityFramework target (which links the plugins) and the
            // main app target, so the frameworks + keep-symbols flags are present wherever
            // the static lib and IL2CPP's generated references end up.
            var targets = new[]
            {
                project.GetUnityFrameworkTargetGuid(),
                project.GetUnityMainTargetGuid(),
            };

            foreach (var targetGuid in targets)
            {
                foreach (var framework in RequiredFrameworks)
                {
                    project.AddFrameworkToProject(targetGuid, framework + ".framework", weak: false);
                }

                foreach (var symbol in CAbiSymbols)
                {
                    project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-u " + symbol);
                }
            }

            project.WriteToFile(projectPath);
        }
    }
}
#endif
