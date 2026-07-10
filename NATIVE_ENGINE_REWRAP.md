# Unity → native-engine re-wrap (architecture blueprint)

**Branch (proposed):** `feat/native-engine-rewrap`. Big restructuring — Unity stops
being a C# *engine* and becomes a thin *wrapper* over native engines, one per
platform. This is the Unity analog of `sdk-flutter/NATIVE_ENGINE_REWRAP.md`; read
that first. This doc adapts the same shape to Unity's IL2CPP/Mono + P/Invoke +
`AndroidJavaObject` + `.jslib` realities.

## Decision (2026-07-09, follows the 2026-07-08 KMP-unification pivot)

- The Unity SDK is a **pure thin wrapper**. The C# engine under
  `Runtime/Internal/` (HTTP via the generated `AttriaxSdkClient` + queue/retry/
  consent/sessions/deep-links/referrer/SKAN) is **retired**.
- Engine per platform:
  | Platform | Native engine | Artifact | Binding |
  | --- | --- | --- | --- |
  | Android (device) | **KMP core** | `com.attriax:core-android` (AAR) | `AndroidJavaObject` / `AndroidJavaProxy` |
  | iOS | **KMP core** | KMP **XCFramework** *(built at the Mac)* | `[DllImport("__Internal")]` P/Invoke + `MonoPInvokeCallback` |
  | Windows / Linux / macOS standalone | **KMP core** | Kotlin/Native **shared lib** (`core-mingwx64` / `core-linuxx64` / macos → `.dll`/`.so`/`.dylib` + C header) | `[DllImport]` P/Invoke |
  | Unity Editor (play mode) | **KMP core** | the desktop shared lib for the host OS (Windows here) | same P/Invoke path as standalone |
  | WebGL | **sdk-js** (`@attriax/js`) | bundled JS + a `.jslib` shim | `[DllImport("__Internal")]` into the `.jslib` |
- This resolves the "KMP has no JS target" wrinkle exactly as Flutter did: **WebGL
  runs on sdk-js**, every other Unity target (incl. Editor + desktop players) runs
  on the KMP core. No engine logic remains in C#.
- **SCOPE LOCKED (2026-07-09, user):** Unity must ship on **ALL** platforms
  including **Editor runtime** and **WebGL** — WebGL-over-sdk-js is IN scope (not
  deferred), and the **Editor runs the real engine** (the desktop shared lib), not
  a stub. What the Editor-real-engine path additionally requires is enumerated in
  "Editor runtime — requirements" below.
- **The Editor runs the same engine as a device.** This is the single most
  important Unity-specific win (see Risks — the editor-amplification saga): Editor
  play mode P/Invokes the desktop shared lib, so a play session drives the real
  KMP engine with its file-backed store instead of the C# static-memory
  `AttriaxPlayerPrefs` that survived Enter/Exit-play-mode + `PlayerPrefs.DeleteAll()`.

## Current state (what we're inverting)

The Unity SDK today is a UPM package `com.attriax.unity` (`Runtime/` + `Editor/` +
`Samples~/`). The **C# layer is the engine** — the entire `Runtime/Internal/` tree
is a faithful C# port of the Flutter engine:

- **Composition root:** `AttriaxRuntime` (`Runtime/Internal/AttriaxRuntime.cs` +
  `.Activation.cs`/`.Bootstrap.cs`), wiring ~30 managers.
- **Transport / HTTP:** the generated OpenAPI client
  `Runtime/Internal/Generated/AttriaxSdkClient/**` (UnityWebRequest-based) behind
  `Transport/AttriaxGeneratedGateway.cs` + `AttriaxGeneratedRequestFactory.cs`,
  driven by `AttriaxRequestManager.cs`.
- **Queue / retry / batching:** `AttriaxRequestQueue.cs`,
  `Dispatch/AttriaxRequestRetryPolicy.cs`, `Dispatch/AttriaxBatchLimits.cs`.
- **Consent:** `AttriaxConsentManager.cs`, `AttriaxConsentStore.cs`,
  `Consent/AttriaxConsentPolicy.cs`, `Consent/AttriaxConsentQueuePolicy.cs`.
- **Sessions:** `AttriaxSessionManager.cs`, `Session/AttriaxSessionContinuationPolicy.cs`,
  `AttriaxSessionState.cs`, `AttriaxSessionStore.cs`.
- **Deep links / referrer:** `AttriaxDeepLinkManager.cs`,
  `DeepLinks/AttriaxDeepLinkBrowserHandler.cs`, `AttriaxReferrerManager.cs`,
  `Referrers/*`, `AttriaxInstallReferrerStore.cs`,
  `AttriaxPlatformInstallReferrerManager.cs`.
- **SKAN:** `AttriaxSkanManager.cs`, `Skan/*`.
- **Crash / app-open / lifecycle:** `AttriaxCrashReportingCoordinator.cs`,
  `AttriaxAppOpenManager.cs`, `AttriaxAppOpenLaunchCoordinator.cs`,
  `AttriaxLifecycleDispatcher.cs` (a `MonoBehaviour` that also owns
  `AttriaxPlayerPrefs`, the persistence layer), `AttriaxContextManager.cs`,
  `AttriaxContextSnapshotBuilder.cs`.
- **Native signal bridges (the ONLY native code today):** `AttriaxNativeBridge.cs`
  → `Runtime/Plugins/Android/AttriaxUnity.androidlib/` (Java:
  `AttriaxUnityAndroidBridge.java`, `AdvertisingIdProvider.java`,
  `AttriaxUnityInAppBrowserActivity.java`) and `Runtime/Plugins/iOS/AttriaxUnityIOS.mm`.
  These are **thin signal providers only** (native context, install referrer, ATT
  status, SKAN update, browser-open, attribution clipboard, WebView UA) — exactly
  the Flutter "signal provider" role.

**Public surface (kept, backward-compatible):** `Attriax.cs` (the `IDisposable`
instance + `Configured` singleton + `InitializeConfiguredAsync`), `AttriaxTracking.cs`,
`AttriaxConsent.cs`, `AttriaxSynchronization.cs`, `AttriaxDeepLinks.cs`,
`AttriaxReferrer.cs`, `AttriaxSkan.cs`, `AttriaxBehaviour.cs` (the `MonoBehaviour`
component), `AttriaxTypes.cs`/`AttriaxProjectSettings.cs`/`AttriaxAnalyticsKeys.cs`.
These call into `AttriaxRuntime`.

The re-wrap **inverts** this: the engine moves into native; a new C# platform
abstraction expands from *signals* to the *full engine surface*; the C# engine
(`Runtime/Internal/` minus the DTO/type shims the public surface still needs) is
deleted; the two native plugin projects are **repurposed** from signal providers
to KMP-engine hosts.

## Target architecture

```
app code / scene
  │  (unchanged public C# API — backward compatible)
  ▼
Attriax / AttriaxBehaviour (C# facade + MonoBehaviour)   ← NO engine logic; forwards every call
  │      Tracking / Consent / Synchronization / DeepLinks / Referrer / Skan
  ▼
IAttriaxEnginePlatform (C#)   ← NEW: full engine command + event surface, platform-dispatched
  │            │                 │                    │
  ▼            ▼                 ▼                    ▼
Android      iOS              Desktop + Editor       WebGL
core-android  XCFramework      core-mingwx64/…        sdk-js
(AndroidJava) (P/Invoke)       (P/Invoke shared lib)  (.jslib)
  engine       engine            engine                 engine
```

- **Public C# API is unchanged** — `new Attriax(config)` + `InitializeAsync`,
  `Attriax.Configured`, `AttriaxBehaviour` inspector flow, `tracking.RecordEvent`,
  `consent.Gdpr.*`, `deepLinks.*`, `synchronization.Subscribe`, `referrer.*`,
  `skan.*`. Existing scenes/scripts don't change.
- The facade + `AttriaxBehaviour` hold **no engine state/logic**. They keep only
  Unity-glue responsibilities: lifecycle wiring (`AttriaxLifecycleDispatcher`
  feeding deep-link/pause/scene signals into the engine), the multi-instance
  Editor warning, and the `Configured` singleton bookkeeping. The **native engine
  is authoritative** for identity, queue, consent, sessions.
- `AttriaxRuntime` is **replaced** by a thin C# coordinator that owns one
  `IAttriaxEnginePlatform` and marshals commands/events (the class name can stay
  for source-compat with the internal surfaces, but its body becomes forwarding).

### C# facade contract — `IAttriaxEnginePlatform`

The single seam every facade method routes through. It mirrors the **KMP public
surface** (`Attriax` + `tracking`/`consent`/`deepLinks`/`synchronization`/
`referrer`/`skan`, `com/attriax/sdk/Attriax.kt`) 1:1 — the same shape the Flutter
platform interface uses. sdk-js exposes the same shape for WebGL. Wire DTOs
already match (all three cores were built to the same API contract).

**Commands (C# → native; `Task`/`Task<T>` for the async ones, sync getters where
the KMP surface is synchronous):**

- Lifecycle: `Initialize(config)`, `Reset()`, `Dispose()`, `Flush()`,
  `SetEnabled(bool)`.
- Tracking: `RecordEvent`, `RecordPurchase`, `RecordRefund`, `RecordAdRevenue`,
  `RecordAdEvent`, `RecordError`, `RecordPageView`, `RecordNotification`,
  `SetUser`, `SetUserProperty`/`SetUserProperties`/`ClearUserProperties`,
  `RegisterFirebaseMessagingToken`, `RegisterApplePushToken`.
- Consent: `SetGdprConsent`, `SetNotRequired`, `ResetConsent`,
  `RequestDataErasure`, `SetAnonymousTracking(bool)`; ATT
  `SetAttStatus`/`RequestAttAuthorization`.
- Deep links: `HandleIncomingLink(uri, isInitialLink)`,
  `RecordDeepLinkConversion`, `CreateDynamicLink`, `WaitForInitialDeepLink`,
  `WaitDeepLinkResolution`.
- Revenue/Apple: `ValidateReceipt`, `SubmitAsaToken`, `UpdateSkanConversionValue`.
- Getters/snapshots: `DeviceId`, `IsFirstLaunch`, `IsInitialized`,
  `SynchronizationState`, `SdkSnapshot`, referrer getters
  (`OriginalInstallReferrer`/`ReinstallReferrer`/`RawInstallReferrer`/
  `SessionReferrer`/`LatestDeepLinkReferrer`), `SkanState`.

**Events (native → C#; delivered onto the Unity main thread via
`AttriaxLifecycleDispatcher.PostToMainThread` — fire-and-forget, NEVER the blocking
`InvokeOnMainThread`, see Risks):**

- Synchronization-state transitions → `AttriaxSynchronization.Subscribe` /
  `AttriaxBehaviour.SynchronizationChanged`.
- Raw + resolved deep-link events + initial-link resolution →
  `AttriaxDeepLinks.RawStream`/`Stream` / `AttriaxBehaviour.DeepLinkReceived`.

### Native bindings

- **Android** (`Runtime/Plugins/Android/`): the existing `.androidlib` is
  **repurposed** — it depends on `com.attriax:core-android` (AAR dropped into
  `Runtime/Plugins/Android/`), and its Java class (rename/extend
  `AttriaxUnityAndroidBridge`) builds one `com.attriax.sdk.Attriax` via
  `AttriaxSdk.create(context, config)` (`core/src/androidMain/.../AttriaxSdk.kt`)
  and exposes command methods. The C# platform impl holds an `AndroidJavaObject`
  wrapping that bridge; commands are `Call`/`CallStatic` off the main thread.
  **Engine→C# callbacks** (sync-state, deep-link) cross back via an
  `AndroidJavaProxy` implementing the KMP listener interfaces
  (`AttriaxSynchronizationStateListener`, `AttriaxDeepLinkListener`/
  `AttriaxRawDeepLinkListener`); the proxy `Invoke` marshals onto the Unity main
  thread. The old signal Java (`AdvertisingIdProvider`, in-app browser activity)
  is **superseded** by the KMP core's own `com.attriax.sdk.android.*` adapters
  (OkHttp, SharedPreferences, Play install-referrer, ProcessLifecycle, ACTION_VIEW
  browser opener) — it can be deleted once the AAR is in.
- **iOS** (`Runtime/Plugins/iOS/`): embed the KMP **XCFramework**; expand
  `AttriaxUnityIOS.mm` into a C-ABI shim that holds the KMP engine (via the
  Swift/ObjC facade the KMP `iosMain` will expose) + the Apple seams
  (ATT/IDFA/SKAN/ASA/App-Attest — the KMP `iosMain` actuals). C# binds the C
  functions with `[DllImport("__Internal")]`; callbacks return via C function
  pointers registered from C# as `static` methods annotated
  `[AOT.MonoPInvokeCallback(typeof(...))]`. **Mac-gated** (XCFramework + iosMain
  actuals don't exist yet).
- **Desktop** (`Runtime/Plugins/x86_64/` etc.): KMP must emit a **shared lib** with
  a generated C header — add `binaries { sharedLib() }` (or `executable`→`sharedLib`)
  to the `mingwX64`/`linuxX64` (and future macos) targets in
  `sdk-kmp/core/build.gradle.kts`; today they only produce **klibs**
  (`PUBLISHING.md`), which are Kotlin-consumer artifacts, NOT P/Invoke-loadable.
  The desktop transport already exists (`AttriaxDesktopNative.create`, Ktor
  WinHttp/Curl, POSIX file store). C# binds the exported C functions with
  `[DllImport("attriax_core")]`. **This is a required new sdk-kmp build output.**
- **Editor** (`Runtime/Plugins/x86_64/` on the host OS, marked Editor-compatible):
  P/Invokes the **same** desktop shared lib as the standalone player. This makes
  Editor play mode exercise the real engine with its file-backed store. Dispose
  discipline is critical — hook `EditorApplication.playModeStateChanged` (and the
  facade `Dispose`) to tear the native instance down on play-mode exit so the
  loaded lib's global engine state doesn't leak across Enter/Exit like the C#
  statics did.
- **WebGL** (`Runtime/Plugins/WebGL/`): bundle `@attriax/js` (IIFE/global build)
  and a `.jslib` that forwards C# `[DllImport("__Internal")]` calls into it;
  engine→C# callbacks come back via `SendMessage(gameObject, method, json)` to a
  hidden Attriax GameObject (or a registered callback table). Mirrors how
  Flutter-web wraps sdk-js. Feasible; scope is an open question (see Uncertainties).

## Phased plan (each phase ends verifiable)

0. **Architecture + branch** — this doc. Cut `feat/native-engine-rewrap`. ✅
1. **Introduce `IAttriaxEnginePlatform`** — define the full command+event surface
   in C#; provide a `FakeEnginePlatform` for EditMode tests; reshape `AttriaxRuntime`
   into the coordinator that owns the interface. Verify: package compiles, EditMode
   tests pass against the fake, public API unchanged.
2. **Android binding** — drop the `core-android` AAR into `Runtime/Plugins/Android/`,
   repurpose the `.androidlib` bridge to `AttriaxSdk.create`, implement the C#
   platform over `AndroidJavaObject` + `AndroidJavaProxy` callbacks. Verify on a
   device/emulator: init→`/open` 201, event→`/batch`, sync-state stream, deep link;
   cross-check the dashboard row against the dev API (identity resolved, non-anon).
3. **Desktop + Editor binding** — add `sharedLib()` outputs to sdk-kmp
   mingwX64/linuxX64, drop the host shared lib into `Runtime/Plugins/x86_64/`,
   implement the C# `[DllImport]` platform, wire play-mode-exit dispose. Verify in
   the **Editor** first (real engine, live dev API — satisfies the standing
   live-test rule on the dev box), then a Windows standalone player build.
4. **WebGL binding** — bundle sdk-js + `.jslib`; implement the C# platform + the
   `SendMessage` callback path. Verify a WebGL build in a browser against the dev API.
5. **Delete the C# engine** — remove `Runtime/Internal/**` (engine managers +
   generated `AttriaxSdkClient` + signal bridges), keeping only the public-surface
   type shims still referenced. Migrate the C# tests from engine-internal to
   facade-level. Verify: package compiles with the engine gone; all four platforms
   still green.
6. **iOS binding (Mac-gated)** — XCFramework + expanded `.mm` + P/Invoke platform;
   device verify.
7. **Parity + verification** — run the sample scenes on each platform; device +
   live-API smokes; cross-check wire shapes against the running API; update
   `AttriaxUnityPackageExporter` + export a clean `.unitypackage`.

Mac-gated: Phase 6 (iOS). Everything else runs on the Windows dev box (Android via
emulator/device; desktop+Editor native; WebGL in a browser).

### Engine-selection seam (landed 2026-07-10 — incremental start of Phase 5)

The facade rewire is being done **incrementally**, with the C# engine kept as the
default and fallback on every platform, so each native binding can be flipped +
live-verified one at a time (zero-behavior-change checkpoints) instead of a
big-bang swap. This first step introduces only the *seam*:

- **`Runtime/Internal/IAttriaxEngine.cs`** — the facade-facing engine surface (the
  exact public-API-shaped members `AttriaxRuntime` already exposes: option-object
  commands, synchronous property getters, `Subscribe*` handles, `IDisposable`).
  The seven public wrappers (`AttriaxTracking`/`Consent`/`DeepLinks`/`Referrer`/
  `Skan`/`Synchronization` + the top-level `Attriax`) now depend on this interface
  instead of the concrete `AttriaxRuntime`.
- **`AttriaxRuntime : IAttriaxEngine`** — implemented with **zero signature
  changes** (five deep-link getters were widened `internal`→`public` on the
  already-internal class purely to satisfy implicit interface implementation).
- **`Runtime/Internal/AttriaxEngineSelector.cs`** — `Create(config)` picks the
  engine. **Today it returns the managed C# engine on EVERY `RuntimePlatform`.**
  `TryCreateNativeEngine` holds the per-platform hooks (Android/iOS/desktop+Editor/
  WebGL), all inactive (`return null`) → C# fallback everywhere.

**Why this seam and not routing facades through `IAttriaxEnginePlatform`
directly:** `IAttriaxEnginePlatform` is the *wire-shaped* native contract
(decomposed primitive args, all-async getters, C# events) mirroring the Flutter/KMP
platform interface. It does **not** match the facade surface, so binding facades to
it now would require rewriting every facade **and** building the full
option-object→primitive-arg lowering adapter up front — large churn with real
behavior-change risk, and premature. Instead the facades bind to `IAttriaxEngine`
(the surface `AttriaxRuntime` *already* satisfies), and when a platform's native
engine is ready a thin `AttriaxEnginePlatformAdapter : IAttriaxEngine` will wrap its
`IAttriaxEnginePlatform` impl and be returned from the matching selector branch.
`IAttriaxEnginePlatform` is unchanged by this step.

## Packaging

Unity consumes native libs by folder convention under `Runtime/Plugins/`:
`Android/` (AAR + `.androidlib`), `iOS/` (XCFramework + `.mm`), `x86_64/`
(desktop `.dll`/`.so`/`.dylib`, platform-and-Editor targeted via `.meta` import
settings), `WebGL/` (`.jslib` + bundled sdk-js). The KMP artifacts flow in via a
**pre-export copy step**:

1. `sdk-kmp`: `./gradlew :core:publishToMavenLocal` (AAR) and the new
   `sharedLib` assemble tasks (desktop libs); build the XCFramework at the Mac.
2. A staging script copies `core-android.aar` → `Runtime/Plugins/Android/`, the
   desktop shared libs → `Runtime/Plugins/x86_64/<platform>/`, the XCFramework →
   `Runtime/Plugins/iOS/`, and a built `@attriax/js` bundle → `Runtime/Plugins/WebGL/`,
   each with the correct `.meta` platform-inclusion flags.
3. `AttriaxUnityPackageExporter.PackageExportPaths`
   (`Assets/Editor/AttriaxUnityPackageExporter.cs`) gains the `Runtime/Plugins/**`
   paths so the exported `.unitypackage` bundles every native artifact.

No CI (per project rule) — these are manual/local publish + copy + export steps,
consistent with `sdk-kmp/PUBLISHING.md`.

## What gets deleted (superseded by the KMP core)

Entire `Runtime/Internal/` engine, each superseded by its KMP equivalent — with
the **hard-won Unity fixes now inherited from KMP** (the C# reimplementations are
retired, not re-audited):

- `AttriaxRequestQueue` / `Dispatch/*` / `AttriaxRequestManager` /
  `Transport/*` + `Generated/AttriaxSdkClient/**` → KMP
  `AttriaxQueueManager` + `AttriaxDispatcher` + Ktor/OkHttp transport. **The
  build-then-flush re-stamp fix (`RestampQueuedRequestsForLiveContext`, memory
  `sdk-unity-queued-request-restamp`) is moot** — KMP resolves identity before
  enqueue (Flutter-style), so Unity's unique build-then-flush window disappears.
- `AttriaxConsentManager` / `AttriaxConsentStore` / `Consent/*` → KMP
  `AttriaxConsentManager` + `AttriaxConsentQueuePolicy`. **The consent-sync
  downgrade race (memory `sdk-unity-consent-sync-downgrade-race`) is inherited-fixed**
  — KMP carries the generation-guard + re-sync loop (the root cause of the whole
  anonymous-analytics saga).
- `AttriaxSessionManager` / `Session/*` / `AttriaxSessionState`/`Store` → KMP
  `AttriaxSessionManager` + `AttriaxSessionLifecycleManager`.
- `AttriaxDeepLinkManager` / `DeepLinks/*` / `AttriaxReferrerManager` /
  `Referrers/*` / install-referrer stores → KMP `AttriaxDeepLinkManager` +
  `AttriaxReferrerCoordinator` + `AttriaxInstallReferrerCoordinator`.
- `AttriaxSkanManager` / `Skan/*` → KMP `AttriaxSkanEngine`.
- `AttriaxCrashReportingCoordinator` / `AttriaxAppOpenManager` /
  `AttriaxContextManager`/`SnapshotBuilder` → KMP crash-reporting manager +
  app-open bootstrap + context snapshot.
- `AttriaxNativeBridge` + the `.androidlib` signal Java + `.mm` signal methods →
  superseded by KMP `com.attriax.sdk.android.*` adapters and the future
  `iosMain` actuals. The plugin **projects** survive (repurposed as engine hosts);
  the signal **code** is deleted.

**Retained C#:** `AttriaxLifecycleDispatcher` (still needed — it is the Unity
lifecycle/main-thread bridge that feeds `Application.deepLinkActivated`, pause/
focus, scene changes into the engine and marshals callbacks; but
`AttriaxPlayerPrefs` inside it is deleted — persistence moves into the native
engine's own store). All public-surface classes + `AttriaxProjectSettings` +
`AttriaxConfig`/`AttriaxInitOptions` DTOs.

## Risks / rules

- **Backward compatibility:** the public C# API MUST stay identical (existing
  scenes/scripts unaffected). Any change is a breaking-change decision. Keep the
  `Attriax.Configured` singleton + `AttriaxBehaviour` inspector flow intact.
- **IL2CPP AOT + P/Invoke callbacks:** on iOS/WebGL (and IL2CPP standalone) every
  native→managed callback must be a **`static`** method tagged
  `[AOT.MonoPInvokeCallback(typeof(TDelegate))]`; no instance/closure delegates
  can cross the boundary (AOT can't JIT the thunk). Callback context must be passed
  as an opaque handle (e.g. `GCHandle` int) and resolved C#-side. Budget for this
  on the iOS + WebGL platforms specifically.
- **Main-thread marshaling (ties to `sdk-unity-debuglog-mainthread-deadlock`):**
  engine→C# callbacks (`AndroidJavaProxy.Invoke`, P/Invoke callbacks, WebGL
  `SendMessage`) arrive off the Unity main thread. Deliver them via
  `AttriaxLifecycleDispatcher.PostToMainThread` (fire-and-forget) — **never** the
  blocking `InvokeOnMainThread`+`Wait`, which deadlocks if the caller holds an
  engine lock the main thread also contends for. This lesson survives the rewrite;
  the C# engine locks are gone, but the native engine can still fan callbacks from
  its own threads.
- **Editor engine lifetime (ties to `sdk-unity-anonymous-fragmentation-and-editor-amplification`):**
  the win is that Editor now runs the real engine + file-backed store, so stale
  static memory across Enter/Exit-play-mode no longer amplifies one device into
  "4-6 users". BUT the loaded native lib has its own process-global engine state;
  dispose it on `playModeStateChanged`→exit so it doesn't leak the same way.
- **WebGL parity nuance:** sdk-js has two *intentional* divergences from the native
  core (terminal-retry drop; batch-identity `projectToken`) — documented in memory
  (`sdk-js-retry-divergence`, `sdk-js-batch-identity-divergence`), not bugs.
  Unity-WebGL inherits sdk-js behavior on those points, exactly like Flutter-web.
- **Reference:** the retired C# engine is the historical behavior reference; the
  KMP core (parity-audited, P1–P7b closed) and sdk-js are the new source of truth.
  Keep the C# engine in git history for rollback.
- **No push** until the user asks; **child-before-root** submodule discipline
  (sdk-kmp changes for `sharedLib()` land + are committed in sdk-kmp first, then
  sdk-unity, then the root records new pointers).

## Editor runtime — requirements (decided: real engine)

The Editor (Mono on the host OS — Windows on the dev box) loads the **same desktop
shared lib as the standalone player** and P/Invokes it. This is the fix for the
editor-amplification saga (real engine + file-backed store replaces the C# statics
that survived Enter/Exit + `PlayerPrefs.DeleteAll()`). Concretely it requires:

1. **A C-ABI export layer in `sdk-kmp` (net-new engine surface, NOT just a build
   flag).** The KMP native engine (`AttriaxDesktopNative.create`, Ktor + POSIX
   file store) EXISTS, but nothing exposes it as C-callable functions. We add
   Kotlin/Native `@CName`-exported entry points over it (create / init /
   record* / flush / getters / dispose + a callback-registration function for
   sync-state & deep-link events) and `binaries { sharedLib() }` so the build emits
   `.dll`/`.so`/`.dylib` + a generated C header. **Shared prerequisite:** the
   Flutter Windows/Linux FFI phase needs the exact same output — build once, reuse.
2. **Windows Editor DLL-lock. DECIDED (2026-07-09, user): option (b) — the
   reliable method.** On Windows, once Mono P/Invokes a native `.dll`, the Editor
   process holds the file **locked for its whole session** — a `[DllImport]`-bound
   engine DLL can't be rebuilt/replaced without restarting the Editor (the classic
   Unity native-plugin problem). We therefore bind the engine via **dynamic
   `LoadLibrary`/`GetProcAddress`/`FreeLibrary`** (a hand-written function-pointer
   layer) instead of static `[DllImport]`, so the native lib can be **unloaded on
   play-mode exit** and rebuilt without restarting the Editor. This costs a small
   native-loader abstraction (`INativeLibraryLoader` with a Windows `Kernel32`
   backing + a POSIX `dlopen`/`dlsym`/`dlclose` backing for macOS/Linux Editors);
   it is the reliable path and doubles as the clean teardown for point 3. Standalone
   IL2CPP players still use static `[DllImport("__Internal")]`/direct linkage — the
   dynamic loader is an **Editor/desktop-Mono** concern.
3. **Play-mode dispose (implementation, not a decision).** Hook
   `EditorApplication.playModeStateChanged` → ExitingPlayMode to dispose the native
   engine so its process-global state doesn't leak across Enter/Exit — this is what
   actually kills the amplification. Included by default.

## Settled scope (2026-07-09, user)

- **WebGL: IN.** Required target — the sdk-js bundle + `.jslib` + `SendMessage`
  callback plumbing + AOT callback work is a committed phase (Phase 4), not deferred.
- **Editor: real engine, dynamic-loaded** (option (b), reliable) — per "Editor
  runtime — requirements" above.
- **sdk-kmp `sharedLib()` + C-ABI: IN scope** (shared with Flutter desktop FFI).
  Committed and pushed in sdk-kmp first (child-before-root).
- **Wrapper→native data forwarding is MANDATORY and must be exhaustive.** Unity (and
  Flutter/RN) can observe runtime data the native core cannot derive on its own
  (Unity `Application`/`SystemInfo`/`Screen` fields, engine/graphics info, the app's
  own build metadata, navigation/scene signals, framework-obtained ATT status, push
  tokens, etc.). Every such datum MUST be forwarded across the platform seam into the
  native engine's `AttriaxConfig`/context/command args — nothing may be silently
  dropped. The exhaustive field-by-field contract (who can supply each field:
  native-derivable / wrapper-only / both; required vs optional; wire destination) is
  produced by the workspace **ideal-architecture review** and is the authority both
  this and the Flutter blueprint defer to.
