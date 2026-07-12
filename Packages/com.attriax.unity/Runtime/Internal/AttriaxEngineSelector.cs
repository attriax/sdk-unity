#nullable enable
using UnityEngine;

namespace Attriax.Unity.Internal
{
    /// <summary>
    /// Chooses the <see cref="IAttriaxEngine"/> implementation backing a public
    /// <c>Attriax</c> instance. This is the single seam through which the SDK
    /// selects between the managed C# engine and a per-platform native engine
    /// during the native re-wrap (see <c>NATIVE_ENGINE_REWRAP.md</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Today the managed C# engine (<see cref="AttriaxRuntime"/>) is selected
    /// on EVERY <see cref="RuntimePlatform"/> — Editor, desktop, Android, iOS, and
    /// WebGL.</b> This is a pure refactor: behavior is identical to constructing
    /// <see cref="AttriaxRuntime"/> inline as before. No platform is flipped to a
    /// native engine here.
    /// </para>
    /// <para>
    /// Native engines land per platform in later phases and each requires live
    /// verification before it is switched on. The <see cref="TryCreateNativeEngine"/>
    /// hook below enumerates the target platforms with the binding that will plug
    /// in (all inactive — every branch returns <c>null</c>, so the C# engine is
    /// always chosen). To flip a platform, adapt its
    /// <c>Engine.IAttriaxEnginePlatform</c> implementation onto
    /// <see cref="IAttriaxEngine"/> (e.g. via a future
    /// <c>AttriaxEnginePlatformAdapter</c>) and return it from the matching case.
    /// </para>
    /// </remarks>
    internal static class AttriaxEngineSelector
    {
        /// <summary>
        /// Creates the engine backing an <c>Attriax</c> instance for the current
        /// runtime platform. Returns a native engine when one is active for the
        /// platform, otherwise the managed C# engine (the default + universal
        /// fallback).
        /// </summary>
        public static IAttriaxEngine Create(AttriaxConfig config)
        {
            IAttriaxEngine? native = TryCreateNativeEngine(config);
            return native ?? new AttriaxRuntime(config);
        }

        /// <summary>
        /// Per-platform native-engine hook. Every branch returns <c>null</c>
        /// today: no platform is flipped to native yet, so the caller falls back
        /// to the managed C# engine everywhere.
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
                    // FUTURE (Phase 6, Mac-gated): iOS KMP XCFramework via P/Invoke.
                    return null;

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
