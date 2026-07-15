#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity;
using UnityEngine;
// `Attriax` (the root namespace) shadows the `Attriax.Unity.Attriax` type in the
// global scope, so alias the engine type explicitly.
using AttriaxSdk = Attriax.Unity.Attriax;

/// <summary>
/// Interactive Attriax demo: a tiny tap-to-score game wired to the real SDK, so the
/// engine can be exercised on-device by hand instead of read out of a log.
///
/// Everything is drawn with IMGUI (<see cref="OnGUI"/>) on purpose: it needs no Canvas,
/// EventSystem, TextMeshPro assets or prefabs, so the whole scene can be generated from
/// a batchmode build script and still render a real, tappable UI.
///
/// The live status panel (initialized / deviceId / synchronization state) is the point:
/// `Synchronized` appearing on screen is the SDK confirming the backend accepted
/// `/open` + `/batch`.
/// </summary>
public sealed class AttriaxDemo : MonoBehaviour
{
    private const string Tag = "ATTRIAX_DEMO";
    private const string ProjectToken = "ax_b62ee57056374b76aa09b26fa071e561";
    private const string ApiBaseUrl = "https://api.attriax.com";
    private const int LevelUpEvery = 5;

    private AttriaxSdk? _attriax;
    private bool _ready;
    private string _bootError = "";

    // Game state.
    private int _score;
    private int _level = 1;
    private int _coins;

    // SDK state mirrored for the status panel.
    private string _deviceId = "…";
    private AttriaxSynchronizationState _sync = AttriaxSynchronizationState.Initializing;
    private string _attStatus = "unknown";
    private bool? _doNotSell;

    private readonly List<string> _log = new List<string>();
    private Vector2 _logScroll;
    private GUIStyle? _title, _label, _button, _statusValue, _logStyle;

    private async void Start()
    {
        Application.targetFrameRate = 60;
        Log("booting SDK → " + ApiBaseUrl);
        try
        {
            var attriax = new AttriaxSdk(new AttriaxConfig
            {
                ProjectToken = ProjectToken,
                ApiBaseUrl = ApiBaseUrl,
                EnableDebugLogs = true,
            });
            _attriax = attriax;

            // Mirror engine sync transitions into the panel. This is the assertion the
            // whole demo exists for: it only reaches Synchronized once the queue drains.
            attriax.Synchronization.Subscribe(state =>
            {
                _sync = state;
                Log("sync → " + state);
            });

            await attriax.InitializeAsync(new AttriaxInitOptions { CaptureInitialUrl = true });

            _ready = true;
            _sync = attriax.Synchronization.State;
            _deviceId = attriax.DeviceId ?? "(null)";
            Log("initialized. deviceId=" + _deviceId);

            attriax.Tracking.RecordEvent("demo_app_start", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object> { ["surface"] = "unity_demo" },
                FlushImmediately = true,
            });
            Log("event demo_app_start sent");

            _attStatus = (await attriax.Consent.Att.GetTrackingAuthorizationStatusAsync()).ToString();
            _doNotSell = await attriax.Consent.Ccpa.GetDoNotSellAsync();
        }
        catch (Exception error)
        {
            _bootError = error.Message;
            Log("ERROR " + error);
            Debug.LogError($"[{Tag}] init failed: {error}");
        }
    }

    // ---------------------------------------------------------------- game

    private void Tap()
    {
        _score++;
        var sdk = _attriax;
        if (sdk == null)
        {
            return;
        }

        if (_score % LevelUpEvery == 0)
        {
            _level++;
            sdk.Tracking.RecordEvent("level_complete", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object>
                {
                    ["level"] = _level - 1,
                    ["score"] = _score,
                },
                FlushImmediately = true,
            });
            Log($"level {_level - 1} complete → event sent (score={_score})");
        }
    }

    private void BuyCoins()
    {
        var sdk = _attriax;
        if (sdk == null)
        {
            return;
        }

        _coins += 500;
        sdk.Tracking.RecordPurchase(4.99, new AttriaxRecordPurchaseOptions
        {
            Currency = "USD",
            PurchaseType = "one_time",
            ProductId = "coins_500",
            TransactionId = Guid.NewGuid().ToString(),
        });
        Log("purchase $4.99 (coins_500) → revenue event sent");
    }

    private async void RequestAtt()
    {
        var sdk = _attriax;
        if (sdk == null)
        {
            return;
        }

        Log("requesting ATT authorization…");
        try
        {
            var status = await sdk.Consent.Att.RequestTrackingAuthorizationAsync();
            _attStatus = status.ToString();
            Log("ATT → " + status);
        }
        catch (Exception e)
        {
            Log("ATT error: " + e.Message);
        }
    }

    private async void ToggleDoNotSell()
    {
        var sdk = _attriax;
        if (sdk == null)
        {
            return;
        }

        var next = !(_doNotSell ?? false);
        await sdk.Consent.Ccpa.SetDoNotSellAsync(next);
        _doNotSell = await sdk.Consent.Ccpa.GetDoNotSellAsync();
        Log("CCPA doNotSell → " + _doNotSell);
    }

    private void SendCustomEvent()
    {
        var sdk = _attriax;
        if (sdk == null)
        {
            return;
        }

        sdk.Tracking.SetUser("demo_user_1");
        sdk.Tracking.SetUserProperty("favourite_level", _level);
        sdk.Tracking.RecordEvent("custom_demo_event", new AttriaxTrackEventOptions
        {
            EventData = new Dictionary<string, object>
            {
                ["score"] = _score,
                ["coins"] = _coins,
                ["at"] = DateTimeOffset.UtcNow.ToString("o"),
            },
            FlushImmediately = true,
        });
        Log("setUser + custom_demo_event sent");
    }

    private void Log(string line)
    {
        var stamp = DateTime.Now.ToString("HH:mm:ss");
        _log.Insert(0, $"[{stamp}] {line}");
        if (_log.Count > 40)
        {
            _log.RemoveAt(_log.Count - 1);
        }

        Debug.Log($"[{Tag}] {line}");
    }

    // ----------------------------------------------------------------- UI

    private void EnsureStyles()
    {
        if (_title != null)
        {
            return;
        }

        // Scale for retina phones — IMGUI's default
        // point sizes are unreadable at native resolution.
        var s = Mathf.Max(2, Screen.width / 380);
        _title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11 * s, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
        };
        _label = new GUIStyle(GUI.skin.label) { fontSize = 7 * s };
        _statusValue = new GUIStyle(GUI.skin.label) { fontSize = 7 * s, fontStyle = FontStyle.Bold };
        _button = new GUIStyle(GUI.skin.button) { fontSize = 8 * s, fontStyle = FontStyle.Bold };
        _logStyle = new GUIStyle(GUI.skin.label) { fontSize = 5 * s, wordWrap = false };
    }

    private void OnGUI()
    {
        EnsureStyles();
        var pad = Screen.width * 0.05f;
        var w = Screen.width - (pad * 2);
        var btnH = Screen.height * 0.07f;

        GUILayout.BeginArea(new Rect(pad, pad * 1.4f, w, Screen.height - (pad * 2.4f)));

        GUILayout.Label("Attriax Unity Demo", _title);
        GUILayout.Space(pad * 0.3f);

        // ---- status panel (the real assertion) ----
        GUILayout.BeginVertical(GUI.skin.box);
        Row("SDK", _ready ? "initialized ✓" : (_bootError.Length > 0 ? "FAILED" : "starting…"),
            _ready ? Color.green : (_bootError.Length > 0 ? Color.red : Color.yellow));
        Row("sync", _sync.ToString(),
            _sync == AttriaxSynchronizationState.Synchronized ? Color.green :
            _sync == AttriaxSynchronizationState.Failed || _sync == AttriaxSynchronizationState.Offline
                ? Color.red : Color.yellow);
        Row("deviceId", _deviceId, Color.white);
        Row("ATT", _attStatus, Color.white);
        Row("CCPA doNotSell", _doNotSell?.ToString() ?? "unset", Color.white);
        if (_bootError.Length > 0)
        {
            GUILayout.Label(_bootError, _label);
        }

        GUILayout.EndVertical();

        // ---- the "game" ----
        GUILayout.Space(pad * 0.3f);
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label($"Score {_score}", _statusValue);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Level {_level}", _statusValue);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Coins {_coins}", _statusValue);
        GUILayout.EndHorizontal();

        GUILayout.Space(pad * 0.2f);
        if (GUILayout.Button($"TAP TO SCORE  (+1)\nevery {LevelUpEvery} taps → level_complete event",
                _button, GUILayout.Height(btnH * 1.6f)))
        {
            Tap();
        }

        GUILayout.Space(pad * 0.2f);
        if (GUILayout.Button("Buy 500 coins — $4.99  (revenue event)", _button, GUILayout.Height(btnH)))
        {
            BuyCoins();
        }

        if (GUILayout.Button("Send custom event + setUser", _button, GUILayout.Height(btnH)))
        {
            SendCustomEvent();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Request ATT", _button, GUILayout.Height(btnH)))
        {
            RequestAtt();
        }

        if (GUILayout.Button("Toggle CCPA", _button, GUILayout.Height(btnH)))
        {
            ToggleDoNotSell();
        }

        GUILayout.EndHorizontal();

        // ---- live log ----
        GUILayout.Space(pad * 0.2f);
        GUILayout.Label("Activity", _label);
        _logScroll = GUILayout.BeginScrollView(_logScroll, GUI.skin.box);
        foreach (var line in _log)
        {
            GUILayout.Label(line, _logStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void Row(string name, string value, Color color)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, _label, GUILayout.Width(Screen.width * 0.34f));
        var prev = GUI.color;
        GUI.color = color;
        GUILayout.Label(value, _statusValue);
        GUI.color = prev;
        GUILayout.EndHorizontal();
    }

    private void OnDestroy()
    {
        _attriax?.Dispose();
    }
}
