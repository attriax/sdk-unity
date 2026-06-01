# Attriax Unity SDK Development Guide

## Quick Reference

### Project Structure

- `Packages/com.attriax.unity/` — publishable Unity package surface
- `Packages/com.attriax.unity/Runtime/` — public runtime APIs and shared logic
- `Packages/com.attriax.unity/Runtime/Internal/` — orchestration managers,
  persistence, generated gateway wrappers, and internal helpers
- `Packages/com.attriax.unity/Runtime/Internal/Generated/AttriaxSdkClient/` —
  generated internal transport client
- `Packages/com.attriax.unity/Runtime/Plugins/Android/` — Android native bridge
- `Packages/com.attriax.unity/Runtime/Plugins/iOS/` — iOS native bridge
- `Packages/com.attriax.unity/Samples~/` — public sample assets and scripts
- `Packages/com.attriax.unity/Tests/Editor/` — EditMode regression tests
- `Assets/Editor/` — Unity batch export tooling for `.unitypackage`

### Essential Commands

Run these from the workspace root:

```bash
npm install
npm run sdk:unity:generate
npm run unity:test:editor
npm run sdk:unity:validate
npm run unity:release:check
```

The supported generated-client workflow is documented in `SDK_CLIENT_GENERATION.md`.

### Development Checklist

- [ ] Regenerated the Unity SDK client if the SDK contract changed
- [ ] Added or updated EditMode tests
- [ ] Expanded the public sample when public integration behavior changed
- [ ] Updated repository and package README files when setup or behavior changed
- [ ] Updated both changelogs for release-facing changes
- [ ] Ran `npm run unity:test:editor`
- [ ] Ran `npm run sdk:unity:validate`
- [ ] Re-tested Android/iOS device behavior when the native bridge changed

## Feature Workflow

When adding a feature:

1. Extend the public surface in `Packages/com.attriax.unity/Runtime/` only when
   the behavior belongs in the user-facing SDK.
2. Keep orchestration and persistence in focused managers under
   `Runtime/Internal/` instead of growing `AttriaxRuntime` indiscriminately.
3. Update the Android or iOS bridge under `Runtime/Plugins/` only when the
   feature truly needs a native hook.
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

- The Unity package is embedded in a minimal Unity project so the same source
  tree can ship as both a UPM package and a `.unitypackage` export.
- The generated client is intentionally embedded inside the runtime assembly so
  internal runtime code can use generated internal transport types directly.
- If the Unity editor lives at a non-default path, set `UNITY_EDITOR_PATH`
  before running the batch release checks.