#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Attriax.Unity.Internal
{
    internal static class AttriaxNativeBridge
    {
        internal static Task<AttriaxNativeContextPayload> CollectNativeContextAsync(
            AttriaxPlatformType platform,
            bool collectAdvertisingId)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return Task.FromResult(ParseNativeContext(CallAndroidNativeContext(collectAdvertisingId)));
                case AttriaxPlatformType.IOS:
                    return Task.FromResult(ParseNativeContext(CallIosNativeContext(collectAdvertisingId)));
                default:
                    return Task.FromResult(new AttriaxNativeContextPayload());
            }
        }

        internal static Task<AttriaxTrackingAuthorizationStatus> GetTrackingAuthorizationStatusAsync(
            AttriaxPlatformType platform)
        {
            if (platform != AttriaxPlatformType.IOS)
            {
                return Task.FromResult(AttriaxTrackingAuthorizationStatus.NotSupported);
            }

            return Task.FromResult(ParseTrackingAuthorizationStatus(CallIosTrackingAuthorizationStatus()));
        }

        internal static Task<AttriaxTrackingAuthorizationStatus> RequestTrackingAuthorizationAsync(
            AttriaxPlatformType platform)
        {
            if (platform != AttriaxPlatformType.IOS)
            {
                return Task.FromResult(AttriaxTrackingAuthorizationStatus.NotSupported);
            }

            return Task.FromResult(ParseTrackingAuthorizationStatus(CallIosRequestTrackingAuthorization()));
        }

        internal static Task<AttriaxInstallReferrerContextPayload> CollectInstallReferrerAsync(
            AttriaxPlatformType platform)
        {
            switch (platform)
            {
                case AttriaxPlatformType.Android:
                    return Task.FromResult(
                        ParseInstallReferrerContext(CallAndroidBridge("collectInstallReferrerJson")));
                case AttriaxPlatformType.IOS:
                    return Task.FromResult(ParseInstallReferrerContext(CallIosInstallReferrer()));
                default:
                    return Task.FromResult(new AttriaxInstallReferrerContextPayload());
            }
        }

        internal static Task<string?> ReadAttributionClipboardAsync(AttriaxPlatformType platform)
        {
            if (platform != AttriaxPlatformType.IOS)
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult(CallIosReadAttributionClipboard());
        }

        internal static Task<string?> CollectWebViewUserAgentAsync(AttriaxPlatformType platform)
        {
            if (platform != AttriaxPlatformType.IOS)
            {
                return Task.FromResult<string?>(null);
            }

            return Task.Run(CallIosCollectWebViewUserAgent);
        }

        internal static Task<bool> OpenBrowserUrlAsync(
            AttriaxPlatformType platform,
            string? url,
            AttriaxResolvedUrlOpenMode openMode)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Task.FromResult(false);
            }

            var opened = AttriaxLifecycleDispatcher.InvokeOnMainThread(() =>
            {
                switch (platform)
                {
                    case AttriaxPlatformType.Android:
                        return CallAndroidOpenBrowserUrl(url, openMode);
                    case AttriaxPlatformType.IOS:
                        return CallIosOpenBrowserUrl(url, openMode);
                    default:
                        Application.OpenURL(url);
                        return true;
                }
            });

            return Task.FromResult(opened);
        }

        internal static Task<AttriaxSkanUpdateResult> UpdateSkanConversionValueAsync(
            AttriaxPlatformType platform,
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
            if (platform != AttriaxPlatformType.IOS)
            {
                return Task.FromResult(new AttriaxSkanUpdateResult
                {
                    Status = AttriaxSkanUpdateStatus.NotSupported,
                    Message = "SKAdNetwork updates are only supported on iOS.",
                    FineValue = fineValue,
                    CoarseValue = coarseValue,
                    LockWindow = lockWindow,
                });
            }

            var payload = CallIosSkanUpdate(fineValue, coarseValue, lockWindow);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return Task.FromResult(new AttriaxSkanUpdateResult
                {
                    Status = AttriaxSkanUpdateStatus.Error,
                    Message = "The iOS native bridge did not return a SKAN update result.",
                    FineValue = fineValue,
                    CoarseValue = coarseValue,
                    LockWindow = lockWindow,
                });
            }

            try
            {
                var json = JObject.Parse(payload);
                return Task.FromResult(new AttriaxSkanUpdateResult
                {
                    Status = ParseSkanUpdateStatus(json.Value<string>("status")),
                    Message = json.Value<string>("message"),
                    FineValue = json.Value<int?>("fineValue") ?? fineValue,
                    CoarseValue = ParseSkanCoarseValue(json.Value<string>("coarseValue")) ?? coarseValue,
                    LockWindow = json.Value<bool?>("lockWindow") ?? lockWindow,
                });
            }
            catch (Exception error)
            {
                return Task.FromResult(new AttriaxSkanUpdateResult
                {
                    Status = AttriaxSkanUpdateStatus.Error,
                    Message = "Failed to parse the iOS SKAN update result: " + error.Message,
                    FineValue = fineValue,
                    CoarseValue = coarseValue,
                    LockWindow = lockWindow,
                });
            }
        }

        private static AttriaxNativeContextPayload ParseNativeContext(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new AttriaxNativeContextPayload();
            }

            try
            {
                var json = JObject.Parse(payload);
                return new AttriaxNativeContextPayload
                {
                    InstallReferrer = json.Value<string>("installReferrer"),
                    AndroidId = json.Value<string>("androidId"),
                    AdvertisingId = json.Value<string>("advertisingId"),
                    Metadata = ReadMetadata(json),
                };
            }
            catch (Exception error)
            {
                return new AttriaxNativeContextPayload
                {
                    Metadata = new Dictionary<string, object>
                    {
                        ["bridgeParseError"] = error.Message,
                    },
                };
            }
        }

        private static AttriaxInstallReferrerContextPayload ParseInstallReferrerContext(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new AttriaxInstallReferrerContextPayload();
            }

            try
            {
                var json = JObject.Parse(payload);
                return new AttriaxInstallReferrerContextPayload
                {
                    InstallReferrer = json.Value<string>("installReferrer"),
                    InstallBeginTimestampSeconds = json.Value<long?>("installBeginTimestampSeconds"),
                    ReferrerClickTimestampSeconds = json.Value<long?>("referrerClickTimestampSeconds"),
                    GooglePlayInstantParam = json.Value<bool?>("googlePlayInstantParam"),
                    Metadata = ReadMetadata(json),
                };
            }
            catch (Exception error)
            {
                return new AttriaxInstallReferrerContextPayload
                {
                    Metadata = new Dictionary<string, object>
                    {
                        ["bridgeParseError"] = error.Message,
                    },
                };
            }
        }

        private static Dictionary<string, object> ReadMetadata(JObject json)
        {
            var metadata = json["metadata"] as JObject;
            if (metadata == null)
            {
                return new Dictionary<string, object>();
            }

            return metadata.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
        }

        private static string? CallAndroidBridge(string methodName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var bridge = new AndroidJavaClass("com.attriax.unity.AttriaxUnityAndroidBridge"))
                {
                    if (activity == null)
                    {
                        return null;
                    }

                    return bridge.CallStatic<string>(methodName, activity);
                }
            }
            catch (Exception error)
            {
                AttriaxLifecycleDispatcher.InvokeOnMainThread(
                    () => Debug.LogWarning($"[Attriax] Android native bridge call failed: {error.Message}"));
                return null;
            }
#else
            return null;
#endif
        }

        private static string? CallAndroidNativeContext(bool collectAdvertisingId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var bridge = new AndroidJavaClass("com.attriax.unity.AttriaxUnityAndroidBridge"))
                {
                    if (activity == null)
                    {
                        return null;
                    }

                    return bridge.CallStatic<string>(
                        "collectNativeContextJson",
                        activity,
                        collectAdvertisingId);
                }
            }
            catch (Exception error)
            {
                AttriaxLifecycleDispatcher.InvokeOnMainThread(
                    () => Debug.LogWarning($"[Attriax] Android native bridge call failed: {error.Message}"));
                return null;
            }
#else
            return null;
#endif
        }

        private static bool CallAndroidOpenBrowserUrl(
            string url,
            AttriaxResolvedUrlOpenMode openMode)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var bridge = new AndroidJavaClass("com.attriax.unity.AttriaxUnityAndroidBridge"))
                {
                    if (activity == null)
                    {
                        return false;
                    }

                    return bridge.CallStatic<bool>(
                        "openBrowserUrl",
                        activity,
                        url,
                        NormalizeOpenModeValue(openMode));
                }
            }
            catch (Exception error)
            {
                AttriaxLifecycleDispatcher.InvokeOnMainThread(
                    () => Debug.LogWarning($"[Attriax] Android browser bridge call failed: {error.Message}"));
                return false;
            }
#else
            return false;
#endif
        }

        private static string? CallIosNativeContext(bool collectAdvertisingId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(() => AttriaxUnity_CopyNativeContextJson(collectAdvertisingId));
#else
            return null;
#endif
        }

        private static string? CallIosTrackingAuthorizationStatus()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(AttriaxUnity_CopyTrackingAuthorizationStatusString);
    #else
            return null;
    #endif
        }

        private static string? CallIosRequestTrackingAuthorization()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(AttriaxUnity_RequestTrackingAuthorization);
    #else
            return null;
    #endif
        }

        private static string? CallIosInstallReferrer()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(AttriaxUnity_CopyInstallReferrerJson);
#else
            return null;
#endif
        }

        private static string? CallIosReadAttributionClipboard()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(AttriaxUnity_CopyAttributionClipboardText);
    #else
            return null;
    #endif
        }

        private static string? CallIosCollectWebViewUserAgent()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(AttriaxUnity_CopyWebViewUserAgent);
    #else
            return null;
    #endif
        }

        private static bool CallIosOpenBrowserUrl(
            string url,
            AttriaxResolvedUrlOpenMode openMode)
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return AttriaxUnity_OpenBrowserUrl(url, NormalizeOpenModeValue(openMode));
    #else
            return false;
    #endif
        }

        private static string? CallIosSkanUpdate(
            int fineValue,
            AttriaxSkanCoarseValue? coarseValue,
            bool lockWindow)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return CopyStringFromNative(() =>
                AttriaxUnity_CopySkanUpdateResultJson(
                    fineValue,
                    NormalizeSkanCoarseValue(coarseValue),
                    lockWindow));
#else
            return null;
#endif
        }

        private static string NormalizeOpenModeValue(AttriaxResolvedUrlOpenMode openMode)
        {
            return openMode == AttriaxResolvedUrlOpenMode.External
            ? "external"
            : "in_app";
        }

        private static string? NormalizeSkanCoarseValue(AttriaxSkanCoarseValue? coarseValue)
        {
            return coarseValue switch
            {
                AttriaxSkanCoarseValue.Low => "low",
                AttriaxSkanCoarseValue.Medium => "medium",
                AttriaxSkanCoarseValue.High => "high",
                _ => null,
            };
        }

        private static AttriaxSkanUpdateStatus ParseSkanUpdateStatus(string? value)
        {
            return value switch
            {
                "updated" => AttriaxSkanUpdateStatus.Updated,
                "skipped" => AttriaxSkanUpdateStatus.Skipped,
                "already_at_or_above_value" => AttriaxSkanUpdateStatus.AlreadyAtOrAboveValue,
                "invalid_value" => AttriaxSkanUpdateStatus.InvalidValue,
                "disabled" => AttriaxSkanUpdateStatus.Disabled,
                "not_supported" => AttriaxSkanUpdateStatus.NotSupported,
                _ => AttriaxSkanUpdateStatus.Error,
            };
        }

        private static AttriaxSkanCoarseValue? ParseSkanCoarseValue(string? value)
        {
            return value switch
            {
                "low" => AttriaxSkanCoarseValue.Low,
                "medium" => AttriaxSkanCoarseValue.Medium,
                "high" => AttriaxSkanCoarseValue.High,
                _ => null,
            };
        }

        private static AttriaxTrackingAuthorizationStatus ParseTrackingAuthorizationStatus(
            string? value)
        {
            return value switch
            {
                "not_supported" => AttriaxTrackingAuthorizationStatus.NotSupported,
                "disabled" => AttriaxTrackingAuthorizationStatus.Disabled,
                "not_determined" => AttriaxTrackingAuthorizationStatus.NotDetermined,
                "restricted" => AttriaxTrackingAuthorizationStatus.Restricted,
                "denied" => AttriaxTrackingAuthorizationStatus.Denied,
                "authorized" => AttriaxTrackingAuthorizationStatus.Authorized,
                "timed_out" => AttriaxTrackingAuthorizationStatus.TimedOut,
                "unknown" => AttriaxTrackingAuthorizationStatus.Unknown,
                _ => AttriaxTrackingAuthorizationStatus.Unknown,
            };
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopyNativeContextJson(
            [MarshalAs(UnmanagedType.I1)] bool collectAdvertisingId);

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopyTrackingAuthorizationStatusString();

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_RequestTrackingAuthorization();

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopyInstallReferrerJson();

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopyAttributionClipboardText();

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopyWebViewUserAgent();

        [DllImport("__Internal")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AttriaxUnity_OpenBrowserUrl(
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.LPStr)] string openMode);

        [DllImport("__Internal")]
        private static extern IntPtr AttriaxUnity_CopySkanUpdateResultJson(
            int fineValue,
            [MarshalAs(UnmanagedType.LPStr)] string? coarseValue,
            [MarshalAs(UnmanagedType.I1)] bool lockWindow);

        [DllImport("__Internal")]
        private static extern void AttriaxUnity_FreeString(IntPtr value);

        private static string? CopyStringFromNative(Func<IntPtr> nativeCall)
        {
            var pointer = nativeCall();
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringAnsi(pointer);
            }
            finally
            {
                AttriaxUnity_FreeString(pointer);
            }
        }
#endif
    }

    internal sealed class AttriaxNativeContextPayload
    {
        internal string? InstallReferrer { get; set; }

        internal string? AndroidId { get; set; }

        internal string? AdvertisingId { get; set; }

        internal Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    internal sealed class AttriaxInstallReferrerContextPayload
    {
        internal string? InstallReferrer { get; set; }

        internal long? InstallBeginTimestampSeconds { get; set; }

        internal long? ReferrerClickTimestampSeconds { get; set; }

        internal bool? GooglePlayInstantParam { get; set; }

        internal Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}