#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxDeepLinkManager
    {
        private readonly AttriaxRuntimeState _runtimeState;
        private readonly IAttriaxDeepLinkConversionResolver _conversionResolver;
        private readonly AttriaxEventHub _eventHub;
        private readonly Dictionary<AttriaxRawDeepLinkEvent, TaskCompletionSource<AttriaxDeepLinkEvent>> _pendingDeepLinkResolutions =
            new Dictionary<AttriaxRawDeepLinkEvent, TaskCompletionSource<AttriaxDeepLinkEvent>>();

        private TaskCompletionSource<AttriaxDeepLinkEvent?> _initialDeepLinkTaskSource =
            CreateInitialDeepLinkTaskSource();
        private bool _initialDeepLinkResolved;
        private AttriaxRawDeepLinkEvent? _rawInitialDeepLinkValue;
        private AttriaxDeepLinkEvent? _initialDeepLinkValue;
        private AttriaxDeepLinkEvent? _latestDeepLink;
        private AttriaxDeepLinkEvent? _pendingAppOpenInitialDeepLink;
        private bool _initialUrlPending;
        private bool _appOpenPending;
        private int _generation;

        public AttriaxDeepLinkManager(
            AttriaxRuntimeState runtimeState,
            IAttriaxDeepLinkConversionResolver conversionResolver,
            AttriaxEventHub eventHub)
        {
            _runtimeState = runtimeState;
            _conversionResolver = conversionResolver;
            _eventHub = eventHub;
        }

        public Task<AttriaxDeepLinkEvent?> InitialDeepLink => _initialDeepLinkTaskSource.Task;

        public AttriaxRawDeepLinkEvent? RawInitialDeepLinkValue => _rawInitialDeepLinkValue;

        public AttriaxDeepLinkEvent? InitialDeepLinkValue => _initialDeepLinkValue;

        public bool InitialDeepLinkResolved => _initialDeepLinkResolved;

        public AttriaxDeepLinkEvent? LatestDeepLink => _latestDeepLink;

        public Task<AttriaxDeepLinkEvent> WaitForResolutionAsync(AttriaxRawDeepLinkEvent rawEvent)
        {
            if (_pendingDeepLinkResolutions.TryGetValue(rawEvent, out var pendingResolution))
            {
                return pendingResolution.Task;
            }

            return Task.FromException<AttriaxDeepLinkEvent>(
                new InvalidOperationException("No pending or completed resolution exists for this raw deep link."));
        }

        public void BeginInitialProbe(bool captureInitialUrl)
        {
            _initialUrlPending = captureInitialUrl;
            _appOpenPending = _runtimeState.IsEnabled;
            _pendingAppOpenInitialDeepLink = null;
        }

        public void Reset()
        {
            _generation += 1;
            var resetError = new InvalidOperationException(
                "Attriax SDK state was reset before deep-link resolution completed.");
            foreach (var pendingResolution in _pendingDeepLinkResolutions.Values)
            {
                pendingResolution.TrySetException(resetError);
            }
            _pendingDeepLinkResolutions.Clear();
            if (!_initialDeepLinkResolved)
            {
                _initialDeepLinkTaskSource.TrySetResult(null);
            }

            _initialDeepLinkTaskSource = CreateInitialDeepLinkTaskSource();
            _initialDeepLinkResolved = false;
            _rawInitialDeepLinkValue = null;
            _initialDeepLinkValue = null;
            _latestDeepLink = null;
            _pendingAppOpenInitialDeepLink = null;
            _initialUrlPending = false;
            _appOpenPending = false;
        }

        public async Task<AttriaxDeepLinkEvent?> RecordConversionAsync(AttriaxDeepLinkConversionOptions options)
        {
            if (!_runtimeState.IsEnabled)
            {
                return null;
            }

            return await _conversionResolver.ResolveDeepLinkConversionAsync(
                    options,
                    null,
                    DateTimeOffset.UtcNow)
                .ConfigureAwait(false);
        }

        public Task CaptureInitialUrlAsync(string? url)
        {
            if (!TryBuildRawDeepLinkEvent(url, true, out var rawEvent))
            {
                MarkInitialUrlUnavailable();
                return Task.CompletedTask;
            }

            var options = new AttriaxDeepLinkConversionOptions
            {
                Uri = url,
                IsInitialLink = true,
                Source = "initial_url",
            };
            TrackRawDeepLink(options, rawEvent);

            return Task.CompletedTask;
        }

        public void HandleDeepLinkActivated(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (!TryBuildRawDeepLinkEvent(url, false, out var rawEvent))
            {
                return;
            }

            TrackRawDeepLink(new AttriaxDeepLinkConversionOptions
            {
                Uri = url,
                Source = "deep_link_activated",
            }, rawEvent);
        }

        public void MarkInitialUrlUnavailable()
        {
            _initialUrlPending = false;
            if (_pendingAppOpenInitialDeepLink != null)
            {
                var pendingAppOpenDeepLink = _pendingAppOpenInitialDeepLink;
                _pendingAppOpenInitialDeepLink = null;
                CompleteInitial(pendingAppOpenDeepLink);
                return;
            }

            TryCompleteInitialNull();
        }

        public void HandleAppOpenEvent(AttriaxDeepLinkEvent deepLinkEvent)
        {
            _appOpenPending = false;
            _latestDeepLink = deepLinkEvent;
            if (_initialDeepLinkResolved)
            {
                return;
            }

            if (_initialUrlPending)
            {
                _pendingAppOpenInitialDeepLink = deepLinkEvent;
                return;
            }

            CompleteInitial(deepLinkEvent);
        }

        public void MarkAppOpenUnavailable()
        {
            _appOpenPending = false;
            TryCompleteInitialNull();
        }

        public void CompleteInitial(AttriaxDeepLinkEvent? deepLinkEvent)
        {
            if (!_initialDeepLinkResolved)
            {
                _initialDeepLinkValue = deepLinkEvent;
                _initialDeepLinkResolved = true;
            }

            if (deepLinkEvent != null)
            {
                _latestDeepLink = deepLinkEvent;
            }

            _pendingAppOpenInitialDeepLink = null;
            _initialDeepLinkTaskSource.TrySetResult(deepLinkEvent);
        }

        public void FailInitial(Exception exception)
        {
            if (_initialDeepLinkResolved)
            {
                return;
            }

            _initialDeepLinkResolved = true;
            _initialDeepLinkTaskSource.TrySetException(exception);
        }

        private void TryCompleteInitialNull()
        {
            if (_initialDeepLinkResolved || _initialUrlPending || _appOpenPending)
            {
                return;
            }

            CompleteInitial(null);
        }

        private static TaskCompletionSource<AttriaxDeepLinkEvent?> CreateInitialDeepLinkTaskSource()
        {
            return new TaskCompletionSource<AttriaxDeepLinkEvent?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private static TaskCompletionSource<AttriaxDeepLinkEvent> CreatePendingResolutionTaskSource()
        {
            return new TaskCompletionSource<AttriaxDeepLinkEvent>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private void TrackRawDeepLink(
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent rawEvent)
        {
            var generation = _generation;
            var pendingResolution = CreatePendingResolutionTaskSource();
            _pendingDeepLinkResolutions[rawEvent] = pendingResolution;

            if (rawEvent.IsInitial)
            {
                _rawInitialDeepLinkValue = rawEvent;
            }

            _eventHub.EmitRawDeepLinkEvent(rawEvent);
            _ = ResolveRawDeepLinkAsync(options, rawEvent, pendingResolution, generation);
        }

        private async Task ResolveRawDeepLinkAsync(
            AttriaxDeepLinkConversionOptions options,
            AttriaxRawDeepLinkEvent rawEvent,
            TaskCompletionSource<AttriaxDeepLinkEvent> pendingResolution,
            int generation)
        {
            try
            {
                var resolvedEvent = await _conversionResolver.ResolveDeepLinkConversionAsync(
                        options,
                        rawEvent,
                        rawEvent.ReceivedAt)
                    .ConfigureAwait(false);

                if (generation != _generation)
                {
                    return;
                }

                pendingResolution.TrySetResult(resolvedEvent);
                _latestDeepLink = resolvedEvent;
                _eventHub.EmitDeepLinkEvent(resolvedEvent);

                if (rawEvent.IsInitial)
                {
                    _initialUrlPending = false;
                    _pendingAppOpenInitialDeepLink = null;
                    CompleteInitial(resolvedEvent);
                }
            }
            catch (Exception error)
            {
                if (generation != _generation)
                {
                    return;
                }

                pendingResolution.TrySetException(error);
                if (rawEvent.IsInitial)
                {
                    _initialUrlPending = false;
                    _pendingAppOpenInitialDeepLink = null;
                    FailInitial(error);
                }
            }
        }

        private static bool TryBuildRawDeepLinkEvent(
            string? url,
            bool isInitial,
            out AttriaxRawDeepLinkEvent rawEvent)
        {
            rawEvent = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri) || parsedUri == null)
            {
                return false;
            }

            rawEvent = new AttriaxRawDeepLinkEvent
            {
                Uri = parsedUri,
                ReceivedAt = DateTimeOffset.UtcNow,
                IsInitial = isInitial,
            };

            return true;
        }
    }
}