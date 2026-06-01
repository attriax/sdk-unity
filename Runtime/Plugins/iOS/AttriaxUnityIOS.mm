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

static id AttriaxUnityReadEntitlementValue(NSString *key) {
    SecTaskRef task = SecTaskCreateFromSelf(nil);
    if (task == nil) {
        return nil;
    }

    CFErrorRef error = nil;
    CFTypeRef value = SecTaskCopyValueForEntitlement(task, (__bridge CFStringRef)key, &error);
    if (error != nil) {
        CFRelease(error);
    }
    CFRelease(task);

    return CFBridgingRelease(value);
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