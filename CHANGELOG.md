# Changelog

## Unreleased

- Raise the batch body-size cap from 48 KB to 256 KB per request to match the higher `/api/sdk/v1/batch` limit; oversized-batch splitting and single-item drop behavior are unchanged.

## 0.5.0

- Breaking: tracking event methods are now fire-and-forget and return `void` instead of `Task`. `TrackEventAsync`, `RecordPurchaseAsync`, `RecordRefundAsync`, `RecordAdRevenueAsync`, `RecordAdEventAsync`, `RecordErrorAsync`, `TrackPageViewAsync`, `SetUserAsync`, `SetUserPropertyAsync`, `SetUserPropertiesAsync`, and `ClearUserPropertiesAsync` become `RecordEvent`, `RecordPurchase`, `RecordRefund`, `RecordAdRevenue`, `RecordAdEvent`, `RecordError`, `RecordPageView`, `SetUser`, `SetUserProperty`, `SetUserProperties`, and `ClearUserProperties`, queuing in the background and logging failures instead of surfacing them.
- Breaking: rename `Tracking.IdentifyAsync(...)` to `Tracking.Identify(...)` (obsolete-aliased), with `Tracking.SetUser(...)` as the primary call.
- Breaking: move `RegisterFirebaseMessagingTokenAsync(...)` and `RegisterApplePushTokenAsync(...)` from the top-level `Attriax` instance onto the `Tracking` API.
- Breaking: simplify `AttriaxValidateReceiptOptions` to a single required `Receipt` payload plus `Provider`, `Environment`, `TransactionId`, `ProductId`, and `Test`, removing `OriginalTransactionId`, `Store`, `PackageName`, `PurchaseToken`, `ReceiptData`, `SignedPayload`, and `ReceiptSignature`; `ValidateReceiptAsync(...)` now throws when the receipt is empty.
- Breaking: require a non-empty `Uri` on `AttriaxDeepLinkConversionOptions` and remove the separate `LinkPath` field.
- Breaking: remove the `AppToken` alias from `AttriaxConfig` in favor of `ProjectToken`.
- Breaking: remove the `Enabled` and `EventsEnabled` overrides from `AttriaxInitOptions`; runtime dispatch is still controlled by `Tracking.Enabled`.
- Add `Referrer.GetSessionReferrerAsync()` and `Referrer.GetLatestDeepLinkReferrerAsync()`, returning the new `AttriaxDeepLinkReferrerDetails` for the deep-link referrer that opened or was last seen in the current session.
- Add the `AttriaxAnalyticsEventKeys` constants for standardized event names.
- Reuse an existing live runtime for the same project token instead of constructing a second one: `AttriaxBehaviour` now adopts a configured singleton or an already-created instance and no longer disposes an externally owned runtime.
- Marshal outbound requests onto the Unity main thread through a `SynchronizationContext`-based dispatcher so tracking calls are safe to invoke from background threads.
- Replace the fixed retry delay with a capped, jittered exponential backoff (2s base, doubling to a 5-minute cap) while still honoring server `Retry-After`, keeping the 8-attempt / 7-day terminal-drop limits and exempting deep-link resolution requests.
- Cap event batches at 100 items and ~48 KB per request when packing the queue.
- Clamp the session-continuation window to between 60 seconds and 30 minutes (twice the heartbeat) so short first-launch heartbeats no longer collapse sessions on brief backgrounding.
- Throw `ArgumentNullException` when constructing `Attriax` with a null config.

## 0.4.1

- Breaking: remove the deprecated Unity GDPR auto-detection toggle.
- Breaking: the SDK no longer runs timezone auto-detection automatically during init.
- Update runtime wrappers, consent-manager tests, and package docs to remove the deprecated config toggle.

## 0.4.0

- Add `AttriaxConfig.AnonymousTracking` and `attriax.Tracking.AnonymousTrackingEnabled` so anonymous-capable GDPR traffic can keep flowing or stay buffered until consent resolves.
- Add `attriax.Consent.Gdpr.RequestDataErasureAsync()` and refresh the package docs around GDPR-owned backend data erasure and anonymous analytics.
- Keep unresolved GDPR traffic on the anonymous path by default without materializing device identity, while attribution-only work remains withheld until consent allows it.
- Persist the first-launch marker separately from full runtime state so unresolved-consent relaunches are not treated as first launch.
- Remove the unused dynamic-link `PreviewImagePath` contract fields to align the Unity package with the shared SDK contract.
- Expand EditMode and public-surface coverage around runtime activation, bootstrap, request-factory metadata, consent management, and grouped tracking APIs.

## 0.3.0

- Added the regulation-scoped `attriax.Consent.Gdpr` API with consent checks, explicit category decisions, `SetNotRequired()`, and `Reset()`.
- Added GDPR-aware pending dispatch gates, anonymous analytics-capable delivery for denied analytics paths, and attribution withholding until attribution consent or not-required status.
- Kept GDPR consent check/write requests consent-id-only in the generated Unity SDK client.
- Reattached device identity to queued analytics-capable requests when GDPR resolves as not required.
- Refreshed package-local GDPR documentation and regenerated release-facing Unity package metadata for 0.3.0.

## 0.2.0

- Added runtime SKAN support, including generated SKAN contract models and the new `AttriaxSkanManager` flow.
- Added backend-driven browser handling for resolved deep links, including Android and iOS native bridge updates for in-app browser actions.
- Improved app-open request handling in the runtime pipeline and expanded EditMode coverage around app-open and SKAN behavior.
- Refreshed the exported `.unitypackage` artifact and release validation outputs for the updated runtime.

## 0.1.0

- Added reinstall-aware startup attribution with retained original and latest reinstall referrer details.
- Added `InstallState`, `OriginalInstallReferrer`, and `ReinstallReferrer` to the public app-open snapshot.
- Added the `Referrer` facade and deprecated the single `InstallReferrer` alias.
- Changed deep-link handling to split raw native/browser captures from resolved Attriax events via `RawStream`, `RawInitialDeepLink`, and `WaitResolutionAsync(...)`.
- Regenerated the embedded SDK client and started sending Play install-referrer timing metadata in open requests.
- Updated the Unity samples and package documentation for the 0.1.0 startup flow.

## 0.0.1

- First public Unity SDK repository release.
- Embedded generated SDK client and validated `.unitypackage` export flow.
- Apache-2.0 licensing and public release documentation.