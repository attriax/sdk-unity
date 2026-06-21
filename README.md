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

- Call `attriax.Tracking.RecordError(...)` when you want to report a handled
  exception with extra metadata.
- Unhandled Unity exceptions observed through `Application.logMessageReceivedThreaded`
  are captured automatically and queued for delivery when possible.
- Crash payloads include app, device, and session context so dashboard crash
  analytics can be reviewed next to attribution data.

## Push Notification Attribution

Attriax never sends pushes itself. The host app keeps its existing push stack
(Firebase Cloud Messaging, APNs, Unity Mobile Notifications, or any push plugin)
and *forwards* notification lifecycle events to the SDK from its own handlers.
This is manual forwarding by design: the SDK coexists with whatever notification
plugin you already ship, and never registers competing message receivers.

The `Tracking` facade exposes one general method plus three lifecycle helpers:

```csharp
attriax.Tracking.RecordNotification(type, notificationId, options);   // general
attriax.Tracking.RecordNotificationReceived(notificationId, options); // received
attriax.Tracking.RecordNotificationOpened(notificationId, options);   // opened
attriax.Tracking.RecordNotificationDismissed(notificationId, options);// dismissed
```

The three lifecycle types map to `AttriaxNotificationEventType`:

| Type        | When to call it                              | What it measures                                                                 |
| ----------- | -------------------------------------------- | -------------------------------------------------------------------------------- |
| `Received`  | Push arrives / is displayed on the device    | Deliverability — how many pushes actually reached the device.                    |
| `Opened`    | User taps the notification                   | High-value re-engagement attribution — ties downstream conversions/revenue back. |
| `Dismissed` | User swipes the notification away (see below)| Best-effort negative signal — the message was seen but ignored.                  |

`AttriaxRecordNotificationOptions` carries the attribution context:

- `LinkId` / `CampaignId` — references to an existing Attriax tracked link or
  campaign, threaded through from your notification payload.
- `Title` — optional human-readable notification title.
- `Source` — `Fcm`, `Apns`, or `Other`. Leave it `null` and pass `Payload`, and
  the source is **inferred from the payload shape** (an `aps` envelope means APNs,
  a `google.*` / `gcm.*` key means FCM); otherwise the server falls back to `other`.
- `Payload` — the raw FCM/APNs data map. It is preserved under a `payload` key in
  the notification metadata so attribution context survives the trip to the server.
- `Metadata` / `FlushImmediately` — extra metadata and immediate-flush control,
  exactly like `RecordEvent`.

Notification events route through the same offline-persisted, batched, retried
queue as `RecordEvent`, and honor the same app-open-first and GDPR-consent
semantics.

### Where to call it — opened (tap)

Call `RecordNotificationOpened` from the same handler where you route the user
after they tap a push. Pull `linkId` / `campaignId` out of the payload your
backend attached when it built the notification:

```csharp
using System.Collections.Generic;
using Attriax.Unity;

// Invoked from your FCM/APNs tap handler (or Unity Mobile Notifications, or a push plugin).
public void OnNotificationTapped(IDictionary<string, object> payload)
{
    var notificationId = payload.TryGetValue("notification_id", out var id)
        ? id?.ToString()
        : System.Guid.NewGuid().ToString();

    attriax.Tracking.RecordNotificationOpened(
        notificationId,
        new AttriaxRecordNotificationOptions
        {
            LinkId = payload.TryGetValue("ax_link_id", out var link) ? link?.ToString() : null,
            CampaignId = payload.TryGetValue("ax_campaign_id", out var camp) ? camp?.ToString() : null,
            Title = payload.TryGetValue("title", out var title) ? title?.ToString() : null,
            Payload = payload,   // source inferred from the payload shape (aps -> apns, google.* -> fcm)
        });

    // ...then deep-link / route the user as you already do.
}
```

### Where to call it — received (delivery)

Call `RecordNotificationReceived` from your background/foreground message handler
when a push is delivered, so deliverability is measured independently of whether
the user opens it:

```csharp
// Invoked from your FCM onMessage / APNs willPresent (foreground) handler.
public void OnNotificationReceived(IDictionary<string, object> payload)
{
    var notificationId = payload.TryGetValue("notification_id", out var id)
        ? id?.ToString()
        : System.Guid.NewGuid().ToString();

    attriax.Tracking.RecordNotificationReceived(
        notificationId,
        new AttriaxRecordNotificationOptions
        {
            LinkId = payload.TryGetValue("ax_link_id", out var link) ? link?.ToString() : null,
            CampaignId = payload.TryGetValue("ax_campaign_id", out var camp) ? camp?.ToString() : null,
            Source = AttriaxNotificationEventSource.Fcm, // or leave null and let Payload infer it
            Payload = payload,
        });
}
```

If you bootstrap through `AttriaxBehaviour` instead of holding the instance
directly, the same calls work through its nullable facade:
`behaviour.Tracking?.RecordNotificationOpened(notificationId, options);`.

### The `dismissed` caveat

`RecordNotificationDismissed` is a **best-effort** negative signal and is only
observable when **your app builds the notification itself** and wires up a
dismiss callback:

- **Android** — only when you post the notification locally and set a
  `deleteIntent` (the swipe-away / "delete" intent). OS-displayed messages built
  from a remote `notification` payload do not deliver a dismiss callback to your
  app.
- **iOS** — only via a custom notification-category dismiss action the user
  explicitly triggers; the system does not report a plain swipe-away, and nothing
  is delivered while the app is terminated.

So treat `dismissed` as opportunistic: record it where the platform gives you a
real dismiss callback, and never assume "not opened" equals "dismissed." `Received`
and `Opened` are the reliable signals.

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
