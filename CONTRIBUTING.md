# Contributing To The Attriax Unity SDK

This repository is the publishable Unity SDK workspace for Attriax.

## Code Of Conduct

Be respectful, direct, and constructive in review and issue discussions.

## Getting Started

1. Clone the workspace and initialize the Unity repository locally.
2. Install the root Node dependencies with `npm install` from the workspace
   root.
3. Open `sdk-unity/` in Unity `6000.2.7f2` unless the repository documents a
   newer required version.
4. Create a feature branch before making changes.

## Before You Commit

Run the relevant checks from the workspace root:

```bash
npm run unity:test:editor
npm run sdk:unity:validate
```

When contract-driven transport code changed, run this instead:

```bash
npm run sdk:unity:generate
```

## Contribution Expectations

- Keep public runtime behavior aligned with `sdk-flutter/` unless a
  Unity-specific divergence is deliberate and documented.
- Add EditMode tests for logic changes in `Runtime/Internal/`.
- Update the public sample when user-visible integration behavior changes.
- Update both repository and package documentation when setup, release, or
  behavior expectations change.
- Keep generated files under
  `Packages/com.attriax.unity/Runtime/Internal/Generated/AttriaxSdkClient/`
  generated; do not hand-edit them unless you are fixing generation tooling.

## Pull Request Notes

Summaries should explain:

- what changed
- why it changed
- how it was validated
- whether Android/iOS behavior was re-tested when native code changed

## Reporting Issues

When filing bugs, include:

- Unity version
- target platform
- steps to reproduce
- relevant console or batch log output
- whether the issue affects the embedded package, the exported `.unitypackage`,
  or both