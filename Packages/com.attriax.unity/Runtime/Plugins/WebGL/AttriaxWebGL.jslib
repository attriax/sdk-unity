// Unity WebGL <-> @attriax/js (sdk-js) bridge.
//
// This is the WebGL transport for `AttriaxWebGLEnginePlatform` (the C# half). It
// drives the same engine the Flutter web binding drives (`AttriaxWeb` over
// `dart:js_interop`, calling `globalThis.AttriaxJs.Attriax`). The C# side speaks a
// uniform JSON dispatch envelope; this router translates each dispatch to the real
// sdk-js object API, then hands results (and engine events) back to C# through the
// two registered `[MonoPInvokeCallback]` function pointers.
//
// The `@attriax/js` IIFE bundle must have attached `globalThis.AttriaxJs` before
// `AttriaxWebGL_Create` runs; it is shipped as the preload plugin
// `AttriaxJsBundle.jspre` in this same folder.
//
// Marshalling contract:
// - All commands are async: `dispatch(...)` returns a Promise; on settle the router
//   calls the C# result trampoline with (requestId, ptr-to-envelope-json).
// - The router owns each result/event buffer: it _malloc()s, hands the pointer to
//   the dynCall, then _free()s once the synchronous trampoline returns (the C# side
//   copies to a managed string in-place before returning).
mergeInto(LibraryManager.library, {
  $AttriaxWebGL: {
    instances: {},
    nextHandle: 1,
    resultCb: 0,
    eventCb: 0,

    sdkNamespace: function () {
      var ns = (typeof globalThis !== 'undefined' && globalThis.AttriaxJs) ||
        (typeof window !== 'undefined' && window.AttriaxJs) || null;
      return ns;
    },

    // Push a {ok,value}/{ok,error} envelope back to the C# result trampoline.
    complete: function (requestId, envelope) {
      var cb = AttriaxWebGL.resultCb;
      if (!cb) {
        return;
      }
      var json = JSON.stringify(envelope);
      var len = lengthBytesUTF8(json) + 1;
      var buf = _malloc(len);
      stringToUTF8(json, buf, len);
      {{{ makeDynCall('vii', 'cb') }}}(requestId, buf);
      _free(buf);
    },

    // Push an engine event ({type, ...}) back to the C# event trampoline.
    emit: function (handle, payload) {
      var cb = AttriaxWebGL.eventCb;
      if (!cb) {
        return;
      }
      var json = JSON.stringify(payload);
      var len = lengthBytesUTF8(json) + 1;
      var buf = _malloc(len);
      stringToUTF8(json, buf, len);
      {{{ makeDynCall('vii', 'cb') }}}(handle, buf);
      _free(buf);
    },

    // Subscribe to the sdk-js streams once `init` resolves; mirror AttriaxWeb.
    wireStreams: function (handle, rec) {
      var sdk = rec.sdk;
      try {
        rec.unsubs.push(sdk.synchronization.subscribe(function (state) {
          AttriaxWebGL.emit(handle, { type: 'synchronizationState', state: state });
        }));
        rec.unsubs.push(sdk.deepLinks.stream.subscribe(function (evt) {
          AttriaxWebGL.emit(handle, { type: 'deepLink', event: evt });
        }));
        rec.unsubs.push(sdk.deepLinks.rawStream.subscribe(function (evt) {
          AttriaxWebGL.emit(handle, { type: 'rawDeepLink', event: evt });
        }));
      } catch (e) {
        // A stream that fails to attach must not abort init.
      }

      // sdk-js has no discrete initial-link-resolution stream; synthesize a single
      // resolution once the launch-URL probe settles, mirroring the native bindings'
      // initial-deep-link channel.
      Promise.resolve()
        .then(function () { return sdk.deepLinks.waitForInitialDeepLink(); })
        .then(function (evt) {
          AttriaxWebGL.emit(handle, { type: 'initialDeepLink', resolved: true, event: evt == null ? null : evt });
        })
        .catch(function () {
          AttriaxWebGL.emit(handle, { type: 'initialDeepLink', resolved: true, event: null });
        });
    },

    // Route one dispatch to the sdk-js object API. Returns a Promise of the value.
    dispatch: function (handle, method, args) {
      return Promise.resolve().then(function () {
        var rec = AttriaxWebGL.instances[handle];
        if (!rec) {
          throw new Error('unknown_engine_handle');
        }
        var sdk = rec.sdk;
        var a = args && args.length ? JSON.parse(args) : {};

        switch (method) {
          // ---- lifecycle ----
          case 'init':
            return sdk.init({}).then(function () {
              AttriaxWebGL.wireStreams(handle, rec);
              return null;
            });
          case 'flush':
            return sdk.flush();
          case 'reset':
            return sdk.reset();
          case 'dispose':
            sdk.dispose();
            return null;

          // ---- tracking ----
          case 'recordEvent':
            return sdk.tracking.recordEvent(a.name, {
              eventData: a.eventData,
              flushImmediately: a.flushImmediately,
            });
          case 'recordPageView':
            return sdk.tracking.recordPageView(a.pageName, {
              pageClass: a.pageClass,
              pageTitle: a.pageTitle,
              previousPageName: a.previousPageName,
              parameters: a.parameters,
              source: a.source,
              flushImmediately: a.flushImmediately,
            });
          case 'recordPurchase':
            return sdk.tracking.recordPurchase(a.revenue, {
              currency: a.currency,
              revenueInMicros: a.revenueInMicros,
              purchaseType: a.purchaseType,
              productId: a.productId,
              transactionId: a.transactionId,
              originalTransactionId: a.originalTransactionId,
              validationProvider: a.validationProvider,
              validationEnvironment: a.validationEnvironment,
              purchaseToken: a.purchaseToken,
              receiptData: a.receiptData,
              signedPayload: a.signedPayload,
              receiptSignature: a.receiptSignature,
              isRenewal: a.isRenewal,
              quantity: a.quantity,
              store: a.store,
              packageName: a.packageName,
              voided: a.voided,
              test: a.test,
              validationId: a.validationId,
              metadata: a.metadata,
              flushImmediately: a.flushImmediately,
            });
          case 'recordRefund':
            return sdk.tracking.recordRefund(a.revenue, {
              currency: a.currency,
              revenueInMicros: a.revenueInMicros,
              purchaseType: a.purchaseType,
              productId: a.productId,
              transactionId: a.transactionId,
              originalTransactionId: a.originalTransactionId,
              quantity: a.quantity,
              store: a.store,
              packageName: a.packageName,
              voided: a.voided,
              test: a.test,
              reason: a.reason,
              metadata: a.metadata,
              flushImmediately: a.flushImmediately,
            });
          case 'recordAdRevenue':
            return sdk.tracking.recordAdRevenue(a.revenue, {
              currency: a.currency,
              revenueInMicros: a.revenueInMicros,
              adNetwork: a.adNetwork,
              adFormat: a.adFormat,
              adType: a.adType,
              adPlacement: a.adPlacement,
              test: a.test,
              metadata: a.metadata,
              flushImmediately: a.flushImmediately,
            });
          case 'recordAdEvent':
            return sdk.tracking.recordAdEvent(a.type, {
              adNetwork: a.adNetwork,
              mediationNetwork: a.mediationNetwork,
              adUnitId: a.adUnitId,
              adPlacement: a.adPlacement,
              adFormat: a.adFormat,
              adType: a.adType,
              failureReason: a.failureReason,
              loadLatencyMs: a.loadLatencyMs,
              rewardType: a.rewardType,
              rewardAmount: a.rewardAmount,
              test: a.test,
              metadata: a.metadata,
              flushImmediately: a.flushImmediately,
            });
          case 'recordNotification':
            return sdk.tracking.recordNotification(a.type, a.notificationId, {
              linkId: a.linkId,
              campaignId: a.campaignId,
              title: a.title,
              source: a.source,
              payload: a.payload,
              metadata: a.metadata,
              flushImmediately: a.flushImmediately,
            });
          case 'recordError': {
            var err = new Error(a.message);
            if (a.exceptionType) {
              err.name = a.exceptionType;
            }
            if (a.stackTrace) {
              err.stack = a.stackTrace;
            }
            return sdk.tracking.recordError(err, {
              source: a.source,
              isFatal: a.fatal,
              reason: a.reason,
              metadata: a.metadata,
            });
          }

          // ---- identity ----
          case 'setUser':
            return sdk.tracking.setUser(a.userId == null ? null : a.userId, { userName: a.userName });
          case 'setUserProperty':
            return sdk.tracking.setUserProperty(a.name, a.value == null ? null : a.value);
          case 'setUserProperties':
            return sdk.tracking.setUserProperties(a.properties || {});
          case 'clearUserProperties':
            return sdk.tracking.clearUserProperties(a.propertyNames || []);

          // ---- deep links ----
          case 'handleIncomingLink':
            return sdk.deepLinks.recordDeepLink({ uri: a.uri, isInitialLink: a.isInitialLink });
          case 'recordDeepLink':
            return sdk.deepLinks.recordDeepLink({ uri: a.uri, metadata: a.metadata, source: a.source });
          case 'waitForInitialDeepLink':
            return sdk.deepLinks.waitForInitialDeepLink();
          case 'waitResolution':
            return sdk.deepLinks.waitResolution({ uri: a.uri, receivedAt: a.receivedAt, isInitial: a.isInitial });
          case 'createDynamicLink':
            return sdk.deepLinks.createDynamicLink({
              name: a.name,
              destinationUrl: a.destinationUrl,
              group: a.group,
              prefix: a.prefix,
              socialPreview: a.socialPreview,
              utms: a.utms,
              redirects: a.redirects,
              data: a.data,
            });

          // ---- receipt validation ----
          case 'validateReceipt':
            return sdk.validateReceipt({
              receipt: a.receipt,
              test: a.test,
              provider: a.provider,
              environment: a.environment,
              productId: a.productId,
              transactionId: a.transactionId,
            });

          // ---- consent / toggles ----
          case 'setGdprConsent':
            sdk.consent.gdpr.setConsent({ analytics: a.analytics, attribution: a.attribution, adEvents: a.adEvents });
            return null;
          case 'setGdprConsentNotRequired':
            sdk.consent.gdpr.setNotRequired();
            return null;
          case 'resetGdprConsent':
            sdk.consent.gdpr.reset();
            return null;
          case 'setAnonymousTracking':
            sdk.tracking.anonymousTrackingEnabled = !!a.enabled;
            return null;
          case 'setEnabled':
            sdk.enabled = !!a.enabled;
            return null;
          case 'setEventTrackingEnabled':
            sdk.tracking.enabled = !!a.enabled;
            return null;

          // ---- getters ----
          case 'getDeviceId':
            return sdk.deviceId == null ? null : sdk.deviceId;
          case 'getIsFirstLaunch':
            return !!sdk.isFirstLaunch;
          case 'getIsInitialized':
            return !!sdk.isInitialized;
          case 'getSdkSnapshot':
            return sdk.sdkSnapshot == null ? null : sdk.sdkSnapshot;
          case 'getEnabled':
            return !!sdk.enabled;
          case 'getEventTrackingEnabled':
            return !!sdk.tracking.enabled;
          case 'getAnonymousTracking':
            return !!sdk.tracking.anonymousTrackingEnabled;
          case 'getSynchronizationState':
            return sdk.synchronization.state;
          case 'getIsSynchronized':
            return !!sdk.synchronization.isSynchronized;
          case 'getOriginalInstallReferrer':
            return sdk.referrer.getOriginalInstallReferrer();
          case 'getReinstallReferrer':
            return sdk.referrer.getReinstallReferrer();
          case 'getSessionReferrer':
            return sdk.referrer.getSessionReferrer();
          case 'getLatestDeepLinkReferrer':
            return sdk.referrer.getLatestDeepLinkReferrer();
          case 'getLatestDeepLink':
            return sdk.deepLinks.latestDeepLink == null ? null : sdk.deepLinks.latestDeepLink;
          case 'getInitialDeepLink':
            return sdk.deepLinks.initialDeepLink == null ? null : sdk.deepLinks.initialDeepLink;
          case 'getRawInitialDeepLink':
            return sdk.deepLinks.rawInitialDeepLink == null ? null : sdk.deepLinks.rawInitialDeepLink;
          case 'getInitialDeepLinkResolved':
            return !!sdk.deepLinks.initialDeepLinkResolved;
          case 'needsGdprConsent':
            return sdk.consent.gdpr.needsConsent({ localOnly: a.localOnly });
          case 'getIsWaitingForGdprConsent':
            return !!sdk.consent.gdpr.isWaitingForConsent;

          default:
            throw new Error('unsupported_method:' + method);
        }
      });
    },
  },

  AttriaxWebGL_Create__deps: ['$AttriaxWebGL'],
  AttriaxWebGL_Create: function (configPtr) {
    var ns = AttriaxWebGL.sdkNamespace();
    if (!ns || !ns.Attriax) {
      if (typeof console !== 'undefined') {
        console.error('[Attriax][WebGL] globalThis.AttriaxJs is not loaded; ensure AttriaxJsBundle.jspre ships with the build.');
      }
      return 0;
    }
    try {
      var config = JSON.parse(UTF8ToString(configPtr));
      var sdk = new ns.Attriax(config);
      var handle = AttriaxWebGL.nextHandle++;
      AttriaxWebGL.instances[handle] = { sdk: sdk, unsubs: [] };
      return handle;
    } catch (e) {
      if (typeof console !== 'undefined') {
        console.error('[Attriax][WebGL] Failed to construct the @attriax/js engine:', e);
      }
      return 0;
    }
  },

  AttriaxWebGL_RegisterCallbacks__deps: ['$AttriaxWebGL'],
  AttriaxWebGL_RegisterCallbacks: function (resultCb, eventCb) {
    AttriaxWebGL.resultCb = resultCb;
    AttriaxWebGL.eventCb = eventCb;
  },

  AttriaxWebGL_Dispatch__deps: ['$AttriaxWebGL'],
  AttriaxWebGL_Dispatch: function (handle, requestId, methodPtr, argsPtr) {
    var method = UTF8ToString(methodPtr);
    var args = UTF8ToString(argsPtr);
    AttriaxWebGL.dispatch(handle, method, args).then(function (value) {
      AttriaxWebGL.complete(requestId, { ok: true, value: value === undefined ? null : value });
    }).catch(function (err) {
      var message = err && err.message ? err.message : String(err);
      AttriaxWebGL.complete(requestId, { ok: false, error: message });
    });
  },

  AttriaxWebGL_Destroy__deps: ['$AttriaxWebGL'],
  AttriaxWebGL_Destroy: function (handle) {
    var rec = AttriaxWebGL.instances[handle];
    if (!rec) {
      return;
    }
    for (var i = 0; i < rec.unsubs.length; i++) {
      try {
        var unsub = rec.unsubs[i];
        if (typeof unsub === 'function') {
          unsub();
        } else if (unsub && typeof unsub.unsubscribe === 'function') {
          unsub.unsubscribe();
        }
      } catch (e) {
        // Best-effort teardown.
      }
    }
    delete AttriaxWebGL.instances[handle];
  },
});
