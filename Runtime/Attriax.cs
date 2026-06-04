#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attriax.Unity.Internal;
using UnityEngine;

namespace Attriax.Unity
{
    /// <summary>
    /// Primary runtime entry point for the Attriax Unity SDK.
    /// </summary>
    public sealed class Attriax : IDisposable
    {
        private readonly AttriaxRuntime _runtime;

        /// <summary>
        /// Creates a new SDK instance from an explicit runtime configuration.
        /// </summary>
        public Attriax(AttriaxConfig config)
        {
            _runtime = new AttriaxRuntime(config);
            Consent = new AttriaxConsent(_runtime);
            Synchronization = new AttriaxSynchronization(_runtime);
            Tracking = new AttriaxTracking(this);
            DeepLinks = new AttriaxDeepLinks(_runtime);
            Referrer = new AttriaxReferrer(_runtime);
            Skan = new AttriaxSkan(_runtime);
        }

        /// <summary>
        /// Gets the saved project configuration asset if one exists under Resources.
        /// </summary>
        public static AttriaxProjectSettings? ConfiguredSettings =>
            AttriaxConfiguredRuntime.Settings;

        /// <summary>
        /// Returns <see langword="true"/> when a saved project configuration is available.
        /// </summary>
        public static bool HasConfiguredSettings => AttriaxConfiguredRuntime.HasConfiguredSettings;

        /// <summary>
        /// Gets the singleton SDK instance created from the saved project configuration.
        /// </summary>
        public static Attriax Configured => AttriaxConfiguredRuntime.Configured;

        /// <summary>
        /// Ensures the configured singleton instance is created and initialized.
        /// </summary>
        public static Task<Attriax> InitializeConfiguredAsync()
        {
            return AttriaxConfiguredRuntime.InitializeConfiguredAsync();
        }

        /// <summary>
        /// Regulation-scoped consent helpers for the current instance.
        /// </summary>
        public AttriaxConsent Consent { get; }

        /// <summary>
        /// Gets synchronization state and subscriptions for the current instance.
        /// </summary>
        public AttriaxSynchronization Synchronization { get; }

        /// <summary>
        /// Event tracking, revenue, identity, and handled-error helpers for the current instance.
        /// </summary>
        public AttriaxTracking Tracking { get; }

        /// <summary>
        /// Deep-link state and subscriptions for initial, live, and deferred links.
        /// </summary>
        public AttriaxDeepLinks DeepLinks { get; }

        /// <summary>
        /// Startup referrer state retained for this device.
        /// </summary>
        public AttriaxReferrer Referrer { get; }

        /// <summary>
        /// Dashboard-managed SKAN state and manual update helpers for the current instance.
        /// </summary>
        public AttriaxSkan Skan { get; }

        /// <summary>
        /// Returns the normalized runtime configuration for this instance.
        /// </summary>
        public AttriaxConfig Config
        {
            get { return _runtime.Config; }
        }

        /// <summary>
        /// Returns <see langword="true"/> once initialization has completed.
        /// </summary>
        public bool IsInitialized
        {
            get { return _runtime.IsInitialized; }
        }

        /// <summary>
        /// Enables or disables the SDK globally for this instance.
        /// </summary>
        public bool Enabled
        {
            get { return _runtime.Enabled; }
            set { _runtime.SetEnabled(value); }
        }

        /// <summary>
        /// Returns <see langword="true"/> for the first open tracked on this device.
        /// </summary>
        public bool IsFirstLaunch
        {
            get { return _runtime.IsFirstLaunch; }
        }

        /// <summary>
        /// Returns the stable Attriax device identifier persisted for this runtime.
        /// </summary>
        public string? DeviceId
        {
            get { return _runtime.DeviceId; }
        }

        /// <summary>
        /// Returns the SDK version and metadata snapshot captured during initialization.
        /// </summary>
        public AttriaxSdkSnapshot? SdkSnapshot
        {
            get { return _runtime.SdkSnapshot; }
        }

        /// <summary>
        /// Initializes the SDK instance, captures runtime context, and schedules the
        /// standard app-open request in the background when the SDK is enabled.
        /// </summary>
        public Task InitializeAsync(AttriaxInitOptions? options = null)
        {
            return _runtime.InitializeAsync(options ?? new AttriaxInitOptions());
        }

        /// <summary>
        /// Clears SDK-owned persisted state and returns this instance to pre-init state.
        /// Call <see cref="InitializeAsync"/> again before reusing the instance.
        /// </summary>
        public Task ResetAsync()
        {
            return _runtime.ResetAsync();
        }

        /// <summary>
        /// Validates a purchase receipt immediately and returns the public result.
        /// </summary>
        public Task<AttriaxRevenueReceiptValidationResult> ValidateReceiptAsync(
            AttriaxValidateReceiptOptions? options = null)
        {
            return _runtime.ValidateReceiptAsync(options ?? new AttriaxValidateReceiptOptions());
        }

        private static Dictionary<string, object> CloneMetadata(IDictionary<string, object>? metadata)
        {
            if (metadata == null || metadata.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            return new Dictionary<string, object>(metadata);
        }

        private static void AddTrimmed(IDictionary<string, object> target, string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            target[key] = value.Trim();
        }

        /// <summary>
        /// Releases the SDK instance and pending subscriptions.
        /// </summary>
        public void Dispose()
        {
            _runtime.Dispose();
        }
    }
}