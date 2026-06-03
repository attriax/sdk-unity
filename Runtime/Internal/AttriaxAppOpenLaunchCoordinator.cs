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
        private readonly Action<string, string?> _debugLog;

        private Task? _inFlight;

        public AttriaxAppOpenLaunchCoordinator(
            Func<bool> didSchedule,
            Func<bool> allowsAttributionTracking,
            Func<Task<AttriaxSdkRuntimeConfig>> ensureRuntimeConfigLoadedAsync,
            Func<bool, Task<Dictionary<string, object>>> buildDeviceMetadataOverridesAsync,
            Func<bool, bool, string?> installReferrerOverrideForAppOpen,
            Func<string?, IDictionary<string, object>, Task> scheduleAppOpenAsync,
            Action<string, string?> debugLog)
        {
            _didSchedule = didSchedule ?? throw new ArgumentNullException(nameof(didSchedule));
            _allowsAttributionTracking = allowsAttributionTracking ?? throw new ArgumentNullException(nameof(allowsAttributionTracking));
            _ensureRuntimeConfigLoadedAsync = ensureRuntimeConfigLoadedAsync ?? throw new ArgumentNullException(nameof(ensureRuntimeConfigLoadedAsync));
            _buildDeviceMetadataOverridesAsync = buildDeviceMetadataOverridesAsync ?? throw new ArgumentNullException(nameof(buildDeviceMetadataOverridesAsync));
            _installReferrerOverrideForAppOpen = installReferrerOverrideForAppOpen ?? throw new ArgumentNullException(nameof(installReferrerOverrideForAppOpen));
            _scheduleAppOpenAsync = scheduleAppOpenAsync ?? throw new ArgumentNullException(nameof(scheduleAppOpenAsync));
            _debugLog = debugLog ?? throw new ArgumentNullException(nameof(debugLog));
        }

        public void Reset()
        {
            _inFlight = null;
            _debugLog("Reset app-open launch coordinator state.", (string?)null);
        }

        public Task ScheduleIfNeeded(bool isInitialized, bool isEnabled)
        {
            var allowsAttributionTracking = _allowsAttributionTracking();
            var didSchedule = _didSchedule();
            if (!isInitialized || !isEnabled || !allowsAttributionTracking || didSchedule)
            {
                _debugLog(
                    "Skipping launch app-open scheduling.",
                    "initialized=" + isInitialized
                    + ", enabled=" + isEnabled
                    + ", allowsAttributionTracking=" + allowsAttributionTracking
                    + ", didSchedule=" + didSchedule
                    + ", inFlight=" + (_inFlight != null));
                return Task.CompletedTask;
            }

            var inFlight = _inFlight;
            if (inFlight != null)
            {
                _debugLog("Reusing in-flight launch app-open scheduling task.", (string?)null);
                return inFlight;
            }

            Task scheduling = null!;
            scheduling = RunAsync();
            _inFlight = scheduling;
            _debugLog("Created new launch app-open scheduling task.", (string?)null);
            return scheduling;

            async Task RunAsync()
            {
                try
                {
                    _debugLog("Loading runtime config for launch app-open scheduling.", (string?)null);
                    var runtimeConfig = await _ensureRuntimeConfigLoadedAsync().ConfigureAwait(false);
                    var currentAllowsAttributionTracking = _allowsAttributionTracking();
                    _debugLog(
                        "Runtime config loaded for launch app-open scheduling.",
                        "clipboardAttributionEnabled=" + runtimeConfig.ClipboardAttributionEnabled
                        + ", allowsAttributionTracking=" + currentAllowsAttributionTracking);
                    var deviceMetadataOverrides = await _buildDeviceMetadataOverridesAsync(currentAllowsAttributionTracking)
                        .ConfigureAwait(false);
                    var installReferrerOverride = _installReferrerOverrideForAppOpen(
                        runtimeConfig.ClipboardAttributionEnabled,
                        currentAllowsAttributionTracking);
                    _debugLog(
                        "Scheduling launch app-open after building metadata overrides.",
                        "metadataCount=" + deviceMetadataOverrides.Count
                        + ", installReferrerOverridePresent=" + (!string.IsNullOrWhiteSpace(installReferrerOverride)));
                    await _scheduleAppOpenAsync(
                            installReferrerOverride,
                            deviceMetadataOverrides)
                        .ConfigureAwait(false);
                    _debugLog("Launch app-open scheduling completed.", (string?)null);
                }
                finally
                {
                    if (ReferenceEquals(_inFlight, scheduling))
                    {
                        _inFlight = null;
                        _debugLog("Cleared in-flight launch app-open scheduling task.", (string?)null);
                    }
                }
            }
        }
    }
}