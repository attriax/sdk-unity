#nullable enable
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Attriax.Unity.Samples
{
    /// <summary>
    /// Builds the Rich Integration uGUI surface at runtime and binds it to the
    /// <see cref="AttriaxRichSampleController"/> on the same GameObject. Building the
    /// Canvas in code keeps the shipped <c>.unity</c> scene tiny and diff-friendly
    /// (one GameObject) while still producing genuine uGUI Buttons / Text / ScrollRect
    /// that a developer can inspect and click during manual testing.
    ///
    /// The view is intentionally passive: it never calls the SDK. It reads the
    /// controller's snapshot + recent-activity feed and forwards button clicks to
    /// the controller's public methods. It re-renders only when the controller
    /// flips its dirty flag, so background-thread SDK callbacks never touch uGUI.
    /// </summary>
    [RequireComponent(typeof(AttriaxRichSampleController))]
    [AddComponentMenu("Attriax/Samples/Attriax Rich Sample View")]
    public sealed class AttriaxRichSampleView : MonoBehaviour
    {
        private AttriaxRichSampleController _controller = null!;
        private Font _font = null!;
        private Text _statusText = null!;
        private Text _activityText = null!;
        private GameObject _consentBanner = null!;

        private void Awake()
        {
            _controller = GetComponent<AttriaxRichSampleController>();
            _font = ResolveFont();
            EnsureEventSystem();
            BuildUi();
        }

        private void Update()
        {
            if (_controller.IsDirty)
            {
                Render();
                _controller.ClearDirty();
            }
        }

        // ---------------------------------------------------------------------
        // Rendering
        // ---------------------------------------------------------------------

        private void Render()
        {
            _statusText.text = _controller.StatusMessage + "\n\n" + _controller.BuildSnapshot();
            _consentBanner.SetActive(_controller.IsWaitingForConsent);

            var builder = new StringBuilder();
            foreach (var entry in _controller.SnapshotActivity())
            {
                builder.Append(entry.At.ToString("HH:mm:ss"));
                builder.Append(entry.IsError ? "  [!] " : "  ");
                builder.Append(entry.Title);
                if (!string.IsNullOrEmpty(entry.Detail))
                {
                    builder.Append(" — ");
                    builder.Append(entry.Detail);
                }

                builder.Append('\n');
            }

            _activityText.text = builder.Length == 0 ? "No activity yet." : builder.ToString();
        }

        // ---------------------------------------------------------------------
        // UI construction
        // ---------------------------------------------------------------------

        private void BuildUi()
        {
            var canvasGo = new GameObject("AttriaxRichSampleCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var root = CreateChild(canvasGo.transform, "Root");
            StretchFull(root.rectTransform);
            var rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(16, 16, 16, 16);
            rootLayout.spacing = 8f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            AddBackground(root.gameObject, new Color(0.09f, 0.10f, 0.13f, 1f));

            CreateLabel(root.rectTransform, "Attriax Unity SDK — Rich Integration Sample", 22, FontStyle.Bold, 30f);

            // Status / snapshot panel.
            var statusPanel = CreateChild(root.rectTransform, "StatusPanel");
            AddBackground(statusPanel.gameObject, new Color(0.14f, 0.16f, 0.20f, 1f));
            var statusPadding = statusPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            statusPadding.padding = new RectOffset(10, 10, 8, 8);
            statusPadding.childControlWidth = true;
            statusPadding.childControlHeight = true;
            _statusText = CreateLabel(statusPanel.rectTransform, "…", 14, FontStyle.Normal, 0f);
            _statusText.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // Consent gate banner (shown only while consent is pending).
            _consentBanner = BuildConsentBanner(root.rectTransform);
            _consentBanner.SetActive(false);

            // Body: buttons on the left, recent-activity log on the right.
            var body = CreateChild(root.rectTransform, "Body");
            var bodyLayout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 12f;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = true;
            var bodyElement = body.gameObject.AddComponent<LayoutElement>();
            bodyElement.flexibleHeight = 1f;

            var buttonsContent = CreateScrollView(body.rectTransform, "Actions", 0.62f);
            BuildActionButtons(buttonsContent);

            var activityContent = CreateScrollView(body.rectTransform, "RecentActivity", 0.38f);
            CreateLabel(activityContent, "Recent Activity", 16, FontStyle.Bold, 24f);
            _activityText = CreateLabel(activityContent, "No activity yet.", 13, FontStyle.Normal, 0f);
            _activityText.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            Render();
        }

        private GameObject BuildConsentBanner(RectTransform parent)
        {
            var banner = CreateChild(parent, "ConsentBanner");
            AddBackground(banner.gameObject, new Color(0.35f, 0.24f, 0.05f, 1f));
            var layout = banner.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;

            var label = CreateLabel(banner.rectTransform, "GDPR consent required before attribution starts.", 14, FontStyle.Bold, 0f);
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            AddButton(banner.rectTransform, "Accept analytics", _controller.AcceptConsent, 160f);
            AddButton(banner.rectTransform, "Reject", _controller.RejectConsent, 100f);
            return banner.gameObject;
        }

        private void BuildActionButtons(RectTransform content)
        {
            AddSection(content, "Lifecycle");
            AddAsyncButton(content, "Start / Initialize", _controller.StartSampleAsync);
            AddAsyncButton(content, "Refresh diagnostics", _controller.RefreshDiagnosticsAsync);
            AddButton(content, "Toggle SDK enabled", _controller.ToggleSdkEnabled);
            AddButton(content, "Toggle events enabled", _controller.ToggleEventsEnabled);
            AddButton(content, "Toggle anonymous tracking", _controller.ToggleAnonymousTracking);
            AddAsyncButton(content, "ResetAsync", _controller.ResetAsync);

            AddSection(content, "Events & revenue");
            AddButton(content, "Record custom event", _controller.RecordCustomEvent);
            AddButton(content, "Record page view", _controller.RecordPageView);
            AddButton(content, "Record purchase", _controller.RecordPurchase);
            AddButton(content, "Record refund", _controller.RecordRefund);
            AddButton(content, "Record ad revenue", _controller.RecordAdRevenue);
            AddButton(content, "Record ad event (reward)", _controller.RecordAdEvent);
            AddButton(content, "Record handled error", _controller.RecordError);
            AddButton(content, "Record notification opened", _controller.RecordNotificationOpened);

            AddSection(content, "Identity");
            AddButton(content, "Set user", _controller.SetExampleUser);
            AddButton(content, "Clear user", _controller.ClearExampleUser);
            AddButton(content, "Identify (obsolete alias)", _controller.IdentifyExampleUser);
            AddButton(content, "Set user property", _controller.SetUserProperty);
            AddButton(content, "Set user properties", _controller.SetUserProperties);
            AddButton(content, "Clear user properties", _controller.ClearUserProperties);

            AddSection(content, "GDPR consent");
            AddButton(content, "Grant consent", _controller.AcceptConsent);
            AddButton(content, "Deny consent", _controller.RejectConsent);
            AddButton(content, "Mark not required", _controller.MarkConsentNotRequired);
            AddButton(content, "Reset consent", _controller.ResetConsent);
            AddAsyncButton(content, "Needs consent (local)", () => _controller.CheckNeedsConsentAsync(true));
            AddAsyncButton(content, "Needs consent (remote)", () => _controller.CheckNeedsConsentAsync(false));
            AddAsyncButton(content, "Request data erasure", _controller.RequestDataErasureAsync);

            AddSection(content, "ATT (iOS only)");
            AddAsyncButton(content, "Request ATT authorization", _controller.RequestAttAsync);
            AddAsyncButton(content, "Refresh ATT status", _controller.RefreshAttStatusAsync);

            AddSection(content, "SKAN (iOS only)");
            AddButton(content, "Refresh SKAN state", _controller.RefreshSkanState);
            AddAsyncButton(content, "Update SKAN conversion value", _controller.UpdateSkanConversionValueAsync);

            AddSection(content, "Deep links");
            AddAsyncButton(content, "Create dynamic link", _controller.CreateDynamicLinkAsync);
            AddAsyncButton(content, "Record manual deep link", _controller.RecordManualDeepLinkAsync);

            AddSection(content, "Referrer (Android-meaningful)");
            AddAsyncButton(content, "Refresh all referrers", _controller.RefreshReferrersAsync);

            AddSection(content, "Revenue validation");
            AddAsyncButton(content, "Validate receipt", _controller.ValidateReceiptAsync);

            AddSection(content, "Push tokens (needs Firebase / APNs)");
            AddAsyncButton(content, "Register FCM token", _controller.RegisterFcmTokenAsync);
            AddAsyncButton(content, "Register APNs token", _controller.RegisterApnsTokenAsync);
        }

        // ---------------------------------------------------------------------
        // uGUI helpers
        // ---------------------------------------------------------------------

        private void AddSection(RectTransform parent, string title)
        {
            var label = CreateLabel(parent, title, 15, FontStyle.Bold, 26f);
            label.color = new Color(0.62f, 0.78f, 1f, 1f);
        }

        private void AddButton(RectTransform parent, string label, System.Action onClick, float minWidth = 0f)
        {
            var buttonGo = CreateChild(parent, "Button:" + label);
            AddBackground(buttonGo.gameObject, new Color(0.20f, 0.28f, 0.42f, 1f));
            var button = buttonGo.gameObject.AddComponent<Button>();
            var element = buttonGo.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 30f;
            if (minWidth > 0f)
            {
                element.minWidth = minWidth;
                element.flexibleWidth = 0f;
            }

            var text = CreateLabel(buttonGo.rectTransform, label, 13, FontStyle.Normal, 0f);
            text.alignment = TextAnchor.MiddleCenter;
            StretchFull(text.rectTransform);
            button.onClick.AddListener(() => onClick());
        }

        private void AddAsyncButton(RectTransform parent, string label, System.Func<System.Threading.Tasks.Task> action)
        {
            AddButton(parent, label, () => { _ = action(); });
        }

        private RectTransform CreateScrollView(RectTransform parent, string name, float flexibleWidth)
        {
            var scrollGo = CreateChild(parent, "Scroll:" + name);
            AddBackground(scrollGo.gameObject, new Color(0.11f, 0.13f, 0.17f, 1f));
            var scrollRect = scrollGo.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;
            var scrollElement = scrollGo.gameObject.AddComponent<LayoutElement>();
            scrollElement.flexibleWidth = flexibleWidth;
            scrollElement.flexibleHeight = 1f;

            var viewport = CreateChild(scrollGo.rectTransform, "Viewport");
            StretchFull(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            scrollRect.viewport = viewport.rectTransform;

            var content = CreateChild(viewport.rectTransform, "Content");
            var contentRect = content.rectTransform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 5f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRect;

            return contentRect;
        }

        private Text CreateLabel(RectTransform parent, string value, int fontSize, FontStyle style, float minHeight)
        {
            var labelGo = CreateChild(parent, "Label");
            var text = labelGo.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            if (minHeight > 0f)
            {
                labelGo.gameObject.AddComponent<LayoutElement>().minHeight = minHeight;
            }

            return text;
        }

        private static Image CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            return image;
        }

        private static Image CreateChild(RectTransform parent, string name)
        {
            return CreateChild((Transform)parent, name);
        }

        private static void AddBackground(GameObject go, Color color)
        {
            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = color;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }

        private static Font ResolveFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Segoe UI" }, 14);
            }

            return font;
        }
    }
}
