#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    internal sealed class AttriaxContextSnapshotBuilder
    {
        private readonly AttriaxConfig _config;
        private readonly string _sdkApiVersion;
        private readonly string _sdkPackageVersion;
        private readonly Func<string?> _resolveCurrentTimezone;

        public AttriaxContextSnapshotBuilder(
            AttriaxConfig config,
            string sdkApiVersion,
            string sdkPackageVersion,
            Func<string?> resolveCurrentTimezone)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sdkApiVersion = sdkApiVersion ?? throw new ArgumentNullException(nameof(sdkApiVersion));
            _sdkPackageVersion = sdkPackageVersion ?? throw new ArgumentNullException(nameof(sdkPackageVersion));
            _resolveCurrentTimezone = resolveCurrentTimezone ?? throw new ArgumentNullException(nameof(resolveCurrentTimezone));
        }

        public AttriaxContextSnapshot BuildAnonymousContextSnapshot(
            AttriaxPlatformType platform,
            bool isFirstLaunch)
        {
            return new AttriaxContextSnapshot
            {
                Platform = platform,
                DeviceId = string.Empty,
                IsFirstLaunch = isFirstLaunch,
                Sdk = new AttriaxSdkSnapshot
                {
                    ApiVersion = _sdkApiVersion,
                    PackageVersion = _sdkPackageVersion,
                    Metadata = CollectSdkMetadata(platform),
                },
                App = new AttriaxAppSnapshot
                {
                    Version = string.IsNullOrWhiteSpace(_config.AppVersion) ? Application.version : _config.AppVersion,
                    BuildNumber = FirstNonEmpty(_config.AppBuildNumber, Application.buildGUID),
                    PackageName = string.IsNullOrWhiteSpace(_config.AppPackageName) ? Application.identifier : _config.AppPackageName,
                },
                Device = new AttriaxDeviceSnapshot
                {
                    Timezone = _resolveCurrentTimezone(),
                    SupportedAbis = new List<string>(),
                    Metadata = new Dictionary<string, object>(),
                },
            };
        }

        public AttriaxContextSnapshot BuildContextSnapshot(
            AttriaxPlatformType platform,
            string deviceId,
            bool isFirstLaunch,
            AttriaxNativeContextPayload nativeContext,
            AttriaxInstallReferrerContextPayload installReferrerContext,
            string? rawPlatformInstallReferrer)
        {
            return new AttriaxContextSnapshot
            {
                Platform = platform,
                DeviceId = deviceId,
                IsFirstLaunch = isFirstLaunch,
                RawPlatformInstallReferrer = rawPlatformInstallReferrer,
                InstallBeginTimestampSeconds = installReferrerContext.InstallBeginTimestampSeconds,
                ReferrerClickTimestampSeconds = installReferrerContext.ReferrerClickTimestampSeconds,
                GooglePlayInstantParam = installReferrerContext.GooglePlayInstantParam,
                Sdk = new AttriaxSdkSnapshot
                {
                    ApiVersion = _sdkApiVersion,
                    PackageVersion = _sdkPackageVersion,
                    Metadata = CollectSdkMetadata(platform),
                },
                App = new AttriaxAppSnapshot
                {
                    Version = string.IsNullOrWhiteSpace(_config.AppVersion) ? Application.version : _config.AppVersion,
                    BuildNumber = FirstNonEmpty(_config.AppBuildNumber, Application.buildGUID),
                    PackageName = string.IsNullOrWhiteSpace(_config.AppPackageName) ? Application.identifier : _config.AppPackageName,
                },
                Device = CollectDeviceSnapshot(platform, nativeContext, installReferrerContext),
            };
        }

        private AttriaxDeviceSnapshot CollectDeviceSnapshot(
            AttriaxPlatformType platform,
            AttriaxNativeContextPayload nativeContext,
            AttriaxInstallReferrerContextPayload installReferrerContext)
        {
            var metadata = new Dictionary<string, object>
            {
                ["unityRuntime"] = CollectUnityRuntimeMetadata(platform),
                ["nativeContext"] = nativeContext.Metadata,
                ["installReferrerContext"] = installReferrerContext.Metadata,
            };

            if (platform == AttriaxPlatformType.UnityEditor)
            {
                metadata["editorContext"] = CollectEditorMetadata();
            }

            if (platform == AttriaxPlatformType.Windows ||
                platform == AttriaxPlatformType.MacOS ||
                platform == AttriaxPlatformType.Linux)
            {
                metadata["desktopContext"] = CollectDesktopMetadata(platform);
            }

            var language = ReadString(nativeContext.Metadata, "locale")
                ?? ReadString(nativeContext.Metadata, "language")
                ?? CultureInfo.CurrentCulture.Name
                ?? Application.systemLanguage.ToString();
            var timezone = ReadString(nativeContext.Metadata, "timezone")
                ?? TimeZoneInfo.Local.Id;
            var screenResolution = GetScreenResolution();
            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();
            var devicePixelRatio = GetDevicePixelRatio(nativeContext.Metadata);
            var colorDepth = ReadInt(nativeContext.Metadata, "colorDepth");

            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = ReadString(nativeContext.Metadata, "model") ?? SystemInfo.deviceModel,
                        Name = ReadString(nativeContext.Metadata, "device")
                            ?? ReadString(nativeContext.Metadata, "product")
                            ?? SystemInfo.deviceName,
                        Brand = ReadString(nativeContext.Metadata, "brand"),
                        Manufacturer = ReadString(nativeContext.Metadata, "manufacturer"),
                        Hardware = ReadString(nativeContext.Metadata, "hardware") ?? SystemInfo.processorType,
                        OsVersion = ReadString(nativeContext.Metadata, "releaseVersion") ?? SystemInfo.operatingSystem,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        AdvertisingId = _config.CollectAdvertisingId ? nativeContext.AdvertisingId : null,
                        AndroidId = nativeContext.AndroidId,
                        IsPhysicalDevice = true,
                        SupportedAbis = ReadStringList(nativeContext.Metadata, "supportedAbis"),
                        Metadata = metadata,
                    };
                case AttriaxPlatformType.IOS:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = ReadString(nativeContext.Metadata, "deviceModel") ?? SystemInfo.deviceModel,
                        Name = ReadString(nativeContext.Metadata, "deviceName") ?? SystemInfo.deviceName,
                        Brand = "Apple",
                        Manufacturer = "Apple",
                        Hardware = ReadString(nativeContext.Metadata, "hardwareModel") ?? SystemInfo.deviceModel,
                        OsVersion = ReadString(nativeContext.Metadata, "systemVersion") ?? SystemInfo.operatingSystem,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        AdvertisingId = _config.CollectAdvertisingId ? nativeContext.AdvertisingId : null,
                        IsPhysicalDevice = !ReadBoolean(nativeContext.Metadata, "isSimulator"),
                        SupportedAbis = new List<string>(),
                        Metadata = metadata,
                    };
                case AttriaxPlatformType.UnityEditor:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = SystemInfo.deviceModel,
                        Name = Environment.MachineName,
                        Brand = "Unity",
                        Manufacturer = "Unity",
                        Hardware = SystemInfo.processorType,
                        OsVersion = Environment.OSVersion.VersionString,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        IsPhysicalDevice = false,
                        SupportedAbis = new List<string>(),
                        Metadata = metadata,
                    };
                case AttriaxPlatformType.Windows:
                case AttriaxPlatformType.MacOS:
                case AttriaxPlatformType.Linux:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = SystemInfo.deviceModel,
                        Name = Environment.MachineName,
                        Brand = GetDesktopBrand(platform),
                        Manufacturer = GetDesktopBrand(platform),
                        Hardware = SystemInfo.processorType,
                        OsVersion = Environment.OSVersion.VersionString,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        IsPhysicalDevice = true,
                        SupportedAbis = new List<string>(),
                        Metadata = metadata,
                    };
                case AttriaxPlatformType.Web:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = SystemInfo.deviceModel,
                        Name = SystemInfo.deviceName,
                        Hardware = SystemInfo.processorType,
                        OsVersion = SystemInfo.operatingSystem,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        IsPhysicalDevice = !Application.isEditor,
                        SupportedAbis = new List<string>(),
                        Metadata = metadata,
                    };
                default:
                    return new AttriaxDeviceSnapshot
                    {
                        Model = SystemInfo.deviceModel,
                        Name = SystemInfo.deviceName,
                        Hardware = SystemInfo.processorType,
                        OsVersion = SystemInfo.operatingSystem,
                        Language = language,
                        Timezone = timezone,
                        ScreenResolution = screenResolution,
                        ScreenWidth = screenWidth,
                        ScreenHeight = screenHeight,
                        DevicePixelRatio = devicePixelRatio,
                        ColorDepth = colorDepth,
                        IsPhysicalDevice = !Application.isEditor,
                        SupportedAbis = new List<string>(),
                        Metadata = metadata,
                    };
            }
        }

        private Dictionary<string, object> CollectSdkMetadata(AttriaxPlatformType platform)
        {
            var metadata = new Dictionary<string, object>(_config.SdkMetadata)
            {
                ["clientRuntime"] = "unity",
                ["executionEnvironment"] = Application.isEditor ? "unity_editor" : "unity_player",
                ["unityVersion"] = Application.unityVersion,
                ["runtimePlatform"] = Application.platform.ToString(),
            };

            if (platform == AttriaxPlatformType.UnityEditor)
            {
                metadata["editorHostPlatform"] = DetectEditorHostPlatform();
            }

            return metadata;
        }

        private Dictionary<string, object> CollectUnityRuntimeMetadata(AttriaxPlatformType platform)
        {
            var metadata = new Dictionary<string, object>
            {
                ["unityVersion"] = Application.unityVersion,
                ["runtimePlatform"] = Application.platform.ToString(),
                ["resolvedPlatform"] = PlatformName(platform),
                ["productName"] = Application.productName,
                ["companyName"] = Application.companyName,
                ["deviceType"] = SystemInfo.deviceType.ToString(),
                ["processorType"] = SystemInfo.processorType,
                ["processorCount"] = SystemInfo.processorCount,
                ["systemMemorySize"] = SystemInfo.systemMemorySize,
                ["graphicsDeviceName"] = SystemInfo.graphicsDeviceName,
                ["graphicsDeviceVendor"] = SystemInfo.graphicsDeviceVendor,
                ["graphicsMemorySize"] = SystemInfo.graphicsMemorySize,
                ["graphicsDeviceType"] = SystemInfo.graphicsDeviceType.ToString(),
                ["operatingSystem"] = SystemInfo.operatingSystem,
                ["operatingSystemFamily"] = SystemInfo.operatingSystemFamily.ToString(),
                ["internetReachability"] = Application.internetReachability.ToString(),
            };

            if (!string.IsNullOrWhiteSpace(AttriaxLifecycleDispatcher.InitialAbsoluteUrl))
            {
                metadata["absoluteUrl"] = AttriaxLifecycleDispatcher.InitialAbsoluteUrl;
            }

            return metadata;
        }

        private static Dictionary<string, object> CollectDesktopMetadata(AttriaxPlatformType platform)
        {
            return new Dictionary<string, object>
            {
                ["platform"] = PlatformName(platform),
                ["machineName"] = Environment.MachineName,
                ["osVersion"] = Environment.OSVersion.VersionString,
                ["is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem,
                ["is64BitProcess"] = Environment.Is64BitProcess,
                ["processorCount"] = Environment.ProcessorCount,
            };
        }

        private static Dictionary<string, object> CollectEditorMetadata()
        {
            return new Dictionary<string, object>
            {
                ["hostPlatform"] = DetectEditorHostPlatform(),
                ["productName"] = Application.productName,
                ["companyName"] = Application.companyName,
                ["isBatchMode"] = Application.isBatchMode,
            };
        }

        private static string? GetScreenResolution()
        {
            try
            {
                if (Screen.width > 0 && Screen.height > 0)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}x{1}", Screen.width, Screen.height);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static int? GetScreenWidth()
        {
            try
            {
                if (Screen.width > 0)
                {
                    return Screen.width;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static int? GetScreenHeight()
        {
            try
            {
                if (Screen.height > 0)
                {
                    return Screen.height;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static double? GetDevicePixelRatio(IDictionary<string, object> source)
        {
            var nativeScale = ReadDouble(source, "devicePixelRatio")
                ?? ReadDouble(source, "screenScale");
            if (nativeScale.HasValue && nativeScale.Value > 0)
            {
                return nativeScale.Value;
            }

            try
            {
                if (Screen.dpi > 0f)
                {
                    var dpiRatio = Screen.dpi / 96f;
                    if (dpiRatio > 0f && !float.IsNaN(dpiRatio) && !float.IsInfinity(dpiRatio))
                    {
                        return Math.Round(dpiRatio, 4, MidpointRounding.AwayFromZero);
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static string? ReadString(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<string>();
            }

            return rawValue.ToString();
        }

        private static bool ReadBoolean(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return false;
            }

            if (rawValue is bool boolValue)
            {
                return boolValue;
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<bool>();
            }

            bool.TryParse(rawValue.ToString(), out var parsed);
            return parsed;
        }

        private static int? ReadInt(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is int intValue)
            {
                return intValue;
            }

            if (rawValue is long longValue)
            {
                return Convert.ToInt32(longValue, CultureInfo.InvariantCulture);
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<int?>();
            }

            return int.TryParse(rawValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (int?)null;
        }

        private static double? ReadDouble(IDictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is double doubleValue)
            {
                return doubleValue;
            }

            if (rawValue is float floatValue)
            {
                return floatValue;
            }

            if (rawValue is decimal decimalValue)
            {
                return Convert.ToDouble(decimalValue, CultureInfo.InvariantCulture);
            }

            if (rawValue is JValue jValue)
            {
                return jValue.ToObject<double?>();
            }

            return double.TryParse(rawValue.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (double?)null;
        }

        private static IList<string> ReadStringList(IDictionary<string, object> source, string key)
        {
            var values = new List<string>();
            if (source == null || !source.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return values;
            }

            if (rawValue is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    var value = item.ToObject<string>();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        values.Add(value);
                    }
                }

                return values;
            }

            if (rawValue is IEnumerable enumerable && !(rawValue is string))
            {
                foreach (var item in enumerable)
                {
                    var value = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        values.Add(value);
                    }
                }
            }

            return values;
        }

        private static string GetDesktopBrand(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Windows:
                    return "Microsoft";
                case AttriaxPlatformType.MacOS:
                    return "Apple";
                case AttriaxPlatformType.Linux:
                    return "Linux";
                default:
                    return "Desktop";
            }
        }

        private static string DetectEditorHostPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return "windows";
                case RuntimePlatform.OSXEditor:
                    return "macos";
                case RuntimePlatform.LinuxEditor:
                    return "linux";
                default:
                    return "unknown";
            }
        }

        private static string? FirstNonEmpty(string? first, string? second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }

            return string.IsNullOrWhiteSpace(second) ? null : second;
        }

        private static string PlatformName(AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return "android";
                case AttriaxPlatformType.IOS:
                    return "ios";
                case AttriaxPlatformType.Web:
                    return "web";
                case AttriaxPlatformType.Windows:
                    return "windows";
                case AttriaxPlatformType.MacOS:
                    return "macos";
                case AttriaxPlatformType.Linux:
                    return "linux";
                case AttriaxPlatformType.UnityEditor:
                    return "unity_editor";
                default:
                    return "unknown";
            }
        }
    }
}
