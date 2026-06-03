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
        private readonly Action<string, string?> _debugLog;

        private Task<AttriaxAppOpenResult>? _openTrackingTask;
        private AttriaxAppOpenResult? _lastAppOpenResult;

        public AttriaxAppOpenManager(
            AttriaxRuntimeState runtimeState,
            IAttriaxAppOpenPipeline pipeline,
            AttriaxEventHub eventHub,
            Action<string, string?> debugLog)
        {
            _runtimeState = runtimeState;
            _pipeline = pipeline;
            _eventHub = eventHub;
            _debugLog = debugLog;
        }

        public AttriaxAppOpen? LastPublicResult => _pipeline.ToPublicAppOpen(_lastAppOpenResult);

        public bool HasSuccessfulResult => _lastAppOpenResult != null;

        public bool DidSchedule => _openTrackingTask != null || _lastAppOpenResult != null;

        public Task<AttriaxAppOpenResult>? CurrentTask => _openTrackingTask;

        public void ScheduleIfNeeded()
        {
            _debugLog(
                "App-open manager ScheduleIfNeeded invoked.",
                "initialized=" + _runtimeState.IsInitialized
                + ", enabled=" + _runtimeState.IsEnabled
                + ", currentTaskStatus=" + DescribeTaskStatus(_openTrackingTask)
                + ", hasLastResult=" + (_lastAppOpenResult != null));
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
                _debugLog(
                    "Skipping app-open scheduling.",
                    "initialized=" + _runtimeState.IsInitialized
                    + ", enabled=" + _runtimeState.IsEnabled
                    + ", currentTaskStatus=" + DescribeTaskStatus(_openTrackingTask));
                return Task.CompletedTask;
            }

            _debugLog(
                "Scheduling app-open task.",
                "installReferrerOverridePresent=" + (!string.IsNullOrWhiteSpace(installReferrerOverride))
                + ", metadataCount=" + (deviceMetadataOverrides != null ? deviceMetadataOverrides.Count : 0));
            try
            {
                _openTrackingTask = _pipeline.EnqueueOpenAsync(
                    installReferrerOverride,
                    deviceMetadataOverrides);
            }
            catch (Exception exception)
            {
                _debugLog(
                    "App-open scheduling threw before the app-open task could be created.",
                    exception.Message);
                throw;
            }

            _debugLog(
                "App-open task created.",
                "taskStatus=" + DescribeTaskStatus(_openTrackingTask));
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
                        _debugLog("Ignoring stale app-open completion callback.", (string?)null);
                        return;
                    }

                    if (completedTask.IsFaulted || completedTask.IsCanceled)
                    {
                        _debugLog(
                            "App-open task finished without success; clearing current task.",
                            "faulted=" + completedTask.IsFaulted + ", canceled=" + completedTask.IsCanceled);
                        _openTrackingTask = null;
                        return;
                    }

                    _debugLog(
                        "App-open task completed successfully.",
                        "taskStatus=" + DescribeTaskStatus(completedTask));
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
            _debugLog("Reset app-open manager state.", (string?)null);
        }

        public void HandleResult(AttriaxAppOpenResult result)
        {
            if (_openTrackingTask == null)
            {
                _debugLog("Ignoring app-open result because no current app-open task exists.", (string?)null);
                return;
            }

            _lastAppOpenResult = result;
            var deepLinkEvent = _pipeline.BuildDeepLinkEventFromAppOpenResult(result);
            _debugLog(
                "Stored successful app-open result.",
                "hasDeepLink=" + (deepLinkEvent != null));

            if (deepLinkEvent != null)
            {
                _eventHub.EmitDeepLinkEvent(deepLinkEvent);
            }
        }

        private static string DescribeTaskStatus(Task? task)
        {
            return task == null ? "null" : task.Status.ToString();
        }
    }
}