# Attriax Unity SDK

The Attriax Unity SDK mirrors the existing Flutter and browser runtimes:

- app-open tracking during initialization
- persistent device identity per Attriax project token
- offline queueing and synchronization state reporting
- deep-link activation and manual deep-link resolution
- custom events, standardized page-view events, and identification
- dynamic-link creation through the Attriax API

## Integration Modes

### Configured singleton

Use `Tools > Attriax > Configuration` to create `Assets/Resources/Attriax/AttriaxSettings.asset` and let the SDK manage a shared singleton host for you.

```csharp
using Attriax.Unity;

var attriax = await Attriax.InitializeConfiguredAsync();
var initialRawDeepLink = attriax.DeepLinks.RawInitialDeepLink;
var initialDeepLink = initialRawDeepLink == null
    ? await attriax.DeepLinks.WaitForInitialDeepLink
    : await attriax.DeepLinks.WaitResolutionAsync(initialRawDeepLink);
var originalInstallReferrer = await attriax.Referrer.GetOriginalInstallReferrerAsync();
var reinstallReferrer = await attriax.Referrer.GetReinstallReferrerAsync();
await attriax.Tracking.TrackEventAsync("purchase_completed");
```

This is the recommended path when you want one SDK instance that can auto-start before the first scene and stay alive across scene loads.

### Persistent component host

If you prefer a MonoBehaviour-owned runtime, add `AttriaxBehaviour` to a bootstrap GameObject in the editor or create it from code.

```csharp
using Attriax.Unity;

var host = await AttriaxBehaviour.CreateAndInitializeHostAsync(
    new AttriaxConfig
    {
        ProjectToken = "ax_your_project_token",
        EnableDebugLogs = true,
        AutomaticSceneTracking = true,
    },
    new AttriaxInitOptions
    {
        CaptureInitialUrl = true,
    },
    persistAcrossScenes: true);

var attriax = host.Instance!;
```

This gives you a `DontDestroyOnLoad` host by default while keeping the GameObject/component model that many Unity teams expect.

### Fully manual runtime

If you do not want any helper host GameObject, instantiate `Attriax` directly and own the lifecycle yourself.

```csharp
using Attriax.Unity;

var attriax = new Attriax(new AttriaxConfig
{
    ProjectToken = "ax_your_project_token",
    EnableDebugLogs = true,
});

await attriax.InitializeAsync();
```

When you do not provide overrides, the runtime reports `Application.version`, `Application.buildGUID`, and `Application.identifier` for app version, build number, and package identifier respectively. That means WebGL builds also get a per-build identifier without extra SDK configuration.

## Platform Setup

### iOS deep links

- Add your Attriax project subdomain to the app's Associated Domains entitlements.
- Verify the incoming universal link is preserved until the SDK initializes.
- If you need the first native/browser deep-link capture explicitly, read `attriax.DeepLinks.RawInitialDeepLink` and await `WaitResolutionAsync(rawEvent)` for its resolved event.

### Android app links and install referrer

- Configure Android App Links or your chosen deep-link intent filters for the same Attriax domain you use in the dashboard.
- Keep the Play Install Referrer path available in release builds. Install-referrer attribution only exists on Android and is most reliable when QA starts from a fresh install.
- Read retained startup attribution from `attriax.Referrer.GetOriginalInstallReferrerAsync()` and `attriax.Referrer.GetReinstallReferrerAsync()`.
- If a tracked install looks organic, verify both the incoming launch URL handling and that the install came from a Play-managed source.

### Firebase uninstall tracking

- Call `RegisterFirebaseMessagingTokenAsync(token)` after the SDK is initialized and whenever Firebase rotates the token.
- On Apple platforms, Firebase uninstall detection still depends on APNs being configured correctly inside Firebase so the app can receive a valid FCM registration token.
- If your native bridge already exposes the raw APNs device token, call `RegisterApplePushTokenAsync(token)` to register it with Attriax as a separate Apple token provider.
- Pass `null` or whitespace to clear the currently registered uninstall token for that device.
- Attriax uninstall-token registration currently applies to Android and iOS only.

## Deep Links And Lifecycle

- `attriax.DeepLinks.RawStream.Subscribe(...)` receives native/browser deep-link captures immediately. Await `WaitResolutionAsync(rawEvent)` when you want the resolved Attriax event for that raw input.
- `attriax.DeepLinks.Stream.Subscribe(...)` receives already-resolved `AttriaxDeepLinkEvent` values. Deferred app-open deep links only appear on this resolved stream.
- `attriax.DeepLinks.RawInitialDeepLink` exposes the first startup raw capture, while `InitialDeepLink`, `InitialDeepLinkResolved`, and `WaitForInitialDeepLink` expose the resolved startup state.
- The richer app-open snapshot stays internal in Unity now. Read public startup attribution state through `attriax.DeepLinks.WaitForInitialDeepLink`, `attriax.Referrer.GetOriginalInstallReferrerAsync()`, and `attriax.Referrer.GetReinstallReferrerAsync()`.
- The runtime always sends app-open before any other queued SDK request, and if app-open fails, later queued requests stay deferred until an app-open request succeeds.
- `ResetAsync()` clears SDK-owned persisted state and invalidates in-flight background work so a later initialization starts cleanly.
- `Dispose()` is terminal for that runtime instance. Create a new `Attriax` or `AttriaxBehaviour` instance instead of trying to reuse a disposed one.

## GDPR Consent And Anonymous Analytics

Set `GdprEnabled = true` when the Unity runtime should gate GDPR-regulated tracking. Anonymous-capable event, crash, session, and deep-link traffic still sends immediately while consent is `Unknown` or `Pending`, while attribution-only work stays withheld until consent allows it. Drive your custom privacy UI from `attriax.Consent.Gdpr.State`, `Values`, and `IsWaitingForConsent`, check local or remote requirements with `NeedsConsentAsync(...)`, and persist choices with `SetConsent(...)`, `SetNotRequired()`, or `Reset()`. Use `RequestDataErasureAsync()` when the user asks the SDK to erase its GDPR-owned backend data and local runtime state.

While consent is `Unknown` or `Pending`, the Unity runtime keeps consent-only persistence locally and sends anonymous-capable traffic without Attriax device identity. Once consent is resolved, denied analytics-capable traffic continues without device identity and is bound server-side to anonymous sessions. If GDPR resolves as `NotRequired`, future analytics-capable requests regain device identity before dispatch.

See [Documentation~/gdpr-and-anonymous-analytics.md](Documentation~/gdpr-and-anonymous-analytics.md) for the shared SDK behavior and backend storage model.

## Persistence And Security Notes

- Queue state, session state, the cached raw install referrer, retained original/reinstall referrer details, and most runtime flags are persisted in `PlayerPrefs`.
- Treat that local state as inspectable application data rather than confidential storage.
- Device identity uses platform-specific persistence where available, but custom event metadata and queued payloads should still avoid secrets.

## Manual QA Checklist

- Fresh install from a tracked link records app-open and resolves the expected initial deep link.
- Warm-start raw links arrive through `DeepLinks.RawStream`, and `WaitResolutionAsync(...)` returns the corresponding resolved event.
- Reinstall and app-data-clear paths return the expected `InstallState` and retained original/reinstall referrer values.
- Warm-start deep links arrive through `DeepLinks.Stream` without requiring a reinstall.
- Offline events flush after connectivity returns.
- `RegisterFirebaseMessagingTokenAsync(...)` succeeds on supported mobile platforms with a real FCM token.
- `RegisterApplePushTokenAsync(...)` succeeds on Apple platforms when the app can surface the native APNs device token.
- `Tracking.RecordErrorAsync(...)` and `ValidateReceiptAsync(...)` both reach the expected backend environment.

## Troubleshooting

- No deep link resolved: verify the Attriax domain, mobile entitlements or intent filters, and that the full incoming URL reaches the app before SDK startup completes.
- No install referrer data on Android: test from a fresh install path, check `Referrer.GetOriginalInstallReferrerAsync()` and `Referrer.GetReinstallReferrerAsync()`, and verify the build still includes the Play Install Referrer integration.
- Uninstall token registration does nothing: confirm Firebase Messaging is set up correctly and that you are running on Android or iOS. APNs-only registration is stored by Attriax, but current uninstall probing still uses Firebase Admin and FCM tokens.
- State seems to survive editor or app restarts unexpectedly: clear `PlayerPrefs` for the current package/app identifier before rerunning attribution QA.

## Obfuscation And Stripping

The runtime assembly ships with assembly-level protection for common Unity pipelines:

- `Obfuscation(Exclude = true, ApplyToMembers = true)` to tell compatible C# obfuscators not to rename the SDK assembly.
- `AlwaysLinkAssembly` to keep the runtime assembly linked even when Unity stripping is aggressive.

If your obfuscator ignores assembly-level attributes, exclude `Attriax.Runtime` from renaming. At minimum, keep these symbols stable:

- `Attriax`
- `AttriaxBehaviour`
- `AttriaxProjectSettings`
- `AttriaxConfiguredHost`

Those types cover the public runtime entry point, the persistent MonoBehaviour host, the Resources-backed settings asset, and the configured singleton bootstrap path.

See the included sample scripts for MonoBehaviour-hosted startup,
configured-singleton startup, fully manual runtime ownership, and runtime
diagnostics.

## License

Apache-2.0. See `LICENSE`.