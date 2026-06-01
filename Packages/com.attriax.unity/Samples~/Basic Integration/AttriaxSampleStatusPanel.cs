#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Attriax.Unity.Samples
{
    public sealed class AttriaxSampleStatusPanel : MonoBehaviour
    {
        [SerializeField] private AttriaxBehaviour? _attriaxBehaviour;
        [SerializeField] private bool _useConfiguredSingleton;
        [SerializeField] private bool _resolveInitialDeepLinkOnStart = true;
        [SerializeField, TextArea(2, 4)] private string _synchronizationState = AttriaxSynchronizationState.Initializing.ToString();
        [SerializeField, TextArea(2, 4)] private string _startupSummary = "pending";
        [SerializeField, TextArea(2, 4)] private string _initialDeepLinkSummary = "pending";
        [SerializeField, TextArea(2, 4)] private string _latestDeepLinkSummary = "none";
        [SerializeField, TextArea(2, 4)] private string _originalInstallReferrerSummary = "pending";
        [SerializeField, TextArea(2, 4)] private string _reinstallReferrerSummary = "pending";

        private Attriax? _attriax;
        private IDisposable? _deepLinkSubscription;
        private IDisposable? _synchronizationSubscription;

        private async void Start()
        {
            try
            {
                var attriax = _attriax = await ResolveRuntimeAsync();
                _synchronizationState = attriax.Synchronization.State.ToString();
                _synchronizationSubscription = attriax.Synchronization.Subscribe(
                    state => _synchronizationState = state.ToString());
                _deepLinkSubscription = attriax.DeepLinks.Stream.Subscribe(HandleDeepLinkEvent);

                if (_resolveInitialDeepLinkOnStart)
                {
                    var initialDeepLinkEvent = await attriax.DeepLinks.WaitForInitialDeepLink;
                    _initialDeepLinkSummary = await DescribeDeepLinkEventAsync(initialDeepLinkEvent);
                }

                _startupSummary = DescribeStartupState(attriax);
                _originalInstallReferrerSummary = DescribeInstallReferrer(
                    await attriax.Referrer.GetOriginalInstallReferrerAsync());
                _reinstallReferrerSummary = DescribeInstallReferrer(
                    await attriax.Referrer.GetReinstallReferrerAsync());
            }
            catch (Exception error)
            {
                _startupSummary = "error: " + error.Message;
                Debug.LogException(error, this);
            }
        }

        private async Task<Attriax> ResolveRuntimeAsync()
        {
            if (_attriaxBehaviour != null)
            {
                if (!_attriaxBehaviour.IsReady)
                {
                    await _attriaxBehaviour.InitializeAsync();
                }

                return _attriaxBehaviour.Instance!;
            }

            if (_useConfiguredSingleton)
            {
                return await Attriax.InitializeConfiguredAsync();
            }

            throw new InvalidOperationException(
                "Assign an AttriaxBehaviour instance or enable the configured singleton path.");
        }

        private void HandleDeepLinkEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            _latestDeepLinkSummary = DescribeDeepLinkEvent(deepLinkEvent);
        }

        private static string DescribeStartupState(Attriax attriax)
        {
            return "firstLaunch=" + attriax.IsFirstLaunch
                + ", trackingEnabled=" + attriax.Tracking.Enabled
                + ", initialDeepLinkResolved=" + attriax.DeepLinks.InitialDeepLinkResolved;
        }

        private static Task<string> DescribeDeepLinkEventAsync(AttriaxDeepLinkEvent? deepLinkEvent)
        {
            if (deepLinkEvent == null)
            {
                return Task.FromResult("no deep link resolved");
            }

            return Task.FromResult(DescribeDeepLinkEvent(deepLinkEvent));
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