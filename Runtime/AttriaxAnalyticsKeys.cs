#nullable enable

namespace Attriax.Unity
{
    public static class AttriaxAnalyticsEventKeys
    {
        public const string SignUp = "sign_up";
        public const string Login = "login";
        public const string TutorialBegin = "tutorial_begin";
        public const string TutorialComplete = "tutorial_complete";
        public const string LevelStart = "level_start";
        public const string LevelComplete = "level_complete";
        public const string LevelUp = "level_up";
        public const string AddPaymentInfo = "add_payment_info";
        public const string AddToCart = "add_to_cart";
        public const string CheckoutStarted = "checkout_started";
        public const string Purchase = "purchase";
        public const string Refund = "refund";
        public const string SubscriptionStarted = "subscription_started";
        public const string SubscriptionRenewed = "subscription_renewed";
        public const string TrialStarted = "trial_started";
        public const string AdRequest = "ad_request";
        public const string AdLoad = "ad_load";
        public const string AdLoadFailed = "ad_load_failed";
        public const string AdShow = "ad_show";
        public const string AdShowFailed = "ad_show_failed";
        public const string AdImpression = "ad_impression";
        public const string AdClick = "ad_click";
        public const string AdDismiss = "ad_dismiss";
        public const string AdReward = "ad_reward";
        public const string AdRevenue = "ad_revenue";
        public const string PageView = "page_view";
    }

    public static class AttriaxAnalyticsParamKeys
    {
        public const string Revenue = "revenue";
        public const string Currency = "currency";
        public const string RevenueInMicros = "revenueInMicros";
        public const string RevenueType = "revenueType";
        public const string PurchaseType = "purchaseType";
        public const string Method = "method";
        public const string PaymentType = "paymentType";
        public const string ProductId = "productId";
        public const string TransactionId = "transactionId";
        public const string OriginalTransactionId = "originalTransactionId";
        public const string ValidationProvider = "validationProvider";
        public const string ValidationEnvironment = "validationEnvironment";
        public const string PurchaseToken = "purchaseToken";
        public const string ReceiptData = "receiptData";
        public const string SignedPayload = "signedPayload";
        public const string ReceiptSignature = "receiptSignature";
        public const string IsRenewal = "isRenewal";
        public const string Quantity = "quantity";
        public const string Store = "store";
        public const string PackageName = "packageName";
        public const string Voided = "voided";
        public const string Test = "test";
        public const string ValidationId = "validationId";
        public const string Reason = "reason";
        public const string AdNetwork = "adNetwork";
        public const string MediationNetwork = "mediationNetwork";
        public const string AdUnitId = "adUnitId";
        public const string AdPlacement = "adPlacement";
        public const string AdFormat = "adFormat";
        public const string AdType = "adType";
        public const string FailureReason = "failureReason";
        public const string LoadLatencyMs = "loadLatencyMs";
        public const string RewardType = "rewardType";
        public const string RewardAmount = "rewardAmount";
        public const string PageName = "pageName";
        public const string PageClass = "pageClass";
        public const string PageTitle = "pageTitle";
        public const string PreviousPageName = "previousPageName";
        public const string Source = "source";
        public const string Day = "day";
        public const string ActualDay = "actualDay";
        public const string RetentionType = "retentionType";
    }
}