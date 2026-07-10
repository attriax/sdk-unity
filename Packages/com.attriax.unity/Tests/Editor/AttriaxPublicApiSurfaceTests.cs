#nullable enable
using System.Reflection;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxPublicApiSurfaceTests
    {
        private const BindingFlags PublicInstanceDeclaredOnly =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        [Test]
        public void RootApiExposesGroupedFacadesInsteadOfLegacyTrackingMembers()
        {
            Assert.That(typeof(Attriax).GetProperty(nameof(Attriax.Tracking), PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(Attriax).GetProperty("EventsEnabled", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("TrackEventAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RecordEventAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("SetUserAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RecordErrorAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RegisterFirebaseMessagingTokenAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RegisterApplePushTokenAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("CreateDynamicLinkAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RecordDeepLinkConversionAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RequestTrackingAuthorizationAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("GetTrackingAuthorizationStatusAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("WaitForAppOpenTrackingAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("FlushAsync", PublicInstanceDeclaredOnly), Is.Null);
        }

        [Test]
        public void TrackingFacadeOwnsTrackingAndIdentityMembers()
        {
            Assert.That(typeof(AttriaxTracking).GetProperty("Enabled", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetProperty("AnonymousTrackingEnabled", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordEvent", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordPurchase", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordRefund", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdRevenue", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdEvent", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordError", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordPageView", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordNotification", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordNotificationReceived", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordNotificationOpened", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordNotificationDismissed", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RegisterFirebaseMessagingTokenAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RegisterApplePushTokenAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUser", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserProperty", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserProperties", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("ClearUserProperties", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordEventAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordPurchaseAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordRefundAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdRevenueAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdEventAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordErrorAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordPageViewAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserPropertyAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserPropertiesAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("ClearUserPropertiesAsync", PublicInstanceDeclaredOnly), Is.Null);
        }

        [Test]
        public void DeepLinksAndConsentFacadesOwnEntrySpecificHelpers()
        {
            Assert.That(typeof(AttriaxDeepLinks).GetMethod("CreateDynamicLinkAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxDeepLinks).GetMethod("RecordDeepLinkConversionAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxGdprConsent).GetMethod("RequestDataErasureAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxAttConsent).GetMethod("RequestTrackingAuthorizationAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxAttConsent).GetMethod("GetTrackingAuthorizationStatusAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
        }
    }
}