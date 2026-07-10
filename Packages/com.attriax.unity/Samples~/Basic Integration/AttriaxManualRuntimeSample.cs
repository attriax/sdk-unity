#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attriax.Unity.Samples
{
    public sealed class AttriaxManualRuntimeSample : MonoBehaviour
    {
        [SerializeField] private string _projectToken = "ax_your_project_token";
        [SerializeField] private string _apiBaseUrl = string.Empty;
        [SerializeField] private bool _enableDebugLogs = true;

        private Attriax? _attriax;
        private IDisposable? _deepLinkSubscription;
        private IDisposable? _synchronizationSubscription;

        private async void Start()
        {
            try
            {
                var attriax = new Attriax(new AttriaxConfig
                {
                    ProjectToken = _projectToken,
                    ApiBaseUrl = string.IsNullOrWhiteSpace(_apiBaseUrl) ? null : _apiBaseUrl,
                    EnableDebugLogs = _enableDebugLogs,
                });
                _attriax = attriax;

                _synchronizationSubscription = attriax.Synchronization.Subscribe(
                    state => Debug.Log("Manual Attriax synchronization state: " + state, this));
                _deepLinkSubscription = attriax.DeepLinks.Stream.Subscribe(HandleDeepLinkEvent);

                await attriax.InitializeAsync(new AttriaxInitOptions
                {
                    CaptureInitialUrl = true,
                });

                await attriax.Tracking.TrackEventAsync("unity_manual_runtime_started", new AttriaxTrackEventOptions
                {
                    EventData = new Dictionary<string, object>
                    {
                        ["scene"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                        ["startedAt"] = DateTimeOffset.UtcNow.ToString("o"),
                    },
                });
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
        }

        private void HandleDeepLinkEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            if (deepLinkEvent.Found)
            {
                Debug.Log("Manual Attriax deep link: " + deepLinkEvent.Uri.AbsolutePath, this);
                return;
            }

            Debug.LogWarning("Manual Attriax deep-link was unmatched: " + deepLinkEvent.Uri, this);
        }

        private void OnDestroy()
        {
            _deepLinkSubscription?.Dispose();
            _synchronizationSubscription?.Dispose();
            _attriax?.Dispose();
        }
    }
}