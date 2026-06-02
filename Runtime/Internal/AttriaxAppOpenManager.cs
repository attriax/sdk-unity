#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxAppOpenManager
    {
        private readonly AttriaxRuntimeState _runtimeState;
        private readonly IAttriaxAppOpenPipeline _pipeline;
        private readonly AttriaxEventHub _eventHub;

        private Task<AttriaxAppOpenResult>? _openTrackingTask;
        private AttriaxAppOpenResult? _lastAppOpenResult;

        public AttriaxAppOpenManager(
            AttriaxRuntimeState runtimeState,
            IAttriaxAppOpenPipeline pipeline,
            AttriaxEventHub eventHub)
        {
            _runtimeState = runtimeState;
            _pipeline = pipeline;
            _eventHub = eventHub;
        }

        public AttriaxAppOpen? LastPublicResult => _pipeline.ToPublicAppOpen(_lastAppOpenResult);

        public bool HasSuccessfulResult => _lastAppOpenResult != null;

        public bool DidSchedule => _openTrackingTask != null || _lastAppOpenResult != null;

        public Task<AttriaxAppOpenResult>? CurrentTask => _openTrackingTask;

        public void ScheduleIfNeeded()
        {
            _ = ScheduleAsync();
        }

        public Task ScheduleAsync(
            string? installReferrerOverride = null,
            IDictionary<string, object>? deviceMetadataOverrides = null)
        {
            if (!_runtimeState.IsInitialized ||
                !_runtimeState.IsEnabled ||
                _openTrackingTask != null)
            {
                return Task.CompletedTask;
            }

            _openTrackingTask = _pipeline.EnqueueOpenAsync(
                installReferrerOverride,
                deviceMetadataOverrides);
            ObserveCompletion(_openTrackingTask);
            _ = _pipeline.ResolveInstallReferrerFromAppOpenAsync(_openTrackingTask);
            return Task.CompletedTask;
        }

        private void ObserveCompletion(Task<AttriaxAppOpenResult> openTrackingTask)
        {
            _ = openTrackingTask.ContinueWith(
                completedTask =>
                {
                    if (!ReferenceEquals(_openTrackingTask, completedTask))
                    {
                        return;
                    }

                    if (completedTask.IsFaulted || completedTask.IsCanceled)
                    {
                        _openTrackingTask = null;
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
        }

        public async Task<AttriaxAppOpen?> WaitForPublicResultAsync()
        {
            if (_openTrackingTask == null)
            {
                return null;
            }

            var result = await _openTrackingTask.ConfigureAwait(false);
            return _pipeline.ToPublicAppOpen(result);
        }

        public void Reset()
        {
            _openTrackingTask = null;
            _lastAppOpenResult = null;
        }

        public void HandleResult(AttriaxAppOpenResult result)
        {
            if (_openTrackingTask == null)
            {
                return;
            }

            _lastAppOpenResult = result;

            var deepLinkEvent = _pipeline.BuildDeepLinkEventFromAppOpenResult(result);
            if (deepLinkEvent != null)
            {
                _eventHub.EmitDeepLinkEvent(deepLinkEvent);
            }
        }
    }
}