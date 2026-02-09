using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Util;

namespace Game.UI
{
    /// <summary>
    /// Adds UI buttons to the map scene (Inventory, etc.)
    /// Attach to any GameObject in MapScene or let MapController create it.
    /// </summary>
    public class MapUIButtons : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool createOnAwake = true;

        private Canvas canvas;
        private Button inventoryButton;

        void Awake()
        {
            if (createOnAwake)
            {
                CreateButtons();
            }
        }

        public void CreateButtons()
        {
            // Find or create canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("MapUICanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // On top of map
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            CreateInventoryButton();
        }

        private void CreateInventoryButton()
        {
            // Create button container
            var buttonGo = new GameObject("InventoryButton");
            buttonGo.transform.SetParent(canvas.transform, false);

            // Position at top-right, below any exit button
            var rt = buttonGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -70); // Below typical exit button position
            rt.sizeDelta = new Vector2(120, 40);

            // Add Image background
            var img = buttonGo.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);

            // Add Button component
            inventoryButton = buttonGo.AddComponent<Button>();
            inventoryButton.onClick.AddListener(OnInventoryClicked);

            // Add hover color
            var colors = inventoryButton.colors;
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.6f, 1f);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.4f, 1f);
            inventoryButton.colors = colors;

            // Add text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "Inventory (C)";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            Debug.Log("[MapUIButtons] Inventory button created");
        }

        private void OnInventoryClicked()
        {
            Debug.Log("[MapUIButtons] Inventory button clicked");

            // Find MapController and call its OpenCharacterMenu method
            var mapController = FindObjectOfType<MapController>();
            if (mapController != null)
            {
                mapController.OpenCharacterMenuPublic();
            }
            else
            {
                // Fallback: just go directly
                SceneRouter.GoToCharacterMenu("MapScene");
            }
        }

        void Update()
        {
            // Also listen for 'I' key as alternative to 'C'
            if (Input.GetKeyDown(KeyCode.I))
            {
                OnInventoryClicked();
            }
        }
    }
}
