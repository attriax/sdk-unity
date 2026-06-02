#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attriax.Unity
{
    /// <summary>
    /// Event tracking, revenue, identity, and handled-error helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxTracking
    {
        private readonly Attriax _attriax;

        internal AttriaxTracking(Attriax attriax)
        {
            _attriax = attriax;
        }

        /// <summary>
        /// Enables or disables event dispatching while keeping the SDK instance alive.
        /// </summary>
        public bool Enabled
        {
            get { return _attriax.EventsEnabled; }
            set { _attriax.EventsEnabled = value; }
        }

        /// <summary>
        /// Whether anonymous-capable GDPR traffic is dispatched before consent resolves.
        /// </summary>
        public bool AnonymousTrackingEnabled
        {
            get { return _attriax.AnonymousTrackingEnabled; }
            set { _attriax.AnonymousTrackingEnabled = value; }
        }

        /// <summary>
        /// Queues a custom event for delivery to the Attriax backend.
        /// </summary>
        public Task RecordEventAsync(string eventName, AttriaxTrackEventOptions? options = null)
        {
            return _attriax.RecordEventAsync(eventName, options ?? new AttriaxTrackEventOptions());
        }

        /// <summary>
        /// Queues a standardized purchase revenue event for delivery to Attriax.
        /// </summary>
        public Task RecordPurchaseAsync(double revenue, AttriaxRecordPurchaseOptions? options = null)
        {
            return _attriax.RecordPurchaseAsync(revenue, options ?? new AttriaxRecordPurchaseOptions());
        }

        /// <summary>
        /// Queues a standardized refund revenue event for delivery to Attriax.
        /// </summary>
        public Task RecordRefundAsync(double revenue, AttriaxRecordRefundOptions? options = null)
        {
            return _attriax.RecordRefundAsync(revenue, options ?? new AttriaxRecordRefundOptions());
        }

        /// <summary>
        /// Queues a standardized ad revenue event for delivery to Attriax.
        /// </summary>
        public Task RecordAdRevenueAsync(double revenue, AttriaxRecordAdRevenueOptions? options = null)
        {
            return _attriax.RecordAdRevenueAsync(revenue, options ?? new AttriaxRecordAdRevenueOptions());
        }

        /// <summary>
        /// Queues a canonical ad lifecycle event for delivery to Attriax.
        /// </summary>
        public Task RecordAdEventAsync(AttriaxAdEventType type, AttriaxRecordAdEventOptions? options = null)
        {
            return _attriax.RecordAdEventAsync(type, options ?? new AttriaxRecordAdEventOptions());
        }

        /// <summary>
        /// Records an exception or crash payload for later delivery to the Attriax backend.
        /// </summary>
        public Task RecordErrorAsync(Exception error, AttriaxRecordErrorOptions? options = null)
        {
            return _attriax.RecordErrorAsync(error, options ?? new AttriaxRecordErrorOptions());
        }

        /// <summary>
        /// Tracks a standardized page-view event.
        /// </summary>
        public Task RecordPageViewAsync(string pageName, AttriaxPageViewOptions? options = null)
        {
            return _attriax.RecordPageViewAsync(pageName, options ?? new AttriaxPageViewOptions());
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
            return _attriax.RegisterFirebaseMessagingTokenAsync(token, metadata);
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
            return _attriax.RegisterApplePushTokenAsync(token, metadata);
        }

        /// <summary>
        /// Applies a user id and default user metadata to future tracked events.
        /// Passing null clears the currently associated user id.
        /// </summary>
        public Task SetUserAsync(string? userId, AttriaxSetUserOptions? options = null)
        {
            return _attriax.SetUserAsync(userId, options ?? new AttriaxSetUserOptions());
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="SetUserAsync"/> kept on the tracking facade.
        /// </summary>
        [Obsolete("Use Tracking.SetUserAsync(...) instead.")]
        public Task IdentifyAsync(string? userId, AttriaxIdentifyOptions? options = null)
        {
            return _attriax.IdentifyAsync(userId, options ?? new AttriaxIdentifyOptions());
        }

        /// <summary>
        /// Sets or clears a single default user property for future tracked events.
        /// </summary>
        public Task SetUserPropertyAsync(string name, object? value)
        {
            return _attriax.SetUserPropertyAsync(name, value);
        }

        /// <summary>
        /// Merges multiple default user properties for future tracked events.
        /// Null values clear individual property keys.
        /// </summary>
        public Task SetUserPropertiesAsync(IDictionary<string, object?> properties)
        {
            return _attriax.SetUserPropertiesAsync(properties);
        }

        /// <summary>
        /// Clears all default user properties or only the requested property names.
        /// </summary>
        public Task ClearUserPropertiesAsync(IReadOnlyCollection<string>? propertyNames = null)
        {
            return _attriax.ClearUserPropertiesAsync(propertyNames);
        }
    }
}