#nullable enable
using System;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxStoredConsentState
    {
        public string? State { get; set; }

        public AttriaxGdprConsentValues? Values { get; set; }

        public string? CheckedAt { get; set; }

        public string? CountryCode { get; set; }

        public string? RegionSource { get; set; }

        public bool PendingSync { get; set; }
    }

    internal interface IAttriaxConsentStore
    {
        AttriaxStoredConsentState? ReadConsentState();

        void WriteConsentState(AttriaxStoredConsentState state);

        void ClearConsentState();
    }

    internal sealed class AttriaxPlayerPrefsConsentStore : IAttriaxConsentStore
    {
        private readonly string _storageKey;
        private readonly Action<string, string?> _debugLog;

        public AttriaxPlayerPrefsConsentStore(string storageKey, Action<string, string?> debugLog)
        {
            _storageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
            _debugLog = debugLog ?? throw new ArgumentNullException(nameof(debugLog));
        }

        public AttriaxStoredConsentState? ReadConsentState()
        {
            if (!AttriaxPlayerPrefs.HasKey(_storageKey))
            {
                return null;
            }

            var raw = AttriaxPlayerPrefs.GetString(_storageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<AttriaxStoredConsentState>(raw);
            }
            catch (Exception error)
            {
                _debugLog("Failed to parse persisted GDPR consent state. Discarding it.", error.Message);
                ClearConsentState();
                return null;
            }
        }

        public void WriteConsentState(AttriaxStoredConsentState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            AttriaxPlayerPrefs.SetString(_storageKey, JsonConvert.SerializeObject(state));
            AttriaxPlayerPrefs.Save();
        }

        public void ClearConsentState()
        {
            AttriaxPlayerPrefs.DeleteKey(_storageKey);
            AttriaxPlayerPrefs.Save();
        }
    }
}