#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using GeneratedUninstallTokenProvider = Attriax.Unity.Generated.Model.AppUserUninstallTokenProvider;
using AppUserGdprConsentState = Attriax.Unity.Generated.Model.AppUserGdprConsentState;
using GeneratedAppVersionContextDto = Attriax.Unity.Generated.Model.AppVersionContextDto;
using GeneratedDeviceContextDto = Attriax.Unity.Generated.Model.DeviceContextDto;
using GeneratedPlatform = Attriax.Unity.Generated.Model.Platform;
using GeneratedSdkVersionContextDto = Attriax.Unity.Generated.Model.SdkVersionContextDto;
using SdkCreateDynamicLinkDto = Attriax.Unity.Generated.Model.SdkCreateDynamicLinkDto;
using SdkEventDto = Attriax.Unity.Generated.Model.SdkEventDto;
using SdkNotificationDto = Attriax.Unity.Generated.Model.SdkNotificationDto;
using GeneratedNotificationEventType = Attriax.Unity.Generated.Model.NotificationEventType;
using GeneratedNotificationEventSource = Attriax.Unity.Generated.Model.NotificationEventSource;
using SdkV1GdprConsentCheckDto = Attriax.Unity.Generated.Model.SdkV1GdprConsentCheckDto;
using SdkV1GdprConsentValuesDto = Attriax.Unity.Generated.Model.SdkV1GdprConsentValuesDto;
using SdkV1GdprConsentWriteDto = Attriax.Unity.Generated.Model.SdkV1GdprConsentWriteDto;
using SdkRegisterUninstallTokenDto = Attriax.Unity.Generated.Model.SdkRegisterUninstallTokenDto;
using SdkSessionDto = Attriax.Unity.Generated.Model.SdkSessionDto;
using SdkSessionLifecycleKind = Attriax.Unity.Generated.Model.SdkSessionLifecycleKind;
using SdkUserDto = Attriax.Unity.Generated.Model.SdkUserDto;
using SdkV1DeepLinkResolveDto = Attriax.Unity.Generated.Model.SdkV1DeepLinkResolveDto;
using SdkV1OpenDto = Attriax.Unity.Generated.Model.SdkV1OpenDto;
using SdkV1RevenueReceiptValidateDto = Attriax.Unity.Generated.Model.SdkV1RevenueReceiptValidateDto;
using SdkV1UnityEditorValidateDto = Attriax.Unity.Generated.Model.SdkV1UnityEditorValidateDto;

namespace Attriax.Unity.Internal
{
    internal static class AttriaxGeneratedRequestFactory
    {
        public static SdkV1OpenDto BuildOpenRequest(
            string projectToken,
            string deviceIdSource,
            AttriaxContextSnapshot snapshot,
            AttriaxSessionSnapshot? session,
            string? installReferrerOverride = null,
            IDictionary<string, object>? deviceMetadataOverrides = null)
        {
            return new SdkV1OpenDto(
                app: new GeneratedAppVersionContextDto(
                    buildNumber: snapshot.App.BuildNumber,
                    packageName: snapshot.App.PackageName,
                    version: snapshot.App.Version),
                projectToken: projectToken,
                device: new GeneratedDeviceContextDto(
                    advertisingId: snapshot.Device.AdvertisingId,
                    androidId: snapshot.Device.AndroidId,
                    brand: snapshot.Device.Brand,
                    colorDepth: ToGeneratedNumber(snapshot.Device.ColorDepth),
                    devicePixelRatio: ToGeneratedNumber(snapshot.Device.DevicePixelRatio),
                    hardware: snapshot.Device.Hardware,
                    isPhysicalDevice: snapshot.Device.IsPhysicalDevice ?? false,
                    language: snapshot.Device.Language,
                    manufacturer: snapshot.Device.Manufacturer,
                    metadata: AttriaxObjectNormalizer.NormalizeObjectMap(
                        MergeDeviceMetadata(snapshot.Device.Metadata, deviceMetadataOverrides)),
                    model: snapshot.Device.Model,
                    name: snapshot.Device.Name,
                    osVersion: snapshot.Device.OsVersion,
                    screenHeight: ToGeneratedNumber(snapshot.Device.ScreenHeight),
                    screenResolution: snapshot.Device.ScreenResolution,
                    screenWidth: ToGeneratedNumber(snapshot.Device.ScreenWidth),
                    supportedAbis: snapshot.Device.SupportedAbis != null
                        ? new List<string>(snapshot.Device.SupportedAbis)
                        : new List<string>(),
                    timezone: snapshot.Device.Timezone),
                deviceId: snapshot.DeviceId,
                deviceIdSource: deviceIdSource,
                googlePlayInstantParam: snapshot.GooglePlayInstantParam ?? default,
                installBeginTimestampSeconds: ToGeneratedNumber(snapshot.InstallBeginTimestampSeconds),
                installReferrer: AttriaxRawInstallReferrerNormalizer.Normalize(installReferrerOverride)
                    ?? AttriaxRawInstallReferrerNormalizer.Normalize(snapshot.RawPlatformInstallReferrer),
                isFirstLaunch: session?.IsFirstLaunch ?? snapshot.IsFirstLaunch,
                platform: MapGeneratedPlatform(snapshot.Platform),
                referrerClickTimestampSeconds: ToGeneratedNumber(snapshot.ReferrerClickTimestampSeconds),
                sessionId: session?.Id,
                sessionStartedAt: session != null ? session.StartedAt.UtcDateTime : default,
                sdk: new GeneratedSdkVersionContextDto(
                    apiVersion: snapshot.Sdk.ApiVersion,
                    metadata: AttriaxObjectNormalizer.NormalizeObjectMap(snapshot.Sdk.Metadata),
                    packageVersion: snapshot.Sdk.PackageVersion));
        }

        private static decimal ToGeneratedNumber(int? value)
        {
            return value.HasValue ? value.Value : default;
        }

        private static decimal ToGeneratedNumber(double? value)
        {
            return value.HasValue ? (decimal)value.Value : default;
        }

        private static decimal ToGeneratedNumber(long? value)
        {
            return value.HasValue ? value.Value : default;
        }

        private static Dictionary<string, object> MergeDeviceMetadata(
            IDictionary<string, object>? metadata,
            IDictionary<string, object>? overrides)
        {
            var merged = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>();

            if (overrides == null)
            {
                return merged;
            }

            foreach (var entry in overrides)
            {
                merged[entry.Key] = entry.Value;
            }

            return merged;
        }

        public static SdkEventDto BuildTrackEventRequest(
            string projectToken,
            string? deviceId,
            string? deviceIdSource,
            string eventName,
            AttriaxTrackEventOptions options,
            AttriaxSessionSnapshot? session,
            DateTimeOffset occurredAt)
        {
            return new SdkEventDto(
                projectToken: projectToken,
                clientOccurredAt: occurredAt.UtcDateTime,
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                eventData: AttriaxObjectNormalizer.NormalizeObjectMap(options.EventData),
                eventName: eventName,
                sessionId: session?.Id,
                sessionRelativeTimeMs: session != null ? GetSessionRelativeTimeMs(session, occurredAt) : 0m);
        }

        public static SdkNotificationDto BuildTrackNotificationRequest(
            string projectToken,
            string? deviceId,
            string? deviceIdSource,
            AttriaxNotificationEventType type,
            string notificationId,
            AttriaxPlatformType platform,
            string? linkId,
            string? campaignId,
            string? title,
            AttriaxNotificationEventSource? source,
            string? sessionId,
            IDictionary<string, object>? metadata,
            DateTimeOffset occurredAt)
        {
            var normalizedNotificationId = notificationId?.Trim();
            if (string.IsNullOrEmpty(normalizedNotificationId))
            {
                throw new ArgumentException("notificationId must not be empty.", nameof(notificationId));
            }

            return new SdkNotificationDto(
                projectToken: projectToken,
                campaignId: NormalizeOptionalString(campaignId),
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                linkId: NormalizeOptionalString(linkId),
                metadata: AttriaxObjectNormalizer.NormalizeObjectMap(metadata),
                notificationId: normalizedNotificationId,
                occurredAt: occurredAt.UtcDateTime,
                platform: MapGeneratedPlatform(platform),
                sessionId: NormalizeOptionalString(sessionId),
                source: MapGeneratedNotificationEventSource(source),
                title: NormalizeOptionalString(title),
                type: MapGeneratedNotificationEventType(type));
        }

        private static GeneratedNotificationEventType MapGeneratedNotificationEventType(
            AttriaxNotificationEventType type)
        {
            switch (type)
            {
                case AttriaxNotificationEventType.Received:
                    return GeneratedNotificationEventType.Received;
                case AttriaxNotificationEventType.Opened:
                    return GeneratedNotificationEventType.Opened;
                case AttriaxNotificationEventType.Dismissed:
                    return GeneratedNotificationEventType.Dismissed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported notification event type.");
            }
        }

        private static GeneratedNotificationEventSource? MapGeneratedNotificationEventSource(
            AttriaxNotificationEventSource? source)
        {
            switch (source)
            {
                case null:
                    return null;
                case AttriaxNotificationEventSource.Fcm:
                    return GeneratedNotificationEventSource.Fcm;
                case AttriaxNotificationEventSource.Apns:
                    return GeneratedNotificationEventSource.Apns;
                case AttriaxNotificationEventSource.Other:
                    return GeneratedNotificationEventSource.Other;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unsupported notification event source.");
            }
        }

        public static AttriaxCrashRequest BuildTrackCrashRequest(
            string projectToken,
            string? deviceId,
            string? deviceIdSource,
            AttriaxContextSnapshot snapshot,
            AttriaxSessionSnapshot? session,
            string source,
            bool isFatal,
            string exceptionType,
            string message,
            string stackTrace,
            string? reason,
            IDictionary<string, object>? metadata,
            DateTimeOffset occurredAt)
        {
            return new AttriaxCrashRequest
            {
                ProjectToken = projectToken,
                DeviceId = deviceId,
                DeviceIdSource = deviceIdSource,
                Platform = MapCrashPlatform(snapshot.Platform),
                Source = source,
                ClientOccurredAt = occurredAt,
                IsFatal = isFatal,
                ExceptionType = exceptionType,
                Message = message,
                StackTrace = stackTrace,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason,
                SessionId = session?.Id,
                SessionRelativeTimeMs = session != null ? GetSessionRelativeTimeMs(session, occurredAt) : null,
                Locale = snapshot.Device.Language,
                AppVersion = snapshot.App.Version,
                AppBuildNumber = snapshot.App.BuildNumber,
                AppPackageName = snapshot.App.PackageName,
                SdkApiVersion = snapshot.Sdk.ApiVersion,
                SdkPackageVersion = session?.SdkPackageVersion ?? snapshot.Sdk.PackageVersion,
                IsFirstLaunch = session?.IsFirstLaunch ?? snapshot.IsFirstLaunch,
                Metadata = AttriaxObjectNormalizer.NormalizeObjectMap(metadata),
            };
        }

        public static SdkSessionDto BuildTrackSessionRequest(
            string projectToken,
            string? deviceId,
            string? deviceIdSource,
            AttriaxSessionSnapshot session,
            SdkSessionLifecycleKind kind,
            DateTimeOffset occurredAt,
            IDictionary<string, object>? metadata)
        {
            return new SdkSessionDto(
                appBuildNumber: session.AppBuildNumber,
                appPackageName: session.AppPackageName,
                projectToken: projectToken,
                appVersion: session.AppVersion,
                clientOccurredAt: occurredAt.UtcDateTime,
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                isFirstLaunch: session.IsFirstLaunch,
                kind: kind,
                locale: session.Locale,
                metadata: AttriaxObjectNormalizer.NormalizeObjectMap(metadata),
                platform: MapGeneratedPlatform(session.Platform),
                sdkApiVersion: "v1",
                sdkPackageVersion: session.SdkPackageVersion,
                sessionId: session.Id,
                sessionRelativeTimeMs: GetSessionRelativeTimeMs(session, occurredAt));
        }

        public static SdkUserDto BuildUserRequest(
            string projectToken,
            string deviceId,
            string deviceIdSource,
            string? userId,
            AttriaxSetUserOptions options,
            bool clearExternalUser)
        {
            var sanitizedOptions = AttriaxUserPropertySanitizer.SanitizeSetUserOptions(options);

            return new SdkUserDto(
                projectToken: projectToken,
                clearAllProperties: sanitizedOptions.ClearAllProperties,
                clearExternalUser: clearExternalUser,
                clearPropertyKeys: NormalizeClearPropertyKeys(sanitizedOptions.ClearPropertyKeys),
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                externalUserId: string.IsNullOrWhiteSpace(userId)
                    ? null
                    : userId,
                externalUserName: clearExternalUser || string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(sanitizedOptions.UserName)
                    ? null
                    : sanitizedOptions.UserName,
                properties: AttriaxUserPropertySanitizer.SanitizeProperties(sanitizedOptions.Properties));
        }

        public static SdkCreateDynamicLinkDto BuildCreateDynamicLinkRequest(
            string projectToken,
            AttriaxCreateDynamicLinkOptions options)
        {
            return new SdkCreateDynamicLinkDto(
                androidRedirect: options.AndroidRedirect ?? default,
                projectToken: projectToken,
                data: AttriaxObjectNormalizer.NormalizeObjectMap(options.Data),
                destinationUrl: string.IsNullOrWhiteSpace(options.DestinationUrl)
                    ? null
                    : ValidateAbsoluteHttpUrl(options.DestinationUrl),
                group: string.IsNullOrWhiteSpace(options.Group) ? null : options.Group,
                iosRedirect: options.IOSRedirect ?? default,
                name: string.IsNullOrWhiteSpace(options.Name) ? null : options.Name,
                prefix: string.IsNullOrWhiteSpace(options.Prefix) ? null : options.Prefix,
                previewDescription: string.IsNullOrWhiteSpace(options.PreviewDescription)
                    ? null
                    : options.PreviewDescription,
                previewTitle: string.IsNullOrWhiteSpace(options.PreviewTitle)
                    ? null
                    : options.PreviewTitle,
                utmCampaign: string.IsNullOrWhiteSpace(options.UtmCampaign)
                    ? null
                    : options.UtmCampaign,
                utmContent: string.IsNullOrWhiteSpace(options.UtmContent)
                    ? null
                    : options.UtmContent,
                utmMedium: string.IsNullOrWhiteSpace(options.UtmMedium)
                    ? null
                    : options.UtmMedium,
                utmSource: string.IsNullOrWhiteSpace(options.UtmSource)
                    ? null
                    : options.UtmSource,
                utmTerm: string.IsNullOrWhiteSpace(options.UtmTerm)
                    ? null
                    : options.UtmTerm);
        }

        public static SdkV1RevenueReceiptValidateDto BuildValidateReceiptRequest(
            string projectToken,
            string? deviceId,
            AttriaxValidateReceiptOptions options,
            DateTimeOffset occurredAt)
        {
            return new SdkV1RevenueReceiptValidateDto(
                projectToken: projectToken,
                clientOccurredAt: occurredAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                deviceId: deviceId,
                environment: NormalizeOptionalString(options.Environment),
                productId: NormalizeOptionalString(options.ProductId),
                provider: NormalizeOptionalString(options.Provider),
                receipt: NormalizeOptionalString(options.Receipt),
                test: options.Test ?? default,
                transactionId: NormalizeOptionalString(options.TransactionId));
        }

        public static SdkV1UnityEditorValidateDto BuildUnityEditorValidateRequest(
            string projectToken,
            string packageVersion,
            string unityVersion,
            string editorHostPlatform)
        {
            return new SdkV1UnityEditorValidateDto(
                projectToken: projectToken,
                editorHostPlatform: NormalizeOptionalString(editorHostPlatform),
                packageVersion: NormalizeOptionalString(packageVersion),
                unityVersion: NormalizeOptionalString(unityVersion));
        }

        public static SdkRegisterUninstallTokenDto BuildRegisterUninstallTokenRequest(
            string projectToken,
            string deviceId,
            string deviceIdSource,
            AttriaxPlatformType platform,
            GeneratedUninstallTokenProvider provider,
            string? token,
            IDictionary<string, object>? metadata)
        {
            return new SdkRegisterUninstallTokenDto(
                projectToken: projectToken,
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                metadata: AttriaxObjectNormalizer.NormalizeObjectMap(metadata),
                platform: MapGeneratedPlatform(platform),
                provider: provider,
                token: NormalizeOptionalString(token));
        }

        public static SdkV1GdprConsentCheckDto BuildGdprConsentCheckRequest(
            string projectToken,
            string consentId)
        {
            return new SdkV1GdprConsentCheckDto(
                projectToken: projectToken,
                consentId: consentId);
        }

        public static SdkV1GdprConsentWriteDto BuildGdprConsentWriteRequest(
            string projectToken,
            string consentId,
            DateTime clientOccurredAt,
            string? countryCode,
            string? regionSource,
            AppUserGdprConsentState state,
            AttriaxGdprConsentValues? values)
        {
            return new SdkV1GdprConsentWriteDto(
                projectToken: projectToken,
                clientOccurredAt: clientOccurredAt.ToUniversalTime(),
                consentId: consentId,
                countryCode: NormalizeOptionalString(countryCode),
                regionSource: NormalizeOptionalString(regionSource),
                state: state,
                values: values == null
                    ? null
                    : new SdkV1GdprConsentValuesDto(
                        adEvents: values.AdEvents,
                        analytics: values.Analytics,
                        attribution: values.Attribution));
        }

        public static SdkV1DeepLinkResolveDto BuildResolveDeepLinkRequest(
            string projectToken,
            string? deviceId,
            string? deviceIdSource,
            bool isFirstLaunch,
            AttriaxPlatformType platform,
            AttriaxDeepLinkConversionOptions options)
        {
            return new SdkV1DeepLinkResolveDto(
                projectToken: projectToken,
                deviceId: deviceId,
                deviceIdSource: deviceIdSource,
                isFirstLaunch: isFirstLaunch,
                linkPath: ExtractLinkPath(options.Uri),
                metadata: AttriaxObjectNormalizer.NormalizeObjectMap(options.Metadata),
                platform: MapGeneratedPlatform(platform),
                rawUrl: string.IsNullOrWhiteSpace(options.Uri) ? null : NormalizeUrl(options.Uri),
                source: string.IsNullOrWhiteSpace(options.Source)
                    ? (options.IsInitialLink ? "initial_url" : "manual")
                    : options.Source);
        }

        internal static GeneratedPlatform MapGeneratedPlatform(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return GeneratedPlatform.Android;
                case AttriaxPlatformType.IOS:
                    return GeneratedPlatform.Ios;
                case AttriaxPlatformType.UnityEditor:
                    return GeneratedPlatform.UnityEditor;
                case AttriaxPlatformType.MacOS:
                    return GeneratedPlatform.Macos;
                case AttriaxPlatformType.Windows:
                    return GeneratedPlatform.Windows;
                case AttriaxPlatformType.Linux:
                    return GeneratedPlatform.Linux;
                case AttriaxPlatformType.Web:
                    return GeneratedPlatform.Web;
                default:
                    return GeneratedPlatform.Unknown;
            }
        }

        private static string MapCrashPlatform(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return "android";
                case AttriaxPlatformType.IOS:
                    return "ios";
                case AttriaxPlatformType.Web:
                    return "web";
                case AttriaxPlatformType.UnityEditor:
                    return "unity_editor";
                case AttriaxPlatformType.Windows:
                    return "windows";
                case AttriaxPlatformType.MacOS:
                    return "macos";
                case AttriaxPlatformType.Linux:
                    return "linux";
                default:
                    return "unknown";
            }
        }

        private static string MapUninstallTrackingPlatform(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return "android";
                case AttriaxPlatformType.IOS:
                    return "ios";
                default:
                    throw new NotSupportedException(
                        "Firebase uninstall tracking is only supported on Android and iOS.");
            }
        }

        private static string ValidateAbsoluteHttpUrl(string value)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Dynamic-link destinationUrl must be an absolute HTTP or HTTPS URL.", nameof(value));
            }

            return uri.ToString();
        }

        private static string NormalizeUrl(string value)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            return value;
        }

        private static string? ExtractLinkPath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
            {
                trimmed = absolute.AbsolutePath;
            }

            var path = trimmed.Trim('/');
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        private static decimal GetSessionRelativeTimeMs(
            AttriaxSessionSnapshot session,
            DateTimeOffset occurredAt)
        {
            var elapsedTicks = (occurredAt - session.StartedAt).Ticks;
            var elapsedMilliseconds = elapsedTicks <= 0
                ? 0L
                : elapsedTicks / TimeSpan.TicksPerMillisecond;
            return Convert.ToDecimal(elapsedMilliseconds, CultureInfo.InvariantCulture);
        }

        private static List<string>? NormalizeClearPropertyKeys(ICollection<string>? clearPropertyKeys)
        {
            if (clearPropertyKeys == null || clearPropertyKeys.Count == 0)
            {
                return null;
            }

            var normalized = new List<string>();
            foreach (var clearPropertyKey in clearPropertyKeys)
            {
                if (string.IsNullOrWhiteSpace(clearPropertyKey))
                {
                    continue;
                }

                normalized.Add(clearPropertyKey);
            }

            return normalized.Count == 0 ? null : normalized;
        }

        private static void AddTrimmed(
            IDictionary<string, object> target,
            string key,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            target[key] = value.Trim();
        }

        private static string? NormalizeOptionalString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
