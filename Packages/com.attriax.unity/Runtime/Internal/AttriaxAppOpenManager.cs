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
        private readonly object _gate = new object();

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

        public AttriaxAppOpen? LastPublicResult
        {
            get
            {
                lock (_gate)
                {
                    return _pipeline.ToPublicAppOpen(_lastAppOpenResult);
                }
            }
        }

        public bool HasSuccessfulResult
        {
            get
            {
                lock (_gate)
                {
                    return _lastAppOpenResult != null;
                }
            }
        }

        public bool DidSchedule
        {
            get
            {
                lock (_gate)
                {
                    return _openTrackingTask != null || _lastAppOpenResult != null;
                }
            }
        }

        public Task<AttriaxAppOpenResult>? CurrentTask
        {
            get
            {
                lock (_gate)
                {
                    return _openTrackingTask;
                }
            }
        }

        public void ScheduleIfNeeded()
        {
            _ = ScheduleAsync();
        }

        public Task ScheduleAsync(
            string? installReferrerOverride = null,
            IDictionary<string, object>? deviceMetadataOverrides = null)
        {
            if (!_runtimeState.IsInitialized ||
                !_runtimeState.IsEnabled)
            {
                return Task.CompletedTask;
            }

            Task<AttriaxAppOpenResult> openTrackingTask;
            lock (_gate)
            {
                if (_openTrackingTask != null)
                {
                    return Task.CompletedTask;
                }

                openTrackingTask = _pipeline.EnqueueOpenAsync(
                    installReferrerOverride,
                    deviceMetadataOverrides);
                _openTrackingTask = openTrackingTask;
            }

            ObserveCompletion(openTrackingTask);
            _ = _pipeline.ResolveInstallReferrerFromAppOpenAsync(openTrackingTask);
            return Task.CompletedTask;
        }

        private void ObserveCompletion(Task<AttriaxAppOpenResult> openTrackingTask)
        {
            _ = openTrackingTask.ContinueWith(
                completedTask =>
                {
                    lock (_gate)
                    {
                        if (!ReferenceEquals(_openTrackingTask, completedTask))
                        {
                            return;
                        }

                        if (completedTask.IsFaulted || completedTask.IsCanceled)
                        {
                            _openTrackingTask = null;
                        }
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
        }

        public async Task<AttriaxAppOpen?> WaitForPublicResultAsync()
        {
            Task<AttriaxAppOpenResult>? openTrackingTask;
            lock (_gate)
            {
                openTrackingTask = _openTrackingTask;
            }

            if (openTrackingTask == null)
            {
                return null;
            }

            var result = await openTrackingTask.ConfigureAwait(false);
            return _pipeline.ToPublicAppOpen(result);
        }

        public void Reset()
        {
            lock (_gate)
            {
                _openTrackingTask = null;
                _lastAppOpenResult = null;
            }
        }

        public void HandleResult(AttriaxAppOpenResult result)
        {
            AttriaxDeepLinkEvent? deepLinkEvent;
            lock (_gate)
            {
                if (_openTrackingTask == null)
                {
                    return;
                }

                _lastAppOpenResult = result;
                deepLinkEvent = _pipeline.BuildDeepLinkEventFromAppOpenResult(result);
            }

            if (deepLinkEvent != null)
            {
                _eventHub.EmitDeepLinkEvent(deepLinkEvent);
            }
        }
    }
}