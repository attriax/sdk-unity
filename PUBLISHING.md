# Attriax Unity Publishing Notes

This document covers the manual release workflow for the Attriax Unity SDK.

The Unity repository ships two release surfaces:

- the repository-root UPM entrypoint used by Package Manager Git URL installs
- the embedded UPM package under `Packages/com.attriax.unity/`
- the exported `.unitypackage` artifact under `sdk-unity/dist/`

Publishing remains manual. CI may validate the repository in the future, but it
must not publish Unity artifacts automatically.

## Release Inputs

- `sdk-unity/package.json` must mirror the publishable package metadata needed
  for Git URL installs from the repository root.
- `Packages/com.attriax.unity/package.json` is the source of truth for the
  package version.
- `sdk-unity/CHANGELOG.md` tracks repository-level release history.
- `Packages/com.attriax.unity/CHANGELOG.md` tracks the publishable package
  release history.
- `Packages/com.attriax.unity/package.json` metadata URLs must resolve before a
  public release.

## Required Release Checks

Run these commands from the workspace root before every Unity release:

```bash
npm install
npm run unity:release:check
```

`npm run unity:release:check` runs the Unity EditMode tests and then exports
`dist/Attriax.Unity.unitypackage` through the supported batch export flow.

If the SDK API contract or generated runtime client changed, refresh that first:

```bash
npm run sdk:unity:generate
```

Set `UNITY_EDITOR_PATH` when the Unity editor is installed somewhere other than
`C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe`.

Run the release flow from a clean git state so version changes, generated
artifacts, and release notes stay easy to review.

## Release Steps

1. Ensure the intended public repository is reachable and that the package
  metadata URLs in both package manifests resolve.
2. Update `sdk-unity/package.json`, `Packages/com.attriax.unity/package.json`,
  and both changelogs for the new version.
3. Run `npm run sdk:unity:generate` if the SDK contract or generated client
   changed.
4. Run `npm run unity:release:check`.
5. Review `dist/Attriax.Unity.unitypackage` and import it into a clean Unity
   project if the release touched the runtime, package structure, or native
   plugins.
6. Tag and publish the release manually through the repository hosting flow.

## Platform Checklist

- Android: verify app-open tracking, install referrer handling, deep-link flows,
  and Firebase uninstall-token registration after any native bridge change.
- iOS: verify app-open tracking, deep-link flows, and Firebase uninstall-token
  registration after any native bridge change.
- Editor and shared-runtime platforms: re-check persistence, queue repair,
  synchronization state reporting, and manual deep-link handling after runtime
  changes.

## Packaging Note

The publishable Unity package is Apache-2.0 so integrators and security
reviewers can inspect the client code. That does not change the commercial
terms of the hosted Attriax service or private backend repositories.