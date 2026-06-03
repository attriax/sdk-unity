#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxAppOpenLaunchCoordinator
    {
        private readonly Func<bool> _didSchedule;
        private readonly Func<bool> _allowsAttributionTracking;
        private readonly Func<Task<AttriaxSdkRuntimeConfig>> _ensureRuntimeConfigLoadedAsync;
        private readonly Func<bool, Task<Dictionary<string, object>>> _buildDeviceMetadataOverridesAsync;
        private readonly Func<bool, bool, string?> _installReferrerOverrideForAppOpen;
        private readonly Func<string?, IDictionary<string, object>, Task> _scheduleAppOpenAsync;
        private readonly object _gate = new object();

        private Task? _inFlight;

        public AttriaxAppOpenLaunchCoordinator(
            Func<bool> didSchedule,
            Func<bool> allowsAttributionTracking,
            Func<Task<AttriaxSdkRuntimeConfig>> ensureRuntimeConfigLoadedAsync,
            Func<bool, Task<Dictionary<string, object>>> buildDeviceMetadataOverridesAsync,
            Func<bool, bool, string?> installReferrerOverrideForAppOpen,
            Func<string?, IDictionary<string, object>, Task> scheduleAppOpenAsync)
        {
            _didSchedule = didSchedule ?? throw new ArgumentNullException(nameof(didSchedule));
            _allowsAttributionTracking = allowsAttributionTracking ?? throw new ArgumentNullException(nameof(allowsAttributionTracking));
            _ensureRuntimeConfigLoadedAsync = ensureRuntimeConfigLoadedAsync ?? throw new ArgumentNullException(nameof(ensureRuntimeConfigLoadedAsync));
            _buildDeviceMetadataOverridesAsync = buildDeviceMetadataOverridesAsync ?? throw new ArgumentNullException(nameof(buildDeviceMetadataOverridesAsync));
            _installReferrerOverrideForAppOpen = installReferrerOverrideForAppOpen ?? throw new ArgumentNullException(nameof(installReferrerOverrideForAppOpen));
            _scheduleAppOpenAsync = scheduleAppOpenAsync ?? throw new ArgumentNullException(nameof(scheduleAppOpenAsync));
        }

        public void Reset()
        {
            lock (_gate)
            {
                _inFlight = null;
            }
        }

        public Task ScheduleIfNeeded(bool isInitialized, bool isEnabled)
        {
            var allowsAttributionTracking = _allowsAttributionTracking();
            var didSchedule = _didSchedule();
            if (!isInitialized || !isEnabled || !allowsAttributionTracking || didSchedule)
            {
                return Task.CompletedTask;
            }

            lock (_gate)
            {
                if (_inFlight != null)
                {
                    return _inFlight;
                }

                Task scheduling = null!;
                scheduling = RunAsync();
                _inFlight = scheduling;
                return scheduling;
            }

            async Task RunAsync()
            {
                try
                {
                    var runtimeConfig = await _ensureRuntimeConfigLoadedAsync().ConfigureAwait(false);
                    var currentAllowsAttributionTracking = _allowsAttributionTracking();
                    var deviceMetadataOverrides = await _buildDeviceMetadataOverridesAsync(currentAllowsAttributionTracking)
                        .ConfigureAwait(false);
                    var installReferrerOverride = _installReferrerOverrideForAppOpen(
                        runtimeConfig.ClipboardAttributionEnabled,
                        currentAllowsAttributionTracking);
                    await _scheduleAppOpenAsync(
                            installReferrerOverride,
                            deviceMetadataOverrides)
                        .ConfigureAwait(false);
                }
                finally
                {
                    lock (_gate)
                    {
                        if (ReferenceEquals(_inFlight, scheduling))
                        {
                            _inFlight = null;
                        }
                    }
                }
            }
        }
    }
}