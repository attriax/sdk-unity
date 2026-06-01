# Basic Integration Sample

This sample folder now demonstrates three public Unity integration paths:

- `AttriaxSampleBootstrap.cs` — a MonoBehaviour-owned `AttriaxBehaviour`
  happy-path bootstrap that tracks an event and applies user data.
- `AttriaxConfiguredSingletonSample.cs` — the configured-singleton flow driven
  by `Tools > Attriax > Configuration` and `Attriax.InitializeConfiguredAsync()`.
- `AttriaxManualRuntimeSample.cs` — a fully manual `new Attriax(...)` flow for
  teams that do not want a helper host object.
- `AttriaxSampleStatusPanel.cs` — inspector-friendly runtime diagnostics for
  synchronization state, startup status, and deep-link results.

Recommended setup:

1. Import the sample through Unity Package Manager.
2. Choose either the `AttriaxBehaviour` flow or the configured singleton flow.
3. Add `AttriaxSampleStatusPanel` to a scene object so you can inspect runtime
   state while testing deep links, initialization, and synchronization.