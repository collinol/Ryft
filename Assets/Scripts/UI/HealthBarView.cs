// Assets/Scripts/UI/HealthBarView.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    /// Minimal per-actor health bar. No per-frame updates.
    /// Create with HealthBarView.Attach(...), then call Set(current,max) when HP changes.
    [DisallowMultipleComponent]
    public class HealthBarView : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] Vector2 size = new(1.2f, 0.15f);
        [SerializeField] Color bgColor   = new(0.85f, 0.85f, 0.85f, 0.95f);
        [SerializeField] Color fillColor = Color.white;

        Canvas canvas;
        Image bg;
        Image fill;

        static Sprite s_WhiteSprite;

        /// Create a world-space bar as a child of `owner`, offset above it.
        public static HealthBarView Attach(Transform owner, Vector3 localOffset, Vector2? sizeOverride = null)
        {
            // Reuse if already present
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

            // World-space canvas on this GO
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;

            // 1x1 white sprite created at runtime (works in all pipelines)
            var white = GetWhiteSprite();

            // Background (simple colored rect)
            var bgGO = new GameObject("BG", typeof(RectTransform));
            bgGO.transform.SetParent(transform, false);
            bg = bgGO.AddComponent<Image>();
            bg.sprite = white;
            bg.type = Image.Type.Simple;
            bg.color = bgColor;
            bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = size;

            // Fill (use Image.fillAmount; requires any sprite)
            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(bg.rectTransform, false);
            fill = fillGO.AddComponent<Image>();
            fill.sprite = white;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0; // left-to-right
            fill.color = fillColor;
            fill.raycastTarget = false;

            var frt = fill.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
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

        /// Update the bar to show `current/max`. Also hides when current <= 0.
        public void Set(int current, int max)
        {
            if (!fill || max <= 0) return;
            current = Mathf.Clamp(current, 0, max);
            fill.fillAmount = (float)current / max;
            if (canvas) canvas.enabled = current > 0;
        }

        /// Convenience when you have an IActor handy.
        public void SetFrom(IActor actor)
        {
            if (actor == null) return;
            Set(actor.Health, Mathf.Max(1, actor.TotalStats.maxHealth));
        }
    }
}
