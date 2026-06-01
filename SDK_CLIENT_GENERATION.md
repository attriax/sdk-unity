# Unity SDK Client Generation

`sdk-unity/` owns a generated C# client for the SDK-only Attriax API contract.

The source of truth stays on the API side in `api/generated/sdk-contract.openapi.json`. The Unity workspace consumes that contract through a root-owned generation script so the process stays repeatable on Windows and does not depend on ad hoc local steps.

## Supported Commands

Run these from the workspace root:

```bash
npm install
npm run sdk:unity:generate
```

Regenerate without the export validation step:

```bash
npm run sdk:unity:generate:fast
```

Revalidate the current generated client without regenerating it:

```bash
npm run sdk:unity:validate
```

## What The Commands Do

`npm run sdk:unity:generate`

1. Regenerates the SDK-only OpenAPI contract in `api/generated/sdk-contract.openapi.json`.
2. Runs OpenAPI Generator with the C# `unityWebRequest` target.
3. Embeds the generated client into `Packages/com.attriax.unity/Runtime/Internal/Generated/AttriaxSdkClient/`.
4. Removes the generator-created child `.asmdef` so the generated code stays in the main `Attriax.Runtime` assembly.
5. Exports the Unity package through `export-unitypackage.ps1` to verify the generated client compiles inside the Unity project.

`npm run sdk:unity:validate`

1. Reuses the existing generated client.
2. Runs the batch Unity export.
3. Verifies that `dist/Attriax.Unity.unitypackage` is produced.

## Why The Generated Client Is Embedded

Unlike the Flutter workspace, the Unity SDK already ships as one embedded UPM package and one exported `.unitypackage` artifact.

Keeping the generated client under `Packages/com.attriax.unity/Runtime/Internal/Generated/` is the simplest reliable layout because:

- the generated files ship with the same Unity package that already contains the public runtime
- the package export process already copies `Packages/com.attriax.unity/Runtime`
- removing the child `.asmdef` keeps the generated C# in the main runtime assembly, which matters because the generator is configured with `nonPublicApi=true`

That last point is important: if the generated client stayed in its own assembly, later runtime integration would not be able to use the generated internal types directly.

## Generated File Ownership

- Treat `Packages/com.attriax.unity/Runtime/Internal/Generated/AttriaxSdkClient/` as generated code.
- Do not hand-edit files there unless you are fixing the regeneration tooling itself.
- If generator output needs to change, update `scripts/unity-sdk-client.mjs` or the API contract source, then regenerate.

## Validation Standard

The supported validation step is the existing batch export:

```powershell
cd sdk-unity
powershell -ExecutionPolicy Bypass -File .\export-unitypackage.ps1
```

That is the cheapest workspace-owned executable check that forces Unity to import and compile the package before writing `dist/Attriax.Unity.unitypackage`.

## Current Runtime Status

The Unity runtime is now integrated with the embedded generated client.

`AttriaxRuntime` still owns queueing, synchronization, context collection, deep-link orchestration, and public result mapping, but SDK request execution now goes through the generated `SdkApi` client under `Packages/com.attriax.unity/Runtime/Internal/Generated/AttriaxSdkClient/` instead of handwritten `UnityWebRequest` endpoint code.