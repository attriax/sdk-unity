#import <Foundation/Foundation.h>
#import <SafariServices/SafariServices.h>
#import <StoreKit/SKAdNetwork.h>
#import <Security/Security.h>
#import <TargetConditionals.h>
#import <UIKit/UIKit.h>
#if __has_include(<WebKit/WebKit.h>)
#import <WebKit/WebKit.h>
#endif
#import <sys/utsname.h>
#if __has_include(<AdSupport/ASIdentifierManager.h>)
#import <AdSupport/ASIdentifierManager.h>
#endif
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#endif
// The KMP core engine, shipped as the AttriaxCore XCFramework (iosMain actuals). Guarded
// so this file still compiles in a Unity project that has not yet embedded the framework.
#if __has_include(<AttriaxCore/AttriaxCore.h>)
#import <AttriaxCore/AttriaxCore.h>
#define ATTRIAX_HAS_KMP_ENGINE 1
#endif

static char *AttriaxUnityMakeCString(NSString *value) {
    if (value == nil) {
        return NULL;
    }

    const char *utf8 = [value UTF8String];
    if (utf8 == NULL) {
        return NULL;
    }

    size_t length = strlen(utf8) + 1;
    char *buffer = (char *)malloc(length);
    if (buffer == NULL) {
        return NULL;
    }

    memcpy(buffer, utf8, length);
    return buffer;
}

static NSString *AttriaxUnityTrimmedString(NSString *value) {
    if (value == nil) {
        return nil;
    }

    NSString *trimmed = [value stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
    return trimmed.length > 0 ? trimmed : nil;
}

static NSString *AttriaxUnityCachedWebViewUserAgent = nil;

static NSString *AttriaxUnitySerializeDictionary(NSDictionary *dictionary) {
    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary options:0 error:&error];
    if (jsonData == nil || error != nil) {
        return @"{}";
    }

    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

// iOS has no SecTask entitlement API (that is macOS-only). Entitlements are read
// best-effort from the embedded provisioning profile, which exists for development,
// ad-hoc, and enterprise builds. App Store builds strip it, so these fields are
// simply absent there — the KMP engine treats them as optional metadata.
static NSDictionary *AttriaxUnityEmbeddedEntitlements(void) {
    static NSDictionary *cached = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSString *path = [NSBundle.mainBundle pathForResource:@"embedded" ofType:@"mobileprovision"];
        if (path == nil) {
            return;
        }

        NSData *data = [NSData dataWithContentsOfFile:path];
        if (data == nil) {
            return;
        }

        // The profile is a CMS/PKCS#7 envelope; the embedded plist sits between the
        // XML prolog and the closing </plist> tag. Slice it out and parse it.
        NSString *raw = [[NSString alloc] initWithData:data encoding:NSISOLatin1StringEncoding];
        NSRange start = [raw rangeOfString:@"<?xml"];
        NSRange end = [raw rangeOfString:@"</plist>"];
        if (start.location == NSNotFound || end.location == NSNotFound || end.location < start.location) {
            return;
        }

        NSRange plistRange = NSMakeRange(start.location, NSMaxRange(end) - start.location);
        NSData *plistData = [[raw substringWithRange:plistRange] dataUsingEncoding:NSISOLatin1StringEncoding];
        NSDictionary *profile = [NSPropertyListSerialization propertyListWithData:plistData
                                                                          options:NSPropertyListImmutable
                                                                           format:nil
                                                                            error:nil];
        id entitlements = profile[@"Entitlements"];
        if ([entitlements isKindOfClass:[NSDictionary class]]) {
            cached = (NSDictionary *)entitlements;
        }
    });

    return cached;
}

static id AttriaxUnityReadEntitlementValue(NSString *key) {
    return AttriaxUnityEmbeddedEntitlements()[key];
}

static NSString *AttriaxUnityReadEntitlementString(NSString *key) {
    id value = AttriaxUnityReadEntitlementValue(key);
    return [value isKindOfClass:[NSString class]] ? (NSString *)value : nil;
}

static NSArray<NSString *> *AttriaxUnityReadEntitlementStringArray(NSString *key) {
    id value = AttriaxUnityReadEntitlementValue(key);
    if ([value isKindOfClass:[NSArray class]]) {
        NSMutableArray<NSString *> *result = [NSMutableArray array];
        for (id item in (NSArray *)value) {
            if ([item isKindOfClass:[NSString class]]) {
                [result addObject:(NSString *)item];
            }
        }
        return result;
    }

    return @[];
}

static NSString *AttriaxUnityReadKeychainDeviceId(void) {
    NSString *service = NSBundle.mainBundle.bundleIdentifier ?: @"com.attriax.sdk";
    NSString *account = @"attriax.device_id";
    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: service,
        (__bridge id)kSecAttrAccount: account,
        (__bridge id)kSecReturnData: @YES,
        (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne,
    };

    CFTypeRef result = nil;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, &result);
    if (status != errSecSuccess || result == nil) {
        if (result != nil) {
            CFRelease(result);
        }
        return nil;
    }

    NSData *data = CFBridgingRelease(result);
    if (![data isKindOfClass:[NSData class]]) {
        return nil;
    }

    NSString *value = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    NSString *trimmed = [value stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
    return trimmed.length > 0 ? trimmed : nil;
}

static NSString *AttriaxUnityHardwareModel(void) {
    struct utsname systemInfo;
    if (uname(&systemInfo) != 0) {
        return nil;
    }

    return [NSString stringWithCString:systemInfo.machine encoding:NSUTF8StringEncoding];
}

static UIWindow *AttriaxUnityActiveWindow(void) {
    if (@available(iOS 13.0, *)) {
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (![scene isKindOfClass:[UIWindowScene class]]) {
                continue;
            }

            for (UIWindow *window in ((UIWindowScene *)scene).windows) {
                if (window.isKeyWindow) {
                    return window;
                }
            }
        }
        return nil;
    }

    return UIApplication.sharedApplication.keyWindow;
}

static UIViewController *AttriaxUnityTopViewController(UIViewController *root) {
    if (root == nil) {
        return nil;
    }

    if ([root isKindOfClass:[UINavigationController class]]) {
        return AttriaxUnityTopViewController(((UINavigationController *)root).visibleViewController);
    }

    if ([root isKindOfClass:[UITabBarController class]]) {
        return AttriaxUnityTopViewController(((UITabBarController *)root).selectedViewController);
    }

    if (root.presentedViewController != nil) {
        return AttriaxUnityTopViewController(root.presentedViewController);
    }

    return root;
}

static NSString *AttriaxUnityNormalizeSkanCoarseValue(const char *value) {
    if (value == NULL) {
        return nil;
    }

    NSString *coarseValue = [[NSString stringWithUTF8String:value] lowercaseString];
    if ([coarseValue isEqualToString:@"low"] ||
        [coarseValue isEqualToString:@"medium"] ||
        [coarseValue isEqualToString:@"high"]) {
        return coarseValue;
    }

    return nil;
}

static NSMutableDictionary *AttriaxUnityCreateSkanResult(
    NSString *status,
    NSString *message,
    NSInteger fineValue,
    NSString *coarseValue,
    BOOL lockWindow) {
    NSMutableDictionary *result = [NSMutableDictionary dictionary];
    result[@"status"] = status ?: @"error";
    result[@"fineValue"] = @(fineValue);
    result[@"lockWindow"] = @(lockWindow);

    if (coarseValue.length > 0) {
        result[@"coarseValue"] = coarseValue;
    }

    if (message.length > 0) {
        result[@"message"] = message;
    }

    return result;
}

static void AttriaxUnityRunOnMainThreadSync(dispatch_block_t block) {
    if (block == nil) {
        return;
    }

    if ([NSThread isMainThread]) {
        block();
    } else {
        dispatch_sync(dispatch_get_main_queue(), block);
    }
}

static SKAdNetworkCoarseConversionValue AttriaxUnityParseSkanCoarseValue(NSString *value) API_AVAILABLE(ios(16.1)) {
    if ([value isEqualToString:@"high"]) {
        return SKAdNetworkCoarseConversionValueHigh;
    }

    if ([value isEqualToString:@"medium"]) {
        return SKAdNetworkCoarseConversionValueMedium;
    }

    return SKAdNetworkCoarseConversionValueLow;
}

static NSString *AttriaxUnityTrackingAuthorizationStatusString(void) {
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
    if (@available(iOS 14.0, *)) {
        switch (ATTrackingManager.trackingAuthorizationStatus) {
            case ATTrackingManagerAuthorizationStatusNotDetermined:
                return @"not_determined";
            case ATTrackingManagerAuthorizationStatusRestricted:
                return @"restricted";
            case ATTrackingManagerAuthorizationStatusDenied:
                return @"denied";
            case ATTrackingManagerAuthorizationStatusAuthorized:
                return @"authorized";
        }
    }
#endif

    return @"not_supported";
}

static NSString *AttriaxUnityReadAdvertisingIdentifier(BOOL collectAdvertisingId) {
    if (!collectAdvertisingId) {
        return nil;
    }

#if __has_include(<AdSupport/ASIdentifierManager.h>)
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
    if (@available(iOS 14.0, *)) {
        if (ATTrackingManager.trackingAuthorizationStatus != ATTrackingManagerAuthorizationStatusAuthorized) {
            return nil;
        }
    }
#endif

    NSString *value = ASIdentifierManager.sharedManager.advertisingIdentifier.UUIDString;
    if ([value isEqualToString:@"00000000-0000-0000-0000-000000000000"]) {
        return nil;
    }

    return value;
#else
    return nil;
#endif
}

extern "C" char *AttriaxUnity_CopyNativeContextJson(bool collectAdvertisingId) {
    @autoreleasepool {
        UIDevice *device = UIDevice.currentDevice;
        UIScreen *screen = UIScreen.mainScreen;
        CGRect bounds = screen.bounds;
        NSMutableDictionary *metadata = [NSMutableDictionary dictionary];
        NSMutableDictionary *payload = [NSMutableDictionary dictionary];

        metadata[@"source"] = @"ios_native";
        metadata[@"timezone"] = NSTimeZone.localTimeZone.name ?: @"";
        metadata[@"locale"] = NSLocale.currentLocale.localeIdentifier ?: @"";
        metadata[@"regionCode"] = NSLocale.currentLocale.countryCode ?: [NSNull null];
        metadata[@"preferredLanguages"] = NSLocale.preferredLanguages ?: @[];
        metadata[@"keychainDeviceId"] = AttriaxUnityReadKeychainDeviceId() ?: [NSNull null];
        metadata[@"vendorIdentifier"] = device.identifierForVendor.UUIDString ?: [NSNull null];
        metadata[@"deviceModel"] = device.model ?: @"";
        metadata[@"deviceName"] = device.name ?: @"";
        metadata[@"localizedModel"] = device.localizedModel ?: @"";
        metadata[@"bundleIdentifier"] = NSBundle.mainBundle.bundleIdentifier ?: [NSNull null];
        metadata[@"systemName"] = device.systemName ?: @"";
        metadata[@"systemVersion"] = device.systemVersion ?: @"";
        metadata[@"screenWidthPoints"] = @(bounds.size.width);
        metadata[@"screenHeightPoints"] = @(bounds.size.height);
        metadata[@"screenScale"] = @(screen.scale);
        metadata[@"isLowPowerModeEnabled"] = @(NSProcessInfo.processInfo.isLowPowerModeEnabled);

#if TARGET_OS_SIMULATOR
        metadata[@"isSimulator"] = @YES;
#else
        metadata[@"isSimulator"] = @NO;
#endif

        NSString *hardwareModel = AttriaxUnityHardwareModel();
        if (hardwareModel != nil) {
            metadata[@"hardwareModel"] = hardwareModel;
        }

        NSString *advertisingIdentifier = AttriaxUnityReadAdvertisingIdentifier(collectAdvertisingId);
        if (advertisingIdentifier != nil) {
            payload[@"advertisingId"] = advertisingIdentifier;
        }

        NSString *applicationIdentifier = AttriaxUnityReadEntitlementString(@"application-identifier");
        if (applicationIdentifier != nil) {
            metadata[@"applicationIdentifier"] = applicationIdentifier;
            NSArray<NSString *> *parts = [applicationIdentifier componentsSeparatedByString:@"."];
            if (parts.count > 0) {
                metadata[@"teamIdentifier"] = parts.firstObject;
            }
        }

        NSString *explicitTeamIdentifier = AttriaxUnityReadEntitlementString(@"com.apple.developer.team-identifier");
        if (explicitTeamIdentifier != nil) {
            metadata[@"teamIdentifier"] = explicitTeamIdentifier;
        }

        NSArray<NSString *> *associatedDomains = AttriaxUnityReadEntitlementStringArray(@"com.apple.developer.associated-domains");
        if (associatedDomains.count > 0) {
            metadata[@"associatedDomains"] = associatedDomains;
        }

        switch (device.userInterfaceIdiom) {
            case UIUserInterfaceIdiomPhone:
                metadata[@"interfaceIdiom"] = @"phone";
                break;
            case UIUserInterfaceIdiomPad:
                metadata[@"interfaceIdiom"] = @"pad";
                break;
            case UIUserInterfaceIdiomMac:
                metadata[@"interfaceIdiom"] = @"mac";
                break;
            case UIUserInterfaceIdiomTV:
                metadata[@"interfaceIdiom"] = @"tv";
                break;
            case UIUserInterfaceIdiomCarPlay:
                metadata[@"interfaceIdiom"] = @"carPlay";
                break;
            default:
                metadata[@"interfaceIdiom"] = @"unspecified";
                break;
        }

        payload[@"metadata"] = metadata;
        return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(payload));
    }
}

extern "C" char *AttriaxUnity_CopyInstallReferrerJson(void) {
    @autoreleasepool {
        NSDictionary *payload = @{
            @"metadata": @{
                @"source": @"ios_install_referrer",
                @"installReferrerStatus": @"unsupported_ios"
            }
        };
        return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(payload));
    }
}

extern "C" char *AttriaxUnity_CopyAttributionClipboardText(void) {
    @autoreleasepool {
        __block NSString *clipboardText = nil;
        AttriaxUnityRunOnMainThreadSync(^{
            clipboardText = AttriaxUnityTrimmedString(UIPasteboard.generalPasteboard.string);
        });
        return AttriaxUnityMakeCString(clipboardText);
    }
}

extern "C" char *AttriaxUnity_CopyWebViewUserAgent(void) {
    @autoreleasepool {
        __block NSString *userAgent = nil;

        AttriaxUnityRunOnMainThreadSync(^{
            if (AttriaxUnityCachedWebViewUserAgent.length > 0) {
                userAgent = AttriaxUnityCachedWebViewUserAgent;
                return;
            }

#if __has_include(<WebKit/WebKit.h>)
            __block BOOL completed = NO;
            WKWebView *webView = [[WKWebView alloc] initWithFrame:CGRectZero];
            [webView evaluateJavaScript:@"navigator.userAgent" completionHandler:^(id value, __unused NSError *error) {
                NSString *resolvedUserAgent = [value isKindOfClass:[NSString class]]
                    ? AttriaxUnityTrimmedString((NSString *)value)
                    : nil;
                if (resolvedUserAgent.length > 0) {
                    AttriaxUnityCachedWebViewUserAgent = resolvedUserAgent;
                    userAgent = resolvedUserAgent;
                }
                completed = YES;
            }];

            NSDate *deadline = [NSDate dateWithTimeIntervalSinceNow:2.0];
            while (!completed && [deadline timeIntervalSinceNow] > 0) {
                [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode beforeDate:[NSDate dateWithTimeIntervalSinceNow:0.01]];
            }
#endif
        });

        return AttriaxUnityMakeCString(userAgent);
    }
}

extern "C" bool AttriaxUnity_OpenBrowserUrl(const char *url, const char *openMode) {
    @autoreleasepool {
        if (url == NULL) {
            return false;
        }

        NSString *urlString = [NSString stringWithUTF8String:url];
        if (urlString.length == 0) {
            return false;
        }

        NSURL *targetUrl = [NSURL URLWithString:urlString];
        if (targetUrl == nil) {
            return false;
        }

        NSString *mode = openMode == NULL
            ? @"in_app"
            : [NSString stringWithUTF8String:openMode];
        __block bool opened = false;

        void (^presentBlock)(void) = ^{
            if ([mode isEqualToString:@"external"]) {
                UIApplication *application = UIApplication.sharedApplication;
                if ([application canOpenURL:targetUrl]) {
                    [application openURL:targetUrl options:@{} completionHandler:nil];
                    opened = true;
                }
                return;
            }

            UIViewController *presenter = AttriaxUnityTopViewController(AttriaxUnityActiveWindow().rootViewController);
            if (presenter == nil) {
                return;
            }

            SFSafariViewController *controller = [[SFSafariViewController alloc] initWithURL:targetUrl];
            [presenter presentViewController:controller animated:YES completion:nil];
            opened = true;
        };

        if ([NSThread isMainThread]) {
            presentBlock();
        } else {
            dispatch_sync(dispatch_get_main_queue(), presentBlock);
        }

        return opened;
    }
}

extern "C" char *AttriaxUnity_CopySkanUpdateResultJson(int fineValue, const char *coarseValue, bool lockWindow) {
    @autoreleasepool {
        NSString *normalizedCoarseValue = AttriaxUnityNormalizeSkanCoarseValue(coarseValue);

        if (@available(iOS 16.1, *)) {
            __block NSString *status = @"updated";
            __block NSString *message = nil;
            __block NSString *appliedCoarseValue = normalizedCoarseValue;
            __block BOOL appliedLockWindow = lockWindow;
            dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);

            AttriaxUnityRunOnMainThreadSync(^{
                if (normalizedCoarseValue.length > 0) {
                    [SKAdNetwork updatePostbackConversionValue:(NSInteger)fineValue
                                                    coarseValue:AttriaxUnityParseSkanCoarseValue(normalizedCoarseValue)
                                                     lockWindow:lockWindow
                                              completionHandler:^(NSError *error) {
                        if (error != nil) {
                            status = @"error";
                            message = error.localizedDescription ?: @"Failed to update the SKAdNetwork conversion value.";
                        }
                        dispatch_semaphore_signal(semaphore);
                    }];
                    return;
                }

                [SKAdNetwork updatePostbackConversionValue:(NSInteger)fineValue
                                         completionHandler:^(NSError *error) {
                    if (error != nil) {
                        status = @"error";
                        message = error.localizedDescription ?: @"Failed to update the SKAdNetwork conversion value.";
                    }
                    dispatch_semaphore_signal(semaphore);
                }];
            });

            long waitResult = dispatch_semaphore_wait(
                semaphore,
                dispatch_time(DISPATCH_TIME_NOW, (int64_t)(2 * NSEC_PER_SEC)));
            if (waitResult != 0) {
                status = @"skipped";
                message = @"Timed out waiting for the SKAdNetwork completion callback.";
            }

            return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(
                AttriaxUnityCreateSkanResult(
                    status,
                    message,
                    fineValue,
                    appliedCoarseValue,
                    appliedLockWindow)));
        }

        if (@available(iOS 15.4, *)) {
            __block NSString *status = @"updated";
            __block NSString *message = nil;
            NSString *appliedCoarseValue = nil;
            BOOL appliedLockWindow = NO;
            if (normalizedCoarseValue.length > 0 || lockWindow) {
                status = @"skipped";
                message = @"Coarse values and lock windows require iOS 16.1 or newer. The fine value was updated only.";
            }

            dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
            AttriaxUnityRunOnMainThreadSync(^{
                [SKAdNetwork updatePostbackConversionValue:(NSInteger)fineValue
                                         completionHandler:^(NSError *error) {
                    if (error != nil) {
                        status = @"error";
                        message = error.localizedDescription ?: @"Failed to update the SKAdNetwork conversion value.";
                    }
                    dispatch_semaphore_signal(semaphore);
                }];
            });

            long waitResult = dispatch_semaphore_wait(
                semaphore,
                dispatch_time(DISPATCH_TIME_NOW, (int64_t)(2 * NSEC_PER_SEC)));
            if (waitResult != 0 && [status isEqualToString:@"updated"]) {
                status = @"skipped";
                message = @"Timed out waiting for the SKAdNetwork completion callback.";
            }

            return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(
                AttriaxUnityCreateSkanResult(
                    status,
                    message,
                    fineValue,
                    appliedCoarseValue,
                    appliedLockWindow)));
        }

        if (@available(iOS 14.0, *)) {
            AttriaxUnityRunOnMainThreadSync(^{
                [SKAdNetwork updateConversionValue:(NSInteger)fineValue];
            });

            NSString *status = @"updated";
            NSString *message = nil;
            if (normalizedCoarseValue.length > 0 || lockWindow) {
                status = @"skipped";
                message = @"Coarse values and lock windows require iOS 16.1 or newer. The fine value was updated only.";
            }

            return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(
                AttriaxUnityCreateSkanResult(
                    status,
                    message,
                    fineValue,
                    nil,
                    NO)));
        }

        return AttriaxUnityMakeCString(AttriaxUnitySerializeDictionary(
            AttriaxUnityCreateSkanResult(
                @"not_supported",
                @"SKAdNetwork conversion-value updates require iOS 14.0 or newer.",
                fineValue,
                nil,
                NO)));
    }
}

extern "C" void AttriaxUnity_FreeString(void *value) {
    if (value != NULL) {
        free(value);
    }
}

extern "C" char *AttriaxUnity_CopyTrackingAuthorizationStatusString(void) {
    @autoreleasepool {
        return AttriaxUnityMakeCString(AttriaxUnityTrackingAuthorizationStatusString());
    }
}

extern "C" char *AttriaxUnity_RequestTrackingAuthorization(void) {
    @autoreleasepool {
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
        if (@available(iOS 14.0, *)) {
            dispatch_async(dispatch_get_main_queue(), ^{
                [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(__unused ATTrackingManagerAuthorizationStatus status) {
                }];
            });
            return AttriaxUnityMakeCString(AttriaxUnityTrackingAuthorizationStatusString());
        }
#endif

        return AttriaxUnityMakeCString(@"not_supported");
    }
}

// ============================================================================
//  KMP engine C-ABI bridge (Phase 6, iOS binding).
// ============================================================================
//
//  Holds the KMP `Attriax` engine — built via the ObjC-exported `AttriaxApple`
//  factory in the embedded AttriaxCore XCFramework — behind a `[DllImport("__Internal")]`
//  C surface that Unity's `IAttriaxEnginePlatform` (iOS impl) P/Invokes. The engine
//  handle is an opaque `void*` (a bridging-retained `AttriaxCoreAttriax*`); the sync
//  callback returns via a C function pointer registered from C# as an
//  `[AOT.MonoPInvokeCallback]` static method.
//
//  ⚠️  CODE-ONLY / UNVERIFIED: the Unity Editor on this project crashes at init
//  (PackageManager forbidden-folder issue), and there is no iOS-Unity Xcode export
//  path on this Mac, so this bridge is NOT compiled or run here. It is written against
//  the verified ObjC API of the shipped XCFramework (same API the Flutter Swift plugin
//  drives), but MUST be built + device-verified on the Windows/Unity box. The core
//  lifecycle + tracking + sync-state are implemented; the remaining commands follow the
//  identical `[[engine tracking] ...]` / `[[engine consent] ...]` pattern.

#ifdef ATTRIAX_HAS_KMP_ENGINE

typedef void (*AttriaxUnitySyncCallback)(const char *stateName);

// A single sync-state listener that forwards KMP transitions to the C# callback.
@interface AttriaxUnitySyncListener : NSObject <AttriaxCoreAttriaxSynchronizationStateListener>
@property (nonatomic, assign) AttriaxUnitySyncCallback callback;
@end

@implementation AttriaxUnitySyncListener
- (void)onSynchronizationStateChangedState:(AttriaxCoreAttriaxSynchronizationState *)state {
    if (self.callback != NULL) {
        self.callback([state.name UTF8String]);
    }
}
@end

static NSString *AttriaxUnityCfgStr(NSDictionary *d, NSString *k) {
    id v = d[k];
    return [v isKindOfClass:[NSString class]] ? (NSString *)v : nil;
}
static BOOL AttriaxUnityCfgBool(NSDictionary *d, NSString *k, BOOL def) {
    id v = d[k];
    return [v isKindOfClass:[NSNumber class]] ? [v boolValue] : def;
}
static int64_t AttriaxUnityCfgI64(NSDictionary *d, NSString *k, int64_t def) {
    id v = d[k];
    return [v isKindOfClass:[NSNumber class]] ? [v longLongValue] : def;
}
static int32_t AttriaxUnityCfgI32(NSDictionary *d, NSString *k, int32_t def) {
    id v = d[k];
    return [v isKindOfClass:[NSNumber class]] ? [v intValue] : def;
}
static AttriaxCoreBoolean *AttriaxUnityCfgKBool(NSDictionary *d, NSString *k) {
    id v = d[k];
    return [v isKindOfClass:[NSNumber class]] ? [[AttriaxCoreBoolean alloc] initWithBool:[v boolValue]] : nil;
}

static AttriaxCoreAttriax *AttriaxUnityEngineFromHandle(void *handle) {
    return handle == NULL ? nil : (__bridge AttriaxCoreAttriax *)handle;
}

/// Build the engine from a JSON config (keys match the Flutter MethodChannel / KMP
/// C-ABI). Returns an opaque retained handle, or NULL on a bad config.
extern "C" void *AttriaxUnityEngine_Create(const char *configJson, const char *userAgent) {
    @autoreleasepool {
        NSString *json = configJson != NULL ? [NSString stringWithUTF8String:configJson] : @"{}";
        NSData *data = [json dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *d = data != nil
            ? [NSJSONSerialization JSONObjectWithData:data options:0 error:nil]
            : nil;
        if (![d isKindOfClass:[NSDictionary class]]) {
            d = @{};
        }

        BOOL attestationEnabled = AttriaxUnityCfgBool(d, @"attestationEnabled", NO);
        id<AttriaxCoreAttriaxAttestationProvider> attestationProvider = nil;
        if (attestationEnabled) {
            attestationProvider = [[AttriaxCoreAttriaxAppAttestProvider alloc]
                initWithDefaults:[[NSUserDefaults alloc] initWithSuiteName:@"com.attriax.sdk.prefs"] ?: NSUserDefaults.standardUserDefaults];
        }

        AttriaxCoreAttriaxConfig *config = [[AttriaxCoreAttriaxConfig alloc]
            initWithProjectToken:AttriaxUnityCfgStr(d, @"projectToken") ?: @""
            apiBaseUrl:AttriaxUnityCfgStr(d, @"apiBaseUrl") ?: @"https://api.attriax.com"
            appVersion:AttriaxUnityCfgStr(d, @"appVersion")
            appBuildNumber:AttriaxUnityCfgStr(d, @"appBuildNumber")
            appPackageName:AttriaxUnityCfgStr(d, @"appPackageName")
            sdkMetadata:[d[@"sdkMetadata"] isKindOfClass:[NSDictionary class]] ? d[@"sdkMetadata"] : nil
            deviceContext:nil
            enableDebugLogs:AttriaxUnityCfgBool(d, @"enableDebugLogs", NO)
            requestTimeoutMs:AttriaxUnityCfgI64(d, @"requestTimeoutMs", 12000)
            maxQueueSize:AttriaxUnityCfgI32(d, @"maxQueueSize", 500)
            eventFlushIntervalMs:AttriaxUnityCfgI64(d, @"eventFlushIntervalMs", 60000)
            flushEventsImmediatelyOnFirstLaunch:AttriaxUnityCfgBool(d, @"flushEventsImmediatelyOnFirstLaunch", YES)
            collectAdvertisingId:AttriaxUnityCfgBool(d, @"collectAdvertisingId", YES)
            automaticCrashReportingEnabled:AttriaxUnityCfgBool(d, @"automaticCrashReportingEnabled", YES)
            gdprEnabled:AttriaxUnityCfgBool(d, @"gdprEnabled", NO)
            anonymousTracking:AttriaxUnityCfgBool(d, @"anonymousTracking", YES)
            sessionTrackingEnabled:AttriaxUnityCfgBool(d, @"sessionTrackingEnabled", YES)
            sessionHeartbeatIntervalMs:AttriaxUnityCfgI64(d, @"sessionHeartbeatIntervalMs", 300000)
            firstLaunchSessionHeartbeatIntervalMs:AttriaxUnityCfgI64(d, @"firstLaunchSessionHeartbeatIntervalMs", 30000)
            installReferrerEnabled:AttriaxUnityCfgBool(d, @"installReferrerEnabled", YES)
            attestationEnabled:attestationEnabled
            attestationProvider:attestationProvider
            pinnedCertificateSha256Fingerprints:[d[@"pinnedCertificateSha256Fingerprints"] isKindOfClass:[NSArray class]] ? d[@"pinnedCertificateSha256Fingerprints"] : @[]
            automaticBrowserHandling:AttriaxUnityCfgBool(d, @"automaticBrowserHandling", YES)
            attStatus:nil
            requestTrackingAuthorizationOnInit:AttriaxUnityCfgBool(d, @"requestTrackingAuthorizationOnInit", NO)
            trackingAuthorizationStatusTimeoutMs:AttriaxUnityCfgI64(d, @"trackingAuthorizationStatusTimeoutMs", 60000)
            skan:nil
            asaTokenCaptureEnabled:AttriaxUnityCfgBool(d, @"asaTokenCaptureEnabled", YES)
            doNotSell:AttriaxUnityCfgKBool(d, @"doNotSell")
            usPrivacy:AttriaxUnityCfgStr(d, @"usPrivacy")];

        NSString *ua = userAgent != NULL ? [NSString stringWithUTF8String:userAgent] : nil;
        // userAgent nil → the KMP layer resolves the real WKWebView Safari UA off-thread.
        AttriaxCoreAttriax *engine = [[AttriaxCoreAttriaxApple shared] createConfig:config userAgent:ua];
        return (void *)CFBridgingRetain(engine);
    }
}

extern "C" void AttriaxUnityEngine_Init(void *handle) {
    @autoreleasepool { [AttriaxUnityEngineFromHandle(handle) doInit]; }
}
extern "C" void AttriaxUnityEngine_Flush(void *handle) {
    @autoreleasepool { [AttriaxUnityEngineFromHandle(handle) flush]; }
}
extern "C" void AttriaxUnityEngine_Reset(void *handle) {
    @autoreleasepool { [AttriaxUnityEngineFromHandle(handle) reset]; }
}
extern "C" void AttriaxUnityEngine_Dispose(void *handle) {
    @autoreleasepool { [AttriaxUnityEngineFromHandle(handle) dispose]; }
}

extern "C" void AttriaxUnityEngine_RecordEvent(void *handle, const char *name, const char *eventDataJson, bool flushImmediately) {
    @autoreleasepool {
        AttriaxCoreAttriax *engine = AttriaxUnityEngineFromHandle(handle);
        if (engine == nil) { return; }
        NSDictionary *eventData = nil;
        if (eventDataJson != NULL) {
            NSData *data = [[NSString stringWithUTF8String:eventDataJson] dataUsingEncoding:NSUTF8StringEncoding];
            id parsed = data != nil ? [NSJSONSerialization JSONObjectWithData:data options:0 error:nil] : nil;
            if ([parsed isKindOfClass:[NSDictionary class]]) { eventData = parsed; }
        }
        [engine recordEventName:(name != NULL ? [NSString stringWithUTF8String:name] : @"")
                      eventData:eventData
               flushImmediately:flushImmediately];
    }
}

extern "C" char *AttriaxUnityEngine_GetDeviceId(void *handle) {
    @autoreleasepool { return AttriaxUnityMakeCString(AttriaxUnityEngineFromHandle(handle).deviceId); }
}
extern "C" bool AttriaxUnityEngine_GetIsInitialized(void *handle) {
    @autoreleasepool { return AttriaxUnityEngineFromHandle(handle).isInitialized; }
}
extern "C" char *AttriaxUnityEngine_SubmitAsaToken(void *handle, const char *token) {
    @autoreleasepool {
        [AttriaxUnityEngineFromHandle(handle) submitAsaTokenToken:(token != NULL ? [NSString stringWithUTF8String:token] : @"")];
        return NULL;
    }
}

// Registers the C# sync-state callback. The listener is retained for the engine's life.
extern "C" void AttriaxUnityEngine_SetSyncCallback(void *handle, AttriaxUnitySyncCallback callback) {
    @autoreleasepool {
        AttriaxCoreAttriax *engine = AttriaxUnityEngineFromHandle(handle);
        if (engine == nil) { return; }
        static AttriaxUnitySyncListener *listener = nil;
        if (listener == nil) { listener = [[AttriaxUnitySyncListener alloc] init]; }
        listener.callback = callback;
        [engine.synchronization addStateListenerListener:listener];
    }
}

extern "C" void AttriaxUnityEngine_Destroy(void *handle) {
    if (handle != NULL) { CFBridgingRelease(handle); }
}

// TODO(windows-verify): the remaining IAttriaxEnginePlatform commands — the tracking
// record* family, setUser*, consent (gdpr/ccpa/att), skan.updateConversionValue, and the
// deep-link listeners — map 1:1 to `[[engine tracking] ...]`, `[[engine consent] gdpr]`,
// `[[engine skan] ...]`, `[[engine deepLinks] ...]` exactly as the Flutter Swift plugin
// does. They are omitted here only because this bridge cannot be compiled/verified on this
// machine (Unity Editor blocked); add them following the identical pattern on the Unity box.

#endif // ATTRIAX_HAS_KMP_ENGINE