# Attriax Unity SDK Development Guide

## Quick Reference

### Project Structure

- `sdk-unity/` root — publishable Unity package surface
- `Runtime/` — public C# facade; **no engine logic** (see `NATIVE_ENGINE_REWRAP.md`)
- `Runtime/Internal/Engine/` — `IAttriaxEnginePlatform` + the per-platform
  native bindings/mappers/adapter (Android JNI, iOS/desktop/macOS C-ABI
  P/Invoke, WebGL `.jslib`)
- `Runtime/Internal/AttriaxLifecycleDispatcher.cs` — the remaining Unity-glue
  manager (lifecycle/deep-link signals in, main-thread callback marshaling out)
- `Runtime/Plugins/Android/` — `.androidlib` + `com.attriax:core` AAR dependency (from Maven Central)
- `Runtime/Plugins/iOS/` — `AttriaxCoreCApi.xcframework` (C-ABI static lib) + signal `.mm`
- `Runtime/Plugins/macOS/`, `Runtime/Plugins/x86_64/Windows|Linux/` — desktop C-ABI shared libs
- `Runtime/Plugins/WebGL/` — `.jslib` router + vendored `@attriax/js` bundle
- `Samples~/` — public sample assets and scripts
- `Tests/Editor/` — EditMode regression tests
- `Assets/Editor/` — Unity batch build/export tooling (per-platform builds + `.unitypackage` export)

### Essential Commands

Run these from the workspace root:

```bash
npm install
npm run unity:test:editor
npm run sdk:unity:validate
npm run unity:release:check
```

### Development Checklist

- [ ] Added or updated EditMode tests
- [ ] Expanded the public sample when public integration behavior changed
- [ ] Updated repository and package README files when setup or behavior changed
- [ ] Updated the changelog for release-facing changes
- [ ] Ran `npm run unity:test:editor`
- [ ] Ran `npm run sdk:unity:validate`
- [ ] Re-tested device/host behavior on the platform(s) whose native binding changed

## Feature Workflow

When adding a feature:

1. Extend the public surface in `Runtime/` only when
   the behavior belongs in the user-facing SDK — the facade should stay a
   thin forwarder onto `IAttriaxEngine`/`IAttriaxEnginePlatform`, not gain
   engine logic of its own.
2. If the feature needs new engine behavior, that behavior belongs in the
   shared `sdk-kmp` core (or `sdk-js` for WebGL), not in this repo's C#.
   `Runtime/Internal/Engine/` only adds the C# plumbing to call it.
3. Update the Android/iOS/desktop/macOS/WebGL binding under
   `Runtime/Plugins/` + `Runtime/Internal/Engine/` only when the feature
   truly needs a new native hook.
4. Demonstrate the public behavior in `Samples~/`.
5. Add or update EditMode tests in `Tests/Editor/` for the owning logic.
6. Re-run the supported validation commands.

## Testing

- `npm run unity:test:editor` runs the package EditMode tests in batch mode and
  writes `dist/unity-editmode-test-results.xml`.
- `npm run sdk:unity:validate` exports `dist/Attriax.Unity.unitypackage` to
  verify the package imports and compiles through the supported export path.
- `npm run unity:release:check` combines both checks.

## Useful Notes

- The publishable package is embedded at `Packages/com.attriax.unity/` inside
  this repository, which also acts as the minimal Unity project used for
  validation and `.unitypackage` export (see `STRUCTURE.md` for why it's
  embedded rather than referenced from the repo root).
- There is no generated HTTP client anymore — the native engine (KMP core /
  sdk-js) owns wire transport entirely; the C# layer only calls into it
  through `IAttriaxEnginePlatform`. See `NATIVE_ENGINE_REWRAP.md` and
  `SDK_CLIENT_GENERATION.md` (retired) for the history.
- If the Unity editor lives at a non-default path, set `UNITY_EDITOR_PATH`
  before running the batch release checks.