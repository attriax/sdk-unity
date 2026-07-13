#nullable enable
using System.Collections.Generic;
using Attriax.Unity.Internal.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    /// <summary>
    /// Equivalence + drift-contract tests for the shared engine argument DTOs
    /// (<see cref="AttriaxEngineArgs"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Equivalence gate.</b> Each <c>Equivalence_*</c> test rebuilds the argument
    /// map the way the hand-written per-transport code used to build it — the verbatim
    /// <c>Args(...)</c> / <c>Put(...)</c> logic, replicated here as the frozen
    /// specification — serializes it, and asserts it is semantically identical (deep
    /// JToken compare, so key order is irrelevant) to the serialized DTO. This proves
    /// the DTO swap changes nothing on the wire, and because the old-way builder is
    /// frozen in this file, it also fails if a DTO field name / casing / null-omission
    /// ever drifts.
    /// </para>
    /// <para>
    /// <b>Golden contract.</b> The <c>Golden_*</c> tests pin the exact serialized
    /// string for a representative instance of each DTO, catching drift against a
    /// literal fixture the same way the spike's golden test did.
    /// </para>
    /// </remarks>
    public sealed class AttriaxEngineArgsContractTests
    {
        // -----------------------------------------------------------------
        // Frozen replica of the old hand-marshalling helpers.
        // -----------------------------------------------------------------

        private static Dictionary<string, object?> Args(params (string Key, object? Value)[] entries)
        {
            var map = new Dictionary<string, object?>();
            foreach (var (key, value) in entries)
            {
                map[key] = value;
            }

            return map;
        }

        private static void Put(IDictionary<string, object?> args, string key, object? value)
        {
            if (value != null)
            {
                args[key] = value;
            }
        }

        private static void AssertSameWire(object? oldArgs, object newDto)
        {
            var oldJson = oldArgs == null ? "{}" : JsonConvert.SerializeObject(oldArgs);
            var newJson = JsonConvert.SerializeObject(newDto);
            Assert.IsTrue(
                JToken.DeepEquals(JToken.Parse(oldJson), JToken.Parse(newJson)),
                $"DTO wire drifted from the hand-marshalled shape.\n  hand: {oldJson}\n  dto:  {newJson}");
        }

        private static readonly Dictionary<string, object> SampleData = new Dictionary<string, object>
        {
            ["level"] = 7,
            ["score"] = 1234L,
            ["world"] = "forest",
            ["boss"] = true,
        };

        // -----------------------------------------------------------------
        // recordEvent.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordEvent_WithData()
        {
            var old = Args(("name", "level_up"), ("flushImmediately", true));
            Put(old, "eventData", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordEvent("level_up", SampleData, true));
        }

        [Test]
        public void Equivalence_RecordEvent_NullDataOmitted()
        {
            var old = Args(("name", "level_up"), ("flushImmediately", false));
            Put(old, "eventData", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordEvent("level_up", null, false));
        }

        // -----------------------------------------------------------------
        // recordPageView.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordPageView_AllPresent()
        {
            var old = Args(("pageName", "Home"), ("source", "auto"), ("flushImmediately", true));
            Put(old, "pageClass", "HomeView");
            Put(old, "pageTitle", "Home Title");
            Put(old, "previousPageName", "Splash");
            Put(old, "parameters", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordPageView(
                "Home", "HomeView", "Home Title", "Splash", SampleData, "auto", true));
        }

        [Test]
        public void Equivalence_RecordPageView_OptionalsOmitted()
        {
            var old = Args(("pageName", "Home"), ("source", "manual"), ("flushImmediately", false));
            Put(old, "pageClass", null);
            Put(old, "pageTitle", null);
            Put(old, "previousPageName", null);
            Put(old, "parameters", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordPageView(
                "Home", null, null, null, null, "manual", false));
        }

        // -----------------------------------------------------------------
        // recordPurchase.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordPurchase_AllPresent()
        {
            var old = Args(
                ("revenue", 9.99),
                ("currency", "EUR"),
                ("revenueInMicros", false),
                ("quantity", 2),
                ("flushImmediately", true));
            Put(old, "purchaseType", "subscription");
            Put(old, "productId", "sword");
            Put(old, "transactionId", "txn-42");
            Put(old, "originalTransactionId", "otxn-1");
            Put(old, "validationProvider", "app_store");
            Put(old, "validationEnvironment", "production");
            Put(old, "purchaseToken", "ptok");
            Put(old, "receiptData", "rdata");
            Put(old, "signedPayload", "spay");
            Put(old, "receiptSignature", "rsig");
            Put(old, "isRenewal", true);
            Put(old, "store", "app_store");
            Put(old, "packageName", "com.game");
            Put(old, "voided", false);
            Put(old, "test", true);
            Put(old, "validationId", "vid-1");
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordPurchase(
                9.99, "EUR", false, "subscription", "sword", "txn-42", "otxn-1", "app_store",
                "production", "ptok", "rdata", "spay", "rsig", true, 2, "app_store", "com.game",
                false, true, "vid-1", SampleData, true));
        }

        [Test]
        public void Equivalence_RecordPurchase_Defaults()
        {
            var old = Args(
                ("revenue", 1.0),
                ("currency", "USD"),
                ("revenueInMicros", false),
                ("quantity", 1),
                ("flushImmediately", true));
            Put(old, "purchaseType", null);
            Put(old, "productId", null);
            Put(old, "transactionId", null);
            Put(old, "originalTransactionId", null);
            Put(old, "validationProvider", null);
            Put(old, "validationEnvironment", null);
            Put(old, "purchaseToken", null);
            Put(old, "receiptData", null);
            Put(old, "signedPayload", null);
            Put(old, "receiptSignature", null);
            Put(old, "isRenewal", null);
            Put(old, "store", null);
            Put(old, "packageName", null);
            Put(old, "voided", null);
            Put(old, "test", null);
            Put(old, "validationId", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordPurchase(
                1.0, "USD", false, null, null, null, null, null, null, null, null, null, null,
                null, 1, null, null, null, null, null, null, true));
        }

        // -----------------------------------------------------------------
        // recordRefund.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordRefund_AllPresent()
        {
            var old = Args(
                ("revenue", 4.5),
                ("currency", "GBP"),
                ("revenueInMicros", true),
                ("quantity", 3),
                ("flushImmediately", true));
            Put(old, "purchaseType", "one_time");
            Put(old, "productId", "gems");
            Put(old, "transactionId", "txn-9");
            Put(old, "originalTransactionId", "otxn-9");
            Put(old, "store", "play_store");
            Put(old, "packageName", "com.game");
            Put(old, "voided", true);
            Put(old, "test", false);
            Put(old, "reason", "chargeback");
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordRefund(
                4.5, "GBP", true, "one_time", "gems", "txn-9", "otxn-9", 3, "play_store",
                "com.game", true, false, "chargeback", SampleData, true));
        }

        [Test]
        public void Equivalence_RecordRefund_Defaults()
        {
            var old = Args(
                ("revenue", 2.0),
                ("currency", "USD"),
                ("revenueInMicros", false),
                ("quantity", 1),
                ("flushImmediately", true));
            Put(old, "purchaseType", null);
            Put(old, "productId", null);
            Put(old, "transactionId", null);
            Put(old, "originalTransactionId", null);
            Put(old, "store", null);
            Put(old, "packageName", null);
            Put(old, "voided", null);
            Put(old, "test", null);
            Put(old, "reason", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordRefund(
                2.0, "USD", false, null, null, null, null, 1, null, null, null, null, null, null, true));
        }

        // -----------------------------------------------------------------
        // recordAdRevenue.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordAdRevenue_AllPresent()
        {
            var old = Args(
                ("revenue", 0.02),
                ("currency", "USD"),
                ("revenueInMicros", false),
                ("flushImmediately", true));
            Put(old, "adNetwork", "admob");
            Put(old, "adFormat", "banner");
            Put(old, "adType", "display");
            Put(old, "adPlacement", "main_menu");
            Put(old, "test", true);
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordAdRevenue(
                0.02, "USD", false, "admob", "banner", "display", "main_menu", true, SampleData, true));
        }

        [Test]
        public void Equivalence_RecordAdRevenue_Defaults()
        {
            var old = Args(
                ("revenue", 0.01),
                ("currency", "USD"),
                ("revenueInMicros", false),
                ("flushImmediately", true));
            Put(old, "adNetwork", null);
            Put(old, "adFormat", null);
            Put(old, "adType", null);
            Put(old, "adPlacement", null);
            Put(old, "test", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordAdRevenue(
                0.01, "USD", false, null, null, null, null, null, null, true));
        }

        // -----------------------------------------------------------------
        // recordAdEvent — the reserved-name key diverges per transport.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordAdEvent_TypeKey_CAbi()
        {
            // Desktop / iOS / WebGL cross the reserved name under `type`.
            var old = Args(("type", "ad_show_failed"), ("flushImmediately", true));
            Put(old, "adNetwork", "admob");
            Put(old, "mediationNetwork", "max");
            Put(old, "adUnitId", "unit-1");
            Put(old, "adPlacement", "reward");
            Put(old, "adFormat", "rewarded");
            Put(old, "adType", "video");
            Put(old, "failureReason", "no_fill");
            Put(old, "loadLatencyMs", 123.4);
            Put(old, "rewardType", "coins");
            Put(old, "rewardAmount", 50.0);
            Put(old, "test", true);
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordAdEvent(
                "ad_show_failed", null, "admob", "max", "unit-1", "reward", "rewarded",
                "video", "no_fill", 123.4, "coins", 50.0, true, SampleData, true));
        }

        [Test]
        public void Equivalence_RecordAdEvent_EventNameKey_Jni()
        {
            // Android crosses the reserved name under `eventName`.
            var old = Args(("eventName", "ad_click"), ("flushImmediately", true));
            Put(old, "adNetwork", null);
            Put(old, "mediationNetwork", null);
            Put(old, "adUnitId", null);
            Put(old, "adPlacement", null);
            Put(old, "adFormat", null);
            Put(old, "adType", null);
            Put(old, "failureReason", null);
            Put(old, "loadLatencyMs", null);
            Put(old, "rewardType", null);
            Put(old, "rewardAmount", null);
            Put(old, "test", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordAdEvent(
                null, "ad_click", null, null, null, null, null, null, null, null, null, null,
                null, null, true));
        }

        // -----------------------------------------------------------------
        // recordNotification.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordNotification_AllPresent()
        {
            var old = Args(("type", "opened"), ("notificationId", "n-1"), ("flushImmediately", true));
            Put(old, "linkId", "l-1");
            Put(old, "campaignId", "c-1");
            Put(old, "title", "Hi");
            Put(old, "source", "push");
            Put(old, "payload", SampleData);
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordNotification(
                "opened", "n-1", "l-1", "c-1", "Hi", "push", SampleData, SampleData, true));
        }

        [Test]
        public void Equivalence_RecordNotification_OptionalsOmitted()
        {
            var old = Args(("type", "received"), ("notificationId", "n-2"), ("flushImmediately", false));
            Put(old, "linkId", null);
            Put(old, "campaignId", null);
            Put(old, "title", null);
            Put(old, "source", null);
            Put(old, "payload", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordNotification(
                "received", "n-2", null, null, null, null, null, null, false));
        }

        // -----------------------------------------------------------------
        // recordError.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_RecordError_AllPresent()
        {
            var old = Args(
                ("message", "boom"),
                ("exceptionType", "NullReferenceException"),
                ("fatal", true),
                ("source", "auto"));
            Put(old, "stackTrace", "at Foo()");
            Put(old, "reason", "bad state");
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordError(
                "boom", "NullReferenceException", "at Foo()", true, "auto", "bad state", SampleData));
        }

        [Test]
        public void Equivalence_RecordError_OptionalsOmitted()
        {
            var old = Args(
                ("message", "boom"),
                ("exceptionType", "Exception"),
                ("fatal", false),
                ("source", "manual"));
            Put(old, "stackTrace", null);
            Put(old, "reason", null);
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordError(
                "boom", "Exception", null, false, "manual", null, null));
        }

        // -----------------------------------------------------------------
        // setUser — userId is emitted even when null (clears the user).
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_SetUser_WithBoth()
        {
            var old = new Dictionary<string, object?> { ["userId"] = "u-1" };
            Put(old, "userName", "Ada");
            AssertSameWire(old, AttriaxEngineArgs.SetUser("u-1", "Ada"));
        }

        [Test]
        public void Equivalence_SetUser_NullUserId_StaysOnWire()
        {
            var old = new Dictionary<string, object?> { ["userId"] = null };
            Put(old, "userName", null);
            AssertSameWire(old, AttriaxEngineArgs.SetUser(null, null));
        }

        [Test]
        public void Golden_SetUser_NullUserId_EmitsNull()
        {
            Assert.AreEqual(
                "{\"userId\":null}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.SetUser(null, null)));
        }

        // -----------------------------------------------------------------
        // setUserProperty — value is emitted even when null (clears the property).
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_SetUserProperty_WithValue()
        {
            var old = new Dictionary<string, object?> { ["name"] = "tier", ["value"] = "gold" };
            AssertSameWire(old, AttriaxEngineArgs.SetUserProperty("tier", "gold"));
        }

        [Test]
        public void Equivalence_SetUserProperty_NullValue_StaysOnWire()
        {
            var old = new Dictionary<string, object?> { ["name"] = "tier", ["value"] = null };
            AssertSameWire(old, AttriaxEngineArgs.SetUserProperty("tier", null));
        }

        [Test]
        public void Golden_SetUserProperty_NullValue_EmitsNull()
        {
            Assert.AreEqual(
                "{\"name\":\"tier\",\"value\":null}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.SetUserProperty("tier", null)));
        }

        // -----------------------------------------------------------------
        // setUserProperties / clearUserProperties.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_SetUserProperties()
        {
            var props = new Dictionary<string, object> { ["a"] = 1L, ["b"] = "two" };
            var old = new Dictionary<string, object?> { ["properties"] = props };
            AssertSameWire(old, AttriaxEngineArgs.SetUserProperties(props));
        }

        [Test]
        public void Equivalence_ClearUserProperties_WithNames()
        {
            var names = new List<string> { "a", "b" };
            var old = new Dictionary<string, object?>();
            Put(old, "propertyNames", names);
            AssertSameWire(old, AttriaxEngineArgs.ClearUserProperties(names));
        }

        [Test]
        public void Equivalence_ClearUserProperties_NullOmitted()
        {
            var old = new Dictionary<string, object?>();
            Put(old, "propertyNames", null);
            AssertSameWire(old, AttriaxEngineArgs.ClearUserProperties(null));
        }

        [Test]
        public void Golden_ClearUserProperties_NullOmitsKey()
        {
            Assert.AreEqual(
                "{}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.ClearUserProperties(null)));
        }

        // -----------------------------------------------------------------
        // handleIncomingLink / recordDeepLink / validateReceipt.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_HandleIncomingLink()
        {
            var old = new Dictionary<string, object?> { ["uri"] = "app://x", ["isInitialLink"] = true };
            AssertSameWire(old, AttriaxEngineArgs.HandleIncomingLink("app://x", true));
        }

        [Test]
        public void Equivalence_RecordDeepLink_WithMetadata()
        {
            var old = Args(("uri", "app://x"), ("source", "manual"));
            Put(old, "metadata", SampleData);
            AssertSameWire(old, AttriaxEngineArgs.RecordDeepLink("app://x", SampleData, "manual"));
        }

        [Test]
        public void Equivalence_RecordDeepLink_MetadataOmitted()
        {
            var old = Args(("uri", "app://x"), ("source", "auto"));
            Put(old, "metadata", null);
            AssertSameWire(old, AttriaxEngineArgs.RecordDeepLink("app://x", null, "auto"));
        }

        [Test]
        public void Equivalence_ValidateReceipt_AllPresent()
        {
            var old = Args(("receipt", "rcpt"), ("test", true));
            Put(old, "provider", "app_store");
            Put(old, "environment", "sandbox");
            Put(old, "productId", "gems");
            Put(old, "transactionId", "txn-1");
            AssertSameWire(old, AttriaxEngineArgs.ValidateReceipt(
                "rcpt", true, "app_store", "sandbox", "gems", "txn-1"));
        }

        [Test]
        public void Equivalence_ValidateReceipt_OptionalsOmitted()
        {
            var old = Args(("receipt", "rcpt"), ("test", false));
            Put(old, "provider", null);
            Put(old, "environment", null);
            Put(old, "productId", null);
            Put(old, "transactionId", null);
            AssertSameWire(old, AttriaxEngineArgs.ValidateReceipt("rcpt", false, null, null, null, null));
        }

        // -----------------------------------------------------------------
        // setGdprConsent / updateSkanConversionValue.
        // -----------------------------------------------------------------

        [Test]
        public void Equivalence_SetGdprConsent()
        {
            var old = new Dictionary<string, object?>
            {
                ["analytics"] = true,
                ["attribution"] = false,
                ["adEvents"] = true,
            };
            AssertSameWire(old, AttriaxEngineArgs.SetGdprConsent(true, false, true));
        }

        [Test]
        public void Equivalence_UpdateSkan_WithCoarse()
        {
            var old = Args(("fineValue", 30), ("lockWindow", true));
            old["coarseValue"] = "high";
            AssertSameWire(old, AttriaxEngineArgs.UpdateSkanConversionValue(30, "high", true));
        }

        [Test]
        public void Equivalence_UpdateSkan_NoCoarseOmitted()
        {
            var old = Args(("fineValue", 10), ("lockWindow", false));
            AssertSameWire(old, AttriaxEngineArgs.UpdateSkanConversionValue(10, null, false));
        }

        // -----------------------------------------------------------------
        // Golden fixtures — pin the exact serialized shape (drift catch).
        // -----------------------------------------------------------------

        [Test]
        public void Golden_RecordEvent()
        {
            Assert.AreEqual(
                "{\"name\":\"level_up\",\"flushImmediately\":false}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.RecordEvent("level_up", null, false)));
        }

        [Test]
        public void Golden_RecordPurchase_Minimal()
        {
            Assert.AreEqual(
                "{\"revenue\":9.99,\"currency\":\"EUR\",\"revenueInMicros\":false,\"quantity\":1,\"flushImmediately\":true,\"productId\":\"sword\",\"transactionId\":\"txn-42\"}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.RecordPurchase(
                    9.99, "EUR", false, null, "sword", "txn-42", null, null, null, null, null, null,
                    null, null, 1, null, null, null, null, null, null, true)));
        }

        [Test]
        public void Golden_RecordAdEvent_TypeKeyOnly()
        {
            Assert.AreEqual(
                "{\"flushImmediately\":true,\"type\":\"ad_click\"}",
                JsonConvert.SerializeObject(AttriaxEngineArgs.RecordAdEvent(
                    "ad_click", null, null, null, null, null, null, null, null, null, null, null,
                    null, null, true)));
        }
    }
}
