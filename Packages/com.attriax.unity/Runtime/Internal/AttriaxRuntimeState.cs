#nullable enable

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxRuntimeState
    {
        public bool IsInitialized { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool AreEventsEnabled { get; set; } = true;

        public bool IsFirstLaunch { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public string DeviceIdSource { get; set; } = string.Empty;

        public void Reset()
        {
            IsInitialized = false;
            IsEnabled = true;
            AreEventsEnabled = true;
            IsFirstLaunch = false;
            DeviceId = string.Empty;
            DeviceIdSource = string.Empty;
        }
    }
}