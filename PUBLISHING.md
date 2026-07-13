# Attriax Unity Publishing Notes

This document covers the manual release workflow for the Attriax Unity SDK.

The Unity repository ships two release surfaces:

- the embedded UPM package at `sdk-unity/Packages/com.attriax.unity/` (see
  `STRUCTURE.md` for why it's embedded there rather than at the repo root)
- the exported `.unitypackage` artifact under `sdk-unity/dist/`

Publishing remains manual. CI may validate the repository in the future, but it
must not publish Unity artifacts automatically.

## Release Inputs

- `sdk-unity/Packages/com.attriax.unity/package.json` is the source of truth
  for the package version.
- `sdk-unity/Packages/com.attriax.unity/CHANGELOG.md` tracks package-level
  release history.
- `sdk-unity/Packages/com.attriax.unity/package.json` metadata URLs must
  resolve before a public release.
- The native artifacts under `Runtime/Plugins/**` (Android AAR dependency,
  iOS xcframework, macOS/Windows/Linux C-ABI shared libs, the WebGL
  `@attriax/js` bundle) come from `sdk-kmp`/`sdk-js`, not from this repo — see
  `NATIVE_ENGINE_REWRAP.md` for how they're produced and staged. Make sure
  they're current before cutting a release; there is no generated C# client
  to refresh in this repo anymore (`SDK_CLIENT_GENERATION.md`, retired).

## Required Release Checks

Run these commands from the workspace root before every Unity release:

```bash
npm install
npm run unity:release:check
```

`npm run unity:release:check` runs the Unity EditMode tests and then exports
`dist/Attriax.Unity.unitypackage` through the supported batch export flow.

Set `UNITY_EDITOR_PATH` when the Unity editor is installed somewhere other
than the default derived from `ProjectSettings/ProjectVersion.txt` (currently
`6000.4.6f1`, i.e.
`C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe`).

Run the release flow from a clean git state so version changes, generated
artifacts, and release notes stay easy to review.

## Release Steps

1. Ensure the intended public repository is reachable, that the package
  metadata URLs in `sdk-unity/Packages/com.attriax.unity/package.json` resolve,
  and that the embedded-package Git URL install flow still works
  (`https://github.com/attriax/sdk-unity.git?path=Packages/com.attriax.unity#<tag>`).
2. Update `sdk-unity/Packages/com.attriax.unity/package.json` and its
  `CHANGELOG.md` for the new version.
3. If any native platform binding changed, republish/rebuild the affected
   artifact from `sdk-kmp` (or `sdk-js` for WebGL) and re-stage it under
   `Runtime/Plugins/**` before proceeding — see `NATIVE_ENGINE_REWRAP.md`.
4. Run `npm run unity:release:check`.
5. Review `dist/Attriax.Unity.unitypackage` and import it into a clean Unity
   project if the release touched the runtime, package structure, or native
   plugins.
6. Tag and publish the release manually through the repository hosting flow.

## Platform Checklist

The engine is native on every platform (see `NATIVE_ENGINE_REWRAP.md`); verify
the platform(s) whose native binding changed:

- Android: app-open tracking, install referrer handling, deep-link flows, and
  Firebase uninstall-token registration.
- iOS: app-open tracking, deep-link flows, and Firebase uninstall-token
  registration. Still Mac-gated for a full on-device pass.
- Windows / Linux / macOS (standalone + Editor): persistence, queue repair,
  synchronization state reporting, and manual deep-link handling against the
  file-backed native store. macOS is still Mac-gated for a full pass.
- WebGL: the same behaviors in a browser build, routed through `@attriax/js`.

## Packaging Note

The publishable Unity package is Apache-2.0 so integrators and security
reviewers can inspect the client code. That does not change the commercial
terms of the hosted Attriax service or private backend repositories.