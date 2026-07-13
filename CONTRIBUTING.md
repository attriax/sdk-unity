# Contributing To The Attriax Unity SDK

This repository is the publishable Unity SDK workspace for Attriax.

## Code Of Conduct

Be respectful, direct, and constructive in review and issue discussions.

## Getting Started

1. Clone the workspace and initialize the Unity repository locally.
2. Install the root Node dependencies with `npm install` from the workspace
   root.
3. Open `sdk-unity/` in the Unity Editor version pinned in
   `ProjectSettings/ProjectVersion.txt` (currently `6000.4.6f1`; the package's
   minimum supported version is `6000.2`, set in
   `Packages/com.attriax.unity/package.json`).
4. Create a feature branch before making changes.

## Before You Commit

Run the relevant checks from the workspace root:

```bash
npm run unity:test:editor
npm run sdk:unity:validate
```

## Contribution Expectations

- The engine is native (the shared `sdk-kmp` core, or `sdk-js` for WebGL) —
  see `NATIVE_ENGINE_REWRAP.md`. Behavior changes belong there, not in this
  repo's C#; this repo only owns the platform bindings under
  `Runtime/Internal/Engine/` + `Runtime/Plugins/` and the public facade.
- Add EditMode tests for logic changes in `Runtime/Internal/`.
- Update the public sample when user-visible integration behavior changes.
- Update repository and package documentation when setup, release, or
  behavior expectations change.

## Pull Request Notes

Summaries should explain:

- what changed
- why it changed
- how it was validated
- whether device/host behavior was re-tested on the platform(s) whose native
  binding changed (Android / iOS / Windows / Linux / macOS / WebGL)

## Reporting Issues

When filing bugs, include:

- Unity version
- target platform
- steps to reproduce
- relevant console or batch log output
- whether the issue affects the Package Manager package, the exported `.unitypackage`,
  or both