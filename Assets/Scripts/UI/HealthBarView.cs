using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    [DisallowMultipleComponent]
    public class HealthBarView : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] Vector2 size = new(1.2f, 0.15f);
        [SerializeField] Color bgColor   = new(0.85f, 0.85f, 0.85f, 0.95f);
        [SerializeField] Color fillColor = Color.white;
        [SerializeField] Vector3 labelOffset = new(0f, 0.22f, 0f);

        [SerializeField] float uiScale = 0.05f;
        [SerializeField] int   labelMinFont = 10;
        [SerializeField] int   labelMaxFont = 24;
        [SerializeField] float labelHeightFraction = 1f;
        [SerializeField] Color labelColor = Color.black;

        Canvas canvas;
        Image bg;
        Image fill;
        Text  label;     // ← NEW

        static Sprite s_WhiteSprite;

        public static HealthBarView Attach(Transform owner, Vector3 localOffset, Vector2? sizeOverride = null)
        {
            var existing = owner.GetComponentInChildren<HealthBarView>(true);
            if (!existing)
            {
                var go = new GameObject("HealthBar", typeof(RectTransform));
                go.transform.SetParent(owner, false);
                go.transform.localPosition = localOffset;
                existing = go.AddComponent<HealthBarView>();
            }
            if (sizeOverride.HasValue) existing.size = sizeOverride.Value;
            existing.BuildIfNeeded();
            return existing;
        }

        void BuildIfNeeded()
        {
            if (canvas) return;

            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            transform.localScale = Vector3.one * uiScale;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f; // 100 px per world unit

            var white = GetWhiteSprite();

            // BG
            var bgGO = new GameObject("BG", typeof(RectTransform));
            bgGO.transform.SetParent(transform, false);
            bg = bgGO.AddComponent<Image>();
            bg.sprite = white;
            bg.type = Image.Type.Simple;
            bg.color = bgColor;
            bg.raycastTarget = false;

            // size is IN WORLD UNITS; convert to pixels for rect size
            var bgPixels = size * scaler.dynamicPixelsPerUnit;
            bg.rectTransform.sizeDelta = bgPixels;

            // Fill
            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(bg.rectTransform, false);
            fill = fillGO.AddComponent<Image>();
            fill.sprite = white;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.color = fillColor;
            fill.raycastTarget = false;

            var frt = fill.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;

            // LABEL (inside BG so it shares the same pixel rect)
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(bg.rectTransform, false);
            label = labelGO.AddComponent<UnityEngine.UI.Text>();
            label.raycastTarget = false;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.color = labelColor;
            label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow   = VerticalWrapMode.Truncate;

            var lrt = label.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            // Derive a sensible font size from the bar height (in pixels)
            int derived = Mathf.RoundToInt(bgPixels.y * labelHeightFraction);
            label.fontSize = Mathf.Clamp(derived, labelMinFont, labelMaxFont);
        }

        static Sprite GetWhiteSprite()
        {
            if (s_WhiteSprite != null) return s_WhiteSprite;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            s_WhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            s_WhiteSprite.name = "HealthBar_White1x1";
            return s_WhiteSprite;
        }

        public void Set(int current, int max)
        {
            if (!fill || max <= 0) return;
            current = Mathf.Clamp(current, 0, max);
            fill.fillAmount = (float)current / max;
            if (canvas) canvas.enabled = current > 0;

            if (label)
                label.text = $"{current}/{max}";
        }

        public void SetFrom(IActor actor)
        {
            if (actor == null) return;
            Set(actor.Health, Mathf.Max(1, actor.TotalStats.maxHealth));
        }
    }
}