#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeneratedPlatform = Attriax.Unity.Generated.Model.Platform;
using SdkV1ConfigDto = Attriax.Unity.Generated.Model.SdkV1ConfigDto;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxSdkRuntimeConfig
    {
        public AttriaxSdkRuntimeConfig(bool clipboardAttributionEnabled = false)
        {
            ClipboardAttributionEnabled = clipboardAttributionEnabled;
        }

        public bool ClipboardAttributionEnabled { get; }
    }

    internal sealed class AttriaxSdkRuntimeConfigCoordinator
    {
        private readonly Func<Task<AttriaxSdkRuntimeConfig>> _loadRuntimeConfigAsync;
        private readonly Func<AttriaxSdkRuntimeConfig, Task>? _onLoadedAsync;
        private readonly object _gate = new object();

        private Task<AttriaxSdkRuntimeConfig>? _inFlight;
        private AttriaxSdkRuntimeConfig _current = new AttriaxSdkRuntimeConfig();
        private bool _didResolve;

        public AttriaxSdkRuntimeConfigCoordinator(
            Func<Task<AttriaxSdkRuntimeConfig>> loadRuntimeConfigAsync,
            Func<AttriaxSdkRuntimeConfig, Task>? onLoadedAsync = null)
        {
            _loadRuntimeConfigAsync = loadRuntimeConfigAsync ?? throw new ArgumentNullException(nameof(loadRuntimeConfigAsync));
            _onLoadedAsync = onLoadedAsync;
        }

        public AttriaxSdkRuntimeConfig Current
        {
            get
            {
                lock (_gate)
                {
                    return _current;
                }
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                _inFlight = null;
                _current = new AttriaxSdkRuntimeConfig();
                _didResolve = false;
            }
        }

        public void PrimeForLaunch(bool isInitialized, bool isEnabled)
        {
            if (!isInitialized || !isEnabled)
            {
                return;
            }

            lock (_gate)
            {
                if (_didResolve || _inFlight != null)
                {
                    return;
                }
            }

            _ = EnsureLoadedAsync();
        }

        public Task<AttriaxSdkRuntimeConfig> EnsureLoadedAsync()
        {
            lock (_gate)
            {
                if (_inFlight != null)
                {
                    return _inFlight;
                }

                if (_didResolve)
                {
                    return Task.FromResult(_current);
                }

                var loading = LoadAsync();
                Task<AttriaxSdkRuntimeConfig> trackedLoading = null!;
                trackedLoading = TrackInFlightAsync(loading, () => trackedLoading);
                _inFlight = trackedLoading;
                return trackedLoading;
            }
        }

        private async Task<AttriaxSdkRuntimeConfig> TrackInFlightAsync(
            Task<AttriaxSdkRuntimeConfig> loading,
            Func<Task<AttriaxSdkRuntimeConfig>> currentTask)
        {
            try
            {
                return await loading.ConfigureAwait(false);
            }
            finally
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_inFlight, currentTask()))
                    {
                        _inFlight = null;
                    }
                }
            }
        }

        private async Task<AttriaxSdkRuntimeConfig> LoadAsync()
        {
            AttriaxSdkRuntimeConfig runtimeConfig;
            try
            {
                runtimeConfig = await _loadRuntimeConfigAsync().ConfigureAwait(false)
                    ?? new AttriaxSdkRuntimeConfig();
            }
            catch (Exception)
            {
                runtimeConfig = new AttriaxSdkRuntimeConfig();
            }

            lock (_gate)
            {
                _current = runtimeConfig;
                _didResolve = true;
            }

            if (_onLoadedAsync != null)
            {
                await _onLoadedAsync(_current).ConfigureAwait(false);
            }

            return _current;
        }
    }

    internal static class AttriaxSdkRuntimeConfigRequestBuilder
    {
        public static SdkV1ConfigDto Build(string projectToken, AttriaxContextSnapshot context)
        {
            if (projectToken == null)
            {
                throw new ArgumentNullException(nameof(projectToken));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new SdkV1ConfigDto(
                projectToken: projectToken,
                packageName: TrimOrNull(context.App.PackageName),
                platform: MapGeneratedPlatform(context.Platform),
                signatureHashes: context.Platform == AttriaxPlatformType.Android
                    ? ReadSignatureHashes(context.Device.Metadata)
                    : null);
        }

        private static List<string>? ReadSignatureHashes(IDictionary<string, object> metadata)
        {
            if (metadata == null ||
                !metadata.TryGetValue("signingSha256Fingerprints", out var rawValue) ||
                rawValue == null)
            {
                return null;
            }

            var normalized = new List<string>();

            if (rawValue is string rawString)
            {
                AppendHash(normalized, rawString);
            }
            else if (rawValue is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    AppendHash(normalized, item?.ToString());
                }
            }

            return normalized.Count > 0 ? normalized : null;
        }

        private static void AppendHash(ICollection<string> output, string? value)
        {
            var trimmed = TrimOrNull(value);
            if (trimmed == null)
            {
                return;
            }

            output.Add(trimmed);
        }

        private static string? TrimOrNull(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static GeneratedPlatform MapGeneratedPlatform(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return GeneratedPlatform.Android;
                case AttriaxPlatformType.IOS:
                    return GeneratedPlatform.Ios;
                case AttriaxPlatformType.UnityEditor:
                    return GeneratedPlatform.UnityEditor;
                case AttriaxPlatformType.Windows:
                    return GeneratedPlatform.Windows;
                case AttriaxPlatformType.MacOS:
                    return GeneratedPlatform.Macos;
                case AttriaxPlatformType.Linux:
                    return GeneratedPlatform.Linux;
                case AttriaxPlatformType.Web:
                    return GeneratedPlatform.Web;
                default:
                    return GeneratedPlatform.Unknown;
            }
        }
    }
}