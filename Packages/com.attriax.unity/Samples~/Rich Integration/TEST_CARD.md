# Attriax Unity — Rich Integration Test Card

A manual pre-publish checklist for the **Rich Integration** sample. Import the sample
(Package Manager → Attriax Unity SDK → Samples → Rich Integration), open
`AttriaxRichSample.unity`, and set the project token + API base URL on the
`AttriaxRichSampleController` in the scene (default dev API: `http://localhost:33000`).

**Engine note:** in the **Editor** and **Windows/Linux standalone** the sample runs the
**native desktop engine** (KMP C-ABI, dynamically loaded). On **Android/iOS** it runs the
native AAR / XCFramework engine. To reach a local dev API from an Android device, run
`adb reverse tcp:33000 tcp:33000` and use `http://localhost:33000`.

Each check lists the action and the expected result.

| # | Check | Expected |
|---|-------|----------|
| 1 | **Init → deviceId.** Enter Play mode / launch. | `IsInitialized == true`, a non-null `DeviceId` shown, synchronization reaches `Synchronized`. |
| 2 | **Event → 201.** Tap *Record custom event* (with properties). | Dev API `POST /api/sdk/v1/events` (or `/batch`) → 201; event appears in the Activity log. |
| 3 | **Revenue.** Record a purchase, then a refund. | Both accepted (201); amount/currency echoed in the log. |
| 4 | **Consent gate.** With `gdprEnabled: true`, try to track before consent, then grant. | Tracking is withheld while `IsWaitingForConsent`; after `SetConsent(analytics:true,…)` the queued events flush and new events send. |
| 5 | **Deep link.** Fire an initial + a runtime deep link (or create then open a dynamic link). | Raw stream emits; `WaitResolutionAsync` returns found; the result is shown. |
| 6 | **Disable / enable.** Toggle `Enabled = false`, fire an event, re-enable. | No network while disabled; tracking + deep-link handling resume after re-enable. |
| 7 | **Reset.** Invoke `ResetAsync()`. | Local identity/consent/queue cleared; next init behaves as a fresh install. |
| 8 | **Offline queue → flush.** Go offline, fire ≥2 events, come back online, `Flush()`. | Events persist across the offline window and all deliver (201) after reconnect. |
| 9 | **Session continuity.** Background then foreground the app (or navigate observed scenes). | App-open/session fires once per resume; sync state stays consistent (no duplicate app-open). |

## Unity-only extras (these APIs exist only on Unity — skip on js/RN)

| # | Check | Expected |
|---|-------|----------|
| A | **SKAN (iOS).** Tap *Update conversion value*. | `Skan.UpdateConversionValueAsync` returns a non-error status on a real iOS device (no-op elsewhere). |
| B | **ATT (iOS).** Tap *Request tracking authorization*. | The ATT prompt fires on-device; `GetTrackingAuthorizationStatusAsync` returns the chosen status. |
| C | **Push-token registration.** Tap *Register push token* (needs Firebase / APNs configured). | Registration succeeds when a token is available; safely no-ops otherwise. |
| D | **Data erasure (GDPR).** Tap *Request data erasure*. | `Consent.Gdpr.RequestDataErasureAsync` is accepted by the API. |

> CCPA / do-not-sell is intentionally **not** in this sample — the Unity public facade
> doesn't expose a `Consent.Ccpa` surface yet (types/bridge only).
