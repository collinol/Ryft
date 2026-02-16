using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Equipment;

namespace Game.UI
{
    /// <summary>
    /// Singleton tooltip that shows equipment information on hover.
    /// Mirrors the CardTooltip pattern.
    /// </summary>
    public class EquipmentTooltip : MonoBehaviour
    {
        public static EquipmentTooltip Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI slotRarityText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI durabilityText;
        [SerializeField] private TextMeshProUGUI effectText;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(15f, -15f);
        [SerializeField] private float fadeSpeed = 12f;

        private bool isShowing;
        private Canvas canvas;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (!tooltipRect) tooltipRect = GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float target = isShowing ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, target, Time.unscaledDeltaTime * fadeSpeed);

            if (isShowing)
            {
                UpdatePosition(Input.mousePosition);
            }
            else if (canvasGroup.alpha < 0.01f)
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdatePosition(Vector2 mousePos)
        {
            if (!tooltipRect || !canvas) return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            localPoint += offset;

            // Clamp to canvas bounds
            Vector2 size = tooltipRect.sizeDelta;
            Vector2 canvasSize = canvasRect.sizeDelta;
            float pivotX = tooltipRect.pivot.x;
            float pivotY = tooltipRect.pivot.y;

            float left  = localPoint.x - size.x * pivotX;
            float right = localPoint.x + size.x * (1f - pivotX);
            float bot   = localPoint.y - size.y * pivotY;
            float top   = localPoint.y + size.y * (1f - pivotY);

            float halfW = canvasSize.x * 0.5f;
            float halfH = canvasSize.y * 0.5f;

            if (right > halfW)  localPoint.x -= (right - halfW);
            if (left  < -halfW) localPoint.x += (-halfW - left);
            if (top   > halfH)  localPoint.y -= (top - halfH);
            if (bot   < -halfH) localPoint.y += (-halfH - bot);

            tooltipRect.localPosition = localPoint;
        }

        // ── Public API ──────────────────────────────────────────

        public void Show(EquipmentInstance item)
        {
            if (item?.def == null) return;

            var def = item.def;

            // Title – colored by rarity
            if (titleText)
            {
                titleText.text = def.displayName;
                titleText.color = GetRarityColor(def.rarity);
            }

            // Slot & Rarity
            if (slotRarityText)
                slotRarityText.text = $"{def.slot} - {def.rarity}";

            // Description
            if (descriptionText)
            {
                bool hasDesc = !string.IsNullOrEmpty(def.description);
                descriptionText.text = hasDesc ? def.description : "";
                descriptionText.gameObject.SetActive(hasDesc);
            }

            // Stats
            if (statsText)
            {
                string s = FormatStats(def);
                statsText.text = s;
                statsText.gameObject.SetActive(!string.IsNullOrEmpty(s));
            }

            // Durability (conditional)
            if (durabilityText)
            {
                bool show = def.maxDurability > 0;
                durabilityText.text = show ? $"Durability: {item.currentDurability}/{def.maxDurability}" : "";
                durabilityText.gameObject.SetActive(show);
            }

            // Effect (conditional)
            if (effectText)
            {
                bool hasEffect = !string.IsNullOrEmpty(def.runtimeEffectTypeName);
                effectText.text = hasEffect ? $"Effect: {ExtractClassName(def.runtimeEffectTypeName)}" : "";
                effectText.gameObject.SetActive(hasEffect);
            }

            isShowing = true;
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f; // start invisible, fade in via Update
        }

        public void Hide()
        {
            isShowing = false;
            // Will fade out and disable in Update
        }

        // ── Helpers ─────────────────────────────────────────────

        private static string FormatStats(EquipmentDef equip)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (equip.bonusStats.maxHealth  != 0) parts.Add($"HP {equip.bonusStats.maxHealth:+#;-#;0}");
            if (equip.bonusStats.strength   != 0) parts.Add($"STR {equip.bonusStats.strength:+#;-#;0}");
            if (equip.bonusStats.intellect  != 0) parts.Add($"INT {equip.bonusStats.intellect:+#;-#;0}");
            if (equip.bonusStats.engineering!= 0) parts.Add($"ENG {equip.bonusStats.engineering:+#;-#;0}");
            return string.Join(" | ", parts);
        }

        private static string ExtractClassName(string fullyQualified)
        {
            if (string.IsNullOrEmpty(fullyQualified)) return "";
            int dot = fullyQualified.LastIndexOf('.');
            return dot >= 0 ? fullyQualified.Substring(dot + 1) : fullyQualified;
        }

        private static Color GetRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common    => new Color(0.7f, 0.7f, 0.7f),
                EquipmentRarity.Uncommon  => new Color(0.3f, 0.9f, 0.3f),
                EquipmentRarity.Rare      => new Color(0.3f, 0.5f, 1f),
                EquipmentRarity.Epic      => new Color(0.8f, 0.3f, 1f),
                EquipmentRarity.Legendary => new Color(1f, 0.85f, 0.2f),
                _                         => Color.white
            };
        }

        // ── Factory (mirrors CardTooltip.Ensure) ────────────────

        public static EquipmentTooltip Ensure(Canvas parentCanvas)
        {
            if (Instance) return Instance;

            var existing = FindObjectOfType<EquipmentTooltip>();
            if (existing) return existing;

            if (parentCanvas == null)
            {
                Debug.LogWarning("[EquipmentTooltip] Cannot create tooltip — Canvas is null!");
                return null;
            }

            // Root GO
            var go = new GameObject("EquipmentTooltip");
            var rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(parentCanvas.transform, false);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot     = new Vector2(0f, 1f); // top-left pivot
            rect.sizeDelta = new Vector2(320f, 200f);

            var tooltip = go.AddComponent<EquipmentTooltip>();

            // Dark background
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // Vertical layout
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 10, 10);
            vlg.spacing = 4f;
            vlg.childAlignment       = TextAnchor.UpperLeft;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // ── Child text rows ──

            // Title
            tooltip.titleText = CreateLabel(go.transform, "Title", 20, FontStyles.Bold, Color.white);

            // Slot & Rarity
            tooltip.slotRarityText = CreateLabel(go.transform, "SlotRarity", 14, FontStyles.Italic,
                new Color(0.75f, 0.75f, 0.75f));

            // Description
            tooltip.descriptionText = CreateLabel(go.transform, "Description", 14, FontStyles.Normal,
                new Color(0.9f, 0.9f, 0.9f), wordWrap: true);

            // Stats
            tooltip.statsText = CreateLabel(go.transform, "Stats", 14, FontStyles.Normal,
                new Color(0.6f, 0.9f, 1f));

            // Durability
            tooltip.durabilityText = CreateLabel(go.transform, "Durability", 13, FontStyles.Normal,
                new Color(0.9f, 0.8f, 0.4f));

            // Effect
            tooltip.effectText = CreateLabel(go.transform, "Effect", 13, FontStyles.Normal,
                new Color(0.5f, 1f, 0.7f));

            tooltip.tooltipRect = rect;
            tooltip.Hide();

            return tooltip;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, float fontSize,
            FontStyles style, Color color, bool wordWrap = false)
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            obj.transform.SetParent(parent, false);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize         = fontSize;
            tmp.fontStyle        = style;
            tmp.color            = color;
            tmp.enableWordWrapping = wordWrap;
            tmp.overflowMode     = TextOverflowModes.Truncate;
            tmp.raycastTarget    = false;
            return tmp;
        }
    }
}
