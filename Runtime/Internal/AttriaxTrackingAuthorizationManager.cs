#nullable enable
using System;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxTrackingAuthorizationManager
    {
        private readonly object _gate = new object();
        private readonly AttriaxConfig _config;
        private readonly AttriaxPlatformType _platformType;
        private readonly Func<Task<AttriaxTrackingAuthorizationStatus>> _getTrackingAuthorizationStatusAsync;
        private readonly Func<Task<AttriaxTrackingAuthorizationStatus>> _requestTrackingAuthorizationAsync;
        private readonly TimeSpan _pendingTrackingAuthorizationPollInterval;
        private readonly TimeSpan _startupTrackingAuthorizationPollInterval;

        private Task<AttriaxTrackingAuthorizationStatus>? _trackingAuthorizationRequest;
        private Task<AttriaxTrackingAuthorizationStatus>? _trackingAuthorizationStartupWait;
        private AttriaxTrackingAuthorizationStatus? _cachedTrackingAuthorizationStatus;
        private bool _didResolveStartupTrackingAuthorization;
        private TaskCompletionSource<bool>? _trackingAuthorizationRequestSignal;

        public AttriaxTrackingAuthorizationManager(
            AttriaxConfig config,
            AttriaxPlatformType platformType,
            Func<Task<AttriaxTrackingAuthorizationStatus>> getTrackingAuthorizationStatusAsync,
            Func<Task<AttriaxTrackingAuthorizationStatus>> requestTrackingAuthorizationAsync,
            TimeSpan? pendingTrackingAuthorizationPollInterval = null,
            TimeSpan? startupTrackingAuthorizationPollInterval = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _platformType = platformType;
            _getTrackingAuthorizationStatusAsync = getTrackingAuthorizationStatusAsync
                ?? throw new ArgumentNullException(nameof(getTrackingAuthorizationStatusAsync));
            _requestTrackingAuthorizationAsync = requestTrackingAuthorizationAsync
                ?? throw new ArgumentNullException(nameof(requestTrackingAuthorizationAsync));
            _pendingTrackingAuthorizationPollInterval =
                pendingTrackingAuthorizationPollInterval ?? TimeSpan.FromMilliseconds(250);
            _startupTrackingAuthorizationPollInterval =
                startupTrackingAuthorizationPollInterval ?? TimeSpan.FromSeconds(1);
        }

        public async Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync()
        {
            var status = await _getTrackingAuthorizationStatusAsync().ConfigureAwait(false);
            CacheTrackingAuthorizationStatus(status);
            return status;
        }

        public Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(
            int? timeoutMs = null)
        {
            lock (_gate)
            {
                if (_trackingAuthorizationRequest != null)
                {
                    return _trackingAuthorizationRequest;
                }

                var timeout = timeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(Math.Max(0, timeoutMs.Value))
                    : (TimeSpan?)null;
                var requestSource = new TaskCompletionSource<AttriaxTrackingAuthorizationStatus>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _trackingAuthorizationRequest = requestSource.Task;
                SignalTrackingAuthorizationRequest();
                _ = CompleteTrackingAuthorizationRequestAsync(requestSource, timeout);
                return requestSource.Task;
            }
        }

        private async Task CompleteTrackingAuthorizationRequestAsync(
            TaskCompletionSource<AttriaxTrackingAuthorizationStatus> requestSource,
            TimeSpan? timeout)
        {
            try
            {
                var status = await RequestTrackingAuthorizationCoreAsync(timeout)
                    .ConfigureAwait(false);
                CacheTrackingAuthorizationStatus(status);
                requestSource.TrySetResult(status);
            }
            catch (Exception ex)
            {
                requestSource.TrySetException(ex);
            }
            finally
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_trackingAuthorizationRequest, requestSource.Task))
                    {
                        _trackingAuthorizationRequest = null;
                    }
                }
            }
        }

        public Task<AttriaxTrackingAuthorizationStatus> WaitForTrackingAuthorizationIfNeededAsync()
        {
            if (_platformType != AttriaxPlatformType.IOS)
            {
                return Task.FromResult(AttriaxTrackingAuthorizationStatus.NotSupported);
            }

            if (!_config.CollectAdvertisingId)
            {
                return Task.FromResult(AttriaxTrackingAuthorizationStatus.Disabled);
            }

            lock (_gate)
            {
                if (_trackingAuthorizationStartupWait != null)
                {
                    return _trackingAuthorizationStartupWait;
                }

                if (IsResolvedTrackingAuthorizationStatus(_cachedTrackingAuthorizationStatus))
                {
                    return Task.FromResult(_cachedTrackingAuthorizationStatus!.Value);
                }

                if (_didResolveStartupTrackingAuthorization)
                {
                    return Task.FromResult(
                        _cachedTrackingAuthorizationStatus
                        ?? AttriaxTrackingAuthorizationStatus.NotDetermined);
                }

                Task<AttriaxTrackingAuthorizationStatus> wait;
                if (_config.RequestTrackingAuthorizationOnInit)
                {
                    wait = _trackingAuthorizationRequest ?? RequestTrackingAuthorizationAsync();
                }
                else
                {
                    wait = PollTrackingAuthorizationStatusAsync();
                }
                _trackingAuthorizationStartupWait = wait;
                return AwaitStartupWaitAsync(wait);
            }
        }

        private async Task<AttriaxTrackingAuthorizationStatus> AwaitStartupWaitAsync(
            Task<AttriaxTrackingAuthorizationStatus> wait)
        {
            try
            {
                return await wait.ConfigureAwait(false);
            }
            finally
            {
                lock (_gate)
                {
                    _didResolveStartupTrackingAuthorization = true;
                    if (ReferenceEquals(_trackingAuthorizationStartupWait, wait))
                    {
                        _trackingAuthorizationStartupWait = null;
                    }
                }
            }
        }

        private async Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationCoreAsync(
            TimeSpan? timeout)
        {
            if (_platformType != AttriaxPlatformType.IOS)
            {
                return await _requestTrackingAuthorizationAsync().ConfigureAwait(false);
            }

            var requestTask = _requestTrackingAuthorizationAsync();
            var remaining = timeout;
            var initialDelay = TrackingAuthorizationPollDelay(
                remaining,
                _pendingTrackingAuthorizationPollInterval);

            if (!requestTask.IsCompleted)
            {
                await WaitForTaskOrDelayAsync(requestTask, initialDelay).ConfigureAwait(false);
                if (remaining.HasValue)
                {
                    remaining = remaining.Value - initialDelay;
                }
            }

            while (true)
            {
                if (requestTask.IsCompleted)
                {
                    var completedStatus = await requestTask.ConfigureAwait(false);
                    if (completedStatus != AttriaxTrackingAuthorizationStatus.NotDetermined)
                    {
                        return completedStatus;
                    }
                }

                var status = await GetTrackingAuthorizationStatusAsync().ConfigureAwait(false);
                if (IsResolvedTrackingAuthorizationStatus(status))
                {
                    return status;
                }

                if (remaining.HasValue && remaining.Value <= TimeSpan.Zero)
                {
                    return AttriaxTrackingAuthorizationStatus.TimedOut;
                }

                var delay = TrackingAuthorizationPollDelay(
                    remaining,
                    _pendingTrackingAuthorizationPollInterval);
                if (requestTask.IsCompleted)
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                }
                else
                {
                    await WaitForTaskOrDelayAsync(requestTask, delay).ConfigureAwait(false);
                }

                if (remaining.HasValue)
                {
                    remaining = remaining.Value - delay;
                }
            }
        }

        private async Task<AttriaxTrackingAuthorizationStatus> PollTrackingAuthorizationStatusAsync()
        {
            var timeout = TimeSpan.FromMilliseconds(
                Math.Max(0, _config.TrackingAuthorizationStatusTimeoutMs));
            var deadline = DateTimeOffset.UtcNow.Add(timeout);

            while (true)
            {
                Task<AttriaxTrackingAuthorizationStatus>? inFlightRequest;
                AttriaxTrackingAuthorizationStatus? cachedStatus;
                lock (_gate)
                {
                    inFlightRequest = _trackingAuthorizationRequest;
                    cachedStatus = _cachedTrackingAuthorizationStatus;
                }

                if (inFlightRequest != null)
                {
                    return await inFlightRequest.ConfigureAwait(false);
                }

                if (IsResolvedTrackingAuthorizationStatus(cachedStatus))
                {
                    return cachedStatus!.Value;
                }

                var status = await GetTrackingAuthorizationStatusAsync().ConfigureAwait(false);
                if (IsResolvedTrackingAuthorizationStatus(status))
                {
                    return status;
                }

                var remaining = deadline - DateTimeOffset.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    return AttriaxTrackingAuthorizationStatus.TimedOut;
                }

                var signal = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                lock (_gate)
                {
                    _trackingAuthorizationRequestSignal = signal;
                    if (_trackingAuthorizationRequest != null)
                    {
                        _trackingAuthorizationRequestSignal = null;
                        continue;
                    }
                }

                var delay = TrackingAuthorizationPollDelay(
                    remaining,
                    _startupTrackingAuthorizationPollInterval);
                await WaitForSignalOrDelayAsync(signal, delay).ConfigureAwait(false);
            }
        }

        private static async Task WaitForTaskOrDelayAsync(Task task, TimeSpan delay)
        {
            await Task.WhenAny(task, Task.Delay(delay)).ConfigureAwait(false);
        }

        private async Task WaitForSignalOrDelayAsync(
            TaskCompletionSource<bool> signal,
            TimeSpan delay)
        {
            await Task.WhenAny(signal.Task, Task.Delay(delay)).ConfigureAwait(false);
            lock (_gate)
            {
                if (ReferenceEquals(_trackingAuthorizationRequestSignal, signal))
                {
                    _trackingAuthorizationRequestSignal = null;
                }
            }
        }

        private void CacheTrackingAuthorizationStatus(AttriaxTrackingAuthorizationStatus status)
        {
            if (status == AttriaxTrackingAuthorizationStatus.TimedOut)
            {
                return;
            }

            lock (_gate)
            {
                _cachedTrackingAuthorizationStatus = status;
            }
        }

        private void SignalTrackingAuthorizationRequest()
        {
            lock (_gate)
            {
                if (_trackingAuthorizationRequestSignal != null &&
                    !_trackingAuthorizationRequestSignal.Task.IsCompleted)
                {
                    _trackingAuthorizationRequestSignal.SetResult(true);
                }
            }
        }

        private static TimeSpan TrackingAuthorizationPollDelay(
            TimeSpan? remaining,
            TimeSpan interval)
        {
            if (!remaining.HasValue)
            {
                return interval;
            }

            return remaining.Value < interval ? remaining.Value : interval;
        }

        private static bool IsResolvedTrackingAuthorizationStatus(
            AttriaxTrackingAuthorizationStatus? status)
        {
            if (!status.HasValue)
            {
                return false;
            }

            return status.Value != AttriaxTrackingAuthorizationStatus.NotDetermined &&
                status.Value != AttriaxTrackingAuthorizationStatus.TimedOut;
        }
    }
}