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
        /// Enables or disables event dispatching while keeping the instance alive.
        /// </summary>
        internal bool EventsEnabled
        {
            get { return _runtime.EventsEnabled; }
            set { _runtime.SetEventsEnabled(value); }
        }

        internal bool AnonymousTrackingEnabled
        {
            get { return _runtime.AnonymousTrackingEnabled; }
            set { _runtime.SetAnonymousTrackingEnabled(value); }
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
        /// Queues a custom event for delivery to the Attriax backend.
        /// </summary>
        internal Task TrackEventAsync(string eventName, AttriaxTrackEventOptions? options = null)
        {
            return _runtime.TrackEventAsync(eventName, options ?? new AttriaxTrackEventOptions());
        }

        /// <summary>
        /// Queues a standardized purchase revenue event for delivery to Attriax.
        /// Use a negative revenue amount to report refunds or downward adjustments.
        /// </summary>
        internal Task RecordPurchaseAsync(double revenue, AttriaxRecordPurchaseOptions? options = null)
        {
            ValidateRevenue(revenue);
            options ??= new AttriaxRecordPurchaseOptions();

            if (options.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Quantity), "Quantity must be positive.");
            }

            var normalizedRevenue = NormalizeRevenueCurrency(revenue, options.Currency);
            var eventData = CloneMetadata(options.Metadata);
            eventData["revenue"] = normalizedRevenue.Revenue;
            eventData["currency"] = normalizedRevenue.Currency;

            if (options.RevenueInMicros)
            {
                eventData["revenueInMicros"] = true;
            }

            AddTrimmed(eventData, "purchaseType", options.PurchaseType);
            AddTrimmed(eventData, "productId", options.ProductId);
            AddTrimmed(eventData, "transactionId", options.TransactionId);
            AddTrimmed(eventData, "originalTransactionId", options.OriginalTransactionId);
            AddTrimmed(eventData, "validationProvider", options.ValidationProvider);
            AddTrimmed(eventData, "validationEnvironment", options.ValidationEnvironment);
            AddTrimmed(eventData, "purchaseToken", options.PurchaseToken);
            AddTrimmed(eventData, "receiptData", options.ReceiptData);
            AddTrimmed(eventData, "signedPayload", options.SignedPayload);
            AddTrimmed(eventData, "receiptSignature", options.ReceiptSignature);

            if (options.IsRenewal.HasValue)
            {
                eventData["isRenewal"] = options.IsRenewal.Value;
            }

            if (options.Quantity != 1)
            {
                eventData["quantity"] = options.Quantity;
            }

            AddTrimmed(eventData, "store", options.Store);
            AddTrimmed(eventData, "packageName", options.PackageName);

            if (options.Voided.HasValue)
            {
                eventData["voided"] = options.Voided.Value;
            }

            if (options.Test.HasValue)
            {
                eventData["test"] = options.Test.Value;
            }

            AddTrimmed(eventData, "validationId", options.ValidationId);

            return TrackEventAsync("purchase", new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately ?? true,
            });
        }

        /// <summary>
        /// Queues a standardized refund revenue event for delivery to Attriax.
        /// </summary>
        internal Task RecordRefundAsync(double revenue, AttriaxRecordRefundOptions? options = null)
        {
            ValidateRevenue(revenue);
            options ??= new AttriaxRecordRefundOptions();

            if (options.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Quantity), "Quantity must be positive.");
            }

            var normalizedRevenue = NormalizeRevenueCurrency(revenue, options.Currency);
            var eventData = CloneMetadata(options.Metadata);
            eventData["revenue"] = normalizedRevenue.Revenue == 0d
                ? 0d
                : -Math.Abs(normalizedRevenue.Revenue);
            eventData["currency"] = normalizedRevenue.Currency;
            eventData["revenueType"] = "refund";

            if (options.RevenueInMicros)
            {
                eventData["revenueInMicros"] = true;
            }

            AddTrimmed(eventData, "purchaseType", options.PurchaseType);
            AddTrimmed(eventData, "productId", options.ProductId);
            AddTrimmed(eventData, "transactionId", options.TransactionId);
            AddTrimmed(eventData, "originalTransactionId", options.OriginalTransactionId);

            if (options.Quantity != 1)
            {
                eventData["quantity"] = options.Quantity;
            }

            AddTrimmed(eventData, "store", options.Store);
            AddTrimmed(eventData, "packageName", options.PackageName);

            if (options.Voided.HasValue)
            {
                eventData["voided"] = options.Voided.Value;
            }

            if (options.Test.HasValue)
            {
                eventData["test"] = options.Test.Value;
            }

            AddTrimmed(eventData, "reason", options.Reason);

            return TrackEventAsync("refund", new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately ?? true,
            });
        }

        /// <summary>
        /// Validates a purchase receipt immediately and returns the public result.
        /// </summary>
        public Task<AttriaxRevenueReceiptValidationResult> ValidateReceiptAsync(
            AttriaxValidateReceiptOptions? options = null)
        {
            return _runtime.ValidateReceiptAsync(options ?? new AttriaxValidateReceiptOptions());
        }

        /// <summary>
        /// Registers the current Firebase Cloud Messaging token for uninstall tracking.
        /// Call this after your app receives an FCM token and again whenever Firebase rotates it.
        /// Pass <c>null</c> or whitespace to clear the currently registered FCM uninstall token for this device.
        /// Attriax currently supports this flow on Android and iOS. On Apple platforms,
        /// Firebase must already map the APNs device token to the FCM registration token.
        /// </summary>
        public Task RegisterFirebaseMessagingTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            return _runtime.RegisterFirebaseMessagingTokenAsync(token, metadata);
        }

        /// <summary>
        /// Registers the current Apple Push Notification service token for uninstall tracking.
        /// Call this after your app receives an APNs device token and again whenever Apple rotates it.
        /// Pass <c>null</c> or whitespace to clear the currently registered APNs uninstall token for this device.
        /// Attriax currently supports this flow on Apple platforms only.
        /// </summary>
        public Task RegisterApplePushTokenAsync(
            string? token,
            IDictionary<string, object>? metadata = null)
        {
            return _runtime.RegisterApplePushTokenAsync(token, metadata);
        }

        /// <summary>
        /// Queues a standardized ad revenue event for delivery to Attriax.
        /// </summary>
        internal Task RecordAdRevenueAsync(double revenue, AttriaxRecordAdRevenueOptions? options = null)
        {
            ValidateRevenue(revenue);
            options ??= new AttriaxRecordAdRevenueOptions();

            var normalizedRevenue = NormalizeRevenueCurrency(revenue, options.Currency);
            var eventData = CloneMetadata(options.Metadata);
            eventData["revenue"] = normalizedRevenue.Revenue;
            eventData["currency"] = normalizedRevenue.Currency;

            if (options.RevenueInMicros)
            {
                eventData["revenueInMicros"] = true;
            }

            AddTrimmed(eventData, "adNetwork", options.AdNetwork);
            AddTrimmed(eventData, "adFormat", options.AdFormat);
            AddTrimmed(eventData, "adType", options.AdType);
            AddTrimmed(eventData, "adPlacement", options.AdPlacement);

            if (options.Test.HasValue)
            {
                eventData["test"] = options.Test.Value;
            }

            return TrackEventAsync("ad_revenue", new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately ?? true,
            });
        }

        /// <summary>
        /// Queues a canonical ad lifecycle event for delivery to Attriax.
        /// </summary>
        internal Task RecordAdEventAsync(AttriaxAdEventType type, AttriaxRecordAdEventOptions? options = null)
        {
            options ??= new AttriaxRecordAdEventOptions();

            var eventData = CloneMetadata(options.Metadata);
            AddTrimmed(eventData, "adNetwork", options.AdNetwork);
            AddTrimmed(eventData, "mediationNetwork", options.MediationNetwork);
            AddTrimmed(eventData, "adUnitId", options.AdUnitId);
            AddTrimmed(eventData, "adPlacement", options.AdPlacement);
            AddTrimmed(eventData, "adFormat", options.AdFormat);
            AddTrimmed(eventData, "adType", options.AdType);
            AddTrimmed(eventData, "failureReason", options.FailureReason);
            AddTrimmed(eventData, "rewardType", options.RewardType);

            if (options.LoadLatencyMs.HasValue)
            {
                ValidateMetric(options.LoadLatencyMs.Value, nameof(options.LoadLatencyMs));
                eventData["loadLatencyMs"] = options.LoadLatencyMs.Value;
            }

            if (options.RewardAmount.HasValue)
            {
                ValidateMetric(options.RewardAmount.Value, nameof(options.RewardAmount));
                eventData["rewardAmount"] = options.RewardAmount.Value;
            }

            if (options.Test.HasValue)
            {
                eventData["test"] = options.Test.Value;
            }

            return TrackEventAsync(MapAdEventName(type), new AttriaxTrackEventOptions
            {
                EventData = eventData,
                FlushImmediately = options.FlushImmediately ?? true,
            });
        }

        /// <summary>
        /// Records an exception or crash payload for later delivery to the Attriax backend.
        /// </summary>
        internal Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions? options = null)
        {
            return _runtime.RecordErrorAsync(error, options ?? new AttriaxRecordErrorOptions());
        }

        /// <summary>
        /// Tracks a standardized page-view event.
        /// </summary>
        internal Task TrackPageViewAsync(string pageName, AttriaxPageViewOptions? options = null)
        {
            return _runtime.TrackPageViewAsync(pageName, options ?? new AttriaxPageViewOptions());
        }

        /// <summary>
        /// Applies an external user id and default user metadata to future tracked events.
        /// Passing null clears the currently associated external user id.
        /// </summary>
        internal Task SetUserAsync(string? externalUserId, AttriaxSetUserOptions? options = null)
        {
            return _runtime.SetUserAsync(externalUserId, options ?? new AttriaxSetUserOptions());
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="SetUserAsync"/>.
        /// </summary>
        internal Task IdentifyAsync(string? externalUserId, AttriaxIdentifyOptions? options = null)
        {
            return SetUserAsync(externalUserId, options ?? new AttriaxIdentifyOptions());
        }

        /// <summary>
        /// Sets or clears a single default user property for future tracked events.
        /// </summary>
        internal Task SetUserPropertyAsync(string name, object? value)
        {
            return _runtime.SetUserPropertyAsync(name, value);
        }

        /// <summary>
        /// Merges multiple default user properties for future tracked events.
        /// Null values clear individual property keys.
        /// </summary>
        internal Task SetUserPropertiesAsync(IDictionary<string, object?> properties)
        {
            return _runtime.SetUserPropertiesAsync(properties);
        }

        /// <summary>
        /// Clears all default user properties or only the requested property names.
        /// </summary>
        internal Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null)
        {
            return _runtime.ClearUserPropertiesAsync(propertyNames);
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

        private static NormalizedRevenue NormalizeRevenueCurrency(double revenue, string currency)
        {
            var normalizedCurrency = string.IsNullOrWhiteSpace(currency)
                ? string.Empty
                : currency.Trim().ToUpperInvariant();

            if (IsValidCurrencyCode(normalizedCurrency))
            {
                return new NormalizedRevenue(revenue, normalizedCurrency);
            }

            Debug.LogWarning(
                $"[Attriax][WARNING] Invalid revenue currency \"{currency}\"; defaulting revenue to 0 USD.");
            return new NormalizedRevenue(0d, "USD");
        }

        private static bool IsValidCurrencyCode(string currency)
        {
            if (currency.Length != 3)
            {
                return false;
            }

            for (var index = 0; index < currency.Length; index += 1)
            {
                if (currency[index] < 'A' || currency[index] > 'Z')
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateRevenue(double revenue)
        {
            if (double.IsNaN(revenue) || double.IsInfinity(revenue))
            {
                throw new ArgumentOutOfRangeException(nameof(revenue), "Revenue must be finite.");
            }
        }

        private static void ValidateMetric(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, $"{name} must be finite.");
            }
        }

        private static string MapAdEventName(AttriaxAdEventType type)
        {
            return type switch
            {
                AttriaxAdEventType.Request => "ad_request",
                AttriaxAdEventType.Load => "ad_load",
                AttriaxAdEventType.LoadFailed => "ad_load_failed",
                AttriaxAdEventType.Show => "ad_show",
                AttriaxAdEventType.ShowFailed => "ad_show_failed",
                AttriaxAdEventType.Impression => "ad_impression",
                AttriaxAdEventType.Click => "ad_click",
                AttriaxAdEventType.Dismiss => "ad_dismiss",
                AttriaxAdEventType.Reward => "ad_reward",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported ad event type."),
            };
        }

        /// <summary>
        /// Releases the SDK instance and pending subscriptions.
        /// </summary>
        public void Dispose()
        {
            _runtime.Dispose();
        }

        private readonly struct NormalizedRevenue
        {
            public NormalizedRevenue(double revenue, string currency)
            {
                Revenue = revenue;
                Currency = currency;
            }

            public double Revenue { get; }

            public string Currency { get; }
        }
    }
}