#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using AttriaxSdk = Attriax.Unity.Attriax;

namespace Attriax.Unity.Editor
{
    /// <summary>
    /// Batchmode Editor-process smoke for the desktop native engine. Unlike the
    /// standalone player, this runs with <c>Application.platform ==
    /// RuntimePlatform.WindowsEditor</c>, so it exercises the exact engine-selector
    /// branch the Editor play mode takes — proving the C-ABI shared lib loads and
    /// drives the dev API from inside the long-lived Editor process. Invoke with:
    ///
    ///   Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; \
    ///     -executeMethod Attriax.Unity.Editor.AttriaxDesktopEditorSmoke.Run
    /// </summary>
    public static class AttriaxDesktopEditorSmoke
    {
        private const string Tag = "ATTRIAX_EDITOR_SMOKE";
        private const string DevToken = "ax_4961d1f22e274281919b1b021ec2eb48";
        private const string DevApiBaseUrl = "http://localhost:33000";

        public static void Run()
        {
            Debug.Log($"[{Tag}] platform={Application.platform} constructing Attriax against {DevApiBaseUrl}");
            AttriaxSdk? attriax = null;
            try
            {
                attriax = new AttriaxSdk(new AttriaxConfig
                {
                    ProjectToken = DevToken,
                    ApiBaseUrl = DevApiBaseUrl,
                    EnableDebugLogs = true,
                });

                // Blocking is safe here: init + state-seeding run on the engine worker
                // thread and never marshal back to the Editor main thread.
                attriax.InitializeAsync(new AttriaxInitOptions { CaptureInitialUrl = true })
                    .GetAwaiter().GetResult();
                Debug.Log($"[{Tag}] Initialized. isInitialized={attriax.IsInitialized} deviceId={attriax.DeviceId}");

                attriax.Tracking.RecordEvent("unity_editor_desktop_smoke_open", new AttriaxTrackEventOptions
                {
                    EventData = new Dictionary<string, object>
                    {
                        ["source"] = "AttriaxDesktopEditorSmoke",
                        ["startedAt"] = DateTimeOffset.UtcNow.ToString("o"),
                    },
                    FlushImmediately = true,
                });
                attriax.Tracking.RecordEvent("unity_editor_desktop_smoke_event", new AttriaxTrackEventOptions
                {
                    EventData = new Dictionary<string, object> { ["n"] = 2 },
                    FlushImmediately = true,
                });
                Debug.Log($"[{Tag}] Recorded 2 events (flushImmediately); waiting for network flush...");

                // Let the engine's background flush executor complete the round-trips.
                Thread.Sleep(6000);
                Debug.Log($"[{Tag}] DONE: requests dispatched via the Editor native engine.");
            }
            catch (Exception error)
            {
                Debug.LogError($"[{Tag}] ERROR: {error}");
            }
            finally
            {
                attriax?.Dispose();
                // Give Dispose (worker-thread destroy + FreeLibrary) a moment to settle.
                Thread.Sleep(1500);
            }
        }
    }
}
