#nullable enable
using UnityEngine;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Chooses the <see cref="IAttriaxEngine"/> implementation backing a public
    /// <c>Attriax</c> instance. This is the single seam through which the SDK
    /// binds to a per-platform native engine (see <c>NATIVE_ENGINE_REWRAP.md</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The managed C# engine has been removed: every supported
    /// <see cref="RuntimePlatform"/> — Editor, desktop, Android, iOS, and WebGL —
    /// is served by a native <c>Engine.IAttriaxEnginePlatform</c> binding, bridged
    /// onto <see cref="IAttriaxEngine"/> by <c>Engine.AttriaxEnginePlatformAdapter</c>.
    /// </para>
    /// <para>
    /// <see cref="TryCreateNativeEngine"/> maps each platform to its binding. A
    /// platform with no native branch is unsupported and
    /// <see cref="Create"/> throws <see cref="System.NotSupportedException"/> rather
    /// than falling back to a managed engine.
    /// </para>
    /// </remarks>
    internal static class AttriaxEngineSelector
    {
        /// <summary>
        /// Creates the native engine backing an <c>Attriax</c> instance for the
        /// current runtime platform. Throws
        /// <see cref="System.NotSupportedException"/> when the platform has no
        /// native engine binding.
        /// </summary>
        public static IAttriaxEngine Create(AttriaxConfig config)
        {
            IAttriaxEngine? native = TryCreateNativeEngine(config);
            if (native == null)
            {
                throw new System.NotSupportedException(
                    $"Attriax has no native engine for {Application.platform}.");
            }

            return native;
        }

        /// <summary>
        /// Per-platform native-engine hook. Returns the native
        /// <see cref="IAttriaxEngine"/> for the current platform, or <c>null</c>
        /// when the platform is unsupported.
        /// </summary>
        private static IAttriaxEngine? TryCreateNativeEngine(AttriaxConfig config)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
#if UNITY_ANDROID && !UNITY_EDITOR
                    // Phase 2: drive the KMP core-android AAR through
                    // Engine.AttriaxAndroidEnginePlatform, bridged onto IAttriaxEngine
                    // by the generic Engine.AttriaxEnginePlatformAdapter. Compiled only
                    // into Android player builds — the Editor cannot open on this
                    // project (PackageManager forbidden-folder crash), so the binding
                    // stays code-complete + adapter-unit-verified here, with on-device
                    // live verification pending.
                    return new Engine.AttriaxEnginePlatformAdapter(
                        config,
                        new Engine.AttriaxAndroidEnginePlatform());
#else
                    return null;
#endif

                case RuntimePlatform.IPhonePlayer:
#if UNITY_IOS && !UNITY_EDITOR
                    // U-5b: drive the KMP core through its flat C-ABI static library
                    // (libattriax_core.a, statically linked into the IL2CPP app) via
                    // Engine.AttriaxIosEnginePlatform's [DllImport("__Internal")] P/Invokes,
                    // bridged onto IAttriaxEngine by the generic
                    // Engine.AttriaxEnginePlatformAdapter. Compiled only into iOS player
                    // builds — __Internal resolves only in a real device/simulator build, so
                    // the binding stays code-complete here with on-device live verification
                    // pending.
                    return new Engine.AttriaxEnginePlatformAdapter(
                        config,
                        new Engine.AttriaxIosEnginePlatform());
#else
                    return null;
#endif

                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX))
                    // Phase 3: drive the KMP core through its C-ABI shared library
                    // (attriax_core.dll / libattriax_core.so) via
                    // Engine.AttriaxDesktopEnginePlatform, bridged onto IAttriaxEngine by
                    // the generic Engine.AttriaxEnginePlatformAdapter. The Editor loads the
                    // same shared lib as the standalone player, so play mode exercises the
                    // real engine.
                    return new Engine.AttriaxEnginePlatformAdapter(
                        config,
                        new Engine.AttriaxDesktopEnginePlatform());
#else
                    return null;
#endif

                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
                    // U-5a: drive the KMP core through its macOS C-ABI dylib
                    // (libattriax_core.dylib) via the SAME Engine.AttriaxDesktopEnginePlatform
                    // dlopen binding the Windows/Linux desktop uses, bridged onto
                    // IAttriaxEngine by the generic Engine.AttriaxEnginePlatformAdapter. The
                    // Editor loads the same universal dylib as the standalone player, so play
                    // mode exercises the real engine.
                    return new Engine.AttriaxEnginePlatformAdapter(
                        config,
                        new Engine.AttriaxDesktopEnginePlatform());
#else
                    return null;
#endif

                case RuntimePlatform.WebGLPlayer:
#if UNITY_WEBGL && !UNITY_EDITOR
                    // Phase 4: drive the sdk-js (@attriax/js) engine through the
                    // Engine.AttriaxWebGLEnginePlatform .jslib bridge, bridged onto
                    // IAttriaxEngine by the generic Engine.AttriaxEnginePlatformAdapter.
                    // Compiled only into WebGL player builds — the Editor cannot open on
                    // this project (PackageManager forbidden-folder crash) and __Internal
                    // P/Invoke resolves only in a real WebGL build, so the binding stays
                    // code-complete + adapter-unit-verified here, with in-browser live
                    // verification pending.
                    return new Engine.AttriaxEnginePlatformAdapter(
                        config,
                        new Engine.AttriaxWebGLEnginePlatform());
#else
                    return null;
#endif

                default:
                    return null;
            }
        }
    }
}
