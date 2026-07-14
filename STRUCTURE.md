# Attriax Unity Plugin Structure

## Overview

`sdk-unity/` is a thin Unity project (`Assets/`, `ProjectSettings/`, `Packages/`)
that embeds the publishable Attriax Unity package under
`Packages/com.attriax.unity/`. Embedding the package under `Packages/` (rather
than referencing the repo root through `file:..`) is required: Unity 6000.2
rejects a local package whose folder is the project root or an ancestor of it,
which crashed the Editor during asset-database init. The same embedded folder is
what the Git-URL install (`?path=Packages/com.attriax.unity`) and the
`.unitypackage` export consume.

## Repository Layout

```text
sdk-unity/
├── Assets/
│   └── Editor/                                   # Batch build/export entrypoints (Android/Desktop/iOS/WebGL builds, .unitypackage export)
├── Packages/
│   ├── manifest.json                             # Unity-project manifest (package auto-discovered as embedded)
│   └── com.attriax.unity/                        # Embedded, publishable UPM package
│       ├── package.json
│       ├── README.md
│       ├── CHANGELOG.md
│       ├── LICENSE
│       ├── Runtime/                              # Public API + thin native-engine wrapper (see NATIVE_ENGINE_REWRAP.md)
│       │   ├── Internal/
│       │   │   └── Engine/                       # IAttriaxEnginePlatform + per-platform bindings/mappers/adapter
│       │   └── Plugins/
│       │       ├── Android/                      # .androidlib + com.attriax:core AAR dependency from Maven Central (JNI)
│       │       ├── iOS/                           # AttriaxCoreCApi.xcframework (C-ABI static lib) + signal .mm
│       │       ├── macOS/                        # libattriax_core.dylib (C-ABI, dlopen)
│       │       ├── x86_64/Windows|Linux/          # attriax_core.dll / libattriax_core.so (C-ABI, dlopen)
│       │       └── WebGL/                         # .jslib router + vendored @attriax/js .jspre bundle
│       ├── Editor/                               # Editor tooling shipped with the package
│       ├── Samples~/                             # Public samples imported through Package Manager
│       ├── Documentation~/                       # Package documentation (excluded from import)
│       └── Tests/
│           └── Editor/                           # EditMode regression tests
├── dist/                                         # Generated .unitypackage and batch test results
├── export-unitypackage.ps1                       # Supported Unity export wrapper
├── NATIVE_ENGINE_REWRAP.md                       # Native-engine migration: final architecture, decisions, verification status
└── PUBLISHING.md
```

## Architectural Notes

- `Packages/com.attriax.unity/Runtime/Attriax.cs` is the primary public entry
  point.
- `AttriaxBehaviour.cs` exposes the MonoBehaviour-owned integration path.
- **The engine is native, not C#.** The managed C# engine (the old
  `AttriaxRuntime` + its exclusive managers, stores, and generated HTTP
  client) was deleted; see `NATIVE_ENGINE_REWRAP.md`. `Runtime/Internal/Engine/`
  now holds `IAttriaxEnginePlatform` and the five platform bindings
  (Android/iOS/Windows+Linux/macOS/WebGL), each driving either the shared KMP
  core or (WebGL only) `@attriax/js`. `AttriaxEngineSelector` picks the
  binding for the running platform; there is no C# fallback.
- `Runtime/Internal/AttriaxLifecycleDispatcher.cs` is the remaining
  Unity-glue manager: it feeds lifecycle/deep-link signals into the engine and
  marshals native→C# callbacks onto the main thread. It holds no engine state.
- `Samples~/` should demonstrate public integration paths only; internal QA
  flows belong elsewhere.

## Validation Flow

- `npm run unity:test:editor` runs the package EditMode tests in batch mode.
- `npm run sdk:unity:validate` exports the package to verify Unity imports and
  compiles it.
- `npm run unity:release:check` runs both checks as the release gate.