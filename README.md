# Attriax Unity Plugin

`sdk-unity/` is the Unity SDK workspace for Attriax.

`sdk-flutter/` is the primary SDK implementation. Keep the Unity runtime
behaviorally aligned with Flutter unless a Unity-specific divergence is
intentional and documented.

The repository root is the publishable UPM package while also carrying the
minimal Unity project files needed to validate and export the SDK as a
traditional `.unitypackage` artifact for teams that still import plugins that
way.

## Install From Git

Use the repository URL directly in Unity Package Manager:

```text
https://github.com/attriax/sdk-unity.git
```

Or add it in a project manifest:

```json
{
  "dependencies": {
    "com.attriax.unity": "https://github.com/attriax/sdk-unity.git#v0.4.1"
  }
}
```

The repo root now contains the actual package manifest and runtime folders, so
the plain Git URL resolves directly without a `?path=` suffix.

## Layout

- `Runtime/` — public runtime APIs, shared managers, and generated client code
- `Runtime/Internal/Generated/AttriaxSdkClient/` — generated internal API client embedded into the runtime package
- `Assets/Editor/` — batch export tooling for `.unitypackage` generation
- `Packages/manifest.json` — local Unity-project wrapper that consumes the root package through `file:..`
- `dist/` — generated artifacts

## Platform Support

- Android and iOS include SDK-owned native bridge code under
  `Runtime/Plugins/`.
- Unity Editor and shared-runtime platforms use the shared C# runtime path for
  queueing, synchronization, deep links, user state, and event tracking.
- The Unity runtime should stay behaviorally aligned with `sdk-flutter/`
  unless a Unity-specific divergence is intentional and documented.

## GDPR Consent

`AttriaxConfig.GdprEnabled` defaults to `false`. Turn it on only when the Unity
runtime should gate GDPR-regulated tracking. Anonymous-capable activity still
sends immediately while consent is unresolved, while attribution-only work
stays withheld until consent allows it.

```csharp
var config = new AttriaxConfig
{
  ProjectToken = "ax_your_project_token",
    GdprEnabled = true,
};

var attriax = new Attriax(config);
await attriax.InitializeAsync(new AttriaxInitOptions());

var needsLocalConsent = await attriax.Consent.Gdpr.NeedsConsentAsync(localOnly: true);
if (needsLocalConsent)
{
  attriax.Consent.Gdpr.SetConsent(
        analytics: true,
        attribution: true,
        adEvents: false);
}

var needsConsent = await attriax.Consent.Gdpr.NeedsConsentAsync();

// Or, when the device should not be gated at all:
attriax.Consent.Gdpr.SetNotRequired();

// Reset later if the game needs to re-ask:
attriax.Consent.Gdpr.Reset();

// Request backend erasure of the current SDK-owned GDPR data:
await attriax.Consent.Gdpr.RequestDataErasureAsync();
```

See [Documentation~/gdpr-and-anonymous-analytics.md](Documentation~/gdpr-and-anonymous-analytics.md) for the full GDPR and anonymous analytics behavior, including how unresolved consent still sends anonymous-capable traffic immediately and how denied analytics is stored without device identity.

## Crash Reporting

The Unity runtime supports both manual and automatic crash reporting.

- Call `attriax.Tracking.RecordErrorAsync(...)` when you want to report a handled
  exception with extra metadata.
- Unhandled Unity exceptions observed through `Application.logMessageReceivedThreaded`
  are captured automatically and queued for delivery when possible.
- Crash payloads include app, device, and session context so dashboard crash
  analytics can be reviewed next to attribution data.

## Generated SDK Client

From the workspace root:

```bash
npm install
npm run sdk:unity:generate
npm run sdk:unity:validate
```

- `npm run sdk:unity:generate` regenerates the internal Unity client from `api/generated/sdk-contract.openapi.json` and then validates it by exporting the Unity package.
- `npm run sdk:unity:generate:fast` skips the post-generation validation run when you only want a quick regeneration.
- `npm run sdk:unity:validate` reruns the Unity export-only validation against the already-generated client.
- `npm run unity:test:editor` runs the Unity package EditMode tests in batch mode.
- `npm run unity:release:check` runs both the EditMode tests and the export validation as the repository release gate.
- The Unity runtime now executes SDK API requests through the embedded generated client inside `Runtime/Internal/Generated/AttriaxSdkClient/`.
- See `SDK_CLIENT_GENERATION.md` for the supported workflow and design notes.
- See `DEVELOPMENT.md`, `PUBLISHING.md`, and `STRUCTURE.md` for contributor and release workflow details.

## Internal Tester

The scene-navigation QA app now lives outside the SDK workspace in the sibling
`../tester-unity/` project.

That separate consumer project depends on the local package at
`../sdk-unity` and is the supported place to verify
automatic scene tracking, persistent SDK host lifetime, and queued `page_view`
payloads from a real package consumer.

## Export

Prefer the supported wrapper script from this project root:

```powershell
powershell -ExecutionPolicy Bypass -File .\export-unitypackage.ps1
```

The raw Unity command used by that script is:

```powershell
"C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe" `
  -batchmode `
  -projectPath . `
  -attriaxExportPackage `
  -logFile -
```

The exported artifact is written to `dist/Attriax.Unity.unitypackage`.

## License

This repository and the publishable Unity package ship under Apache-2.0.
The package source now lives at the repository root, so the same `LICENSE` file
is preserved for both workspace development and Package Manager consumers.

The commercial Attriax hosted service and sibling repositories outside this
repository remain separate from this SDK source release.
