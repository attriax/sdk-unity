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
        public Task TrackEventAsync(string eventName, AttriaxTrackEventOptions? options = null)
        {
            return _attriax.TrackEventAsync(eventName, options ?? new AttriaxTrackEventOptions());
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
        public Task TrackPageViewAsync(string pageName, AttriaxPageViewOptions? options = null)
        {
            return _attriax.TrackPageViewAsync(pageName, options ?? new AttriaxPageViewOptions());
        }

        /// <summary>
        /// Applies an external user id and default user metadata to future tracked events.
        /// Passing null clears the currently associated external user id.
        /// </summary>
        public Task SetUserAsync(string? externalUserId, AttriaxSetUserOptions? options = null)
        {
            return _attriax.SetUserAsync(externalUserId, options ?? new AttriaxSetUserOptions());
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="SetUserAsync"/> kept on the tracking facade.
        /// </summary>
        [Obsolete("Use Tracking.SetUserAsync(...) instead.")]
        public Task IdentifyAsync(string? externalUserId, AttriaxIdentifyOptions? options = null)
        {
            return _attriax.IdentifyAsync(externalUserId, options ?? new AttriaxIdentifyOptions());
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