# Unity SDK Client Generation — RETIRED

This workflow no longer applies. `sdk-unity` used to own a generated C# HTTP
client for the SDK-only Attriax API contract, embedded at
`Runtime/Internal/Generated/AttriaxSdkClient/` and consumed by the managed C#
engine (`AttriaxRuntime`). Both the generated client and the engine that used
it were deleted as part of the native-engine re-wrap — see
`NATIVE_ENGINE_REWRAP.md`.

## Why it's gone

The SDK is now a thin wrapper over native engines (the shared `sdk-kmp` core
for Android/iOS/desktop/macOS, `sdk-js`/`@attriax/js` for WebGL). Wire
transport — HTTP requests, the OpenAPI contract, retry/batching — is owned
entirely by those native engines now, not by C#. There is nothing left in this
repo for a generated C# HTTP client to serve, so `Runtime/Internal/Generated/`
no longer exists.

## If you're looking for the old workflow

The retired C# engine (including the generated client it depended on) is
preserved on the `backup/unity-csharp-engine-2026-07-12` branch (pushed) for
historical reference. This document is kept only so links to it don't 404 and
so anyone who finds a stale reference to `sdk:unity:generate` understands why
it no longer does anything useful for this package.

If the SDK-only API contract changes going forward, update the consumer that
actually needs it — the `sdk-kmp` core's own HTTP layer (or `sdk-js` for
WebGL) — not this repo.
