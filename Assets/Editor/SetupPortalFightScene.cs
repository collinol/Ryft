using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Game.Combat;

public class SetupPortalFightScene : EditorWindow
{
    [MenuItem("Tools/Setup/Configure PortalFight Scene")]
    public static void ConfigurePortalFightScene()
    {
        // Save current scene
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (!EditorUtility.DisplayDialog("Unsaved Changes",
                "You have unsaved changes. Do you want to continue?",
                "Yes", "Cancel"))
            {
                return;
            }
        }

        // Load PortalFight scene
        string scenePath = "Assets/Scenes/PortalFight.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (!scene.IsValid())
        {
            Debug.LogError($"Failed to load scene at {scenePath}");
            return;
        }

        Debug.Log("=== Configuring PortalFight Scene ===");

        // Find or create RuntimeEnemySpawner
        var spawner = FindObjectOfType<RuntimeEnemySpawner>();

        if (spawner == null)
        {
            // Create new GameObject with RuntimeEnemySpawner
            GameObject spawnerObj = new GameObject("RuntimeEnemySpawner");
            spawner = spawnerObj.AddComponent<RuntimeEnemySpawner>();
            Debug.Log("Created new RuntimeEnemySpawner GameObject");
        }
        else
        {
            Debug.Log("Found existing RuntimeEnemySpawner");
        }

        // Configure spawner for random 2-4 enemies
        SerializedObject so = new SerializedObject(spawner);

        so.FindProperty("numberOfEnemies").intValue = 2;
        so.FindProperty("randomizeCount").boolValue = true;
        so.FindProperty("minEnemies").intValue = 2;
        so.FindProperty("maxEnemies").intValue = 4;

        so.FindProperty("baseY").floatValue = 0.5f;  // Raised to 0.5 so enemies are visible above cards
        so.FindProperty("spacingX").floatValue = 2.5f;
        so.FindProperty("centerEnemies").boolValue = true;

        so.FindProperty("healthBarOffsetY").floatValue = 1.5f;

        so.ApplyModifiedProperties();

        Debug.Log("Configured RuntimeEnemySpawner:");
        Debug.Log("  - Random count: 2-4 enemies");
        Debug.Log("  - Centered positioning");
        Debug.Log("  - Spacing: 2.5 units");

        // Add RedOverlayDebugger to auto-fix red overlay issues
        var debugger = spawner.gameObject.GetComponent<Game.Combat.RedOverlayDebugger>();
        if (debugger == null)
        {
            debugger = spawner.gameObject.AddComponent<Game.Combat.RedOverlayDebugger>();
            Debug.Log("Added RedOverlayDebugger to auto-fix overlay issues");
        }

        // Remove old static goblins if they exist
        var existingGoblins = FindObjectsOfType<Game.Enemies.GoblinEnemy>();
        int removedCount = 0;
        foreach (var goblin in existingGoblins)
        {
            Debug.Log($"Removing static goblin: {goblin.gameObject.name}");
            DestroyImmediate(goblin.gameObject);
            removedCount++;
        }

        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} static goblin(s) - they will be spawned at runtime");
        }

        // Mark scene as dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("\n✓ PortalFight scene configured successfully!");
        Debug.Log("  - Random enemy spawning enabled (2-4 enemies)");
        Debug.Log("  - Old static enemies removed");
        Debug.Log("  - Scene saved");

        EditorUtility.DisplayDialog("Success",
            "PortalFight scene configured!\n\n" +
            "- RuntimeEnemySpawner added\n" +
            "- Will spawn 2-4 random enemies\n" +
            "- Old static enemies removed\n\n" +
            "Test by clicking a Rift node on the map!",
            "OK");
    }

    [MenuItem("Tools/Setup/Verify Scene Navigation")]
    public static void VerifySceneNavigation()
    {
        Debug.Log("=== Verifying Scene Navigation ===\n");

        // Check if scenes exist in build settings
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MapScene.unity",
            "Assets/Scenes/FightScene.unity",
            "Assets/Scenes/PortalFight.unity"
        };

        bool allScenesFound = true;
        var buildScenes = EditorBuildSettings.scenes;

        foreach (string scenePath in scenePaths)
        {
            bool inBuild = false;
            bool enabled = false;

            foreach (var buildScene in buildScenes)
            {
                if (buildScene.path == scenePath)
                {
                    inBuild = true;
                    enabled = buildScene.enabled;
                    break;
                }
            }

            if (inBuild && enabled)
            {
                Debug.Log($"✓ {System.IO.Path.GetFileNameWithoutExtension(scenePath)} - In build and enabled");
            }
            else if (inBuild && !enabled)
            {
                Debug.LogWarning($"⚠ {System.IO.Path.GetFileNameWithoutExtension(scenePath)} - In build but DISABLED");
                allScenesFound = false;
            }
            else
            {
                Debug.LogError($"✗ {System.IO.Path.GetFileNameWithoutExtension(scenePath)} - NOT in build settings!");
                allScenesFound = false;
            }
        }

        if (!allScenesFound)
        {
            Debug.LogWarning("\n⚠ Some scenes are missing from Build Settings!");
            Debug.LogWarning("Go to File > Build Settings and add the missing scenes.");

            if (EditorUtility.DisplayDialog("Missing Scenes",
                "Some scenes are not in Build Settings.\n\nWould you like to add them now?",
                "Yes", "No"))
            {
                AddScenesToBuildSettings();
            }
        }
        else
        {
            Debug.Log("\n✓ All scenes are properly configured in Build Settings!");
        }
    }

    private static void AddScenesToBuildSettings()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MapScene.unity",
            "Assets/Scenes/FightScene.unity",
            "Assets/Scenes/PortalFight.unity"
        };

        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (string scenePath in scenePaths)
        {
            bool exists = false;
            foreach (var scene in buildScenes)
            {
                if (scene.path == scenePath)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists && System.IO.File.Exists(scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Added {scenePath} to Build Settings");
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("✓ Build Settings updated!");
    }
}
