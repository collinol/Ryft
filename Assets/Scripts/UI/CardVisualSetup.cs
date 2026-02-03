using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// Sets up the visual layout for vertical card-style ability buttons
    /// Automatically configures icon, label, and cost display positions
    /// </summary>
    [ExecuteInEditMode]
    public class CardVisualSetup : MonoBehaviour
    {
        [Header("Card Layout Settings")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private CardLayoutStyle layoutStyle = CardLayoutStyle.Standard;

        [Header("Component References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text costText;

        [Header("Layout Dimensions")]
        [SerializeField] private float iconSize = 60f;
        [SerializeField] private float iconTopOffset = 10f;
        [SerializeField] private float labelBottomOffset = 10f;
        [SerializeField] private float costCornerOffset = 8f;

        void Start()
        {
            if (Application.isPlaying && autoSetup)
            {
                SetupCardLayout();
            }
        }

        [ContextMenu("Setup Card Layout")]
        public void SetupCardLayout()
        {
            // Find components if not assigned
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (labelText == null)
                labelText = transform.Find("Label")?.GetComponent<TMP_Text>();
            if (costText == null)
                costText = transform.Find("CooldownText")?.GetComponent<TMP_Text>();

            switch (layoutStyle)
            {
                case CardLayoutStyle.Standard:
                    SetupStandardLayout();
                    break;
                case CardLayoutStyle.IconTop:
                    SetupIconTopLayout();
                    break;
                case CardLayoutStyle.Compact:
                    SetupCompactLayout();
                    break;
            }

            Debug.Log($"[CardVisualSetup] Configured {layoutStyle} layout for {gameObject.name}");
        }

        private void SetupStandardLayout()
        {
            var cardRect = GetComponent<RectTransform>();
            if (!cardRect) return;

            // Icon at top center
            if (iconImage)
            {
                var iconRect = iconImage.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 1f);
                iconRect.anchorMax = new Vector2(0.5f, 1f);
                iconRect.pivot = new Vector2(0.5f, 1f);
                iconRect.anchoredPosition = new Vector2(0, -iconTopOffset);
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            // Label at bottom center
            if (labelText)
            {
                var labelRect = labelText.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0.4f); // Bottom 40% of card
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0, labelBottomOffset);
                labelRect.sizeDelta = Vector2.zero;

                labelText.alignment = TextAlignmentOptions.Center;
                labelText.enableWordWrapping = true;
            }

            // Cost at top-left corner
            if (costText)
            {
                var costRect = costText.GetComponent<RectTransform>();
                costRect.anchorMin = new Vector2(0f, 1f);
                costRect.anchorMax = new Vector2(0f, 1f);
                costRect.pivot = new Vector2(0f, 1f);
                costRect.anchoredPosition = new Vector2(costCornerOffset, -costCornerOffset);
                costRect.sizeDelta = new Vector2(30, 30);

                costText.alignment = TextAlignmentOptions.Center;
                costText.fontSize = 16;
                costText.fontStyle = FontStyles.Bold;
            }
        }

        private void SetupIconTopLayout()
        {
            // Icon takes up top 50% of card
            if (iconImage)
            {
                var iconRect = iconImage.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(1f, 1f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = Vector2.zero;
            }

            // Label takes up bottom 50%
            if (labelText)
            {
                var labelRect = labelText.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = Vector2.zero;
                labelRect.sizeDelta = Vector2.zero;

                labelText.alignment = TextAlignmentOptions.Center;
                labelText.enableWordWrapping = true;
            }

            // Cost in corner
            if (costText)
            {
                var costRect = costText.GetComponent<RectTransform>();
                costRect.anchorMin = new Vector2(0f, 1f);
                costRect.anchorMax = new Vector2(0f, 1f);
                costRect.pivot = new Vector2(0f, 1f);
                costRect.anchoredPosition = new Vector2(costCornerOffset, -costCornerOffset);
                costRect.sizeDelta = new Vector2(25, 25);
            }
        }

        private void SetupCompactLayout()
        {
            // Smaller icon
            if (iconImage)
            {
                var iconRect = iconImage.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 1f);
                iconRect.anchorMax = new Vector2(0.5f, 1f);
                iconRect.pivot = new Vector2(0.5f, 1f);
                iconRect.anchoredPosition = new Vector2(0, -5);
                iconRect.sizeDelta = new Vector2(45, 45);
            }

            // More space for label
            if (labelText)
            {
                var labelRect = labelText.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.05f, 0f);
                labelRect.anchorMax = new Vector2(0.95f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0, 5);
                labelRect.sizeDelta = Vector2.zero;

                labelText.alignment = TextAlignmentOptions.Center;
                labelText.enableWordWrapping = true;
                labelText.fontSize = 12;
            }

            // Smaller cost badge
            if (costText)
            {
                var costRect = costText.GetComponent<RectTransform>();
                costRect.anchorMin = new Vector2(0f, 1f);
                costRect.anchorMax = new Vector2(0f, 1f);
                costRect.pivot = new Vector2(0f, 1f);
                costRect.anchoredPosition = new Vector2(5, -5);
                costRect.sizeDelta = new Vector2(20, 20);

                costText.fontSize = 14;
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying && autoSetup)
            {
                // Auto-find components in editor
                if (iconImage == null)
                    iconImage = transform.Find("Icon")?.GetComponent<Image>();
                if (labelText == null)
                    labelText = transform.Find("Label")?.GetComponent<TMP_Text>();
                if (costText == null)
                    costText = transform.Find("CooldownText")?.GetComponent<TMP_Text>();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Apply to All Card Buttons")]
        private void ApplyToAllButtons()
        {
            var buttons = FindObjectsOfType<AbilityButton>();
            int count = 0;

            foreach (var btn in buttons)
            {
                var setup = btn.GetComponent<CardVisualSetup>();
                if (setup == null)
                {
                    setup = btn.gameObject.AddComponent<CardVisualSetup>();
                }
                setup.layoutStyle = this.layoutStyle;
                setup.SetupCardLayout();
                count++;
            }

            Debug.Log($"[CardVisualSetup] Applied layout to {count} buttons");
        }
#endif
    }

    public enum CardLayoutStyle
    {
        Standard,   // Icon top, label bottom, cost corner
        IconTop,    // Icon 50%, label 50%
        Compact     // Smaller elements, more compact
    }
}
