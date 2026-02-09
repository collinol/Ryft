using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Equipment;
using Game.Util;

namespace Game.UI.Inventory
{
    /// <summary>
    /// Runtime setup for CharacterMenuScene.
    /// Creates all necessary UI components if they don't exist.
    /// Attach this to a GameObject in the CharacterMenuScene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CharacterMenuSetup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool createUIOnStart = true;
        [SerializeField] private bool seedInventoryFromDatabase = true;

        private Canvas canvas;
        private Canvas overlayCanvas; // For exit button and title
        private EquipmentUIController uiController;

        void Awake()
        {
            Debug.Log("[CharacterMenuSetup] Awake called");

            // Ensure EquipmentManager exists
            if (EquipmentManager.Instance == null)
            {
                var mgrGo = new GameObject("EquipmentManager");
                mgrGo.AddComponent<EquipmentManager>();
                DontDestroyOnLoad(mgrGo);
            }

            // Ensure EventSystem exists for button clicks
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[CharacterMenuSetup] Created EventSystem");
            }
        }

        void Start()
        {
            Debug.Log("[CharacterMenuSetup] Start called");

            if (createUIOnStart)
            {
                SetupUI();
            }

            if (seedInventoryFromDatabase)
            {
                SeedInventory();
            }

            // ALWAYS create the exit button overlay (on top of everything)
            CreateExitButtonOverlay();
        }

        void SetupUI()
        {
            // Check if EquipmentUIController already exists
            uiController = FindObjectOfType<EquipmentUIController>();
            if (uiController != null)
            {
                Debug.Log("[CharacterMenuSetup] EquipmentUIController already exists, refreshing...");
                uiController.RefreshFromManager();
                return;
            }

            Debug.Log("[CharacterMenuSetup] Creating equipment UI from scratch...");

            // Find or create canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("CharacterMenuCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Create background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Create main container
            var containerGo = new GameObject("EquipmentContainer");
            containerGo.transform.SetParent(canvas.transform, false);
            var containerRt = containerGo.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.1f, 0.15f);
            containerRt.anchorMax = new Vector2(0.9f, 0.85f);
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;

            // Create horizontal layout for the two grids
            var hlg = containerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Create EquipmentUIController
            var controllerGo = new GameObject("EquipmentUIController");
            controllerGo.transform.SetParent(canvas.transform, false);
            uiController = controllerGo.AddComponent<EquipmentUIController>();

            // Create Character Grid (equipment slots)
            var charGridGo = CreateGrid(containerGo.transform, "CharacterGrid", true);
            var charGrid = charGridGo.GetComponent<EquipmentGridUI>();

            // Create Inventory Grid
            var invGridGo = CreateGrid(containerGo.transform, "InventoryGrid", false);
            var invGrid = invGridGo.GetComponent<EquipmentGridUI>();

            // Wire up the controller (using reflection since fields are serialized)
            SetPrivateField(uiController, "characterGrid", charGrid);
            SetPrivateField(uiController, "inventoryGrid", invGrid);
            SetPrivateField(uiController, "seedManagerFromDatabase", false); // We handle seeding separately

            // Build grids and populate
            charGrid.Build(null);
            invGrid.Build(null);

            // Refresh
            uiController.RefreshFromManager();

            Debug.Log("[CharacterMenuSetup] Equipment UI created successfully");
        }

        GameObject CreateGrid(Transform parent, string name, bool isCharacter)
        {
            var gridGo = new GameObject(name);
            gridGo.transform.SetParent(parent, false);

            var rt = gridGo.AddComponent<RectTransform>();
            rt.sizeDelta = isCharacter ? new Vector2(320, 400) : new Vector2(420, 400);

            // Add EquipmentGridUI component
            var grid = gridGo.AddComponent<EquipmentGridUI>();

            // Configure grid via reflection (since fields are serialized)
            SetPrivateField(grid, "isCharacterGrid", isCharacter);
            SetPrivateField(grid, "rows", isCharacter ? 5 : 4);
            SetPrivateField(grid, "cols", isCharacter ? 2 : 4);
            SetPrivateField(grid, "cellSize", 80);
            SetPrivateField(grid, "spacing", 8);

            // Set character slot mapping for character grid
            if (isCharacter)
            {
                var slots = new System.Collections.Generic.List<EquipmentSlot>
                {
                    EquipmentSlot.Head, EquipmentSlot.Shoulders,
                    EquipmentSlot.Chest, EquipmentSlot.Hands,
                    EquipmentSlot.Legs, EquipmentSlot.Feet,
                    EquipmentSlot.Weapon, EquipmentSlot.Offhand,
                    EquipmentSlot.Accessory1, EquipmentSlot.Accessory2
                };
                SetPrivateField(grid, "characterSlots", slots);
            }

            // Create header
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(gridGo.transform, false);
            var headerRt = headerGo.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 0);
            headerRt.anchoredPosition = new Vector2(0, 10);
            headerRt.sizeDelta = new Vector2(0, 30);

            var headerText = headerGo.AddComponent<TextMeshProUGUI>();
            headerText.text = isCharacter ? "EQUIPMENT" : "INVENTORY";
            headerText.fontSize = 24;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = Color.white;
            headerText.fontStyle = FontStyles.Bold;

            SetPrivateField(grid, "headerText", headerText);

            // Create grid layout container
            var layoutGo = new GameObject("GridLayout");
            layoutGo.transform.SetParent(gridGo.transform, false);
            var layoutRt = layoutGo.AddComponent<RectTransform>();
            layoutRt.anchorMin = Vector2.zero;
            layoutRt.anchorMax = Vector2.one;
            layoutRt.offsetMin = new Vector2(0, 0);
            layoutRt.offsetMax = new Vector2(0, -40); // Leave room for header

            var glg = layoutGo.AddComponent<GridLayoutGroup>();
            SetPrivateField(grid, "grid", glg);

            // Create cell prefab
            var cellPrefab = CreateCellPrefab();
            SetPrivateField(grid, "cellPrefab", cellPrefab);

            return gridGo;
        }

        EquipmentCellUI CreateCellPrefab()
        {
            var cellGo = new GameObject("CellPrefab");
            cellGo.SetActive(false); // Prefab shouldn't be visible

            var rt = cellGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(cellGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);

            // Item anchor
            var anchorGo = new GameObject("ItemAnchor");
            anchorGo.transform.SetParent(cellGo.transform, false);
            var anchorRt = anchorGo.AddComponent<RectTransform>();
            anchorRt.anchorMin = new Vector2(0.1f, 0.1f);
            anchorRt.anchorMax = new Vector2(0.9f, 0.9f);
            anchorRt.offsetMin = Vector2.zero;
            anchorRt.offsetMax = Vector2.zero;

            // Add cell component
            var cell = cellGo.AddComponent<EquipmentCellUI>();
            SetPrivateField(cell, "background", bgImg);
            SetPrivateField(cell, "itemAnchor", anchorRt);

            // Create item view prefab
            var itemPrefab = CreateItemViewPrefab();
            SetPrivateField(cell, "itemViewPrefab", itemPrefab);

            return cell;
        }

        EquipmentItemUI CreateItemViewPrefab()
        {
            var itemGo = new GameObject("ItemViewPrefab");
            itemGo.SetActive(false);

            var rt = itemGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Icon image
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(itemGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;

            var itemUI = itemGo.AddComponent<EquipmentItemUI>();
            SetPrivateField(itemUI, "icon", iconImg);

            return itemUI;
        }

        void SeedInventory()
        {
            var mgr = EquipmentManager.Instance;
            if (mgr == null) return;

            // Only seed if inventory is empty
            if (mgr.Inventory.Count > 0)
            {
                Debug.Log("[CharacterMenuSetup] Inventory already has items, skipping seed");
                return;
            }

            var db = EquipmentDatabase.Load();
            if (db == null)
            {
                Debug.LogWarning("[CharacterMenuSetup] EquipmentDatabase not found");
                return;
            }

            mgr.SeedFromDatabase(db, clear: false);
            Debug.Log($"[CharacterMenuSetup] Seeded inventory with {mgr.Inventory.Count} items");

            // Refresh UI if it exists
            if (uiController != null)
            {
                uiController.RefreshFromManager();
            }
        }

        // Helper to set private serialized fields
        void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[CharacterMenuSetup] Could not find field '{fieldName}' on {obj.GetType().Name}");
            }
        }

        /// <summary>
        /// Creates an overlay canvas with the exit button, title, and instructions.
        /// This is on a high sorting order so it appears on top of everything.
        /// </summary>
        void CreateExitButtonOverlay()
        {
            Debug.Log("[CharacterMenuSetup] Creating exit button overlay...");

            // Create overlay canvas with high sorting order
            var overlayGo = new GameObject("ExitButtonOverlay");
            overlayCanvas = overlayGo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 999; // Very high to be on top
            var scaler = overlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            overlayGo.AddComponent<GraphicRaycaster>();

            // Create title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(overlayCanvas.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0, -20);
            titleRt.sizeDelta = new Vector2(500, 50);

            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "INVENTORY & EQUIPMENT";
            titleText.fontSize = 36;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;

            // Create EXIT button at top-right
            var buttonGo = new GameObject("ExitButton");
            buttonGo.transform.SetParent(overlayCanvas.transform, false);

            var rt = buttonGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(140, 60);

            // Red background
            var img = buttonGo.AddComponent<Image>();
            img.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            img.raycastTarget = true;

            // Button component
            var btn = buttonGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(OnExitClicked);

            var colors = btn.colors;
            colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
            btn.colors = colors;

            // Button text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var btnText = textGo.AddComponent<TextMeshProUGUI>();
            btnText.text = "EXIT";
            btnText.fontSize = 28;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.raycastTarget = false;

            // Create instructions at bottom
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
            instructText.fontSize = 20;
            instructText.alignment = TextAlignmentOptions.Center;
            instructText.color = new Color(0.8f, 0.8f, 0.8f);

            Debug.Log("[CharacterMenuSetup] Exit button overlay created!");
        }

        void OnExitClicked()
        {
            Debug.Log("[CharacterMenuSetup] EXIT button clicked!");

            // Try to use SceneRouter if return scene is set
            try
            {
                SceneRouter.ReturnToPreviousScene();
            }
            catch
            {
                // Fallback: go directly to MapScene
                Debug.Log("[CharacterMenuSetup] Fallback: Loading MapScene directly");
                SceneManager.LoadScene("MapScene", LoadSceneMode.Single);
            }
        }

        void Update()
        {
            // Handle ESC and I keys to exit
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("[CharacterMenuSetup] Exit key pressed");
                OnExitClicked();
            }
        }
    }
}
