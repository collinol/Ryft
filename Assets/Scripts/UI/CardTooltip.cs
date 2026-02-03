using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// Singleton tooltip that shows card information on hover
    /// </summary>
    public class CardTooltip : MonoBehaviour
    {
        public static CardTooltip Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private RectTransform tooltipRect;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(10f, 10f);
        [SerializeField] private float fadeSpeed = 10f;

        private bool isShowing = false;
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

        void Update()
        {
            if (isShowing)
            {
                // Follow mouse position
                Vector2 mousePos = Input.mousePosition;
                UpdatePosition(mousePos);

                // Fade in
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
            }
            else
            {
                // Fade out
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            }
        }

        private void UpdatePosition(Vector2 mousePos)
        {
            if (!tooltipRect || !canvas) return;

            // Convert mouse position to canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            // Apply offset
            localPoint += offset;

            // Keep tooltip on screen
            Vector2 sizeDelta = tooltipRect.sizeDelta;
            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRect.sizeDelta;

            // Clamp to canvas bounds
            float halfWidth = sizeDelta.x * 0.5f;
            float halfHeight = sizeDelta.y * 0.5f;

            localPoint.x = Mathf.Clamp(localPoint.x, -canvasSize.x * 0.5f + halfWidth, canvasSize.x * 0.5f - halfWidth);
            localPoint.y = Mathf.Clamp(localPoint.y, -canvasSize.y * 0.5f + halfHeight, canvasSize.y * 0.5f - halfHeight);

            tooltipRect.localPosition = localPoint;
        }

        public void Show(string title, string description, int energyCost, int power = 0, int scaling = 0)
        {
            if (titleText) titleText.text = title;
            if (descriptionText) descriptionText.text = description;

            // Build stats text
            string stats = $"Energy: {energyCost}";
            if (power > 0 || scaling > 0)
            {
                stats += $"\nPower: {power}";
                if (scaling > 0) stats += $" (+{scaling} per stat)";
            }
            if (statsText) statsText.text = stats;

            isShowing = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            isShowing = false;
            if (canvasGroup) canvasGroup.alpha = 0f;
            // Don't immediately disable - let it fade out
            Invoke(nameof(DelayedDisable), 0.2f);
        }

        private void DelayedDisable()
        {
            if (!isShowing)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Ensure a tooltip exists in the scene. Call this from your UI setup.
        /// </summary>
        public static CardTooltip Ensure(Canvas canvas)
        {
            if (Instance) return Instance;

            // Try to find existing
            var existing = FindObjectOfType<CardTooltip>();
            if (existing) return existing;

            if (canvas == null)
            {
                Debug.LogWarning("[CardTooltip] Cannot create tooltip - Canvas is null!");
                return null;
            }

            // Create new
            GameObject go = new GameObject("CardTooltip");

            // Add RectTransform first (required for UI)
            var rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(canvas.transform, false);

            Debug.Log($"[CardTooltip] Creating tooltip, canvas: {canvas.name}");

            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(300f, 150f);

            var tooltip = go.AddComponent<CardTooltip>();

            // Create background
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Create layout
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 5f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            try
            {
                // Create title text
                Debug.Log("[CardTooltip] Creating title text...");
                GameObject titleObj = new GameObject("Title");
                var titleRect = titleObj.AddComponent<RectTransform>();
                titleObj.transform.SetParent(go.transform, false);
                var titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.fontSize = 18;
                titleText.fontStyle = TMPro.FontStyles.Bold;
                titleText.color = Color.white;
                titleText.text = "Card Name";
                tooltip.titleText = titleText;
                Debug.Log("[CardTooltip] Title text created successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CardTooltip] Error creating title text: {e.Message}\n{e.StackTrace}");
            }

            // Create description text
            GameObject descObj = new GameObject("Description");
            var descRect = descObj.AddComponent<RectTransform>();
            descObj.transform.SetParent(go.transform, false);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.fontSize = 14;
            descText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            descText.text = "Card description";
            descText.enableWordWrapping = true;
            tooltip.descriptionText = descText;

            // Create stats text
            GameObject statsObj = new GameObject("Stats");
            var statsRect = statsObj.AddComponent<RectTransform>();
            statsObj.transform.SetParent(go.transform, false);
            var statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.fontSize = 12;
            statsText.color = new Color(0.7f, 0.9f, 1f, 1f);
            statsText.text = "Energy: 1";
            tooltip.statsText = statsText;

            tooltip.tooltipRect = rect;
            tooltip.Hide();

            return tooltip;
        }
    }
}
