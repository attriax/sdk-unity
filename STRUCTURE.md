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
│   └── Editor/                                   # Batch export entrypoint for .unitypackage
├── Packages/
│   ├── manifest.json                             # Unity-project manifest (package auto-discovered as embedded)
│   └── com.attriax.unity/                        # Embedded, publishable UPM package
│       ├── package.json
│       ├── README.md
│       ├── CHANGELOG.md
│       ├── LICENSE
│       ├── Runtime/                              # Public API, runtime assembly, shared managers
│       │   ├── Internal/                         # Queueing, synchronization, context, lifecycle, generated gateway
│       │   │   └── Generated/
│       │   │       └── AttriaxSdkClient/         # Generated internal transport client
│       │   └── Plugins/
│       │       ├── Android/                      # Android native bridge
│       │       └── iOS/                          # iOS native bridge
│       ├── Editor/                               # Editor tooling shipped with the package
│       ├── Samples~/                             # Public samples imported through Package Manager
│       ├── Documentation~/                       # Package documentation (excluded from import)
│       └── Tests/
│           └── Editor/                           # EditMode regression tests
├── dist/                                         # Generated .unitypackage and batch test results
├── export-unitypackage.ps1                       # Supported Unity export wrapper
├── SDK_CLIENT_GENERATION.md                      # Generated-client workflow
└── PUBLISHING.md
```

## Architectural Notes

- `Packages/com.attriax.unity/Runtime/Attriax.cs` is the primary public entry
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