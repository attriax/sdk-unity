#ifndef KONAN_LIBATTRIAX_CORE_H
#define KONAN_LIBATTRIAX_CORE_H
#ifdef __cplusplus
extern "C" {
#endif
#ifdef __cplusplus
typedef bool            libattriax_core_KBoolean;
#else
typedef _Bool           libattriax_core_KBoolean;
#endif
typedef unsigned short     libattriax_core_KChar;
typedef signed char        libattriax_core_KByte;
typedef short              libattriax_core_KShort;
typedef int                libattriax_core_KInt;
typedef long long          libattriax_core_KLong;
typedef unsigned char      libattriax_core_KUByte;
typedef unsigned short     libattriax_core_KUShort;
typedef unsigned int       libattriax_core_KUInt;
typedef unsigned long long libattriax_core_KULong;
typedef float              libattriax_core_KFloat;
typedef double             libattriax_core_KDouble;
typedef float __attribute__ ((__vector_size__ (16))) libattriax_core_KVector128;
typedef void*              libattriax_core_KNativePtr;
struct libattriax_core_KType;
typedef struct libattriax_core_KType libattriax_core_KType;

typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Byte;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Short;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Int;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Long;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Float;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Double;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Char;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Boolean;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Unit;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_UByte;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_UShort;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_UInt;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_ULong;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxApple;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxConfig;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Function0;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_Attriax;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleConnectivityMonitor;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleScheduler;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUrlSessionHttpClient;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_HttpResponse;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserAgent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppAttestProvider;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_platform_Foundation_NSUserDefaults;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppAttestProvider_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosDeviceIdSources;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosLifecycleBinder;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallengeFetcher;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_HttpClient;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Any;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_collections_Map;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentTransport;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxExecutor;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState_UNKNOWN;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState_NOT_REQUIRED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState_PENDING;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState_GRANTED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_ANALYTICS;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_AD_EVENTS;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_ATTRIBUTION;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_SESSION;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_DEEP_LINK;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal_UNINSTALL_TRACKING;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode_IDENTIFIED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode_ANONYMOUS;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode_WITHHELD;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Function1;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentRequestRewrites;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxHttpConsentTransport;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkDeferredRecovery;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Function5;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxAppOpenHoist;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_collections_List;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxDispatcher;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Function2;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Timeout;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Transport;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider_Unavailable;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_json_Json;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_json_Json_JsonParseException;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Pair;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_collections_Set;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchLimits;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityResolver;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_DeviceIdSources;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxIdGenerator;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_ByteArray;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxUserAgent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxHttpException;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxTimeoutException;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_kotlin_Throwable;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxTransportException;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder_Noop;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxConsent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxReferrer;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkan;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxTracking;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_Attriax_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_REQUEST;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_LOAD;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_LOAD_FAILED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_SHOW;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_SHOW_FAILED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_IMPRESSION;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_CLICK;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_DISMISS;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType_REWARD;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType_RECEIVED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType_OPENED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType_DISMISSED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource_FCM;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource_APNS;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource_OTHER;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus_AUTHORIZED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus_DENIED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus_RESTRICTED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus_NOT_DETERMINED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus_UNKNOWN;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProviderSlug;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_NoopAttestationProvider;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxConfig_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxAttConsent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger_COLD_START;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger_FOREGROUND;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger_DEFERRED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_MATCHED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_UNMATCHED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_INVALID;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_IN_APP;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_EXTERNAL;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_UNKNOWN;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_VERIFIED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_REJECTED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_PENDING;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_UNCONFIGURED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_PROVIDER_ERROR;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_PASSTHROUGH;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType_REFERRER;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType_FINGERPRINT;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType_EXTERNAL;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType_ORGANIC;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttributionType_Companion;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanState;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue_LOW;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue_MEDIUM;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue_HIGH;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_UPDATED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_SKIPPED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_ALREADY_AT_OR_ABOVE_VALUE;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_INVALID_VALUE;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_DISABLED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_NOT_SUPPORTED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus_ERROR;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_INITIALIZING;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_SYNCHRONIZING;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_DEFERRED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_SYNCHRONIZED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_OFFLINE;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_FAILED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState_DISABLED;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationStateListener;
typedef struct {
  libattriax_core_KNativePtr pinned;
} libattriax_core_kref_com_attriax_sdk_AttriaxVersion;

extern void* attriax_create(void* configJson, void* dataDir);
extern void attriax_destroy(void* handle);
extern void* attriax_dispatch(void* handle, void* method, void* argsJson);
extern void attriax_free_string(void* ptr);
extern void attriax_register_event_callback(void* handle, void* callback, void* userData);

typedef struct {
  /* Service functions. */
  void (*DisposeStablePointer)(libattriax_core_KNativePtr ptr);
  void (*DisposeString)(const char* string);
  libattriax_core_KBoolean (*IsInstance)(libattriax_core_KNativePtr ref, const libattriax_core_KType* type);
  libattriax_core_kref_kotlin_Byte (*createNullableByte)(libattriax_core_KByte);
  libattriax_core_KByte (*getNonNullValueOfByte)(libattriax_core_kref_kotlin_Byte);
  libattriax_core_kref_kotlin_Short (*createNullableShort)(libattriax_core_KShort);
  libattriax_core_KShort (*getNonNullValueOfShort)(libattriax_core_kref_kotlin_Short);
  libattriax_core_kref_kotlin_Int (*createNullableInt)(libattriax_core_KInt);
  libattriax_core_KInt (*getNonNullValueOfInt)(libattriax_core_kref_kotlin_Int);
  libattriax_core_kref_kotlin_Long (*createNullableLong)(libattriax_core_KLong);
  libattriax_core_KLong (*getNonNullValueOfLong)(libattriax_core_kref_kotlin_Long);
  libattriax_core_kref_kotlin_Float (*createNullableFloat)(libattriax_core_KFloat);
  libattriax_core_KFloat (*getNonNullValueOfFloat)(libattriax_core_kref_kotlin_Float);
  libattriax_core_kref_kotlin_Double (*createNullableDouble)(libattriax_core_KDouble);
  libattriax_core_KDouble (*getNonNullValueOfDouble)(libattriax_core_kref_kotlin_Double);
  libattriax_core_kref_kotlin_Char (*createNullableChar)(libattriax_core_KChar);
  libattriax_core_KChar (*getNonNullValueOfChar)(libattriax_core_kref_kotlin_Char);
  libattriax_core_kref_kotlin_Boolean (*createNullableBoolean)(libattriax_core_KBoolean);
  libattriax_core_KBoolean (*getNonNullValueOfBoolean)(libattriax_core_kref_kotlin_Boolean);
  libattriax_core_kref_kotlin_Unit (*createNullableUnit)(void);
  libattriax_core_kref_kotlin_UByte (*createNullableUByte)(libattriax_core_KUByte);
  libattriax_core_KUByte (*getNonNullValueOfUByte)(libattriax_core_kref_kotlin_UByte);
  libattriax_core_kref_kotlin_UShort (*createNullableUShort)(libattriax_core_KUShort);
  libattriax_core_KUShort (*getNonNullValueOfUShort)(libattriax_core_kref_kotlin_UShort);
  libattriax_core_kref_kotlin_UInt (*createNullableUInt)(libattriax_core_KUInt);
  libattriax_core_KUInt (*getNonNullValueOfUInt)(libattriax_core_kref_kotlin_UInt);
  libattriax_core_kref_kotlin_ULong (*createNullableULong)(libattriax_core_KULong);
  libattriax_core_KULong (*getNonNullValueOfULong)(libattriax_core_kref_kotlin_ULong);

  /* User functions. */
  struct {
    struct {
      struct {
        struct {
          struct {
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxApple (*_instance)();
              const char* (*get_VERSION)(libattriax_core_kref_com_attriax_sdk_AttriaxApple thiz);
              libattriax_core_kref_com_attriax_sdk_Attriax (*create)(libattriax_core_kref_com_attriax_sdk_AttriaxApple thiz, libattriax_core_kref_com_attriax_sdk_AttriaxConfig config, const char* userAgent, libattriax_core_kref_kotlin_Function0 advertisingIdSupplier);
            } AttriaxApple;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleConnectivityMonitor (*AttriaxAppleConnectivityMonitor)();
                libattriax_core_KBoolean (*isConnected)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleConnectivityMonitor thiz);
                void (*register_)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleConnectivityMonitor thiz, libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener listener);
                void (*unregister)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleConnectivityMonitor thiz, libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener listener);
              } AttriaxAppleConnectivityMonitor;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleScheduler (*AttriaxAppleScheduler)();
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle (*scheduleOnce)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleScheduler thiz, libattriax_core_KLong delayMs, libattriax_core_kref_kotlin_Function0 action);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle (*schedulePeriodic)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleScheduler thiz, libattriax_core_KLong intervalMs, libattriax_core_kref_kotlin_Function0 action);
                void (*shutdown)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleScheduler thiz);
              } AttriaxAppleScheduler;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUrlSessionHttpClient (*AttriaxAppleUrlSessionHttpClient)(const char* baseUrl, const char* userAgent, libattriax_core_KLong requestTimeoutMs);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*get)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUrlSessionHttpClient thiz, const char* path);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*post)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUrlSessionHttpClient thiz, const char* path, const char* body);
              } AttriaxAppleUrlSessionHttpClient;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserAgent (*_instance)();
                libattriax_core_KLong (*get_DEFAULT_PROBE_TIMEOUT_MS)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserAgent thiz);
                const char* (*resolve)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserAgent thiz, const char* suppliedUserAgent, const char* osVersion, libattriax_core_KLong probeTimeoutMs);
              } AttriaxAppleUserAgent;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore_Companion (*_instance)();
                  const char* (*get_SUITE_NAME)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore_Companion thiz);
                } Companion;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore (*AttriaxAppleUserDefaultsStore)(const char* suiteName);
                const char* (*getString)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore thiz, const char* key);
                void (*putString)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore thiz, const char* key, const char* value);
                void (*remove)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppleUserDefaultsStore thiz, const char* key);
              } AttriaxAppleUserDefaultsStore;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppAttestProvider_Companion (*_instance)();
                } Companion;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppAttestProvider (*AttriaxAppAttestProvider)(libattriax_core_kref_platform_Foundation_NSUserDefaults defaults);
                libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken (*attest)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxAppAttestProvider thiz, const char* nonce);
              } AttriaxAppAttestProvider;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosDeviceIdSources (*AttriaxIosDeviceIdSources)(libattriax_core_KBoolean collectAdvertisingId, libattriax_core_kref_kotlin_Function0 advertisingIdSupplier);
                const char* (*advertisingId)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosDeviceIdSources thiz);
                const char* (*androidSsaid)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosDeviceIdSources thiz);
                const char* (*iosIdfv)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosDeviceIdSources thiz);
              } AttriaxIosDeviceIdSources;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosLifecycleBinder (*AttriaxIosLifecycleBinder)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager lifecycleManager);
                void (*bind)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosLifecycleBinder thiz);
                void (*unbind)(libattriax_core_kref_com_attriax_sdk_apple_AttriaxIosLifecycleBinder thiz);
              } AttriaxIosLifecycleBinder;
            } apple;
            struct {
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallengeFetcher (*AttriaxAttestationChallengeFetcher)(libattriax_core_kref_com_attriax_sdk_internal_HttpClient transport);
                  libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge (*fetch)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallengeFetcher thiz);
                } AttriaxAttestationChallengeFetcher;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge (*AttriaxAttestationChallenge)(const char* nonce, libattriax_core_kref_kotlin_Int expiresInSeconds);
                  libattriax_core_kref_kotlin_Int (*get_expiresInSeconds)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                  const char* (*get_nonce)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                  libattriax_core_kref_kotlin_Int (*component2)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge (*copy)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz, const char* nonce, libattriax_core_kref_kotlin_Int expiresInSeconds);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationChallenge thiz);
                } AttriaxAttestationChallenge;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationManager (*AttriaxAttestationManager)(libattriax_core_KBoolean enabled, libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider provider, libattriax_core_kref_kotlin_Function0 fetchChallenge);
                  libattriax_core_KBoolean (*get_isEnabled)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationManager thiz);
                  libattriax_core_kref_kotlin_collections_Map (*resolveEnvelope)(libattriax_core_kref_com_attriax_sdk_internal_attestation_AttriaxAttestationManager thiz);
                } AttriaxAttestationManager;
              } attestation;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager (*AttriaxConsentManager)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig config, libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock clock, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore consentStore, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentTransport transport, libattriax_core_kref_com_attriax_sdk_internal_AttriaxExecutor syncExecutor);
                  libattriax_core_KBoolean (*get_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*set_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_KBoolean value);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get_gdprConsentState)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*get_gdprConsentValues)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*get_isWaitingForGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_kref_kotlin_Function0 (*get_onStateChanged)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*set_onStateChanged)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_kref_kotlin_Function0 set);
                  libattriax_core_KBoolean (*get_shouldDeferNetworkDispatch)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*allowsAdEventsTracking)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*allowsAnalyticsTracking)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*allowsAttributionTracking)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*canCaptureSignal)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                  void (*clearMemory)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*flushPendingSync)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_KBoolean (*needsConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_KBoolean localOnly);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy (*policy)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*reset)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*restore)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  void (*setConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_KBoolean analytics, libattriax_core_KBoolean attribution, libattriax_core_KBoolean adEvents);
                  void (*setNotRequired)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*trackingDecisionFor)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentManager thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                } AttriaxConsentManager;
                struct {
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get)(); /* enum entry for UNKNOWN. */
                  } UNKNOWN;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get)(); /* enum entry for NOT_REQUIRED. */
                  } NOT_REQUIRED;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get)(); /* enum entry for PENDING. */
                  } PENDING;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get)(); /* enum entry for GRANTED. */
                  } GRANTED;
                  libattriax_core_KType* (*_type)(void);
                } AttriaxGdprConsentState;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*AttriaxGdprConsentValues)(libattriax_core_KBoolean analytics, libattriax_core_KBoolean attribution, libattriax_core_KBoolean adEvents);
                  libattriax_core_KBoolean (*get_adEvents)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_KBoolean (*get_analytics)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_KBoolean (*get_attribution)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_KBoolean (*component2)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*copy)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz, libattriax_core_KBoolean analytics, libattriax_core_KBoolean attribution, libattriax_core_KBoolean adEvents);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues thiz);
                } AttriaxGdprConsentValues;
                struct {
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for ANALYTICS. */
                  } ANALYTICS;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for AD_EVENTS. */
                  } AD_EVENTS;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for ATTRIBUTION. */
                  } ATTRIBUTION;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for SESSION. */
                  } SESSION;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for DEEP_LINK. */
                  } DEEP_LINK;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal (*get)(); /* enum entry for UNINSTALL_TRACKING. */
                  } UNINSTALL_TRACKING;
                  libattriax_core_KType* (*_type)(void);
                } AttriaxTrackingSignal;
                struct {
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode (*get)(); /* enum entry for IDENTIFIED. */
                  } IDENTIFIED;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode (*get)(); /* enum entry for ANONYMOUS. */
                  } ANONYMOUS;
                  struct {
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode (*get)(); /* enum entry for WITHHELD. */
                  } WITHHELD;
                  libattriax_core_KType* (*_type)(void);
                } AttriaxTrackingIdentityMode;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision_Companion (*_instance)();
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*get_ANONYMOUS)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision_Companion thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*get_IDENTIFIED)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision_Companion thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*get_WITHHELD)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*AttriaxTrackingDecision)(libattriax_core_KBoolean capture, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode identityMode, libattriax_core_KBoolean deferNetwork);
                  libattriax_core_KBoolean (*get_attachDeviceIdentity)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_KBoolean (*get_capture)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_KBoolean (*get_deferNetwork)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode (*get_identityMode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_KBoolean (*get_sendNetworkDirectly)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode (*component2)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*copy)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz, libattriax_core_KBoolean capture, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingIdentityMode identityMode, libattriax_core_KBoolean deferNetwork);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision thiz);
                } AttriaxTrackingDecision;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire (*_instance)();
                  const char* (*get_GRANTED)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz);
                  const char* (*get_NOT_REQUIRED)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz);
                  const char* (*get_PENDING)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz);
                  const char* (*get_UNKNOWN)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*fromWire)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz, const char* raw);
                  const char* (*toWire)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStateWire thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state);
                } AttriaxConsentStateWire;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy (*AttriaxConsentPolicy)(libattriax_core_KBoolean gdprEnabled, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, libattriax_core_KBoolean anonymousTrackingEnabled);
                  libattriax_core_KBoolean (*get_isWaitingForGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz);
                  libattriax_core_KBoolean (*get_shouldDeferNetworkDispatch)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz);
                  libattriax_core_KBoolean (*allowsCategory)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_kotlin_Function1 selector);
                  libattriax_core_KBoolean (*canCaptureCategory)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_kotlin_Function1 selector, libattriax_core_KBoolean allowWhileWaiting);
                  libattriax_core_KBoolean (*canCaptureSignal)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                  libattriax_core_KBoolean (*canCaptureWhileWaiting)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                  libattriax_core_KBoolean (*isAnonymousCapableSignal)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                  libattriax_core_KBoolean (*isSignalGranted)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*trackingDecisionFor)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingSignal signal);
                } AttriaxConsentPolicy;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy (*AttriaxConsentQueuePolicy)(libattriax_core_kref_kotlin_Function0 isWaitingForGdprConsent, libattriax_core_kref_kotlin_Function0 anonymousTrackingEnabled, libattriax_core_kref_kotlin_Function0 allowsAttributionTracking, libattriax_core_kref_kotlin_Function1 trackingDecisionFor);
                  libattriax_core_KBoolean (*isRequestAllowedByResolvedConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                  libattriax_core_KBoolean (*shouldAnonymizeQueuedRequest)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                  libattriax_core_KBoolean (*shouldIdentifyQueuedRequestForResolvedConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxTrackingDecision (*trackingDecisionForQueuedRequest)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentQueuePolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                } AttriaxConsentQueuePolicy;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentRequestRewrites (*_instance)();
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*anonymize)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentRequestRewrites thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*identify)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentRequestRewrites thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, const char* deviceId, const char* deviceIdSource);
                } AttriaxConsentRequestRewrites;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent (*AttriaxStoredConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, const char* countryCode, const char* regionSource, const char* checkedAtIso, libattriax_core_KBoolean pendingSync);
                  const char* (*get_checkedAtIso)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*get_countryCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_KBoolean (*get_pendingSync)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*get_regionSource)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get_state)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*get_values)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*component1)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*component2)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_KBoolean (*component6)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent (*copy)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, const char* countryCode, const char* regionSource, const char* checkedAtIso, libattriax_core_KBoolean pendingSync);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent thiz);
                } AttriaxStoredConsent;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore_Companion (*_instance)();
                    const char* (*get_KEY_CONSENT)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore_Companion thiz);
                    const char* (*get_KEY_CONSENT_ID)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore (*AttriaxConsentStore)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore store);
                  void (*clear)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore thiz);
                  const char* (*ensureConsentId)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent (*read)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore thiz);
                  void (*write)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentStore thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxStoredConsent data);
                } AttriaxConsentStore;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*AttriaxRemoteConsentStatus)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, libattriax_core_KBoolean needsConsent, const char* countryCode, const char* regionSource, const char* checkedAtIso);
                  const char* (*get_checkedAtIso)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*get_countryCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_KBoolean (*get_needsConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*get_regionSource)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get_state)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*get_values)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*component1)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*component2)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*component6)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*copy)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, libattriax_core_KBoolean needsConsent, const char* countryCode, const char* regionSource, const char* checkedAtIso);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus thiz);
                } AttriaxRemoteConsentStatus;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*checkGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentTransport thiz, const char* projectToken, const char* consentId);
                  void (*eraseGdprData)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentTransport thiz, const char* projectToken, const char* deviceId);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*upsertGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxConsentTransport thiz, const char* projectToken, const char* consentId, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, const char* countryCode, const char* regionSource, const char* clientOccurredAtIso);
                } AttriaxConsentTransport;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxHttpConsentTransport (*AttriaxHttpConsentTransport)(libattriax_core_kref_com_attriax_sdk_internal_HttpClient http);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*checkGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxHttpConsentTransport thiz, const char* projectToken, const char* consentId);
                  void (*eraseGdprData)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxHttpConsentTransport thiz, const char* projectToken, const char* deviceId);
                  libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxRemoteConsentStatus (*upsertGdprConsent)(libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxHttpConsentTransport thiz, const char* projectToken, const char* consentId, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState state, libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues values, const char* countryCode, const char* regionSource, const char* clientOccurredAtIso);
                } AttriaxHttpConsentTransport;
              } consent;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkDeferredRecovery (*_instance)();
                  const char* (*get_INSTALL_STATE_APP_DATA_CLEAR)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkDeferredRecovery thiz);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*recover)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkDeferredRecovery thiz, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_KLong fallbackTimeMs);
                } AttriaxDeepLinkDeferredRecovery;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion (*_instance)();
                    libattriax_core_KLong (*get_DEFAULT_DEDUP_WINDOW_MS)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion thiz);
                    libattriax_core_KLong (*get_DEFAULT_MANUAL_CONVERSION_TIMEOUT_MS)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion thiz);
                    libattriax_core_KLong (*get_DEFAULT_WAIT_TIMEOUT_MS)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion thiz);
                    const char* (*get_SOURCE_AUTOMATIC)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion thiz);
                    const char* (*get_SOURCE_MANUAL)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager (*AttriaxDeepLinkManager)(libattriax_core_kref_kotlin_Function0 nowMs, libattriax_core_kref_kotlin_Function5 resolveDispatch, libattriax_core_kref_kotlin_Function0 readDeferredHandled, libattriax_core_kref_kotlin_Function1 writeDeferredHandled, libattriax_core_kref_kotlin_Function1 handleBrowserAction, libattriax_core_KLong dedupWindowMs);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*get_initialDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz);
                  libattriax_core_KBoolean (*get_isInitialDeepLinkResolved)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*get_latestDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz);
                  libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*get_rawInitialDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz);
                  void (*addListener)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener listener);
                  void (*addRawListener)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener listener);
                  void (*completeInitialLinkIfAbsent)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz);
                  void (*handleDeferredAppOpen)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_kotlin_collections_Map openResponseData);
                  void (*handleIncomingLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, const char* rawUri, libattriax_core_KBoolean isInitialLink, const char* source);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*recordDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, const char* rawUri, libattriax_core_kref_kotlin_collections_Map metadata, const char* source, libattriax_core_KLong timeoutMs);
                  void (*removeListener)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener listener);
                  void (*removeRawListener)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener listener);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*waitForInitialDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_KLong timeoutMs);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*waitResolution)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkManager thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent rawEvent, libattriax_core_KLong timeoutMs);
                } AttriaxDeepLinkManager;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver (*_instance)();
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*buildResolution)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult result, libattriax_core_KLong clickedAtMs, libattriax_core_KLong consumedAtMs, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger trigger, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri fallbackUri, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent rawEvent, libattriax_core_KBoolean handledBySdk);
                  libattriax_core_kref_kotlin_collections_Map (*buildResolveMetadata)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KBoolean isInitialLink, libattriax_core_kref_kotlin_collections_Map extra);
                  libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*decodeBrowserAction)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_kotlin_Any value);
                  libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult (*decodeResolution)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_kotlin_collections_Map data);
                  const char* (*extractLinkPathFromUri)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri);
                  libattriax_core_KBoolean (*isAttriaxDomain)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri);
                  const char* (*normalizeLinkPath)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, const char* path);
                  libattriax_core_kref_kotlin_collections_Map (*stringMap)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxDeepLinkResolver thiz, libattriax_core_kref_kotlin_Any value);
                } AttriaxDeepLinkResolver;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri_Companion (*_instance)();
                    libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*parse)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri_Companion thiz, const char* rawInput);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*AttriaxUri)(const char* raw, const char* scheme, const char* host, const char* path, libattriax_core_kref_kotlin_collections_Map queryParametersAll);
                  const char* (*get_host)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*get_path)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  libattriax_core_kref_kotlin_collections_Map (*get_queryParametersAll)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*get_raw)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*get_scheme)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  libattriax_core_kref_kotlin_collections_Map (*component5)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*copy)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz, const char* raw, const char* scheme, const char* host, const char* path, libattriax_core_kref_kotlin_collections_Map queryParametersAll);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                  libattriax_core_KBoolean (*isScheme)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz, const char* candidate);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri thiz);
                } AttriaxUri;
              } deeplink;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxAppOpenHoist (*_instance)();
                  libattriax_core_kref_kotlin_collections_List (*prioritize)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxAppOpenHoist thiz, libattriax_core_kref_kotlin_collections_List queue);
                } AttriaxAppOpenHoist;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive (*AttriaxBatchKeepAlive)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, const char* sessionId, libattriax_core_KLong occurredAtMs);
                  libattriax_core_KLong (*get_occurredAtMs)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*get_request)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  const char* (*get_sessionId)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  const char* (*get_syntheticId)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*component1)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive (*copy)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, const char* sessionId, libattriax_core_KLong occurredAtMs);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxBatchKeepAlive thiz);
                } AttriaxBatchKeepAlive;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxDispatcher (*AttriaxDispatcher)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager queue, libattriax_core_kref_com_attriax_sdk_internal_HttpClient transport, libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock clock, libattriax_core_kref_kotlin_Function2 onDelivered, libattriax_core_kref_kotlin_Function1 buildSessionKeepAliveBatch, libattriax_core_kref_kotlin_Function2 onSessionKeepAliveDelivered, libattriax_core_kref_kotlin_Function2 onDropped);
                  libattriax_core_KInt (*flush)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxDispatcher thiz);
                } AttriaxDispatcher;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http (*Http)(libattriax_core_KInt statusCode, const char* retryAfter);
                    const char* (*get_retryAfter)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                    libattriax_core_KInt (*get_statusCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                    libattriax_core_KInt (*component1)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                    const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http (*copy)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz, libattriax_core_KInt statusCode, const char* retryAfter);
                    libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz, libattriax_core_kref_kotlin_Any other);
                    libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                    const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Http thiz);
                  } Http;
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Timeout (*_instance)();
                    libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Timeout thiz, libattriax_core_kref_kotlin_Any other);
                    libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Timeout thiz);
                    const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Timeout thiz);
                  } Timeout;
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Transport (*_instance)();
                    libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Transport thiz, libattriax_core_kref_kotlin_Any other);
                    libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Transport thiz);
                    const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure_Transport thiz);
                  } Transport;
                  libattriax_core_KType* (*_type)(void);
                } AttriaxFailure;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy (*_instance)();
                  libattriax_core_KLong (*get_BASE_BACKOFF_MS)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  libattriax_core_KLong (*get_MAX_BACKOFF_MS)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  libattriax_core_KLong (*get_MAX_RETRY_AGE_MS)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  libattriax_core_KInt (*get_MAX_RETRY_ATTEMPTS)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  const char* (*get_REASON_MAX_AGE)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  const char* (*get_REASON_MAX_ATTEMPTS)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz);
                  libattriax_core_KLong (*backoffRetryAtMs)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_KLong attemptedAtMs, libattriax_core_KInt attemptCount);
                  const char* (*errorClass)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure failure);
                  libattriax_core_kref_kotlin_Int (*httpStatusCode)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure failure);
                  libattriax_core_KBoolean (*isRetryable)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure failure);
                  libattriax_core_KBoolean (*isRetryableHttpStatus)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_KInt statusCode);
                  libattriax_core_KLong (*nextRetryAtMs)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure failure, libattriax_core_KLong attemptedAtMs, libattriax_core_KInt nextAttemptCount);
                  libattriax_core_kref_kotlin_Long (*retryAfterAtMs)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxFailure failure, libattriax_core_KLong attemptedAtMs);
                  const char* (*terminalDropReason)(libattriax_core_kref_com_attriax_sdk_internal_dispatch_AttriaxRetryPolicy thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, libattriax_core_KInt attemptCount, libattriax_core_KLong createdAtMs, libattriax_core_KLong nowMs);
                } AttriaxRetryPolicy;
              } dispatch;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*AttriaxInstallReferrerDetails)(const char* rawReferrer, libattriax_core_kref_kotlin_Long installBeginTimestampSeconds, libattriax_core_kref_kotlin_Long referrerClickTimestampSeconds, libattriax_core_kref_kotlin_Boolean googlePlayInstantParam);
                  libattriax_core_kref_kotlin_Boolean (*get_googlePlayInstantParam)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_kotlin_Long (*get_installBeginTimestampSeconds)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  const char* (*get_rawReferrer)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_kotlin_Long (*get_referrerClickTimestampSeconds)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_kotlin_Long (*component2)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_kotlin_Long (*component3)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_kotlin_Boolean (*component4)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*copy)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz, const char* rawReferrer, libattriax_core_kref_kotlin_Long installBeginTimestampSeconds, libattriax_core_kref_kotlin_Long referrerClickTimestampSeconds, libattriax_core_kref_kotlin_Boolean googlePlayInstantParam);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KBoolean (*hasReferrer)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails thiz);
                } AttriaxInstallReferrerDetails;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider_Unavailable (*_instance)();
                    libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*fetch)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider_Unavailable thiz);
                  } Unavailable;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*fetch)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider thiz);
                } AttriaxInstallReferrerProvider;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator_Companion (*_instance)();
                    libattriax_core_KLong (*get_DEFAULT_RETRY_DELAY_MS)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator_Companion thiz);
                    const char* (*get_KEY_INSTALL_REFERRER)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator (*AttriaxInstallReferrerCoordinator)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerProvider provider, libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore store, libattriax_core_KBoolean enabled, libattriax_core_KLong retryDelayMs, libattriax_core_kref_kotlin_Function1 sleeper);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*cachedDetails)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerDetails (*fetchAndPersist)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator thiz);
                  libattriax_core_KBoolean (*needsFetch)(libattriax_core_kref_com_attriax_sdk_internal_installreferrer_AttriaxInstallReferrerCoordinator thiz);
                } AttriaxInstallReferrerCoordinator;
              } installreferrer;
              struct {
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_json_Json_JsonParseException (*JsonParseException)(const char* message);
                  } JsonParseException;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_json_Json (*_instance)();
                  libattriax_core_kref_kotlin_Any (*decode)(libattriax_core_kref_com_attriax_sdk_internal_json_Json thiz, const char* text);
                  libattriax_core_kref_kotlin_collections_List (*decodeArray)(libattriax_core_kref_com_attriax_sdk_internal_json_Json thiz, const char* text);
                  libattriax_core_kref_kotlin_collections_Map (*decodeObject)(libattriax_core_kref_com_attriax_sdk_internal_json_Json thiz, const char* text);
                  const char* (*encode)(libattriax_core_kref_com_attriax_sdk_internal_json_Json thiz, libattriax_core_kref_kotlin_Any value);
                  libattriax_core_KInt (*encodedByteSize)(libattriax_core_kref_com_attriax_sdk_internal_json_Json thiz, libattriax_core_kref_kotlin_Any value);
                } Json;
              } json;
              struct {
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult (*DecodeResult)(libattriax_core_kref_kotlin_collections_List queue, libattriax_core_KBoolean clearedWholePayload, libattriax_core_KInt droppedEntryCount);
                    libattriax_core_KBoolean (*get_clearedWholePayload)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_KInt (*get_droppedEntryCount)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_kref_kotlin_collections_List (*get_queue)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_kref_kotlin_collections_List (*component1)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_KBoolean (*component2)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_KInt (*component3)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult (*copy)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz, libattriax_core_kref_kotlin_collections_List queue, libattriax_core_KBoolean clearedWholePayload, libattriax_core_KInt droppedEntryCount);
                    libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz, libattriax_core_kref_kotlin_Any other);
                    libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                    const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult thiz);
                  } DecodeResult;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec (*_instance)();
                  libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec_DecodeResult (*decode)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec thiz, const char* rawPayload);
                  const char* (*encode)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec thiz, libattriax_core_kref_kotlin_collections_List queue);
                  libattriax_core_kref_kotlin_Pair (*normalize)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec thiz, const char* rawKind, libattriax_core_kref_kotlin_collections_Map rawBody);
                  const char* (*pathForKind)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueCodec thiz, const char* kind);
                } AttriaxQueueCodec;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager_Companion (*_instance)();
                    const char* (*get_KEY_QUEUE)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager (*AttriaxQueueManager)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore store, libattriax_core_KInt maxQueueSize);
                  libattriax_core_KInt (*discardWhere)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz, libattriax_core_kref_kotlin_Function1 predicate);
                  void (*enqueue)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz, libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest request);
                  libattriax_core_kref_kotlin_collections_List (*readAll)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz);
                  libattriax_core_KInt (*rewriteWhere)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz, libattriax_core_kref_kotlin_Function1 transform);
                  void (*writeAll)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz, libattriax_core_kref_kotlin_collections_List queue);
                  void (*writeAllPreservingNew)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueueManager thiz, libattriax_core_kref_kotlin_collections_List remaining, libattriax_core_kref_kotlin_collections_Set snapshotIds);
                } AttriaxQueueManager;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest (*AttriaxQueuedRequest)(const char* id, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, libattriax_core_KLong createdAtMs, libattriax_core_KInt attemptCount, libattriax_core_kref_kotlin_Long lastAttemptAtMs, const char* lastErrorClass, libattriax_core_kref_kotlin_Int lastHttpStatusCode, libattriax_core_kref_kotlin_Long nextRetryAtMs);
                  libattriax_core_KInt (*get_attemptCount)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_KLong (*get_createdAtMs)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  const char* (*get_id)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Long (*get_lastAttemptAtMs)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  const char* (*get_lastErrorClass)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Int (*get_lastHttpStatusCode)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Long (*get_nextRetryAtMs)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*get_request)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*component2)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_KInt (*component4)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Long (*component5)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  const char* (*component6)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Int (*component7)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_kotlin_Long (*component8)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest (*copy)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz, const char* id, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request, libattriax_core_KLong createdAtMs, libattriax_core_KInt attemptCount, libattriax_core_kref_kotlin_Long lastAttemptAtMs, const char* lastErrorClass, libattriax_core_kref_kotlin_Int lastHttpStatusCode, libattriax_core_kref_kotlin_Long nextRetryAtMs);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_queue_AttriaxQueuedRequest thiz);
                } AttriaxQueuedRequest;
              } queue;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints (*_instance)();
                  const char* (*get_ASA_TOKEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_ATTESTATION_CHALLENGE)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_BATCH)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_CONFIG)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_CONSENT_CHECK)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_CONSENT_UPSERT)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_CRASHES)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_DEEP_LINKS_RESOLVE)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_DYNAMIC_LINKS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_EVENTS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_GDPR_ERASE)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_NOTIFICATIONS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_OPEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_RECEIPTS_VALIDATE)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_REVENUE_CONVERT)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_SESSIONS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_SKAN_CV_CONFIG)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_UNINSTALL_TOKENS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                  const char* (*get_USERS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxEndpoints thiz);
                } AttriaxEndpoints;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion (*_instance)();
                    const char* (*get_FIELD_DEVICE_ID)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_FIELD_DEVICE_ID_SOURCE)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_FIELD_LEGACY_APP_TOKEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_FIELD_PROJECT_TOKEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_CREATE_DYNAMIC_LINK)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_OPEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_REGISTER_UNINSTALL_TOKEN)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_RESOLVE_DEEP_LINK)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_TRACK_CRASH)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_TRACK_EVENT)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_TRACK_NOTIFICATION)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_TRACK_SESSION)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_KIND_USER)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                    const char* (*get_LEGACY_KIND_IDENTIFY)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest_Companion thiz);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*AttriaxApiRequest)(const char* kind, const char* path, libattriax_core_kref_kotlin_collections_Map body);
                  const char* (*get_batchKindName)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_kref_kotlin_collections_Map (*get_body)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_KBoolean (*get_isAppOpen)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_KBoolean (*get_isBatchable)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_KBoolean (*get_isTerminalDropExempt)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  const char* (*get_kind)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  const char* (*get_path)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_kref_kotlin_collections_Map (*component3)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*copy)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz, const char* kind, const char* path, libattriax_core_kref_kotlin_collections_Map body);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest thiz);
                } AttriaxApiRequest;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchLimits (*_instance)();
                  libattriax_core_KInt (*get_MAX_BODY_BYTES)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchLimits thiz);
                  libattriax_core_KInt (*get_MAX_ITEMS)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchLimits thiz);
                } AttriaxBatchLimits;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity (*AttriaxBatchIdentity)(const char* projectToken, const char* deviceId, const char* deviceIdSource);
                  const char* (*get_deviceId)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*get_deviceIdSource)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*get_projectToken)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity (*copy)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz, const char* projectToken, const char* deviceId, const char* deviceIdSource);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity thiz);
                } AttriaxBatchIdentity;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem (*QueuedItem)(const char* id, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                    const char* (*get_id)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*get_request)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                    const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*component2)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem (*copy)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz, const char* id, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                    libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz, libattriax_core_kref_kotlin_Any other);
                    libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                    const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching_QueuedItem thiz);
                  } QueuedItem;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching (*_instance)();
                  const char* (*batchRequestId)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, const char* firstQueuedId);
                  libattriax_core_kref_kotlin_collections_Map (*buildBatchBody)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, libattriax_core_kref_kotlin_collections_List group);
                  libattriax_core_KBoolean (*canShare)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest left, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest right);
                  libattriax_core_kref_kotlin_collections_List (*collectSendableRun)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, libattriax_core_kref_kotlin_collections_List queue, libattriax_core_KInt startIndex);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatchIdentity (*identityOf)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                  libattriax_core_kref_kotlin_collections_Map (*itemBody)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxBatching thiz, libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest request);
                } AttriaxBatching;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders (*_instance)();
                  libattriax_core_kref_kotlin_collections_Map (*buildAsaTokenBody)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* token);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildCrash)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot context, const char* deviceId, const char* deviceIdSource, const char* source, libattriax_core_KBoolean isFatal, const char* exceptionType, const char* message, const char* stackTrace, libattriax_core_KBoolean isFirstLaunch, const char* clientOccurredAtIso, const char* reason, const char* sessionId, libattriax_core_kref_kotlin_Long sessionRelativeTimeMs, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_kref_kotlin_collections_Map (*buildCreateDynamicLink)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* name, const char* destinationUrl, const char* group, const char* prefix, libattriax_core_kref_kotlin_Boolean iosRedirect, libattriax_core_kref_kotlin_Boolean androidRedirect, const char* previewTitle, const char* previewDescription, const char* utmSource, const char* utmMedium, const char* utmCampaign, const char* utmTerm, const char* utmContent, libattriax_core_kref_kotlin_collections_Map data);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildEvent)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* eventName, libattriax_core_kref_kotlin_collections_Map eventData, const char* deviceId, const char* deviceIdSource, const char* sessionId, libattriax_core_kref_kotlin_Long sessionRelativeTimeMs, const char* clientOccurredAtIso);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildNotification)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* platform, const char* type, const char* notificationId, const char* deviceId, const char* deviceIdSource, const char* linkId, const char* campaignId, const char* title, const char* source, const char* sessionId, const char* occurredAtIso, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildOpen)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot context, const char* deviceId, const char* deviceIdSource, libattriax_core_KBoolean isFirstLaunch, const char* sessionId, const char* sessionStartedAtIso, const char* installReferrer, libattriax_core_kref_kotlin_Long installBeginTimestampSeconds, libattriax_core_kref_kotlin_Long referrerClickTimestampSeconds, libattriax_core_kref_kotlin_Boolean googlePlayInstantParam, libattriax_core_kref_kotlin_collections_Map attestation, const char* attStatus, libattriax_core_kref_kotlin_Boolean doNotSell, const char* usPrivacy, libattriax_core_kref_kotlin_collections_Map sdkMetadata);
                  libattriax_core_kref_kotlin_collections_Map (*buildReceiptValidate)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* receipt, const char* deviceId, const char* clientOccurredAtIso, const char* provider, const char* environment, const char* transactionId, const char* productId, libattriax_core_kref_kotlin_Boolean test);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildResolveDeepLink)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* platform, const char* source, libattriax_core_KBoolean isFirstLaunch, const char* deviceId, const char* deviceIdSource, const char* rawUrl, const char* linkPath, const char* sessionId, libattriax_core_kref_kotlin_Long sessionRelativeTimeMs, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildSession)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* kind, const char* sessionId, const char* deviceId, const char* deviceIdSource, const char* clientOccurredAtIso, libattriax_core_kref_kotlin_Long sessionRelativeTimeMs, const char* platform, const char* locale, libattriax_core_kref_kotlin_Boolean isFirstLaunch, const char* appVersion, const char* appBuildNumber, const char* appPackageName, const char* sdkApiVersion, const char* sdkPackageVersion, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildUninstallToken)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* deviceId, const char* deviceIdSource, const char* platform, const char* provider, const char* token, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxApiRequest (*buildUser)(libattriax_core_kref_com_attriax_sdk_internal_request_AttriaxRequestBuilders thiz, const char* projectToken, const char* externalUserId, const char* externalUserName, libattriax_core_kref_kotlin_collections_Map properties, const char* deviceId, const char* deviceIdSource, libattriax_core_KBoolean clearExternalUser, libattriax_core_kref_kotlin_collections_List clearPropertyKeys, libattriax_core_KBoolean clearAllProperties, libattriax_core_kref_kotlin_Boolean doNotSell, const char* usPrivacy);
                } AttriaxRequestBuilders;
              } request;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*AttriaxSessionSnapshot)(const char* sessionId, libattriax_core_KLong startedAtMs, libattriax_core_KLong lastActivityAtMs, libattriax_core_KLong heartbeatIntervalMs, const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber, const char* locale, libattriax_core_KBoolean isFirstLaunch, const char* sdkPackageVersion);
                  const char* (*get_appBuildNumber)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_appPackageName)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_appVersion)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_deviceId)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*get_heartbeatIntervalMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KBoolean (*get_isFirstLaunch)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*get_lastActivityAtMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_locale)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_platform)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_sdkPackageVersion)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*get_sessionId)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*get_startedAtMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component10)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KBoolean (*component11)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component12)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*component2)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*component4)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component6)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component7)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component8)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  const char* (*component9)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*copy)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz, const char* sessionId, libattriax_core_KLong startedAtMs, libattriax_core_KLong lastActivityAtMs, libattriax_core_KLong heartbeatIntervalMs, const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber, const char* locale, libattriax_core_KBoolean isFirstLaunch, const char* sdkPackageVersion);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                  libattriax_core_KLong (*sessionRelativeTimeMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz, libattriax_core_KLong occurredAtMs);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot thiz);
                } AttriaxSessionSnapshot;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext (*AttriaxSessionContext)(const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber);
                  const char* (*get_appBuildNumber)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*get_appPackageName)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*get_appVersion)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*get_deviceId)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*get_platform)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext (*copy)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz, const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext thiz);
                } AttriaxSessionContext;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle (*_instance)();
                    libattriax_core_kref_kotlin_collections_List (*get_ALL)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                    const char* (*get_END)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                    const char* (*get_HEARTBEAT)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                    const char* (*get_PAUSE)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                    const char* (*get_RESUME)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                    const char* (*get_START)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation_Lifecycle thiz);
                  } Lifecycle;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation (*_instance)();
                  libattriax_core_KLong (*get_MAX_WINDOW_MS)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation thiz);
                  libattriax_core_KLong (*get_MIN_WINDOW_MS)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation thiz);
                  libattriax_core_KLong (*continuationWindowMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation thiz, libattriax_core_KLong heartbeatIntervalMs);
                  libattriax_core_KLong (*inferredRecoveredEndAtMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot snapshot, libattriax_core_KLong nowMs);
                  libattriax_core_KBoolean (*shouldContinue)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContinuation thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot snapshot, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext context, libattriax_core_KLong nowMs);
                } AttriaxSessionContinuation;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent (*AttriaxSessionLifecycleEvent)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot session, const char* kind, libattriax_core_KLong occurredAtMs, libattriax_core_kref_kotlin_collections_Map metadata);
                  const char* (*get_kind)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_kref_kotlin_collections_Map (*get_metadata)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_KLong (*get_occurredAtMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*get_session)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*component1)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_kref_kotlin_collections_Map (*component4)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent (*copy)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot session, const char* kind, libattriax_core_KLong occurredAtMs, libattriax_core_kref_kotlin_collections_Map metadata);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent thiz);
                } AttriaxSessionLifecycleEvent;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager (*AttriaxSessionLifecycleManager)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager sessionManager, libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock clock, libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler scheduler, libattriax_core_kref_kotlin_Function0 isEnabled, libattriax_core_kref_kotlin_Function0 currentIdentity, libattriax_core_kref_kotlin_Function1 enqueueLifecycle, libattriax_core_kref_kotlin_Function0 requestFlush);
                  libattriax_core_KBoolean (*get_inBackground)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz);
                  void (*activate)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleEvent (*buildKeepAliveHeartbeat)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_KLong occurredAtMs);
                  void (*deactivate)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz);
                  void (*handleBackground)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_KLong atMs);
                  void (*handleDetached)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_KLong atMs);
                  void (*handleForeground)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_KLong atMs);
                  void (*handleSuccessfulForegroundFlush)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, const char* sessionId, libattriax_core_KLong occurredAtMs);
                  void (*reset)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz);
                  void (*seedInitialSessionStart)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot session);
                  void (*seedRecoveredSessionEnd)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionLifecycleManager thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot session);
                } AttriaxSessionLifecycleManager;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity (*AttriaxSessionIdentity)(const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber, const char* locale, libattriax_core_KBoolean isFirstLaunch, const char* sdkPackageVersion);
                  const char* (*get_appBuildNumber)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_appPackageName)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_appVersion)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_deviceId)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  libattriax_core_KBoolean (*get_isFirstLaunch)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_locale)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_platform)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*get_sdkPackageVersion)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component6)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  libattriax_core_KBoolean (*component7)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*component8)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity (*copy)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz, const char* deviceId, const char* platform, const char* appPackageName, const char* appVersion, const char* appBuildNumber, const char* locale, libattriax_core_KBoolean isFirstLaunch, const char* sdkPackageVersion);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionContext (*toContinuationContext)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity thiz);
                } AttriaxSessionIdentity;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult (*AttriaxSessionRestoreResult)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot currentSession, libattriax_core_KBoolean startedNewSession, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot replacedSession);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*get_currentSession)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*get_replacedSession)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_KBoolean (*get_startedNewSession)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*component1)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_KBoolean (*component2)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*component3)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult (*copy)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot currentSession, libattriax_core_KBoolean startedNewSession, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot replacedSession);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult thiz);
                } AttriaxSessionRestoreResult;
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager (*AttriaxSessionManager)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock clock, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore snapshotStore, libattriax_core_KLong heartbeatIntervalMs, libattriax_core_KLong firstLaunchHeartbeatIntervalMs, libattriax_core_kref_kotlin_Function0 generateSessionId);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*get_currentSession)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz);
                  libattriax_core_KBoolean (*get_isTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz);
                  void (*clear)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*end)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz, libattriax_core_KLong atMs);
                  libattriax_core_KLong (*inferredRecoveredEndAtMs)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot session);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*recordActivity)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz, libattriax_core_KLong atMs);
                  void (*reset)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult (*restoreOrStart)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity identity);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionRestoreResult (*resumeOrStart)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionManager thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionIdentity identity, libattriax_core_KLong atMs);
                } AttriaxSessionManager;
                struct {
                  struct {
                    libattriax_core_KType* (*_type)(void);
                    libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore_Companion (*_instance)();
                    const char* (*get_KEY_SESSION)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore_Companion thiz);
                    libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*decode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore_Companion thiz, libattriax_core_kref_kotlin_collections_Map map);
                    libattriax_core_kref_kotlin_collections_Map (*encode)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore_Companion thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot snapshot);
                  } Companion;
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore (*AttriaxSessionSnapshotStore)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore store);
                  void (*clear)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*read)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore thiz);
                  void (*write)(libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshotStore thiz, libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot snapshot);
                } AttriaxSessionSnapshotStore;
              } session;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock_Companion (*_instance)();
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock (*get_SYSTEM)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock_Companion thiz);
                } Companion;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_KLong (*nowMs)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxClock thiz);
              } AttriaxClock;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot (*AttriaxContextSnapshot)(const char* packageName, const char* appVersion, const char* appBuildNumber, const char* deviceModel, const char* deviceManufacturer, const char* osVersion, const char* deviceTimezone, const char* deviceLocale, const char* deviceBrand, const char* deviceHardware, const char* deviceName, libattriax_core_kref_kotlin_Boolean deviceIsPhysical, libattriax_core_kref_kotlin_Int screenWidth, libattriax_core_kref_kotlin_Int screenHeight, const char* screenResolution, libattriax_core_kref_kotlin_Double devicePixelRatio, libattriax_core_kref_kotlin_Int colorDepth, libattriax_core_kref_kotlin_collections_List supportedAbis, libattriax_core_kref_kotlin_collections_Map deviceMetadata, const char* advertisingId, const char* androidId, const char* platform, const char* sdkApiVersion, const char* sdkPackageVersion);
                const char* (*get_advertisingId)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_androidId)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_appBuildNumber)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_appVersion)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*get_colorDepth)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceBrand)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceHardware)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Boolean (*get_deviceIsPhysical)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceLocale)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceManufacturer)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_collections_Map (*get_deviceMetadata)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceModel)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceName)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Double (*get_devicePixelRatio)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_deviceTimezone)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_osVersion)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_packageName)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_platform)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*get_screenHeight)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_screenResolution)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*get_screenWidth)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_sdkApiVersion)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*get_sdkPackageVersion)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_collections_List (*get_supportedAbis)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component10)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component11)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Boolean (*component12)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*component13)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*component14)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component15)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Double (*component16)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_Int (*component17)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_collections_List (*component18)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_kotlin_collections_Map (*component19)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component20)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component21)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component22)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component23)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component24)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component3)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component4)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component5)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component6)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component7)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component8)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*component9)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot (*copy)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz, const char* packageName, const char* appVersion, const char* appBuildNumber, const char* deviceModel, const char* deviceManufacturer, const char* osVersion, const char* deviceTimezone, const char* deviceLocale, const char* deviceBrand, const char* deviceHardware, const char* deviceName, libattriax_core_kref_kotlin_Boolean deviceIsPhysical, libattriax_core_kref_kotlin_Int screenWidth, libattriax_core_kref_kotlin_Int screenHeight, const char* screenResolution, libattriax_core_kref_kotlin_Double devicePixelRatio, libattriax_core_kref_kotlin_Int colorDepth, libattriax_core_kref_kotlin_collections_List supportedAbis, libattriax_core_kref_kotlin_collections_Map deviceMetadata, const char* advertisingId, const char* androidId, const char* platform, const char* sdkApiVersion, const char* sdkPackageVersion);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
                const char* (*userAgentDescriptor)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxContextSnapshot thiz);
              } AttriaxContextSnapshot;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource (*_instance)();
                const char* (*get_ANDROID_GAID)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource thiz);
                const char* (*get_ANDROID_SSAID)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource thiz);
                const char* (*get_IOS_IDFA)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource thiz);
                const char* (*get_IOS_IDFV)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource thiz);
                const char* (*get_PERSISTENT_STORAGE)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdSource thiz);
              } AttriaxDeviceIdSource;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId (*ResolvedDeviceId)(const char* value, const char* source, libattriax_core_KBoolean isFallback);
                libattriax_core_KBoolean (*get_isFallback)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                const char* (*get_source)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                const char* (*get_value)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                const char* (*component1)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId (*copy)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz, const char* value, const char* source, libattriax_core_KBoolean isFallback);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId thiz);
              } ResolvedDeviceId;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityResolver (*AttriaxDeviceIdentityResolver)(libattriax_core_kref_com_attriax_sdk_internal_DeviceIdSources sources, libattriax_core_KBoolean collectAdvertisingId, const char* advertisingIdSource);
                libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId (*resolve)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityResolver thiz, const char* fallbackDeviceId);
              } AttriaxDeviceIdentityResolver;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore_Companion (*_instance)();
                  const char* (*get_KEY_DEVICE_ID)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore_Companion thiz);
                  const char* (*get_KEY_DEVICE_ID_SOURCE)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore_Companion thiz);
                } Companion;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore (*AttriaxDeviceIdentityStore)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore store, libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityResolver resolver);
                void (*clear)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore thiz);
                libattriax_core_kref_com_attriax_sdk_internal_ResolvedDeviceId (*loadOrCreate)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxDeviceIdentityStore thiz);
              } AttriaxDeviceIdentityStore;
              struct {
                libattriax_core_KType* (*_type)(void);
                void (*execute)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxExecutor thiz, libattriax_core_kref_kotlin_Function0 command);
              } AttriaxExecutor;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxIdGenerator (*_instance)();
                const char* (*formatId)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxIdGenerator thiz, libattriax_core_kref_kotlin_ByteArray bytes);
                const char* (*generate)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxIdGenerator thiz);
              } AttriaxIdGenerator;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue (*NormalizedRevenue)(libattriax_core_KDouble revenue, const char* currency);
                  const char* (*get_currency)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                  libattriax_core_KDouble (*get_revenue)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                  libattriax_core_KDouble (*component1)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                  const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue (*copy)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz, libattriax_core_KDouble revenue, const char* currency);
                  libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz, libattriax_core_kref_kotlin_Any other);
                  libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                  const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue thiz);
                } NormalizedRevenue;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource (*inferNotificationSource)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, libattriax_core_kref_kotlin_collections_Map payload);
                libattriax_core_KBoolean (*isValidCurrency)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, const char* currency);
                libattriax_core_kref_kotlin_collections_Map (*mergeNotificationMetadata)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_kref_kotlin_collections_Map payload);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue_NormalizedRevenue (*normalizeRevenueCurrency)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, libattriax_core_KDouble revenue, const char* currency);
                libattriax_core_KDouble (*refundRevenue)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, libattriax_core_KDouble normalizedRevenue);
                const char* (*trimOrNull)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxRevenue thiz, const char* value);
              } AttriaxRevenue;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxUserAgent (*_instance)();
                const char* (*format)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxUserAgent thiz, const char* osVersion, const char* descriptor, const char* packageVersion, const char* client, const char* osName);
              } AttriaxUserAgent;
              struct {
                libattriax_core_KType* (*_type)(void);
                const char* (*getString)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore thiz, const char* key);
                void (*putString)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore thiz, const char* key, const char* value);
                void (*remove)(libattriax_core_kref_com_attriax_sdk_internal_KeyValueStore thiz, const char* key);
              } KeyValueStore;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*HttpResponse)(libattriax_core_KInt statusCode, const char* body, libattriax_core_kref_kotlin_collections_Map headers);
                const char* (*get_body)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                libattriax_core_kref_kotlin_collections_Map (*get_headers)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                libattriax_core_KInt (*get_statusCode)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                libattriax_core_KInt (*component1)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                const char* (*component2)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                libattriax_core_kref_kotlin_collections_Map (*component3)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*copy)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz, libattriax_core_KInt statusCode, const char* body, libattriax_core_kref_kotlin_collections_Map headers);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
                const char* (*header)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz, const char* name);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_internal_HttpResponse thiz);
              } HttpResponse;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxHttpException (*AttriaxHttpException)(libattriax_core_KInt statusCode, const char* responseBody, libattriax_core_kref_kotlin_collections_Map headers, const char* message);
                libattriax_core_kref_kotlin_collections_Map (*get_headers)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxHttpException thiz);
                const char* (*get_responseBody)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxHttpException thiz);
                libattriax_core_KInt (*get_statusCode)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxHttpException thiz);
              } AttriaxHttpException;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxTimeoutException (*AttriaxTimeoutException)(const char* message, libattriax_core_kref_kotlin_Throwable cause);
              } AttriaxTimeoutException;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxTransportException (*AttriaxTransportException)(const char* message, libattriax_core_kref_kotlin_Throwable cause);
              } AttriaxTransportException;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*get)(libattriax_core_kref_com_attriax_sdk_internal_HttpClient thiz, const char* path);
                libattriax_core_kref_com_attriax_sdk_internal_HttpResponse (*post)(libattriax_core_kref_com_attriax_sdk_internal_HttpClient thiz, const char* path, const char* body);
              } HttpClient;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  void (*onConnectivityRestored)(libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener thiz);
                } Listener;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_KBoolean (*isConnected)(libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor thiz);
                void (*register_)(libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor thiz, libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener listener);
                void (*unregister)(libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor thiz, libattriax_core_kref_com_attriax_sdk_internal_ConnectivityMonitor_Listener listener);
              } ConnectivityMonitor;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  void (*cancel)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle thiz);
                } ScheduledHandle;
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle (*scheduleOnce)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler thiz, libattriax_core_KLong delayMs, libattriax_core_kref_kotlin_Function0 action);
                libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler_ScheduledHandle (*schedulePeriodic)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxScheduler thiz, libattriax_core_KLong intervalMs, libattriax_core_kref_kotlin_Function0 action);
              } AttriaxScheduler;
              struct {
                struct {
                  libattriax_core_KType* (*_type)(void);
                  libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder_Noop (*_instance)();
                  void (*bind)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder_Noop thiz);
                  void (*unbind)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder_Noop thiz);
                } Noop;
                libattriax_core_KType* (*_type)(void);
                void (*bind)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder thiz);
                void (*unbind)(libattriax_core_kref_com_attriax_sdk_internal_AttriaxLifecycleBinder thiz);
              } AttriaxLifecycleBinder;
              struct {
                libattriax_core_KType* (*_type)(void);
                const char* (*advertisingId)(libattriax_core_kref_com_attriax_sdk_internal_DeviceIdSources thiz);
                const char* (*androidSsaid)(libattriax_core_kref_com_attriax_sdk_internal_DeviceIdSources thiz);
                const char* (*iosIdfv)(libattriax_core_kref_com_attriax_sdk_internal_DeviceIdSources thiz);
              } DeviceIdSources;
            } internal;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_Attriax_Companion (*_instance)();
              } Companion;
              libattriax_core_KType* (*_type)(void);
              libattriax_core_KBoolean (*get_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*set_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_Attriax thiz, libattriax_core_KBoolean value);
              libattriax_core_kref_com_attriax_sdk_AttriaxConsent (*get_consent)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_internal_session_AttriaxSessionSnapshot (*get_currentSession)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks (*get_deepLinks)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              const char* (*get_deviceId)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_KBoolean (*get_enabled)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*set_enabled)(libattriax_core_kref_com_attriax_sdk_Attriax thiz, libattriax_core_KBoolean value);
              libattriax_core_KBoolean (*get_isFirstLaunch)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_KBoolean (*get_isInitialized)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxReferrer (*get_referrer)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot (*get_sdkSnapshot)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkan (*get_skan)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization (*get_synchronization)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxTracking (*get_tracking)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*dispose)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*flush)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*init)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*recordEvent)(libattriax_core_kref_com_attriax_sdk_Attriax thiz, const char* name, libattriax_core_kref_kotlin_collections_Map eventData, libattriax_core_KBoolean flushImmediately);
              void (*reset)(libattriax_core_kref_com_attriax_sdk_Attriax thiz);
              void (*submitAsaToken)(libattriax_core_kref_com_attriax_sdk_Attriax thiz, const char* token);
              libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult (*validateReceipt)(libattriax_core_kref_com_attriax_sdk_Attriax thiz, const char* receipt, libattriax_core_KBoolean test, const char* provider, const char* environment, const char* productId, const char* transactionId);
            } Attriax;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys (*_instance)();
              const char* (*get_ADD_PAYMENT_INFO)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_ADD_TO_CART)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_CLICK)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_DISMISS)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_IMPRESSION)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_LOAD)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_LOAD_FAILED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_REQUEST)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_REVENUE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_REWARD)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_SHOW)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_AD_SHOW_FAILED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_CHECKOUT_STARTED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_LEVEL_COMPLETE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_LEVEL_START)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_LEVEL_UP)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_LOGIN)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_PAGE_VIEW)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_PURCHASE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_REFUND)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_SIGN_UP)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_SUBSCRIPTION_RENEWED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_SUBSCRIPTION_STARTED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_TRIAL_STARTED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_TUTORIAL_BEGIN)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
              const char* (*get_TUTORIAL_COMPLETE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsEventKeys thiz);
            } AttriaxAnalyticsEventKeys;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys (*_instance)();
              const char* (*get_AD_FORMAT)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_AD_NETWORK)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_AD_PLACEMENT)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_AD_TYPE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_AD_UNIT_ID)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_CURRENCY)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_FAILURE_REASON)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_IS_RENEWAL)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_LEVEL)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_LOAD_LATENCY_MS)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_MEDIATION_NETWORK)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_METHOD)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_ORIGINAL_TRANSACTION_ID)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PACKAGE_NAME)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PAGE_CLASS)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PAGE_NAME)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PAGE_TITLE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PAYMENT_TYPE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PREVIOUS_PAGE_NAME)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PRODUCT_ID)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PURCHASE_TOKEN)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_PURCHASE_TYPE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_QUANTITY)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REASON)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_RECEIPT_DATA)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_RECEIPT_SIGNATURE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REVENUE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REVENUE_IN_MICROS)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REVENUE_TYPE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REWARD_AMOUNT)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_REWARD_TYPE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_SIGNED_PAYLOAD)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_SOURCE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_STORE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_TEST)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_TRANSACTION_ID)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_VALIDATION_ENVIRONMENT)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_VALIDATION_ID)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_VALIDATION_PROVIDER)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_VALUE)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
              const char* (*get_VOIDED)(libattriax_core_kref_com_attriax_sdk_AttriaxAnalyticsParamKeys thiz);
            } AttriaxAnalyticsParamKeys;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for REQUEST. */
              } REQUEST;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for LOAD. */
              } LOAD;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for LOAD_FAILED. */
              } LOAD_FAILED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for SHOW. */
              } SHOW;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for SHOW_FAILED. */
              } SHOW_FAILED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for IMPRESSION. */
              } IMPRESSION;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for CLICK. */
              } CLICK;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for DISMISS. */
              } DISMISS;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType (*get)(); /* enum entry for REWARD. */
              } REWARD;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_eventName)(libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType thiz);
            } AttriaxAdEventType;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType (*get)(); /* enum entry for RECEIVED. */
              } RECEIVED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType (*get)(); /* enum entry for OPENED. */
              } OPENED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType (*get)(); /* enum entry for DISMISSED. */
              } DISMISSED;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_wireValue)(libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType thiz);
            } AttriaxNotificationEventType;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource (*get)(); /* enum entry for FCM. */
              } FCM;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource (*get)(); /* enum entry for APNS. */
              } APNS;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource (*get)(); /* enum entry for OTHER. */
              } OTHER;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_wireValue)(libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource thiz);
            } AttriaxNotificationEventSource;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get)(); /* enum entry for AUTHORIZED. */
              } AUTHORIZED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get)(); /* enum entry for DENIED. */
              } DENIED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get)(); /* enum entry for RESTRICTED. */
              } RESTRICTED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get)(); /* enum entry for NOT_DETERMINED. */
              } NOT_DETERMINED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get)(); /* enum entry for UNKNOWN. */
              } UNKNOWN;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_wireValue)(libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus thiz);
            } AttriaxAttStatus;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProviderSlug (*_instance)();
              const char* (*get_APP_ATTEST)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProviderSlug thiz);
              const char* (*get_PLAY_INTEGRITY)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProviderSlug thiz);
            } AttriaxAttestationProviderSlug;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken (*AttriaxAttestationToken)(const char* token, const char* keyId);
              const char* (*get_keyId)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
              const char* (*get_token)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz, const char* token, const char* keyId);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken thiz);
            } AttriaxAttestationToken;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken (*attest)(libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider thiz, const char* nonce);
            } AttriaxAttestationProvider;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_NoopAttestationProvider (*_instance)();
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationToken (*attest)(libattriax_core_kref_com_attriax_sdk_NoopAttestationProvider thiz, const char* nonce);
            } NoopAttestationProvider;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener (*get_Unavailable)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener_Companion thiz);
              } Companion;
              libattriax_core_KType* (*_type)(void);
              libattriax_core_KBoolean (*open)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserOpener thiz, const char* url);
            } AttriaxBrowserOpener;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxConfig_Companion (*_instance)();
                const char* (*get_DEFAULT_API_BASE_URL)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig_Companion thiz);
              } Companion;
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxConfig (*AttriaxConfig)(const char* projectToken, const char* apiBaseUrl, const char* appVersion, const char* appBuildNumber, const char* appPackageName, libattriax_core_kref_kotlin_collections_Map sdkMetadata, libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext deviceContext, libattriax_core_KBoolean enableDebugLogs, libattriax_core_KLong requestTimeoutMs, libattriax_core_KInt maxQueueSize, libattriax_core_KLong eventFlushIntervalMs, libattriax_core_KBoolean flushEventsImmediatelyOnFirstLaunch, libattriax_core_KBoolean collectAdvertisingId, libattriax_core_KBoolean automaticCrashReportingEnabled, libattriax_core_KBoolean gdprEnabled, libattriax_core_KBoolean anonymousTracking, libattriax_core_KBoolean sessionTrackingEnabled, libattriax_core_KLong sessionHeartbeatIntervalMs, libattriax_core_KLong firstLaunchSessionHeartbeatIntervalMs, libattriax_core_KBoolean installReferrerEnabled, libattriax_core_KBoolean attestationEnabled, libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider attestationProvider, libattriax_core_kref_kotlin_collections_List pinnedCertificateSha256Fingerprints, libattriax_core_KBoolean automaticBrowserHandling, libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus attStatus, libattriax_core_KBoolean requestTrackingAuthorizationOnInit, libattriax_core_KLong trackingAuthorizationStatusTimeoutMs, libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig skan, libattriax_core_KBoolean asaTokenCaptureEnabled, libattriax_core_kref_kotlin_Boolean doNotSell, const char* usPrivacy);
              libattriax_core_KBoolean (*get_anonymousTracking)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_apiBaseUrl)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_appBuildNumber)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_appPackageName)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_appVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_asaTokenCaptureEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get_attStatus)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_attestationEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider (*get_attestationProvider)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_automaticBrowserHandling)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_automaticCrashReportingEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_collectAdvertisingId)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext (*get_deviceContext)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_Boolean (*get_doNotSell)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_enableDebugLogs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*get_eventFlushIntervalMs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*get_firstLaunchSessionHeartbeatIntervalMs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_flushEventsImmediatelyOnFirstLaunch)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_gdprEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_installReferrerEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KInt (*get_maxQueueSize)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_normalizedProjectToken)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_collections_List (*get_pinnedCertificateSha256Fingerprints)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_projectToken)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*get_requestTimeoutMs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_requestTrackingAuthorizationOnInit)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_sdkMetadata)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*get_sessionHeartbeatIntervalMs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*get_sessionTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig (*get_skan)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*get_trackingAuthorizationStatusTimeoutMs)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*get_usPrivacy)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KInt (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component13)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component14)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component15)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component16)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component17)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*component18)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*component19)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component20)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component21)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider (*component22)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_collections_List (*component23)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component24)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*component25)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component26)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*component27)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig (*component28)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component29)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_Boolean (*component30)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component31)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_kotlin_collections_Map (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KBoolean (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_KLong (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxConfig (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz, const char* projectToken, const char* apiBaseUrl, const char* appVersion, const char* appBuildNumber, const char* appPackageName, libattriax_core_kref_kotlin_collections_Map sdkMetadata, libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext deviceContext, libattriax_core_KBoolean enableDebugLogs, libattriax_core_KLong requestTimeoutMs, libattriax_core_KInt maxQueueSize, libattriax_core_KLong eventFlushIntervalMs, libattriax_core_KBoolean flushEventsImmediatelyOnFirstLaunch, libattriax_core_KBoolean collectAdvertisingId, libattriax_core_KBoolean automaticCrashReportingEnabled, libattriax_core_KBoolean gdprEnabled, libattriax_core_KBoolean anonymousTracking, libattriax_core_KBoolean sessionTrackingEnabled, libattriax_core_KLong sessionHeartbeatIntervalMs, libattriax_core_KLong firstLaunchSessionHeartbeatIntervalMs, libattriax_core_KBoolean installReferrerEnabled, libattriax_core_KBoolean attestationEnabled, libattriax_core_kref_com_attriax_sdk_AttriaxAttestationProvider attestationProvider, libattriax_core_kref_kotlin_collections_List pinnedCertificateSha256Fingerprints, libattriax_core_KBoolean automaticBrowserHandling, libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus attStatus, libattriax_core_KBoolean requestTrackingAuthorizationOnInit, libattriax_core_KLong trackingAuthorizationStatusTimeoutMs, libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig skan, libattriax_core_KBoolean asaTokenCaptureEnabled, libattriax_core_kref_kotlin_Boolean doNotSell, const char* usPrivacy);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxConfig thiz);
            } AttriaxConfig;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttConsent (*get_att)(libattriax_core_kref_com_attriax_sdk_AttriaxConsent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent (*get_ccpa)(libattriax_core_kref_com_attriax_sdk_AttriaxConsent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent (*get_gdpr)(libattriax_core_kref_com_attriax_sdk_AttriaxConsent thiz);
            } AttriaxConsent;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_kotlin_Boolean (*get_doNotSell)(libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent thiz);
              const char* (*get_usPrivacy)(libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent thiz);
              void (*set)(libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent thiz, libattriax_core_kref_kotlin_Boolean doNotSell, const char* usPrivacy);
              void (*setDoNotSell)(libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent thiz, libattriax_core_kref_kotlin_Boolean doNotSell);
              void (*setUsPrivacy)(libattriax_core_kref_com_attriax_sdk_AttriaxCcpaConsent thiz, const char* usPrivacy);
            } AttriaxCcpaConsent;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*get_status)(libattriax_core_kref_com_attriax_sdk_AttriaxAttConsent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus (*requestAuthorization)(libattriax_core_kref_com_attriax_sdk_AttriaxAttConsent thiz, libattriax_core_kref_kotlin_Long timeoutMs);
              void (*setStatus)(libattriax_core_kref_com_attriax_sdk_AttriaxAttConsent thiz, libattriax_core_kref_com_attriax_sdk_AttriaxAttStatus status);
            } AttriaxAttConsent;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_KBoolean (*get_isWaitingForConsent)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentState (*get_state)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_consent_AttriaxGdprConsentValues (*get_values)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
              libattriax_core_KBoolean (*needsConsent)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz, libattriax_core_KBoolean localOnly);
              void (*requestDataErasure)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
              void (*reset)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
              void (*setConsent)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz, libattriax_core_KBoolean analytics, libattriax_core_KBoolean attribution, libattriax_core_KBoolean adEvents);
              void (*setNotRequired)(libattriax_core_kref_com_attriax_sdk_AttriaxGdprConsent thiz);
            } AttriaxGdprConsent;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*get)(); /* enum entry for COLD_START. */
              } COLD_START;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*get)(); /* enum entry for FOREGROUND. */
              } FOREGROUND;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*get)(); /* enum entry for DEFERRED. */
              } DEFERRED;
              libattriax_core_KType* (*_type)(void);
            } AttriaxDeepLinkTrigger;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*get)(); /* enum entry for MATCHED. */
              } MATCHED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*get)(); /* enum entry for UNMATCHED. */
              } UNMATCHED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*get)(); /* enum entry for INVALID. */
              } INVALID;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*fromWire)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus_Companion thiz, const char* value);
              } Companion;
              libattriax_core_KType* (*_type)(void);
            } AttriaxDeepLinkResolutionStatus;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*get)(); /* enum entry for IN_APP. */
              } IN_APP;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*get)(); /* enum entry for EXTERNAL. */
              } EXTERNAL;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*get)(); /* enum entry for UNKNOWN. */
              } UNKNOWN;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*fromWire)(libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode_Companion thiz, const char* value);
              } Companion;
              libattriax_core_KType* (*_type)(void);
            } AttriaxResolvedUrlOpenMode;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*AttriaxBrowserAction)(const char* url, libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode openMode);
              libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*get_openMode)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
              const char* (*get_url)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz, const char* url, libattriax_core_kref_com_attriax_sdk_AttriaxResolvedUrlOpenMode openMode);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction thiz);
            } AttriaxBrowserAction;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*AttriaxRawDeepLinkEvent)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong receivedAtMs, libattriax_core_KBoolean isInitial);
              libattriax_core_KBoolean (*get_isInitial)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_KLong (*get_receivedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*get_uri)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_KLong (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong receivedAtMs, libattriax_core_KBoolean isInitial);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent thiz);
            } AttriaxRawDeepLinkEvent;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*AttriaxDeepLinkEvent)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong clickedAtMs, libattriax_core_KLong consumedAtMs, libattriax_core_KBoolean found, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger trigger, libattriax_core_KBoolean isAttriaxSubDomain, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus status, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent rawEvent, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction, libattriax_core_KBoolean handledBySdk);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*get_browserAction)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KLong (*get_clickedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KLong (*get_consumedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_data)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_found)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_handledBySdk)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_isAttriaxSubDomain)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_isColdStart)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_isDeferred)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*get_isForeground)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*get_rawEvent)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*get_status)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*get_trigger)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*get_uri)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_utm)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_kotlin_collections_Map (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KLong (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_KBoolean (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_kotlin_collections_Map (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong clickedAtMs, libattriax_core_KLong consumedAtMs, libattriax_core_KBoolean found, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger trigger, libattriax_core_KBoolean isAttriaxSubDomain, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus status, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent rawEvent, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction, libattriax_core_KBoolean handledBySdk);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent thiz);
            } AttriaxDeepLinkEvent;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult (*AttriaxDeepLinkResolutionResult)(libattriax_core_KBoolean matched, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus status, libattriax_core_KBoolean isFirstLaunch, const char* reason, libattriax_core_kref_kotlin_Long consumedAtMs, const char* path, const char* uri, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*get_browserAction)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_Long (*get_consumedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_data)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_KBoolean (*get_isFirstLaunch)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_KBoolean (*get_matched)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*get_path)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*get_reason)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*get_status)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*get_uri)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_utm)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_Long (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz, libattriax_core_KBoolean matched, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionStatus status, libattriax_core_KBoolean isFirstLaunch, const char* reason, libattriax_core_kref_kotlin_Long consumedAtMs, const char* path, const char* uri, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkResolutionResult thiz);
            } AttriaxDeepLinkResolutionResult;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects (*AttriaxDynamicLinkRedirects)(libattriax_core_kref_kotlin_Boolean ios, libattriax_core_kref_kotlin_Boolean android);
              libattriax_core_kref_kotlin_Boolean (*get_android)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
              libattriax_core_kref_kotlin_Boolean (*get_ios)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
              libattriax_core_kref_kotlin_Boolean (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
              libattriax_core_kref_kotlin_Boolean (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz, libattriax_core_kref_kotlin_Boolean ios, libattriax_core_kref_kotlin_Boolean android);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects thiz);
            } AttriaxDynamicLinkRedirects;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview (*AttriaxDynamicLinkSocialPreview)(const char* title, const char* description);
              const char* (*get_description)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
              const char* (*get_title)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz, const char* title, const char* description);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview thiz);
            } AttriaxDynamicLinkSocialPreview;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms (*AttriaxDynamicLinkUtms)(const char* source, const char* medium, const char* campaign, const char* term, const char* content);
              const char* (*get_campaign)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*get_content)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*get_medium)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*get_source)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*get_term)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz, const char* source, const char* medium, const char* campaign, const char* term, const char* content);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms thiz);
            } AttriaxDynamicLinkUtms;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord (*AttriaxDynamicLinkRecord)(const char* id, const char* path, const char* shortUrl, const char* name, const char* destinationUrl, const char* group, const char* prefix, libattriax_core_kref_kotlin_collections_Map data);
              libattriax_core_kref_kotlin_collections_Map (*get_data)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_destinationUrl)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_group)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_id)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_name)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_path)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_prefix)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*get_shortUrl)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              libattriax_core_kref_kotlin_collections_Map (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz, const char* id, const char* path, const char* shortUrl, const char* name, const char* destinationUrl, const char* group, const char* prefix, libattriax_core_kref_kotlin_collections_Map data);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord thiz);
            } AttriaxDynamicLinkRecord;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult (*AttriaxCreateDynamicLinkResult)(const char* shortUrl, libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord record);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord (*get_record)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
              const char* (*get_shortUrl)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz, const char* shortUrl, libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRecord record);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult thiz);
            } AttriaxCreateDynamicLinkResult;
            struct {
              libattriax_core_KType* (*_type)(void);
              void (*onDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent event);
            } AttriaxDeepLinkListener;
            struct {
              libattriax_core_KType* (*_type)(void);
              void (*onRawDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent event);
            } AttriaxRawDeepLinkListener;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*get_initialDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              libattriax_core_KBoolean (*get_initialDeepLinkResolved)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*get_latestDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent (*get_rawInitialDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              void (*addListener)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener listener);
              void (*addRawListener)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener listener);
              void (*completeInitialLinkIfAbsent)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxCreateDynamicLinkResult (*createDynamicLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, const char* name, const char* destinationUrl, const char* group, const char* prefix, libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkSocialPreview socialPreview, libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkUtms utms, libattriax_core_kref_com_attriax_sdk_AttriaxDynamicLinkRedirects redirects, libattriax_core_kref_kotlin_collections_Map data);
              void (*handleUri)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, const char* rawUri, libattriax_core_KBoolean isInitialLink);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*recordDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, const char* uri, libattriax_core_kref_kotlin_collections_Map metadata, const char* source);
              void (*removeListener)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkListener listener);
              void (*removeRawListener)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkListener listener);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*waitForInitialDeepLink)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkEvent (*waitResolution)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinks thiz, libattriax_core_kref_com_attriax_sdk_AttriaxRawDeepLinkEvent rawEvent);
            } AttriaxDeepLinks;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext (*AttriaxDeviceContext)(const char* model, const char* manufacturer, const char* osVersion, const char* timezone, const char* language, const char* brand, const char* hardware, const char* name, libattriax_core_kref_kotlin_Boolean isPhysicalDevice, libattriax_core_kref_kotlin_Int screenWidth, libattriax_core_kref_kotlin_Int screenHeight, const char* screenResolution, libattriax_core_kref_kotlin_Double devicePixelRatio, libattriax_core_kref_kotlin_Int colorDepth, libattriax_core_kref_kotlin_collections_List supportedAbis, libattriax_core_kref_kotlin_collections_Map metadata, const char* advertisingId, const char* androidId);
              const char* (*get_advertisingId)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_androidId)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_brand)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*get_colorDepth)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Double (*get_devicePixelRatio)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_hardware)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Boolean (*get_isPhysicalDevice)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_language)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_manufacturer)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_metadata)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_model)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_name)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_osVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*get_screenHeight)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_screenResolution)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*get_screenWidth)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_collections_List (*get_supportedAbis)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*get_timezone)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Double (*component13)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Int (*component14)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_collections_List (*component15)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_collections_Map (*component16)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component17)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component18)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_kotlin_Boolean (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz, const char* model, const char* manufacturer, const char* osVersion, const char* timezone, const char* language, const char* brand, const char* hardware, const char* name, libattriax_core_kref_kotlin_Boolean isPhysicalDevice, libattriax_core_kref_kotlin_Int screenWidth, libattriax_core_kref_kotlin_Int screenHeight, const char* screenResolution, libattriax_core_kref_kotlin_Double devicePixelRatio, libattriax_core_kref_kotlin_Int colorDepth, libattriax_core_kref_kotlin_collections_List supportedAbis, libattriax_core_kref_kotlin_collections_Map metadata, const char* advertisingId, const char* androidId);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDeviceContext thiz);
            } AttriaxDeviceContext;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult (*fromResponse)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult_Companion thiz, libattriax_core_kref_kotlin_Any decoded);
              } Companion;
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult (*AttriaxRevenueReceiptValidationResult)(const char* validationId, libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus status, libattriax_core_kref_kotlin_collections_Map publicReceipt, const char* requestVersion, libattriax_core_kref_kotlin_Long acceptedAtMs, const char* provider, const char* environment, const char* transactionId, const char* originalTransactionId, const char* productId, const char* failureReason, libattriax_core_kref_kotlin_Long expiresAtMs, libattriax_core_kref_kotlin_collections_Map providerResult);
              libattriax_core_kref_kotlin_Long (*get_acceptedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_environment)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_Long (*get_expiresAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_failureReason)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_originalTransactionId)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_productId)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_provider)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_providerResult)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_publicReceipt)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_requestVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get_status)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_transactionId)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*get_validationId)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_Long (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*component13)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_collections_Map (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_kotlin_Long (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz, const char* validationId, libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus status, libattriax_core_kref_kotlin_collections_Map publicReceipt, const char* requestVersion, libattriax_core_kref_kotlin_Long acceptedAtMs, const char* provider, const char* environment, const char* transactionId, const char* originalTransactionId, const char* productId, const char* failureReason, libattriax_core_kref_kotlin_Long expiresAtMs, libattriax_core_kref_kotlin_collections_Map providerResult);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationResult thiz);
            } AttriaxRevenueReceiptValidationResult;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for VERIFIED. */
              } VERIFIED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for REJECTED. */
              } REJECTED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for PENDING. */
              } PENDING;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for UNCONFIGURED. */
              } UNCONFIGURED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for PROVIDER_ERROR. */
              } PROVIDER_ERROR;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*get)(); /* enum entry for PASSTHROUGH. */
              } PASSTHROUGH;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus (*fromWire)(libattriax_core_kref_com_attriax_sdk_AttriaxRevenueReceiptValidationStatus_Companion thiz, const char* value);
              } Companion;
              libattriax_core_KType* (*_type)(void);
            } AttriaxRevenueReceiptValidationStatus;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails (*getLatestDeepLinkReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxReferrer thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails (*getOriginalInstallReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxReferrer thiz);
              const char* (*getRawInstallReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxReferrer thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails (*getReinstallReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxReferrer thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails (*getSessionReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxReferrer thiz);
            } AttriaxReferrer;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttributionType (*get)(); /* enum entry for REFERRER. */
              } REFERRER;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttributionType (*get)(); /* enum entry for FINGERPRINT. */
              } FINGERPRINT;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttributionType (*get)(); /* enum entry for EXTERNAL. */
              } EXTERNAL;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttributionType (*get)(); /* enum entry for ORGANIC. */
              } ORGANIC;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttributionType_Companion (*_instance)();
                libattriax_core_kref_com_attriax_sdk_AttributionType (*fromWire)(libattriax_core_kref_com_attriax_sdk_AttributionType_Companion thiz, const char* value);
              } Companion;
              libattriax_core_KType* (*_type)(void);
            } AttributionType;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails (*AttriaxInstallReferrerDetails)(const char* rawPlatformInstallReferrer, const char* source, const char* medium, const char* campaign, const char* term, const char* content, const char* adNetwork, const char* adClickId, libattriax_core_kref_com_attriax_sdk_AttributionType attributionType, const char* deepLinkUrl, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri deepLinkUri, libattriax_core_kref_kotlin_collections_Map deepLinkData, const char* registeredAt, libattriax_core_kref_kotlin_Long installBeginTimestampSeconds, libattriax_core_kref_kotlin_Long referrerClickTimestampSeconds, libattriax_core_kref_kotlin_Boolean googlePlayInstantParam, libattriax_core_KDouble precision);
              const char* (*get_adClickId)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_adNetwork)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttributionType (*get_attributionType)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_campaign)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_content)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_deepLinkData)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*get_deepLinkUri)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_deepLinkUrl)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Boolean (*get_googlePlayInstantParam)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Long (*get_installBeginTimestampSeconds)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_medium)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_KDouble (*get_precision)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_rawPlatformInstallReferrer)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Long (*get_referrerClickTimestampSeconds)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_registeredAt)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_source)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*get_term)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_utm)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component13)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Long (*component14)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Long (*component15)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_kotlin_Boolean (*component16)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_KDouble (*component17)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttributionType (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz, const char* rawPlatformInstallReferrer, const char* source, const char* medium, const char* campaign, const char* term, const char* content, const char* adNetwork, const char* adClickId, libattriax_core_kref_com_attriax_sdk_AttributionType attributionType, const char* deepLinkUrl, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri deepLinkUri, libattriax_core_kref_kotlin_collections_Map deepLinkData, const char* registeredAt, libattriax_core_kref_kotlin_Long installBeginTimestampSeconds, libattriax_core_kref_kotlin_Long referrerClickTimestampSeconds, libattriax_core_kref_kotlin_Boolean googlePlayInstantParam, libattriax_core_KDouble precision);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxInstallReferrerDetails thiz);
            } AttriaxInstallReferrerDetails;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails (*AttriaxDeepLinkReferrerDetails)(libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong receivedAtMs, libattriax_core_KLong clickedAtMs, libattriax_core_KLong consumedAtMs, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger trigger, libattriax_core_KBoolean isAttriaxDomain, libattriax_core_KBoolean found, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*get_browserAction)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*get_clickedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*get_consumedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_data)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KBoolean (*get_found)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KBoolean (*get_isAttriaxDomain)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*get_receivedAtMs)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*get_trigger)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*get_uri)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_utm)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KLong (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KBoolean (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_KBoolean (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_kotlin_collections_Map (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz, libattriax_core_kref_com_attriax_sdk_internal_deeplink_AttriaxUri uri, libattriax_core_KLong receivedAtMs, libattriax_core_KLong clickedAtMs, libattriax_core_KLong consumedAtMs, libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkTrigger trigger, libattriax_core_KBoolean isAttriaxDomain, libattriax_core_KBoolean found, libattriax_core_kref_kotlin_collections_Map data, libattriax_core_kref_kotlin_collections_Map utm, libattriax_core_kref_com_attriax_sdk_AttriaxBrowserAction browserAction);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxDeepLinkReferrerDetails thiz);
            } AttriaxDeepLinkReferrerDetails;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot (*AttriaxSdkSnapshot)(const char* apiVersion, const char* packageVersion, libattriax_core_kref_kotlin_collections_Map metadata);
              const char* (*get_apiVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              libattriax_core_kref_kotlin_collections_Map (*get_metadata)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              const char* (*get_packageVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              libattriax_core_kref_kotlin_collections_Map (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz, const char* apiVersion, const char* packageVersion, libattriax_core_kref_kotlin_collections_Map metadata);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSdkSnapshot thiz);
            } AttriaxSdkSnapshot;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanState (*get_state)(libattriax_core_kref_com_attriax_sdk_AttriaxSkan thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig (*fetchConversionConfig)(libattriax_core_kref_com_attriax_sdk_AttriaxSkan thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult (*updateConversionValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkan thiz, libattriax_core_KInt fineValue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_KBoolean lockWindow);
            } AttriaxSkan;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get)(); /* enum entry for LOW. */
              } LOW;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get)(); /* enum entry for MEDIUM. */
              } MEDIUM;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get)(); /* enum entry for HIGH. */
              } HIGH;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_wireValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue thiz);
            } AttriaxSkanCoarseValue;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for UPDATED. */
              } UPDATED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for SKIPPED. */
              } SKIPPED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for ALREADY_AT_OR_ABOVE_VALUE. */
              } ALREADY_AT_OR_ABOVE_VALUE;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for INVALID_VALUE. */
              } INVALID_VALUE;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for DISABLED. */
              } DISABLED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for NOT_SUPPORTED. */
              } NOT_SUPPORTED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get)(); /* enum entry for ERROR. */
              } ERROR;
              libattriax_core_KType* (*_type)(void);
              const char* (*get_wireValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus thiz);
            } AttriaxSkanUpdateStatus;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult (*AttriaxSkanUpdateResult)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus status, const char* message, libattriax_core_kref_kotlin_Int fineValue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_kref_kotlin_Boolean lockWindow);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get_coarseValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_kotlin_Int (*get_fineValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_kotlin_Boolean (*get_lockWindow)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              const char* (*get_message)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*get_status)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_kotlin_Int (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_kotlin_Boolean (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz, libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateStatus status, const char* message, libattriax_core_kref_kotlin_Int fineValue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_kref_kotlin_Boolean lockWindow);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanUpdateResult thiz);
            } AttriaxSkanUpdateResult;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanState (*AttriaxSkanState)(libattriax_core_KBoolean enabled, libattriax_core_kref_kotlin_Int fineValue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_KBoolean lockWindow);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get_coarseValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_KBoolean (*get_enabled)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_kref_kotlin_Int (*get_fineValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_KBoolean (*get_lockWindow)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_kref_kotlin_Int (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_KBoolean (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanState (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz, libattriax_core_KBoolean enabled, libattriax_core_kref_kotlin_Int fineValue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_KBoolean lockWindow);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanState thiz);
            } AttriaxSkanState;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig (*AttriaxSkanConfig)(libattriax_core_KBoolean enabled, libattriax_core_KBoolean registerFirstLaunchValue);
              libattriax_core_KBoolean (*get_enabled)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
              libattriax_core_KBoolean (*get_registerFirstLaunchValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
              libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
              libattriax_core_KBoolean (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz, libattriax_core_KBoolean enabled, libattriax_core_KBoolean registerFirstLaunchValue);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConfig thiz);
            } AttriaxSkanConfig;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig (*AttriaxSkanConversionConfig)(libattriax_core_kref_kotlin_Int schemaVersion, const char* schemaUpdatedAt, libattriax_core_KBoolean enabled, libattriax_core_kref_kotlin_collections_List rules, const char* disclaimer);
              const char* (*get_disclaimer)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_KBoolean (*get_enabled)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_kref_kotlin_collections_List (*get_rules)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              const char* (*get_schemaUpdatedAt)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_kref_kotlin_Int (*get_schemaVersion)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_kref_kotlin_Int (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_KBoolean (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_kref_kotlin_collections_List (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              const char* (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz, libattriax_core_kref_kotlin_Int schemaVersion, const char* schemaUpdatedAt, libattriax_core_KBoolean enabled, libattriax_core_kref_kotlin_collections_List rules, const char* disclaimer);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanConversionConfig thiz);
            } AttriaxSkanConversionConfig;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule (*AttriaxSkanCvRule)(const char* id, const char* groupId, const char* groupDisplayName, libattriax_core_KInt startBit, libattriax_core_KInt bitCount, libattriax_core_KInt rank, libattriax_core_KInt bitContribution, const char* whenEvent, libattriax_core_kref_kotlin_collections_List whenConditions, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition whenRevenue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_KBoolean lockWindow);
              libattriax_core_KInt (*get_bitContribution)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*get_bitCount)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*get_coarseValue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*get_groupDisplayName)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*get_groupId)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*get_id)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KBoolean (*get_lockWindow)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*get_rank)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*get_startBit)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_kotlin_collections_List (*get_whenConditions)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*get_whenEvent)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition (*get_whenRevenue)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition (*component10)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue (*component11)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KBoolean (*component12)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*component4)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*component5)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*component6)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_KInt (*component7)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*component8)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_kotlin_collections_List (*component9)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz, const char* id, const char* groupId, const char* groupDisplayName, libattriax_core_KInt startBit, libattriax_core_KInt bitCount, libattriax_core_KInt rank, libattriax_core_KInt bitContribution, const char* whenEvent, libattriax_core_kref_kotlin_collections_List whenConditions, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition whenRevenue, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCoarseValue coarseValue, libattriax_core_KBoolean lockWindow);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRule thiz);
            } AttriaxSkanCvRule;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition (*AttriaxSkanCvCondition)(const char* paramKey, const char* operator_, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue value);
              const char* (*get_operator)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              const char* (*get_paramKey)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue (*get_value)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              const char* (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue (*component3)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz, const char* paramKey, const char* operator_, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue value);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvCondition thiz);
            } AttriaxSkanCvCondition;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition (*AttriaxSkanCvRevenueCondition)(const char* operator_, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue value);
              const char* (*get_operator)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue (*get_value)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
              const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue (*component2)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz, const char* operator_, libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue value);
              libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz, libattriax_core_kref_kotlin_Any other);
              libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
              const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvRevenueCondition thiz);
            } AttriaxSkanCvRevenueCondition;
            struct {
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue (*StringValue)(const char* value);
                const char* (*get_value)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz);
                const char* (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz, const char* value);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_StringValue thiz);
              } StringValue;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue (*NumberValue)(libattriax_core_KDouble value);
                libattriax_core_KDouble (*get_value)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz);
                libattriax_core_KDouble (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz, libattriax_core_KDouble value);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_NumberValue thiz);
              } NumberValue;
              struct {
                libattriax_core_KType* (*_type)(void);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue (*BoolValue)(libattriax_core_KBoolean value);
                libattriax_core_KBoolean (*get_value)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz);
                libattriax_core_KBoolean (*component1)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz);
                libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue (*copy)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz, libattriax_core_KBoolean value);
                libattriax_core_KBoolean (*equals)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz, libattriax_core_kref_kotlin_Any other);
                libattriax_core_KInt (*hashCode)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz);
                const char* (*toString)(libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue_BoolValue thiz);
              } BoolValue;
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxSkanCvValue (*AttriaxSkanCvValue)();
            } AttriaxSkanCvValue;
            struct {
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for INITIALIZING. */
              } INITIALIZING;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for SYNCHRONIZING. */
              } SYNCHRONIZING;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for DEFERRED. */
              } DEFERRED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for SYNCHRONIZED. */
              } SYNCHRONIZED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for OFFLINE. */
              } OFFLINE;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for FAILED. */
              } FAILED;
              struct {
                libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get)(); /* enum entry for DISABLED. */
              } DISABLED;
              libattriax_core_KType* (*_type)(void);
            } AttriaxSynchronizationState;
            struct {
              libattriax_core_KType* (*_type)(void);
              void (*onSynchronizationStateChanged)(libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationStateListener thiz, libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState state);
            } AttriaxSynchronizationStateListener;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_KBoolean (*get_isSynchronized)(libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization thiz);
              libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationState (*get_state)(libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization thiz);
              void (*addStateListener)(libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization thiz, libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationStateListener listener);
              void (*removeStateListener)(libattriax_core_kref_com_attriax_sdk_AttriaxSynchronization thiz, libattriax_core_kref_com_attriax_sdk_AttriaxSynchronizationStateListener listener);
            } AttriaxSynchronization;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_KBoolean (*get_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz);
              void (*set_anonymousTrackingEnabled)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_KBoolean value);
              libattriax_core_KBoolean (*get_enabled)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz);
              void (*set_enabled)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_KBoolean value);
              void (*clearUserProperties)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_kref_kotlin_collections_List propertyNames);
              void (*recordAdEvent)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_kref_com_attriax_sdk_AttriaxAdEventType type, const char* adNetwork, const char* mediationNetwork, const char* adUnitId, const char* adPlacement, const char* adFormat, const char* adType, const char* failureReason, libattriax_core_kref_kotlin_Double loadLatencyMs, const char* rewardType, libattriax_core_kref_kotlin_Double rewardAmount, libattriax_core_kref_kotlin_Boolean test, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordAdRevenue)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_KDouble revenue, const char* currency, libattriax_core_KBoolean revenueInMicros, const char* adNetwork, const char* adFormat, const char* adType, const char* adPlacement, libattriax_core_kref_kotlin_Boolean test, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordError)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_kref_kotlin_Throwable error, const char* stackTrace, libattriax_core_KBoolean fatal, const char* source, const char* reason, libattriax_core_kref_kotlin_collections_Map metadata);
              void (*recordEvent)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* name, libattriax_core_kref_kotlin_collections_Map eventData, libattriax_core_KBoolean flushImmediately);
              void (*recordNotification)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventType type, const char* notificationId, const char* linkId, const char* campaignId, const char* title, libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource source, libattriax_core_kref_kotlin_collections_Map payload, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordNotificationDismissed)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* notificationId, const char* linkId, const char* campaignId, const char* title, libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource source, libattriax_core_kref_kotlin_collections_Map payload, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordNotificationOpened)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* notificationId, const char* linkId, const char* campaignId, const char* title, libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource source, libattriax_core_kref_kotlin_collections_Map payload, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordNotificationReceived)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* notificationId, const char* linkId, const char* campaignId, const char* title, libattriax_core_kref_com_attriax_sdk_AttriaxNotificationEventSource source, libattriax_core_kref_kotlin_collections_Map payload, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordPageView)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* pageName, const char* pageClass, const char* pageTitle, const char* previousPageName, libattriax_core_kref_kotlin_collections_Map parameters, const char* source, libattriax_core_KBoolean flushImmediately);
              void (*recordPurchase)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_KDouble revenue, const char* currency, libattriax_core_KBoolean revenueInMicros, const char* purchaseType, const char* productId, const char* transactionId, const char* originalTransactionId, const char* validationProvider, const char* validationEnvironment, const char* purchaseToken, const char* receiptData, const char* signedPayload, const char* receiptSignature, libattriax_core_kref_kotlin_Boolean isRenewal, libattriax_core_KInt quantity, const char* store, const char* packageName, libattriax_core_kref_kotlin_Boolean voided, libattriax_core_kref_kotlin_Boolean test, const char* validationId, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*recordRefund)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_KDouble revenue, const char* currency, libattriax_core_KBoolean revenueInMicros, const char* purchaseType, const char* productId, const char* transactionId, const char* originalTransactionId, libattriax_core_KInt quantity, const char* store, const char* packageName, libattriax_core_kref_kotlin_Boolean voided, libattriax_core_kref_kotlin_Boolean test, const char* reason, libattriax_core_kref_kotlin_collections_Map metadata, libattriax_core_KBoolean flushImmediately);
              void (*registerApplePushToken)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* token, libattriax_core_kref_kotlin_collections_Map metadata);
              void (*registerFirebaseMessagingToken)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* token, libattriax_core_kref_kotlin_collections_Map metadata);
              void (*setUser)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* userId, const char* userName);
              void (*setUserProperties)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, libattriax_core_kref_kotlin_collections_Map properties);
              void (*setUserProperty)(libattriax_core_kref_com_attriax_sdk_AttriaxTracking thiz, const char* name, libattriax_core_kref_kotlin_Any value);
            } AttriaxTracking;
            struct {
              libattriax_core_KType* (*_type)(void);
              libattriax_core_kref_com_attriax_sdk_AttriaxVersion (*_instance)();
              const char* (*get_API_VERSION)(libattriax_core_kref_com_attriax_sdk_AttriaxVersion thiz);
              const char* (*get_PACKAGE_VERSION)(libattriax_core_kref_com_attriax_sdk_AttriaxVersion thiz);
            } AttriaxVersion;
            void* (*attriaxCreate)(void* configJson, void* dataDir);
            void (*attriaxDestroy)(void* handle);
            void* (*attriaxDispatch)(void* handle, void* method, void* argsJson);
            void (*attriaxFreeString)(void* ptr);
            void (*attriaxRegisterEventCallback)(void* handle, void* callback, void* userData);
          } sdk;
        } attriax;
      } com;
    } root;
  } kotlin;
} libattriax_core_ExportedSymbols;
extern libattriax_core_ExportedSymbols* libattriax_core_symbols(void);
#ifdef __cplusplus
}  /* extern "C" */
#endif
#endif  /* KONAN_LIBATTRIAX_CORE_H */
