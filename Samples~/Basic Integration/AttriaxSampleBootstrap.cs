#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attriax.Unity.Samples
{
    public sealed class AttriaxSampleBootstrap : MonoBehaviour
    {
        [SerializeField] private AttriaxBehaviour? _attriax;
        [SerializeField] private string _externalUserId = "demo-user-1";

        private void Start()
        {
            if (_attriax == null)
            {
                Debug.LogWarning("Assign an AttriaxBehaviour instance before running the sample.", this);
                return;
            }

            _attriax.DeepLinkReceived += HandleDeepLinkReceived;
            _attriax.SynchronizationChanged += HandleSynchronizationChanged;
            _ = RunSampleFlowAsync();
        }

        private async System.Threading.Tasks.Task RunSampleFlowAsync()
        {
            await _attriax.InitializeAsync();
            var tracking = _attriax.Tracking!;

            await tracking.TrackEventAsync("unity_sample_started", new AttriaxTrackEventOptions
            {
                EventData = new Dictionary<string, object>
                {
                    ["scene"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    ["timestamp"] = DateTimeOffset.UtcNow.ToString("o"),
                },
            });
            await tracking.SetUserAsync(_externalUserId, new AttriaxSetUserOptions
            {
                ExternalUserName = "Unity Sample User",
            });
            await tracking.SetUserPropertiesAsync(new Dictionary<string, object?>
            {
                ["sampleGroup"] = "unity",
                ["flowVersion"] = 1,
            });
        }

        private void HandleDeepLinkReceived(AttriaxDeepLinkEvent deepLinkEvent)
        {
            if (deepLinkEvent.Found)
            {
                Debug.Log("Resolved Attriax deep link: " + deepLinkEvent.Uri.AbsolutePath, this);
                return;
            }

            Debug.LogWarning("Attriax deep-link was unmatched: " + deepLinkEvent.Uri, this);
        }

        private void HandleSynchronizationChanged(AttriaxSynchronizationState state)
        {
            Debug.Log("Attriax synchronization state: " + state, this);
        }

        private void OnDestroy()
        {
            if (_attriax == null)
            {
                return;
            }

            _attriax.DeepLinkReceived -= HandleDeepLinkReceived;
            _attriax.SynchronizationChanged -= HandleSynchronizationChanged;
        }
    }
}