package com.attriax.unity;

import android.annotation.SuppressLint;
import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.net.Uri;
import android.net.http.SslError;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.ViewGroup;
import android.webkit.RenderProcessGoneDetail;
import android.webkit.SslErrorHandler;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import androidx.activity.ComponentActivity;
import androidx.activity.OnBackPressedCallback;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.view.WindowCompat;
import androidx.core.view.WindowInsetsControllerCompat;
import java.lang.ref.WeakReference;
import java.net.URISyntaxException;

public final class AttriaxUnityInAppBrowserActivity extends ComponentActivity {
    public static final String EXTRA_URL = "attriax.browser_url";

    private static final String TAG = "AttriaxUnityBrowser";

    private WebView webView;
    private final OnBackPressedCallback backPressedCallback = new OnBackPressedCallback(true) {
        @Override
        public void handleOnBackPressed() {
            if (webView != null && webView.canGoBack()) {
                webView.goBack();
                return;
            }

            finish();
        }
    };

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final String url = getIntent().getStringExtra(EXTRA_URL);
        if (url == null || url.trim().isEmpty()) {
            finish();
            return;
        }

        getOnBackPressedDispatcher().addCallback(this, backPressedCallback);
        webView = createWebView();
        setContentView(webView);
        configureStatusBar();
        webView.loadUrl(url);
    }

    @Override
    protected void onPause() {
        if (webView != null) {
            webView.onPause();
            webView.pauseTimers();
        }
        super.onPause();
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (webView != null) {
            webView.resumeTimers();
            webView.onResume();
        }
    }

    @Override
    protected void onDestroy() {
        backPressedCallback.remove();
        destroyWebView();
        super.onDestroy();
    }

    @SuppressLint("SetJavaScriptEnabled")
    private WebView createWebView() {
        final WebView view = new WebView(this);
        view.setBackgroundColor(Color.WHITE);
        view.setLayoutParams(
            new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            )
        );

        final WebSettings settings = view.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setDomStorageEnabled(true);
        settings.setAllowFileAccess(false);
        settings.setAllowContentAccess(false);
        settings.setJavaScriptCanOpenWindowsAutomatically(false);
        settings.setSupportMultipleWindows(false);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            settings.setMixedContentMode(WebSettings.MIXED_CONTENT_NEVER_ALLOW);
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            settings.setSafeBrowsingEnabled(true);
        }

        view.setWebViewClient(new AttriaxBrowserWebViewClient(this));
        return view;
    }

    private void configureStatusBar() {
        final boolean supportsDarkStatusBarIcons = Build.VERSION.SDK_INT >= Build.VERSION_CODES.M;
        getWindow().setStatusBarColor(
            supportsDarkStatusBarIcons ? Color.WHITE : Color.parseColor("#202124")
        );

        final WindowInsetsControllerCompat insetsController =
            WindowCompat.getInsetsController(getWindow(), getWindow().getDecorView());
        if (insetsController != null) {
            insetsController.setAppearanceLightStatusBars(supportsDarkStatusBarIcons);
        }
    }

    private void destroyWebView() {
        final WebView currentWebView = webView;
        webView = null;
        if (currentWebView == null) {
            return;
        }

        currentWebView.stopLoading();
        currentWebView.loadUrl("about:blank");
        currentWebView.onPause();
        currentWebView.clearHistory();
        currentWebView.removeAllViews();
        currentWebView.setWebViewClient(null);

        if (currentWebView.getParent() instanceof ViewGroup) {
            ((ViewGroup) currentWebView.getParent()).removeView(currentWebView);
        }

        currentWebView.destroy();
    }

    private boolean handleExternalNavigation(@NonNull Uri uri) {
        if ("intent".equalsIgnoreCase(uri.getScheme())) {
            return launchIntentUri(uri.toString());
        }

        final Intent externalIntent = new Intent(Intent.ACTION_VIEW, uri);
        return startSafely(externalIntent);
    }

    private boolean launchIntentUri(@NonNull String rawUri) {
        try {
            final Intent intent = Intent.parseUri(rawUri, Intent.URI_INTENT_SCHEME);
            intent.addCategory(Intent.CATEGORY_BROWSABLE);
            intent.setComponent(null);
            intent.setSelector(null);

            if (startSafely(intent)) {
                return true;
            }

            final String fallbackUrl = intent.getStringExtra("browser_fallback_url");
            if (fallbackUrl != null && webView != null && !fallbackUrl.trim().isEmpty()) {
                webView.loadUrl(fallbackUrl);
                return true;
            }
        } catch (URISyntaxException exception) {
            Log.w(TAG, "Invalid intent URI in embedded browser", exception);
        }

        return false;
    }

    private boolean startSafely(@NonNull Intent intent) {
        try {
            startActivity(intent);
            return true;
        } catch (ActivityNotFoundException exception) {
            Log.w(TAG, "No activity available to handle browser navigation", exception);
            return false;
        }
    }

    private void handleSslError(@Nullable SslError error, @NonNull SslErrorHandler handler) {
        Log.w(TAG, "Blocking browser navigation because of an SSL error: " + error);
        handler.cancel();

        if (webView != null) {
            webView.stopLoading();
        }

        finish();
    }

    private void handleRenderProcessGone(@Nullable RenderProcessGoneDetail detail) {
        Log.e(TAG, "Embedded browser render process exited: " + detail);
        destroyWebView();
        finish();
    }

    private static final class AttriaxBrowserWebViewClient extends WebViewClient {
        private final WeakReference<AttriaxUnityInAppBrowserActivity> activityReference;

        AttriaxBrowserWebViewClient(@NonNull AttriaxUnityInAppBrowserActivity activity) {
            activityReference = new WeakReference<>(activity);
        }

        @Override
        public boolean shouldOverrideUrlLoading(
            @NonNull WebView view,
            @NonNull WebResourceRequest request
        ) {
            if (!request.isForMainFrame()) {
                return false;
            }

            return shouldOverrideUri(request.getUrl());
        }

        @Override
        @SuppressWarnings("deprecation")
        public boolean shouldOverrideUrlLoading(@NonNull WebView view, @NonNull String url) {
            return shouldOverrideUri(Uri.parse(url));
        }

        @Override
        public void onPageStarted(
            @NonNull WebView view,
            @NonNull String url,
            @Nullable Bitmap favicon
        ) {
            super.onPageStarted(view, url, favicon);
        }

        @Override
        public void onReceivedSslError(
            @NonNull WebView view,
            @NonNull SslErrorHandler handler,
            @NonNull SslError error
        ) {
            final AttriaxUnityInAppBrowserActivity activity = activityReference.get();
            if (activity == null || activity.isFinishing()) {
                handler.cancel();
                return;
            }

            activity.handleSslError(error, handler);
        }

        @Override
        public boolean onRenderProcessGone(
            @NonNull WebView view,
            @NonNull RenderProcessGoneDetail detail
        ) {
            final AttriaxUnityInAppBrowserActivity activity = activityReference.get();
            if (activity == null || activity.isFinishing()) {
                return true;
            }

            activity.handleRenderProcessGone(detail);
            return true;
        }

        @Override
        public void onReceivedError(
            @NonNull WebView view,
            @NonNull WebResourceRequest request,
            @NonNull WebResourceError error
        ) {
            super.onReceivedError(view, request, error);

            if (request.isForMainFrame()) {
                Log.w(TAG, "Embedded browser request failed: " + error);
            }
        }

        private boolean shouldOverrideUri(@NonNull Uri uri) {
            final String scheme = uri.getScheme();
            if (scheme == null) {
                return false;
            }

            if ("http".equalsIgnoreCase(scheme) || "https".equalsIgnoreCase(scheme)) {
                return false;
            }

            final AttriaxUnityInAppBrowserActivity activity = activityReference.get();
            return activity != null && activity.handleExternalNavigation(uri);
        }
    }
}