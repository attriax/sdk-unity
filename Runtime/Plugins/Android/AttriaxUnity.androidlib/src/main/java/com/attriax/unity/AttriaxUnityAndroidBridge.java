package com.attriax.unity;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.InstallSourceInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.content.pm.verify.domain.DomainVerificationManager;
import android.content.pm.verify.domain.DomainVerificationUserState;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;
import com.android.installreferrer.api.InstallReferrerClient;
import com.android.installreferrer.api.InstallReferrerStateListener;
import com.android.installreferrer.api.ReferrerDetails;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import org.json.JSONArray;
import org.json.JSONObject;

public final class AttriaxUnityAndroidBridge {
    private AttriaxUnityAndroidBridge() {
    }

    public static String collectNativeContextJson(Activity activity, boolean collectAdvertisingId) {
        Context context = activity.getApplicationContext() != null
            ? activity.getApplicationContext()
            : activity;
        JSONObject payload = new JSONObject();
        JSONObject metadata = new JSONObject();

        try {
            metadata.put("source", "android_native");
            metadata.put("timezone", TimeZone.getDefault().getID());
            metadata.put("locale", Locale.getDefault().toLanguageTag());
            metadata.put("packageName", context.getPackageName());
            metadata.put("brand", Build.BRAND);
            metadata.put("manufacturer", Build.MANUFACTURER);
            metadata.put("model", Build.MODEL);
            metadata.put("device", Build.DEVICE);
            metadata.put("product", Build.PRODUCT);
            metadata.put("hardware", Build.HARDWARE);
            metadata.put("releaseVersion", Build.VERSION.RELEASE);
            metadata.put("sdkInt", Build.VERSION.SDK_INT);

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                metadata.put("securityPatch", Build.VERSION.SECURITY_PATCH);
            }

            JSONArray supportedAbis = new JSONArray();
            if (Build.SUPPORTED_ABIS != null) {
                for (String abi : Build.SUPPORTED_ABIS) {
                    supportedAbis.put(abi);
                }
            }
            metadata.put("supportedAbis", supportedAbis);

            PackageManager packageManager = context.getPackageManager();
            PackageInfo packageInfo = packageManager.getPackageInfo(context.getPackageName(), 0);
            metadata.put("versionName", packageInfo.versionName);

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                metadata.put("versionCode", packageInfo.getLongVersionCode());
            } else {
                metadata.put("versionCode", packageInfo.versionCode);
            }

            String rawAndroidId = Settings.Secure.getString(
                context.getContentResolver(),
                Settings.Secure.ANDROID_ID
            );
            if (rawAndroidId != null && !rawAndroidId.isEmpty()) {
                payload.put("androidId", rawAndroidId);
            }

            if (collectAdvertisingId) {
                String advertisingId = AdvertisingIdProvider.fetch(context);
                if (advertisingId != null && !advertisingId.isEmpty()) {
                    payload.put("advertisingId", advertisingId);
                }
            }

            appendInstallerPackageName(context, metadata);
            appendSigningFingerprints(context, metadata);
            appendDomainVerificationState(context, metadata);
        } catch (Exception exception) {
            try {
                metadata.put("nativeContextError", exception.getMessage());
            } catch (Exception ignored) {
            }
        }

        try {
            payload.put("metadata", metadata);
        } catch (Exception ignored) {
        }

        return payload.toString();
    }

    public static String collectInstallReferrerJson(Activity activity) {
        Context context = activity.getApplicationContext() != null
            ? activity.getApplicationContext()
            : activity;
        JSONObject payload = new JSONObject();
        JSONObject metadata = new JSONObject();

        try {
            metadata.put("source", "android_install_referrer");
        } catch (Exception ignored) {
        }

        final String[] installReferrer = new String[1];
        final CountDownLatch latch = new CountDownLatch(1);

        try {
            final InstallReferrerClient client = InstallReferrerClient.newBuilder(context).build();
            client.startConnection(new InstallReferrerStateListener() {
                @Override
                public void onInstallReferrerSetupFinished(int responseCode) {
                    try {
                        switch (responseCode) {
                            case InstallReferrerClient.InstallReferrerResponse.OK:
                                ReferrerDetails details = client.getInstallReferrer();
                                metadata.put("installReferrerStatus", "ok");
                                installReferrer[0] = details.getInstallReferrer();
                                metadata.put(
                                    "referrerClickTimestampSeconds",
                                    details.getReferrerClickTimestampSeconds()
                                );
                                metadata.put(
                                    "installBeginTimestampSeconds",
                                    details.getInstallBeginTimestampSeconds()
                                );
                                metadata.put(
                                    "googlePlayInstantParam",
                                    details.getGooglePlayInstantParam()
                                );
                                break;
                            case InstallReferrerClient.InstallReferrerResponse.FEATURE_NOT_SUPPORTED:
                                metadata.put("installReferrerStatus", "feature_not_supported");
                                break;
                            case InstallReferrerClient.InstallReferrerResponse.SERVICE_UNAVAILABLE:
                                metadata.put("installReferrerStatus", "service_unavailable");
                                break;
                            case InstallReferrerClient.InstallReferrerResponse.DEVELOPER_ERROR:
                                metadata.put("installReferrerStatus", "developer_error");
                                break;
                            case InstallReferrerClient.InstallReferrerResponse.SERVICE_DISCONNECTED:
                                metadata.put("installReferrerStatus", "service_disconnected");
                                break;
                            case InstallReferrerClient.InstallReferrerResponse.PERMISSION_ERROR:
                                metadata.put("installReferrerStatus", "permission_error");
                                break;
                            default:
                                metadata.put("installReferrerStatus", "unknown_response");
                                metadata.put("installReferrerCode", responseCode);
                                break;
                        }
                    } catch (Exception exception) {
                        try {
                            metadata.put("installReferrerError", exception.getMessage());
                        } catch (Exception ignored) {
                        }
                    } finally {
                        try {
                            client.endConnection();
                        } catch (Exception ignored) {
                        }
                        latch.countDown();
                    }
                }

                @Override
                public void onInstallReferrerServiceDisconnected() {
                    try {
                        metadata.put("installReferrerStatus", "service_disconnected");
                    } catch (Exception ignored) {
                    }
                    latch.countDown();
                }
            });

            if (!latch.await(12, TimeUnit.SECONDS)) {
                metadata.put("installReferrerStatus", "timeout_unity");
                try {
                    client.endConnection();
                } catch (Exception ignored) {
                }
            }
        } catch (Exception exception) {
            try {
                metadata.put("installReferrerStatus", "error_unity");
                metadata.put("installReferrerError", exception.getMessage());
            } catch (Exception ignored) {
            }
        }

        try {
            if (installReferrer[0] != null && !installReferrer[0].isEmpty()) {
                payload.put("installReferrer", installReferrer[0]);
            }
            payload.put("metadata", metadata);
        } catch (Exception ignored) {
        }

        return payload.toString();
    }

    public static boolean openBrowserUrl(Activity activity, String url, String openMode) {
        if (activity == null || url == null || url.trim().isEmpty()) {
            return false;
        }

        final boolean[] opened = new boolean[1];
        final CountDownLatch latch = new CountDownLatch(1);

        activity.runOnUiThread(() -> {
            try {
                Intent intent;
                if ("external".equals(openMode)) {
                    intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                } else {
                    intent = new Intent(activity, AttriaxUnityInAppBrowserActivity.class);
                    intent.putExtra(AttriaxUnityInAppBrowserActivity.EXTRA_URL, url);
                }

                activity.startActivity(intent);
                opened[0] = true;
            } catch (Exception ignored) {
                opened[0] = false;
            } finally {
                latch.countDown();
            }
        });

        try {
            latch.await(10, TimeUnit.SECONDS);
        } catch (InterruptedException ignored) {
            Thread.currentThread().interrupt();
            return false;
        }

        return opened[0];
    }

    private static void appendInstallerPackageName(Context context, JSONObject metadata) {
        try {
            PackageManager packageManager = context.getPackageManager();
            String installerPackageName;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                InstallSourceInfo installSourceInfo = packageManager.getInstallSourceInfo(
                    context.getPackageName()
                );
                installerPackageName = installSourceInfo.getInstallingPackageName();
            } else {
                installerPackageName = packageManager.getInstallerPackageName(context.getPackageName());
            }

            if (installerPackageName != null) {
                metadata.put("installerPackageName", installerPackageName);
            }
        } catch (Exception exception) {
            try {
                metadata.put("installerPackageNameError", exception.getMessage());
            } catch (Exception ignored) {
            }
        }
    }

    @SuppressWarnings("deprecation")
    private static void appendSigningFingerprints(Context context, JSONObject metadata) {
        try {
            PackageManager packageManager = context.getPackageManager();
            PackageInfo packageInfo;

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                packageInfo = packageManager.getPackageInfo(
                    context.getPackageName(),
                    PackageManager.GET_SIGNING_CERTIFICATES
                );
            } else {
                packageInfo = packageManager.getPackageInfo(
                    context.getPackageName(),
                    PackageManager.GET_SIGNATURES
                );
            }

            List<String> fingerprints = new ArrayList<>();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P && packageInfo.signingInfo != null) {
                Signature[] signatures = packageInfo.signingInfo.hasMultipleSigners()
                    ? packageInfo.signingInfo.getApkContentsSigners()
                    : packageInfo.signingInfo.getSigningCertificateHistory();

                if (signatures != null) {
                    for (Signature signature : signatures) {
                        String fingerprint = toSha256Fingerprint(signature.toByteArray());
                        if (fingerprint != null) {
                            fingerprints.add(fingerprint);
                        }
                    }
                }
            } else if (packageInfo.signatures != null) {
                for (Signature signature : packageInfo.signatures) {
                    String fingerprint = toSha256Fingerprint(signature.toByteArray());
                    if (fingerprint != null) {
                        fingerprints.add(fingerprint);
                    }
                }
            }

            if (!fingerprints.isEmpty()) {
                JSONArray values = new JSONArray();
                for (String fingerprint : fingerprints) {
                    values.put(fingerprint);
                }
                metadata.put("signingSha256Fingerprints", values);
            }
        } catch (Exception exception) {
            try {
                metadata.put("signingFingerprintError", exception.getMessage());
            } catch (Exception ignored) {
            }
        }
    }

    private static void appendDomainVerificationState(Context context, JSONObject metadata) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            return;
        }

        try {
            DomainVerificationManager manager = context.getSystemService(DomainVerificationManager.class);
            if (manager == null) {
                return;
            }

            DomainVerificationUserState state = manager.getDomainVerificationUserState(
                context.getPackageName()
            );
            if (state == null) {
                return;
            }

            JSONObject domains = new JSONObject();
            for (Map.Entry<String, Integer> entry : state.getHostToStateMap().entrySet()) {
                domains.put(entry.getKey(), entry.getValue());
            }
            metadata.put("domainVerificationState", domains);
            metadata.put("linkHandlingAllowed", state.isLinkHandlingAllowed());
        } catch (Exception exception) {
            try {
                metadata.put("domainVerificationError", exception.getMessage());
            } catch (Exception ignored) {
            }
        }
    }

    private static String toSha256Fingerprint(byte[] input) {
        try {
            MessageDigest digest = MessageDigest.getInstance("SHA-256");
            byte[] hashed = digest.digest(input);
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < hashed.length; index += 1) {
                if (index > 0) {
                    builder.append(':');
                }
                builder.append(String.format(Locale.US, "%02X", hashed[index]));
            }
            return builder.toString();
        } catch (Exception ignored) {
            return null;
        }
    }
}