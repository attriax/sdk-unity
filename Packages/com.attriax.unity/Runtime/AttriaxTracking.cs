#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Attriax.Unity.Internal;

namespace Attriax.Unity
{
    /// <summary>
    /// Event tracking, revenue, identity, and handled-error helpers exposed by the Unity SDK.
    /// </summary>
    public sealed class AttriaxTracking
    {
        private readonly IAttriaxEngine _attriax;

        internal AttriaxTracking(IAttriaxEngine attriax)
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
        public void RecordEvent(string eventName, AttriaxTrackEventOptions? options = null)
        {
            FireAndForget(_attriax.RecordEventAsync(eventName, options ?? new AttriaxTrackEventOptions()));
        }

        /// <summary>
        /// Queues a standardized purchase revenue event for delivery to Attriax.
        /// </summary>
        public void RecordPurchase(double revenue, AttriaxRecordPurchaseOptions? options = null)
        {
            FireAndForget(_attriax.RecordPurchaseAsync(revenue, options ?? new AttriaxRecordPurchaseOptions()));
        }

        /// <summary>
        /// Queues a standardized refund revenue event for delivery to Attriax.
        /// </summary>
        public void RecordRefund(double revenue, AttriaxRecordRefundOptions? options = null)
        {
            FireAndForget(_attriax.RecordRefundAsync(revenue, options ?? new AttriaxRecordRefundOptions()));
        }

        /// <summary>
        /// Queues a standardized ad revenue event for delivery to Attriax.
        /// </summary>
        public void RecordAdRevenue(double revenue, AttriaxRecordAdRevenueOptions? options = null)
        {
            FireAndForget(_attriax.RecordAdRevenueAsync(revenue, options ?? new AttriaxRecordAdRevenueOptions()));
        }

        /// <summary>
        /// Queues a canonical ad lifecycle event for delivery to Attriax.
        /// </summary>
        public void RecordAdEvent(AttriaxAdEventType type, AttriaxRecordAdEventOptions? options = null)
        {
            FireAndForget(_attriax.RecordAdEventAsync(type, options ?? new AttriaxRecordAdEventOptions()));
        }

        /// <summary>
        /// Records an exception or crash payload for later delivery to the Attriax backend.
        /// </summary>
        public void RecordError(Exception error, AttriaxRecordErrorOptions? options = null)
        {
            FireAndForget(_attriax.RecordErrorAsync(error, options ?? new AttriaxRecordErrorOptions()));
        }

        /// <summary>
        /// Tracks a standardized page-view event.
        /// </summary>
        public void RecordPageView(string pageName, AttriaxPageViewOptions? options = null)
        {
            FireAndForget(_attriax.RecordPageViewAsync(pageName, options ?? new AttriaxPageViewOptions()));
        }

        /// <summary>
        /// Records a push-notification lifecycle event for attribution.
        ///
        /// Attriax never sends pushes itself: call this from the host app's own
        /// FCM/APNs handler, threading through any Attriax linkId/campaignId reference
        /// embedded in the notification payload. Pass the raw FCM/APNs data map as
        /// <see cref="AttriaxRecordNotificationOptions.Payload"/> and it is preserved
        /// under a <c>payload</c> key in the notification metadata.
        ///
        /// Routes through the same offline-persisted, batched, retried queue as
        /// <see cref="RecordEvent"/>, and honors the same app-open-first / consent semantics.
        /// </summary>
        public void RecordNotification(
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxRecordNotificationOptions? options = null)
        {
            FireAndForget(_attriax.RecordNotificationAsync(
                type,
                notificationId,
                options ?? new AttriaxRecordNotificationOptions()));
        }

        /// <summary>
        /// Records that a push notification was received / displayed.
        /// </summary>
        public void RecordNotificationReceived(
            string notificationId,
            AttriaxRecordNotificationOptions? options = null)
        {
            RecordNotification(AttriaxNotificationEventType.Received, notificationId, options);
        }

        /// <summary>
        /// Records that a push notification was opened (tapped).
        /// </summary>
        public void RecordNotificationOpened(
            string notificationId,
            AttriaxRecordNotificationOptions? options = null)
        {
            RecordNotification(AttriaxNotificationEventType.Opened, notificationId, options);
        }

        /// <summary>
        /// Records that a push notification was dismissed without opening.
        /// </summary>
        public void RecordNotificationDismissed(
            string notificationId,
            AttriaxRecordNotificationOptions? options = null)
        {
            RecordNotification(AttriaxNotificationEventType.Dismissed, notificationId, options);
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
        public void SetUser(string? userId, AttriaxSetUserOptions? options = null)
        {
            FireAndForget(_attriax.SetUserAsync(userId, options ?? new AttriaxSetUserOptions()));
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="SetUser"/> kept on the tracking API.
        /// </summary>
        [Obsolete("Use Tracking.SetUser(...) instead.")]
        public void Identify(string? userId, AttriaxIdentifyOptions? options = null)
        {
            FireAndForget(_attriax.IdentifyAsync(userId, options ?? new AttriaxIdentifyOptions()));
        }

        /// <summary>
        /// Sets or clears a single default user property for future tracked events.
        /// </summary>
        public void SetUserProperty(string name, object? value)
        {
            FireAndForget(_attriax.SetUserPropertyAsync(name, value));
        }

        /// <summary>
        /// Merges multiple default user properties for future tracked events.
        /// Null values clear individual property keys.
        /// </summary>
        public void SetUserProperties(IDictionary<string, object?> properties)
        {
            FireAndForget(_attriax.SetUserPropertiesAsync(properties));
        }

        /// <summary>
        /// Clears all default user properties or only the requested property names.
        /// </summary>
        public void ClearUserProperties(IReadOnlyCollection<string>? propertyNames = null)
        {
            FireAndForget(_attriax.ClearUserPropertiesAsync(propertyNames));
        }

        private static async void FireAndForget(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
