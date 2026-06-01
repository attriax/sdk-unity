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
            Assert.That(typeof(Attriax).GetMethod("SetUserAsync", PublicInstanceDeclaredOnly), Is.Null);
            Assert.That(typeof(Attriax).GetMethod("RecordErrorAsync", PublicInstanceDeclaredOnly), Is.Null);
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
            Assert.That(typeof(AttriaxTracking).GetMethod("TrackEventAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordPurchaseAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordRefundAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdRevenueAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordAdEventAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("RecordErrorAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("TrackPageViewAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserPropertyAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("SetUserPropertiesAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
            Assert.That(typeof(AttriaxTracking).GetMethod("ClearUserPropertiesAsync", PublicInstanceDeclaredOnly), Is.Not.Null);
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