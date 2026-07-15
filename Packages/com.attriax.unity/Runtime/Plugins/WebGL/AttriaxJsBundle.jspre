globalThis.AttriaxJs = (function (exports) {
  'use strict';

  var __defProp = Object.defineProperty;
  var __defNormalProp = (obj, key, value) => key in obj ? __defProp(obj, key, { enumerable: true, configurable: true, writable: true, value }) : obj[key] = value;
  var __publicField = (obj, key, value) => __defNormalProp(obj, typeof key !== "symbol" ? key + "" : key, value);

  // src/errors.ts
  var AttriaxApiError = class extends Error {
    constructor(message, statusCode, retriable = false, shouldDrop = false, retryAfterMs) {
      super(message);
      __publicField(this, "statusCode", statusCode);
      __publicField(this, "retriable", retriable);
      __publicField(this, "shouldDrop", shouldDrop);
      __publicField(this, "retryAfterMs", retryAfterMs);
      this.name = "AttriaxApiError";
    }
  };

  // src/types.ts
  var attriaxSdkApiVersion = "v1";
  var attriaxSdkPackageVersion = "0.6.0";
  var AttributionType = /* @__PURE__ */ ((AttributionType2) => {
    AttributionType2["Referrer"] = "referrer";
    AttributionType2["Fingerprint"] = "fingerprint";
    AttributionType2["External"] = "external";
    AttributionType2["Organic"] = "organic";
    return AttributionType2;
  })(AttributionType || {});
  var AttriaxInstallState = /* @__PURE__ */ ((AttriaxInstallState2) => {
    AttriaxInstallState2["Existing"] = "existing";
    AttriaxInstallState2["NewInstall"] = "new_install";
    AttriaxInstallState2["Reinstall"] = "reinstall";
    AttriaxInstallState2["AppDataClear"] = "app_data_clear";
    return AttriaxInstallState2;
  })(AttriaxInstallState || {});
  var AttriaxDeepLinkResolutionStatus = /* @__PURE__ */ ((AttriaxDeepLinkResolutionStatus2) => {
    AttriaxDeepLinkResolutionStatus2["Matched"] = "matched";
    AttriaxDeepLinkResolutionStatus2["Unmatched"] = "unmatched";
    AttriaxDeepLinkResolutionStatus2["Invalid"] = "invalid";
    return AttriaxDeepLinkResolutionStatus2;
  })(AttriaxDeepLinkResolutionStatus || {});
  var AttriaxDeepLinkTrigger = /* @__PURE__ */ ((AttriaxDeepLinkTrigger2) => {
    AttriaxDeepLinkTrigger2["ColdStart"] = "coldStart";
    AttriaxDeepLinkTrigger2["Foreground"] = "foreground";
    AttriaxDeepLinkTrigger2["Deferred"] = "deferred";
    return AttriaxDeepLinkTrigger2;
  })(AttriaxDeepLinkTrigger || {});
  var AttriaxSynchronizationState = /* @__PURE__ */ ((AttriaxSynchronizationState2) => {
    AttriaxSynchronizationState2["Initializing"] = "initializing";
    AttriaxSynchronizationState2["Synchronizing"] = "synchronizing";
    AttriaxSynchronizationState2["Deferred"] = "deferred";
    AttriaxSynchronizationState2["Synchronized"] = "synchronized";
    AttriaxSynchronizationState2["Offline"] = "offline";
    AttriaxSynchronizationState2["Failed"] = "failed";
    AttriaxSynchronizationState2["Disabled"] = "disabled";
    return AttriaxSynchronizationState2;
  })(AttriaxSynchronizationState || {});
  var AttriaxGdprConsentState = /* @__PURE__ */ ((AttriaxGdprConsentState2) => {
    AttriaxGdprConsentState2["Unknown"] = "unknown";
    AttriaxGdprConsentState2["NotRequired"] = "not_required";
    AttriaxGdprConsentState2["Pending"] = "pending";
    AttriaxGdprConsentState2["Granted"] = "granted";
    return AttriaxGdprConsentState2;
  })(AttriaxGdprConsentState || {});
  var AttriaxRevenueReceiptValidationStatus = /* @__PURE__ */ ((AttriaxRevenueReceiptValidationStatus2) => {
    AttriaxRevenueReceiptValidationStatus2["Verified"] = "verified";
    AttriaxRevenueReceiptValidationStatus2["Rejected"] = "rejected";
    AttriaxRevenueReceiptValidationStatus2["Pending"] = "pending";
    AttriaxRevenueReceiptValidationStatus2["Unconfigured"] = "unconfigured";
    AttriaxRevenueReceiptValidationStatus2["ProviderError"] = "provider_error";
    AttriaxRevenueReceiptValidationStatus2["Passthrough"] = "passthrough";
    return AttriaxRevenueReceiptValidationStatus2;
  })(AttriaxRevenueReceiptValidationStatus || {});

  // src/attriax-consent.ts
  var AttriaxConsent = class {
    constructor(client2) {
      __publicField(this, "gdpr");
      __publicField(this, "ccpa");
      this.gdpr = new AttriaxGdprConsent(client2);
      this.ccpa = new AttriaxCcpaConsent(client2);
    }
  };
  var AttriaxCcpaConsent = class {
    constructor(client2) {
      __publicField(this, "client", client2);
    }
    /**
     * Current CCPA do-not-sell election: the value supplied via
     * {@link AttriaxConfig.doNotSell} or {@link setDoNotSell}/{@link set}, else
     * `null` (unset → omitted from the wire).
     */
    get doNotSell() {
      return this.client.currentCcpaDoNotSell;
    }
    /**
     * Current raw IAB US-Privacy string: the value supplied via
     * {@link AttriaxConfig.usPrivacy} or {@link setUsPrivacy}/{@link set}, else
     * `null` (unset/blank → omitted from the wire).
     */
    get usPrivacy() {
      return this.client.currentCcpaUsPrivacy;
    }
    /**
     * Sets the CCPA do-not-sell election. It is emitted (unless `null`) top-level
     * on the next app-open / identify. An explicit `false` is sent (it may clear a
     * prior server-side latch); `null` returns to the omitted (unset) state.
     */
    setDoNotSell(value) {
      this.client.setCcpaDoNotSell(value);
    }
    /**
     * Sets the raw IAB US-Privacy string (for example `1YYN`). It is emitted
     * (unless `null`/blank) top-level on the next app-open / identify, capped at 16
     * characters. `null`/blank returns to the omitted state.
     */
    setUsPrivacy(value) {
      this.client.setCcpaUsPrivacy(value);
    }
    /** Combined setter for both CCPA fields (see {@link setDoNotSell}/{@link setUsPrivacy}). */
    set(doNotSell, usPrivacy) {
      this.client.setCcpaDoNotSell(doNotSell);
      this.client.setCcpaUsPrivacy(usPrivacy);
    }
  };
  var AttriaxGdprConsent = class {
    constructor(client2) {
      __publicField(this, "client", client2);
    }
    /** Current locally known GDPR consent state. */
    get state() {
      return this.client.currentGdprConsentState;
    }
    /** Current resolved consent values, when consent has been granted explicitly. */
    get values() {
      return this.client.currentGdprConsentValues;
    }
    /** Whether the SDK is still waiting for a GDPR decision. */
    get isWaitingForConsent() {
      return this.client.isWaitingForGdprConsent;
    }
    /**
     * Resolves whether this browser still needs a GDPR decision.
     */
    needsConsent(options = {}) {
      return this.client.needsGdprConsent(options);
    }
    /** Stores a granted GDPR decision locally and syncs it to Attriax. */
    setConsent(options) {
      this.client.setGdprConsent(options);
    }
    /** Marks GDPR consent as not required for this browser and syncs that state. */
    setNotRequired() {
      this.client.setGdprConsentNotRequired();
    }
    /** Resets consent back to a pending state and syncs that state. */
    reset() {
      this.client.resetGdprConsent();
    }
  };

  // src/attriax-deep-links.ts
  var AttriaxDeepLinks = class {
    constructor(client2) {
      __publicField(this, "client", client2);
      /** Raw deep-link input subscription. */
      __publicField(this, "rawStream", {
        subscribe: (listener) => createAttriaxSubscription(this.client.subscribeToRawDeepLinks(listener))
      });
      /**
       * Resolved deep-link event subscription.
       *
       * Automatic incoming links emit here after Attriax resolves them. Deferred
       * app-open matches are also emitted here as already-resolved deep-link events.
       *
       * `subscribe()` returns a subscription that can be invoked directly or by
       * calling `subscription.unsubscribe()`.
       */
      __publicField(this, "stream", {
        subscribe: (listener) => createAttriaxSubscription(this.client.subscribeToDeepLinks(listener))
      });
    }
    /** Launch raw deep-link event captured during startup, when one was present. */
    get rawInitialDeepLink() {
      return this.client.currentRawInitialDeepLink;
    }
    /**
     * Launch deep-link event captured during startup, when one was present.
     *
     * This stays `null` until the initial-link probe completes. Use
     * `initialDeepLinkResolved` to distinguish "not resolved yet" from
     * "resolved and no initial deep link was found".
     */
    get initialDeepLink() {
      return this.client.currentInitialDeepLink;
    }
    /** Whether the initial deep-link probe has completed for this browser session. */
    get initialDeepLinkResolved() {
      return this.client.isInitialDeepLinkResolved;
    }
    /**
     * Waits for the initial deep-link probe to finish if it is still pending.
     *
     * This resolves to the launch deep-link event, or `null` when no initial
     * deep link was present.
     */
    waitForInitialDeepLink() {
      return this.client.waitForInitialDeepLink();
    }
    /** Waits for the resolved deep-link event corresponding to `rawEvent`. */
    waitResolution(rawEvent) {
      return this.client.waitForDeepLinkResolution(rawEvent);
    }
    /**
     * Creates a dynamic link through the Attriax backend and returns the final
     * short URL. Redirect options override project defaults per platform.
     */
    createDynamicLink(options) {
      return this.client.createDynamicLink(options);
    }
    /** Resolves and records a deep-link conversion event for the provided URI. */
    recordDeepLink(options) {
      return this.client.recordDeepLink(options);
    }
    /**
     * Most recent handled deep-link event seen by the SDK.
     */
    get latestDeepLink() {
      return this.client.currentLatestDeepLink;
    }
  };
  function createAttriaxSubscription(unsubscribe) {
    const subscription = (() => {
      unsubscribe();
    });
    subscription.unsubscribe = unsubscribe;
    return subscription;
  }

  // src/attriax-referrer.ts
  var AttriaxReferrer = class {
    constructor(client2) {
      __publicField(this, "client", client2);
    }
    /**
     * Original install referrer persisted for this browser install.
     *
     * This resolves from local storage on later launches, or after the first
     * successful app-open request on a fresh install or app-data-clear flow.
     * If tracking is disabled, or GDPR attribution consent is required but not
     * granted, Attriax cannot request or persist this attribution result.
     */
    getOriginalInstallReferrer() {
      return this.client.getOriginalInstallReferrer();
    }
    /**
     * Reinstall referrer persisted for the current installation, when one exists.
     *
     * This resolves after the first successful app-open request that classifies
     * the launch as a reinstall, or from cached storage on later launches.
     * If tracking is disabled, or GDPR attribution consent is required but not
     * granted, Attriax cannot request or persist this attribution result.
     */
    getReinstallReferrer() {
      return this.client.getReinstallReferrer();
    }
    /**
     * Deep-link referrer that opened the current session.
     *
     * This waits for the startup deep-link flow to settle. It resolves to a
     * cold-start or deferred deep-link referrer, or `null` when the current
     * session started without one.
     */
    getSessionReferrer() {
      return this.client.getSessionReferrer();
    }
    /**
     * Most recent deep-link referrer observed in the current session.
     *
     * If no deep link has been received yet, this waits for the next handled
     * deep-link event.
     */
    getLatestDeepLinkReferrer() {
      return this.client.getLatestDeepLinkReferrer();
    }
  };

  // src/attriax-tracking.ts
  var AttriaxTracking = class {
    constructor(client2) {
      __publicField(this, "client", client2);
    }
    /** Whether event-style tracking is currently enabled. */
    get enabled() {
      return this.client.eventsEnabled;
    }
    /** Updates whether event-style tracking is enabled. */
    set enabled(value) {
      this.client.eventsEnabled = value;
    }
    /** Whether anonymous-capable GDPR traffic is dispatched before consent resolves. */
    get anonymousTrackingEnabled() {
      return this.client.anonymousTrackingEnabled;
    }
    /** Updates whether anonymous-capable GDPR traffic is dispatched before consent resolves. */
    set anonymousTrackingEnabled(value) {
      this.client.anonymousTrackingEnabled = value;
    }
    recordEvent(eventName, options) {
      return this.client.recordEvent(eventName, options);
    }
    /**
     * Records a push-notification lifecycle event for attribution.
     *
     * Attriax never sends pushes itself: call this from the host app's own
     * FCM/APNs handler, threading through any Attriax `linkId`/`campaignId`
     * reference embedded in the notification payload. Pass the raw FCM/APNs data
     * map as `payload` and it is preserved under a `payload` key in the
     * notification metadata, and used to infer `source` when it is not supplied.
     *
     * Routes through the same offline-persisted, retried queue as `recordEvent`
     * and honors the same SDK/events-enabled and GDPR analytics-consent gates.
     */
    recordNotification(type, notificationId, options) {
      return this.client.recordNotification(type, notificationId, options);
    }
    /** Records that a push notification was received / displayed. */
    recordNotificationReceived(notificationId, options) {
      return this.client.recordNotificationReceived(notificationId, options);
    }
    /** Records that a push notification was opened (tapped). */
    recordNotificationOpened(notificationId, options) {
      return this.client.recordNotificationOpened(notificationId, options);
    }
    /** Records that a push notification was dismissed without opening. */
    recordNotificationDismissed(notificationId, options) {
      return this.client.recordNotificationDismissed(notificationId, options);
    }
    recordPurchase(revenue, options) {
      return this.client.recordPurchase(revenue, options);
    }
    recordRefund(revenue, options) {
      return this.client.recordRefund(revenue, options);
    }
    recordAdRevenue(revenue, options) {
      return this.client.recordAdRevenue(revenue, options);
    }
    recordAdEvent(type, options) {
      return this.client.recordAdEvent(type, options);
    }
    recordError(error, options) {
      return this.client.recordError(error, options);
    }
    recordPageView(pageName, options) {
      return this.client.recordPageView(pageName, options);
    }
    setUser(userId, options) {
      return this.client.setUser(userId, options);
    }
    setUserProperty(name, value) {
      return this.client.setUserProperty(name, value);
    }
    setUserProperties(properties) {
      return this.client.setUserProperties(properties);
    }
    clearUserProperties(propertyNames) {
      return this.client.clearUserProperties(propertyNames);
    }
  };

  // src/internal/browser.ts
  function createStorage(options = {}) {
    const fallback = /* @__PURE__ */ new Map();
    let didReportPersistentStorageUnavailable = false;
    const reportPersistentStorageUnavailable = (error) => {
      if (didReportPersistentStorageUnavailable || !hasBrowserWindowContext()) {
        return;
      }
      didReportPersistentStorageUnavailable = true;
      options.onPersistentStorageUnavailable?.(error);
    };
    const localStorageRef = getLocalStorage({
      onAccessError: reportPersistentStorageUnavailable
    });
    if (!localStorageRef) {
      return {
        getItem: (key) => fallback.get(key) ?? null,
        setItem: (key, value) => {
          fallback.set(key, value);
        },
        removeItem: (key) => {
          fallback.delete(key);
        }
      };
    }
    return {
      getItem(key) {
        try {
          return localStorageRef.getItem(key);
        } catch (error) {
          reportPersistentStorageUnavailable(error);
          return fallback.get(key) ?? null;
        }
      },
      setItem(key, value) {
        try {
          localStorageRef.setItem(key, value);
        } catch (error) {
          reportPersistentStorageUnavailable(error);
          fallback.set(key, value);
        }
      },
      removeItem(key) {
        try {
          localStorageRef.removeItem(key);
        } catch (error) {
          reportPersistentStorageUnavailable(error);
          fallback.delete(key);
        }
      }
    };
  }
  function hasBrowserWindowContext() {
    return typeof window !== "undefined" && typeof document !== "undefined" && typeof navigator !== "undefined" && typeof window.location?.href === "string" && typeof window.addEventListener === "function" && typeof document.addEventListener === "function";
  }
  function getCurrentUrl() {
    if (typeof window === "undefined" || !window.location?.href) {
      return null;
    }
    try {
      return new URL(window.location.href);
    } catch {
      return null;
    }
  }
  function generateId() {
    if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
      return crypto.randomUUID();
    }
    if (typeof crypto !== "undefined" && typeof crypto.getRandomValues === "function") {
      const bytes = new Uint8Array(16);
      crypto.getRandomValues(bytes);
      bytes[6] = (bytes[6] ?? 0) & 15 | 64;
      bytes[8] = (bytes[8] ?? 0) & 63 | 128;
      const hex = [];
      for (let i = 0; i < bytes.length; i++) {
        hex.push((bytes[i] ?? 0).toString(16).padStart(2, "0"));
      }
      return `${hex.slice(0, 4).join("")}-${hex.slice(4, 6).join("")}-${hex.slice(6, 8).join("")}-${hex.slice(8, 10).join("")}-${hex.slice(10, 16).join("")}`;
    }
    return `unsafe-${unsafeFallbackId()}`;
  }
  var unsafeIdCounter = 0;
  function unsafeFallbackId() {
    unsafeIdCounter = unsafeIdCounter + 1 >>> 0;
    const now = Date.now().toString(36);
    const counter = unsafeIdCounter.toString(36);
    const rand1 = Math.random().toString(36).slice(2, 12);
    const rand2 = Math.random().toString(36).slice(2, 12);
    const perf = typeof performance !== "undefined" && typeof performance.now === "function" ? Math.floor(performance.now() * 1e3).toString(36) : "0";
    return `${now}-${counter}-${perf}-${rand1}${rand2}`;
  }
  function isBrowserOnline() {
    return typeof navigator === "undefined" || navigator.onLine !== false;
  }
  function shouldCaptureInitialUrl(url) {
    return url.pathname !== "/" || url.search.length > 0 || url.hash.length > 0;
  }
  function normalizeUrlInput(value) {
    if (!value) {
      return void 0;
    }
    return value instanceof URL ? value.toString() : value;
  }
  function openResolvedBrowserAction(url, openMode) {
    if (!hasBrowserWindowContext()) {
      return false;
    }
    if (openMode === "external" /* External */) {
      return window.open(url.toString(), "_blank", "noopener,noreferrer") !== null;
    }
    window.location.assign(url.toString());
    return true;
  }
  function toRelativeLinkPath(url) {
    return `${url.pathname}${url.search}${url.hash}`;
  }
  function safeNavigatorLanguage() {
    return typeof navigator !== "undefined" ? normalizeString(navigator.language) : void 0;
  }
  function safeNavigatorAppName() {
    return typeof navigator !== "undefined" ? normalizeString(navigator.appName) : void 0;
  }
  function safeNavigatorBrowserName() {
    return typeof navigator !== "undefined" ? normalizeString(navigator.appCodeName) : void 0;
  }
  function safeNavigatorPlatform() {
    if (typeof navigator === "undefined") {
      return void 0;
    }
    const navigatorWithUserAgentData = navigator;
    return normalizeString(navigatorWithUserAgentData.userAgentData?.platform) || normalizeString(navigator.platform) || void 0;
  }
  function safeNavigatorVendor() {
    return typeof navigator !== "undefined" ? normalizeString(navigator.vendor) : void 0;
  }
  function safeUserAgent() {
    if (typeof navigator === "undefined") {
      return void 0;
    }
    const direct = normalizeString(navigator.userAgent);
    if (direct) {
      return direct;
    }
    const navigatorWithUserAgentData = navigator;
    const brands = navigatorWithUserAgentData.userAgentData?.brands?.map((brand) => {
      const brandName = normalizeString(brand.brand);
      const version = normalizeString(brand.version);
      if (!brandName) {
        return void 0;
      }
      return version ? `${brandName}/${version}` : brandName;
    }).filter((value) => Boolean(value)) ?? [];
    const platformTokens = [
      normalizeString(navigatorWithUserAgentData.userAgentData?.platform) || normalizeString(navigator.platform),
      navigatorWithUserAgentData.userAgentData?.mobile === true ? "mobile" : void 0,
      normalizeString(navigator.language)
    ].filter((value) => Boolean(value));
    if (brands.length > 0 || platformTokens.length > 0) {
      const prefix = platformTokens.length > 0 ? `Mozilla/5.0 (${platformTokens.join("; ")})` : "Mozilla/5.0";
      return [prefix, ...brands].join(" ");
    }
    const fallback = [
      normalizeString(navigator.vendor),
      normalizeString(navigator.appVersion),
      normalizeString(navigator.product)
    ].filter((value) => Boolean(value));
    return fallback.length > 0 ? fallback.join(" ") : void 0;
  }
  function safeDocumentTitle() {
    return typeof document !== "undefined" ? normalizeString(document.title) : void 0;
  }
  function safeDocumentReferrer() {
    return typeof document !== "undefined" ? normalizeString(document.referrer) : void 0;
  }
  function safeTimeZone() {
    try {
      return normalizeString(Intl.DateTimeFormat().resolvedOptions().timeZone);
    } catch {
      return void 0;
    }
  }
  function safeScreenResolution() {
    if (typeof window === "undefined" || !window.screen) {
      return void 0;
    }
    const width = window.screen.width;
    const height = window.screen.height;
    if (!Number.isFinite(width) || !Number.isFinite(height) || width <= 0 || height <= 0) {
      return void 0;
    }
    return `${width}x${height}`;
  }
  function safeScreenWidth() {
    return normalizePositiveNumber(typeof window !== "undefined" ? window.screen?.width : void 0);
  }
  function safeScreenHeight() {
    return normalizePositiveNumber(typeof window !== "undefined" ? window.screen?.height : void 0);
  }
  function safeDevicePixelRatio() {
    return normalizePositiveNumber(typeof window !== "undefined" ? window.devicePixelRatio : void 0);
  }
  function safeColorDepth() {
    return normalizePositiveInteger(typeof window !== "undefined" ? window.screen?.colorDepth : void 0);
  }
  function getLocalStorage(options = {}) {
    try {
      return typeof window !== "undefined" ? window.localStorage : null;
    } catch (error) {
      options.onAccessError?.(error);
      return null;
    }
  }
  function normalizeString(value) {
    const normalized = value?.trim();
    return normalized ? normalized : void 0;
  }
  function normalizePositiveNumber(value) {
    if (typeof value !== "number" || !Number.isFinite(value) || value <= 0) {
      return void 0;
    }
    return value;
  }
  function normalizePositiveInteger(value) {
    const normalized = normalizePositiveNumber(value);
    return normalized == null ? void 0 : Math.round(normalized);
  }
  function detectBotEnvironment() {
    const ua = safeUserAgent();
    if (!ua) {
      return false;
    }
    if (typeof navigator !== "undefined" && navigator.webdriver === true) {
      return true;
    }
    if (typeof window !== "undefined") {
      if (window.screen?.width === 0 || window.screen?.height === 0) {
        return true;
      }
      if (typeof window.requestAnimationFrame === "undefined") {
        return true;
      }
    }
    const lowerUa = ua.toLowerCase();
    const botPatterns = [
      "bot",
      "crawler",
      "spider",
      "scraper",
      "wget",
      "curl",
      "headless",
      "gptbot",
      "claudebot",
      "anthropic",
      "googlebot",
      "bingbot",
      "yandexbot",
      "baiduspider",
      "facebookbot",
      "twitterbot",
      "linkedinbot",
      "commoncrawl",
      "semrush",
      "ahrefs",
      "mj12bot",
      "screaming frog",
      "applebot",
      "duckduckbot",
      "petalbot",
      "whatsapp",
      "slackbot",
      "telegrambot",
      "discordbot",
      "redditbot"
    ];
    if (botPatterns.some((p) => lowerUa.includes(p))) {
      return true;
    }
    return false;
  }

  // src/internal/ad-events.ts
  var ATTRIAX_AD_EVENT_NAME_BY_TYPE = {
    request: "ad_request",
    load: "ad_load",
    load_failed: "ad_load_failed",
    show: "ad_show",
    show_failed: "ad_show_failed",
    impression: "ad_impression",
    click: "ad_click",
    dismiss: "ad_dismiss",
    reward: "ad_reward"
  };
  function isAdEventName(eventName) {
    return eventName === "ad_revenue" || Object.values(ATTRIAX_AD_EVENT_NAME_BY_TYPE).includes(
      eventName
    );
  }

  // src/internal/activation-coordinator.ts
  var AttriaxActivationCoordinator = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    completeInitialization(options) {
      if (!options.enabled) {
        this.options.appOpenLaunchCoordinator.disable();
        this.options.deepLinkManager.observeInitialUrlCapture(false);
        this.options.synchronizer.setState("disabled" /* Disabled */);
        return "disabled";
      }
      if (this.options.shouldDeferNetworkDispatch()) {
        this.options.appOpenLaunchCoordinator.defer();
        this.options.deepLinkManager.observeInitialUrlCapture(options.captureInitialUrlEnabled);
        this.options.sessionLifecycleManager.restartSessionHeartbeatTimer();
        this.options.synchronizer.setState("deferred" /* Deferred */);
        return "deferred";
      }
      this.options.appOpenLaunchCoordinator.activate({
        initialized: true,
        enabled: true,
        allowsAttributionTracking: this.options.allowsAttributionTracking()
      });
      this.options.deepLinkManager.observeInitialUrlCapture(options.captureInitialUrlEnabled);
      this.options.sessionLifecycleManager.restartSessionHeartbeatTimer();
      void this.options.consentManager.flushPendingSync();
      void this.options.flushQueue();
      this.options.synchronizer.setState(
        this.options.synchronizer.queue.length > 0 ? "synchronizing" /* Synchronizing */ : "synchronized" /* Synchronized */
      );
      return "active";
    }
    applyEnabledState(options) {
      if (!options.enabled) {
        this.options.sessionLifecycleManager.stopSessionHeartbeatTimer();
        this.options.synchronizer.setState("disabled" /* Disabled */);
        return "disabled";
      }
      this.options.sessionLifecycleManager.isBackgrounded = false;
      if (this.options.shouldTrackSessionActivity()) {
        this.options.sessionManager.resumeOrStartSession(/* @__PURE__ */ new Date());
      } else {
        this.options.sessionManager.clear();
      }
      if (this.options.shouldDeferNetworkDispatch()) {
        this.options.appOpenLaunchCoordinator.defer();
        this.options.deepLinkManager.observeInitialUrlCapture(options.captureInitialUrlEnabled);
        this.options.sessionLifecycleManager.restartSessionHeartbeatTimer();
        this.options.synchronizer.setState("deferred" /* Deferred */);
        return "deferred";
      }
      this.options.appOpenLaunchCoordinator.activate({
        initialized: true,
        enabled: true,
        allowsAttributionTracking: this.options.allowsAttributionTracking(),
        prepareInstallReferrer: true
      });
      this.options.deepLinkManager.observeInitialUrlCapture(options.captureInitialUrlEnabled);
      this.options.sessionLifecycleManager.restartSessionHeartbeatTimer();
      void this.options.consentManager.flushPendingSync();
      void this.options.flushQueue();
      this.options.synchronizer.setState("synchronizing" /* Synchronizing */);
      return "active";
    }
  };

  // src/internal/app-open-launch-coordinator.ts
  var AttriaxAppOpenLaunchCoordinator = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    prepareForReenable() {
      this.options.installReferrerManager.prepareForReenable();
    }
    activate(options) {
      if (!options.allowsAttributionTracking) {
        this.disable();
        return;
      }
      if (options.prepareInstallReferrer) {
        this.options.installReferrerManager.prepareForEnabledState();
      }
      this.options.appOpenManager.scheduleIfNeeded({
        initialized: options.initialized,
        enabled: options.enabled
      });
      if (!this.options.appOpenManager.didSchedule) {
        this.options.appOpenManager.resetInstallReferrer();
      }
    }
    defer() {
      this.options.appOpenManager.resetInstallReferrer();
    }
    disable() {
      this.options.appOpenManager.resetInstallReferrer();
      this.options.installReferrerManager.completeDisabled();
    }
  };

  // src/internal/app-open-tracker.ts
  var AttriaxAppOpenTracker = class {
    constructor() {
      __publicField(this, "trackingPromise", null);
      __publicField(this, "installReferrerPromiseValue", Promise.resolve(null));
      __publicField(this, "lastResultValue", null);
    }
    get lastResult() {
      return this.lastResultValue;
    }
    get installReferrer() {
      return this.installReferrerPromiseValue;
    }
    get didSchedule() {
      return this.trackingPromise !== null;
    }
    resetInstallReferrer() {
      this.installReferrerPromiseValue = Promise.resolve(null);
    }
    schedule(track) {
      if (this.trackingPromise) {
        return this.trackingPromise;
      }
      const trackedPromise = track().then((result) => {
        this.lastResultValue = result;
        return result;
      });
      this.trackingPromise = trackedPromise;
      this.installReferrerPromiseValue = trackedPromise.then(
        (result) => result?.installReferrer ?? null
      );
      void this.installReferrerPromiseValue.catch(() => void 0);
      return trackedPromise;
    }
    async waitForResult() {
      return this.trackingPromise ? await this.trackingPromise : null;
    }
    reset() {
      this.trackingPromise = null;
      this.installReferrerPromiseValue = Promise.resolve(null);
      this.lastResultValue = null;
    }
  };

  // src/internal/json.ts
  function normalizeJsonObject(value) {
    if (!value) {
      return void 0;
    }
    return normalizeJsonMap(value);
  }
  function normalizeJsonMap(input) {
    const output = {};
    for (const [key, value] of Object.entries(input)) {
      output[key] = normalizeJsonValue(value);
    }
    return output;
  }
  function normalizeJsonValue(value) {
    if (value === null) {
      return null;
    }
    if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
      return value;
    }
    if (Array.isArray(value)) {
      return value.map(normalizeJsonValue);
    }
    return normalizeJsonMap(value);
  }
  function ensureRecord(value) {
    if (!value || typeof value !== "object" || Array.isArray(value)) {
      throw new AttriaxApiError("Invalid Attriax response payload.", void 0, false, true);
    }
    return value;
  }
  function ensureString(value, fieldName) {
    if (typeof value !== "string" || value.trim().length === 0) {
      throw new AttriaxApiError(
        `Missing or invalid "${fieldName}" in Attriax response.`,
        void 0,
        false,
        true
      );
    }
    return value;
  }
  function optionalString(value) {
    return typeof value === "string" && value.length > 0 ? value : void 0;
  }
  function optionalDate(value) {
    if (typeof value !== "string") {
      return void 0;
    }
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? void 0 : date;
  }
  function optionalJsonObject(value) {
    if (!value || typeof value !== "object" || Array.isArray(value)) {
      return void 0;
    }
    const jsonObject = {};
    for (const [key, nestedValue] of Object.entries(value)) {
      jsonObject[key] = coerceJsonValue(nestedValue);
    }
    return jsonObject;
  }
  function coerceJsonValue(value) {
    if (value === null || typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
      return value;
    }
    if (Array.isArray(value)) {
      return value.map(coerceJsonValue);
    }
    if (value && typeof value === "object") {
      const nested = {};
      for (const [key, nestedValue] of Object.entries(value)) {
        nested[key] = coerceJsonValue(nestedValue);
      }
      return nested;
    }
    return String(value);
  }

  // src/internal/app-open-manager.ts
  var AttriaxAppOpenManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "tracker", new AttriaxAppOpenTracker());
      __publicField(this, "scheduledDeferred", null);
    }
    get installReferrer() {
      return this.tracker.installReferrer;
    }
    get didSchedule() {
      return this.tracker.didSchedule;
    }
    resetInstallReferrer() {
      this.tracker.resetInstallReferrer();
    }
    async waitForScheduledResult() {
      if (!this.tracker.didSchedule) {
        const scheduledDeferred = this.scheduledDeferred ?? (this.scheduledDeferred = createDeferred());
        await scheduledDeferred.promise;
      }
      if (!this.tracker.didSchedule) {
        return null;
      }
      return this.tracker.waitForResult();
    }
    scheduleIfNeeded({
      initialized,
      enabled
    }) {
      if (!initialized || !enabled || this.tracker.didSchedule) {
        return;
      }
      void this.schedule();
    }
    dispose() {
      this.completeScheduled();
    }
    reset() {
      this.completeScheduled();
      this.scheduledDeferred = null;
      this.tracker.reset();
    }
    schedule() {
      void this.tracker.schedule(
        () => this.options.requestManager.enqueue({
          kind: "open",
          payload: this.buildOpenRequest()
        })
      ).catch(() => void 0);
      this.completeScheduled();
    }
    buildOpenRequest() {
      const snapshot = this.options.contextManager.requireSnapshot();
      const currentSession = this.options.sessionManager.currentSession;
      const device = {
        metadata: normalizeJsonObject(snapshot.device.metadata)
      };
      if (snapshot.device.model) {
        device.model = snapshot.device.model;
      }
      if (snapshot.device.name) {
        device.name = snapshot.device.name;
      }
      if (snapshot.device.brand) {
        device.brand = snapshot.device.brand;
      }
      if (snapshot.device.manufacturer) {
        device.manufacturer = snapshot.device.manufacturer;
      }
      if (snapshot.device.hardware) {
        device.hardware = snapshot.device.hardware;
      }
      if (snapshot.device.osVersion) {
        device.osVersion = snapshot.device.osVersion;
      }
      if (snapshot.device.language) {
        device.language = snapshot.device.language;
      }
      if (snapshot.device.timezone) {
        device.timezone = snapshot.device.timezone;
      }
      if (snapshot.device.screenResolution) {
        device.screenResolution = snapshot.device.screenResolution;
      }
      if (snapshot.device.screenWidth != null) {
        device.screenWidth = snapshot.device.screenWidth;
      }
      if (snapshot.device.screenHeight != null) {
        device.screenHeight = snapshot.device.screenHeight;
      }
      if (snapshot.device.devicePixelRatio != null) {
        device.devicePixelRatio = snapshot.device.devicePixelRatio;
      }
      if (snapshot.device.colorDepth != null) {
        device.colorDepth = snapshot.device.colorDepth;
      }
      const app = {};
      if (snapshot.app.version) {
        app.version = snapshot.app.version;
      }
      if (snapshot.app.buildNumber) {
        app.buildNumber = snapshot.app.buildNumber;
      }
      if (snapshot.app.packageName) {
        app.packageName = snapshot.app.packageName;
      }
      const sdk = {
        apiVersion: attriaxSdkApiVersion,
        packageVersion: attriaxSdkPackageVersion
      };
      const sdkMetadata = normalizeJsonObject(this.options.config.sdkMetadata);
      if (sdkMetadata) {
        sdk.metadata = sdkMetadata;
      }
      return {
        platform: snapshot.platform,
        deviceId: snapshot.deviceId ?? this.options.contextManager.requireDeviceId(),
        deviceIdSource: this.options.contextManager.requireDeviceIdSource(),
        sdk,
        app,
        device,
        isFirstLaunch: currentSession?.isFirstLaunch ?? snapshot.isFirstLaunch,
        ...currentSession ? {
          sessionId: currentSession.id,
          sessionStartedAt: currentSession.startedAt.toISOString()
        } : {},
        // CCPA fields ride top-level (like `attStatus`), never nested under
        // `device`. A null doNotSell and a null/blank usPrivacy are omitted.
        ...this.options.ccpaState.toWireFields()
      };
    }
    completeScheduled() {
      const scheduledDeferred = this.scheduledDeferred;
      if (!scheduledDeferred || scheduledDeferred.isCompleted) {
        return;
      }
      scheduledDeferred.resolve();
    }
  };
  function createDeferred() {
    let resolve;
    let reject;
    let isCompleted = false;
    const promise = new Promise((resolved, rejected) => {
      resolve = (value) => {
        isCompleted = true;
        resolved(value);
      };
      reject = (reason) => {
        isCompleted = true;
        rejected(reason);
      };
    });
    return {
      promise,
      resolve,
      reject,
      get isCompleted() {
        return isCompleted;
      }
    };
  }

  // src/internal/page-tracking.ts
  var historyWrapState = null;
  function acquireHistoryWrap(onChange) {
    if (typeof window === "undefined") {
      return () => void 0;
    }
    if (!historyWrapState) {
      const originalPushState = window.history.pushState.bind(
        window.history
      );
      const originalReplaceState = window.history.replaceState.bind(window.history);
      const state = {
        refCount: 0,
        originalPushState,
        originalReplaceState,
        listeners: /* @__PURE__ */ new Set()
      };
      const notify = () => {
        for (const listener of state.listeners) {
          try {
            listener();
          } catch {
          }
        }
      };
      window.history.pushState = ((data, unused, url) => {
        originalPushState(data, unused, url);
        notify();
      });
      window.history.replaceState = ((data, unused, url) => {
        originalReplaceState(data, unused, url);
        notify();
      });
      historyWrapState = state;
    }
    historyWrapState.listeners.add(onChange);
    historyWrapState.refCount += 1;
    return () => {
      if (!historyWrapState) {
        return;
      }
      historyWrapState.listeners.delete(onChange);
      historyWrapState.refCount -= 1;
      if (historyWrapState.refCount <= 0) {
        window.history.pushState = historyWrapState.originalPushState;
        window.history.replaceState = historyWrapState.originalReplaceState;
        historyWrapState = null;
      }
    };
  }
  function attachAutomaticPageTracker(options) {
    if (typeof window === "undefined") {
      return () => void 0;
    }
    let scheduledTimer = null;
    let lastTrackedPageName = null;
    const trackCurrentPage = () => {
      scheduledTimer = null;
      if (!options.isEnabled()) {
        return;
      }
      const url = getCurrentUrl();
      if (!url) {
        return;
      }
      const currentPageName = toRelativeLinkPath(url);
      if (!currentPageName || currentPageName === lastTrackedPageName) {
        return;
      }
      const previousPageName = lastTrackedPageName ?? void 0;
      lastTrackedPageName = currentPageName;
      void options.onRecordPageView(currentPageName, {
        previousPageName,
        pageTitle: safeDocumentTitle(),
        source: options.source ?? "automatic_page_tracking"
      }).catch((error) => {
        options.onError?.(error);
      });
    };
    const scheduleTrack = () => {
      if (scheduledTimer !== null) {
        globalThis.clearTimeout(scheduledTimer);
      }
      scheduledTimer = globalThis.setTimeout(trackCurrentPage, 0);
    };
    const popStateHandler = () => {
      scheduleTrack();
    };
    const hashChangeHandler = () => {
      scheduleTrack();
    };
    const releaseHistoryWrap = acquireHistoryWrap(scheduleTrack);
    window.addEventListener("popstate", popStateHandler);
    window.addEventListener("hashchange", hashChangeHandler);
    scheduleTrack();
    return () => {
      if (scheduledTimer !== null) {
        globalThis.clearTimeout(scheduledTimer);
      }
      releaseHistoryWrap();
      window.removeEventListener("popstate", popStateHandler);
      window.removeEventListener("hashchange", hashChangeHandler);
    };
  }

  // src/internal/crash-reporting-coordinator.ts
  var AttriaxCrashReportingCoordinator = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    attach() {
      if (typeof window === "undefined") {
        return [];
      }
      const errorHandler = (event) => {
        if (!this.shouldTrackCrashes()) {
          return;
        }
        const payload = toWindowErrorPayload(event);
        if (!payload) {
          return;
        }
        void this.options.trackingManager.recordError(payload.error, {
          source: "window_error",
          metadata: payload.metadata,
          reason: "Unhandled browser error"
        }).catch((error) => {
          this.options.logger.error("Automatic browser error reporting failed.", { error });
        });
      };
      const unhandledRejectionHandler = (event) => {
        if (!this.shouldTrackCrashes()) {
          return;
        }
        const rejectionEvent = event;
        const metadata = {};
        const constructorName = rejectionEvent.reason && typeof rejectionEvent.reason === "object" && rejectionEvent.reason.constructor?.name ? rejectionEvent.reason.constructor.name : void 0;
        if (constructorName) {
          metadata.reasonType = constructorName;
        }
        void this.options.trackingManager.recordError(rejectionEvent.reason, {
          source: "unhandled_rejection",
          metadata: Object.keys(metadata).length > 0 ? metadata : void 0,
          reason: "Unhandled promise rejection"
        }).catch((error) => {
          this.options.logger.error("Automatic promise rejection reporting failed.", { error });
        });
      };
      window.addEventListener("error", errorHandler);
      window.addEventListener("unhandledrejection", unhandledRejectionHandler);
      return [
        () => window.removeEventListener("error", errorHandler),
        () => window.removeEventListener("unhandledrejection", unhandledRejectionHandler)
      ];
    }
    shouldTrackCrashes() {
      return this.options.isInitialized() && !this.options.isDisposed() && this.options.settingsState.isEnabled;
    }
  };
  function toWindowErrorPayload(event) {
    const errorEvent = event;
    const hasMessage = typeof errorEvent.message === "string" && errorEvent.message.trim().length > 0;
    if (errorEvent.error == null && !hasMessage) {
      return null;
    }
    const metadata = {};
    if (typeof errorEvent.filename === "string" && errorEvent.filename.trim().length > 0) {
      metadata.filename = errorEvent.filename;
    }
    if (typeof errorEvent.lineno === "number" && Number.isFinite(errorEvent.lineno)) {
      metadata.lineNumber = errorEvent.lineno;
    }
    if (typeof errorEvent.colno === "number" && Number.isFinite(errorEvent.colno)) {
      metadata.columnNumber = errorEvent.colno;
    }
    return {
      error: errorEvent.error ?? errorEvent.message ?? "Unhandled browser error",
      metadata: Object.keys(metadata).length > 0 ? metadata : void 0
    };
  }

  // src/internal/browser-bindings-manager.ts
  var AttriaxBrowserBindingsManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "cleanupCallbacks", []);
      __publicField(this, "attached", false);
      __publicField(this, "crashReportingCoordinator");
      this.crashReportingCoordinator = new AttriaxCrashReportingCoordinator({
        logger: options.logger,
        settingsState: options.settingsState,
        trackingManager: options.trackingManager,
        isInitialized: options.isInitialized,
        isDisposed: options.isDisposed
      });
    }
    attach() {
      if (this.attached || typeof window === "undefined" || typeof window.addEventListener !== "function") {
        return;
      }
      this.attached = true;
      const documentRef = typeof document !== "undefined" ? document : null;
      this.options.sessionLifecycleManager.isBackgrounded = documentRef?.visibilityState === "hidden";
      const onlineHandler = () => {
        if (this.options.settingsState.isEnabled) {
          void this.options.flush();
        }
      };
      const offlineHandler = () => {
        if (this.options.settingsState.isEnabled) {
          this.options.synchronizer.setState("offline" /* Offline */);
        }
      };
      const visibilityHandler = () => {
        if (documentRef?.visibilityState === "hidden") {
          this.options.sessionLifecycleManager.handleBrowserHidden();
        } else if (documentRef?.visibilityState === "visible") {
          this.options.sessionLifecycleManager.handleBrowserVisible();
        }
        if (this.options.settingsState.isEnabled) {
          void this.options.flush();
        }
      };
      const pageHideHandler = () => {
        this.options.sessionLifecycleManager.handlePageHide();
        void this.options.flush();
      };
      window.addEventListener("online", onlineHandler);
      window.addEventListener("offline", offlineHandler);
      window.addEventListener("pagehide", pageHideHandler);
      documentRef?.addEventListener("visibilitychange", visibilityHandler);
      this.cleanupCallbacks.push(() => window.removeEventListener("online", onlineHandler));
      this.cleanupCallbacks.push(() => window.removeEventListener("offline", offlineHandler));
      this.cleanupCallbacks.push(() => window.removeEventListener("pagehide", pageHideHandler));
      this.cleanupCallbacks.push(...this.crashReportingCoordinator.attach());
      if (documentRef) {
        this.cleanupCallbacks.push(
          () => documentRef.removeEventListener("visibilitychange", visibilityHandler)
        );
      }
      if (this.options.automaticPageTrackingEnabled) {
        this.cleanupCallbacks.push(
          attachAutomaticPageTracker({
            isEnabled: () => this.options.isInitialized() && !this.options.isDisposed() && this.options.settingsState.isEnabled && this.options.settingsState.areEventsEnabled,
            onRecordPageView: (pageName, options) => this.options.trackingManager.recordPageView(pageName, options),
            onError: (error) => {
              this.options.logger.error("Automatic page tracking failed.", { error });
            }
          })
        );
      }
    }
    dispose() {
      if (!this.attached) {
        return;
      }
      this.attached = false;
      for (const cleanup of this.cleanupCallbacks.splice(0)) {
        cleanup();
      }
    }
  };

  // src/internal/bootstrap-coordinator.ts
  var AttriaxBootstrapCoordinator = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    async initialize(options) {
      this.options.consentManager.init();
      this.options.syncRuntimePersistenceMode();
      const resolvedEnabled = this.options.runtimeSettingsStore.readEnabled(true);
      const resolvedEventsEnabled = this.options.runtimeSettingsStore.readEventsEnabled(true);
      this.options.settingsState.restore({
        enabled: resolvedEnabled,
        eventsEnabled: resolvedEventsEnabled
      });
      await this.options.contextManager.init({
        allowDeviceIdentity: this.options.allowDeviceIdentity()
      });
      if (!this.options.isStillValid(options.generation)) {
        return "aborted";
      }
      if (this.options.shouldTrackSessionActivity()) {
        this.options.restoreOrStartSession(/* @__PURE__ */ new Date());
      } else {
        this.options.sessionManager.clear();
      }
      this.options.installReferrerManager.init({
        enabled: resolvedEnabled && this.options.shouldEnableInstallReferrer()
      });
      this.options.markInitialized();
      this.options.launchStateStore.markLaunched();
      this.options.browserBindingsManager.attach();
      return this.options.activationCoordinator.completeInitialization({
        enabled: resolvedEnabled,
        captureInitialUrlEnabled: options.captureInitialUrlEnabled
      });
    }
  };

  // src/internal/ccpa-state.ts
  var AttriaxCcpaState = class {
    constructor(seed) {
      __publicField(this, "doNotSellValue");
      __publicField(this, "usPrivacyValue");
      this.doNotSellValue = seed.doNotSell ?? null;
      this.usPrivacyValue = seed.usPrivacy ?? null;
    }
    /** Current do-not-sell election, or `null` when unset. */
    get doNotSell() {
      return this.doNotSellValue;
    }
    /** Current raw IAB US-Privacy string, or `null` when unset. */
    get usPrivacy() {
      return this.usPrivacyValue;
    }
    setDoNotSell(value) {
      this.doNotSellValue = value ?? null;
    }
    setUsPrivacy(value) {
      this.usPrivacyValue = value ?? null;
    }
    /**
     * Resolves the top-level CCPA wire fields for an app-open / identify request.
     *
     * A `null` `doNotSell` is omitted; an explicit `true`/`false` is emitted (a
     * deliberate `false` may clear a prior server-side latch). A `null`/blank
     * `usPrivacy` is omitted; otherwise it is emitted, defensively capped at 16
     * characters (the API DTO's `@MaxLength(16)`).
     */
    toWireFields() {
      const fields = {};
      if (this.doNotSellValue != null) {
        fields.doNotSell = this.doNotSellValue;
      }
      if (this.usPrivacyValue != null && this.usPrivacyValue.trim().length > 0) {
        fields.usPrivacy = this.usPrivacyValue.slice(0, 16);
      }
      return fields;
    }
  };

  // src/generated/core/bodySerializer.gen.ts
  var jsonBodySerializer = {
    bodySerializer: (body) => JSON.stringify(
      body,
      (_key, value) => typeof value === "bigint" ? value.toString() : value
    )
  };

  // src/generated/core/serverSentEvents.gen.ts
  var createSseClient = ({
    onRequest,
    onSseError,
    onSseEvent,
    responseTransformer,
    responseValidator,
    sseDefaultRetryDelay,
    sseMaxRetryAttempts,
    sseMaxRetryDelay,
    sseSleepFn,
    url,
    ...options
  }) => {
    let lastEventId;
    const sleep = sseSleepFn ?? ((ms) => new Promise((resolve) => setTimeout(resolve, ms)));
    const createStream = async function* () {
      let retryDelay = sseDefaultRetryDelay ?? 3e3;
      let attempt = 0;
      const signal = options.signal ?? new AbortController().signal;
      while (true) {
        if (signal.aborted) break;
        attempt++;
        const headers = options.headers instanceof Headers ? options.headers : new Headers(options.headers);
        if (lastEventId !== void 0) {
          headers.set("Last-Event-ID", lastEventId);
        }
        try {
          const requestInit = {
            redirect: "follow",
            ...options,
            body: options.serializedBody,
            headers,
            signal
          };
          let request = new Request(url, requestInit);
          if (onRequest) {
            request = await onRequest(url, requestInit);
          }
          const _fetch = options.fetch ?? globalThis.fetch;
          const response = await _fetch(request);
          if (!response.ok)
            throw new Error(
              `SSE failed: ${response.status} ${response.statusText}`
            );
          if (!response.body) throw new Error("No body in SSE response");
          const reader = response.body.pipeThrough(new TextDecoderStream()).getReader();
          let buffer = "";
          const abortHandler = () => {
            try {
              reader.cancel();
            } catch {
            }
          };
          signal.addEventListener("abort", abortHandler);
          try {
            while (true) {
              const { done, value } = await reader.read();
              if (done) break;
              buffer += value;
              buffer = buffer.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
              const chunks = buffer.split("\n\n");
              buffer = chunks.pop() ?? "";
              for (const chunk of chunks) {
                const lines = chunk.split("\n");
                const dataLines = [];
                let eventName;
                for (const line of lines) {
                  if (line.startsWith("data:")) {
                    dataLines.push(line.replace(/^data:\s*/, ""));
                  } else if (line.startsWith("event:")) {
                    eventName = line.replace(/^event:\s*/, "");
                  } else if (line.startsWith("id:")) {
                    lastEventId = line.replace(/^id:\s*/, "");
                  } else if (line.startsWith("retry:")) {
                    const parsed = Number.parseInt(
                      line.replace(/^retry:\s*/, ""),
                      10
                    );
                    if (!Number.isNaN(parsed)) {
                      retryDelay = parsed;
                    }
                  }
                }
                let data;
                let parsedJson = false;
                if (dataLines.length) {
                  const rawData = dataLines.join("\n");
                  try {
                    data = JSON.parse(rawData);
                    parsedJson = true;
                  } catch {
                    data = rawData;
                  }
                }
                if (parsedJson) {
                  if (responseValidator) {
                    await responseValidator(data);
                  }
                  if (responseTransformer) {
                    data = await responseTransformer(data);
                  }
                }
                onSseEvent?.({
                  data,
                  event: eventName,
                  id: lastEventId,
                  retry: retryDelay
                });
                if (dataLines.length) {
                  yield data;
                }
              }
            }
          } finally {
            signal.removeEventListener("abort", abortHandler);
            reader.releaseLock();
          }
          break;
        } catch (error) {
          onSseError?.(error);
          if (sseMaxRetryAttempts !== void 0 && attempt >= sseMaxRetryAttempts) {
            break;
          }
          const backoff = Math.min(
            retryDelay * 2 ** (attempt - 1),
            sseMaxRetryDelay ?? 3e4
          );
          await sleep(backoff);
        }
      }
    };
    const stream = createStream();
    return { stream };
  };

  // src/generated/core/pathSerializer.gen.ts
  var separatorArrayExplode = (style) => {
    switch (style) {
      case "label":
        return ".";
      case "matrix":
        return ";";
      case "simple":
        return ",";
      default:
        return "&";
    }
  };
  var separatorArrayNoExplode = (style) => {
    switch (style) {
      case "form":
        return ",";
      case "pipeDelimited":
        return "|";
      case "spaceDelimited":
        return "%20";
      default:
        return ",";
    }
  };
  var separatorObjectExplode = (style) => {
    switch (style) {
      case "label":
        return ".";
      case "matrix":
        return ";";
      case "simple":
        return ",";
      default:
        return "&";
    }
  };
  var serializeArrayParam = ({
    allowReserved,
    explode,
    name,
    style,
    value
  }) => {
    if (!explode) {
      const joinedValues2 = (allowReserved ? value : value.map((v) => encodeURIComponent(v))).join(separatorArrayNoExplode(style));
      switch (style) {
        case "label":
          return `.${joinedValues2}`;
        case "matrix":
          return `;${name}=${joinedValues2}`;
        case "simple":
          return joinedValues2;
        default:
          return `${name}=${joinedValues2}`;
      }
    }
    const separator = separatorArrayExplode(style);
    const joinedValues = value.map((v) => {
      if (style === "label" || style === "simple") {
        return allowReserved ? v : encodeURIComponent(v);
      }
      return serializePrimitiveParam({
        allowReserved,
        name,
        value: v
      });
    }).join(separator);
    return style === "label" || style === "matrix" ? separator + joinedValues : joinedValues;
  };
  var serializePrimitiveParam = ({
    allowReserved,
    name,
    value
  }) => {
    if (value === void 0 || value === null) {
      return "";
    }
    if (typeof value === "object") {
      throw new Error(
        "Deeply-nested arrays/objects aren\u2019t supported. Provide your own `querySerializer()` to handle these."
      );
    }
    return `${name}=${allowReserved ? value : encodeURIComponent(value)}`;
  };
  var serializeObjectParam = ({
    allowReserved,
    explode,
    name,
    style,
    value,
    valueOnly
  }) => {
    if (value instanceof Date) {
      return valueOnly ? value.toISOString() : `${name}=${value.toISOString()}`;
    }
    if (style !== "deepObject" && !explode) {
      let values = [];
      Object.entries(value).forEach(([key, v]) => {
        values = [
          ...values,
          key,
          allowReserved ? v : encodeURIComponent(v)
        ];
      });
      const joinedValues2 = values.join(",");
      switch (style) {
        case "form":
          return `${name}=${joinedValues2}`;
        case "label":
          return `.${joinedValues2}`;
        case "matrix":
          return `;${name}=${joinedValues2}`;
        default:
          return joinedValues2;
      }
    }
    const separator = separatorObjectExplode(style);
    const joinedValues = Object.entries(value).map(
      ([key, v]) => serializePrimitiveParam({
        allowReserved,
        name: style === "deepObject" ? `${name}[${key}]` : key,
        value: v
      })
    ).join(separator);
    return style === "label" || style === "matrix" ? separator + joinedValues : joinedValues;
  };

  // src/generated/core/utils.gen.ts
  var PATH_PARAM_RE = /\{[^{}]+\}/g;
  var defaultPathSerializer = ({ path, url: _url }) => {
    let url = _url;
    const matches = _url.match(PATH_PARAM_RE);
    if (matches) {
      for (const match of matches) {
        let explode = false;
        let name = match.substring(1, match.length - 1);
        let style = "simple";
        if (name.endsWith("*")) {
          explode = true;
          name = name.substring(0, name.length - 1);
        }
        if (name.startsWith(".")) {
          name = name.substring(1);
          style = "label";
        } else if (name.startsWith(";")) {
          name = name.substring(1);
          style = "matrix";
        }
        const value = path[name];
        if (value === void 0 || value === null) {
          continue;
        }
        if (Array.isArray(value)) {
          url = url.replace(
            match,
            serializeArrayParam({ explode, name, style, value })
          );
          continue;
        }
        if (typeof value === "object") {
          url = url.replace(
            match,
            serializeObjectParam({
              explode,
              name,
              style,
              value,
              valueOnly: true
            })
          );
          continue;
        }
        if (style === "matrix") {
          url = url.replace(
            match,
            `;${serializePrimitiveParam({
            name,
            value
          })}`
          );
          continue;
        }
        const replaceValue = encodeURIComponent(
          style === "label" ? `.${value}` : value
        );
        url = url.replace(match, replaceValue);
      }
    }
    return url;
  };
  var getUrl = ({
    baseUrl,
    path,
    query,
    querySerializer,
    url: _url
  }) => {
    const pathUrl = _url.startsWith("/") ? _url : `/${_url}`;
    let url = (baseUrl ?? "") + pathUrl;
    if (path) {
      url = defaultPathSerializer({ path, url });
    }
    let search = query ? querySerializer(query) : "";
    if (search.startsWith("?")) {
      search = search.substring(1);
    }
    if (search) {
      url += `?${search}`;
    }
    return url;
  };
  function getValidRequestBody(options) {
    const hasBody = options.body !== void 0;
    const isSerializedBody = hasBody && options.bodySerializer;
    if (isSerializedBody) {
      if ("serializedBody" in options) {
        const hasSerializedBody = options.serializedBody !== void 0 && options.serializedBody !== "";
        return hasSerializedBody ? options.serializedBody : null;
      }
      return options.body !== "" ? options.body : null;
    }
    if (hasBody) {
      return options.body;
    }
    return void 0;
  }

  // src/generated/core/auth.gen.ts
  var getAuthToken = async (auth, callback) => {
    const token = typeof callback === "function" ? await callback(auth) : callback;
    if (!token) {
      return;
    }
    if (auth.scheme === "bearer") {
      return `Bearer ${token}`;
    }
    if (auth.scheme === "basic") {
      return `Basic ${btoa(token)}`;
    }
    return token;
  };

  // src/generated/client/utils.gen.ts
  var createQuerySerializer = ({
    parameters = {},
    ...args
  } = {}) => {
    const querySerializer = (queryParams) => {
      const search = [];
      if (queryParams && typeof queryParams === "object") {
        for (const name in queryParams) {
          const value = queryParams[name];
          if (value === void 0 || value === null) {
            continue;
          }
          const options = parameters[name] || args;
          if (Array.isArray(value)) {
            const serializedArray = serializeArrayParam({
              allowReserved: options.allowReserved,
              explode: true,
              name,
              style: "form",
              value,
              ...options.array
            });
            if (serializedArray) search.push(serializedArray);
          } else if (typeof value === "object") {
            const serializedObject = serializeObjectParam({
              allowReserved: options.allowReserved,
              explode: true,
              name,
              style: "deepObject",
              value,
              ...options.object
            });
            if (serializedObject) search.push(serializedObject);
          } else {
            const serializedPrimitive = serializePrimitiveParam({
              allowReserved: options.allowReserved,
              name,
              value
            });
            if (serializedPrimitive) search.push(serializedPrimitive);
          }
        }
      }
      return search.join("&");
    };
    return querySerializer;
  };
  var getParseAs = (contentType) => {
    if (!contentType) {
      return "stream";
    }
    const cleanContent = contentType.split(";")[0]?.trim();
    if (!cleanContent) {
      return;
    }
    if (cleanContent.startsWith("application/json") || cleanContent.endsWith("+json")) {
      return "json";
    }
    if (cleanContent === "multipart/form-data") {
      return "formData";
    }
    if (["application/", "audio/", "image/", "video/"].some(
      (type) => cleanContent.startsWith(type)
    )) {
      return "blob";
    }
    if (cleanContent.startsWith("text/")) {
      return "text";
    }
    return;
  };
  var checkForExistence = (options, name) => {
    if (!name) {
      return false;
    }
    if (options.headers.has(name) || options.query?.[name] || options.headers.get("Cookie")?.includes(`${name}=`)) {
      return true;
    }
    return false;
  };
  var setAuthParams = async ({
    security,
    ...options
  }) => {
    for (const auth of security) {
      if (checkForExistence(options, auth.name)) {
        continue;
      }
      const token = await getAuthToken(auth, options.auth);
      if (!token) {
        continue;
      }
      const name = auth.name ?? "Authorization";
      switch (auth.in) {
        case "query":
          if (!options.query) {
            options.query = {};
          }
          options.query[name] = token;
          break;
        case "cookie":
          options.headers.append("Cookie", `${name}=${token}`);
          break;
        case "header":
        default:
          options.headers.set(name, token);
          break;
      }
    }
  };
  var buildUrl = (options) => getUrl({
    baseUrl: options.baseUrl,
    path: options.path,
    query: options.query,
    querySerializer: typeof options.querySerializer === "function" ? options.querySerializer : createQuerySerializer(options.querySerializer),
    url: options.url
  });
  var mergeConfigs = (a, b) => {
    const config = { ...a, ...b };
    if (config.baseUrl?.endsWith("/")) {
      config.baseUrl = config.baseUrl.substring(0, config.baseUrl.length - 1);
    }
    config.headers = mergeHeaders(a.headers, b.headers);
    return config;
  };
  var headersEntries = (headers) => {
    const entries = [];
    headers.forEach((value, key) => {
      entries.push([key, value]);
    });
    return entries;
  };
  var mergeHeaders = (...headers) => {
    const mergedHeaders = new Headers();
    for (const header of headers) {
      if (!header) {
        continue;
      }
      const iterator = header instanceof Headers ? headersEntries(header) : Object.entries(header);
      for (const [key, value] of iterator) {
        if (value === null) {
          mergedHeaders.delete(key);
        } else if (Array.isArray(value)) {
          for (const v of value) {
            mergedHeaders.append(key, v);
          }
        } else if (value !== void 0) {
          mergedHeaders.set(
            key,
            typeof value === "object" ? JSON.stringify(value) : value
          );
        }
      }
    }
    return mergedHeaders;
  };
  var Interceptors = class {
    constructor() {
      __publicField(this, "fns", []);
    }
    clear() {
      this.fns = [];
    }
    eject(id) {
      const index = this.getInterceptorIndex(id);
      if (this.fns[index]) {
        this.fns[index] = null;
      }
    }
    exists(id) {
      const index = this.getInterceptorIndex(id);
      return Boolean(this.fns[index]);
    }
    getInterceptorIndex(id) {
      if (typeof id === "number") {
        return this.fns[id] ? id : -1;
      }
      return this.fns.indexOf(id);
    }
    update(id, fn) {
      const index = this.getInterceptorIndex(id);
      if (this.fns[index]) {
        this.fns[index] = fn;
        return id;
      }
      return false;
    }
    use(fn) {
      this.fns.push(fn);
      return this.fns.length - 1;
    }
  };
  var createInterceptors = () => ({
    error: new Interceptors(),
    request: new Interceptors(),
    response: new Interceptors()
  });
  var defaultQuerySerializer = createQuerySerializer({
    allowReserved: false,
    array: {
      explode: true,
      style: "form"
    },
    object: {
      explode: true,
      style: "deepObject"
    }
  });
  var defaultHeaders = {
    "Content-Type": "application/json"
  };
  var createConfig = (override = {}) => ({
    ...jsonBodySerializer,
    headers: defaultHeaders,
    parseAs: "auto",
    querySerializer: defaultQuerySerializer,
    ...override
  });

  // src/generated/client/client.gen.ts
  var createClient = (config = {}) => {
    let _config = mergeConfigs(createConfig(), config);
    const getConfig = () => ({ ..._config });
    const setConfig = (config2) => {
      _config = mergeConfigs(_config, config2);
      return getConfig();
    };
    const interceptors = createInterceptors();
    const beforeRequest = async (options) => {
      const opts = {
        ..._config,
        ...options,
        fetch: options.fetch ?? _config.fetch ?? globalThis.fetch,
        headers: mergeHeaders(_config.headers, options.headers),
        serializedBody: void 0
      };
      if (opts.security) {
        await setAuthParams({
          ...opts,
          security: opts.security
        });
      }
      if (opts.requestValidator) {
        await opts.requestValidator(opts);
      }
      if (opts.body !== void 0 && opts.bodySerializer) {
        opts.serializedBody = opts.bodySerializer(opts.body);
      }
      if (opts.body === void 0 || opts.serializedBody === "") {
        opts.headers.delete("Content-Type");
      }
      const url = buildUrl(opts);
      return { opts, url };
    };
    const request = async (options) => {
      const { opts, url } = await beforeRequest(options);
      const requestInit = {
        redirect: "follow",
        ...opts,
        body: getValidRequestBody(opts)
      };
      let request2 = new Request(url, requestInit);
      for (const fn of interceptors.request.fns) {
        if (fn) {
          request2 = await fn(request2, opts);
        }
      }
      const _fetch = opts.fetch;
      let response;
      try {
        response = await _fetch(request2);
      } catch (error2) {
        let finalError2 = error2;
        for (const fn of interceptors.error.fns) {
          if (fn) {
            finalError2 = await fn(
              error2,
              void 0,
              request2,
              opts
            );
          }
        }
        finalError2 = finalError2 || {};
        if (opts.throwOnError) {
          throw finalError2;
        }
        return opts.responseStyle === "data" ? void 0 : {
          error: finalError2,
          request: request2,
          response: void 0
        };
      }
      for (const fn of interceptors.response.fns) {
        if (fn) {
          response = await fn(response, request2, opts);
        }
      }
      const result = {
        request: request2,
        response
      };
      if (response.ok) {
        const parseAs = (opts.parseAs === "auto" ? getParseAs(response.headers.get("Content-Type")) : opts.parseAs) ?? "json";
        if (response.status === 204 || response.headers.get("Content-Length") === "0") {
          let emptyData;
          switch (parseAs) {
            case "arrayBuffer":
            case "blob":
            case "text":
              emptyData = await response[parseAs]();
              break;
            case "formData":
              emptyData = new FormData();
              break;
            case "stream":
              emptyData = response.body;
              break;
            case "json":
            default:
              emptyData = {};
              break;
          }
          return opts.responseStyle === "data" ? emptyData : {
            data: emptyData,
            ...result
          };
        }
        let data;
        switch (parseAs) {
          case "arrayBuffer":
          case "blob":
          case "formData":
          case "json":
          case "text":
            data = await response[parseAs]();
            break;
          case "stream":
            return opts.responseStyle === "data" ? response.body : {
              data: response.body,
              ...result
            };
        }
        if (parseAs === "json") {
          if (opts.responseValidator) {
            await opts.responseValidator(data);
          }
          if (opts.responseTransformer) {
            data = await opts.responseTransformer(data);
          }
        }
        return opts.responseStyle === "data" ? data : {
          data,
          ...result
        };
      }
      const textError = await response.text();
      let jsonError;
      try {
        jsonError = JSON.parse(textError);
      } catch {
      }
      const error = jsonError ?? textError;
      let finalError = error;
      for (const fn of interceptors.error.fns) {
        if (fn) {
          finalError = await fn(error, response, request2, opts);
        }
      }
      finalError = finalError || {};
      if (opts.throwOnError) {
        throw finalError;
      }
      return opts.responseStyle === "data" ? void 0 : {
        error: finalError,
        ...result
      };
    };
    const makeMethodFn = (method) => (options) => request({ ...options, method });
    const makeSseFn = (method) => async (options) => {
      const { opts, url } = await beforeRequest(options);
      return createSseClient({
        ...opts,
        body: opts.body,
        headers: opts.headers,
        method,
        onRequest: async (url2, init) => {
          let request2 = new Request(url2, init);
          for (const fn of interceptors.request.fns) {
            if (fn) {
              request2 = await fn(request2, opts);
            }
          }
          return request2;
        },
        url
      });
    };
    return {
      buildUrl,
      connect: makeMethodFn("CONNECT"),
      delete: makeMethodFn("DELETE"),
      get: makeMethodFn("GET"),
      getConfig,
      head: makeMethodFn("HEAD"),
      interceptors,
      options: makeMethodFn("OPTIONS"),
      patch: makeMethodFn("PATCH"),
      post: makeMethodFn("POST"),
      put: makeMethodFn("PUT"),
      request,
      setConfig,
      sse: {
        connect: makeSseFn("CONNECT"),
        delete: makeSseFn("DELETE"),
        get: makeSseFn("GET"),
        head: makeSseFn("HEAD"),
        options: makeSseFn("OPTIONS"),
        patch: makeSseFn("PATCH"),
        post: makeSseFn("POST"),
        put: makeSseFn("PUT"),
        trace: makeSseFn("TRACE")
      },
      trace: makeMethodFn("TRACE")
    };
  };

  // src/generated/client.gen.ts
  var client = createClient(createConfig());

  // src/generated/sdk.gen.ts
  var sdkControllerBatchV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/batch",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerUpsertGdprConsentV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/consent/gdpr",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerCheckGdprConsentV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/consent/gdpr/check",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerRecordCrashV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/crashes",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerResolveDeepLinkV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/deep-links/resolve",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerCreateDynamicLinkV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/dynamic-links",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerRecordEventV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/events",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerRecordNotificationV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/notifications",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerOpenV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/open",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerValidateReceiptV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/revenue/receipts/validate",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerRecordSessionV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/sessions",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });
  var sdkControllerSetUserV1 = (options) => (options.client ?? client).post({
    url: "/api/sdk/v1/users",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers
    }
  });

  // src/internal/api.ts
  function createAttriaxSdkClient(config, baseFetch) {
    const client2 = createClient({
      baseUrl: config.sdkClientBaseUrl,
      keepalive: true,
      fetch: createTimedFetch(config.requestTimeoutMs, baseFetch)
    });
    client2.interceptors.error.use((error, response) => normalizeSdkError(error, response));
    return client2;
  }
  function unwrapSuccessEnvelope(payload) {
    if (hasUnexpectedContentType(payload.response)) {
      throw buildUnexpectedResponseError(
        payload.response,
        buildUnexpectedContentTypeMessage(payload.response)
      );
    }
    if (!payload.data || typeof payload.data !== "object" || !("data" in payload.data)) {
      throw new AttriaxApiError(
        "Invalid Attriax API response envelope.",
        payload.response.status,
        false,
        true
      );
    }
    return payload.data.data;
  }
  function normalizeTransportError(error) {
    if (error instanceof AttriaxApiError) {
      return error;
    }
    if (error instanceof Error && error.name === "AbortError") {
      return new AttriaxApiError("Attriax request timed out.", void 0, true, false);
    }
    if (error instanceof Error) {
      return new AttriaxApiError(
        buildTransportErrorMessage(error),
        void 0,
        true,
        false
      );
    }
    return new AttriaxApiError("Unknown Attriax transport error.", void 0, true, false);
  }
  function parseAppOpenResult(payload) {
    const json = ensureRecord(payload);
    const installState = parseInstallState(json.installState);
    const originalInstallReferrer = parseOptionalInstallReferrerDetails(
      json.originalInstallReferrer
    );
    const reinstallReferrer = parseOptionalInstallReferrerDetails(
      json.reinstallReferrer
    );
    const installReferrer = parseOptionalInstallReferrerDetails(json.installReferrer) ?? selectCurrentInstallReferrer({
      installState,
      originalInstallReferrer,
      reinstallReferrer
    });
    return {
      userId: ensureString(
        json.userId ?? json.externalUserId,
        "userId"
      ),
      isNewUser: Boolean(json.isNewUser),
      isFirstLaunch: Boolean(json.isFirstLaunch),
      installState,
      requestVersion: optionalString(json.requestVersion),
      acceptedAt: optionalDate(json.acceptedAt),
      deepLink: json.deepLink ? parseDeepLink(json.deepLink) : void 0,
      deepLinkClickedAt: optionalDate(json.deepLinkClickedAt),
      deepLinkConsumedAt: optionalDate(json.deepLinkConsumedAt),
      originalInstallReferrer,
      reinstallReferrer,
      installReferrer
    };
  }
  function parseDeepLinkResolutionResult(payload) {
    const json = ensureRecord(payload);
    return {
      matched: Boolean(json.matched),
      status: parseResolutionStatus(json.status),
      isFirstLaunch: Boolean(json.isFirstLaunch),
      reason: optionalString(json.reason),
      deepLink: json.deepLink ? parseDeepLink(json.deepLink) : void 0,
      browserAction: parseOptionalBrowserAction(json.browserAction),
      requestVersion: optionalString(json.requestVersion),
      acceptedAt: optionalDate(json.acceptedAt),
      consumedAt: optionalDate(json.consumedAt)
    };
  }
  function parseDynamicLinkCreateResult(payload) {
    const json = ensureRecord(payload);
    const linkJson = ensureRecord(json.link);
    return {
      requestVersion: optionalString(json.requestVersion),
      acceptedAt: optionalDate(json.acceptedAt),
      link: {
        id: ensureString(linkJson.id, "link.id"),
        path: ensureString(linkJson.path, "link.path"),
        shortUrl: ensureString(linkJson.shortUrl, "link.shortUrl"),
        name: optionalString(linkJson.name),
        destinationUrl: optionalString(linkJson.destinationUrl),
        group: optionalString(linkJson.group),
        prefix: optionalString(linkJson.prefix),
        data: optionalJsonObject(linkJson.data),
        previewTitle: optionalString(linkJson.previewTitle),
        previewDescription: optionalString(linkJson.previewDescription),
        iosRedirect: optionalBoolean(linkJson.iosRedirect),
        androidRedirect: optionalBoolean(linkJson.androidRedirect),
        utmSource: optionalString(linkJson.utmSource),
        utmMedium: optionalString(linkJson.utmMedium),
        utmCampaign: optionalString(linkJson.utmCampaign),
        utmTerm: optionalString(linkJson.utmTerm),
        utmContent: optionalString(linkJson.utmContent),
        createdAt: optionalDate(linkJson.createdAt)
      }
    };
  }
  function parseRevenueReceiptValidationResult(payload) {
    const json = ensureRecord(payload);
    return {
      validationId: ensureString(json.validationId, "validationId"),
      status: parseRevenueReceiptValidationStatus(json.status),
      requestVersion: optionalString(json.requestVersion),
      acceptedAt: optionalDate(json.acceptedAt),
      provider: optionalString(json.provider),
      environment: optionalString(json.environment),
      transactionId: optionalString(json.transactionId),
      originalTransactionId: optionalString(json.originalTransactionId),
      productId: optionalString(json.productId),
      failureReason: optionalString(json.failureReason),
      expiresAt: optionalDate(json.expiresAt),
      providerResult: optionalJsonObject(json.providerResult),
      publicReceipt: optionalJsonObject(json.publicReceipt) ?? {}
    };
  }
  function parseDeepLink(payload) {
    const json = ensureRecord(payload);
    return {
      path: ensureString(json.path, "path"),
      data: optionalJsonObject(json.data),
      uri: optionalUrl(json.uri),
      utm: parseOptionalUtmParameters(json.utm)
    };
  }
  function parseOptionalUtmParameters(payload) {
    if (payload == null) {
      return void 0;
    }
    const json = ensureRecord(payload);
    return {
      source: optionalString(json.source),
      medium: optionalString(json.medium),
      campaign: optionalString(json.campaign),
      term: optionalString(json.term),
      content: optionalString(json.content)
    };
  }
  function parseOptionalBrowserAction(payload) {
    if (payload == null) {
      return void 0;
    }
    const json = ensureRecord(payload);
    const url = optionalUrl(json.url);
    if (!url) {
      return void 0;
    }
    return {
      url,
      openMode: parseResolvedUrlOpenMode(json.openMode)
    };
  }
  function parseOptionalInstallReferrerDetails(payload) {
    return payload == null ? void 0 : parseInstallReferrerDetails(payload);
  }
  function parseInstallReferrerDetails(payload) {
    const json = ensureRecord(payload);
    const deepLinkUrl = optionalString(json.deepLinkUrl);
    const deepLinkUri = optionalUrl(json.deepLinkUri) ?? optionalUrl(deepLinkUrl);
    return {
      rawPlatformInstallReferrer: optionalString(json.rawPlatformInstallReferrer),
      source: optionalString(json.source),
      medium: optionalString(json.medium),
      campaign: optionalString(json.campaign),
      term: optionalString(json.term),
      content: optionalString(json.content),
      adNetwork: optionalString(json.adNetwork),
      adClickId: optionalString(json.adClickId),
      attributionType: parseAttributionType(json.attributionType),
      deepLinkUrl,
      deepLinkUri,
      deepLinkData: optionalJsonObject(json.deepLinkData),
      registeredAt: optionalDate(json.registeredAt),
      installBeginTimestampSeconds: optionalNumber(json.installBeginTimestampSeconds),
      referrerClickTimestampSeconds: optionalNumber(json.referrerClickTimestampSeconds),
      googlePlayInstantParam: optionalBoolean(json.googlePlayInstantParam),
      precision: typeof json.precision === "number" ? json.precision : 0
    };
  }
  function optionalBoolean(value) {
    return typeof value === "boolean" ? value : void 0;
  }
  function optionalNumber(value) {
    return typeof value === "number" && Number.isFinite(value) ? value : void 0;
  }
  function optionalUrl(value) {
    const candidate = optionalString(value);
    if (!candidate) {
      return void 0;
    }
    try {
      return new URL(candidate);
    } catch {
      return void 0;
    }
  }
  function parseAttributionType(value) {
    switch (value) {
      case "referrer" /* Referrer */:
        return "referrer" /* Referrer */;
      case "fingerprint" /* Fingerprint */:
        return "fingerprint" /* Fingerprint */;
      case "external" /* External */:
        return "external" /* External */;
      case "organic" /* Organic */:
      default:
        return "organic" /* Organic */;
    }
  }
  function parseInstallState(value) {
    switch (value) {
      case "new_install" /* NewInstall */:
        return "new_install" /* NewInstall */;
      case "reinstall" /* Reinstall */:
        return "reinstall" /* Reinstall */;
      case "app_data_clear" /* AppDataClear */:
        return "app_data_clear" /* AppDataClear */;
      case "existing" /* Existing */:
      default:
        return "existing" /* Existing */;
    }
  }
  function selectCurrentInstallReferrer({
    installState,
    originalInstallReferrer,
    reinstallReferrer
  }) {
    switch (installState) {
      case "new_install" /* NewInstall */:
      case "app_data_clear" /* AppDataClear */:
        return originalInstallReferrer ?? reinstallReferrer;
      case "reinstall" /* Reinstall */:
      case "existing" /* Existing */:
      default:
        return reinstallReferrer ?? originalInstallReferrer;
    }
  }
  function parseResolutionStatus(value) {
    switch (value) {
      case "matched" /* Matched */:
        return "matched" /* Matched */;
      case "unmatched" /* Unmatched */:
        return "unmatched" /* Unmatched */;
      case "invalid" /* Invalid */:
      default:
        return "invalid" /* Invalid */;
    }
  }
  function parseResolvedUrlOpenMode(value) {
    switch (value) {
      case "in_app":
      case "inApp":
        return "inApp" /* InApp */;
      case "external":
        return "external" /* External */;
      case "unknown":
      default:
        return "unknown" /* Unknown */;
    }
  }
  function parseRevenueReceiptValidationStatus(value) {
    switch (value) {
      case "verified" /* Verified */:
        return "verified" /* Verified */;
      case "pending" /* Pending */:
        return "pending" /* Pending */;
      case "unconfigured" /* Unconfigured */:
        return "unconfigured" /* Unconfigured */;
      case "provider_error" /* ProviderError */:
        return "provider_error" /* ProviderError */;
      case "passthrough" /* Passthrough */:
        return "passthrough" /* Passthrough */;
      case "rejected" /* Rejected */:
      default:
        return "rejected" /* Rejected */;
    }
  }
  function isRetriableHttpStatus(statusCode) {
    return statusCode === 408 || statusCode === 425 || statusCode === 429 || statusCode >= 500;
  }
  function buildUnexpectedResponseError(response, message) {
    const retriable = isRetriableHttpStatus(response.status);
    return new AttriaxApiError(message, response.status, retriable, !retriable);
  }
  function normalizeApiFailure(payload, statusCode, response) {
    const failure = payload && typeof payload === "object" ? payload : null;
    const message = typeof failure?.message === "string" && failure.message.length > 0 ? failure.message : typeof payload === "string" && payload.length > 0 ? payload : `Attriax API request failed with status ${statusCode}.`;
    if (isRetriableHttpStatus(statusCode)) {
      return new AttriaxApiError(
        message,
        statusCode,
        true,
        false,
        parseRetryAfterMs(response)
      );
    }
    return new AttriaxApiError(message, statusCode, false, true);
  }
  function parseRetryAfterMs(response) {
    const headerValue = response?.headers.get("retry-after")?.trim();
    if (!headerValue) {
      return void 0;
    }
    const seconds = Number(headerValue);
    if (Number.isInteger(seconds)) {
      return seconds > 0 ? seconds * 1e3 : void 0;
    }
    const dateMs = Date.parse(headerValue);
    if (Number.isNaN(dateMs)) {
      return void 0;
    }
    const deltaMs = dateMs - Date.now();
    return deltaMs > 0 ? deltaMs : void 0;
  }
  function createTimedFetch(timeoutMs, baseFetch) {
    return async (input, init) => {
      const underlyingFetch = baseFetch ?? globalThis.fetch;
      if (typeof underlyingFetch !== "function") {
        throw new AttriaxApiError(
          "Fetch API is not available in this environment.",
          void 0,
          true,
          false
        );
      }
      const controller = typeof AbortController !== "undefined" ? new AbortController() : null;
      const upstreamSignal = init?.signal ?? (input instanceof Request ? input.signal : void 0);
      let removeAbortListener;
      if (controller && upstreamSignal) {
        if (upstreamSignal.aborted) {
          controller.abort();
        } else {
          const abortHandler = () => controller.abort();
          upstreamSignal.addEventListener("abort", abortHandler, { once: true });
          removeAbortListener = () => upstreamSignal.removeEventListener("abort", abortHandler);
        }
      }
      const timer = controller ? globalThis.setTimeout(() => controller.abort(), timeoutMs) : null;
      try {
        return await underlyingFetch(input, {
          ...init,
          signal: controller?.signal ?? init?.signal
        });
      } catch (error) {
        if (controller && !controller.signal.aborted && isAbortSignalCompatibilityError(error)) {
          return await underlyingFetch(input, init);
        }
        throw normalizeTransportError(error);
      } finally {
        removeAbortListener?.();
        if (timer !== null) {
          globalThis.clearTimeout(timer);
        }
      }
    };
  }
  function isAbortSignalCompatibilityError(error) {
    return error instanceof TypeError && /signal.*instance of AbortSignal/i.test(error.message);
  }
  function normalizeSdkError(error, response) {
    if (!response) {
      return normalizeTransportError(error);
    }
    return normalizeApiFailure(error, response.status, response);
  }
  function buildUnexpectedContentTypeMessage(response) {
    const contentType = response.headers.get("content-type");
    return `Attriax API returned an unexpected content type${contentType ? ` (${contentType})` : ""}.`;
  }
  function hasUnexpectedContentType(response) {
    const contentType = response.headers.get("content-type");
    return !contentType || !contentType.toLowerCase().includes("json");
  }
  function buildTransportErrorMessage(error) {
    const normalizedMessage = error.message.trim();
    if (isLikelyBrowserFetchFailure(error)) {
      return normalizedMessage.length > 0 ? `Attriax request failed in the browser before a response was received: ${normalizedMessage}. This usually means the API is unreachable, HTTPS or mixed-content rules blocked the request, or the server rejected the browser preflight/CORS request.` : "Attriax request failed in the browser before a response was received. This usually means the API is unreachable, HTTPS or mixed-content rules blocked the request, or the server rejected the browser preflight/CORS request.";
    }
    return normalizedMessage.length > 0 ? normalizedMessage : "Unknown Attriax transport error.";
  }
  function isLikelyBrowserFetchFailure(error) {
    const message = error.message.toLowerCase();
    return error.name === "TypeError" && (message.includes("failed to fetch") || message.includes("load failed") || message.includes("networkerror"));
  }

  // src/internal/gdpr-region.ts
  var EXPLICIT_GDPR_TIMEZONES = /* @__PURE__ */ new Set([
    "Arctic/Longyearbyen",
    "Asia/Famagusta",
    "Asia/Nicosia",
    "Atlantic/Azores",
    "Atlantic/Canary",
    "Atlantic/Faroe",
    "Atlantic/Madeira",
    "Atlantic/Reykjavik",
    // EU outermost regions / overseas territories where GDPR applies but the
    // timezone is not under `Europe/`. Without these they would fall through to
    // the `NotRequired` default below — the under-protective direction.
    "America/Cayenne",
    // French Guiana
    "America/Guadeloupe",
    // Guadeloupe
    "America/Marigot",
    // Saint-Martin
    "America/Martinique",
    // Martinique
    "America/St_Barthelemy",
    // Saint-Barthélemy
    "Indian/Mayotte",
    // Mayotte
    "Indian/Reunion"
    // Réunion
  ]);
  var EXCLUDED_EUROPE_TIMEZONES = /* @__PURE__ */ new Set([
    "Europe/Andorra",
    "Europe/Belgrade",
    "Europe/Chisinau",
    "Europe/Istanbul",
    "Europe/Kaliningrad",
    "Europe/Kiev",
    "Europe/Kirov",
    "Europe/Kyiv",
    "Europe/Minsk",
    "Europe/Moscow",
    "Europe/Podgorica",
    "Europe/Pristina",
    "Europe/Samara",
    "Europe/Sarajevo",
    "Europe/Simferopol",
    "Europe/Skopje",
    "Europe/Tirane",
    "Europe/Uzhgorod",
    "Europe/Volgograd",
    "Europe/Zaporozhye"
  ]);
  var TIMEZONE_ALIASES = /* @__PURE__ */ new Map([
    ["Belarus Standard Time", "Europe/Minsk"],
    ["Central Europe Standard Time", "Europe/Budapest"],
    ["Central European Standard Time", "Europe/Warsaw"],
    ["E. Europe Standard Time", "Europe/Chisinau"],
    ["FLE Standard Time", "Europe/Helsinki"],
    ["GMT Standard Time", "Europe/London"],
    ["GTB Standard Time", "Europe/Bucharest"],
    ["Greenwich Standard Time", "Atlantic/Reykjavik"],
    ["Kaliningrad Standard Time", "Europe/Kaliningrad"],
    ["Romance Standard Time", "Europe/Paris"],
    ["Russia Time Zone 3", "Europe/Samara"],
    ["Russian Standard Time", "Europe/Moscow"],
    ["Turkey Standard Time", "Europe/Istanbul"],
    ["Volgograd Standard Time", "Europe/Volgograd"],
    ["W. Europe Standard Time", "Europe/Berlin"]
  ]);
  function resolveGdprStateForTimezone(timezone) {
    const normalized = canonicalizeTimezone(timezone);
    if (!normalized || !normalized.includes("/")) {
      return null;
    }
    if (EXPLICIT_GDPR_TIMEZONES.has(normalized)) {
      return "pending" /* Pending */;
    }
    if (normalized.startsWith("Europe/")) {
      return EXCLUDED_EUROPE_TIMEZONES.has(normalized) ? "not_required" /* NotRequired */ : "pending" /* Pending */;
    }
    return "not_required" /* NotRequired */;
  }
  function canonicalizeTimezone(timezone) {
    const normalized = timezone?.trim();
    if (!normalized) {
      return void 0;
    }
    return TIMEZONE_ALIASES.get(normalized) ?? normalized;
  }

  // src/internal/consent-manager.ts
  var AttriaxConsentManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "stateValue", "unknown" /* Unknown */);
      __publicField(this, "valuesValue", null);
      __publicField(this, "anonymousTrackingEnabledValue");
      __publicField(this, "countryCodeValue", null);
      __publicField(this, "regionSourceValue", null);
      __publicField(this, "checkedAtValue", null);
      __publicField(this, "pendingSync", false);
      __publicField(this, "didRestore", false);
      /**
       * Monotonic counter bumped on every local consent decision. Network echoes
       * capture the generation before their await; a mismatch on return means a
       * newer local decision landed mid-flight and the (stale) echo must be
       * discarded instead of downgrading the newer values.
       */
      __publicField(this, "consentGeneration", 0);
      __publicField(this, "needsConsentPromise", null);
      __publicField(this, "pendingSyncPromise", null);
      this.anonymousTrackingEnabledValue = options.config.anonymousTracking;
    }
    get gdprConsentState() {
      return this.stateValue;
    }
    get gdprConsentValues() {
      return this.valuesValue;
    }
    get anonymousTrackingEnabled() {
      return this.anonymousTrackingEnabledValue;
    }
    get isWaitingForGdprConsent() {
      return this.stateValue === "pending" /* Pending */ || this.stateValue === "unknown" /* Unknown */;
    }
    get shouldDeferNetworkDispatch() {
      return this.options.config.gdprEnabled && this.isWaitingForGdprConsent && !this.anonymousTrackingEnabledValue;
    }
    get allowsAnalyticsTracking() {
      return this.allowsCategory((values) => values.analytics);
    }
    get allowsAttributionTracking() {
      return this.allowsCategory((values) => values.attribution);
    }
    get allowsAdEventsTracking() {
      return this.allowsCategory((values) => values.adEvents);
    }
    get canCaptureAnalytics() {
      return this.trackingDecisionFor("analytics").capture;
    }
    get canCaptureAttribution() {
      return this.trackingDecisionFor("attribution").capture;
    }
    get canCaptureAdEvents() {
      return this.trackingDecisionFor("adEvents").capture;
    }
    trackingDecisionFor(signal) {
      if (!this.options.config.gdprEnabled) {
        return trackingDecision({
          capture: true,
          identityMode: "identified",
          deferNetwork: false
        });
      }
      if (this.isWaitingForGdprConsent) {
        return trackingDecision({
          capture: canCaptureWhileWaiting(signal),
          identityMode: "anonymous",
          deferNetwork: !this.anonymousTrackingEnabledValue
        });
      }
      if (this.stateValue === "not_required" /* NotRequired */) {
        return trackingDecision({
          capture: true,
          identityMode: "identified",
          deferNetwork: false
        });
      }
      const values = this.valuesValue;
      if (this.stateValue !== "granted" /* Granted */ || !values) {
        return trackingDecision({
          capture: false,
          identityMode: "withheld",
          deferNetwork: false
        });
      }
      if (isSignalGranted(signal, values)) {
        return trackingDecision({
          capture: true,
          identityMode: "identified",
          deferNetwork: false
        });
      }
      if (this.anonymousTrackingEnabledValue && isAnonymousCapableSignal(signal)) {
        return trackingDecision({
          capture: true,
          identityMode: "anonymous",
          deferNetwork: false
        });
      }
      return trackingDecision({
        capture: false,
        identityMode: "withheld",
        deferNetwork: false
      });
    }
    init() {
      this.restore();
    }
    clearMemory() {
      this.stateValue = "unknown" /* Unknown */;
      this.valuesValue = null;
      this.countryCodeValue = null;
      this.regionSourceValue = null;
      this.checkedAtValue = null;
      this.pendingSync = false;
      this.didRestore = false;
      this.consentGeneration += 1;
      this.needsConsentPromise = null;
      this.pendingSyncPromise = null;
    }
    async flushPendingSync() {
      this.restore();
      return this.flushPendingSyncInternal();
    }
    async needsConsent(options = {}) {
      const localOnly = options.localOnly ?? false;
      this.restore();
      const canUseCachedState = (this.stateValue === "granted" /* Granted */ || this.stateValue === "not_required" /* NotRequired */) && (localOnly || !this.shouldRefreshRemoteDecision());
      if (canUseCachedState) {
        if (!localOnly) {
          void this.flushPendingSyncInternal();
        }
        return this.isWaitingForGdprConsent;
      }
      const inFlight = this.needsConsentPromise;
      if (inFlight) {
        return inFlight;
      }
      const resolution = this.resolveNeedsConsent({ localOnly });
      this.needsConsentPromise = resolution;
      return resolution.finally(() => {
        if (this.needsConsentPromise === resolution) {
          this.needsConsentPromise = null;
        }
      });
    }
    setConsent(options) {
      this.applyState({
        state: "granted" /* Granted */,
        values: {
          analytics: options.analytics,
          attribution: options.attribution,
          adEvents: options.adEvents
        },
        checkedAt: /* @__PURE__ */ new Date(),
        countryCode: this.countryCodeValue,
        regionSource: "manual",
        pendingSync: true
      });
      this.consentGeneration += 1;
      void this.persistAndFlush();
    }
    setNotRequired() {
      this.applyState({
        state: "not_required" /* NotRequired */,
        values: null,
        checkedAt: /* @__PURE__ */ new Date(),
        countryCode: this.countryCodeValue,
        regionSource: "manual",
        pendingSync: true
      });
      this.consentGeneration += 1;
      void this.persistAndFlush();
    }
    reset() {
      this.applyState({
        state: "unknown" /* Unknown */,
        values: null,
        checkedAt: /* @__PURE__ */ new Date(),
        countryCode: null,
        regionSource: null,
        pendingSync: true
      });
      this.consentGeneration += 1;
      void this.persistAndFlush();
    }
    setAnonymousTrackingEnabled(options) {
      if (this.anonymousTrackingEnabledValue === options.enabled) {
        return;
      }
      this.anonymousTrackingEnabledValue = options.enabled;
      this.options.onStateChanged?.();
    }
    allowsCategory(selector) {
      if (!this.options.config.gdprEnabled) {
        return true;
      }
      switch (this.stateValue) {
        case "not_required" /* NotRequired */:
          return true;
        case "granted" /* Granted */:
          return this.valuesValue != null && selector(this.valuesValue);
        case "pending" /* Pending */:
        case "unknown" /* Unknown */:
          return false;
      }
    }
    shouldRefreshRemoteDecision() {
      return this.regionSourceValue === "local_only_timezone" || this.regionSourceValue === "local_only_timezone_unresolved" || this.regionSourceValue === "auto_timezone" || this.regionSourceValue === "local_timezone_fallback";
    }
    async resolveNeedsConsent(options) {
      if (this.pendingSync && this.stateValue === "unknown" /* Unknown */) {
        await this.flushPendingSyncInternal();
        if (this.pendingSync && this.stateValue === "unknown" /* Unknown */) {
          return true;
        }
      }
      if (!options.localOnly && hasBrowserWindowContext()) {
        try {
          const generation = this.consentGeneration;
          const status = unwrapSuccessEnvelope(
            await sdkControllerCheckGdprConsentV1({
              client: this.options.sdkClient,
              body: {
                projectToken: this.options.config.projectToken,
                consentId: this.ensureConsentId()
              },
              throwOnError: true
            })
          );
          if (this.consentGeneration === generation) {
            this.applyRemoteStatus(status, { pendingSync: false });
          } else {
            void this.flushPendingSyncInternal();
          }
          return this.isWaitingForGdprConsent;
        } catch (error) {
          this.options.logger.warning(
            "Failed to check GDPR consent with Attriax. Falling back to local timezone detection.",
            { error: normalizeTransportError(error) }
          );
        }
      }
      const localState = resolveGdprStateForTimezone(this.options.contextManager.resolveTimezone());
      if (localState != null) {
        this.applyState({
          state: localState,
          values: null,
          checkedAt: /* @__PURE__ */ new Date(),
          countryCode: null,
          regionSource: options.localOnly ? "local_only_timezone" : "local_timezone_fallback",
          pendingSync: false
        });
        this.persistCurrentState();
        return this.isWaitingForGdprConsent;
      }
      this.applyState({
        state: "unknown" /* Unknown */,
        values: null,
        checkedAt: /* @__PURE__ */ new Date(),
        countryCode: null,
        regionSource: options.localOnly ? "local_only_timezone_unresolved" : "local_timezone_fallback",
        pendingSync: false
      });
      this.persistCurrentState();
      return true;
    }
    restore() {
      if (this.didRestore) {
        return;
      }
      const raw = this.options.store.readConsentState();
      if (!raw) {
        this.didRestore = true;
        return;
      }
      try {
        const stored = JSON.parse(raw);
        this.stateValue = this.stateFromStorage(stored.state);
        this.valuesValue = normalizeConsentValues(stored.values);
        this.countryCodeValue = normalizeString2(stored.countryCode)?.toUpperCase() ?? null;
        this.regionSourceValue = normalizeString2(stored.regionSource) ?? null;
        this.checkedAtValue = normalizeDate(stored.checkedAt);
        this.pendingSync = stored.pendingSync === true;
      } catch {
        this.options.store.clearConsentState();
      }
      this.didRestore = true;
    }
    persistCurrentState() {
      if (!this.pendingSync && this.stateValue === "unknown" /* Unknown */) {
        this.options.store.clearConsentState();
        return;
      }
      this.options.store.writeConsentState(
        JSON.stringify({
          state: this.stateToStorage(this.stateValue),
          values: this.valuesValue,
          checkedAt: this.checkedAtValue?.toISOString(),
          countryCode: this.countryCodeValue,
          regionSource: this.regionSourceValue,
          pendingSync: this.pendingSync
        })
      );
    }
    async persistAndFlush() {
      this.persistCurrentState();
      await this.flushPendingSyncInternal();
    }
    async flushPendingSyncInternal() {
      if (!this.pendingSync) {
        return;
      }
      if (!hasBrowserWindowContext()) {
        return;
      }
      const inFlight = this.pendingSyncPromise;
      if (inFlight) {
        return inFlight;
      }
      const sync = this.syncPendingState();
      this.pendingSyncPromise = sync;
      return sync.finally(() => {
        if (this.pendingSyncPromise === sync) {
          this.pendingSyncPromise = null;
        }
      });
    }
    async syncPendingState() {
      for (; ; ) {
        const generation = this.consentGeneration;
        try {
          const status = unwrapSuccessEnvelope(
            await sdkControllerUpsertGdprConsentV1({
              client: this.options.sdkClient,
              body: {
                projectToken: this.options.config.projectToken,
                consentId: this.ensureConsentId(),
                state: this.sdkStateFromPublic(this.stateValue),
                values: this.valuesValue == null ? void 0 : toSdkConsentValues(this.valuesValue),
                countryCode: this.countryCodeValue ?? void 0,
                regionSource: this.regionSourceValue ?? void 0,
                clientOccurredAt: this.checkedAtValue?.toISOString()
              },
              throwOnError: true
            })
          );
          if (this.consentGeneration !== generation) {
            continue;
          }
          this.applyRemoteStatus(status, { pendingSync: false });
          return;
        } catch (error) {
          this.options.logger.warning(
            "Failed to sync GDPR consent state to Attriax. The SDK will retry later.",
            { error: normalizeTransportError(error) }
          );
          this.pendingSync = true;
          this.persistCurrentState();
          return;
        }
      }
    }
    applyRemoteStatus(status, options) {
      let mappedState = this.publicStateFromSdk(status.state);
      const mappedValues = normalizeConsentValues(status.values);
      if (mappedState === "granted" /* Granted */ && mappedValues == null) {
        mappedState = "pending" /* Pending */;
      }
      this.applyState({
        state: mappedState,
        values: mappedValues,
        checkedAt: normalizeDate(status.checkedAt) ?? /* @__PURE__ */ new Date(),
        countryCode: normalizeString2(status.countryCode),
        regionSource: normalizeString2(status.regionSource),
        pendingSync: options.pendingSync
      });
      this.persistCurrentState();
    }
    applyState(options) {
      const nextCountryCode = normalizeString2(options.countryCode)?.toUpperCase() ?? null;
      const nextRegionSource = normalizeString2(options.regionSource) ?? null;
      const changed = options.state !== this.stateValue || !areConsentValuesEqual(options.values, this.valuesValue);
      this.stateValue = options.state;
      this.valuesValue = options.values;
      this.countryCodeValue = nextCountryCode;
      this.regionSourceValue = nextRegionSource;
      this.checkedAtValue = options.checkedAt;
      this.pendingSync = options.pendingSync;
      this.didRestore = true;
      if (changed) {
        this.options.onStateChanged?.();
      }
    }
    stateFromStorage(state) {
      switch (state) {
        case "not_required" /* NotRequired */:
          return "not_required" /* NotRequired */;
        case "pending" /* Pending */:
          return "pending" /* Pending */;
        case "granted" /* Granted */:
          return "granted" /* Granted */;
        default:
          return "unknown" /* Unknown */;
      }
    }
    stateToStorage(state) {
      return state;
    }
    sdkStateFromPublic(state) {
      switch (state) {
        case "not_required" /* NotRequired */:
          return "not_required";
        case "pending" /* Pending */:
          return "pending";
        case "granted" /* Granted */:
          return "granted";
        case "unknown" /* Unknown */:
          return "unknown";
      }
    }
    publicStateFromSdk(state) {
      switch (state) {
        case "not_required":
          return "not_required" /* NotRequired */;
        case "pending":
          return "pending" /* Pending */;
        case "granted":
          return "granted" /* Granted */;
        case "unknown":
          return "unknown" /* Unknown */;
      }
    }
    ensureConsentId() {
      const existing = this.options.store.readConsentId();
      if (existing) {
        return existing;
      }
      const generated = generateId();
      this.options.store.writeConsentId(generated);
      return generated;
    }
  };
  function normalizeConsentValues(values) {
    if (!values) {
      return null;
    }
    return {
      analytics: Boolean(values.analytics),
      attribution: Boolean(values.attribution),
      adEvents: Boolean(values.adEvents)
    };
  }
  function toSdkConsentValues(values) {
    return {
      analytics: values.analytics,
      attribution: values.attribution,
      adEvents: values.adEvents
    };
  }
  function areConsentValuesEqual(left, right) {
    if (left === right) {
      return true;
    }
    if (!left || !right) {
      return false;
    }
    return left.analytics === right.analytics && left.attribution === right.attribution && left.adEvents === right.adEvents;
  }
  function normalizeString2(value) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }
  function normalizeDate(value) {
    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? null : value;
    }
    if (!value) {
      return null;
    }
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }
  function trackingDecision(options) {
    return {
      ...options,
      attachDeviceIdentity: options.identityMode === "identified",
      sendNetworkDirectly: options.capture && !options.deferNetwork
    };
  }
  function canCaptureWhileWaiting(signal) {
    return isAnonymousCapableSignal(signal);
  }
  function isAnonymousCapableSignal(signal) {
    return ["analytics", "adEvents", "session", "deepLink"].includes(signal);
  }
  function isSignalGranted(signal, values) {
    switch (signal) {
      case "analytics":
        return values.analytics;
      case "adEvents":
        return values.adEvents;
      case "session":
        return values.analytics || values.adEvents;
      case "deepLink":
      case "attribution":
      case "uninstallTracking":
        return values.attribution;
    }
  }

  // src/internal/consent-store.ts
  var AttriaxConsentStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    readConsentState() {
      return this.options.storageManager.readValue("gdprConsent");
    }
    writeConsentState(value) {
      this.options.storageManager.writeValue("gdprConsent", value);
    }
    clearConsentState() {
      this.options.storageManager.removeValue("gdprConsent");
    }
    readConsentId() {
      return this.options.storageManager.readValue("gdprConsentId");
    }
    writeConsentId(value) {
      this.options.storageManager.writeValue("gdprConsentId", value);
    }
  };

  // src/internal/config.ts
  var defaultApiBaseUrl = "https://api.attriax.com";
  var defaultRequestTimeoutMs = 12e3;
  var defaultMaxQueueSize = 500;
  var defaultSessionHeartbeatIntervalMs = 3e5;
  var defaultFirstLaunchSessionHeartbeatIntervalMs = 3e4;
  var defaultEventFlushIntervalMs = 6e4;
  var defaultStorageKeyPrefix = "attriax:web";
  function normalizeConfig(config) {
    const projectToken = config.projectToken;
    if (!projectToken || projectToken.trim().length === 0) {
      throw new Error("AttriaxConfig.projectToken is required.");
    }
    const apiBaseUrl = normalizeApiBaseUrl(
      config.apiBaseUrl || defaultApiBaseUrl
    );
    const eventFlushIntervalMs = config.eventFlushIntervalMs == null ? defaultEventFlushIntervalMs : config.eventFlushIntervalMs;
    if (eventFlushIntervalMs < 0) {
      throw new Error("AttriaxConfig.eventFlushIntervalMs must not be negative.");
    }
    return {
      projectToken,
      platform: config.platform ?? "web",
      apiBaseUrl,
      sdkClientBaseUrl: buildSdkClientBaseUrl(apiBaseUrl),
      appVersion: config.appVersion,
      appBuildNumber: config.appBuildNumber,
      appPackageName: config.appPackageName,
      sdkMetadata: buildSdkMetadata(config),
      enableDebugLogs: config.enableDebugLogs ?? false,
      automaticBrowserHandling: config.automaticBrowserHandling ?? true,
      sessionTrackingEnabled: config.sessionTrackingEnabled ?? true,
      automaticPageTracking: config.automaticPageTracking ?? true,
      gdprEnabled: config.gdprEnabled ?? false,
      anonymousTracking: config.anonymousTracking ?? true,
      ...typeof config.doNotSell === "boolean" ? { doNotSell: config.doNotSell } : {},
      ...config.usPrivacy != null ? { usPrivacy: config.usPrivacy } : {},
      sessionHeartbeatIntervalMs: config.sessionHeartbeatIntervalMs != null && config.sessionHeartbeatIntervalMs > 0 ? config.sessionHeartbeatIntervalMs : defaultSessionHeartbeatIntervalMs,
      firstLaunchSessionHeartbeatIntervalMs: config.firstLaunchSessionHeartbeatIntervalMs != null && config.firstLaunchSessionHeartbeatIntervalMs > 0 ? config.firstLaunchSessionHeartbeatIntervalMs : defaultFirstLaunchSessionHeartbeatIntervalMs,
      eventFlushIntervalMs,
      flushEventsImmediatelyOnFirstLaunch: config.flushEventsImmediatelyOnFirstLaunch ?? true,
      requestTimeoutMs: config.requestTimeoutMs ?? defaultRequestTimeoutMs,
      maxQueueSize: config.maxQueueSize ?? defaultMaxQueueSize,
      storageKeyPrefix: config.storageKeyPrefix ?? defaultStorageKeyPrefix
    };
  }
  function normalizeApiBaseUrl(value) {
    const normalized = value.replace(/\/+$/, "");
    let url;
    try {
      url = new URL(normalized);
    } catch {
      throw new Error("AttriaxConfig.apiBaseUrl must be a valid absolute URL.");
    }
    const hostname = normalizeHost(url.hostname);
    const isLocalhost = hostname === "localhost" || hostname === "127.0.0.1" || hostname === "::1";
    if (url.protocol !== "https:" && !(isLocalhost && url.protocol === "http:")) {
      throw new Error(
        "AttriaxConfig.apiBaseUrl must use HTTPS unless it targets localhost."
      );
    }
    return normalized;
  }
  function normalizeHost(hostname) {
    if (hostname.startsWith("[") && hostname.endsWith("]")) {
      return hostname.slice(1, -1);
    }
    return hostname;
  }
  function buildSdkClientBaseUrl(apiBaseUrl) {
    if (apiBaseUrl.endsWith("/api/sdk")) {
      return apiBaseUrl.slice(0, -"/api/sdk".length);
    }
    if (apiBaseUrl.endsWith("/api")) {
      return apiBaseUrl.slice(0, -"/api".length);
    }
    return apiBaseUrl;
  }
  function buildSdkMetadata(config) {
    const sdkMetadata = config.sdkMetadata ? normalizeJsonMap(config.sdkMetadata) : {};
    const clientRuntime = sdkMetadata.clientRuntime === "react" || sdkMetadata.clientRuntime === "react_native" ? sdkMetadata.clientRuntime : "web";
    return {
      ...sdkMetadata,
      clientRuntime
    };
  }

  // src/internal/context-manager.ts
  var AttriaxContextManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "deviceIdValue", null);
      __publicField(this, "deviceIdSourceValue", null);
      __publicField(this, "firstLaunchValue", false);
      __publicField(this, "snapshotValue", null);
      __publicField(this, "initGeneration", 0);
      __publicField(this, "identityStore");
      __publicField(this, "launchStateStore");
      __publicField(this, "snapshotBuilder");
      this.identityStore = options.identityStore;
      this.launchStateStore = options.launchStateStore;
      this.snapshotBuilder = options.snapshotBuilder;
    }
    get deviceId() {
      return this.deviceIdValue;
    }
    get deviceIdSource() {
      return this.deviceIdSourceValue;
    }
    get isFirstLaunch() {
      return this.firstLaunchValue;
    }
    get sdkSnapshot() {
      return this.snapshotValue?.sdk ?? null;
    }
    get snapshot() {
      return this.snapshotValue;
    }
    resolveTimezone() {
      return this.snapshotValue?.device.timezone ?? safeTimeZone();
    }
    async init(options = {}) {
      const generation = ++this.initGeneration;
      const isFirstLaunch = !this.launchStateStore.hasLaunched();
      this.firstLaunchValue = isFirstLaunch;
      let snapshot;
      if (options.allowDeviceIdentity === false) {
        this.deviceIdValue = null;
        this.deviceIdSourceValue = null;
        snapshot = await this.snapshotBuilder.buildAnonymousStartupSnapshot({
          isFirstLaunch
        });
      } else {
        snapshot = await this.buildIdentifiedSnapshot({ isFirstLaunch });
      }
      if (generation !== this.initGeneration) {
        return;
      }
      this.snapshotValue = snapshot;
    }
    async ensureIdentifiedContext() {
      const snapshot = await this.buildIdentifiedSnapshot({
        isFirstLaunch: this.snapshotValue?.isFirstLaunch ?? this.firstLaunchValue
      });
      if (this.snapshotValue !== null) {
        this.snapshotValue = snapshot;
      }
      return snapshot;
    }
    requireDeviceId() {
      if (!this.deviceIdValue) {
        throw new Error("Attriax device ID is not available before init().");
      }
      return this.deviceIdValue;
    }
    requireDeviceIdSource() {
      return this.deviceIdSourceValue || "persistent_storage";
    }
    requireSnapshot() {
      const snapshot = this.snapshotValue;
      if (!snapshot) {
        throw new Error("Attriax context snapshot is not available before init().");
      }
      return snapshot;
    }
    reset() {
      this.initGeneration += 1;
      this.deviceIdValue = null;
      this.deviceIdSourceValue = null;
      this.firstLaunchValue = false;
      this.snapshotValue = null;
    }
    ensureDeviceIdentity() {
      const deviceIdentity = this.identityStore.restoreOrCreateDeviceIdentity();
      this.deviceIdValue = deviceIdentity.deviceId;
      this.deviceIdSourceValue = deviceIdentity.source;
      return deviceIdentity;
    }
    async buildIdentifiedSnapshot(options) {
      const deviceIdentity = this.ensureDeviceIdentity();
      this.deviceIdValue = deviceIdentity.deviceId;
      this.deviceIdSourceValue = deviceIdentity.source;
      this.options.logger.verbose(
        `Using device ID (${deviceIdentity.source}):`,
        this.deviceIdValue
      );
      return this.snapshotBuilder.build({
        deviceId: deviceIdentity.deviceId,
        isFirstLaunch: options.isFirstLaunch
      });
    }
  };

  // src/internal/context-identity-store.ts
  var AttriaxContextIdentityStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    restoreOrCreateDeviceIdentity() {
      const existingDeviceId = this.options.storageManager.readValue("deviceId");
      const existingDeviceIdSource = this.options.storageManager.readValue("deviceIdSource");
      if (existingDeviceId) {
        const source2 = existingDeviceIdSource || "persistent_storage";
        if (!existingDeviceIdSource) {
          this.options.storageManager.writeValue("deviceIdSource", source2);
        }
        return {
          deviceId: existingDeviceId,
          source: source2
        };
      }
      const generated = generateId();
      const source = "persistent_storage";
      this.options.storageManager.writeValue("deviceId", generated);
      this.options.storageManager.writeValue("deviceIdSource", source);
      return {
        deviceId: generated,
        source
      };
    }
  };

  // src/internal/app-info.ts
  async function loadWebAppInfo(options = {}) {
    const fetcher = options.fetcher ?? globalThis.fetch;
    if (typeof fetcher !== "function") {
      return null;
    }
    const attemptedUrls = /* @__PURE__ */ new Set();
    const cacheBuster = (options.cacheBusterFactory ?? (() => Date.now()))();
    const baseUrls = options.baseUrls ?? [safeDocumentBaseUrl, safeLocationHref];
    const timeoutMs = options.timeoutMs;
    for (const baseUrlProvider of baseUrls) {
      const baseUrl = baseUrlProvider();
      if (!baseUrl) {
        continue;
      }
      const versionUrl = buildVersionJsonUrl(baseUrl, cacheBuster).toString();
      if (attemptedUrls.has(versionUrl)) {
        continue;
      }
      attemptedUrls.add(versionUrl);
      try {
        const response = await fetchWithTimeout(fetcher, versionUrl, timeoutMs);
        if (!response.ok) {
          continue;
        }
        const json = await response.json();
        const snapshot = {
          ...normalizeString3(json.version) ? { version: normalizeString3(json.version) } : {},
          ...normalizeString3(json.build_number) || normalizeString3(json.buildNumber) ? {
            buildNumber: normalizeString3(json.build_number) || normalizeString3(json.buildNumber)
          } : {},
          ...normalizeString3(json.package_name) || normalizeString3(json.packageName) ? {
            packageName: normalizeString3(json.package_name) || normalizeString3(json.packageName)
          } : {}
        };
        if (snapshot.version || snapshot.buildNumber || snapshot.packageName) {
          return snapshot;
        }
      } catch {
        continue;
      }
    }
    return null;
  }
  async function fetchWithTimeout(fetcher, input, timeoutMs) {
    if (timeoutMs == null || timeoutMs <= 0 || typeof AbortController === "undefined") {
      return fetcher(input);
    }
    const controller = new AbortController();
    const timer = globalThis.setTimeout(() => controller.abort(), timeoutMs);
    try {
      return await fetcher(input, { signal: controller.signal });
    } finally {
      globalThis.clearTimeout(timer);
    }
  }
  function buildVersionJsonUrl(baseUrl, cacheBuster) {
    const fragmentIndex = baseUrl.indexOf("#");
    const sanitizedBaseUrl = fragmentIndex >= 0 ? baseUrl.slice(0, fragmentIndex) : baseUrl;
    const url = new URL(sanitizedBaseUrl, "https://attriax.invalid");
    const pathSegments = url.pathname.split("/").filter(Boolean);
    const lastSegment = pathSegments[pathSegments.length - 1];
    const looksLikeHtml = Boolean(lastSegment && /[^/]+\.html$/i.test(lastSegment));
    const shouldTrimLastSegment = looksLikeHtml || (url.protocol === "http:" || url.protocol === "https:") && url.pathname.length > 1 && !url.pathname.endsWith("/");
    if (shouldTrimLastSegment) {
      pathSegments.pop();
    }
    url.pathname = `/${[...pathSegments, "version.json"].join("/")}`.replace(/\/+/g, "/");
    url.search = `cachebuster=${cacheBuster}`;
    url.hash = "";
    return url;
  }
  function safeDocumentBaseUrl() {
    return typeof document !== "undefined" ? normalizeString3(document.baseURI) : void 0;
  }
  function safeLocationHref() {
    return typeof window !== "undefined" ? normalizeString3(window.location?.href) : void 0;
  }
  function normalizeString3(value) {
    if (typeof value !== "string" && typeof value !== "number") {
      return void 0;
    }
    const normalized = String(value).trim();
    return normalized ? normalized : void 0;
  }

  // src/internal/context-snapshot-builder.ts
  var AttriaxContextSnapshotBuilder = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    async build(options) {
      const app = await this.buildAppSnapshot();
      return this.buildSnapshot({
        deviceId: options.deviceId,
        isFirstLaunch: options.isFirstLaunch,
        app,
        device: this.buildWebDeviceSnapshot({ includePageMetadata: true })
      });
    }
    async buildAnonymousStartupSnapshot(options) {
      const app = await this.buildAppSnapshot();
      return this.buildSnapshot({
        deviceId: null,
        isFirstLaunch: options.isFirstLaunch,
        app,
        device: this.buildWebDeviceSnapshot({ includePageMetadata: false })
      });
    }
    buildWebDeviceSnapshot(options) {
      const browserUrl = options.includePageMetadata ? getCurrentUrl()?.toString() : void 0;
      const referrer = options.includePageMetadata ? safeDocumentReferrer() : void 0;
      const title = options.includePageMetadata ? safeDocumentTitle() : void 0;
      const language = safeNavigatorLanguage();
      const timezone = safeTimeZone();
      const screenResolution = safeScreenResolution();
      const screenWidth = safeScreenWidth();
      const screenHeight = safeScreenHeight();
      const devicePixelRatio = safeDevicePixelRatio();
      const colorDepth = safeColorDepth();
      const deviceName = safeNavigatorAppName();
      const deviceModel = safeNavigatorBrowserName() ?? safeUserAgent();
      const devicePlatform = safeNavigatorPlatform();
      const deviceVendor = safeNavigatorVendor();
      const userAgent = safeUserAgent();
      const deviceMetadata = {};
      if (browserUrl) {
        deviceMetadata.url = browserUrl;
      }
      if (referrer) {
        deviceMetadata.referrer = referrer;
      }
      if (title) {
        deviceMetadata.title = title;
      }
      if (userAgent) {
        deviceMetadata.userAgent = userAgent;
      }
      const isBot = detectBotEnvironment();
      if (isBot) {
        deviceMetadata.isBot = true;
      }
      return {
        ...deviceModel ? { model: deviceModel } : {},
        ...deviceName ? { name: deviceName } : {},
        ...deviceVendor ? { brand: deviceVendor, manufacturer: deviceVendor } : {},
        ...devicePlatform ? { hardware: devicePlatform, osVersion: devicePlatform } : {},
        ...language ? { language } : {},
        ...timezone ? { timezone } : {},
        ...screenResolution ? { screenResolution } : {},
        ...screenWidth != null ? { screenWidth } : {},
        ...screenHeight != null ? { screenHeight } : {},
        ...devicePixelRatio != null ? { devicePixelRatio } : {},
        ...colorDepth != null ? { colorDepth } : {},
        supportedAbis: [],
        metadata: deviceMetadata
      };
    }
    async buildAppSnapshot() {
      const appInfo = await loadWebAppInfo({
        timeoutMs: this.options.config.requestTimeoutMs
      });
      return {
        ...this.options.config.appVersion || appInfo?.version ? { version: this.options.config.appVersion ?? appInfo?.version } : {},
        ...this.options.config.appBuildNumber || appInfo?.buildNumber ? {
          buildNumber: this.options.config.appBuildNumber ?? appInfo?.buildNumber
        } : {},
        ...this.options.config.appPackageName || appInfo?.packageName ? {
          packageName: this.options.config.appPackageName ?? appInfo?.packageName
        } : {}
      };
    }
    buildSnapshot(options) {
      return {
        platform: this.options.config.platform,
        deviceId: options.deviceId,
        isFirstLaunch: options.isFirstLaunch,
        sdk: {
          apiVersion: attriaxSdkApiVersion,
          packageVersion: attriaxSdkPackageVersion,
          metadata: { ...this.options.config.sdkMetadata }
        },
        app: options.app,
        device: options.device
      };
    }
  };

  // src/internal/deep-links.ts
  var syntheticDeepLinkOrigin = "https://attriax.invalid";
  function normalizeLinkPath(path) {
    if (typeof path !== "string") {
      return void 0;
    }
    const trimmed = path.trim();
    if (!trimmed) {
      return void 0;
    }
    const normalized = trimmed.replace(/^\/+/, "").replace(/\/+$/, "");
    return normalized || void 0;
  }
  function buildDeepLinkUri(options) {
    const uri = normalizeUrlInput(options.uri);
    if (uri) {
      try {
        return new URL(uri);
      } catch {
        const path = uri.startsWith("/") ? uri : `/${uri}`;
        return new URL(path, syntheticDeepLinkOrigin);
      }
    }
    return void 0;
  }
  function isAttriaxUrl(url) {
    const host = url?.hostname?.trim().toLowerCase();
    return typeof host === "string" && host.endsWith(".attriax.com");
  }
  function buildResolution(resolution, {
    clickedAt,
    trigger,
    isAttriaxSubDomain,
    fallbackUri,
    rawEvent,
    handledBySdk = false
  }) {
    return {
      uri: resolution.deepLink?.uri ?? fallbackUri ?? new URL(
        `/${normalizeLinkPath(resolution.deepLink?.path) ?? ""}`,
        syntheticDeepLinkOrigin
      ),
      clickedAt,
      consumedAt: resolution.consumedAt ?? resolution.acceptedAt ?? /* @__PURE__ */ new Date(),
      found: resolution.matched,
      trigger,
      isAttriaxSubDomain,
      rawEvent,
      data: stringifyData(resolution.deepLink?.data),
      utm: resolution.deepLink?.utm,
      browserAction: resolution.browserAction,
      handledBySdk,
      isDeferred: trigger === "deferred" /* Deferred */,
      isColdStart: trigger === "coldStart" /* ColdStart */,
      isForeground: trigger === "foreground" /* Foreground */
    };
  }
  function buildDeferredUri(result) {
    const deferredUri = result.installReferrer?.deepLinkUri;
    if (deferredUri) {
      return new URL(deferredUri.toString());
    }
    const deferredUrl = result.installReferrer?.deepLinkUrl?.trim();
    if (deferredUrl) {
      try {
        return new URL(deferredUrl);
      } catch {
      }
    }
    const normalizedPath = normalizeLinkPath(result.deepLink?.path);
    return new URL(
      normalizedPath ? `/${normalizedPath}` : "/",
      syntheticDeepLinkOrigin
    );
  }
  function buildDeferredResolution(result, { fallbackTime }) {
    const uri = buildDeferredUri(result);
    return {
      uri,
      clickedAt: result.deepLinkClickedAt ?? result.acceptedAt ?? fallbackTime,
      consumedAt: result.deepLinkConsumedAt ?? result.acceptedAt ?? fallbackTime,
      found: Boolean(result.deepLink),
      trigger: "deferred" /* Deferred */,
      isAttriaxSubDomain: isAttriaxUrl(uri),
      data: stringifyData(
        result.deepLink?.data ?? result.installReferrer?.deepLinkData
      ),
      utm: result.deepLink?.utm,
      handledBySdk: false,
      isDeferred: true,
      isColdStart: false,
      isForeground: false
    };
  }
  function stringifyData(data) {
    if (!data || Object.keys(data).length === 0) {
      return void 0;
    }
    return Object.fromEntries(
      Object.entries(data).map(([key, value]) => [
        key,
        value == null ? "" : String(value)
      ])
    );
  }
  function extractLinkPathFromUrl(url) {
    const candidate = url.pathname && url.pathname !== "/" ? toRelativeLinkPath(url) : url.hostname;
    return normalizeLinkPath(candidate);
  }

  // src/internal/deep-link-manager.ts
  var AttriaxDeepLinkManager = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    get rawInitialDeepLink() {
      return this.options.eventHub.rawInitialDeepLink;
    }
    get initialDeepLink() {
      return this.options.eventHub.initialDeepLink;
    }
    get isInitialDeepLinkResolved() {
      return this.options.eventHub.isInitialDeepLinkResolved;
    }
    waitForInitialDeepLink() {
      return this.options.eventHub.waitForInitialDeepLink();
    }
    waitForResolution(rawEvent) {
      return this.options.eventHub.waitForResolution(rawEvent);
    }
    get latestDeepLink() {
      return this.options.eventHub.latestDeepLink;
    }
    subscribeToRawDeepLinks(listener) {
      return this.options.eventHub.subscribeToRawDeepLinks(listener);
    }
    subscribe(listener) {
      return this.options.eventHub.subscribeToDeepLinks(listener);
    }
    observeInitialUrlCapture(enabled) {
      if (!enabled) {
        this.options.eventHub.completeInitialDeepLinkIfAbsent();
        return;
      }
      this.captureInitialUrl();
    }
    recordDeepLink(options) {
      this.options.sessionManager?.prepareTrackedSessionActivity(/* @__PURE__ */ new Date());
      return this.resolveDeepLink(options);
    }
    handleDeferredAppOpen(result) {
      if (!result.deepLink) {
        return;
      }
      this.options.eventHub.emitResolvedDeepLink(
        buildDeferredResolution(result, {
          fallbackTime: /* @__PURE__ */ new Date()
        })
      );
    }
    captureInitialUrl() {
      const url = getCurrentUrl();
      if (!url || !shouldCaptureInitialUrl(url)) {
        this.options.eventHub.completeInitialDeepLinkIfAbsent();
        return;
      }
      this.handleIncomingDeepLink(url, { isInitialLink: true });
    }
    async resolveDeepLink(options) {
      const isFirstLaunch = this.options.contextManager.isFirstLaunch;
      const clickedAt = /* @__PURE__ */ new Date();
      const uri = buildDeepLinkUri(options);
      if (!uri) {
        throw new Error("recordDeepLink() requires a URI.");
      }
      const resolution = await this.resolvePayload({
        platform: this.options.platform,
        rawUrl: uri.toString(),
        linkPath: extractLinkPathFromUrl(uri),
        source: options.source ?? (options.isInitialLink ? "initial_url" : "manual"),
        isFirstLaunch,
        metadata: normalizeJsonObject(options.metadata)
      });
      const handledBySdk = this.handleBrowserAction(resolution);
      return buildResolution(resolution, {
        clickedAt,
        trigger: options.isInitialLink ? "coldStart" /* ColdStart */ : "foreground" /* Foreground */,
        isAttriaxSubDomain: isAttriaxUrl(uri),
        fallbackUri: uri,
        handledBySdk
      });
    }
    handleIncomingDeepLink(url, { isInitialLink }) {
      const receivedAt = /* @__PURE__ */ new Date();
      const resolutionPromise = this.resolvePayload({
        platform: this.options.platform,
        rawUrl: url.toString(),
        source: "attriax_sdk",
        isFirstLaunch: this.options.contextManager.isFirstLaunch,
        metadata: normalizeJsonObject({
          isInitialLink,
          queryParameters: Object.fromEntries(url.searchParams.entries())
        })
      });
      const event = this.options.eventHub.emitPendingDeepLink({
        uri: url,
        receivedAt,
        isInitialLink
      });
      void resolutionPromise.then(
        (resolution) => {
          const handledBySdk = this.handleBrowserAction(resolution);
          const resolvedEvent = buildResolution(resolution, {
            clickedAt: receivedAt,
            trigger: isInitialLink ? "coldStart" /* ColdStart */ : "foreground" /* Foreground */,
            isAttriaxSubDomain: isAttriaxUrl(url),
            fallbackUri: url,
            rawEvent: event,
            handledBySdk
          });
          this.options.eventHub.resolvePendingDeepLink({
            event,
            resolution: resolvedEvent
          });
          this.options.eventHub.emitResolvedDeepLink(resolvedEvent);
        },
        (error) => {
          this.options.eventHub.failPendingDeepLink({ event, error });
        }
      );
      return event;
    }
    handleBrowserAction(resolution) {
      if (!this.options.automaticBrowserHandling || !resolution.browserAction) {
        return false;
      }
      return openResolvedBrowserAction(
        resolution.browserAction.url,
        resolution.browserAction.openMode
      );
    }
    resolvePayload(payload) {
      const requestPayload = this.withOptionalDeviceIdentity(payload);
      const decision = this.trackingDecision();
      const shouldSendDirectly = decision.sendNetworkDirectly && !decision.attachDeviceIdentity;
      if (shouldSendDirectly) {
        if (!this.options.requestDispatcher) {
          throw new Error(
            "Direct deep-link resolution requires a request dispatcher."
          );
        }
        return this.options.requestDispatcher.resolveDeepLink(requestPayload);
      }
      return this.options.requestManager.enqueue({
        kind: "deepLinkResolve",
        payload: requestPayload
      });
    }
    withOptionalDeviceIdentity(payload) {
      if (!this.trackingDecision().attachDeviceIdentity) {
        return payload;
      }
      return {
        ...payload,
        deviceId: this.options.contextManager.requireDeviceId(),
        deviceIdSource: this.options.contextManager.requireDeviceIdSource()
      };
    }
    trackingDecision() {
      return this.options.trackingDecision?.() ?? {
        capture: true,
        identityMode: "identified",
        deferNetwork: false,
        attachDeviceIdentity: true,
        sendNetworkDirectly: true
      };
    }
  };

  // src/internal/dynamic-link-creation-manager.ts
  var AttriaxDynamicLinkCreationManager = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    createDynamicLink(options = {}) {
      return this.options.requestDispatcher.createDynamicLink(
        buildCreateDynamicLinkRequest(options)
      );
    }
  };
  function buildCreateDynamicLinkRequest(options) {
    const destinationUrl = options.destinationUrl ? validateDynamicLinkDestinationUrl(trimOrNull(options.destinationUrl) ?? options.destinationUrl) : void 0;
    return {
      ...trimOrNull(options.name) ? { name: trimOrNull(options.name) } : {},
      ...destinationUrl ? { destinationUrl } : {},
      ...trimOrNull(options.group) ? { group: trimOrNull(options.group) } : {},
      ...trimOrNull(options.prefix) ? { prefix: trimOrNull(options.prefix) } : {},
      ...typeof options.redirects?.ios === "boolean" ? { iosRedirect: options.redirects.ios } : {},
      ...typeof options.redirects?.android === "boolean" ? { androidRedirect: options.redirects.android } : {},
      ...trimOrNull(options.socialPreview?.title) ? { previewTitle: trimOrNull(options.socialPreview?.title) } : {},
      ...trimOrNull(options.socialPreview?.description) ? { previewDescription: trimOrNull(options.socialPreview?.description) } : {},
      ...trimOrNull(options.utms?.source) ? { utmSource: trimOrNull(options.utms?.source) } : {},
      ...trimOrNull(options.utms?.medium) ? { utmMedium: trimOrNull(options.utms?.medium) } : {},
      ...trimOrNull(options.utms?.campaign) ? { utmCampaign: trimOrNull(options.utms?.campaign) } : {},
      ...trimOrNull(options.utms?.term) ? { utmTerm: trimOrNull(options.utms?.term) } : {},
      ...trimOrNull(options.utms?.content) ? { utmContent: trimOrNull(options.utms?.content) } : {},
      ...options.data ? { data: normalizeJsonObject(options.data) } : {}
    };
  }
  function trimOrNull(value) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : void 0;
  }
  function validateDynamicLinkDestinationUrl(destinationUrl) {
    let parsed;
    try {
      parsed = new URL(destinationUrl);
    } catch {
      throw new AttriaxApiError(
        "Dynamic link destinationUrl must be a valid absolute URL.",
        void 0,
        false,
        true
      );
    }
    if (parsed.protocol !== "https:" && parsed.protocol !== "http:") {
      throw new AttriaxApiError(
        "Dynamic link destinationUrl must use HTTP or HTTPS.",
        void 0,
        false,
        true
      );
    }
    return parsed.toString();
  }

  // src/internal/event-hub.ts
  var AttriaxEventHub = class {
    constructor() {
      __publicField(this, "rawDeepLinkListeners", /* @__PURE__ */ new Set());
      __publicField(this, "deepLinkListeners", /* @__PURE__ */ new Set());
      __publicField(this, "synchronizationListeners", /* @__PURE__ */ new Set());
      __publicField(this, "pendingDeepLinkResolutions", /* @__PURE__ */ new Map());
      __publicField(this, "initialDeepLinkDeferred", createDeferred2());
      __publicField(this, "rawInitialDeepLinkValue", null);
      __publicField(this, "initialDeepLinkValue", null);
      __publicField(this, "initialDeepLinkResolvedValue", false);
      __publicField(this, "latestDeepLinkValue", null);
      __publicField(this, "hasPendingInitialDeepLink", false);
      __publicField(this, "synchronizationStateValue", "initializing" /* Initializing */);
    }
    get rawInitialDeepLink() {
      return this.rawInitialDeepLinkValue;
    }
    get initialDeepLink() {
      return this.initialDeepLinkValue;
    }
    get isInitialDeepLinkResolved() {
      return this.initialDeepLinkResolvedValue;
    }
    waitForInitialDeepLink() {
      return this.initialDeepLinkDeferred.promise;
    }
    waitForResolution(rawEvent) {
      const pendingResolution = this.pendingDeepLinkResolutions.get(rawEvent);
      if (pendingResolution) {
        return pendingResolution.promise;
      }
      return Promise.reject(
        new Error("No pending or completed resolution exists for this raw deep link.")
      );
    }
    get latestDeepLink() {
      return this.latestDeepLinkValue;
    }
    get synchronizationState() {
      return this.synchronizationStateValue;
    }
    subscribeToRawDeepLinks(listener) {
      this.rawDeepLinkListeners.add(listener);
      return () => {
        this.rawDeepLinkListeners.delete(listener);
      };
    }
    subscribeToDeepLinks(listener) {
      this.deepLinkListeners.add(listener);
      return () => {
        this.deepLinkListeners.delete(listener);
      };
    }
    subscribeToSynchronization(listener) {
      this.synchronizationListeners.add(listener);
      return () => {
        this.synchronizationListeners.delete(listener);
      };
    }
    emitSynchronizationState(value) {
      if (this.synchronizationStateValue === value) {
        return;
      }
      this.synchronizationStateValue = value;
      for (const listener of this.synchronizationListeners) {
        listener(value);
      }
    }
    emitPendingDeepLink({
      uri,
      receivedAt,
      isInitialLink
    }) {
      const pendingResolution = createDeferred2();
      const event = createRawDeepLinkEvent({
        uri,
        receivedAt,
        isInitial: isInitialLink
      });
      this.pendingDeepLinkResolutions.set(event, pendingResolution);
      if (isInitialLink) {
        this.hasPendingInitialDeepLink = true;
        this.rawInitialDeepLinkValue = event;
      }
      this.notifyRawDeepLinkListeners(event);
      return event;
    }
    resolvePendingDeepLink({
      event,
      resolution
    }) {
      const pendingResolution = this.pendingDeepLinkResolutions.get(event);
      if (!pendingResolution || pendingResolution.isCompleted) {
        return;
      }
      pendingResolution.resolve(resolution);
      if (event.isInitial) {
        this.hasPendingInitialDeepLink = false;
      }
    }
    failPendingDeepLink({
      event,
      error
    }) {
      const pendingResolution = this.pendingDeepLinkResolutions.get(event);
      if (!pendingResolution || pendingResolution.isCompleted) {
        return;
      }
      this.completeWithError(pendingResolution, error);
      if (event.isInitial) {
        this.hasPendingInitialDeepLink = false;
        if (!this.initialDeepLinkResolvedValue) {
          this.initialDeepLinkResolvedValue = true;
          this.initialDeepLinkDeferred.reject(error);
        }
      }
    }
    emitResolvedDeepLink(event) {
      this.latestDeepLinkValue = event;
      if (event.isColdStart) {
        this.completeInitialDeepLink(event);
      }
      this.notifyDeepLinkListeners(event);
      return event;
    }
    completeInitialDeepLinkIfAbsent() {
      if (this.hasPendingInitialDeepLink || this.initialDeepLinkResolvedValue) {
        return;
      }
      this.completeInitialDeepLink(null);
    }
    reset() {
      for (const pendingResolution of this.pendingDeepLinkResolutions.values()) {
        if (!pendingResolution.isCompleted) {
          this.completeWithError(
            pendingResolution,
            new Error("Attriax SDK state was reset before deep-link resolution completed.")
          );
        }
      }
      this.pendingDeepLinkResolutions.clear();
      if (!this.initialDeepLinkResolvedValue) {
        this.initialDeepLinkDeferred.resolve(null);
      }
      this.initialDeepLinkDeferred = createDeferred2();
      this.rawInitialDeepLinkValue = null;
      this.initialDeepLinkValue = null;
      this.initialDeepLinkResolvedValue = false;
      this.latestDeepLinkValue = null;
      this.hasPendingInitialDeepLink = false;
      this.synchronizationStateValue = "initializing" /* Initializing */;
    }
    dispose() {
      for (const pendingResolution of this.pendingDeepLinkResolutions.values()) {
        if (!pendingResolution.isCompleted) {
          this.completeWithError(
            pendingResolution,
            new Error("Attriax SDK disposed before deep-link resolution completed.")
          );
        }
      }
      this.pendingDeepLinkResolutions.clear();
      this.completeInitialDeepLinkIfAbsent();
      this.rawDeepLinkListeners.clear();
      this.deepLinkListeners.clear();
      this.synchronizationListeners.clear();
    }
    completeInitialDeepLink(result) {
      if (this.initialDeepLinkResolvedValue) {
        return;
      }
      this.initialDeepLinkValue = result;
      this.initialDeepLinkResolvedValue = true;
      if (result) {
        this.latestDeepLinkValue = result;
      }
      this.initialDeepLinkDeferred.resolve(result);
    }
    notifyRawDeepLinkListeners(event) {
      for (const listener of this.rawDeepLinkListeners) {
        listener(event);
      }
    }
    notifyDeepLinkListeners(event) {
      for (const listener of this.deepLinkListeners) {
        listener(event);
      }
    }
    completeWithError(deferred, error) {
      void deferred.promise.catch(() => void 0);
      deferred.reject(error);
    }
  };
  function createDeferred2() {
    let resolve;
    let reject;
    let isCompleted = false;
    const promise = new Promise((resolved, rejected) => {
      resolve = (value) => {
        isCompleted = true;
        resolved(value);
      };
      reject = (reason) => {
        isCompleted = true;
        rejected(reason);
      };
    });
    return {
      promise,
      resolve,
      reject,
      get isCompleted() {
        return isCompleted;
      }
    };
  }
  function createRawDeepLinkEvent({
    uri,
    receivedAt,
    isInitial
  }) {
    return {
      uri,
      receivedAt,
      isInitial
    };
  }

  // src/internal/install-referrer-manager.ts
  var AttriaxInstallReferrerManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "originalDeferred", null);
      __publicField(this, "originalValue", null);
      __publicField(this, "originalLoaded", false);
      __publicField(this, "originalCompletedForDisabled", false);
      __publicField(this, "reinstallDeferred", null);
      __publicField(this, "reinstallValue", null);
      __publicField(this, "reinstallLoaded", false);
      __publicField(this, "reinstallCompletedForDisabled", false);
      __publicField(this, "observationPromise", null);
      __publicField(this, "observationGeneration", 0);
    }
    getOriginalInstallReferrer() {
      return this.getDeferredPromise(this.originalDeferred);
    }
    getReinstallReferrer() {
      return this.getDeferredPromise(this.reinstallDeferred);
    }
    init({ enabled }) {
      this.ensureDeferreds();
      this.restoreStoredValues();
      if (!enabled) {
        this.completeDisabled();
        return;
      }
      this.reopenDeferredsIfNeeded();
      this.completeLoadedValues();
      if (this.areReferrersLoaded()) {
        return;
      }
      this.ensureObservationStarted();
    }
    prepareForEnabledState() {
      this.ensureDeferreds();
      this.restoreStoredValues();
      this.reopenDeferredsIfNeeded();
      this.completeLoadedValues();
      if (this.areReferrersLoaded()) {
        return;
      }
      this.ensureObservationStarted();
    }
    prepareForReenable() {
      this.ensureDeferreds();
      this.restoreStoredValues();
      this.reopenDeferredsIfNeeded();
      this.completeLoadedValues();
      if (this.areReferrersLoaded()) {
        return;
      }
      this.ensureObservationStarted();
    }
    reset() {
      this.observationGeneration += 1;
      this.originalDeferred = null;
      this.originalValue = null;
      this.originalLoaded = false;
      this.originalCompletedForDisabled = false;
      this.reinstallDeferred = null;
      this.reinstallValue = null;
      this.reinstallLoaded = false;
      this.reinstallCompletedForDisabled = false;
      this.observationPromise = null;
    }
    completeDisabled() {
      this.completeOriginal(null, { disabledResult: true });
      this.completeReinstall(null, { disabledResult: true });
    }
    dispose() {
      if (this.originalDeferred && !this.originalDeferred.isCompleted) {
        this.originalDeferred.resolve(null);
      }
      if (this.reinstallDeferred && !this.reinstallDeferred.isCompleted) {
        this.reinstallDeferred.resolve(null);
      }
    }
    getDeferredPromise(deferred) {
      if (!deferred) {
        return Promise.reject(new Error("Attriax.init() must complete before using this SDK instance."));
      }
      return deferred.promise;
    }
    restoreStoredValues() {
      if (!this.originalLoaded) {
        const originalStored = this.options.store.readOriginalInstallReferrer();
        if (originalStored.loaded) {
          this.originalLoaded = true;
          this.originalValue = originalStored.value;
        } else {
          const legacyStored = this.options.store.readLegacyInstallReferrer();
          if (legacyStored.loaded) {
            this.originalLoaded = true;
            this.originalValue = legacyStored.value;
            this.options.store.writeOriginalInstallReferrer(legacyStored.value);
          }
        }
      }
      if (!this.reinstallLoaded) {
        const reinstallStored = this.options.store.readReinstallReferrer();
        if (reinstallStored.loaded) {
          this.reinstallLoaded = true;
          this.reinstallValue = reinstallStored.value;
        }
      }
    }
    ensureObservationStarted() {
      if (this.areReferrersLoaded() || this.observationPromise) {
        return;
      }
      const generation = this.observationGeneration;
      const observation = this.observeAppOpen(generation);
      this.observationPromise = observation;
      observation.finally(() => {
        if (this.observationPromise === observation) {
          this.observationPromise = null;
        }
      });
    }
    async observeAppOpen(generation) {
      try {
        const result = await this.options.appOpenManager.waitForScheduledResult();
        if (generation !== this.observationGeneration) {
          return;
        }
        const originalInstallReferrer = this.resolveOriginalInstallReferrer(result);
        const reinstallReferrer = this.resolveReinstallReferrer(result);
        this.originalValue = originalInstallReferrer;
        this.originalLoaded = true;
        this.reinstallValue = reinstallReferrer;
        this.reinstallLoaded = true;
        this.options.store.writeOriginalInstallReferrer(originalInstallReferrer);
        this.options.store.writeReinstallReferrer(reinstallReferrer);
        this.completeOriginal(originalInstallReferrer);
        this.completeReinstall(reinstallReferrer);
      } catch {
        if (generation !== this.observationGeneration) {
          return;
        }
        this.originalValue = null;
        this.originalLoaded = true;
        this.reinstallValue = null;
        this.reinstallLoaded = true;
        this.options.store.writeOriginalInstallReferrer(null);
        this.options.store.writeReinstallReferrer(null);
        this.completeOriginal(null);
        this.completeReinstall(null);
      }
    }
    resolveOriginalInstallReferrer(result) {
      if (!result) {
        return null;
      }
      if (result.originalInstallReferrer) {
        return result.originalInstallReferrer;
      }
      if (!result.installReferrer) {
        return null;
      }
      return result.installState === "reinstall" /* Reinstall */ ? null : result.installReferrer;
    }
    resolveReinstallReferrer(result) {
      if (!result) {
        return null;
      }
      if (result.reinstallReferrer) {
        return result.reinstallReferrer;
      }
      if (result.installState !== "reinstall" /* Reinstall */) {
        return null;
      }
      return result.installReferrer ?? null;
    }
    ensureDeferreds() {
      this.originalDeferred ?? (this.originalDeferred = createDeferred3());
      this.reinstallDeferred ?? (this.reinstallDeferred = createDeferred3());
    }
    completeOriginal(details, { disabledResult = false } = {}) {
      const deferred = this.originalDeferred;
      if (!deferred || deferred.isCompleted) {
        return;
      }
      this.originalCompletedForDisabled = disabledResult;
      deferred.resolve(details);
    }
    completeReinstall(details, { disabledResult = false } = {}) {
      const deferred = this.reinstallDeferred;
      if (!deferred || deferred.isCompleted) {
        return;
      }
      this.reinstallCompletedForDisabled = disabledResult;
      deferred.resolve(details);
    }
    completeLoadedValues() {
      if (this.originalLoaded) {
        this.completeOriginal(this.originalValue);
      }
      if (this.reinstallLoaded) {
        this.completeReinstall(this.reinstallValue);
      }
    }
    reopenDeferredsIfNeeded() {
      if (!this.originalDeferred || this.originalDeferred.isCompleted && this.originalCompletedForDisabled) {
        this.originalDeferred = createDeferred3();
        this.originalCompletedForDisabled = false;
      }
      if (!this.reinstallDeferred || this.reinstallDeferred.isCompleted && this.reinstallCompletedForDisabled) {
        this.reinstallDeferred = createDeferred3();
        this.reinstallCompletedForDisabled = false;
      }
    }
    areReferrersLoaded() {
      return this.originalLoaded && this.reinstallLoaded;
    }
  };
  function createDeferred3() {
    let resolve;
    let reject;
    let isCompleted = false;
    const promise = new Promise((resolved, rejected) => {
      resolve = (value) => {
        isCompleted = true;
        resolved(value);
      };
      reject = (reason) => {
        isCompleted = true;
        rejected(reason);
      };
    });
    return {
      promise,
      resolve,
      reject,
      get isCompleted() {
        return isCompleted;
      }
    };
  }

  // src/internal/install-referrer-store.ts
  var AttriaxInstallReferrerStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    readOriginalInstallReferrer() {
      return this.readStoredDetails("originalInstallReferrer", "originalInstallReferrerLoaded");
    }
    readLegacyInstallReferrer() {
      return this.readStoredDetails("installReferrer", "installReferrerLoaded");
    }
    readReinstallReferrer() {
      return this.readStoredDetails("reinstallReferrer", "reinstallReferrerLoaded");
    }
    writeOriginalInstallReferrer(details) {
      this.writeStoredDetails("originalInstallReferrer", "originalInstallReferrerLoaded", details);
    }
    writeReinstallReferrer(details) {
      this.writeStoredDetails("reinstallReferrer", "reinstallReferrerLoaded", details);
    }
    readStoredDetails(valueKey, loadedKey) {
      const loaded = this.options.storageManager.readBoolean(loadedKey) ?? false;
      if (!loaded) {
        return { loaded: false, value: null };
      }
      const raw = this.options.storageManager.readValue(valueKey);
      if (!raw) {
        return { loaded: true, value: null };
      }
      try {
        return { loaded: true, value: JSON.parse(raw) };
      } catch {
        return { loaded: true, value: null };
      }
    }
    writeStoredDetails(valueKey, loadedKey, details) {
      this.options.storageManager.writeBoolean(loadedKey, true);
      if (!details) {
        this.options.storageManager.removeValue(valueKey);
        return;
      }
      this.options.storageManager.writeValue(valueKey, JSON.stringify(details));
    }
  };

  // src/internal/launch-state-store.ts
  var AttriaxLaunchStateStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    hasLaunched() {
      return this.options.storageManager.readBoolean("hasLaunched") ?? false;
    }
    markLaunched() {
      this.options.storageManager.writeBoolean("hasLaunched", true);
    }
  };

  // src/internal/logger.ts
  var AttriaxLogger = class {
    constructor({ enableDebugLogs }) {
      __publicField(this, "enableDebugLogsValue");
      this.enableDebugLogsValue = enableDebugLogs;
    }
    get enableDebugLogs() {
      return this.enableDebugLogsValue;
    }
    setDebugLogsEnabled({ enabled }) {
      this.enableDebugLogsValue = enabled;
      this.verbose(`Debug logging ${enabled ? "enabled" : "disabled"}.`);
    }
    verbose(message, ...args) {
      if (!this.enableDebugLogsValue) {
        return;
      }
      this.write("VERBOSE", message, { args });
    }
    warning(message, { error, stackTrace } = {}) {
      this.write("WARNING", message, { error, stackTrace });
    }
    error(message, { error, stackTrace } = {}) {
      this.write("ERROR", message, { error, stackTrace });
    }
    write(level, message, {
      args = [],
      error,
      stackTrace
    }) {
      const consoleRef = globalThis.console;
      const writer = level === "ERROR" ? consoleRef.error?.bind(consoleRef) : level === "WARNING" ? consoleRef.warn?.bind(consoleRef) : consoleRef.debug?.bind(consoleRef);
      writer?.(`[Attriax][${level}] ${message}`, ...args);
      if (!this.enableDebugLogsValue) {
        return;
      }
      if (error !== void 0) {
        writer?.(`[Attriax][${level}]`, error);
      }
      const resolvedStackTrace = stackTrace ?? (error instanceof Error ? error.stack : void 0);
      if (resolvedStackTrace) {
        writer?.(`[Attriax][${level}] stackTrace`, resolvedStackTrace);
      }
    }
  };

  // src/internal/queue-store.ts
  var AttriaxQueueStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    readQueue() {
      return this.options.storageManager.readQueue();
    }
    writeQueue(entries) {
      this.options.storageManager.writeQueue(entries);
    }
  };

  // src/internal/batch-identity.ts
  function canShareBatchIdentity(left, right) {
    return left.payload.deviceId === right.payload.deviceId && left.payload.deviceIdSource === right.payload.deviceIdSource;
  }

  // src/internal/request-body-builder.ts
  function mapUserRequestToWire(payload) {
    const { userId, userName, clearUser, ...rest } = payload;
    return {
      ...rest,
      ...userId != null ? { externalUserId: userId } : {},
      ...userName != null ? { externalUserName: userName } : {},
      ...clearUser != null ? { clearExternalUser: clearUser } : {}
    };
  }
  function toWireRequestBody(kind, payload) {
    if (kind === "user") {
      return mapUserRequestToWire(
        payload
      );
    }
    return payload;
  }

  // src/internal/request-dispatcher.ts
  var SDK_BATCH_MAX_ITEM_COUNT = 100;
  var SDK_BATCH_MAX_BODY_BYTES = 256 * 1024;
  var AttriaxRequestDispatcher = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    createDynamicLink(payload) {
      return this.sendGeneratedRequest({
        payload,
        invoke: (body) => sdkControllerCreateDynamicLinkV1({
          body,
          client: this.options.sdkClient,
          throwOnError: true
        }),
        parse: parseDynamicLinkCreateResult
      });
    }
    validateRevenueReceipt(payload) {
      return this.sendGeneratedRequest({
        payload,
        invoke: (body) => sdkControllerValidateReceiptV1({
          body,
          client: this.options.sdkClient,
          throwOnError: true
        }),
        parse: parseRevenueReceiptValidationResult
      });
    }
    resolveDeepLink(payload) {
      return this.sendGeneratedRequest({
        payload,
        invoke: (body) => sdkControllerResolveDeepLinkV1({
          body,
          client: this.options.sdkClient,
          throwOnError: true
        }),
        parse: parseDeepLinkResolutionResult
      });
    }
    async performQueueEntry(entry) {
      switch (entry.kind) {
        case "open": {
          const openEntry = entry;
          const result = await this.sendGeneratedRequest({
            payload: openEntry.payload,
            invoke: (body) => sdkControllerOpenV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: parseAppOpenResult
          });
          this.options.handleDeferredAppOpen(result);
          return result;
        }
        case "event": {
          const eventEntry = entry;
          await this.sendGeneratedRequest({
            payload: eventEntry.payload,
            invoke: (body) => sdkControllerRecordEventV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: () => void 0
          });
          return void 0;
        }
        case "notification": {
          const notificationEntry = entry;
          await this.sendGeneratedRequest({
            payload: notificationEntry.payload,
            invoke: (body) => sdkControllerRecordNotificationV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: () => void 0
          });
          return void 0;
        }
        case "trackCrash": {
          const crashEntry = entry;
          await this.sendGeneratedRequest({
            payload: crashEntry.payload,
            invoke: (body) => sdkControllerRecordCrashV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: () => void 0
          });
          return void 0;
        }
        case "session": {
          const sessionEntry = entry;
          await this.sendGeneratedRequest({
            payload: sessionEntry.payload,
            invoke: (body) => sdkControllerRecordSessionV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: () => void 0
          });
          return void 0;
        }
        case "user": {
          const userEntry = entry;
          await this.sendGeneratedRequest({
            payload: mapUserRequestToWire(userEntry.payload),
            invoke: (body) => sdkControllerSetUserV1({
              body,
              client: this.options.sdkClient,
              throwOnError: true
            }),
            parse: () => void 0
          });
          return void 0;
        }
        case "deepLinkResolve": {
          const deepLinkEntry = entry;
          return this.resolveDeepLink(
            deepLinkEntry.payload
          );
        }
        default: {
          throw new AttriaxApiError(
            `Unsupported queue entry: ${entry.kind}`,
            void 0,
            false,
            true
          );
        }
      }
    }
    /**
     * Reports whether a single queued entry, sent alone, would exceed the batch
     * item-count or body-size caps and therefore can never fit any batch. This
     * deliberately ignores identity/keepalive errors (which are real send
     * failures handled by {@link sendBatchQueueEntries}) so the synchronizer only
     * evicts entries that are genuinely too large.
     */
    exceedsBatchSizeCap(entry) {
      try {
        const envelope = this.buildBatchEnvelope([entry]);
        return envelope.body.items.length > SDK_BATCH_MAX_ITEM_COUNT || envelope.encodedBody.length > SDK_BATCH_MAX_BODY_BYTES;
      } catch {
        return false;
      }
    }
    getSendableBatchEntries(entries) {
      let sendableCount = 0;
      for (let index = 0; index < entries.length; index += 1) {
        const candidate = entries.slice(0, index + 1);
        if (!this.fitsBatchEnvelope(candidate)) {
          break;
        }
        sendableCount = index + 1;
      }
      return entries.slice(0, sendableCount);
    }
    async sendBatchQueueEntries(entries) {
      const firstEntry = entries[0];
      if (!firstEntry) {
        throw new AttriaxApiError(
          "Attriax batch request requires at least one queued entry.",
          void 0,
          false,
          true
        );
      }
      if (!firstEntry.payload.deviceId) {
        throw new AttriaxApiError(
          "Anonymous SDK requests cannot be sent through the identified batch endpoint.",
          void 0,
          false,
          true
        );
      }
      this.assertSharedBatchIdentity(entries);
      const envelope = this.buildBatchEnvelope(entries);
      if (envelope.body.items.length > SDK_BATCH_MAX_ITEM_COUNT || envelope.encodedBody.length > SDK_BATCH_MAX_BODY_BYTES) {
        throw new AttriaxApiError(
          "Attriax batch request exceeds the supported payload size.",
          413,
          false,
          true
        );
      }
      await this.requestSdk(
        sdkControllerBatchV1({
          body: envelope.body,
          client: this.options.sdkClient,
          throwOnError: true
        })
      );
      if (envelope.keepAlive) {
        this.options.handleSessionKeepAliveDelivered?.(
          envelope.keepAlive.sessionId,
          envelope.keepAlive.occurredAt
        );
      }
    }
    async requestSdk(operation) {
      return unwrapSuccessEnvelope(await operation);
    }
    async sendGeneratedRequest({
      payload,
      invoke,
      parse
    }) {
      const response = await this.requestSdk(invoke(this.withProjectToken(payload)));
      return parse(response);
    }
    withProjectToken(payload) {
      return {
        projectToken: this.options.projectToken,
        ...payload
      };
    }
    assertSharedBatchIdentity(entries) {
      const firstEntry = entries[0];
      if (!firstEntry) {
        return;
      }
      for (const entry of entries.slice(1)) {
        if (!canShareBatchIdentity(firstEntry, entry)) {
          throw new AttriaxApiError(
            "Attriax batch entries must share the same device identity.",
            400,
            false,
            true
          );
        }
      }
    }
    fitsBatchEnvelope(entries) {
      if (entries.length === 0) {
        return false;
      }
      try {
        const envelope = this.buildBatchEnvelope(entries);
        return envelope.body.items.length <= SDK_BATCH_MAX_ITEM_COUNT && envelope.encodedBody.length <= SDK_BATCH_MAX_BODY_BYTES;
      } catch {
        return false;
      }
    }
    buildBatchEnvelope(entries) {
      const firstEntry = entries[0];
      if (!firstEntry) {
        throw new AttriaxApiError(
          "Attriax batch request requires at least one queued entry.",
          void 0,
          false,
          true
        );
      }
      const deviceId = firstEntry.payload.deviceId;
      if (!deviceId) {
        throw new AttriaxApiError(
          "Anonymous SDK requests cannot be sent through the identified batch endpoint.",
          void 0,
          false,
          true
        );
      }
      const keepAlive = this.options.getSessionKeepAliveBatchEntry?.(entries) ?? null;
      if (keepAlive && (keepAlive.payload.deviceId !== firstEntry.payload.deviceId || keepAlive.payload.deviceIdSource !== firstEntry.payload.deviceIdSource)) {
        throw new AttriaxApiError(
          "Attriax batch keepalive entry must share the same device identity.",
          400,
          false,
          true
        );
      }
      const body = {
        requestId: this.buildBatchRequestId(firstEntry.id),
        projectToken: this.options.projectToken,
        deviceId,
        ...firstEntry.payload.deviceIdSource ? { deviceIdSource: firstEntry.payload.deviceIdSource } : {},
        items: [
          ...entries.map((entry) => ({
            kind: entry.kind,
            body: this.stripBatchSharedIdentity(this.toBatchItemPayload(entry))
          })),
          ...keepAlive ? [
            {
              kind: "session",
              body: this.stripBatchSharedIdentity(keepAlive.payload)
            }
          ] : []
        ]
      };
      return {
        keepAlive,
        body,
        encodedBody: new TextEncoder().encode(JSON.stringify(body))
      };
    }
    // Normalize a queued entry to its wire shape before it goes into a batch
    // item body. Routes through the shared {@link toWireRequestBody} builder so
    // the batch item body carries exactly the same wire fields as the
    // single-request path. `user` entries store the public field names
    // (userId/userName/clearUser) which the builder maps to the wire DTO
    // (externalUserId/externalUserName/clearExternalUser); skipping that mapping
    // here is the batch-path drift the API rejects with HTTP 400.
    toBatchItemPayload(entry) {
      return toWireRequestBody(entry.kind, entry.payload);
    }
    stripBatchSharedIdentity(payload) {
      const {
        deviceId: _deviceId,
        deviceIdSource: _deviceIdSource,
        ...rest
      } = payload;
      return rest;
    }
    buildBatchRequestId(entryId) {
      return `batch_${entryId}`;
    }
  };

  // src/internal/request-manager.ts
  var AttriaxRequestManager = class {
    constructor() {
      __publicField(this, "synchronizer", null);
    }
    get isBound() {
      return this.synchronizer != null;
    }
    bindSynchronizer(synchronizer) {
      this.synchronizer = synchronizer;
    }
    enqueue(request, options = {}) {
      const synchronizer = this.synchronizer;
      if (!synchronizer) {
        return Promise.reject(
          new Error("Attriax request manager is not bound to a synchronizer.")
        );
      }
      return synchronizer.enqueue(request, {
        flushImmediately: options.flushImmediately
      });
    }
  };

  // src/internal/runtime-settings-store.ts
  var AttriaxRuntimeSettingsStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    readEnabled(defaultValue) {
      return this.options.storageManager.readBoolean("enabled") ?? defaultValue;
    }
    readEventsEnabled(defaultValue) {
      return this.options.storageManager.readBoolean("eventsEnabled") ?? defaultValue;
    }
    writeEnabled(enabled) {
      try {
        this.options.storageManager.writeBoolean("enabled", enabled);
      } catch (error) {
        this.options.logger.warning("Failed to persist the Attriax enabled preference.", {
          error
        });
      }
    }
    writeEventsEnabled(enabled) {
      try {
        this.options.storageManager.writeBoolean("eventsEnabled", enabled);
      } catch (error) {
        this.options.logger.warning("Failed to persist the Attriax event preference.", {
          error
        });
      }
    }
  };

  // src/internal/runtime-settings-state.ts
  var AttriaxRuntimeSettingsState = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "isEnabledValue", true);
      __publicField(this, "areEventsEnabledValue", true);
      __publicField(this, "requestedEnabledOverrideValue", null);
      __publicField(this, "requestedEventsEnabledOverrideValue", null);
    }
    get isEnabled() {
      return this.isEnabledValue;
    }
    get areEventsEnabled() {
      return this.areEventsEnabledValue;
    }
    get requestedEnabledOverride() {
      return this.requestedEnabledOverrideValue;
    }
    get requestedEventsEnabledOverride() {
      return this.requestedEventsEnabledOverrideValue;
    }
    restore({ enabled, eventsEnabled }) {
      this.isEnabledValue = enabled;
      this.areEventsEnabledValue = eventsEnabled;
      this.requestedEnabledOverrideValue = enabled;
      this.requestedEventsEnabledOverrideValue = eventsEnabled;
    }
    setEnabled({
      enabled,
      initialized,
      applyState,
      onPreparingToEnable
    }) {
      this.requestedEnabledOverrideValue = enabled;
      if (this.isEnabledValue === enabled && initialized) {
        this.persistEnabledPreference(enabled);
        return;
      }
      this.isEnabledValue = enabled;
      if (enabled) {
        onPreparingToEnable?.();
      }
      this.persistEnabledPreference(enabled);
      try {
        applyState(enabled);
      } catch (error) {
        this.options.logger.error("Failed to update Attriax enabled state.", { error });
        throw error;
      }
    }
    setEventsEnabled({ enabled }) {
      this.requestedEventsEnabledOverrideValue = enabled;
      this.areEventsEnabledValue = enabled;
      this.options.logger.verbose(`Attriax custom events ${enabled ? "enabled" : "disabled"}.`);
      this.persistEventsEnabledPreference(enabled);
    }
    persistEnabledPreference(enabled) {
      this.options.store.writeEnabled(enabled);
    }
    persistEventsEnabledPreference(enabled) {
      this.options.store.writeEventsEnabled(enabled);
    }
  };

  // src/internal/session-store.ts
  var AttriaxSessionStore = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    readSessionSnapshot() {
      return this.options.storageManager.readSessionSnapshot();
    }
    writeSessionSnapshot(session) {
      this.options.storageManager.writeSessionSnapshot(session);
    }
  };

  // src/internal/session-lifecycle-manager.ts
  var AttriaxSessionLifecycleManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "sessionHeartbeatTimeout", null);
      __publicField(this, "backgroundedValue", false);
    }
    get isBackgrounded() {
      return this.backgroundedValue;
    }
    set isBackgrounded(value) {
      this.backgroundedValue = value;
    }
    handleBrowserHidden() {
      if (!this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled || this.backgroundedValue) {
        return;
      }
      this.backgroundedValue = true;
      this.stopSessionHeartbeatTimer();
      const currentSession = this.options.sessionManager.recordExistingSessionActivity(/* @__PURE__ */ new Date());
      if (!currentSession) {
        return;
      }
      this.options.sessionManager.flushPendingRecoveredSessionEnd();
      this.options.sessionManager.queueSessionLifecycle(
        "pause",
        currentSession,
        currentSession.lastActivityAt
      );
    }
    handleBrowserVisible() {
      const wasBackgrounded = this.backgroundedValue;
      this.backgroundedValue = false;
      if (!this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled) {
        return;
      }
      const occurredAt = /* @__PURE__ */ new Date();
      const sessionResult = this.options.sessionManager.resumeOrStartSession(occurredAt);
      if (wasBackgrounded) {
        this.options.sessionManager.flushPendingRecoveredSessionEnd();
        this.options.sessionManager.queueSessionLifecycle(
          sessionResult.startedNewSession ? "start" : "resume",
          sessionResult.currentSession,
          sessionResult.startedNewSession ? sessionResult.currentSession.startedAt : occurredAt
        );
      }
      this.restartSessionHeartbeatTimer();
    }
    handlePageHide() {
      if (!this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled) {
        return;
      }
      this.backgroundedValue = true;
      this.stopSessionHeartbeatTimer();
      const endedSession = this.options.sessionManager.endCurrentSession(/* @__PURE__ */ new Date());
      if (!endedSession) {
        return;
      }
      this.options.sessionManager.flushPendingRecoveredSessionEnd();
      this.options.sessionManager.queueSessionLifecycle(
        "end",
        endedSession,
        endedSession.lastActivityAt
      );
    }
    restartSessionHeartbeatTimer() {
      this.stopSessionHeartbeatTimer();
      if (!this.options.isInitialized() || !this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled || this.backgroundedValue || !this.options.sessionManager.currentSession) {
        return;
      }
      const intervalMs = Math.max(
        this.options.sessionManager.currentSession.heartbeatIntervalMs,
        1e3
      );
      this.sessionHeartbeatTimeout = setTimeout(() => {
        this.sessionHeartbeatTimeout = null;
        this.sendSessionHeartbeat();
        this.restartSessionHeartbeatTimer();
      }, intervalMs);
    }
    stopSessionHeartbeatTimer() {
      if (this.sessionHeartbeatTimeout == null) {
        return;
      }
      clearTimeout(this.sessionHeartbeatTimeout);
      this.sessionHeartbeatTimeout = null;
    }
    handleSuccessfulForegroundFlush(sessionId, occurredAt) {
      if (!this.options.isInitialized() || !this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled || this.backgroundedValue) {
        return;
      }
      const currentSession = this.options.sessionManager.currentSession;
      if (!currentSession || currentSession.id !== sessionId) {
        return;
      }
      this.options.sessionManager.recordExistingSessionActivity(occurredAt);
      this.restartSessionHeartbeatTimer();
    }
    sendSessionHeartbeat() {
      if (!this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled || this.backgroundedValue) {
        return;
      }
      const occurredAt = /* @__PURE__ */ new Date();
      const sessionResult = this.options.sessionManager.resumeOrStartSession(occurredAt);
      this.options.sessionManager.flushPendingRecoveredSessionEnd();
      this.options.sessionManager.queueSessionLifecycle(
        sessionResult.startedNewSession ? "start" : "heartbeat",
        sessionResult.currentSession,
        sessionResult.startedNewSession ? sessionResult.currentSession.startedAt : occurredAt
      );
    }
  };

  // src/internal/session-manager.ts
  var ATTRIAX_MIN_SESSION_CONTINUATION_WINDOW_MS = 6e4;
  var ATTRIAX_MAX_SESSION_CONTINUATION_WINDOW_MS = 30 * 6e4;
  function attriaxSessionContinuationWindowMs(heartbeatIntervalMs) {
    const raw = Math.max(heartbeatIntervalMs, 1e3) * 2;
    return Math.min(
      ATTRIAX_MAX_SESSION_CONTINUATION_WINDOW_MS,
      Math.max(ATTRIAX_MIN_SESSION_CONTINUATION_WINDOW_MS, raw)
    );
  }
  var AttriaxSessionManager = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "currentSessionValue", null);
      __publicField(this, "pendingRecoveredSessionEndValue", null);
    }
    get currentSession() {
      return this.currentSessionValue;
    }
    set currentSession(value) {
      this.currentSessionValue = value;
    }
    get pendingRecoveredSessionEnd() {
      return this.pendingRecoveredSessionEndValue;
    }
    set pendingRecoveredSessionEnd(value) {
      this.pendingRecoveredSessionEndValue = value;
    }
    clear() {
      this.currentSessionValue = null;
      this.pendingRecoveredSessionEndValue = null;
      this.options.store.writeSessionSnapshot(null);
    }
    syncCurrentSessionContext() {
      const currentSession = this.currentSessionValue;
      const snapshot = this.options.contextManager.snapshot;
      if (!currentSession || !snapshot || !snapshot.deviceId) {
        return;
      }
      if (currentSession.deviceId === snapshot.deviceId) {
        return;
      }
      this.currentSessionValue = {
        ...currentSession,
        deviceId: snapshot.deviceId,
        locale: snapshot.device.language ?? currentSession.locale,
        appVersion: snapshot.app.version ?? currentSession.appVersion,
        appBuildNumber: snapshot.app.buildNumber ?? currentSession.appBuildNumber,
        appPackageName: snapshot.app.packageName ?? currentSession.appPackageName
      };
      this.options.store.writeSessionSnapshot(this.currentSessionValue);
    }
    restoreOrStartSession(at) {
      const restored = this.options.store.readSessionSnapshot();
      return this.activateSession(restored, at);
    }
    resumeOrStartSession(at) {
      const existingSession = this.currentSessionValue ?? this.options.store.readSessionSnapshot();
      return this.activateSession(existingSession, at);
    }
    recordExistingSessionActivity(at) {
      if (!this.currentSessionValue) {
        return null;
      }
      if (at.getTime() > this.currentSessionValue.lastActivityAt.getTime()) {
        this.currentSessionValue = {
          ...this.currentSessionValue,
          lastActivityAt: at
        };
        this.options.store.writeSessionSnapshot(this.currentSessionValue);
      }
      return this.currentSessionValue;
    }
    endCurrentSession(at) {
      const currentSession = this.recordExistingSessionActivity(at);
      this.currentSessionValue = null;
      this.options.store.writeSessionSnapshot(null);
      return currentSession;
    }
    flushPendingRecoveredSessionEnd() {
      const pendingSession = this.pendingRecoveredSessionEndValue;
      if (!pendingSession || !this.options.isEnabled() || !this.options.canTrackSessions() || !this.options.config.sessionTrackingEnabled) {
        return;
      }
      this.pendingRecoveredSessionEndValue = null;
      this.queueSessionLifecycle("end", pendingSession, this.inferSessionEndAt(pendingSession), {
        recovered: true
      });
    }
    inferSessionEndAt(session) {
      return new Date(
        session.lastActivityAt.getTime() + attriaxSessionContinuationWindowMs(session.heartbeatIntervalMs)
      );
    }
    queueSessionLifecycle(kind, session, occurredAt, metadata) {
      if (!this.options.config.sessionTrackingEnabled || !this.options.canTrackSessions()) {
        return;
      }
      const request = this.buildSessionRequest(kind, session, occurredAt, metadata);
      void this.options.requestManager.enqueue({ kind: "session", payload: request }).catch((error) => {
        if (error instanceof AttriaxApiError && error.shouldDrop && error.message === "Attriax instance was disposed before queued work completed.") {
          this.options.logger.verbose("Dropping session lifecycle request because the Attriax instance was disposed.");
          return;
        }
        this.options.logger.error("Session lifecycle queueing failed.", { error });
      });
    }
    prepareTrackedSessionActivity(at) {
      if (!this.options.config.sessionTrackingEnabled || !this.options.canTrackSessions()) {
        return null;
      }
      const sessionResult = this.resumeOrStartSession(at);
      this.flushPendingRecoveredSessionEnd();
      if (sessionResult.startedNewSession) {
        this.queueSessionLifecycle(
          "start",
          sessionResult.currentSession,
          sessionResult.currentSession.startedAt
        );
      }
      return sessionResult.currentSession;
    }
    buildHeartbeatKeepAliveRequest(session, occurredAt) {
      return this.buildSessionRequest("heartbeat", session, occurredAt);
    }
    activateSession(existingSession, at) {
      if (existingSession && this.shouldContinueSession(existingSession, at)) {
        const currentSession2 = {
          ...existingSession,
          lastActivityAt: at.getTime() > existingSession.lastActivityAt.getTime() ? at : existingSession.lastActivityAt
        };
        this.currentSessionValue = currentSession2;
        this.options.store.writeSessionSnapshot(currentSession2);
        return {
          currentSession: currentSession2,
          startedNewSession: false,
          replacedSession: null
        };
      }
      const currentSession = this.buildNewSession(at);
      this.currentSessionValue = currentSession;
      this.options.store.writeSessionSnapshot(currentSession);
      if (existingSession) {
        this.pendingRecoveredSessionEndValue = existingSession;
      }
      return {
        currentSession,
        startedNewSession: true,
        replacedSession: existingSession
      };
    }
    buildNewSession(at) {
      const snapshot = this.options.contextManager.requireSnapshot();
      return {
        id: generateId(),
        deviceId: snapshot.deviceId,
        platform: snapshot.platform,
        locale: snapshot.device.language,
        isFirstLaunch: this.options.contextManager.isFirstLaunch,
        startedAt: at,
        lastActivityAt: at,
        heartbeatIntervalMs: this.options.contextManager.isFirstLaunch ? this.options.config.firstLaunchSessionHeartbeatIntervalMs : this.options.config.sessionHeartbeatIntervalMs,
        appVersion: snapshot.app.version,
        appBuildNumber: snapshot.app.buildNumber,
        appPackageName: snapshot.app.packageName,
        sdkPackageVersion: attriaxSdkPackageVersion
      };
    }
    shouldContinueSession(session, at) {
      if (session.deviceId !== this.options.contextManager.deviceId || session.platform !== this.options.config.platform) {
        return false;
      }
      const app = this.options.contextManager.snapshot?.app;
      if (session.appPackageName !== app?.packageName || session.appVersion !== app?.version || session.appBuildNumber !== app?.buildNumber) {
        return false;
      }
      if (at.getTime() < session.startedAt.getTime()) {
        return false;
      }
      const continuationWindowMs = attriaxSessionContinuationWindowMs(
        session.heartbeatIntervalMs
      );
      return at.getTime() - session.lastActivityAt.getTime() <= continuationWindowMs;
    }
    buildSessionRequest(kind, session, occurredAt, metadata) {
      const request = {
        platform: this.options.config.platform,
        ...this.options.trackingDecision().attachDeviceIdentity ? {
          deviceId: session.deviceId ?? this.options.contextManager.requireDeviceId(),
          deviceIdSource: this.options.contextManager.requireDeviceIdSource()
        } : {},
        sessionId: session.id,
        kind,
        sessionRelativeTimeMs: this.getSessionRelativeTimeMs(session, occurredAt),
        clientOccurredAt: occurredAt.toISOString(),
        sdkApiVersion: attriaxSdkApiVersion,
        sdkPackageVersion: session.sdkPackageVersion
      };
      if (session.locale) {
        request.locale = session.locale;
      }
      if (session.appVersion) {
        request.appVersion = session.appVersion;
      }
      if (session.appBuildNumber) {
        request.appBuildNumber = session.appBuildNumber;
      }
      if (session.appPackageName) {
        request.appPackageName = session.appPackageName;
      }
      const normalizedMetadata = normalizeJsonObject2(metadata);
      if (normalizedMetadata) {
        request.metadata = normalizedMetadata;
      }
      return request;
    }
    getSessionRelativeTimeMs(session, occurredAt) {
      return Math.max(0, occurredAt.getTime() - session.startedAt.getTime());
    }
  };
  function normalizeJsonObject2(value) {
    if (!value) {
      return void 0;
    }
    const normalizedEntries = Object.entries(value).filter(([, entry]) => entry !== void 0);
    if (normalizedEntries.length === 0) {
      return void 0;
    }
    return Object.fromEntries(normalizedEntries);
  }

  // src/internal/storage-manager.ts
  function isSdkPlatform(value) {
    return value === "web" || value === "ios" || value === "android";
  }
  var _AttriaxStorageManager = class _AttriaxStorageManager {
    constructor(options) {
      __publicField(this, "storage");
      __publicField(this, "storageKeyPrefix");
      __publicField(this, "storageNamespace");
      __publicField(this, "logger");
      __publicField(this, "onQueueEntryDropped");
      __publicField(this, "memoryValues", /* @__PURE__ */ new Map());
      __publicField(this, "runtimePersistenceMode", "fullRuntime");
      this.storage = options.storage;
      this.storageKeyPrefix = options.storageKeyPrefix;
      this.storageNamespace = buildStorageNamespace(options.projectToken);
      this.logger = options.logger;
      this.onQueueEntryDropped = options.onQueueEntryDropped;
    }
    readValue(name) {
      if (this.memoryValues.has(name)) {
        return this.memoryValues.get(name) ?? null;
      }
      if (!this.canUsePersistentStorageForKey(name)) {
        return null;
      }
      return this.storage.getItem(this.key(name));
    }
    writeValue(name, value) {
      this.memoryValues.set(name, value);
      if (!this.canUsePersistentStorageForKey(name)) {
        this.removePersistedValue(name);
        return;
      }
      this.storage.setItem(this.key(name), value);
    }
    removeValue(name) {
      this.memoryValues.delete(name);
      this.removePersistedValue(name);
    }
    readBoolean(name) {
      const value = this.readValue(name);
      if (value === "true") {
        return true;
      }
      if (value === "false") {
        return false;
      }
      return null;
    }
    writeBoolean(name, value) {
      this.writeValue(name, String(value));
    }
    resolveInitialRuntimePersistenceMode({ gdprEnabled }) {
      if (!gdprEnabled) {
        return "fullRuntime";
      }
      const raw = this.storage.getItem(this.key("gdprConsent"));
      if (!raw) {
        return "consentOnly";
      }
      try {
        const parsed = JSON.parse(raw);
        if (parsed.state === "not_required") {
          return "fullRuntime";
        }
        if (parsed.state === "granted" && parsed.values != null && (parsed.values.analytics === true || parsed.values.attribution === true || parsed.values.adEvents === true)) {
          return "fullRuntime";
        }
      } catch (error) {
        this.logger?.warning("Failed to parse stored GDPR consent while resolving runtime persistence mode.", {
          error
        });
      }
      return "consentOnly";
    }
    setRuntimePersistenceMode({ mode }) {
      if (this.runtimePersistenceMode === mode) {
        return;
      }
      this.runtimePersistenceMode = mode;
      if (mode === "fullRuntime") {
        this.syncMemoryValuesToPersistentStorage();
        return;
      }
      this.clearRuntimeScopedPersistentStorage();
    }
    readSessionSnapshot() {
      const raw = this.readValue("session");
      if (!raw) {
        return null;
      }
      try {
        const parsed = JSON.parse(raw);
        if (typeof parsed.id !== "string" || !(parsed.deviceId == null || typeof parsed.deviceId === "string") || !isSdkPlatform(parsed.platform) || typeof parsed.isFirstLaunch !== "boolean" || typeof parsed.startedAt !== "string" || typeof parsed.lastActivityAt !== "string" || typeof parsed.heartbeatIntervalMs !== "number" || !Number.isFinite(parsed.heartbeatIntervalMs) || parsed.heartbeatIntervalMs <= 0 || typeof parsed.sdkPackageVersion !== "string") {
          return null;
        }
        const startedAt = new Date(parsed.startedAt);
        const lastActivityAt = new Date(parsed.lastActivityAt);
        if (Number.isNaN(startedAt.getTime()) || Number.isNaN(lastActivityAt.getTime()) || lastActivityAt.getTime() < startedAt.getTime()) {
          return null;
        }
        return {
          id: parsed.id,
          deviceId: typeof parsed.deviceId === "string" ? parsed.deviceId : null,
          platform: parsed.platform,
          locale: typeof parsed.locale === "string" ? parsed.locale : void 0,
          isFirstLaunch: parsed.isFirstLaunch,
          startedAt,
          lastActivityAt,
          heartbeatIntervalMs: parsed.heartbeatIntervalMs,
          appVersion: typeof parsed.appVersion === "string" ? parsed.appVersion : void 0,
          appBuildNumber: typeof parsed.appBuildNumber === "string" ? parsed.appBuildNumber : void 0,
          appPackageName: typeof parsed.appPackageName === "string" ? parsed.appPackageName : void 0,
          sdkPackageVersion: parsed.sdkPackageVersion
        };
      } catch (error) {
        this.logger?.warning("Failed to parse session snapshot from storage. Resetting it.", {
          error
        });
        return null;
      }
    }
    writeSessionSnapshot(session) {
      if (!session) {
        this.removeValue("session");
        return;
      }
      this.writeValue(
        "session",
        JSON.stringify({
          id: session.id,
          deviceId: session.deviceId,
          platform: session.platform,
          locale: session.locale,
          isFirstLaunch: session.isFirstLaunch,
          startedAt: session.startedAt.toISOString(),
          lastActivityAt: session.lastActivityAt.toISOString(),
          heartbeatIntervalMs: session.heartbeatIntervalMs,
          appVersion: session.appVersion,
          appBuildNumber: session.appBuildNumber,
          appPackageName: session.appPackageName,
          sdkPackageVersion: session.sdkPackageVersion
        })
      );
    }
    readQueue() {
      const raw = this.readValue("queue");
      if (!raw) {
        return [];
      }
      try {
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed)) {
          this.logger?.warning("Discarding legacy unversioned queue payload.");
          return [];
        }
        if (parsed && typeof parsed === "object" && parsed.version === _AttriaxStorageManager.QUEUE_SCHEMA_VERSION && Array.isArray(parsed.entries)) {
          const entries = parsed.entries;
          return entries.map(
            (entry) => entry.kind === "identify" ? {
              ...entry,
              kind: "user"
            } : entry
          );
        }
        this.logger?.warning("Discarding queue payload with unsupported schema version.");
        return [];
      } catch (error) {
        this.logger?.warning("Failed to parse queue from storage. Resetting the persisted queue.", {
          error
        });
        return [];
      }
    }
    writeQueue(queue) {
      const key = this.key("queue");
      const serialize = () => JSON.stringify({ version: _AttriaxStorageManager.QUEUE_SCHEMA_VERSION, entries: queue });
      let serialized = serialize();
      const syncQueueMemory = () => {
        if (queue.length === 0) {
          this.memoryValues.delete("queue");
          return;
        }
        this.memoryValues.set("queue", serialized);
      };
      syncQueueMemory();
      if (!this.canUsePersistentStorageForKey("queue")) {
        this.removePersistedValue("queue");
        return;
      }
      try {
        this.storage.setItem(key, serialized);
        return;
      } catch (error) {
        if (!this.isQuotaExceededError(error)) {
          this.logger?.warning("Failed to persist queue to storage.", { error });
          return;
        }
      }
      while (queue.length > 0) {
        const dropped = queue.shift();
        if (dropped) {
          this.onQueueEntryDropped?.(dropped);
        }
        serialized = serialize();
        syncQueueMemory();
        try {
          this.storage.setItem(key, serialized);
          this.logger?.warning("Queue persisted after evicting oldest entries due to storage quota.");
          return;
        } catch (error) {
          if (!this.isQuotaExceededError(error)) {
            this.logger?.warning("Failed to persist queue after eviction.", { error });
            return;
          }
        }
      }
      try {
        this.memoryValues.delete("queue");
        this.storage.removeItem(key);
      } catch {
      }
    }
    buildQuotaExceededError() {
      return new AttriaxApiError(
        "Attriax queue entry was dropped because storage quota was exceeded.",
        void 0,
        false,
        true
      );
    }
    clearAll() {
      for (const name of _AttriaxStorageManager.allStorageKeys) {
        this.removeValue(name);
      }
    }
    key(name) {
      return `${this.storageKeyPrefix}:${this.storageNamespace}:${name}`;
    }
    canUsePersistentStorageForKey(name) {
      if (!_AttriaxStorageManager.runtimeScopedStorageKeys.has(name)) {
        return true;
      }
      return this.runtimePersistenceMode === "fullRuntime";
    }
    clearRuntimeScopedPersistentStorage() {
      for (const name of _AttriaxStorageManager.runtimeScopedStorageKeys) {
        this.removePersistedValue(name);
      }
    }
    syncMemoryValuesToPersistentStorage() {
      for (const name of _AttriaxStorageManager.runtimeScopedStorageKeys) {
        if (!this.memoryValues.has(name)) {
          continue;
        }
        const value = this.memoryValues.get(name);
        if (value == null) {
          this.removePersistedValue(name);
          continue;
        }
        this.storage.setItem(this.key(name), value);
      }
    }
    removePersistedValue(name) {
      this.storage.removeItem(this.key(name));
    }
    isQuotaExceededError(error) {
      if (!error || typeof error !== "object") {
        return false;
      }
      const e = error;
      return e.name === "QuotaExceededError" || e.name === "NS_ERROR_DOM_QUOTA_REACHED" || e.code === 22 || e.code === 1014;
    }
  };
  __publicField(_AttriaxStorageManager, "QUEUE_SCHEMA_VERSION", 1);
  __publicField(_AttriaxStorageManager, "runtimeScopedStorageKeys", /* @__PURE__ */ new Set([
    "deviceId",
    "deviceIdSource",
    "enabled",
    "eventsEnabled",
    "originalInstallReferrer",
    "originalInstallReferrerLoaded",
    "reinstallReferrer",
    "reinstallReferrerLoaded",
    "installReferrer",
    "installReferrerLoaded",
    "queue",
    "session"
  ]));
  __publicField(_AttriaxStorageManager, "consentScopedStorageKeys", /* @__PURE__ */ new Set([
    "gdprConsent",
    "gdprConsentId",
    "hasLaunched"
  ]));
  __publicField(_AttriaxStorageManager, "allStorageKeys", /* @__PURE__ */ new Set([
    ..._AttriaxStorageManager.runtimeScopedStorageKeys,
    ..._AttriaxStorageManager.consentScopedStorageKeys
  ]));
  var AttriaxStorageManager = _AttriaxStorageManager;
  function buildStorageNamespace(projectToken) {
    let hash = 2166136261;
    for (let index = 0; index < projectToken.length; index += 1) {
      hash ^= projectToken.charCodeAt(index);
      hash = Math.imul(hash, 16777619);
    }
    return (hash >>> 0).toString(16).padStart(8, "0");
  }

  // src/internal/synchronizer.ts
  var ATTRIAX_RETRY_BASE_BACKOFF_MS = 2e3;
  var ATTRIAX_RETRY_MAX_BACKOFF_MS = 5 * 6e4;
  function attriaxRetryBackoffMs(attemptCount) {
    const exponent = Math.min(Math.max(attemptCount - 1, 0), 20);
    const scaled = ATTRIAX_RETRY_BASE_BACKOFF_MS * 2 ** exponent;
    return Math.min(ATTRIAX_RETRY_MAX_BACKOFF_MS, scaled);
  }
  var AttriaxSynchronizer = class {
    constructor(options) {
      __publicField(this, "options", options);
      __publicField(this, "pendingRequests", /* @__PURE__ */ new Map());
      __publicField(this, "flushPromise", null);
      __publicField(this, "deferredFlushTimer", null);
      __publicField(this, "queueValue");
      __publicField(this, "synchronizationStateValue", "initializing" /* Initializing */);
      __publicField(this, "retryAttempt", 0);
      __publicField(this, "flushRequested", false);
      this.queueValue = options.queueStore.readQueue();
    }
    get queue() {
      return this.queueValue;
    }
    set queue(value) {
      this.queueValue = value;
    }
    get synchronizationState() {
      return this.synchronizationStateValue;
    }
    setState(value) {
      if (this.synchronizationStateValue === value) {
        return;
      }
      this.synchronizationStateValue = value;
      this.options.onStateChange(value);
    }
    async flush() {
      this.clearDeferredFlushTimer();
      this.flushRequested = true;
      if (this.flushPromise) {
        return this.flushPromise;
      }
      this.flushPromise = this.drainFlushQueue();
      return this.flushPromise;
    }
    /**
     * Drains the queue across repeated passes until no flush is pending. Each pass
     * works from a snapshot, so anything enqueued during a pass's network awaits
     * (or a connectivity change that arrived mid-pass) is delivered by a following
     * pass instead of being stranded behind the `flushPromise` dedup guard.
     *
     * `flushPromise` is cleared in a synchronous `finally` with no `await` between
     * the loop's final `flushRequested` check and the clear, so a concurrent
     * `flush()` either re-arms this loop or starts a fresh one — never both, never
     * neither.
     */
    async drainFlushQueue() {
      try {
        while (this.flushRequested) {
          this.flushRequested = false;
          await this.flushInternal();
          if (this.synchronizationStateValue === "synchronized" /* Synchronized */ && this.queueValue.length > 0) {
            this.flushRequested = true;
          }
        }
      } finally {
        this.flushPromise = null;
      }
    }
    dispose(reason) {
      this.clearDeferredFlushTimer();
      for (const pendingId of Array.from(this.pendingRequests.keys())) {
        this.rejectPending(pendingId, reason);
      }
    }
    async reset(reason) {
      this.clearDeferredFlushTimer();
      const inFlight = this.flushPromise;
      if (inFlight) {
        await inFlight.catch(() => void 0);
      }
      for (const pendingId of Array.from(this.pendingRequests.keys())) {
        this.rejectPending(pendingId, reason);
      }
      this.queueValue = [];
      this.options.queueStore.writeQueue([]);
      this.retryAttempt = 0;
      this.setState("initializing" /* Initializing */);
    }
    discardQueuedEntriesWhere(predicate, reason) {
      const retained = [];
      for (const entry of this.queueValue) {
        if (predicate(entry)) {
          this.rejectPending(entry.id, reason);
          continue;
        }
        retained.push(entry);
      }
      if (retained.length === this.queueValue.length) {
        return;
      }
      this.queueValue = retained;
      this.options.queueStore.writeQueue(this.queueValue);
    }
    rewriteQueuedEntriesWhere(rewrite) {
      let rewriteCount = 0;
      const rewritten = this.queueValue.map((entry) => {
        const replacement = rewrite(entry);
        if (!replacement) {
          return entry;
        }
        rewriteCount += 1;
        return replacement;
      });
      if (rewriteCount > 0) {
        this.queueValue = rewritten;
        this.options.queueStore.writeQueue(this.queueValue);
      }
      return rewriteCount;
    }
    enqueue(request, options = {}) {
      const entry = {
        id: generateId(),
        kind: request.kind,
        createdAt: (/* @__PURE__ */ new Date()).toISOString(),
        payload: request.payload
      };
      this.queueValue.push(entry);
      while (this.queueValue.length > this.options.maxQueueSize) {
        const dropped = this.queueValue.shift();
        if (dropped) {
          this.recordEviction(
            dropped.id,
            "Attriax queue entry was dropped because the queue reached capacity."
          );
        }
      }
      this.options.queueStore.writeQueue(this.queueValue);
      const resultPromise = new Promise((resolve, reject) => {
        this.pendingRequests.set(entry.id, {
          resolve,
          reject
        });
      });
      this.setState("synchronizing" /* Synchronizing */);
      if ((options.flushImmediately ?? true) || this.options.eventFlushIntervalMs === 0) {
        this.setState("synchronizing" /* Synchronizing */);
        void this.flush();
      } else {
        this.setState("deferred" /* Deferred */);
        this.scheduleDeferredFlush();
      }
      return resultPromise;
    }
    scheduleDeferredFlush() {
      if (this.deferredFlushTimer != null) {
        return;
      }
      this.deferredFlushTimer = globalThis.setTimeout(() => {
        this.deferredFlushTimer = null;
        void this.flush();
      }, this.options.eventFlushIntervalMs);
    }
    /**
     * Self-schedules the next flush after a retriable failure using the server's
     * `Retry-After` when present, otherwise a jittered exponential backoff. This
     * replaces the previous fixed-interval re-flush so transient failures back off
     * with increasing spacing instead of spinning against a failing server (B1).
     */
    scheduleRetryFlush(error) {
      this.retryAttempt += 1;
      const delayMs = this.resolveRetryDelayMs(error);
      this.clearDeferredFlushTimer();
      this.deferredFlushTimer = globalThis.setTimeout(() => {
        this.deferredFlushTimer = null;
        void this.flush();
      }, delayMs);
    }
    resolveRetryDelayMs(error) {
      const retryAfterMs = error instanceof AttriaxApiError && typeof error.retryAfterMs === "number" ? error.retryAfterMs : void 0;
      if (retryAfterMs != null && retryAfterMs > 0) {
        return retryAfterMs;
      }
      const backoffMs = attriaxRetryBackoffMs(this.retryAttempt);
      const jitterMs = Math.floor(Math.random() * backoffMs * 0.2);
      return backoffMs + jitterMs;
    }
    clearDeferredFlushTimer() {
      if (this.deferredFlushTimer == null) {
        return;
      }
      globalThis.clearTimeout(this.deferredFlushTimer);
      this.deferredFlushTimer = null;
    }
    async flushInternal() {
      if (this.options.isDisposed() || !this.options.isEnabled()) {
        this.setState("disabled" /* Disabled */);
        return;
      }
      if (!this.options.canDispatchQueue()) {
        this.setState("deferred" /* Deferred */);
        return;
      }
      if (!this.options.isOnline()) {
        this.setState("offline" /* Offline */);
        return;
      }
      if (this.queueValue.length === 0) {
        this.setState("synchronized" /* Synchronized */);
        return;
      }
      this.setState("synchronizing" /* Synchronizing */);
      const queue = this.prioritizedQueueSnapshot();
      const remaining = [];
      const flush = {
        retriableError: null,
        madeProgress: false,
        hadHardFailure: false
      };
      for (let index = 0; index < queue.length; ) {
        const entry = queue[index];
        if (!entry) {
          index += 1;
          continue;
        }
        if (isBatchableQueueEntry(entry)) {
          const { batch, dropped } = this.collectSendableBatch(queue, index);
          if (dropped) {
            flush.madeProgress = true;
            index += 1;
            continue;
          }
          const keep = await this.dispatchBatch(batch, flush);
          remaining.push(...keep);
          if (keep.length < batch.length) {
            flush.madeProgress = true;
          }
          index += batch.length;
          continue;
        }
        const kept = await this.dispatchSingleEntry(entry, flush);
        if (kept) {
          remaining.push(entry);
        }
        index += 1;
      }
      const settledIds = new Set(queue.map((entry) => entry.id));
      for (const entry of remaining) {
        settledIds.delete(entry.id);
      }
      if (settledIds.size > 0) {
        this.queueValue = this.queueValue.filter((entry) => !settledIds.has(entry.id));
        this.options.queueStore.writeQueue(this.queueValue);
      }
      if (!this.options.isOnline()) {
        this.setState("offline" /* Offline */);
        return;
      }
      if (flush.retriableError !== null) {
        if (flush.madeProgress) {
          this.retryAttempt = 0;
        }
        this.scheduleRetryFlush(flush.retriableError);
        this.setState("deferred" /* Deferred */);
        return;
      }
      if (flush.hadHardFailure) {
        this.setState("failed" /* Failed */);
        return;
      }
      this.retryAttempt = 0;
      this.setState("synchronized" /* Synchronized */);
    }
    /**
     * Sends a single queued entry. Returns `true` when the entry should be kept
     * (retriable failure) or `false` when it is settled (delivered, dropped, or
     * permanently failed). A retriable failure never halts the flush.
     */
    async dispatchSingleEntry(entry, flush) {
      try {
        const result = await this.options.performQueueEntry(entry);
        this.resolvePending(entry.id, result);
        flush.madeProgress = true;
        return false;
      } catch (error) {
        if (error instanceof AttriaxApiError && error.shouldDrop) {
          this.rejectPending(entry.id, error);
          flush.madeProgress = true;
          return false;
        }
        if (error instanceof AttriaxApiError && error.retriable) {
          flush.retriableError = error;
          this.options.logger.warning(
            "Retryable Attriax queue request failed; keeping queued payload.",
            { error }
          );
          return true;
        }
        this.rejectPending(entry.id, error);
        flush.hadHardFailure = true;
        return false;
      }
    }
    /**
     * Sends a batch and returns the entries that should be kept for a later retry
     * (empty when every item was delivered or dropped). On a non-retryable batch
     * failure the batch is split in half and each half is dispatched
     * independently, so an oversized batch shrinks until each item is accepted.
     */
    async dispatchBatch(entries, flush) {
      try {
        await this.options.sendBatchQueueEntries(entries);
        for (const entry of entries) {
          this.resolvePending(entry.id, void 0);
        }
        return [];
      } catch (error) {
        const normalizedError = normalizeTransportError(error);
        if (normalizedError.retriable) {
          flush.retriableError = normalizedError;
          this.options.logger.warning(
            "Retryable Attriax batch request failed; keeping queued payload.",
            { error: normalizedError }
          );
          return entries;
        }
        if (entries.length > 1) {
          const splitIndex = Math.floor(entries.length / 2);
          this.options.logger.warning(
            "Attriax batch request failed; splitting queued payload.",
            { error: normalizedError }
          );
          const firstKeep = await this.dispatchBatch(entries.slice(0, splitIndex), flush);
          if (firstKeep.length > 0) {
            return [...firstKeep, ...entries.slice(splitIndex)];
          }
          return this.dispatchBatch(entries.slice(splitIndex), flush);
        }
        const singleEntry = entries[0];
        if (!singleEntry) {
          return [];
        }
        this.rejectPending(singleEntry.id, normalizedError);
        if (!normalizedError.shouldDrop) {
          flush.hadHardFailure = true;
        }
        return [];
      }
    }
    prioritizedQueueSnapshot() {
      const appOpenEntries = [];
      const otherEntries = [];
      for (const entry of this.queueValue) {
        if (entry.kind === "open") {
          appOpenEntries.push(entry);
        } else {
          otherEntries.push(entry);
        }
      }
      if (appOpenEntries.length === 0) {
        return [...otherEntries];
      }
      return [...appOpenEntries, ...otherEntries];
    }
    collectSendableBatch(queue, startIndex) {
      const entries = [];
      for (let index = startIndex; index < queue.length; index += 1) {
        const entry = queue[index];
        if (!entry || !isBatchableQueueEntry(entry)) {
          break;
        }
        const firstEntry = entries[0];
        if (firstEntry && !canShareBatchIdentity(firstEntry, entry)) {
          break;
        }
        entries.push(entry);
      }
      const sendableEntries = this.options.getSendableBatchEntries(entries);
      if (sendableEntries.length > 0) {
        return { batch: sendableEntries, dropped: false };
      }
      const undeliverable = entries[0];
      if (undeliverable && this.options.exceedsBatchSizeCap(undeliverable)) {
        this.options.logger.warning(
          `Attriax queue entry exceeds the batch payload cap and was dropped (kind=${undeliverable.kind}, id=${undeliverable.id}).`
        );
        this.recordEviction(
          undeliverable.id,
          "Attriax queue entry was dropped because it exceeds the batch payload size cap."
        );
        return { batch: [], dropped: true };
      }
      return { batch: entries.slice(0, 1), dropped: false };
    }
    resolvePending(id, value) {
      const pending = this.pendingRequests.get(id);
      if (!pending) {
        return;
      }
      this.pendingRequests.delete(id);
      pending.resolve(value);
    }
    rejectPending(id, reason) {
      const pending = this.pendingRequests.get(id);
      if (!pending) {
        return;
      }
      this.pendingRequests.delete(id);
      pending.reject(reason);
    }
    /**
     * Records that a queued entry was evicted (never delivered) through the same
     * rejection path FIFO overflow uses, so observability stays consistent across
     * capacity-based and size-based drops. The caller is responsible for removing
     * the entry from {@link queueValue} and persisting the result.
     */
    recordEviction(id, message) {
      this.rejectPending(
        id,
        new AttriaxApiError(message, void 0, false, true)
      );
    }
  };
  function isBatchableQueueEntry(entry) {
    if (entry.kind === "user") {
      return true;
    }
    return (entry.kind === "event" || entry.kind === "session") && typeof entry.payload.deviceId === "string";
  }

  // src/internal/user-properties.ts
  var MAX_USER_PROPERTY_KEYS = 30;
  var MAX_USER_PROPERTY_VALUE_LENGTH = 256;
  function sanitizeUserPropertyUpdate(input) {
    const properties = {};
    const clearPropertyKeys = [];
    for (const [rawKey, rawValue] of Object.entries(input)) {
      const normalizedKey = rawKey.trim();
      if (!normalizedKey) {
        continue;
      }
      if (rawValue === null) {
        delete properties[normalizedKey];
        if (!clearPropertyKeys.includes(normalizedKey)) {
          clearPropertyKeys.push(normalizedKey);
        }
        continue;
      }
      const normalizedValue = sanitizeUserPropertyValue(rawValue);
      if (normalizedValue === void 0) {
        continue;
      }
      const clearIndex = clearPropertyKeys.indexOf(normalizedKey);
      if (clearIndex >= 0) {
        clearPropertyKeys.splice(clearIndex, 1);
      }
      if (!(normalizedKey in properties) && Object.keys(properties).length >= MAX_USER_PROPERTY_KEYS) {
        continue;
      }
      properties[normalizedKey] = normalizedValue;
    }
    return {
      properties,
      clearPropertyKeys
    };
  }
  function sanitizeUserPropertyValue(value) {
    switch (typeof value) {
      case "string":
        return value.length <= MAX_USER_PROPERTY_VALUE_LENGTH ? value : value.slice(0, MAX_USER_PROPERTY_VALUE_LENGTH);
      case "number":
        return Number.isFinite(value) ? value : void 0;
      case "boolean":
        return value;
      default:
        return void 0;
    }
  }

  // src/internal/tracking-manager.ts
  var AttriaxTrackingManager = class {
    constructor(options) {
      __publicField(this, "options", options);
    }
    async recordEvent(eventName, options = {}) {
      if (!this.options.settingsState.isEnabled || !this.options.settingsState.areEventsEnabled) {
        this.options.logger.verbose(
          `Ignoring recordEvent("${eventName}") because SDK or events are disabled.`
        );
        return;
      }
      const decision = this.trackingDecisionFor(
        isAdEventName(eventName) ? "adEvents" : "analytics"
      );
      if (!decision.capture) {
        this.options.logger.verbose(
          `Ignoring recordEvent("${eventName}") because GDPR consent blocks this category.`
        );
        return;
      }
      const occurredAt = /* @__PURE__ */ new Date();
      const currentSession = this.options.sessionManager?.prepareTrackedSessionActivity(occurredAt) ?? null;
      await this.options.requestManager.enqueue({
        kind: "event",
        payload: {
          ...decision.attachDeviceIdentity ? {
            deviceId: this.options.contextManager.requireDeviceId(),
            deviceIdSource: this.options.contextManager.requireDeviceIdSource()
          } : {},
          eventName,
          eventData: normalizeJsonObject(options.eventData),
          clientOccurredAt: occurredAt.toISOString(),
          ...currentSession ? {
            sessionId: currentSession.id,
            sessionRelativeTimeMs: this.getSessionRelativeTimeMs(currentSession, occurredAt)
          } : {}
        }
      }, {
        flushImmediately: this.shouldFlushEventImmediately(
          options.flushImmediately ?? false
        )
      });
    }
    async recordNotification(type, notificationId, options = {}) {
      if (!this.options.settingsState.isEnabled || !this.options.settingsState.areEventsEnabled) {
        this.options.logger.verbose(
          `Ignoring recordNotification(${type}) because SDK or events are disabled.`
        );
        return;
      }
      const decision = this.trackingDecisionFor("analytics");
      if (!decision.capture) {
        this.options.logger.verbose(
          `Ignoring recordNotification(${type}) because GDPR consent blocks analytics capture.`
        );
        return;
      }
      const normalizedNotificationId = notificationId.trim();
      if (!normalizedNotificationId) {
        throw new Error("Attriax recordNotification requires a non-empty notificationId.");
      }
      const occurredAt = /* @__PURE__ */ new Date();
      const currentSession = this.options.sessionManager?.prepareTrackedSessionActivity(occurredAt) ?? null;
      const linkId = trimOrUndefined(options.linkId);
      const campaignId = trimOrUndefined(options.campaignId);
      const title = trimOrUndefined(options.title);
      const source = options.source ?? inferNotificationSource(options.payload);
      const metadata = mergeNotificationMetadata(options.metadata, options.payload);
      await this.options.requestManager.enqueue({
        kind: "notification",
        payload: {
          ...decision.attachDeviceIdentity ? {
            deviceId: this.options.contextManager.requireDeviceId(),
            deviceIdSource: this.options.contextManager.requireDeviceIdSource()
          } : {},
          type,
          notificationId: normalizedNotificationId,
          platform: this.options.contextManager.requireSnapshot().platform,
          occurredAt: occurredAt.toISOString(),
          ...linkId ? { linkId } : {},
          ...campaignId ? { campaignId } : {},
          ...title ? { title } : {},
          ...source ? { source } : {},
          ...metadata ? { metadata } : {},
          ...currentSession ? { sessionId: currentSession.id } : {}
        }
      }, {
        flushImmediately: this.shouldFlushEventImmediately(
          options.flushImmediately ?? false
        )
      });
    }
    recordNotificationReceived(notificationId, options = {}) {
      return this.recordNotification("received", notificationId, options);
    }
    recordNotificationOpened(notificationId, options = {}) {
      return this.recordNotification("opened", notificationId, options);
    }
    recordNotificationDismissed(notificationId, options = {}) {
      return this.recordNotification("dismissed", notificationId, options);
    }
    async recordPageView(pageName, options = {}) {
      const currentUrl = getCurrentUrl()?.toString();
      const pageTitle = options.pageTitle ?? safeDocumentTitle();
      await this.recordEvent("page_view", {
        eventData: {
          pageName,
          ...options.pageClass ? { pageClass: options.pageClass } : {},
          ...pageTitle ? { pageTitle } : {},
          ...options.previousPageName ? { previousPageName: options.previousPageName } : {},
          ...currentUrl ? { url: currentUrl } : {},
          source: options.source ?? "manual",
          ...options.parameters ? { parameters: options.parameters } : {}
        },
        flushImmediately: options.flushImmediately
      });
    }
    async recordError(error, options = {}) {
      if (!this.options.settingsState.isEnabled) {
        this.options.logger.verbose("Ignoring recordError() because SDK is disabled.");
        return;
      }
      const decision = this.trackingDecisionFor("analytics");
      if (!decision.capture) {
        this.options.logger.verbose("Ignoring recordError() because GDPR consent blocks analytics capture.");
        return;
      }
      const occurredAt = options.occurredAt ?? /* @__PURE__ */ new Date();
      const currentSession = this.options.sessionManager?.prepareTrackedSessionActivity(occurredAt) ?? null;
      const snapshot = this.options.contextManager.requireSnapshot();
      const normalizedError = normalizeTrackedError(error);
      const metadata = this.buildCrashMetadata(normalizedError.metadata, options.metadata);
      await this.options.requestManager.enqueue({
        kind: "trackCrash",
        payload: {
          ...decision.attachDeviceIdentity ? {
            deviceId: this.options.contextManager.requireDeviceId(),
            deviceIdSource: this.options.contextManager.requireDeviceIdSource()
          } : {},
          platform: snapshot.platform,
          source: options.source || "manual",
          clientOccurredAt: occurredAt.toISOString(),
          isFatal: options.isFatal ?? false,
          exceptionType: normalizedError.exceptionType,
          message: normalizedError.message,
          stackTrace: normalizedError.stackTrace,
          ...options.reason || normalizedError.reason ? { reason: options.reason || normalizedError.reason } : {},
          ...currentSession ? {
            sessionId: currentSession.id,
            sessionRelativeTimeMs: this.getSessionRelativeTimeMs(currentSession, occurredAt)
          } : {},
          ...snapshot.device.language ? { locale: snapshot.device.language } : {},
          ...snapshot.app.version ? { appVersion: snapshot.app.version } : {},
          ...snapshot.app.buildNumber ? { appBuildNumber: snapshot.app.buildNumber } : {},
          ...snapshot.app.packageName ? { appPackageName: snapshot.app.packageName } : {},
          sdkApiVersion: snapshot.sdk.apiVersion,
          sdkPackageVersion: snapshot.sdk.packageVersion,
          isFirstLaunch: snapshot.isFirstLaunch,
          ...metadata ? { metadata } : {}
        }
      }, {
        flushImmediately: true
      });
    }
    async setUser(userId, options = {}) {
      if (!this.options.settingsState.isEnabled) {
        this.options.logger.verbose(`Ignoring setUser("${userId}") because SDK is disabled.`);
        return;
      }
      if (!this.trackingDecisionFor("attribution").capture) {
        this.options.logger.verbose("Ignoring setUser() because GDPR consent blocks attribution capture.");
        return;
      }
      await this.queueUserUpdate({
        ...userId == null ? { clearUser: true } : { userId },
        ...userId != null && options.userName ? { userName: options.userName } : {}
      });
    }
    async setUserProperty(name, value) {
      const trimmedName = name.trim();
      if (!trimmedName) {
        return;
      }
      if (value === null) {
        await this.clearUserProperties([trimmedName]);
        return;
      }
      await this.setUserProperties({
        [trimmedName]: value
      });
    }
    async setUserProperties(properties) {
      if (!this.options.settingsState.isEnabled) {
        this.options.logger.verbose("Ignoring setUserProperties() because SDK is disabled.");
        return;
      }
      if (!this.trackingDecisionFor("attribution").capture) {
        this.options.logger.verbose("Ignoring setUserProperties() because GDPR consent blocks attribution capture.");
        return;
      }
      const { properties: propertiesToSet, clearPropertyKeys } = sanitizeUserPropertyUpdate(properties);
      if (Object.keys(propertiesToSet).length === 0 && clearPropertyKeys.length === 0) {
        return;
      }
      await this.queueUserUpdate({
        ...Object.keys(propertiesToSet).length > 0 ? { properties: propertiesToSet } : {},
        ...clearPropertyKeys.length > 0 ? { clearPropertyKeys } : {}
      });
    }
    async clearUserProperties(propertyNames) {
      if (!this.options.settingsState.isEnabled) {
        this.options.logger.verbose("Ignoring clearUserProperties() because SDK is disabled.");
        return;
      }
      if (!this.trackingDecisionFor("attribution").capture) {
        this.options.logger.verbose("Ignoring clearUserProperties() because GDPR consent blocks attribution capture.");
        return;
      }
      const normalizedPropertyNames = propertyNames?.map((propertyName) => propertyName.trim()).filter((propertyName) => propertyName.length > 0) ?? [];
      await this.queueUserUpdate(
        normalizedPropertyNames.length > 0 ? { clearPropertyKeys: normalizedPropertyNames } : { clearAllProperties: true }
      );
    }
    getSessionRelativeTimeMs(session, occurredAt) {
      return Math.max(0, occurredAt.getTime() - session.startedAt.getTime());
    }
    trackingDecisionFor(signal) {
      return this.options.consent.trackingDecisionFor(signal);
    }
    buildCrashMetadata(errorMetadata, optionsMetadata) {
      const browserMetadata = {};
      const currentUrl = getCurrentUrl()?.toString();
      const documentTitle = safeDocumentTitle();
      const referrer = safeDocumentReferrer();
      const userAgent = safeUserAgent();
      if (currentUrl) {
        browserMetadata.url = currentUrl;
      }
      if (documentTitle) {
        browserMetadata.title = documentTitle;
      }
      if (referrer) {
        browserMetadata.referrer = referrer;
      }
      if (userAgent) {
        browserMetadata.userAgent = userAgent;
      }
      const combined = {
        ...browserMetadata,
        ...errorMetadata || {},
        ...optionsMetadata || {}
      };
      return Object.keys(combined).length > 0 ? normalizeJsonObject(combined) : void 0;
    }
    async queueUserUpdate(payload) {
      this.options.sessionManager?.prepareTrackedSessionActivity(/* @__PURE__ */ new Date());
      await this.options.requestManager.enqueue({
        kind: "user",
        payload: {
          deviceId: this.options.contextManager.requireDeviceId(),
          deviceIdSource: this.options.contextManager.requireDeviceIdSource(),
          // CCPA fields ride identify top-level, same omit/cap rules as the
          // app-open; snapshotted here so the next identify reflects the current
          // election.
          ...this.options.ccpaState.toWireFields(),
          ...payload
        }
      }, {
        flushImmediately: true
      });
    }
    shouldFlushEventImmediately(flushImmediately) {
      if (flushImmediately) {
        return true;
      }
      return this.options.flushEventsImmediatelyOnFirstLaunch && this.options.contextManager.requireSnapshot().isFirstLaunch;
    }
  };
  function normalizeTrackedError(error) {
    if (error instanceof Error) {
      const metadata = {};
      if ("cause" in error && error.cause !== void 0) {
        metadata.cause = coerceJsonValue(safeUnknownValue(error.cause));
      }
      return {
        exceptionType: error.name || error.constructor.name || "Error",
        message: error.message || error.name || "Unknown error",
        stackTrace: error.stack || `${error.name || "Error"}: ${error.message || "Unknown error"}`,
        metadata: Object.keys(metadata).length > 0 ? metadata : void 0
      };
    }
    if (typeof error === "string") {
      return {
        exceptionType: "Error",
        message: error,
        stackTrace: error
      };
    }
    if (typeof error === "number" || typeof error === "boolean" || error === null || error === void 0) {
      const value = error == null ? "Unknown error" : String(error);
      return {
        exceptionType: "Error",
        message: value,
        stackTrace: value
      };
    }
    const record = error;
    const exceptionType = typeof record.name === "string" && record.name.trim().length > 0 ? record.name : error.constructor?.name || "Error";
    const message = typeof record.message === "string" && record.message.trim().length > 0 ? record.message : safeUnknownText(error);
    const stackTrace = typeof record.stack === "string" && record.stack.trim().length > 0 ? record.stack : message;
    const reason = typeof record.reason === "string" && record.reason.trim().length > 0 ? record.reason : void 0;
    return {
      exceptionType,
      message,
      stackTrace,
      reason,
      metadata: {
        rawError: coerceJsonValue(safeUnknownValue(error))
      }
    };
  }
  function safeUnknownText(value) {
    if (typeof value === "string") {
      return value;
    }
    try {
      const serialized = JSON.stringify(value);
      return serialized && serialized !== "{}" ? serialized : String(value);
    } catch {
      return String(value);
    }
  }
  function safeUnknownValue(value) {
    if (value === null || typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
      return value;
    }
    try {
      return JSON.parse(JSON.stringify(value));
    } catch {
      return safeUnknownText(value);
    }
  }
  function trimOrUndefined(value) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : void 0;
  }
  function mergeNotificationMetadata(metadata, payload) {
    const hasPayload = !!payload && Object.keys(payload).length > 0;
    const hasMetadata = !!metadata && Object.keys(metadata).length > 0;
    if (!hasPayload && !hasMetadata) {
      return void 0;
    }
    const combined = {
      ...hasPayload ? { payload: { ...payload } } : {},
      ...metadata ?? {}
    };
    return normalizeJsonObject(combined);
  }
  function inferNotificationSource(payload) {
    if (!payload) {
      return void 0;
    }
    const keys = Object.keys(payload);
    if (keys.length === 0) {
      return void 0;
    }
    if (Object.prototype.hasOwnProperty.call(payload, "aps")) {
      return "apns";
    }
    if (keys.some(
      (key) => key === "google.message_id" || key === "gcm.message_id" || key.startsWith("google.") || key.startsWith("gcm.")
    )) {
      return "fcm";
    }
    return void 0;
  }

  // src/internal/attriax-client.ts
  var CONSENT_DEVICE_IDENTITY_KINDS = [
    "event",
    "notification",
    "trackCrash",
    "session",
    "deepLinkResolve"
  ];
  function hasConsentDeviceIdentityPayload(entry) {
    return CONSENT_DEVICE_IDENTITY_KINDS.includes(
      entry.kind
    );
  }
  var AttriaxClient = class {
    constructor(config) {
      /**
       * Synchronization state for observing the local offline queue.
       */
      __publicField(this, "synchronization");
      __publicField(this, "tracking");
      __publicField(this, "consent");
      __publicField(this, "deepLinks");
      __publicField(this, "referrer");
      __publicField(this, "configValue");
      __publicField(this, "ccpaState");
      __publicField(this, "logger");
      __publicField(this, "storageManager");
      __publicField(this, "sdkClient");
      __publicField(this, "eventHub", new AttriaxEventHub());
      __publicField(this, "appOpenManager");
      __publicField(this, "appOpenLaunchCoordinator");
      __publicField(this, "activationCoordinator");
      __publicField(this, "browserBindingsManager");
      __publicField(this, "bootstrapCoordinator");
      __publicField(this, "consentManager");
      __publicField(this, "contextManager");
      __publicField(this, "deepLinkManager");
      __publicField(this, "dynamicLinkCreationManager");
      __publicField(this, "installReferrerManager");
      __publicField(this, "requestDispatcher");
      __publicField(this, "requestManager");
      __publicField(this, "runtimeSettingsStore");
      __publicField(this, "settingsState");
      __publicField(this, "sessionLifecycleManager");
      __publicField(this, "sessionManager");
      __publicField(this, "synchronizer");
      __publicField(this, "trackingManager");
      __publicField(this, "initializationPromise", null);
      __publicField(this, "consentReconciliationPromise", Promise.resolve());
      __publicField(this, "lifecycleGeneration", 0);
      __publicField(this, "initializedValue", false);
      __publicField(this, "disposed", false);
      __publicField(this, "captureInitialUrlEnabled", true);
      this.configValue = normalizeConfig(config);
      this.ccpaState = new AttriaxCcpaState({
        doNotSell: this.configValue.doNotSell,
        usPrivacy: this.configValue.usPrivacy
      });
      this.logger = new AttriaxLogger({
        enableDebugLogs: this.configValue.enableDebugLogs
      });
      this.storageManager = new AttriaxStorageManager({
        storage: config.storage ?? createStorage({
          onPersistentStorageUnavailable: (error) => {
            this.logger.warning(
              "Persistent browser storage is unavailable. Attriax is falling back to memory-only state; device identity, queued payloads, and install-referrer data will be lost on reload.",
              { error }
            );
          }
        }),
        storageKeyPrefix: this.configValue.storageKeyPrefix,
        projectToken: this.configValue.projectToken,
        logger: this.logger,
        onQueueEntryDropped: (entry) => {
          this.synchronizer.rejectPending(
            entry.id,
            this.storageManager.buildQuotaExceededError()
          );
        }
      });
      this.storageManager.setRuntimePersistenceMode({
        mode: this.storageManager.resolveInitialRuntimePersistenceMode({
          gdprEnabled: this.configValue.gdprEnabled
        })
      });
      this.sdkClient = createAttriaxSdkClient(
        {
          requestTimeoutMs: this.configValue.requestTimeoutMs,
          sdkClientBaseUrl: this.configValue.sdkClientBaseUrl
        },
        config.fetch
      );
      this.runtimeSettingsStore = new AttriaxRuntimeSettingsStore({
        storageManager: this.storageManager,
        logger: this.logger
      });
      this.settingsState = new AttriaxRuntimeSettingsState({
        store: this.runtimeSettingsStore,
        logger: this.logger
      });
      this.settingsState.restore({
        enabled: this.runtimeSettingsStore.readEnabled(true),
        eventsEnabled: this.runtimeSettingsStore.readEventsEnabled(true)
      });
      const launchStateStore = new AttriaxLaunchStateStore({
        storageManager: this.storageManager
      });
      const contextIdentityStore = new AttriaxContextIdentityStore({
        storageManager: this.storageManager
      });
      const contextSnapshotBuilder = new AttriaxContextSnapshotBuilder({
        config: this.configValue
      });
      const consentStore = new AttriaxConsentStore({
        storageManager: this.storageManager
      });
      const sessionStore = new AttriaxSessionStore({
        storageManager: this.storageManager
      });
      const installReferrerStore = new AttriaxInstallReferrerStore({
        storageManager: this.storageManager
      });
      const queueStore = new AttriaxQueueStore({
        storageManager: this.storageManager
      });
      this.requestManager = new AttriaxRequestManager();
      this.contextManager = new AttriaxContextManager({
        identityStore: contextIdentityStore,
        launchStateStore,
        snapshotBuilder: contextSnapshotBuilder,
        logger: this.logger
      });
      this.consentManager = new AttriaxConsentManager({
        config: this.configValue,
        contextManager: this.contextManager,
        sdkClient: this.sdkClient,
        store: consentStore,
        logger: this.logger,
        onStateChanged: () => {
          this.scheduleConsentReconciliation();
        }
      });
      this.sessionManager = new AttriaxSessionManager({
        config: this.configValue,
        contextManager: this.contextManager,
        store: sessionStore,
        isEnabled: () => this.enabledValue,
        canTrackSessions: () => this.shouldTrackSessionActivity,
        trackingDecision: () => this.trackingDecisionFor("session"),
        requestManager: this.requestManager,
        logger: this.logger
      });
      this.sessionLifecycleManager = new AttriaxSessionLifecycleManager({
        config: this.configValue,
        sessionManager: this.sessionManager,
        isInitialized: () => this.initializedValue,
        isEnabled: () => this.enabledValue,
        canTrackSessions: () => this.shouldTrackSessionActivity
      });
      this.appOpenManager = new AttriaxAppOpenManager({
        config: this.configValue,
        contextManager: this.contextManager,
        sessionManager: this.sessionManager,
        requestManager: this.requestManager,
        ccpaState: this.ccpaState
      });
      this.installReferrerManager = new AttriaxInstallReferrerManager({
        store: installReferrerStore,
        appOpenManager: this.appOpenManager
      });
      this.appOpenLaunchCoordinator = new AttriaxAppOpenLaunchCoordinator({
        appOpenManager: this.appOpenManager,
        installReferrerManager: this.installReferrerManager
      });
      this.requestDispatcher = new AttriaxRequestDispatcher({
        projectToken: this.configValue.projectToken,
        sdkClient: this.sdkClient,
        handleDeferredAppOpen: (result) => this.deepLinkManager.handleDeferredAppOpen(result),
        getSessionKeepAliveBatchEntry: (entries) => this.buildSessionKeepAliveBatchEntry(entries),
        handleSessionKeepAliveDelivered: (sessionId, occurredAt) => {
          this.sessionLifecycleManager.handleSuccessfulForegroundFlush(
            sessionId,
            occurredAt
          );
        }
      });
      this.synchronizer = new AttriaxSynchronizer({
        queueStore,
        maxQueueSize: this.configValue.maxQueueSize,
        eventFlushIntervalMs: this.configValue.eventFlushIntervalMs,
        isDisposed: () => this.disposed,
        isEnabled: () => this.enabledValue,
        canDispatchQueue: () => !this.shouldDeferNetworkDispatch,
        isOnline: () => isBrowserOnline(),
        onStateChange: (state) => this.eventHub.emitSynchronizationState(state),
        logger: this.logger,
        performQueueEntry: (entry) => this.requestDispatcher.performQueueEntry(entry),
        getSendableBatchEntries: (entries) => this.requestDispatcher.getSendableBatchEntries(entries),
        exceedsBatchSizeCap: (entry) => this.requestDispatcher.exceedsBatchSizeCap(entry),
        sendBatchQueueEntries: (entries) => this.requestDispatcher.sendBatchQueueEntries(entries)
      });
      this.requestManager.bindSynchronizer(this.synchronizer);
      this.deepLinkManager = new AttriaxDeepLinkManager({
        automaticBrowserHandling: this.configValue.automaticBrowserHandling,
        platform: this.configValue.platform,
        contextManager: this.contextManager,
        eventHub: this.eventHub,
        sessionManager: this.configValue.sessionTrackingEnabled ? this.sessionManager : void 0,
        requestManager: this.requestManager,
        requestDispatcher: this.requestDispatcher,
        trackingDecision: () => this.trackingDecisionFor("deepLink")
      });
      this.trackingManager = new AttriaxTrackingManager({
        consent: this.consentManager,
        contextManager: this.contextManager,
        flushEventsImmediatelyOnFirstLaunch: this.configValue.flushEventsImmediatelyOnFirstLaunch,
        logger: this.logger,
        requestManager: this.requestManager,
        sessionManager: this.configValue.sessionTrackingEnabled ? this.sessionManager : void 0,
        settingsState: this.settingsState,
        ccpaState: this.ccpaState
      });
      this.browserBindingsManager = new AttriaxBrowserBindingsManager({
        automaticPageTrackingEnabled: this.configValue.automaticPageTracking,
        logger: this.logger,
        settingsState: this.settingsState,
        sessionLifecycleManager: this.sessionLifecycleManager,
        synchronizer: this.synchronizer,
        trackingManager: this.trackingManager,
        isInitialized: () => this.initializedValue,
        isDisposed: () => this.disposed,
        flush: () => this.flushQueue()
      });
      this.dynamicLinkCreationManager = new AttriaxDynamicLinkCreationManager({
        requestDispatcher: this.requestDispatcher
      });
      this.activationCoordinator = new AttriaxActivationCoordinator({
        appOpenLaunchCoordinator: this.appOpenLaunchCoordinator,
        consentManager: this.consentManager,
        deepLinkManager: this.deepLinkManager,
        sessionLifecycleManager: this.sessionLifecycleManager,
        sessionManager: this.sessionManager,
        synchronizer: this.synchronizer,
        allowsAttributionTracking: () => this.allowsAttributionTracking,
        shouldDeferNetworkDispatch: () => this.shouldDeferNetworkDispatch,
        shouldTrackSessionActivity: () => this.shouldTrackSessionActivity,
        flushQueue: () => this.flushQueue()
      });
      this.bootstrapCoordinator = new AttriaxBootstrapCoordinator({
        activationCoordinator: this.activationCoordinator,
        browserBindingsManager: this.browserBindingsManager,
        consentManager: this.consentManager,
        contextManager: this.contextManager,
        installReferrerManager: this.installReferrerManager,
        launchStateStore,
        runtimeSettingsStore: this.runtimeSettingsStore,
        sessionManager: this.sessionManager,
        settingsState: this.settingsState,
        allowDeviceIdentity: () => this.shouldMaterializeIdentifiedContext,
        syncRuntimePersistenceMode: () => this.syncRuntimePersistenceMode(),
        shouldTrackSessionActivity: () => this.shouldTrackSessionActivity,
        restoreOrStartSession: (at) => this.restoreOrStartSession(at),
        shouldEnableInstallReferrer: () => this.allowsAttributionTracking || this.isWaitingForGdprConsent,
        markInitialized: () => {
          this.initializedValue = true;
        },
        isStillValid: (generation) => generation === this.lifecycleGeneration && !this.disposed
      });
      const instance = this;
      this.synchronization = {
        get state() {
          return instance.synchronizer.synchronizationState;
        },
        get isSynchronized() {
          return instance.synchronizer.synchronizationState === "synchronized" /* Synchronized */;
        },
        subscribe(listener) {
          return instance.eventHub.subscribeToSynchronization(listener);
        }
      };
      this.tracking = new AttriaxTracking(this);
      this.consent = new AttriaxConsent(this);
      this.deepLinks = new AttriaxDeepLinks(this);
      this.referrer = new AttriaxReferrer(this);
    }
    /** Whether `init()` has completed for this client instance. */
    get isInitialized() {
      return this.initializedValue;
    }
    /** Whether the SDK is enabled globally. */
    get enabled() {
      return this.enabledValue;
    }
    set enabled(value) {
      this.setEnabled(value);
    }
    /** Whether event tracking is enabled while the SDK remains initialized. */
    get eventsEnabled() {
      return this.eventsEnabledValue;
    }
    set eventsEnabled(value) {
      this.setEventsEnabled(value);
    }
    get anonymousTrackingEnabled() {
      return this.consentManager.anonymousTrackingEnabled;
    }
    set anonymousTrackingEnabled(value) {
      this.consentManager.setAnonymousTrackingEnabled({ enabled: value });
    }
    /** Whether the current browser launch is the first one seen by Attriax. */
    get isFirstLaunch() {
      return this.contextManager.isFirstLaunch;
    }
    /** Stable Attriax device identifier persisted for this browser/runtime. */
    get deviceId() {
      return this.contextManager.deviceId;
    }
    /** SDK version and metadata snapshot captured during initialization. */
    get sdkSnapshot() {
      return this.contextManager.sdkSnapshot;
    }
    get currentGdprConsentState() {
      return this.consentManager.gdprConsentState;
    }
    get currentGdprConsentValues() {
      return this.consentManager.gdprConsentValues;
    }
    get isWaitingForGdprConsent() {
      return this.consentManager.isWaitingForGdprConsent;
    }
    /** Original install-referrer details restored from startup app-open state. */
    getOriginalInstallReferrer() {
      if (!this.initializedValue) {
        return Promise.reject(
          new Error(
            "Attriax.init() must complete before using this SDK instance."
          )
        );
      }
      return this.installReferrerManager.getOriginalInstallReferrer();
    }
    /** Reinstall install-referrer details restored from startup app-open state. */
    getReinstallReferrer() {
      if (!this.initializedValue) {
        return Promise.reject(
          new Error(
            "Attriax.init() must complete before using this SDK instance."
          )
        );
      }
      return this.installReferrerManager.getReinstallReferrer();
    }
    /** Deep-link referrer that opened the current session, when one exists. */
    async getSessionReferrer() {
      if (!this.initializedValue) {
        return Promise.reject(
          new Error("Attriax.init() must complete before using this SDK instance.")
        );
      }
      if (!this.enabledValue) {
        return null;
      }
      const currentReferrer = this.resolveSessionOpeningReferrer();
      if (currentReferrer) {
        return currentReferrer;
      }
      await this.deepLinkManager.waitForInitialDeepLink().catch(() => null);
      const initialReferrer = this.resolveSessionOpeningReferrer();
      if (initialReferrer) {
        return initialReferrer;
      }
      await this.appOpenManager.waitForScheduledResult().catch(() => null);
      return this.resolveSessionOpeningReferrer();
    }
    /** Most recent deep-link referrer observed in the current session. */
    getLatestDeepLinkReferrer() {
      if (!this.initializedValue) {
        return Promise.reject(
          new Error("Attriax.init() must complete before using this SDK instance.")
        );
      }
      if (!this.enabledValue) {
        return Promise.resolve(null);
      }
      const currentReferrer = this.resolveLatestDeepLinkReferrer();
      if (currentReferrer) {
        return Promise.resolve(currentReferrer);
      }
      return new Promise((resolve) => {
        const unsubscribe = this.deepLinkManager.subscribe((event) => {
          const nextReferrer = this.toDeepLinkReferrerDetailsForCurrentSession(event);
          if (!nextReferrer) {
            return;
          }
          unsubscribe();
          resolve(nextReferrer);
        });
      });
    }
    get enabledValue() {
      return this.settingsState.isEnabled;
    }
    get eventsEnabledValue() {
      return this.settingsState.areEventsEnabled;
    }
    get queue() {
      return this.synchronizer.queue;
    }
    set queue(value) {
      this.synchronizer.queue = value;
    }
    get currentRawInitialDeepLink() {
      return this.deepLinkManager.rawInitialDeepLink;
    }
    needsGdprConsent(options = {}) {
      this.assertNotDisposed();
      return this.consentManager.needsConsent(options);
    }
    setGdprConsent(options) {
      this.assertNotDisposed();
      this.consentManager.setConsent(options);
    }
    setGdprConsentNotRequired() {
      this.assertNotDisposed();
      this.consentManager.setNotRequired();
    }
    resetGdprConsent() {
      this.assertNotDisposed();
      this.consentManager.reset();
    }
    get currentCcpaDoNotSell() {
      return this.ccpaState.doNotSell;
    }
    get currentCcpaUsPrivacy() {
      return this.ccpaState.usPrivacy;
    }
    setCcpaDoNotSell(value) {
      this.assertNotDisposed();
      this.ccpaState.setDoNotSell(value);
    }
    setCcpaUsPrivacy(value) {
      this.assertNotDisposed();
      this.ccpaState.setUsPrivacy(value);
    }
    get currentInitialDeepLink() {
      return this.deepLinkManager.initialDeepLink;
    }
    get isInitialDeepLinkResolved() {
      return this.deepLinkManager.isInitialDeepLinkResolved;
    }
    waitForInitialDeepLink() {
      return this.deepLinkManager.waitForInitialDeepLink();
    }
    waitForDeepLinkResolution(rawEvent) {
      return this.deepLinkManager.waitForResolution(rawEvent);
    }
    get currentLatestDeepLink() {
      return this.deepLinkManager.latestDeepLink;
    }
    /**
     * Initializes the SDK, captures browser context, schedules the standard
     * app-open request in the background, and starts queue synchronization.
     *
     * INVARIANT — init() MUST NOT BLOCK on the network. It awaits only local
     * context capture, then fire-and-forgets the heavy work (`void flushQueue()` /
     * `void flushPendingSync()`); the app-open POST and sync run in the background.
     * The returned Promise resolves on LOCAL readiness, not a server round-trip.
     * Any future addition here that can block must be scheduled, not awaited.
     */
    async init(options = {}) {
      this.assertNotDisposed();
      if (this.initializedValue) {
        return;
      }
      const inFlight = this.initializationPromise;
      if (inFlight) {
        return inFlight;
      }
      const initialization = this.runInit(options);
      this.initializationPromise = initialization;
      return initialization.finally(() => {
        if (this.initializationPromise === initialization) {
          this.initializationPromise = null;
        }
      });
    }
    async runInit(options) {
      const generation = this.lifecycleGeneration;
      if (this.configValue.platform === "web" && !hasBrowserWindowContext()) {
        throw new Error(
          "Attriax.init() must run in a browser window context. Construct the client during SSR if needed, but only call init() after window and document are available."
        );
      }
      this.logger.verbose("Initializing Attriax SDK.");
      this.synchronizer.setState("initializing" /* Initializing */);
      this.captureInitialUrlEnabled = options.captureInitialUrl !== false;
      const bootstrapResult = await this.bootstrapCoordinator.initialize({
        captureInitialUrlEnabled: this.captureInitialUrlEnabled,
        generation
      });
      if (bootstrapResult === "aborted") {
        return;
      }
      this.logInitializationActivationResult(bootstrapResult);
    }
    async reset() {
      this.assertNotDisposed();
      this.lifecycleGeneration += 1;
      this.initializationPromise = null;
      this.logger.warning(
        "Resetting Attriax SDK state. Call init() again before reusing this instance."
      );
      this.stopSessionHeartbeatTimer();
      this.browserBindingsManager.dispose();
      await this.synchronizer.reset(
        new AttriaxApiError(
          "Attriax SDK state was reset before queued work completed.",
          void 0,
          true,
          true
        )
      );
      this.appOpenManager.reset();
      this.installReferrerManager.reset();
      this.eventHub.reset();
      this.sessionManager.clear();
      this.contextManager.reset();
      this.consentManager.clearMemory();
      this.storageManager.clearAll();
      this.settingsState.restore({ enabled: true, eventsEnabled: true });
      this.initializedValue = false;
    }
    /** Records a custom event with an optional JSON payload. */
    async recordEvent(eventName, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordEvent(eventName, options);
    }
    /** Records a push-notification lifecycle event for attribution. */
    async recordNotification(type, notificationId, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordNotification(type, notificationId, options);
    }
    /** Records that a push notification was received / displayed. */
    async recordNotificationReceived(notificationId, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordNotificationReceived(notificationId, options);
    }
    /** Records that a push notification was opened (tapped). */
    async recordNotificationOpened(notificationId, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordNotificationOpened(notificationId, options);
    }
    /** Records that a push notification was dismissed without opening. */
    async recordNotificationDismissed(notificationId, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordNotificationDismissed(notificationId, options);
    }
    /** Records a standardized purchase revenue event. */
    async recordPurchase(revenue, options = {}) {
      this.assertInitialized();
      assertFiniteRevenue(revenue);
      const quantity = options.quantity ?? 1;
      if (!Number.isInteger(quantity) || quantity <= 0) {
        throw new Error("Attriax purchase quantity must be a positive integer.");
      }
      const normalizedRevenueCurrency = this.normalizeRevenueCurrency(
        revenue,
        options.currency
      );
      await this.trackingManager.recordEvent("purchase", {
        eventData: {
          ...options.metadata ?? {},
          revenue: normalizedRevenueCurrency.revenue,
          currency: normalizedRevenueCurrency.currency,
          ...options.revenueInMicros ? { revenueInMicros: true } : {},
          ...trimOrUndefined2(options.purchaseType) ? { purchaseType: trimOrUndefined2(options.purchaseType) } : {},
          ...trimOrUndefined2(options.productId) ? { productId: trimOrUndefined2(options.productId) } : {},
          ...trimOrUndefined2(options.transactionId) ? { transactionId: trimOrUndefined2(options.transactionId) } : {},
          ...trimOrUndefined2(options.originalTransactionId) ? {
            originalTransactionId: trimOrUndefined2(
              options.originalTransactionId
            )
          } : {},
          ...trimOrUndefined2(options.validationProvider) ? { validationProvider: trimOrUndefined2(options.validationProvider) } : {},
          ...trimOrUndefined2(options.validationEnvironment) ? {
            validationEnvironment: trimOrUndefined2(
              options.validationEnvironment
            )
          } : {},
          ...trimOrUndefined2(options.purchaseToken) ? { purchaseToken: trimOrUndefined2(options.purchaseToken) } : {},
          ...trimOrUndefined2(options.receiptData) ? { receiptData: trimOrUndefined2(options.receiptData) } : {},
          ...trimOrUndefined2(options.signedPayload) ? { signedPayload: trimOrUndefined2(options.signedPayload) } : {},
          ...trimOrUndefined2(options.receiptSignature) ? { receiptSignature: trimOrUndefined2(options.receiptSignature) } : {},
          ...typeof options.isRenewal === "boolean" ? { isRenewal: options.isRenewal } : {},
          ...quantity !== 1 ? { quantity } : {},
          ...trimOrUndefined2(options.store) ? { store: trimOrUndefined2(options.store) } : {},
          ...trimOrUndefined2(options.packageName) ? { packageName: trimOrUndefined2(options.packageName) } : {},
          ...typeof options.voided === "boolean" ? { voided: options.voided } : {},
          ...typeof options.test === "boolean" ? { test: options.test } : {},
          ...trimOrUndefined2(options.validationId) ? { validationId: trimOrUndefined2(options.validationId) } : {}
        },
        flushImmediately: options.flushImmediately ?? true
      });
    }
    /** Records a standardized refund revenue event. */
    async recordRefund(revenue, options = {}) {
      this.assertInitialized();
      assertFiniteRevenue(revenue);
      const quantity = options.quantity ?? 1;
      if (!Number.isInteger(quantity) || quantity <= 0) {
        throw new Error("Attriax refund quantity must be a positive integer.");
      }
      const normalizedRevenueCurrency = this.normalizeRevenueCurrency(
        revenue,
        options.currency
      );
      const refundRevenue = normalizedRevenueCurrency.revenue === 0 ? 0 : -Math.abs(normalizedRevenueCurrency.revenue);
      await this.trackingManager.recordEvent("refund", {
        eventData: {
          ...options.metadata ?? {},
          revenue: refundRevenue,
          currency: normalizedRevenueCurrency.currency,
          revenueType: "refund",
          ...options.revenueInMicros ? { revenueInMicros: true } : {},
          ...trimOrUndefined2(options.purchaseType) ? { purchaseType: trimOrUndefined2(options.purchaseType) } : {},
          ...trimOrUndefined2(options.productId) ? { productId: trimOrUndefined2(options.productId) } : {},
          ...trimOrUndefined2(options.transactionId) ? { transactionId: trimOrUndefined2(options.transactionId) } : {},
          ...trimOrUndefined2(options.originalTransactionId) ? {
            originalTransactionId: trimOrUndefined2(
              options.originalTransactionId
            )
          } : {},
          ...quantity !== 1 ? { quantity } : {},
          ...trimOrUndefined2(options.store) ? { store: trimOrUndefined2(options.store) } : {},
          ...trimOrUndefined2(options.packageName) ? { packageName: trimOrUndefined2(options.packageName) } : {},
          ...typeof options.voided === "boolean" ? { voided: options.voided } : {},
          ...typeof options.test === "boolean" ? { test: options.test } : {},
          ...trimOrUndefined2(options.reason) ? { reason: trimOrUndefined2(options.reason) } : {}
        },
        flushImmediately: options.flushImmediately ?? true
      });
    }
    async validateReceipt(options) {
      this.assertInitialized();
      const deviceId = this.deviceId;
      const receipt = trimOrUndefined2(options.receipt);
      if (!receipt) {
        throw new Error("validateReceipt() requires a non-empty receipt.");
      }
      return this.requestDispatcher.validateRevenueReceipt({
        ...deviceId ? { deviceId } : {},
        clientOccurredAt: (/* @__PURE__ */ new Date()).toISOString(),
        receipt,
        ...trimOrUndefined2(options.provider) ? { provider: trimOrUndefined2(options.provider) } : {},
        ...trimOrUndefined2(options.environment) ? { environment: trimOrUndefined2(options.environment) } : {},
        ...trimOrUndefined2(options.transactionId) ? { transactionId: trimOrUndefined2(options.transactionId) } : {},
        ...trimOrUndefined2(options.productId) ? { productId: trimOrUndefined2(options.productId) } : {},
        ...typeof options.test === "boolean" ? { test: options.test } : {}
      });
    }
    /** Records a standardized ad revenue event. */
    async recordAdRevenue(revenue, options = {}) {
      this.assertInitialized();
      assertFiniteRevenue(revenue);
      const normalizedRevenueCurrency = this.normalizeRevenueCurrency(
        revenue,
        options.currency
      );
      await this.trackingManager.recordEvent("ad_revenue", {
        eventData: {
          ...options.metadata ?? {},
          revenue: normalizedRevenueCurrency.revenue,
          currency: normalizedRevenueCurrency.currency,
          ...options.revenueInMicros ? { revenueInMicros: true } : {},
          ...trimOrUndefined2(options.adNetwork) ? { adNetwork: trimOrUndefined2(options.adNetwork) } : {},
          ...trimOrUndefined2(options.adFormat) ? { adFormat: trimOrUndefined2(options.adFormat) } : {},
          ...trimOrUndefined2(options.adType) ? { adType: trimOrUndefined2(options.adType) } : {},
          ...trimOrUndefined2(options.adPlacement) ? { adPlacement: trimOrUndefined2(options.adPlacement) } : {},
          ...typeof options.test === "boolean" ? { test: options.test } : {}
        },
        flushImmediately: options.flushImmediately ?? true
      });
    }
    /** Records a canonical ad lifecycle event. */
    async recordAdEvent(type, options = {}) {
      this.assertInitialized();
      const eventName = ATTRIAX_AD_EVENT_NAME_BY_TYPE[type];
      if (!eventName) {
        throw new Error(`Unsupported Attriax ad event type: ${String(type)}`);
      }
      await this.trackingManager.recordEvent(eventName, {
        eventData: buildAdEventData(options),
        flushImmediately: options.flushImmediately ?? true
      });
    }
    /** Records an error or crash payload with optional metadata. */
    async recordError(error, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordError(error, options);
    }
    /**
     * Records a standardized `page_view` event used by Attriax page analytics and
     * funnel reporting.
     */
    async recordPageView(pageName, options = {}) {
      this.assertInitialized();
      await this.trackingManager.recordPageView(pageName, options);
    }
    /** Associates the current browser/device with an application user identifier. */
    async setUser(userId, options = {}) {
      this.assertInitialized();
      await this.trackingManager.setUser(userId, options);
    }
    /** Sets or clears a single user property for subsequent event payloads. */
    async setUserProperty(name, value) {
      this.assertInitialized();
      await this.trackingManager.setUserProperty(name, value);
    }
    /** Sets multiple user properties for subsequent event payloads. */
    async setUserProperties(properties) {
      this.assertInitialized();
      await this.trackingManager.setUserProperties(properties);
    }
    /** Clears selected user properties or all stored user properties. */
    async clearUserProperties(propertyNames) {
      this.assertInitialized();
      await this.trackingManager.clearUserProperties(propertyNames);
    }
    /** Creates a short dynamic link that can carry optional routing data. */
    async createDynamicLink(options = {}) {
      this.assertInitialized();
      return this.dynamicLinkCreationManager.createDynamicLink(options);
    }
    /**
     * Resolves a browser URL or link path against Attriax deep-link rules
     * without notifying deep-link subscribers.
     */
    async recordDeepLink(options) {
      this.assertInitialized();
      if (!this.enabledValue) {
        return null;
      }
      return this.deepLinkManager.recordDeepLink(options);
    }
    subscribeToRawDeepLinks(listener) {
      this.assertNotDisposed();
      return this.deepLinkManager.subscribeToRawDeepLinks(listener);
    }
    /** Subscribes to deferred and manually resolved deep-link events. */
    subscribeToDeepLinks(listener) {
      this.assertNotDisposed();
      return this.deepLinkManager.subscribe(listener);
    }
    /** Removes browser listeners and rejects any in-flight queued promises. */
    dispose() {
      if (this.disposed) {
        return;
      }
      this.logger.verbose("Disposing Attriax SDK runtime.");
      this.lifecycleGeneration += 1;
      this.initializationPromise = null;
      this.disposed = true;
      this.stopSessionHeartbeatTimer();
      this.browserBindingsManager.dispose();
      const disposeError = new AttriaxApiError(
        "Attriax instance was disposed before queued work completed.",
        void 0,
        true,
        true
      );
      this.synchronizer.dispose(disposeError);
      this.installReferrerManager.dispose();
      this.appOpenManager.dispose();
      this.eventHub.dispose();
    }
    setEnabled(value) {
      this.settingsState.setEnabled({
        enabled: value,
        initialized: this.initializedValue,
        applyState: (enabled) => this.applyEnabledState(enabled),
        onPreparingToEnable: value && this.allowsAttributionTracking ? () => this.appOpenLaunchCoordinator.prepareForReenable() : void 0
      });
    }
    applyEnabledState(value) {
      const activationResult = this.activationCoordinator.applyEnabledState({
        enabled: value,
        captureInitialUrlEnabled: this.captureInitialUrlEnabled
      });
      this.logEnabledActivationResult(activationResult);
    }
    /**
     * Public flush entry point. Native host runtimes call this when the app is
     * backgrounded to deliver queued work before suspension. No-ops safely before
     * `init()` (nothing is queued) and after `dispose()`.
     */
    async flush() {
      if (this.disposed || !this.initializedValue) {
        return;
      }
      return this.flushQueue();
    }
    async flushQueue() {
      return this.synchronizer.flush();
    }
    buildSessionKeepAliveBatchEntry(entries) {
      const currentSession = this.sessionManager.currentSession;
      if (!currentSession || this.sessionLifecycleManager.isBackgrounded) {
        return null;
      }
      const includesCurrentSessionEvent = entries.some(
        (entry) => entry.kind === "event" && entry.payload.sessionId === currentSession.id
      );
      if (!includesCurrentSessionEvent) {
        return null;
      }
      const occurredAt = /* @__PURE__ */ new Date();
      return {
        sessionId: currentSession.id,
        occurredAt,
        payload: this.sessionManager.buildHeartbeatKeepAliveRequest(
          currentSession,
          occurredAt
        )
      };
    }
    stopSessionHeartbeatTimer() {
      this.sessionLifecycleManager.stopSessionHeartbeatTimer();
    }
    resolveSessionOpeningReferrer() {
      return this.toDeepLinkReferrerDetailsForCurrentSession(
        this.deepLinkManager.initialDeepLink,
        true
      ) ?? this.toDeepLinkReferrerDetailsForCurrentSession(this.deepLinkManager.latestDeepLink, true);
    }
    resolveLatestDeepLinkReferrer() {
      return this.toDeepLinkReferrerDetailsForCurrentSession(this.deepLinkManager.latestDeepLink);
    }
    toDeepLinkReferrerDetailsForCurrentSession(event, requireSessionOpeningEvent = false) {
      if (!event) {
        return null;
      }
      if (requireSessionOpeningEvent && !(event.isColdStart || event.isDeferred)) {
        return null;
      }
      const currentSession = this.sessionManager.currentSession;
      if (currentSession && getDeepLinkSessionObservedAt(event).getTime() < currentSession.startedAt.getTime()) {
        return null;
      }
      return buildDeepLinkReferrerDetails(event);
    }
    restoreOrStartSession(at) {
      return this.sessionManager.restoreOrStartSession(at);
    }
    setEventsEnabled(value) {
      this.settingsState.setEventsEnabled({ enabled: value });
    }
    logEnabledActivationResult(result) {
      switch (result) {
        case "disabled":
          this.logger.warning("Attriax SDK disabled.");
          return;
        case "deferred":
          this.logger.verbose("Attriax SDK deferred network dispatch.");
          return;
        case "active":
          this.logger.verbose("Attriax SDK enabled.");
          return;
      }
    }
    logInitializationActivationResult(result) {
      switch (result) {
        case "disabled":
          this.logger.warning("Attriax SDK initialized in disabled mode.");
          return;
        case "deferred":
          this.logger.verbose("Attriax SDK deferred network dispatch.");
          this.logger.verbose("Attriax SDK initialized.");
          return;
        case "active":
          this.logger.verbose("Attriax SDK initialized.");
          return;
      }
    }
    /**
     * Serializes consent reconciliation. A single `setConsent` typically notifies
     * twice (local apply, then post-sync apply), and the queue identify/anonymize/
     * drop passes each `await`. Chaining the runs prevents two reconciliations
     * from interleaving their queue mutations.
     */
    scheduleConsentReconciliation() {
      this.consentReconciliationPromise = this.consentReconciliationPromise.catch(() => void 0).then(() => this.handleConsentStateChanged());
    }
    async handleConsentStateChanged() {
      if (!this.initializedValue || this.disposed) {
        return;
      }
      this.syncRuntimePersistenceMode();
      if (this.shouldMaterializeIdentifiedContext) {
        await this.contextManager.ensureIdentifiedContext();
        this.sessionManager.syncCurrentSessionContext();
      }
      this.rewriteAndPurgeQueuedRequestsForConsent();
      this.applyEnabledState(this.enabledValue);
    }
    syncRuntimePersistenceMode() {
      this.storageManager.setRuntimePersistenceMode({
        mode: this.shouldMaterializeIdentifiedContext ? "fullRuntime" : "consentOnly"
      });
    }
    rewriteAndPurgeQueuedRequestsForConsent() {
      if (!this.configValue.gdprEnabled || this.isWaitingForGdprConsent) {
        return;
      }
      this.synchronizer.rewriteQueuedEntriesWhere(
        (entry) => this.shouldIdentifyQueuedEntryForResolvedConsent(entry) ? this.identifyQueuedEntryForResolvedConsent(entry) : null
      );
      this.synchronizer.rewriteQueuedEntriesWhere(
        (entry) => this.shouldAnonymizeQueuedEntry(entry) ? this.anonymizeQueuedEntry(entry) : null
      );
      this.synchronizer.discardQueuedEntriesWhere(
        (entry) => !this.isRequestAllowedByResolvedConsent(entry),
        new AttriaxApiError(
          "Queued request was dropped because GDPR consent blocked this category.",
          void 0,
          false,
          true
        )
      );
    }
    shouldIdentifyQueuedEntryForResolvedConsent(entry) {
      if (!this.shouldMaterializeIdentifiedContext) {
        return false;
      }
      switch (entry.kind) {
        case "event":
        case "notification":
        case "trackCrash":
        case "session":
        case "deepLinkResolve": {
          const decision = this.trackingDecisionForQueuedEntry(entry);
          return decision.capture && decision.attachDeviceIdentity && !entry.payload.deviceId;
        }
        default:
          return false;
      }
    }
    identifyQueuedEntryForResolvedConsent(entry) {
      const deviceId = this.contextManager.deviceId;
      if (!deviceId) {
        return null;
      }
      if (!hasConsentDeviceIdentityPayload(entry) || entry.payload.deviceId) {
        return null;
      }
      return {
        ...entry,
        payload: {
          ...entry.payload,
          deviceId,
          deviceIdSource: this.contextManager.requireDeviceIdSource()
        }
      };
    }
    isRequestAllowedByResolvedConsent(entry) {
      return this.trackingDecisionForQueuedEntry(entry).capture;
    }
    shouldAnonymizeQueuedEntry(entry) {
      if (this.isWaitingForGdprConsent) {
        return false;
      }
      const decision = this.trackingDecisionForQueuedEntry(entry);
      return decision.capture && !decision.attachDeviceIdentity && Boolean(entry.payload.deviceId);
    }
    anonymizeQueuedEntry(entry) {
      if (!hasConsentDeviceIdentityPayload(entry)) {
        return entry;
      }
      const {
        deviceId: _deviceId,
        deviceIdSource: _deviceIdSource,
        ...payload
      } = entry.payload;
      return { ...entry, payload };
    }
    normalizeRevenueCurrency(revenue, currency) {
      const normalizedCurrency = trimOrUndefined2(
        currency ?? "USD"
      )?.toUpperCase();
      if (normalizedCurrency && /^[A-Z]{3}$/.test(normalizedCurrency)) {
        return {
          revenue,
          currency: normalizedCurrency
        };
      }
      this.logger.warning(
        `Invalid revenue currency "${currency ?? ""}"; defaulting revenue to 0 USD.`
      );
      return {
        revenue: 0,
        currency: "USD"
      };
    }
    assertInitialized() {
      this.assertNotDisposed();
      if (!this.initializedValue) {
        throw new Error(
          "Attriax.init() must complete before using this SDK instance."
        );
      }
    }
    get shouldDeferNetworkDispatch() {
      return this.consentManager.shouldDeferNetworkDispatch;
    }
    get allowsAnalyticsTracking() {
      return this.consentManager.allowsAnalyticsTracking;
    }
    get allowsAttributionTracking() {
      return this.consentManager.allowsAttributionTracking;
    }
    get allowsAdEventsTracking() {
      return this.consentManager.allowsAdEventsTracking;
    }
    get canCaptureAnalytics() {
      return this.consentManager.canCaptureAnalytics;
    }
    get canCaptureAttribution() {
      return this.consentManager.canCaptureAttribution;
    }
    get shouldTrackSessionActivity() {
      return this.configValue.sessionTrackingEnabled && this.trackingDecisionFor("session").capture;
    }
    get shouldMaterializeIdentifiedContext() {
      const values = this.currentGdprConsentValues;
      return !this.configValue.gdprEnabled || this.currentGdprConsentState === "not_required" /* NotRequired */ || this.currentGdprConsentState === "granted" /* Granted */ && values != null && (values.analytics || values.attribution || values.adEvents);
    }
    trackingDecisionFor(signal) {
      return this.consentManager.trackingDecisionFor(signal);
    }
    trackingDecisionForQueuedEntry(entry) {
      switch (entry.kind) {
        case "event":
          return this.trackingDecisionFor(
            isAdEventName(entry.payload.eventName) ? "adEvents" : "analytics"
          );
        case "notification":
          return this.trackingDecisionFor("analytics");
        case "trackCrash":
          return this.trackingDecisionFor("analytics");
        case "session":
          return this.trackingDecisionFor("session");
        case "deepLinkResolve":
          return this.trackingDecisionFor("deepLink");
        case "user":
        case "open":
          return this.trackingDecisionFor("attribution");
      }
    }
    assertNotDisposed() {
      if (this.disposed) {
        throw new Error("Attriax instance has already been disposed.");
      }
    }
  };
  function trimOrUndefined2(value) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : void 0;
  }
  function buildDeepLinkReferrerDetails(event) {
    return {
      uri: event.uri,
      receivedAt: event.rawEvent?.receivedAt ?? event.clickedAt,
      clickedAt: event.clickedAt,
      consumedAt: event.consumedAt,
      trigger: event.trigger,
      isAttriaxDomain: event.isAttriaxSubDomain,
      found: event.found,
      ...event.data ? { data: event.data } : {},
      ...event.utm ? { utm: event.utm } : {},
      ...event.browserAction ? { browserAction: event.browserAction } : {},
      handledBySdk: event.handledBySdk
    };
  }
  function getDeepLinkSessionObservedAt(event) {
    return event.rawEvent?.receivedAt ?? event.consumedAt;
  }
  function assertFiniteRevenue(revenue) {
    if (!Number.isFinite(revenue)) {
      throw new Error("Attriax revenue must be a finite number.");
    }
  }
  function buildAdEventData(options) {
    const eventData = {
      ...options.metadata ?? {},
      ...trimOrUndefined2(options.adNetwork) ? { adNetwork: trimOrUndefined2(options.adNetwork) } : {},
      ...trimOrUndefined2(options.mediationNetwork) ? { mediationNetwork: trimOrUndefined2(options.mediationNetwork) } : {},
      ...trimOrUndefined2(options.adUnitId) ? { adUnitId: trimOrUndefined2(options.adUnitId) } : {},
      ...trimOrUndefined2(options.adPlacement) ? { adPlacement: trimOrUndefined2(options.adPlacement) } : {},
      ...trimOrUndefined2(options.adFormat) ? { adFormat: trimOrUndefined2(options.adFormat) } : {},
      ...trimOrUndefined2(options.adType) ? { adType: trimOrUndefined2(options.adType) } : {},
      ...trimOrUndefined2(options.failureReason) ? { failureReason: trimOrUndefined2(options.failureReason) } : {},
      ...trimOrUndefined2(options.rewardType) ? { rewardType: trimOrUndefined2(options.rewardType) } : {},
      ...typeof options.test === "boolean" ? { test: options.test } : {}
    };
    if (typeof options.loadLatencyMs === "number") {
      assertFiniteMetric(options.loadLatencyMs, "loadLatencyMs");
      eventData.loadLatencyMs = options.loadLatencyMs;
    }
    if (typeof options.rewardAmount === "number") {
      assertFiniteMetric(options.rewardAmount, "rewardAmount");
      eventData.rewardAmount = options.rewardAmount;
    }
    return eventData;
  }
  function assertFiniteMetric(value, name) {
    if (!Number.isFinite(value)) {
      throw new Error(`Attriax ${name} must be a finite number.`);
    }
  }

  // src/attriax.ts
  var Attriax = class {
    constructor(config) {
      __publicField(this, "client");
      this.client = new AttriaxClient(config);
    }
    /**
     * Synchronization state and updates.
     *
     * Use this to observe when queued work is still pending, actively
     * synchronizing, or fully flushed.
     */
    get synchronization() {
      return this.client.synchronization;
    }
    /** Regulation-scoped consent helpers. */
    get consent() {
      return this.client.consent;
    }
    /** Event-style tracking and user-association helpers. */
    get tracking() {
      return this.client.tracking;
    }
    /**
     * Whether `init()` has completed successfully.
     *
     * Until this becomes `true`, tracking and identification calls throw because
     * the SDK has not finished restoring persisted state or collecting browser
     * context.
     */
    get isInitialized() {
      return this.client.isInitialized;
    }
    /**
     * Whether the SDK is globally enabled.
     *
     * When disabled, no new requests are queued and deep-link listeners are
     * stopped until this property is set back to `true`.
     */
    get enabled() {
      return this.client.enabled;
    }
    set enabled(value) {
      this.client.enabled = value;
    }
    /**
     * Whether the current browser launch is the first one seen by Attriax.
     *
     * This value is restored during `init()` and stays stable for the current
     * browser session.
     */
    get isFirstLaunch() {
      return this.client.isFirstLaunch;
    }
    /**
     * Stable Attriax device identifier restored or generated during `init()`.
     *
     * This becomes available after initialization and remains stable across
     * launches until SDK-owned storage is cleared.
     */
    get deviceId() {
      return this.client.deviceId;
    }
    /**
     * Deep-link state and stream access for immediate, initial, and deferred
     * links.
     *
     * Deferred deep links resolved from the app-open flow are surfaced through
     * these helpers alongside regular incoming links.
     */
    get deepLinks() {
      return this.client.deepLinks;
    }
    /**
     * Startup install-referrer snapshots and session-scoped deep-link referrers
     * exposed through a focused facade.
     *
     * Browser deep-link events remain available on `deepLinks`.
     */
    get referrer() {
      return this.client.referrer;
    }
    /**
     * SDK version and metadata snapshot captured during initialization.
     *
     * This becomes available after `init()` captures the initial runtime state.
     */
    get sdkSnapshot() {
      return this.client.sdkSnapshot;
    }
    /**
     * Initializes the SDK, captures browser context, schedules the standard
     * app-open request in the background, and starts queue synchronization.
     *
     * This is safe to call more than once; concurrent calls share the same
     * in-flight initialization work. Use `tracking.enabled` before or after
     * `init()` when you need to control event-style tracking independently.
     */
    init(options) {
      return this.client.init(options);
    }
    /**
     * Clears SDK-owned persisted state and returns the instance to pre-init state.
     * Call `init()` again before reusing the instance.
     */
    reset() {
      return this.client.reset();
    }
    /**
     * Validates a mobile-store purchase receipt immediately and returns the
     * public result.
     *
     * Use this during a native purchase flow when an embedded web app receives
     * App Store, Google Play, Unity, or other mobile receipt fields from its
     * host app and needs an immediate verification response. This is not a
     * general validator for standalone browser checkout providers. The current
     * SDK device id is attached automatically.
     */
    validateReceipt(options) {
      return this.client.validateReceipt(options);
    }
    /**
     * Flushes queued events and session activity to the API immediately instead
     * of waiting for the next automatic flush interval.
     *
     * The browser SDK flushes automatically on page-hide/visibility changes;
     * native host runtimes (for example React Native) have no equivalent DOM
     * signal, so they call this when the app is backgrounded to deliver queued
     * work before the OS may suspend the process. Resolves once the flush attempt
     * settles; failures are retried by the offline queue on the next flush.
     */
    flush() {
      return this.client.flush();
    }
    /** Releases browser listeners and rejects any in-flight queued promises. */
    dispose() {
      this.client.dispose();
    }
  };

  // src/attriax-analytics-keys.ts
  var AttriaxAnalyticsEventKeys = {
    signUp: "sign_up",
    login: "login",
    tutorialBegin: "tutorial_begin",
    tutorialComplete: "tutorial_complete",
    levelStart: "level_start",
    levelComplete: "level_complete",
    levelUp: "level_up",
    addPaymentInfo: "add_payment_info",
    addToCart: "add_to_cart",
    checkoutStarted: "checkout_started",
    purchase: "purchase",
    refund: "refund",
    subscriptionStarted: "subscription_started",
    subscriptionRenewed: "subscription_renewed",
    trialStarted: "trial_started",
    adRequest: "ad_request",
    adLoad: "ad_load",
    adLoadFailed: "ad_load_failed",
    adShow: "ad_show",
    adShowFailed: "ad_show_failed",
    adImpression: "ad_impression",
    adClick: "ad_click",
    adDismiss: "ad_dismiss",
    adReward: "ad_reward",
    adRevenue: "ad_revenue",
    pageView: "page_view"
  };
  var AttriaxAnalyticsParamKeys = {
    revenue: "revenue",
    currency: "currency",
    revenueInMicros: "revenueInMicros",
    revenueType: "revenueType",
    purchaseType: "purchaseType",
    method: "method",
    paymentType: "paymentType",
    productId: "productId",
    transactionId: "transactionId",
    originalTransactionId: "originalTransactionId",
    validationProvider: "validationProvider",
    validationEnvironment: "validationEnvironment",
    purchaseToken: "purchaseToken",
    receiptData: "receiptData",
    signedPayload: "signedPayload",
    receiptSignature: "receiptSignature",
    isRenewal: "isRenewal",
    quantity: "quantity",
    store: "store",
    packageName: "packageName",
    voided: "voided",
    test: "test",
    validationId: "validationId",
    reason: "reason",
    adNetwork: "adNetwork",
    mediationNetwork: "mediationNetwork",
    adUnitId: "adUnitId",
    adPlacement: "adPlacement",
    adFormat: "adFormat",
    adType: "adType",
    failureReason: "failureReason",
    loadLatencyMs: "loadLatencyMs",
    rewardType: "rewardType",
    rewardAmount: "rewardAmount",
    pageName: "pageName",
    pageClass: "pageClass",
    pageTitle: "pageTitle",
    previousPageName: "previousPageName",
    source: "source",
    day: "day",
    actualDay: "actualDay",
    retentionType: "retentionType"
  };

  exports.Attriax = Attriax;
  exports.AttriaxAnalyticsEventKeys = AttriaxAnalyticsEventKeys;
  exports.AttriaxAnalyticsParamKeys = AttriaxAnalyticsParamKeys;
  exports.AttriaxApiError = AttriaxApiError;
  exports.AttriaxCcpaConsent = AttriaxCcpaConsent;
  exports.AttriaxConsent = AttriaxConsent;
  exports.AttriaxDeepLinkResolutionStatus = AttriaxDeepLinkResolutionStatus;
  exports.AttriaxDeepLinkTrigger = AttriaxDeepLinkTrigger;
  exports.AttriaxDeepLinks = AttriaxDeepLinks;
  exports.AttriaxGdprConsent = AttriaxGdprConsent;
  exports.AttriaxGdprConsentState = AttriaxGdprConsentState;
  exports.AttriaxInstallState = AttriaxInstallState;
  exports.AttriaxReferrer = AttriaxReferrer;
  exports.AttriaxRevenueReceiptValidationStatus = AttriaxRevenueReceiptValidationStatus;
  exports.AttriaxSynchronizationState = AttriaxSynchronizationState;
  exports.AttriaxTracking = AttriaxTracking;
  exports.AttributionType = AttributionType;
  exports.attriaxSdkApiVersion = attriaxSdkApiVersion;
  exports.attriaxSdkPackageVersion = attriaxSdkPackageVersion;

  return exports;

})({});
