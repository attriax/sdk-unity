#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attriax.Unity.Samples
{
    public sealed class AttriaxConfiguredSingletonSample : MonoBehaviour
    {
        [SerializeField] private string _eventName = "unity_configured_singleton_started";
        [SerializeField] private bool _trackEventOnStart = true;

        private IDisposable? _deepLinkSubscription;
        private IDisposable? _synchronizationSubscription;

        private async void Start()
        {
            try
            {
                var attriax = await Attriax.InitializeConfiguredAsync();
                _synchronizationSubscription = attriax.Synchronization.Subscribe(
                    state => Debug.Log("Configured Attriax synchronization state: " + state, this));
                _deepLinkSubscription = attriax.DeepLinks.Stream.Subscribe(HandleDeepLinkEvent);

                if (_trackEventOnStart)
                {
                    await attriax.Tracking.TrackEventAsync(_eventName, new AttriaxTrackEventOptions
                    {
                        EventData = new Dictionary<string, object>
                        {
                            ["scene"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                            ["startedAt"] = DateTimeOffset.UtcNow.ToString("o"),
                        },
                    });
                }

                var initialDeepLinkEvent = await attriax.DeepLinks.WaitForInitialDeepLink;
                var originalInstallReferrer = await attriax.Referrer.GetOriginalInstallReferrerAsync();
                var reinstallReferrer = await attriax.Referrer.GetReinstallReferrerAsync();
                Debug.Log(
                    "Configured Attriax startup: "
                    + "InitialDeepLink=" + DescribeDeepLinkEvent(initialDeepLinkEvent)
                    + ", OriginalReferrer=" + DescribeInstallReferrer(originalInstallReferrer)
                    + ", ReinstallReferrer=" + DescribeInstallReferrer(reinstallReferrer)
                    + ", TrackingEnabled=" + attriax.Tracking.Enabled,
                    this);
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
        }

        private void HandleDeepLinkEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            Debug.Log("Configured Attriax deep link: " + DescribeDeepLinkEvent(deepLinkEvent), this);
        }

        private static string DescribeDeepLinkEvent(AttriaxDeepLinkEvent? result)
        {
            if (result == null)
            {
                return "no deep link resolved";
            }

            return (result.Found ? "matched " : "unmatched ") + result.Uri.AbsolutePath;
        }

        private static string DescribeInstallReferrer(AttriaxInstallReferrerDetails? referrer)
        {
            if (referrer == null)
            {
                return "none";
            }

            return !string.IsNullOrWhiteSpace(referrer.Campaign)
                ? referrer.Campaign
                : referrer.RawPlatformInstallReferrer ?? "available";
        }

        private void OnDestroy()
        {
            _deepLinkSubscription?.Dispose();
            _synchronizationSubscription?.Dispose();
        }
    }
}