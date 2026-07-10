package com.attriax.unity

import android.content.Context
import com.attriax.sdk.Attriax
import com.attriax.sdk.AttriaxAdEventType
import com.attriax.sdk.AttriaxAttStatus
import com.attriax.sdk.AttriaxBrowserAction
import com.attriax.sdk.AttriaxConfig
import com.attriax.sdk.AttriaxCreateDynamicLinkResult
import com.attriax.sdk.AttriaxDeepLinkEvent
import com.attriax.sdk.AttriaxDeepLinkListener
import com.attriax.sdk.AttriaxDeepLinkReferrerDetails
import com.attriax.sdk.AttriaxInstallReferrerDetails
import com.attriax.sdk.AttriaxNotificationEventSource
import com.attriax.sdk.AttriaxNotificationEventType
import com.attriax.sdk.AttriaxRawDeepLinkEvent
import com.attriax.sdk.AttriaxRawDeepLinkListener
import com.attriax.sdk.AttriaxRevenueReceiptValidationResult
import com.attriax.sdk.AttriaxSdk
import com.attriax.sdk.AttriaxSdkSnapshot
import com.attriax.sdk.AttriaxSkanCoarseValue
import com.attriax.sdk.AttriaxSkanState
import com.attriax.sdk.AttriaxSkanUpdateResult
import com.attriax.sdk.AttriaxSynchronizationStateListener
import com.attriax.sdk.internal.deeplink.AttriaxUri
import org.json.JSONArray
import org.json.JSONObject

/**
 * Listener the Unity C# `AndroidJavaProxy` implements to receive engine events.
 *
 * Every callback carries a single primitive string so it round-trips cleanly through
 * Unity's proxy marshalling; the C# side parses the JSON payloads and re-raises them
 * on the Unity main thread (see `AttriaxAndroidEnginePlatform`).
 */
interface AttriaxUnityBridgeListener {
    /** KMP `AttriaxSynchronizationState.name` for the new state. */
    fun onSyncState(name: String)

    /** A serialized resolved [AttriaxDeepLinkEvent]. */
    fun onDeepLink(json: String)

    /** A serialized raw [AttriaxRawDeepLinkEvent]. */
    fun onRawDeepLink(json: String)
}

/**
 * Thin, JNI-friendly Unity <-> KMP-engine bridge (Phase 2 native-engine re-wrap).
 *
 * The engine lives in the KMP core (shipped as the `com.attriax:core` Android AAR).
 * This bridge is the Unity analog of the Flutter Android plugin
 * (`AttriaxAndroidPlugin.kt`): it builds one [Attriax] engine via [AttriaxSdk.create]
 * and exposes a flat [dispatch] command surface (method name + JSON args) plus a
 * [AttriaxUnityBridgeListener] callback for engine events. The command/serialization
 * mapping is reused wholesale from the Flutter plugin — the only difference is the
 * host (Unity `AndroidJavaObject` / `AndroidJavaProxy` instead of a Flutter
 * `MethodChannel` / `EventChannel`).
 *
 * Threading: [create] and [dispatch] are invoked from the C# side on a dedicated
 * JNI-attached worker thread (never Unity's main thread), matching the Flutter
 * plugin's single-thread `worker` executor; the KMP androidMain adapters own their
 * own I/O threads. Engine events fan in on the engine's threads and are forwarded
 * verbatim to [listener]; the C# proxy re-marshals them onto the Unity main thread.
 *
 * [dispatch] returns `null` for void commands and a `{"result": <value>}` JSON
 * envelope for value-returning commands/getters; it throws on error so the C# side
 * can surface a faulted `Task`.
 */
class AttriaxUnityBridge private constructor(
    private val engine: Attriax,
    private val listener: AttriaxUnityBridgeListener,
) {

    private val syncListener =
        AttriaxSynchronizationStateListener { state -> listener.onSyncState(state.name) }
    private val deepLinkListener =
        AttriaxDeepLinkListener { event -> listener.onDeepLink(serializeDeepLinkEvent(event).toString()) }
    private val rawDeepLinkListener =
        AttriaxRawDeepLinkListener { event -> listener.onRawDeepLink(serializeRawDeepLinkEvent(event).toString()) }

    init {
        engine.synchronization.addStateListener(syncListener)
        engine.deepLinks.addListener(deepLinkListener)
        engine.deepLinks.addRawListener(rawDeepLinkListener)
    }

    // -------------------------------------------------------------------------
    // Command dispatch.
    // -------------------------------------------------------------------------

    /**
     * Dispatch a single command by wire name. [argsJson] is the JSON object of
     * wire-keyed arguments (the same keys the Flutter `MethodChannel` and the KMP
     * C-ABI `route(...)` use). Unknown methods throw.
     */
    fun dispatch(method: String, argsJson: String?): String? {
        val a = if (argsJson.isNullOrBlank()) JSONObject() else JSONObject(argsJson)
        return when (method) {
            // ---- lifecycle ----
            "flush" -> { engine.flush(); null }
            "reset" -> { engine.reset(); null }
            "submitAsaToken" -> { engine.submitAsaToken(str(a, "token") ?: ""); null }

            // ---- primitive getters / setters ----
            "getDeviceId" -> result(engine.deviceId)
            "getIsInitialized" -> result(engine.isInitialized)
            "getIsFirstLaunch" -> result(engine.isFirstLaunch)
            "getSdkEnabled" -> result(engine.enabled)
            "setSdkEnabled" -> { engine.enabled = bool(a, "enabled", true); null }
            "getAnonymousTracking" -> result(engine.anonymousTrackingEnabled)
            "setAnonymousTracking" -> { engine.anonymousTrackingEnabled = bool(a, "enabled", true); null }
            "getEventTrackingEnabled" -> result(engine.tracking.enabled)
            "setEventTrackingEnabled" -> { engine.tracking.enabled = bool(a, "enabled", true); null }
            "getSynchronizationState" -> result(engine.synchronization.state.name)
            "getIsSynchronized" -> result(engine.synchronization.isSynchronized)
            "getIsWaitingForGdprConsent" -> result(engine.consent.gdpr.isWaitingForConsent)
            "needsGdprConsent" -> result(engine.consent.gdpr.needsConsent(bool(a, "localOnly", false)))
            "getTrackingAuthorizationStatus" -> result(engine.consent.att.status.wireValue)
            "getDoNotSell" -> result(engine.consent.ccpa.doNotSell)
            "getUsPrivacy" -> result(engine.consent.ccpa.usPrivacy)
            "getSdkSnapshot" -> resultObject(serializeSnapshot(engine.sdkSnapshot))
            "getSkanState" -> resultObject(serializeSkanState(engine.skan.state))

            // ---- referrer getters (KMP referrer getters are synchronous; the wire
            //      timeoutMs is accepted for Flutter parity and ignored here) ----
            "getOriginalInstallReferrer" ->
                resultObject(serializeInstallReferrer(engine.referrer.getOriginalInstallReferrer()))
            "getReinstallReferrer" ->
                resultObject(serializeInstallReferrer(engine.referrer.getReinstallReferrer()))
            "getRawInstallReferrer" -> result(engine.referrer.getRawInstallReferrer())
            "getSessionReferrer" ->
                resultObject(serializeDeepLinkReferrer(engine.referrer.getSessionReferrer()))
            "getLatestDeepLinkReferrer" ->
                resultObject(serializeDeepLinkReferrer(engine.referrer.getLatestDeepLinkReferrer()))

            // ---- deep-link snapshot getters ----
            "getLatestDeepLink" -> resultObject(serializeDeepLinkEvent(engine.deepLinks.latestDeepLink))
            "getInitialDeepLink" -> resultObject(serializeDeepLinkEvent(engine.deepLinks.initialDeepLink))
            "getRawInitialDeepLink" ->
                resultObject(serializeRawDeepLinkEvent(engine.deepLinks.rawInitialDeepLink))
            "getIsInitialDeepLinkResolved" -> result(engine.deepLinks.initialDeepLinkResolved)

            // ---- tracking ----
            "recordEvent" -> {
                engine.recordEvent(
                    name = str(a, "name") ?: "",
                    eventData = map(a, "eventData"),
                    flushImmediately = bool(a, "flushImmediately", false),
                )
                null
            }
            "recordPageView" -> {
                engine.tracking.recordPageView(
                    pageName = str(a, "pageName") ?: "",
                    pageClass = str(a, "pageClass"),
                    pageTitle = str(a, "pageTitle"),
                    previousPageName = str(a, "previousPageName"),
                    parameters = map(a, "parameters"),
                    source = str(a, "source") ?: "manual",
                    flushImmediately = bool(a, "flushImmediately", false),
                )
                null
            }
            "recordPurchase" -> {
                engine.tracking.recordPurchase(
                    revenue = double(a, "revenue", 0.0),
                    currency = str(a, "currency") ?: "USD",
                    revenueInMicros = bool(a, "revenueInMicros", false),
                    purchaseType = str(a, "purchaseType"),
                    productId = str(a, "productId"),
                    transactionId = str(a, "transactionId"),
                    originalTransactionId = str(a, "originalTransactionId"),
                    validationProvider = str(a, "validationProvider"),
                    validationEnvironment = str(a, "validationEnvironment"),
                    purchaseToken = str(a, "purchaseToken"),
                    receiptData = str(a, "receiptData"),
                    signedPayload = str(a, "signedPayload"),
                    receiptSignature = str(a, "receiptSignature"),
                    isRenewal = kbool(a, "isRenewal"),
                    quantity = int(a, "quantity", 1),
                    store = str(a, "store"),
                    packageName = str(a, "packageName"),
                    voided = kbool(a, "voided"),
                    test = kbool(a, "test"),
                    validationId = str(a, "validationId"),
                    metadata = map(a, "metadata"),
                    flushImmediately = bool(a, "flushImmediately", true),
                )
                null
            }
            "recordRefund" -> {
                engine.tracking.recordRefund(
                    revenue = double(a, "revenue", 0.0),
                    currency = str(a, "currency") ?: "USD",
                    revenueInMicros = bool(a, "revenueInMicros", false),
                    purchaseType = str(a, "purchaseType"),
                    productId = str(a, "productId"),
                    transactionId = str(a, "transactionId"),
                    originalTransactionId = str(a, "originalTransactionId"),
                    quantity = int(a, "quantity", 1),
                    store = str(a, "store"),
                    packageName = str(a, "packageName"),
                    voided = kbool(a, "voided"),
                    test = kbool(a, "test"),
                    reason = str(a, "reason"),
                    metadata = map(a, "metadata"),
                    flushImmediately = bool(a, "flushImmediately", true),
                )
                null
            }
            "recordAdRevenue" -> {
                engine.tracking.recordAdRevenue(
                    revenue = double(a, "revenue", 0.0),
                    currency = str(a, "currency") ?: "USD",
                    revenueInMicros = bool(a, "revenueInMicros", false),
                    adNetwork = str(a, "adNetwork"),
                    adFormat = str(a, "adFormat"),
                    adType = str(a, "adType"),
                    adPlacement = str(a, "adPlacement"),
                    test = kbool(a, "test"),
                    metadata = map(a, "metadata"),
                    flushImmediately = bool(a, "flushImmediately", true),
                )
                null
            }
            "recordAdEvent" -> {
                // The platform interface sends the resolved reserved event name
                // (`eventName`, e.g. "ad_show_failed"); resolve it back to the enum
                // whose `eventName` matches so the engine's field->eventData lowering
                // runs. Unknown names fall back to REQUEST.
                engine.tracking.recordAdEvent(
                    type = adEventType(str(a, "eventName")),
                    adNetwork = str(a, "adNetwork"),
                    mediationNetwork = str(a, "mediationNetwork"),
                    adUnitId = str(a, "adUnitId"),
                    adPlacement = str(a, "adPlacement"),
                    adFormat = str(a, "adFormat"),
                    adType = str(a, "adType"),
                    failureReason = str(a, "failureReason"),
                    loadLatencyMs = kdouble(a, "loadLatencyMs"),
                    rewardType = str(a, "rewardType"),
                    rewardAmount = kdouble(a, "rewardAmount"),
                    test = kbool(a, "test"),
                    metadata = map(a, "metadata"),
                    flushImmediately = bool(a, "flushImmediately", true),
                )
                null
            }
            "recordNotification" -> {
                engine.tracking.recordNotification(
                    type = notificationType(str(a, "type")),
                    notificationId = str(a, "notificationId") ?: "",
                    linkId = str(a, "linkId"),
                    campaignId = str(a, "campaignId"),
                    title = str(a, "title"),
                    source = notificationSource(str(a, "source")),
                    payload = map(a, "payload"),
                    metadata = map(a, "metadata"),
                    flushImmediately = bool(a, "flushImmediately", false),
                )
                null
            }
            "recordError" -> {
                engine.tracking.recordError(
                    error = Throwable(str(a, "message") ?: str(a, "error") ?: "error"),
                    stackTrace = str(a, "stackTrace"),
                    fatal = bool(a, "fatal", false),
                    source = str(a, "source") ?: "manual",
                    reason = str(a, "reason"),
                    metadata = map(a, "metadata"),
                )
                null
            }
            "setUser" -> {
                engine.tracking.setUser(userId = str(a, "userId"), userName = str(a, "userName"))
                null
            }
            "setUserProperty" -> {
                engine.tracking.setUserProperty(str(a, "name") ?: "", jsonValue(a.opt("value")))
                null
            }
            "setUserProperties" -> {
                engine.tracking.setUserProperties(map(a, "properties") ?: emptyMap())
                null
            }
            "clearUserProperties" -> {
                engine.tracking.clearUserProperties(strList(a, "propertyNames"))
                null
            }
            "registerPushToken" -> {
                val token = str(a, "token")
                val metadata = map(a, "metadata")
                if (str(a, "provider") == "apns") {
                    engine.tracking.registerApplePushToken(token, metadata)
                } else {
                    engine.tracking.registerFirebaseMessagingToken(token, metadata)
                }
                null
            }

            // ---- consent (GDPR / CCPA / ATT) ----
            "setGdprConsent" -> {
                engine.consent.gdpr.setConsent(
                    analytics = bool(a, "analytics", false),
                    attribution = bool(a, "attribution", false),
                    adEvents = bool(a, "adEvents", false),
                )
                null
            }
            "setGdprConsentNotRequired" -> { engine.consent.gdpr.setNotRequired(); null }
            "resetGdprConsent" -> { engine.consent.gdpr.reset(); null }
            "requestGdprDataErasure" -> { engine.consent.gdpr.requestDataErasure(); null }
            "setCcpaConsent" -> {
                engine.consent.ccpa.set(doNotSell = kbool(a, "doNotSell"), usPrivacy = str(a, "usPrivacy"))
                null
            }
            "setTrackingAuthorizationStatus" -> {
                engine.consent.att.setStatus(attStatus(str(a, "status")))
                null
            }
            "requestTrackingAuthorization" ->
                result(engine.consent.att.requestAuthorization(klong(a, "timeoutMs")).wireValue)

            // ---- SKAN ----
            "updateSkanConversionValue" -> resultObject(
                serializeSkanResult(
                    engine.skan.updateConversionValue(
                        fineValue = int(a, "fineValue", 0),
                        coarseValue = coarse(str(a, "coarseValue")),
                        lockWindow = bool(a, "lockWindow", false),
                    ),
                ),
            )

            // ---- deep links ----
            "handleIncomingLink" -> {
                engine.deepLinks.handleUri(
                    rawUri = str(a, "uri") ?: "",
                    isInitialLink = bool(a, "isInitialLink", false),
                )
                null
            }
            "recordDeepLink" -> resultObject(
                serializeDeepLinkEvent(
                    engine.deepLinks.recordDeepLink(
                        uri = str(a, "uri") ?: "",
                        metadata = map(a, "metadata"),
                        source = str(a, "source") ?: "manual",
                    ),
                ),
            )
            "waitForInitialDeepLink" ->
                resultObject(serializeDeepLinkEvent(engine.deepLinks.waitForInitialDeepLink()))
            "waitForDeepLinkResolution" -> {
                val raw = deserializeRawDeepLinkEvent(a.optJSONObject("rawEvent"))
                resultObject(
                    if (raw == null) null else serializeDeepLinkEvent(engine.deepLinks.waitResolution(raw)),
                )
            }
            "createDynamicLink" -> resultObject(serializeCreateDynamicLink(createDynamicLink(a)))

            // ---- receipt validation ----
            "validateReceipt" -> resultObject(
                serializeReceipt(
                    engine.validateReceipt(
                        receipt = str(a, "receipt") ?: "",
                        test = bool(a, "test", false),
                        provider = str(a, "provider"),
                        environment = str(a, "environment"),
                        productId = str(a, "productId"),
                        transactionId = str(a, "transactionId"),
                    ),
                ),
            )

            else -> throw IllegalArgumentException("Unknown Attriax command: $method")
        }
    }

    /** Detach engine listeners and dispose the runtime (KMP `Attriax.dispose`). */
    fun destroy() {
        engine.synchronization.removeStateListener(syncListener)
        engine.deepLinks.removeListener(deepLinkListener)
        engine.deepLinks.removeRawListener(rawDeepLinkListener)
        engine.dispose()
    }

    private fun createDynamicLink(a: JSONObject): AttriaxCreateDynamicLinkResult {
        // The Unity facade sends a flattened options object matching
        // AttriaxCreateDynamicLinkOptions; map its social-preview / utm / redirect
        // fields to the KMP structured argument types.
        val preview = if (a.has("previewTitle") || a.has("previewDescription")) {
            com.attriax.sdk.AttriaxDynamicLinkSocialPreview(
                title = str(a, "previewTitle"),
                description = str(a, "previewDescription"),
            )
        } else {
            null
        }
        val hasUtm = a.has("utmSource") || a.has("utmMedium") || a.has("utmCampaign") ||
            a.has("utmTerm") || a.has("utmContent")
        val utms = if (hasUtm) {
            com.attriax.sdk.AttriaxDynamicLinkUtms(
                source = str(a, "utmSource"),
                medium = str(a, "utmMedium"),
                campaign = str(a, "utmCampaign"),
                term = str(a, "utmTerm"),
                content = str(a, "utmContent"),
            )
        } else {
            null
        }
        val redirects = if (a.has("iosRedirect") || a.has("androidRedirect")) {
            com.attriax.sdk.AttriaxDynamicLinkRedirects(
                ios = kbool(a, "iosRedirect"),
                android = kbool(a, "androidRedirect"),
            )
        } else {
            null
        }
        return engine.deepLinks.createDynamicLink(
            name = str(a, "name"),
            destinationUrl = str(a, "destinationUrl"),
            group = str(a, "group"),
            prefix = str(a, "prefix"),
            socialPreview = preview,
            utms = utms,
            redirects = redirects,
            data = map(a, "data"),
        )
    }

    // -------------------------------------------------------------------------
    // Serialization (KMP result -> JSON for the C# marshaller). Best-effort, keyed
    // to the C# type surface; mirrors the Flutter plugin's serializers.
    // -------------------------------------------------------------------------

    private fun serializeSnapshot(s: AttriaxSdkSnapshot): JSONObject = JSONObject()
        .put("apiVersion", s.apiVersion)
        .put("packageVersion", s.packageVersion)
        .put("metadata", anyMapToJson(s.metadata))

    private fun serializeSkanState(s: AttriaxSkanState?): JSONObject? {
        if (s == null) return null
        return JSONObject()
            .put("enabled", s.enabled)
            .put("fineValue", s.fineValue)
            .put("coarseValue", s.coarseValue?.wireValue)
            .put("lockWindow", s.lockWindow)
    }

    private fun serializeSkanResult(r: AttriaxSkanUpdateResult): JSONObject = JSONObject()
        .put("status", r.status.wireValue)
        .put("message", r.message)
        .put("fineValue", r.fineValue)
        .put("coarseValue", r.coarseValue?.wireValue)
        .put("lockWindow", r.lockWindow)

    private fun serializeRawDeepLinkEvent(e: AttriaxRawDeepLinkEvent?): JSONObject? {
        if (e == null) return null
        return JSONObject()
            .put("uri", e.uri.raw)
            .put("receivedAtMs", e.receivedAtMs)
            .put("isInitial", e.isInitial)
    }

    private fun serializeDeepLinkEvent(e: AttriaxDeepLinkEvent?): JSONObject? {
        if (e == null) return null
        return JSONObject()
            .put("uri", e.uri.raw)
            .put("clickedAtMs", e.clickedAtMs)
            .put("consumedAtMs", e.consumedAtMs)
            .put("found", e.found)
            .put("trigger", e.trigger.name)
            .put("status", e.status.name)
            .put("isAttriaxSubDomain", e.isAttriaxSubDomain)
            .put("handledBySdk", e.handledBySdk)
            .put("data", stringMapToJson(e.data))
            .put("utm", stringMapToJson(e.utm))
            .put("browserAction", serializeBrowserAction(e.browserAction))
            .put("rawEvent", serializeRawDeepLinkEvent(e.rawEvent))
    }

    private fun serializeInstallReferrer(d: AttriaxInstallReferrerDetails?): JSONObject? {
        if (d == null) return null
        return JSONObject()
            .put("rawPlatformInstallReferrer", d.rawPlatformInstallReferrer)
            .put("source", d.source)
            .put("medium", d.medium)
            .put("campaign", d.campaign)
            .put("term", d.term)
            .put("content", d.content)
            .put("adNetwork", d.adNetwork)
            .put("adClickId", d.adClickId)
            .put("attributionType", d.attributionType.name)
            .put("deepLinkUri", d.deepLinkUrl)
            .put("deepLinkData", stringMapToJson(d.deepLinkData))
            .put("registeredAt", d.registeredAt)
            .put("installBeginTimestampSeconds", d.installBeginTimestampSeconds)
            .put("referrerClickTimestampSeconds", d.referrerClickTimestampSeconds)
            .put("googlePlayInstantParam", d.googlePlayInstantParam)
            .put("precision", d.precision)
    }

    private fun serializeDeepLinkReferrer(d: AttriaxDeepLinkReferrerDetails?): JSONObject? {
        if (d == null) return null
        return JSONObject()
            .put("uri", d.uri.raw)
            .put("receivedAtMs", d.receivedAtMs)
            .put("clickedAtMs", d.clickedAtMs)
            .put("consumedAtMs", d.consumedAtMs)
            .put("trigger", d.trigger.name)
            .put("isAttriaxDomain", d.isAttriaxDomain)
            .put("found", d.found)
            .put("data", stringMapToJson(d.data))
            .put("utm", stringMapToJson(d.utm))
            .put("browserAction", serializeBrowserAction(d.browserAction))
    }

    private fun serializeReceipt(r: AttriaxRevenueReceiptValidationResult): JSONObject = JSONObject()
        .put("validationId", r.validationId)
        .put("status", r.status.name)
        .put("requestVersion", r.requestVersion)
        .put("acceptedAtMs", r.acceptedAtMs)
        .put("provider", r.provider)
        .put("environment", r.environment)
        .put("transactionId", r.transactionId)
        .put("originalTransactionId", r.originalTransactionId)
        .put("productId", r.productId)
        .put("failureReason", r.failureReason)
        .put("expiresAtMs", r.expiresAtMs)
        .put("providerResult", anyMapToJson(r.providerResult))
        .put("publicReceipt", anyMapToJson(r.publicReceipt))

    private fun serializeCreateDynamicLink(r: AttriaxCreateDynamicLinkResult): JSONObject {
        val record = JSONObject()
            .put("id", r.record.id)
            .put("path", r.record.path)
            .put("shortUrl", r.record.shortUrl)
            .put("name", r.record.name)
            .put("destinationUrl", r.record.destinationUrl)
            .put("group", r.record.group)
            .put("prefix", r.record.prefix)
            .put("data", anyMapToJson(r.record.data))
        return JSONObject().put("shortUrl", r.shortUrl).put("record", record)
    }

    private fun serializeBrowserAction(action: AttriaxBrowserAction?): JSONObject? {
        if (action == null) return null
        return JSONObject().put("url", action.url).put("openMode", action.openMode.name)
    }

    private fun deserializeRawDeepLinkEvent(o: JSONObject?): AttriaxRawDeepLinkEvent? {
        if (o == null) return null
        val uri = AttriaxUri.parse(o.opt("uri") as? String) ?: return null
        return AttriaxRawDeepLinkEvent(
            uri = uri,
            receivedAtMs = if (o.has("receivedAtMs")) o.optLong("receivedAtMs") else 0L,
            isInitial = o.optBoolean("isInitial", false),
        )
    }

    // -------------------------------------------------------------------------
    // Companion / factory.
    // -------------------------------------------------------------------------

    companion object {
        /**
         * Build and initialize the engine, attach event listeners, and return the
         * bridge. Runs on the C# JNI worker thread; the Play-Services advertising-id
         * supplier binds off that thread (the KMP androidMain factory resolves + stamps
         * the real Android User-Agent on its OkHttp transport — no synthetic UA here).
         */
        @JvmStatic
        fun create(
            context: Context,
            configJson: String,
            listener: AttriaxUnityBridgeListener,
        ): AttriaxUnityBridge {
            val appContext = context.applicationContext ?: context
            val config = buildConfig(JSONObject(configJson))
            val engine = AttriaxSdk.create(
                context = appContext,
                config = config,
                advertisingIdSupplier = { AdvertisingIdProvider.fetch(appContext) },
            )
            engine.init()
            return AttriaxUnityBridge(engine, listener)
        }

        private fun buildConfig(m: JSONObject): AttriaxConfig = AttriaxConfig(
            projectToken = str(m, "projectToken") ?: "",
            apiBaseUrl = str(m, "apiBaseUrl") ?: AttriaxConfig.DEFAULT_API_BASE_URL,
            appVersion = str(m, "appVersion"),
            appBuildNumber = str(m, "appBuildNumber"),
            appPackageName = str(m, "appPackageName"),
            sdkMetadata = map(m, "sdkMetadata"),
            // deviceContext: null — the androidMain factory auto-captures the
            // non-sensitive device fields; the wrapper has nothing extra Android
            // cannot self-collect.
            deviceContext = null,
            enableDebugLogs = bool(m, "enableDebugLogs", false),
            requestTimeoutMs = long(m, "requestTimeoutMs", 12_000L),
            maxQueueSize = int(m, "maxQueueSize", 500),
            eventFlushIntervalMs = long(m, "eventFlushIntervalMs", 60_000L),
            flushEventsImmediatelyOnFirstLaunch = bool(m, "flushEventsImmediatelyOnFirstLaunch", true),
            collectAdvertisingId = bool(m, "collectAdvertisingId", true),
            automaticCrashReportingEnabled = bool(m, "automaticCrashReportingEnabled", true),
            gdprEnabled = bool(m, "gdprEnabled", false),
            anonymousTracking = bool(m, "anonymousTracking", true),
            sessionTrackingEnabled = bool(m, "sessionTrackingEnabled", true),
            sessionHeartbeatIntervalMs = long(m, "sessionHeartbeatIntervalMs", 300_000L),
            firstLaunchSessionHeartbeatIntervalMs = long(m, "firstLaunchSessionHeartbeatIntervalMs", 30_000L),
            installReferrerEnabled = bool(m, "installReferrerEnabled", true),
            attestationEnabled = bool(m, "attestationEnabled", false),
            pinnedCertificateSha256Fingerprints = strList(m, "pinnedCertificateSha256Fingerprints") ?: emptyList(),
            automaticBrowserHandling = bool(m, "automaticBrowserHandling", true),
            requestTrackingAuthorizationOnInit = bool(m, "requestTrackingAuthorizationOnInit", false),
            trackingAuthorizationStatusTimeoutMs = long(m, "trackingAuthorizationStatusTimeoutMs", 60_000L),
            asaTokenCaptureEnabled = bool(m, "asaTokenCaptureEnabled", true),
            doNotSell = kbool(m, "doNotSell"),
            usPrivacy = str(m, "usPrivacy"),
        )

        // ---- value-envelope helpers ----

        private fun result(value: Any?): String = JSONObject().put("result", value ?: JSONObject.NULL).toString()

        private fun resultObject(value: JSONObject?): String =
            JSONObject().put("result", value ?: JSONObject.NULL).toString()

        // ---- JSON readers (JSONObject-backed; mirror the Flutter plugin's Map helpers) ----

        private fun str(m: JSONObject, key: String): String? = m.opt(key) as? String

        private fun bool(m: JSONObject, key: String, default: Boolean): Boolean = m.optBoolean(key, default)

        private fun kbool(m: JSONObject, key: String): Boolean? =
            if (m.has(key) && !m.isNull(key)) m.optBoolean(key) else null

        private fun int(m: JSONObject, key: String, default: Int): Int = m.optInt(key, default)

        private fun long(m: JSONObject, key: String, default: Long): Long = m.optLong(key, default)

        private fun klong(m: JSONObject, key: String): Long? =
            if (m.has(key) && !m.isNull(key)) m.optLong(key) else null

        private fun double(m: JSONObject, key: String, default: Double): Double = m.optDouble(key, default)

        private fun kdouble(m: JSONObject, key: String): Double? =
            if (m.has(key) && !m.isNull(key)) m.optDouble(key) else null

        private fun map(m: JSONObject, key: String): Map<String, Any?>? {
            val obj = m.optJSONObject(key) ?: return null
            return jsonToMap(obj)
        }

        private fun strList(m: JSONObject, key: String): List<String>? {
            val arr = m.optJSONArray(key) ?: return null
            val out = ArrayList<String>(arr.length())
            for (i in 0 until arr.length()) {
                (arr.opt(i) as? String)?.let { out.add(it) }
            }
            return out
        }

        private fun jsonToMap(obj: JSONObject): Map<String, Any?> {
            val out = LinkedHashMap<String, Any?>()
            val keys = obj.keys()
            while (keys.hasNext()) {
                val key = keys.next()
                out[key] = jsonValue(obj.opt(key))
            }
            return out
        }

        private fun jsonValue(value: Any?): Any? = when (value) {
            null, JSONObject.NULL -> null
            is JSONObject -> jsonToMap(value)
            is JSONArray -> (0 until value.length()).map { jsonValue(value.opt(it)) }
            else -> value
        }

        private fun attStatus(wire: String?): AttriaxAttStatus = when (wire) {
            "authorized" -> AttriaxAttStatus.AUTHORIZED
            "denied" -> AttriaxAttStatus.DENIED
            "restricted" -> AttriaxAttStatus.RESTRICTED
            "not_determined", "notDetermined" -> AttriaxAttStatus.NOT_DETERMINED
            else -> AttriaxAttStatus.UNKNOWN
        }

        private fun coarse(wire: String?): AttriaxSkanCoarseValue? {
            val normalized = wire?.lowercase() ?: return null
            return AttriaxSkanCoarseValue.entries.firstOrNull { it.wireValue == normalized }
        }

        private fun adEventType(eventName: String?): AttriaxAdEventType =
            AttriaxAdEventType.entries.firstOrNull { it.eventName == eventName } ?: AttriaxAdEventType.REQUEST

        private fun notificationType(wire: String?): AttriaxNotificationEventType =
            AttriaxNotificationEventType.entries.firstOrNull { it.wireValue == wire }
                ?: AttriaxNotificationEventType.RECEIVED

        private fun notificationSource(wire: String?): AttriaxNotificationEventSource? {
            if (wire == null) return null
            return AttriaxNotificationEventSource.entries.firstOrNull { it.wireValue == wire }
        }

        // ---- JSON writers ----

        private fun stringMapToJson(map: Map<String, String>?): JSONObject? {
            if (map == null) return null
            val obj = JSONObject()
            for ((k, v) in map) obj.put(k, v)
            return obj
        }

        private fun anyMapToJson(map: Map<String, Any?>?): JSONObject? {
            if (map == null) return null
            val obj = JSONObject()
            for ((k, v) in map) obj.put(k, anyToJson(v))
            return obj
        }

        private fun anyToJson(value: Any?): Any? = when (value) {
            null -> JSONObject.NULL
            is Map<*, *> -> {
                val obj = JSONObject()
                for ((k, v) in value) obj.put(k.toString(), anyToJson(v))
                obj
            }
            is List<*> -> {
                val arr = JSONArray()
                for (item in value) arr.put(anyToJson(item))
                arr
            }
            else -> value
        }
    }
}
