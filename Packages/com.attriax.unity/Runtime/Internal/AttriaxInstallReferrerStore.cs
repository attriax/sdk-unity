#nullable enable
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxInstallReferrerStore
    {
        private readonly System.Func<string, string> _key;
        private string _cachedInstallReferrer = string.Empty;
        private bool _loadedInstallReferrerCache;

        public AttriaxInstallReferrerStore(System.Func<string, string> key)
        {
            _key = key;
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
                _key("installReferrer"),
                string.Empty);
            return string.IsNullOrWhiteSpace(_cachedInstallReferrer) ? null : _cachedInstallReferrer;
        }

        public AttriaxInstallReferrerDetails? ReadPersistedInstallReferrerDetails(string storageKey)
        {
            var serialized = AttriaxPlayerPrefs.GetString(_key(storageKey), string.Empty);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<AttriaxInstallReferrerDetails>(serialized);
            }
            catch (JsonException)
            {
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
            AttriaxPlayerPrefs.SetString(_key("installReferrer"), installReferrer);
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
                _key(storageKey),
                JsonConvert.SerializeObject(installReferrerDetails));
            AttriaxPlayerPrefs.Save();
        }
    }
}
