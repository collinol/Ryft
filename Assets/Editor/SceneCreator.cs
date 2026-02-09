using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Editor tool to create the game scenes with required components.
/// Use via menu: Tools > Ryft > Create Scenes
/// </summary>
public class SceneCreator : EditorWindow
{
    [MenuItem("Tools/Ryft/Create Scenes/Create All Missing Scenes")]
    public static void CreateAllScenes()
    {
        CreateShopScene();
        CreateRestScene();
        CreateTimePortalScene();
        CreateRewardScene();

        EditorUtility.DisplayDialog("Scenes Created",
            "Created missing scenes:\n" +
            "- ShopScene\n" +
            "- RestScene\n" +
            "- TimePortalScene\n" +
            "- RewardScene\n\n" +
            "Remember to add them to Build Settings!",
            "OK");
    }

    [MenuItem("Tools/Ryft/Create Scenes/Shop Scene")]
    public static void CreateShopScene()
    {
        CreateScene("ShopScene", "Game.Shop.ShopSceneController");
    }

    [MenuItem("Tools/Ryft/Create Scenes/Rest Scene")]
    public static void CreateRestScene()
    {
        CreateScene("RestScene", "Game.Rest.RestSceneController");
    }

    [MenuItem("Tools/Ryft/Create Scenes/Time Portal Scene")]
    public static void CreateTimePortalScene()
    {
        CreateScene("TimePortalScene", "Game.TimePortal.TimePortalSceneController");
    }

    [MenuItem("Tools/Ryft/Create Scenes/Reward Scene")]
    public static void CreateRewardScene()
    {
        CreateScene("RewardScene", "Game.Rewards.RewardSceneController");
    }

    private static void CreateScene(string sceneName, string controllerTypeName)
    {
        string scenePath = $"Assets/Scenes/{sceneName}.unity";

        // Check if scene already exists
        if (System.IO.File.Exists(scenePath))
        {
            Debug.Log($"[SceneCreator] Scene already exists: {scenePath}");
            return;
        }

        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create main camera
        var cameraGo = new GameObject("Main Camera");
        var cam = cameraGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.tag = "MainCamera";

        // Create Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // Create EventSystem
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        // Create SceneController
        var controllerGo = new GameObject("SceneController");
        var controllerType = FindType(controllerTypeName);
        if (controllerType != null)
        {
            controllerGo.AddComponent(controllerType);
            Debug.Log($"[SceneCreator] Added {controllerTypeName} to scene");
        }
        else
        {
            Debug.LogWarning($"[SceneCreator] Could not find type: {controllerTypeName}");
        }

        // Save scene
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[SceneCreator] Created scene: {scenePath}");

        // Add to build settings
        AddSceneToBuildSettings(scenePath);
    }

    private static System.Type FindType(string typeName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null) return type;
        }
        return null;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Check if already in build settings
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath) return;
        }

        // Add to build settings
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[SceneCreator] Added to build settings: {scenePath}");
    }

    [MenuItem("Tools/Ryft/Create Scenes/Character Menu Scene")]
    public static void CreateCharacterMenuScene()
    {
        string scenePath = "Assets/Scenes/CharacterMenuScene.unity";

        // Check if scene already exists
        if (System.IO.File.Exists(scenePath))
        {
            Debug.Log($"[SceneCreator] Scene already exists: {scenePath}");
            return;
        }

        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create main camera
        var cameraGo = new GameObject("Main Camera");
        var cam = cameraGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.tag = "MainCamera";

        // Create Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // Create EventSystem
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        // Create CharacterMenuController
        var controllerGo = new GameObject("CharacterMenuController");
        var controllerType = FindType("Game.UI.Inventory.CharacterMenuController");
        if (controllerType != null)
        {
            controllerGo.AddComponent(controllerType);
            Debug.Log("[SceneCreator] Added CharacterMenuController to scene");
        }

        // Create CharacterMenuSetup
        var setupGo = new GameObject("CharacterMenuSetup");
        var setupType = FindType("Game.UI.Inventory.CharacterMenuSetup");
        if (setupType != null)
        {
            setupGo.AddComponent(setupType);
            Debug.Log("[SceneCreator] Added CharacterMenuSetup to scene");
        }

        // Save scene
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[SceneCreator] Created scene: {scenePath}");

        AddSceneToBuildSettings(scenePath);
    }

    [MenuItem("Tools/Ryft/Create Scenes/Add All Scenes to Build Settings")]
    public static void AddAllScenesToBuildSettings()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MapScene.unity",
            "Assets/Scenes/FightScene.unity",
            "Assets/Scenes/PortalFight.unity",
            "Assets/Scenes/ShopScene.unity",
            "Assets/Scenes/RestScene.unity",
            "Assets/Scenes/TimePortalScene.unity",
            "Assets/Scenes/RewardScene.unity",
            "Assets/Scenes/CharacterMenuScene.unity"
        };

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        foreach (var path in scenePaths)
        {
            if (System.IO.File.Exists(path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[SceneCreator] Added to build settings: {path}");
            }
            else
            {
                Debug.LogWarning($"[SceneCreator] Scene not found: {path}");
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[SceneCreator] Build settings updated with {scenes.Count} scenes");
    }

    [MenuItem("Tools/Ryft/Quick Scene Setup Guide")]
    public static void ShowSetupGuide()
    {
        EditorUtility.DisplayDialog("Scene Setup Guide",
            "QUICK SETUP:\n\n" +
            "1. Click 'Tools > Ryft > Create Scenes > Create All Missing Scenes'\n" +
            "   This creates ShopScene, RestScene, TimePortalScene, RewardScene\n\n" +
            "2. Click 'Tools > Ryft > Create Scenes > Add All Scenes to Build Settings'\n" +
            "   This adds all scenes to the build so they can be loaded\n\n" +
            "3. Start the game from MapScene and test each node type:\n" +
            "   - Enemy nodes -> FightScene -> RewardScene -> Map (advanced)\n" +
            "   - Shop nodes -> ShopScene\n" +
            "   - Rest nodes -> RestScene\n" +
            "   - TimePortal nodes -> TimePortalScene\n\n" +
            "SCENE CONTROLLERS (auto-created):\n" +
            "- ShopSceneController: Buy cards/equipment, repair\n" +
            "- RestSceneController: Heal 30% max HP\n" +
            "- TimePortalSceneController: Borrow gear with obligations\n" +
            "- RewardSceneController: Pick 1 of 3 cards (or equip for elites)",
            "Got it!");
    }
}
