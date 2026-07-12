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
    /// Unity already adds the imported xcframework to the UnityFramework target's "Link
    /// Binary With Libraries" (where IL2CPP's <c>libGameAssembly</c> references the
    /// <c>attriax_*</c> symbols), so the static lib is linked and the C-ABI symbols
    /// resolve naturally — this pass only adds the Apple system frameworks the static KMP
    /// core references, mirroring the Flutter iOS plugin's podspec. (No <c>-u</c>
    /// keep-symbol flags: IL2CPP already references the entry points, so they are not
    /// dead-stripped, and forcing them on the main app target — which does not link the
    /// static lib — makes the app link fail with undefined <c>attriax_*</c>.)
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

            // The static lib + IL2CPP's libGameAssembly (which references the attriax_*
            // symbols) both live in the UnityFramework target, so the system frameworks
            // the KMP core links against are added there.
            var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            foreach (var framework in RequiredFrameworks)
            {
                project.AddFrameworkToProject(frameworkTargetGuid, framework + ".framework", weak: false);
            }

            project.WriteToFile(projectPath);
        }
    }
}
#endif
