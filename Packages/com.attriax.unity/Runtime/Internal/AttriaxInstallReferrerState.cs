#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxInstallReferrerState
    {
        private TaskCompletionSource<AttriaxInstallReferrerDetails?>? _taskSource;
        private bool _completedForDisabled;

        public Task<AttriaxInstallReferrerDetails?> Task =>
            _taskSource != null
                ? _taskSource.Task
                : System.Threading.Tasks.Task.FromException<AttriaxInstallReferrerDetails?>(
                    new InvalidOperationException(
                        "Attriax.InitializeAsync() must complete before reading install-referrer details."));

        public bool HasPendingCompletion => _taskSource != null && !_taskSource.Task.IsCompleted;

        public void EnsureTaskSource()
        {
            _taskSource ??= CreateTaskSource();
        }

        public void PrepareForEnabledState(AttriaxInstallReferrerDetails? cachedDetails)
        {
            ReopenIfNeeded();
            if (cachedDetails != null)
            {
                Complete(cachedDetails);
            }
        }

        public void Complete(
            AttriaxInstallReferrerDetails? installReferrerDetails,
            bool disabledResult = false)
        {
            if (_taskSource == null || _taskSource.Task.IsCompleted)
            {
                return;
            }

            _completedForDisabled = disabledResult;
            _taskSource.TrySetResult(installReferrerDetails);
        }

        public void Reset()
        {
            if (_taskSource != null && !_taskSource.Task.IsCompleted)
            {
                _taskSource.TrySetResult(null);
            }

            _taskSource = null;
            _completedForDisabled = false;
        }

        private void ReopenIfNeeded()
        {
            if (_taskSource == null || (_taskSource.Task.IsCompleted && _completedForDisabled))
            {
                _taskSource = CreateTaskSource();
                _completedForDisabled = false;
            }
        }

        private static TaskCompletionSource<AttriaxInstallReferrerDetails?> CreateTaskSource()
        {
            return new TaskCompletionSource<AttriaxInstallReferrerDetails?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}