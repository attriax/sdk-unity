#nullable enable
#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Attriax.Unity.Internal.Engine
{
    /// <summary>
    /// P/Invoke bindings to the KMP engine C-ABI exposed by
    /// <c>Runtime/Plugins/iOS/AttriaxUnityIOS.mm</c> (the <c>AttriaxUnityEngine_*</c>
    /// functions), which drive the KMP <c>Attriax</c> engine held in the embedded
    /// <c>AttriaxCore</c> XCFramework via the ObjC <c>AttriaxApple</c> factory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the iOS half of the Unity native binding (Phase 6). The engine handle is
    /// an opaque <see cref="IntPtr"/>; strings crossing the boundary are copied C-side
    /// and freed via <see cref="AttriaxUnity_FreeString"/>; the sync-state stream returns
    /// via a C function pointer registered as a <see cref="MonoPInvokeCallbackAttribute"/>
    /// static method (required for AOT/IL2CPP on iOS).
    /// </para>
    /// <para>
    /// ⚠️ CODE-ONLY / UNVERIFIED. The Unity Editor on this project crashes at init
    /// (PackageManager forbidden-folder issue) and there is no iOS-Unity Xcode export path
    /// on the authoring Mac, so this binding is not compiled or run here. It is written
    /// against the verified ObjC API of the shipped XCFramework (the same API the Flutter
    /// Swift plugin drives). The <c>IAttriaxEnginePlatform</c> implementation that consumes
    /// these bindings, and the remaining <c>AttriaxUnityEngine_*</c> exports for the full
    /// command surface, are completed + device-verified on the Unity/Windows box — the
    /// tracking/consent/skan/deep-link commands follow the identical pattern to the core
    /// exports declared here.
    /// </para>
    /// </remarks>
    internal static class AttriaxIosEngineNative
    {
        /// <summary>Sync-state callback marshaled from the C-ABI (a UTF-8 KMP enum name).</summary>
        internal delegate void SyncStateCallback(IntPtr stateNameUtf8);

        // ---- engine lifecycle ----

        [DllImport("__Internal")]
        internal static extern IntPtr AttriaxUnityEngine_Create(string configJson, string? userAgent);

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_Init(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_Flush(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_Reset(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_Dispose(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_Destroy(IntPtr handle);

        // ---- tracking / getters ----

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_RecordEvent(
            IntPtr handle, string name, string? eventDataJson, bool flushImmediately);

        [DllImport("__Internal")]
        internal static extern IntPtr AttriaxUnityEngine_GetDeviceId(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern bool AttriaxUnityEngine_GetIsInitialized(IntPtr handle);

        [DllImport("__Internal")]
        internal static extern IntPtr AttriaxUnityEngine_SubmitAsaToken(IntPtr handle, string token);

        // ---- synchronization event ----

        [DllImport("__Internal")]
        internal static extern void AttriaxUnityEngine_SetSyncCallback(IntPtr handle, SyncStateCallback callback);

        // ---- shared string free (declared in the signal-provider section of the .mm) ----

        [DllImport("__Internal")]
        internal static extern void AttriaxUnity_FreeString(IntPtr value);

        /// <summary>Marshal a C-owned UTF-8 string to a managed string and free it.</summary>
        internal static string? ConsumeCString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return null;
            try { return Marshal.PtrToStringUTF8(ptr); }
            finally { AttriaxUnity_FreeString(ptr); }
        }

        /// <summary>
        /// The one process-wide sync-state trampoline. IL2CPP on iOS can only pass a
        /// <c>static</c> method annotated with <see cref="MonoPInvokeCallbackAttribute"/>
        /// as a C function pointer; it forwards to the managed handler set by the platform.
        /// </summary>
        internal static Action<string>? SyncStateHandler;

        [MonoPInvokeCallback(typeof(SyncStateCallback))]
        internal static void OnSyncState(IntPtr stateNameUtf8)
        {
            var name = Marshal.PtrToStringUTF8(stateNameUtf8);
            if (name != null) SyncStateHandler?.Invoke(name);
        }
    }
}
#endif
