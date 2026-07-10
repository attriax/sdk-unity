#nullable enable
using System;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxRuntimeSettingsStore
    {
        private readonly string _deviceIdKey;
        private readonly string _deviceIdSourceKey;
        private readonly string _enabledKey;
        private readonly string _eventsEnabledKey;
        private readonly string _hasLaunchedKey;

        public AttriaxRuntimeSettingsStore(
            string deviceIdKey,
            string deviceIdSourceKey,
            string enabledKey,
            string eventsEnabledKey,
            string hasLaunchedKey)
        {
            _deviceIdKey = deviceIdKey ?? throw new ArgumentNullException(nameof(deviceIdKey));
            _deviceIdSourceKey = deviceIdSourceKey ?? throw new ArgumentNullException(nameof(deviceIdSourceKey));
            _enabledKey = enabledKey ?? throw new ArgumentNullException(nameof(enabledKey));
            _eventsEnabledKey = eventsEnabledKey ?? throw new ArgumentNullException(nameof(eventsEnabledKey));
            _hasLaunchedKey = hasLaunchedKey ?? throw new ArgumentNullException(nameof(hasLaunchedKey));
        }

        public bool ReadEnabled(bool defaultValue)
        {
            return ReadBoolean(_enabledKey, defaultValue);
        }

        public void WriteEnabled(bool enabled)
        {
            WriteBoolean(_enabledKey, enabled);
        }

        public bool ReadEventsEnabled(bool defaultValue)
        {
            return ReadBoolean(_eventsEnabledKey, defaultValue);
        }

        public void WriteEventsEnabled(bool enabled)
        {
            WriteBoolean(_eventsEnabledKey, enabled);
        }

        public bool ReadHasLaunched(bool defaultValue)
        {
            return ReadBoolean(_hasLaunchedKey, defaultValue);
        }

        public void WriteHasLaunched(bool hasLaunched)
        {
            WriteBoolean(_hasLaunchedKey, hasLaunched);
        }

        public string? ReadDeviceId()
        {
            return AttriaxPlayerPrefs.GetString(_deviceIdKey, null);
        }

        public void WriteDeviceId(string deviceId)
        {
            AttriaxPlayerPrefs.SetString(_deviceIdKey, deviceId);
            AttriaxPlayerPrefs.Save();
        }

        public string? ReadDeviceIdSource()
        {
            return AttriaxPlayerPrefs.GetString(_deviceIdSourceKey, null);
        }

        public void WriteDeviceIdSource(string? deviceIdSource)
        {
            if (string.IsNullOrWhiteSpace(deviceIdSource))
            {
                AttriaxPlayerPrefs.DeleteKey(_deviceIdSourceKey);
            }
            else
            {
                AttriaxPlayerPrefs.SetString(_deviceIdSourceKey, deviceIdSource);
            }

            AttriaxPlayerPrefs.Save();
        }

        public void Clear()
        {
            AttriaxPlayerPrefs.DeleteKey(_deviceIdKey);
            AttriaxPlayerPrefs.DeleteKey(_deviceIdSourceKey);
            AttriaxPlayerPrefs.DeleteKey(_enabledKey);
            AttriaxPlayerPrefs.DeleteKey(_eventsEnabledKey);
            AttriaxPlayerPrefs.DeleteKey(_hasLaunchedKey);
            AttriaxPlayerPrefs.Save();
        }

        private static bool ReadBoolean(string key, bool defaultValue)
        {
            if (!AttriaxPlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            return AttriaxPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        private static void WriteBoolean(string key, bool value)
        {
            AttriaxPlayerPrefs.SetInt(key, value ? 1 : 0);
            AttriaxPlayerPrefs.Save();
        }
    }
}