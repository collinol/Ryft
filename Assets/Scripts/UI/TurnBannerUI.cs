// Assets/Scripts/UI/TurnBannerUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// Top-center "Player Turn / Enemy Turn" text only (no background).
    /// Safe: auto-stretches to full canvas, removes old Panel, and assigns Arial if no font.
    public class TurnBannerUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private float topPadding = 24f;

        [Header("Text Style")]
        [SerializeField] private int   fontSize  = 36;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Font  font; // optional; falls back to builtin Arial

        // Built child
        private Text label;

        void Awake()
        {
            EnsureRootStretch();
            CleanupOldPanel();
            EnsureLabel();
            LayoutLabel();
        }

        void OnEnable()
        {
            EnsureRootStretch();
            EnsureLabel();
            LayoutLabel();
        }

        public static TurnBannerUI Ensure(Canvas canvas)
        {
            if (!canvas) return null;
            var existing = canvas.GetComponentInChildren<TurnBannerUI>(true);
            if (existing)
            {
                existing.EnsureRootStretch();
                existing.CleanupOldPanel();
                existing.EnsureLabel();
                existing.LayoutLabel();
                return existing;
            }

            var go = new GameObject("TurnBanner", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            return go.AddComponent<TurnBannerUI>();
        }

        // -------- Public API --------
        public void Show(string text)        { SetText(text); /* no fade */ }
        public void ShowInstant(string text) { SetText(text); }
        public void Hide()                   { SetText(""); }

        // -------- Internals --------
        void EnsureRootStretch()
        {
            var rt = GetComponent<RectTransform>();
            if (!rt) return;
            // Stretch this GO to match the Canvas so our child can anchor to the real top of screen
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot     = new Vector2(0.5f, 0.5f);
        }

        void CleanupOldPanel()
        {
            // Remove dimming panel from older versions if it still exists
            var old = transform.Find("Panel");
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old.gameObject);
                else DestroyImmediate(old.gameObject);
            }
        }

        void EnsureLabel()
        {
            if (label != null) return;

            var labelGO = transform.Find("Label") ? transform.Find("Label").gameObject : null;
            if (!labelGO)
            {
                labelGO = new GameObject("Label", typeof(RectTransform));
                labelGO.transform.SetParent(transform, false);
            }

            label = labelGO.GetComponent<Text>();
            if (!label) label = labelGO.AddComponent<Text>();

            // Assign font safely
            if (!font)
            {
                // Use built-in Arial so we always render text even if no font set in inspector
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            label.font      = font;
            label.fontSize  = fontSize;
            label.color     = textColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow   = VerticalWrapMode.Overflow;
        }

        void LayoutLabel()
        {
            if (!label) return;
            var lrt = label.rectTransform;
            // Stretch horizontally across the screen, sit at the very top with padding
            lrt.anchorMin = new Vector2(0f, 1f);
            lrt.anchorMax = new Vector2(1f, 1f);
            lrt.pivot     = new Vector2(0.5f, 1f);
            lrt.sizeDelta = new Vector2(0f, Mathf.Max(48, fontSize + 12));
            lrt.anchoredPosition = new Vector2(0f, -topPadding);
        }

        void SetText(string s)
        {
            EnsureLabel();
            if (label) label.text = s ?? "";
        }
    }
}
