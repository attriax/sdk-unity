# Changelog

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