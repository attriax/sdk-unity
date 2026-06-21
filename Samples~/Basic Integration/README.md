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

## Push notification attribution

Once the runtime is initialized, forward push notification lifecycle events to
the SDK from your own FCM/APNs (or Unity Mobile Notifications / push plugin)
handlers — Attriax never sends pushes itself:

```csharp
// From your notification tap handler — high-value re-engagement attribution.
attriax.Tracking.RecordNotificationOpened(
    notificationId,
    new AttriaxRecordNotificationOptions
    {
        LinkId = "lnk_promo_winback",       // pulled from the push payload
        CampaignId = "cmp_summer_sale",
        Title = "Your boost is waiting",
        Payload = payload,                  // source inferred: aps -> apns, google.* -> fcm
    });

// From your message-received handler — deliverability.
attriax.Tracking.RecordNotificationReceived(notificationId, options);

// Best-effort negative signal — only when your app builds the notification
// itself and wires a dismiss callback (Android deleteIntent / iOS custom action).
attriax.Tracking.RecordNotificationDismissed(notificationId, options);
```

The `received` / `opened` / `dismissed` types, the `source` inference, and the
`dismissed` caveat are documented in full in the package
[README "Push Notification Attribution" section](../../README.md#push-notification-attribution).