#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    internal static class AttriaxConfiguredRuntime
    {
        internal static AttriaxProjectSettings? Settings
        {
            get { return Resources.Load<AttriaxProjectSettings>(AttriaxProjectSettings.ResourcesPath); }
        }

        internal static bool HasConfiguredSettings => Settings != null;

        internal static Attriax Configured
        {
            get
            {
                var host = EnsureHost();
                if (host.Instance == null)
                {
                    throw new InvalidOperationException(
                        "Configured Attriax instance is unavailable.");
                }

                return host.Instance;
            }
        }

        internal static async Task<Attriax> InitializeConfiguredAsync()
        {
            var host = EnsureHost();
            await host.InitializeIfNeededAsync();
            if (host.Instance == null)
            {
                throw new InvalidOperationException(
                    "Configured Attriax instance is unavailable after initialization.");
            }

            return host.Instance;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapConfiguredRuntime()
        {
            var settings = Settings;
            if (settings == null || !settings.AutoInitializeOnLaunch)
            {
                return;
            }

            try
            {
                var host = AttriaxConfiguredHost.EnsureCreated(settings);
                _ = host.InitializeIfNeededAsync();
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogError(
                    "[Attriax] Automatic SDK bootstrap failed. The SDK will not auto-initialize for this session. "
                    + error.Message);
            }
        }

        private static AttriaxConfiguredHost EnsureHost()
        {
            var settings = Settings;
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "Attriax project settings were not found. Create Assets/Resources/Attriax/AttriaxSettings.asset from the Attriax configuration window first.");
            }

            return AttriaxConfiguredHost.EnsureCreated(settings);
        }
    }
}