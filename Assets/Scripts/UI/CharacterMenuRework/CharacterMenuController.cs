using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Util;

namespace Game.UI.Inventory
{
    /// <summary>
    /// Handles global input for the CharacterMenu scene, including returning to the previous scene with Escape.
    /// Also creates a Back button and title.
    /// </summary>
    public class CharacterMenuController : MonoBehaviour
    {
        private Button exitButton;

        void Awake()
        {
            Debug.Log("[CharacterMenuController] Awake() called");

            // Ensure EventSystem exists FIRST for button clicks
            EnsureEventSystem();
        }

        void Start()
        {
            Debug.Log("[CharacterMenuController] Start() called");

            // Ensure equipment UI setup exists
            EnsureEquipmentUISetup();

            // Create UI with exit button
            CreateUI();
        }

        void EnsureEventSystem()
        {
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[CharacterMenuController] Created EventSystem");
            }
            else
            {
                Debug.Log("[CharacterMenuController] EventSystem already exists");
            }
        }

        void EnsureEquipmentUISetup()
        {
            // Check if EquipmentUIController exists
            var existingUI = FindObjectOfType<EquipmentUIController>();
            if (existingUI != null)
            {
                Debug.Log("[CharacterMenu] EquipmentUIController found, refreshing...");
                existingUI.RefreshFromManager();
                return;
            }

            // Check if CharacterMenuSetup exists
            var existingSetup = FindObjectOfType<CharacterMenuSetup>();
            if (existingSetup != null)
            {
                Debug.Log("[CharacterMenu] CharacterMenuSetup found, it will handle UI creation");
                return;
            }

            // Create CharacterMenuSetup to set up the UI
            Debug.Log("[CharacterMenu] Creating CharacterMenuSetup...");
            var setupGo = new GameObject("CharacterMenuSetup");
            setupGo.AddComponent<CharacterMenuSetup>();
        }

        void Update()
        {
            // Press Escape or I to return to the previous scene (likely MapScene)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[CharacterMenuController] Escape pressed");
                ReturnToPreviousScene();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("[CharacterMenuController] I pressed");
                ReturnToPreviousScene();
            }
        }

        private Canvas overlayCanvas; // High-priority canvas for title, exit button, instructions

        void CreateUI()
        {
            // Create a HIGH priority overlay canvas for our UI elements
            // This ensures they render on top of the equipment grids
            var overlayGo = new GameObject("CharacterMenuOverlay");
            overlayCanvas = overlayGo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 200; // Higher than equipment UI
            var scaler = overlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            overlayGo.AddComponent<GraphicRaycaster>();
            Debug.Log("[CharacterMenuController] Created overlay canvas with sortingOrder=200");

            // Create title
            CreateTitle();

            // Create prominent EXIT button at top-right (like fight scene)
            CreateExitButton();

            // Create instructions text
            CreateInstructions();

            Debug.Log("[CharacterMenuController] UI created successfully");
        }

        void CreateTitle()
        {
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(overlayCanvas.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0, -20);
            titleRt.sizeDelta = new Vector2(400, 50);

            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "INVENTORY & EQUIPMENT";
            titleText.fontSize = 32;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;
        }

        void CreateExitButton()
        {
            // Create EXIT button at top-right corner (similar to FightExitButton)
            var buttonGo = new GameObject("ExitButton");
            buttonGo.transform.SetParent(overlayCanvas.transform, false);

            // Position at top-right
            var rt = buttonGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(120, 50);

            // Background image - make it red/prominent so it's obvious
            var img = buttonGo.AddComponent<Image>();
            img.color = new Color(0.7f, 0.2f, 0.2f, 1f); // Red-ish color
            img.raycastTarget = true;

            // Button component
            exitButton = buttonGo.AddComponent<Button>();
            exitButton.targetGraphic = img;
            exitButton.onClick.AddListener(OnExitButtonClicked);

            // Button colors
            var colors = exitButton.colors;
            colors.normalColor = new Color(0.7f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
            colors.selectedColor = new Color(0.7f, 0.2f, 0.2f, 1f);
            exitButton.colors = colors;

            // Button text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "EXIT";
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false; // Don't block button clicks

            Debug.Log("[CharacterMenuController] Exit button created at top-right");
        }

        void CreateInstructions()
        {
            var instructGo = new GameObject("Instructions");
            instructGo.transform.SetParent(overlayCanvas.transform, false);
            var instructRt = instructGo.AddComponent<RectTransform>();
            instructRt.anchorMin = new Vector2(0.5f, 0f);
            instructRt.anchorMax = new Vector2(0.5f, 0f);
            instructRt.pivot = new Vector2(0.5f, 0f);
            instructRt.anchoredPosition = new Vector2(0, 20);
            instructRt.sizeDelta = new Vector2(600, 40);

            var instructText = instructGo.AddComponent<TextMeshProUGUI>();
            instructText.text = "Press ESC or I to exit | Click items to equip";
            instructText.fontSize = 18;
            instructText.alignment = TextAlignmentOptions.Center;
            instructText.color = new Color(0.7f, 0.7f, 0.7f);
        }

        void OnExitButtonClicked()
        {
            Debug.Log("[CharacterMenuController] Exit button CLICKED!");
            ReturnToPreviousScene();
        }

        void ReturnToPreviousScene()
        {
            Debug.Log("[CharacterMenuController] ReturnToPreviousScene called");

            // Try SceneRouter first
            if (!string.IsNullOrEmpty(GetReturnScene()))
            {
                Debug.Log("[CharacterMenuController] Using SceneRouter.ReturnToPreviousScene()");
                SceneRouter.ReturnToPreviousScene();
            }
            else
            {
                // Fallback: go directly to MapScene
                Debug.Log("[CharacterMenuController] No return scene set, loading MapScene directly");
                SceneManager.LoadScene("MapScene", LoadSceneMode.Single);
            }
        }

        string GetReturnScene()
        {
            // Use reflection to check SceneRouter's _returnScene field
            var field = typeof(SceneRouter).GetField("_returnScene",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (field != null)
            {
                return field.GetValue(null) as string;
            }
            return null;
        }
    }
}
