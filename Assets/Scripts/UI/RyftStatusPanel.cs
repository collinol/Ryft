using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Ryfts;

namespace Game.UI
{
    public class RyftStatusPanel : MonoBehaviour
    {
        public static RyftStatusPanel Instance { get; private set; }

        // --- colour look-up ---
        static readonly Dictionary<RyftColor, Color> ColorMap = new()
        {
            { RyftColor.Orange, new Color32(0xFF, 0x8C, 0x00, 0xFF) },
            { RyftColor.Green,  new Color32(0x22, 0xCC, 0x44, 0xFF) },
            { RyftColor.Blue,   new Color32(0x44, 0x88, 0xFF, 0xFF) },
            { RyftColor.Purple, new Color32(0xAA, 0x44, 0xFF, 0xFF) },
        };

        static readonly string[] PolarityLabel = { "Closed", "Exploded" };

        // runtime refs
        readonly List<IconSlot> slots = new();
        RectTransform stripRT;
        GameObject tooltipPanel;
        RectTransform tooltipRT;
        Text tooltipText;
        float refreshTimer;

        const float RefreshInterval = 0.5f;
        const int IconSize = 32;
        const int IconSpacing = 6;
        const int StripPadding = 8;
        const int TooltipPad = 10;
        const int TooltipWidth = 300;

        // ---- bootstrap ----
        public static RyftStatusPanel Ensure()
        {
            if (Instance) return Instance;
            var go = new GameObject("RyftStatusPanel");
            return go.AddComponent<RyftStatusPanel>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUI();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ---- UI construction ----
        void BuildUI()
        {
            // Canvas lives on this root DontDestroyOnLoad object.
            // Mark the Canvas component with DontSave so that
            // FindObjectOfType<Canvas>() in scene controllers (shop, health
            // station, etc.) won't find it and accidentally parent their UI here.
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvas.hideFlags = HideFlags.DontSave;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Icon strip container — anchored top-right, shifted left to avoid Inventory button
            stripRT = CreateRT("IconStrip", transform);
            stripRT.anchorMin = new Vector2(1, 1);
            stripRT.anchorMax = new Vector2(1, 1);
            stripRT.pivot = new Vector2(1, 1);
            int totalWidth = 8 * IconSize + 7 * IconSpacing + StripPadding * 2;
            int totalHeight = IconSize + StripPadding * 2;
            stripRT.sizeDelta = new Vector2(totalWidth, totalHeight);
            // Inventory button occupies roughly the rightmost 150px; push strip left of that
            stripRT.anchoredPosition = new Vector2(-160, -10);

            // semi-transparent background
            var stripBg = stripRT.gameObject.AddComponent<Image>();
            stripBg.color = new Color(0, 0, 0, 0.35f);
            stripBg.raycastTarget = false;

            // Horizontal layout
            var hlg = stripRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = IconSpacing;
            hlg.padding = new RectOffset(StripPadding, StripPadding, StripPadding, StripPadding);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Create 8 icons: for each colour -> Closed, Exploded
            foreach (RyftColor rc in System.Enum.GetValues(typeof(RyftColor)))
            {
                for (int p = 0; p < 2; p++)
                {
                    var polarity = (EffectPolarity)p;
                    var slot = CreateIcon(stripRT, rc, polarity);
                    slots.Add(slot);
                }
            }

            // Tooltip panel (hidden by default)
            BuildTooltip();
        }

        static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        IconSlot CreateIcon(RectTransform parent, RyftColor rc, EffectPolarity pol)
        {
            var go = new GameObject($"Icon_{rc}_{pol}");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = IconSize;
            le.preferredHeight = IconSize;
            le.minWidth = IconSize;
            le.minHeight = IconSize;

            // background image (the coloured square)
            var img = go.AddComponent<Image>();
            Color baseColor = ColorMap[rc];
            img.color = baseColor;
            img.raycastTarget = true;

            // "X" overlay for exploded icons
            if (pol == EffectPolarity.Negative)
            {
                var ov = new GameObject("X");
                var ovRt = ov.AddComponent<RectTransform>();
                ovRt.SetParent(rt, false);
                ovRt.anchorMin = Vector2.zero;
                ovRt.anchorMax = Vector2.one;
                ovRt.sizeDelta = Vector2.zero;
                var overlayText = ov.AddComponent<Text>();
                overlayText.text = "X";
                overlayText.font = GetFont();
                overlayText.fontSize = 20;
                overlayText.fontStyle = FontStyle.Bold;
                overlayText.alignment = TextAnchor.MiddleCenter;
                overlayText.color = new Color(0, 0, 0, 0.7f);
                overlayText.raycastTarget = false;
            }

            // Count badge
            var badge = new GameObject("Count");
            var badgeRt = badge.AddComponent<RectTransform>();
            badgeRt.SetParent(rt, false);
            badgeRt.anchorMin = new Vector2(1, 0);
            badgeRt.anchorMax = new Vector2(1, 0);
            badgeRt.pivot = new Vector2(1, 0);
            badgeRt.sizeDelta = new Vector2(IconSize, 14);
            badgeRt.anchoredPosition = Vector2.zero;
            var countText = badge.AddComponent<Text>();
            countText.text = "";
            countText.font = GetFont();
            countText.fontSize = 11;
            countText.fontStyle = FontStyle.Bold;
            countText.alignment = TextAnchor.LowerRight;
            countText.color = Color.white;
            countText.raycastTarget = false;

            var outline = badge.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            // Hover handler
            var hover = go.AddComponent<IconHover>();
            hover.panel = this;
            hover.color = rc;
            hover.polarity = pol;

            return new IconSlot
            {
                color = rc,
                polarity = pol,
                image = img,
                baseColor = baseColor,
                countText = countText,
                root = rt,
            };
        }

        void BuildTooltip()
        {
            tooltipPanel = new GameObject("RyftTooltip");
            tooltipRT = tooltipPanel.AddComponent<RectTransform>();
            tooltipRT.SetParent(transform, false);
            tooltipRT.anchorMin = new Vector2(1, 1);
            tooltipRT.anchorMax = new Vector2(1, 1);
            tooltipRT.pivot = new Vector2(1, 1);
            tooltipRT.sizeDelta = new Vector2(TooltipWidth, 60);

            var bg = tooltipPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.88f);
            bg.raycastTarget = false;

            // Text child — fills parent with inner padding
            var textGo = new GameObject("Text");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(tooltipRT, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(TooltipPad, TooltipPad);
            textRt.offsetMax = new Vector2(-TooltipPad, -TooltipPad);

            tooltipText = textGo.AddComponent<Text>();
            tooltipText.font = GetFont();
            tooltipText.fontSize = 14;
            tooltipText.color = Color.white;
            tooltipText.alignment = TextAnchor.UpperLeft;
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipText.verticalOverflow = VerticalWrapMode.Overflow;
            tooltipText.raycastTarget = false;
            tooltipText.supportRichText = true;

            tooltipPanel.SetActive(false);
        }

        // ---- refresh loop ----
        void Update()
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = RefreshInterval;
                RefreshIcons();
            }
        }

        void RefreshIcons()
        {
            var mgr = RyftEffectManager.Instance;
            if (mgr == null) return;

            var effects = mgr.ActiveEffects;

            foreach (var slot in slots)
            {
                int count = 0;
                if (effects != null)
                {
                    for (int i = 0; i < effects.Count; i++)
                    {
                        var e = effects[i];
                        if (e?.Def != null && e.Def.color == slot.color && e.Def.polarity == slot.polarity)
                            count++;
                    }
                }

                bool hasEffects = count > 0;
                Color c = slot.baseColor;
                c.a = hasEffects ? 1f : 0.3f;
                slot.image.color = c;
                slot.countText.text = hasEffects ? count.ToString() : "";
            }
        }

        // ---- hover ----
        internal void ShowTooltip(RyftColor color, EffectPolarity polarity, RectTransform anchor)
        {
            var mgr = RyftEffectManager.Instance;
            if (mgr == null) return;

            string header = $"<b>{color} - {PolarityLabel[(int)polarity]}</b>\n\n";

            var matching = mgr.ActiveEffects?
                .Where(e => e?.Def != null && e.Def.color == color && e.Def.polarity == polarity)
                .ToList();

            string body;
            if (matching == null || matching.Count == 0)
            {
                body = "<i>No effects</i>";
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                foreach (var e in matching)
                {
                    sb.Append("<b>").Append(e.Def.displayName);
                    if (e.stacks > 1) sb.Append($" x{e.stacks}");
                    sb.Append("</b>");
                    sb.AppendLine();
                    if (!string.IsNullOrEmpty(e.Def.description))
                        sb.AppendLine(e.Def.description);
                    sb.AppendLine();
                }
                body = sb.ToString().TrimEnd();
            }

            tooltipText.text = header + body;

            // Force a layout rebuild so preferredHeight is up to date
            Canvas.ForceUpdateCanvases();
            float textHeight = tooltipText.preferredHeight;
            float panelHeight = textHeight + TooltipPad * 2;
            tooltipRT.sizeDelta = new Vector2(TooltipWidth, Mathf.Max(60, panelHeight));

            // Position tooltip directly below the icon strip
            float yBelow = stripRT.anchoredPosition.y - stripRT.sizeDelta.y - 4;
            tooltipRT.anchoredPosition = new Vector2(stripRT.anchoredPosition.x, yBelow);

            tooltipPanel.SetActive(true);
        }

        internal void HideTooltip()
        {
            if (tooltipPanel) tooltipPanel.SetActive(false);
        }

        // ---- helper ----
        static RectTransform CreateRT(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        // ---- data ----
        class IconSlot
        {
            public RyftColor color;
            public EffectPolarity polarity;
            public Image image;
            public Color baseColor;
            public Text countText;
            public RectTransform root;
        }

        // ---- hover component ----
        class IconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            internal RyftStatusPanel panel;
            internal RyftColor color;
            internal EffectPolarity polarity;

            public void OnPointerEnter(PointerEventData eventData)
            {
                panel?.ShowTooltip(color, polarity, GetComponent<RectTransform>());
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                panel?.HideTooltip();
            }
        }
    }
}
