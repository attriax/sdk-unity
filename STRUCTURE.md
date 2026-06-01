# Attriax Unity Plugin Structure

## Overview

`sdk-unity/` is the publishable Attriax Unity package and also a minimal Unity
project wrapper that can export that same source as a traditional
`.unitypackage` artifact.

## Repository Layout

```text
sdk-unity/
├── Assets/
│   └── Editor/                           # Batch export entrypoint for .unitypackage
├── Runtime/                              # Public API, runtime assembly, shared managers
│   ├── Internal/                         # Queueing, synchronization, context, lifecycle, generated gateway
│   │   └── Generated/
│   │       └── AttriaxSdkClient/         # Generated internal transport client
│   └── Plugins/
│       ├── Android/                      # Android native bridge
│       └── iOS/                          # iOS native bridge
├── Editor/                               # Editor tooling shipped with the package
├── Samples~/                             # Public samples imported through Package Manager
├── Tests/
│   └── Editor/                           # EditMode regression tests
├── Packages/manifest.json                # Local Unity-project wrapper consuming the root package
├── README.md
├── CHANGELOG.md
├── package.json
├── dist/                                 # Generated .unitypackage and batch test results
├── export-unitypackage.ps1               # Supported Unity export wrapper
├── SDK_CLIENT_GENERATION.md              # Generated-client workflow
└── PUBLISHING.md
```

## Architectural Notes

- `Runtime/Attriax.cs` is the primary public entry
  point.
- `AttriaxBehaviour.cs` exposes the MonoBehaviour-owned integration path.
- `Runtime/Internal/AttriaxRuntime.cs` coordinates queueing, context,
  synchronization, sessions, app-open flows, and deep links.
- `Runtime/Internal/Generated/AttriaxSdkClient/` stays embedded in the runtime
  package so internal runtime code can consume generated internal types without
  a separate assembly boundary.
- `Samples~/` should demonstrate public integration paths only; internal QA
  flows belong elsewhere.

## Validation Flow

- `npm run unity:test:editor` runs the package EditMode tests in batch mode.
- `npm run sdk:unity:validate` exports the package to verify Unity imports and
  compiles it.
- `npm run unity:release:check` runs both checks as the release gate.