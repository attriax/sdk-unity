#nullable enable
using System;
using Newtonsoft.Json;

namespace Attriax.Unity.Internal
{
    internal interface IAttriaxSessionStore
    {
        AttriaxSessionSnapshot? ReadSessionSnapshot();

        void WriteSessionSnapshot(AttriaxSessionSnapshot? session);
    }

    internal sealed class AttriaxPlayerPrefsSessionStore : IAttriaxSessionStore
    {
        private readonly string _storageKey;
        private readonly Action<string, string?> _debugLog;

        public AttriaxPlayerPrefsSessionStore(string storageKey, Action<string, string?> debugLog)
        {
            _storageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
            _debugLog = debugLog ?? throw new ArgumentNullException(nameof(debugLog));
        }

        public AttriaxSessionSnapshot? ReadSessionSnapshot()
        {
            var serialized = AttriaxPlayerPrefs.GetString(_storageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return null;
            }

            try
            {
                var session = JsonConvert.DeserializeObject<AttriaxSessionSnapshot>(serialized);
                if (session == null ||
                    string.IsNullOrWhiteSpace(session.Id) ||
                    string.IsNullOrWhiteSpace(session.DeviceId) ||
                    string.IsNullOrWhiteSpace(session.SdkPackageVersion) ||
                    session.HeartbeatIntervalMs <= 0 ||
                    session.LastActivityAt < session.StartedAt)
                {
                    return null;
                }

                return session;
            }
            catch (JsonException exception)
            {
                _debugLog("Failed to parse persisted session snapshot. Discarding it.", exception.Message);
                return null;
            }
        }

        public void WriteSessionSnapshot(AttriaxSessionSnapshot? session)
        {
            if (session == null)
            {
                AttriaxPlayerPrefs.DeleteKey(_storageKey);
                AttriaxPlayerPrefs.Save();
                return;
            }

            AttriaxPlayerPrefs.SetString(_storageKey, JsonConvert.SerializeObject(session));
            AttriaxPlayerPrefs.Save();
        }
    }
}