#nullable enable
using System;
using System.Collections.Generic;
using Attriax.Unity;
using UnityEngine;
// `Attriax` (the root namespace) shadows the `Attriax.Unity.Attriax` type in the
// global scope, so alias the engine type explicitly.
using AttriaxSdk = Attriax.Unity.Attriax;

/// <summary>
/// Android device-smoke component for live-verifying the Attriax Unity SDK against
/// the dev API. On a real Android player build the engine selector compiles the
/// native (KMP AAR via JNI) engine — the managed C# engine is excluded — so any
/// successful request from this component exercises the native path.
///
/// Every step logs under the distinctive <c>ATTRIAX_SMOKE</c> tag so it is trivial
/// to find in logcat; the KMP core additionally logs <c>[Attriax][LEVEL]</c> lines
/// (via System.out/System.err) when EnableDebugLogs is set — those lines are the
/// native-engine corroboration.
/// </summary>
public sealed class AttriaxSmoke : MonoBehaviour
{
    private const string Tag = "ATTRIAX_SMOKE";

    // Dev project token + dev API reached from the device via `adb reverse tcp:33000`.
    private const string DevToken = "ax_4961d1f22e274281919b1b021ec2eb48";
    private const string DevApiBaseUrl = "http://localhost:33000";

    private AttriaxSdk? _attriax;

    private async void Start()
    {
        Debug.Log($"[{Tag}] Start: constructing Attriax against {DevApiBaseUrl}");
        try
        {
            var attriax = new AttriaxSdk(new AttriaxConfig
            {
                ProjectToken = DevToken,
                ApiBaseUrl = DevApiBaseUrl,
                EnableDebugLogs = true,
            });
            _attriax = attriax;

            Debug.Log($"[{Tag}] InitializeAsync (session + app-open auto-tracked)...");
            await attriax.InitializeAsync(new AttriaxInitOptions { CaptureInitialUrl = true });
            Debug.Log($"[{Tag}] Initialized. isInitialized={attriax.IsInitialized}");

            attriax.Tracking.RecordEvent("unity_android_smoke_open", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object>
                {
                    ["source"] = "AttriaxSmoke",
                    ["startedAt"] = DateTimeOffset.UtcNow.ToString("o"),
                },
                FlushImmediately = true,
            });
            Debug.Log($"[{Tag}] RecordEvent unity_android_smoke_open (flushImmediately)");

            attriax.Tracking.RecordEvent("unity_android_smoke_event", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object> { ["n"] = 2 },
                FlushImmediately = true,
            });
            Debug.Log($"[{Tag}] RecordEvent unity_android_smoke_event (flushImmediately)");

            Debug.Log($"[{Tag}] DONE: deviceId-bearing requests dispatched via native engine.");
        }
        catch (Exception error)
        {
            Debug.LogError($"[{Tag}] ERROR: {error}");
        }
    }

    private void OnDestroy() => _attriax?.Dispose();
}
