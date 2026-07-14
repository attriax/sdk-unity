package com.attriax.unity

import android.content.Context
import com.attriax.sdk.Attriax
import com.attriax.sdk.AttriaxAttStatus
import com.attriax.sdk.AttriaxBrowserAction
import com.attriax.sdk.AttriaxConfig
import com.attriax.sdk.AttriaxCreateDynamicLinkResult
import com.attriax.sdk.AttriaxDeepLinkEvent
import com.attriax.sdk.AttriaxDeepLinkListener
import com.attriax.sdk.AttriaxDeepLinkReferrerDetails
import com.attriax.sdk.AttriaxDispatchResult
import com.attriax.sdk.AttriaxDispatcher
import com.attriax.sdk.AttriaxInstallReferrerDetails
import com.attriax.sdk.AttriaxRawDeepLinkEvent
import com.attriax.sdk.AttriaxRawDeepLinkListener
import com.attriax.sdk.AttriaxRevenueReceiptValidationResult
import com.attriax.sdk.AttriaxSdk
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
 * [AttriaxUnityBridgeListener] callback for engine events.
 *
 * Like the Flutter plugin, [dispatch] forwards the bulk of the command surface to the
 * KMP core's ONE canonical [AttriaxDispatcher.execute] table rather than owning a
 * parallel `when`. A handful of commands stay engine-direct: those with no dispatch key
 * (`tracking.enabled`, dynamic-link creation, the deep-link resolution waits) and those
 * whose result the Unity C# mapper (`AttriaxAndroidEngineMapper`) reads from the
 * wrapper's UPPERCASE-enum serializer shape (deep-link / referrer / receipt / sync
 * state) rather than the canonical lowercase `.wire()` shape `execute` emits — see the
 * per-branch notes in [dispatch]. Those deep-link serializers are also required by the
 * event listeners, so they stay regardless. The only host difference vs Flutter is the
 * transport (Unity `AndroidJavaObject` / `AndroidJavaProxy` instead of a Flutter
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
     * C-ABI `route(...)` use).
     *
     * Most commands forward 1:1 to the KMP core's canonical [AttriaxDispatcher.execute]
     * (see [forward]); the branches below are the deliberate exceptions. Unknown methods
     * throw (via [forward]'s [AttriaxDispatchResult.Unimplemented] arm).
     */
    fun dispatch(method: String, argsJson: String?): String? {
        val a = if (argsJson.isNullOrBlank()) JSONObject() else JSONObject(argsJson)
        return when (method) {
            // ---- engine-direct: no canonical dispatch key exists ----
            // `tracking.enabled` is not modeled by AttriaxDispatcher.
            "getEventTrackingEnabled" -> result(engine.tracking.enabled)
            "setEventTrackingEnabled" -> { engine.tracking.enabled = bool(a, "enabled", true); null }

            // ---- engine-direct: the Unity C# mapper (AttriaxAndroidEngineMapper) reads
            //      these from the wrapper's serializer shape — UPPERCASE enum `.name`
            //      (COLD_START / VERIFIED / REFERRER / the sync-state name) and the
            //      `deepLinkUri` key — NOT the canonical lowercase `.wire()` / `deepLinkUrl`
            //      shape that execute() emits. Forwarding would silently mis-map, so they
            //      keep the local serializers (which the event listeners also depend on). ----

            // Synchronization state (mapper switches on the UPPERCASE enum name).
            "getSynchronizationState" -> result(engine.synchronization.state.name)

            // ATT: kept wrapper-side like the Flutter plugin — the Unity tracking-auth
            // enum is richer than the engine's canonical status and the mapper consumes
            // the `wireValue` directly.
            "getTrackingAuthorizationStatus" -> result(engine.consent.att.status.wireValue)
            "setTrackingAuthorizationStatus" -> {
                engine.consent.att.setStatus(attStatus(str(a, "status")))
                null
            }
            "requestTrackingAuthorization" ->
                result(engine.consent.att.requestAuthorization(klong(a, "timeoutMs")).wireValue)

            // Deep-link snapshot / resolution getters (UPPERCASE trigger + `rawEvent`).
            "getLatestDeepLink" -> resultObject(serializeDeepLinkEvent(engine.deepLinks.latestDeepLink))
            "getInitialDeepLink" -> resultObject(serializeDeepLinkEvent(engine.deepLinks.initialDeepLink))
            "getRawInitialDeepLink" ->
                resultObject(serializeRawDeepLinkEvent(engine.deepLinks.rawInitialDeepLink))
            "getIsInitialDeepLinkResolved" -> result(engine.deepLinks.initialDeepLinkResolved)
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
            // No dispatch key for dynamic-link creation; stays engine-direct.
            "createDynamicLink" -> resultObject(serializeCreateDynamicLink(createDynamicLink(a)))

            // Referrer getters (mapper expects UPPERCASE `attributionType` + `deepLinkUri`).
            "getOriginalInstallReferrer" ->
                resultObject(serializeInstallReferrer(engine.referrer.getOriginalInstallReferrer()))
            "getReinstallReferrer" ->
                resultObject(serializeInstallReferrer(engine.referrer.getReinstallReferrer()))
            "getSessionReferrer" ->
                resultObject(serializeDeepLinkReferrer(engine.referrer.getSessionReferrer()))
            "getLatestDeepLinkReferrer" ->
                resultObject(serializeDeepLinkReferrer(engine.referrer.getLatestDeepLinkReferrer()))

            // Receipt validation (mapper switches on the UPPERCASE status name).
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

            // ---- name remaps: the C# platform's command name differs from the key ----
            "getSdkEnabled" -> forward("getEnabled", a)
            "setSdkEnabled" -> forward("setEnabled", a)
            // One C# command → the provider-split registration dispatch keys.
            "registerPushToken" -> forward(
                if (str(a, "provider") == "apns") "registerApplePushToken" else "registerFirebaseMessagingToken",
                a,
            )

            // ---- everything else forwards 1:1 to the canonical dispatch table.
            //      (`recordAdEvent` crosses the reserved name under `type`, `recordError`
            //      under `message` — both aligned with AttriaxDispatcher on the C# side.) ----
            else -> forward(method, a)
        }
    }

    /**
     * Forward [method] to the KMP core's canonical [AttriaxDispatcher.execute] and adapt
     * its [AttriaxDispatchResult] to the bridge's JSON reply:
     *  - [AttriaxDispatchResult.Ok] → a `{"result": <value>}` envelope (void commands
     *    carry `null`, which the C# `CallVoid` path ignores);
     *  - [AttriaxDispatchResult.Err] → a thrown exception (surfaced as a faulted C# Task,
     *    matching the old hand-written dispatch's throw-on-error contract); and
     *  - [AttriaxDispatchResult.Unimplemented] → an unknown-command throw.
     */
    private fun forward(method: String, a: JSONObject): String? =
        when (val outcome = AttriaxDispatcher.execute(engine, method, jsonToMap(a))) {
            is AttriaxDispatchResult.Ok -> resultEnvelope(outcome.value)
            is AttriaxDispatchResult.Err -> throw IllegalStateException(outcome.message)
            is AttriaxDispatchResult.Unimplemented ->
                throw IllegalArgumentException("Unknown Attriax command: $method")
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
    // Serialization (KMP result -> JSON for the C# marshaller). Only the shapes the
    // C# mapper reads from a wrapper-specific form (UPPERCASE enum `.name`, `deepLinkUri`)
    // remain here; snapshot / skan maps are produced by AttriaxDispatcher.execute and
    // forwarded verbatim. Best-effort, keyed to the C# type surface.
    // -------------------------------------------------------------------------

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

        /**
         * Wrap a canonical [AttriaxDispatcher] result value (primitive / `Map` / `List` /
         * null) in the `{"result": …}` envelope, recursively lowering `Map`/`List` to
         * `org.json` via [anyToJson] so nested shapes (snapshot metadata, skan maps)
         * cross intact.
         */
        private fun resultEnvelope(value: Any?): String =
            JSONObject().put("result", anyToJson(value)).toString()

        // ---- JSON readers (JSONObject-backed; mirror the Flutter plugin's Map helpers) ----

        private fun str(m: JSONObject, key: String): String? = m.opt(key) as? String

        private fun bool(m: JSONObject, key: String, default: Boolean): Boolean = m.optBoolean(key, default)

        private fun kbool(m: JSONObject, key: String): Boolean? =
            if (m.has(key) && !m.isNull(key)) m.optBoolean(key) else null

        private fun int(m: JSONObject, key: String, default: Int): Int = m.optInt(key, default)

        private fun long(m: JSONObject, key: String, default: Long): Long = m.optLong(key, default)

        private fun klong(m: JSONObject, key: String): Long? =
            if (m.has(key) && !m.isNull(key)) m.optLong(key) else null

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
