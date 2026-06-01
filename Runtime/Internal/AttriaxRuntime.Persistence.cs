#nullable enable
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    internal sealed partial class AttriaxRuntime
    {
        private sealed class AttriaxRuntimeInstallReferrerStore
        {
            private readonly AttriaxRuntime _runtime;
            private string _cachedInstallReferrer = string.Empty;
            private bool _loadedInstallReferrerCache;

            public AttriaxRuntimeInstallReferrerStore(AttriaxRuntime runtime)
            {
                _runtime = runtime;
            }

            public void ResetCache()
            {
                _cachedInstallReferrer = string.Empty;
                _loadedInstallReferrerCache = false;
            }

            public string? ReadPersistedInstallReferrer()
            {
                if (_loadedInstallReferrerCache)
                {
                    return _cachedInstallReferrer;
                }

                _loadedInstallReferrerCache = true;
                _cachedInstallReferrer = AttriaxPlayerPrefs.GetString(
                    _runtime.Key(InstallReferrerStorageKey),
                    string.Empty);
                return string.IsNullOrWhiteSpace(_cachedInstallReferrer) ? null : _cachedInstallReferrer;
            }

            public AttriaxInstallReferrerDetails? ReadPersistedInstallReferrerDetails(string storageKey)
            {
                var serialized = AttriaxPlayerPrefs.GetString(_runtime.Key(storageKey), string.Empty);
                if (string.IsNullOrWhiteSpace(serialized))
                {
                    return null;
                }

                try
                {
                    return JsonConvert.DeserializeObject<AttriaxInstallReferrerDetails>(serialized);
                }
                catch (JsonException exception)
                {
                    _runtime.DebugLog(
                        "Failed to parse persisted install-referrer details. Discarding the cached payload.",
                        exception);
                    return null;
                }
            }

            public void PersistInstallReferrer(string installReferrer)
            {
                if (string.IsNullOrWhiteSpace(installReferrer) || _cachedInstallReferrer == installReferrer)
                {
                    return;
                }

                _cachedInstallReferrer = installReferrer;
                _loadedInstallReferrerCache = true;
                AttriaxPlayerPrefs.SetString(_runtime.Key(InstallReferrerStorageKey), installReferrer);
                AttriaxPlayerPrefs.Save();
            }

            public void PersistInstallReferrerDetails(
                string storageKey,
                AttriaxInstallReferrerDetails installReferrerDetails)
            {
                if (installReferrerDetails == null)
                {
                    return;
                }

                AttriaxPlayerPrefs.SetString(
                    _runtime.Key(storageKey),
                    JsonConvert.SerializeObject(installReferrerDetails));
                AttriaxPlayerPrefs.Save();
            }
        }
    }
}
