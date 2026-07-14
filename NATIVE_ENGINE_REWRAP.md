# Unity native-engine re-wrap — COMPLETE

**Branch:** `feat/native-engine-rewrap`. **Status: the migration is done.** The
Unity SDK is no longer a C# engine — it is a thin wrapper over native engines,
one binding per platform, all ultimately backed by the shared Kotlin
Multiplatform core (`sdk-kmp` → `com.attriax:core`), except WebGL which runs on
`sdk-js` (`@attriax/js`). This is the Unity analog of
`sdk-flutter/NATIVE_ENGINE_REWRAP.md`.

This document is now a historical record of the decisions + the final
architecture, not a phased plan. Read it to understand *why* the SDK is shaped
this way and what to check if a native binding breaks — not as a to-do list.

## What happened

The managed C# engine (`AttriaxRuntime` and everything reachable only from
it — the request queue/manager, event hub, session/consent/tracking/context/
deep-link/referrer/skan/app-open managers and their stores, and the engine's
own generated OpenAPI HTTP client) was **deleted** in
`refactor(unity)!: delete the managed C# engine — native-only on every target`
once every platform had a working native `IAttriaxEnginePlatform` binding.
165 `.cs` files were removed. The old engine is preserved for historical
reference / rollback on the `backup/unity-csharp-engine-2026-07-12` branch
(pushed) — it is not part of `main`/this branch going forward.

## Final architecture

```
app code / scene
  │  (public C# API — unchanged, backward compatible)
  ▼
Attriax / AttriaxBehaviour (C# facade + MonoBehaviour)   ← NO engine logic; forwards every call
  │      Tracking / Consent / Synchronization / DeepLinks / Referrer / Skan
  ▼
IAttriaxEngine → IAttriaxEnginePlatform (C#)   ← full engine command + event surface, platform-dispatched
  │            │                 │                    │              │
  ▼            ▼                 ▼                    ▼              ▼
Android      iOS              Windows/Linux         macOS          WebGL
core AAR     C-ABI static     C-ABI shared lib       C-ABI dylib    @attriax/js
(JNI via     lib (P/Invoke    (dynamic dlopen,       (dynamic       (.jslib +
AndroidJava  __Internal,      Editor + standalone)   dlopen)        .jspre bundle)
Object)      IL2CPP)
```

Engine per platform, as shipped under `Packages/com.attriax.unity/Runtime/Plugins/`:

| Platform | Native engine | Artifact | Binding | C# platform class |
|---|---|---|---|---|
| Android (device/Editor via emulator) | KMP core | `com.attriax:core` AAR (from Maven Central), pulled via the `.androidlib`'s Gradle dep | JNI via `AndroidJavaObject`/`AndroidJavaProxy` on a dedicated worker thread | `AttriaxAndroidEnginePlatform` |
| iOS | KMP core | `AttriaxCoreCApi.xcframework` (static lib, ios-arm64 + simulator) | `[DllImport("__Internal")]` P/Invoke, IL2CPP | `AttriaxIosEnginePlatform` |
| Windows / Linux standalone + Editor (host OS) | KMP core | C-ABI shared lib (`attriax_core.dll` / `libattriax_core.so`) | Dynamic `LoadLibrary`/`dlopen` (not static `[DllImport]` — see Windows Editor note below) | `AttriaxDesktopEnginePlatform` |
| macOS standalone + Editor | KMP core | `libattriax_core.dylib` (universal arm64+x86_64) | Dynamic `dlopen`, same class as Windows/Linux | `AttriaxDesktopEnginePlatform` (macOS-extended) |
| WebGL | sdk-js (`@attriax/js`) | vendored IIFE bundle (`AttriaxJsBundle.jspre`) + `AttriaxWebGL.jslib` router | `[DllImport("__Internal")]` into the `.jslib`, async trampoline callbacks | `AttriaxWebGLEnginePlatform` |

No engine logic remains in C#. `IAttriaxEnginePlatform` is the wire-shaped
native contract (decomposed primitive args, async getters, C# events)
mirroring the KMP public surface (`com/attriax/sdk/Attriax.kt`) 1:1 — the same
shape sdk-flutter's platform interface and sdk-js expose.

### Public API is unchanged

`new Attriax(config)` + `InitializeAsync`, `Attriax.Configured`,
`AttriaxBehaviour`'s inspector flow, `tracking.RecordEvent`, `consent.Gdpr.*`,
`deepLinks.*`, `synchronization.Subscribe`, `referrer.*`, `skan.*` — all
identical to the pre-rewrap surface. Existing scenes/scripts do not need to
change. The facade + `AttriaxBehaviour` hold no engine state; they keep only
Unity-glue responsibilities (lifecycle wiring, the multi-instance Editor
warning, `Configured` singleton bookkeeping).

### What's retained in C#

- **Public surface:** `Attriax.cs`, `AttriaxTracking.cs`, `AttriaxConsent.cs`,
  `AttriaxSynchronization.cs`, `AttriaxDeepLinks.cs`, `AttriaxReferrer.cs`,
  `AttriaxSkan.cs`, `AttriaxBehaviour.cs`, `AttriaxTypes.cs`,
  `AttriaxProjectSettings.cs`, `AttriaxAnalyticsKeys.cs`.
- **Shared infra the native path needs:** `Runtime/Internal/Engine/**` (the
  five platform bindings + `AttriaxEnginePlatformAdapter` + `IAttriaxEnginePlatform`
  port + JSON mappers), `AttriaxEngineSelector` (picks the platform binding;
  throws `NotSupportedException` for an unsupported platform — no C# fallback
  left), `IAttriaxEngine`, `AttriaxConfigGuard` (fail-fast config validation,
  now the sole validator), `AttriaxLifecycleDispatcher` (Unity lifecycle/
  main-thread bridge — feeds `Application.deepLinkActivated`, pause/focus,
  scene changes into the engine and marshals native→C# callbacks onto the main
  thread), `AttriaxNativeBridge` (C#-side native signal collection),
  `AttriaxConfiguredHost`/`AttriaxConfiguredRuntime` (public facade auto-init).
- **Not retained:** `AttriaxPlayerPrefs` (the old C#-static persistence layer)
  — persistence now lives entirely inside each native engine's own store.

## Verification status (be honest about this — don't overclaim)

- **Windows / Linux desktop + Editor:** live-verified against the dev API —
  the Editor loads the real native engine (not a stub), so a play session
  drives the real KMP engine with its file-backed store.
- **Android:** native binding build/link-verified (`.androidlib` Gradle build
  green against the real `com.attriax:core` AAR) with a device smoke harness
  (`Assets/AttriaxSmoke.cs`, `adb reverse`) exercising it against the dev API.
- **WebGL:** binding is contract-complete (all `IAttriaxEnginePlatform`
  members wired) and browser-build-verified via a batchmode build entrypoint
  (`Assets/Editor/AttriaxWebGLBuild.cs`) that produces a player for browser
  smoke testing against the dev API.
- **macOS / iOS: still Mac/backend-gated.** Both bindings are code-complete
  and build/link-verified (macOS: Editor batchmode smoke successfully loaded
  and initialized the dylib in one session; iOS: Unity IL2CPP build +
  `xcodebuild -sdk iphoneos` both succeeded). Neither has a confirmed
  on-device / real-app end-to-end run against the live dev API — iOS
  on-device runtime additionally pends code signing. Treat macOS/iOS as
  **not** fully live-verified until that Mac-side pass happens.

## Packaging

Unity consumes native libs by folder convention under
`Packages/com.attriax.unity/Runtime/Plugins/`: `Android/` (the AAR dependency
+ `.androidlib`), `iOS/` (`AttriaxCoreCApi.xcframework` + the signal `.mm`),
`macOS/` (`libattriax_core.dylib`), `x86_64/Windows|Linux/` (`attriax_core.dll`
/ `libattriax_core.so`), `WebGL/` (`.jslib` + the vendored `@attriax/js`
`.jspre` bundle). `.meta` import settings gate each artifact to its owning
platform (and to the Editor where the dynamic-load path needs it).

`Assets/Editor/AttriaxUnityPackageExporter.cs` exports the whole
`Packages/com.attriax.unity/Runtime` tree (which already includes `Plugins/`)
into `dist/Attriax.Unity.unitypackage` — no separate per-plugin path list is
needed; adding a new native artifact under `Runtime/Plugins/` is picked up
automatically.

Producing the native artifacts themselves is a manual, local step (no CI, per
project rule), driven from `sdk-kmp`:

1. `sdk-kmp`: the Android AAR (`com.attriax:core`) resolves from **Maven Central**
   via the `.androidlib`'s Gradle dependency — no local publish step needed for it.
   Run the `sharedLib`/`staticLib` assemble tasks for the desktop/iOS C-ABI
   artifacts (the iOS xcframework and macOS dylib require building at a Mac). For
   development against an unreleased core, `./gradlew :core:publishToMavenLocal`
   plus `mavenLocal()` in the `.androidlib` still works.
2. Copy the produced artifacts into the matching `Runtime/Plugins/<platform>/`
   folder with correct `.meta` platform-inclusion flags (see the per-platform
   build scripts under `Assets/Editor/` — `AttriaxAndroidBuild.cs`,
   `AttriaxDesktopBuild.cs`, `AttriaxIosBuild.cs`, `AttriaxWebGLBuild.cs` — for
   the current staging/build entrypoints).

## Risks / rules that remain operationally relevant

- **Backward compatibility:** the public C# API must stay identical across any
  future change to a native binding — that was the hard constraint the whole
  rewrap was built to satisfy.
- **IL2CPP AOT + P/Invoke callbacks:** on iOS/WebGL (and IL2CPP standalone)
  every native→managed callback must be a **`static`** method tagged
  `[AOT.MonoPInvokeCallback(typeof(TDelegate))]` — no instance/closure
  delegates can cross the boundary. Callback context is passed as an opaque
  handle and resolved C#-side.
- **Main-thread marshaling** (ties to `sdk-unity-debuglog-mainthread-deadlock`):
  engine→C# callbacks (`AndroidJavaProxy.Invoke`, P/Invoke callbacks, WebGL
  trampolines) arrive off the Unity main thread. They are delivered via
  `AttriaxLifecycleDispatcher.PostToMainThread` (fire-and-forget) —
  **never** the blocking `InvokeOnMainThread`+`Wait`, which can deadlock if
  the caller holds an engine lock the main thread also contends for.
- **Windows Editor DLL-lock:** on Windows, once Mono statically P/Invokes a
  native `.dll`, the Editor process holds the file locked for its whole
  session — a native-plugin engine DLL can't be rebuilt/replaced without
  restarting the Editor. `AttriaxDesktopEnginePlatform` therefore binds via
  **dynamic** `LoadLibrary`/`GetProcAddress`/`FreeLibrary` (Windows) or
  `dlopen`/`dlsym`/`dlclose` (macOS/Linux) instead of static `[DllImport]`, so
  the native lib can be unloaded on play-mode exit and rebuilt without
  restarting the Editor. Standalone IL2CPP players (iOS) still use static
  `[DllImport("__Internal")]` — the dynamic loader is an Editor/desktop-Mono
  concern.
- **Editor engine lifetime** (ties to
  `sdk-unity-anonymous-fragmentation-and-editor-amplification`): the win of
  this migration is that the Editor now runs the real engine with a
  file-backed store, so stale static memory across Enter/Exit-play-mode no
  longer amplifies one device into several dashboard users. The loaded native
  lib still has its own process-global engine state, so it is disposed on
  `EditorApplication.playModeStateChanged` → exiting play mode.
- **WebGL parity nuance:** sdk-js has two *intentional* divergences from the
  native core (terminal-retry drop; batch-identity omits `projectToken`) —
  documented in memory (`sdk-js-retry-divergence`,
  `sdk-js-batch-identity-divergence`), not bugs. Unity-WebGL inherits sdk-js
  behavior on those points, exactly like Flutter-web.
- **Reference for behavior questions:** the retired C# engine
  (`backup/unity-csharp-engine-2026-07-12`) is the historical behavior
  reference for "what did Unity used to do here"; the KMP core and sdk-js are
  the current source of truth going forward.
- **No push** until the user asks; **child-before-root** submodule discipline
  still applies for any future `sdk-kmp` change this SDK depends on.
