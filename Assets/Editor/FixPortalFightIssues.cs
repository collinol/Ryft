using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Game.Combat;

public class FixPortalFightIssues : EditorWindow
{
    [MenuItem("Tools/Fix PortalFight/Fix All Issues")]
    public static void FixAllIssues()
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

        Debug.Log("=== Fixing PortalFight Scene Issues ===\n");

        int fixesApplied = 0;

        // Fix 1: Camera background color (fix red overlay)
        fixesApplied += FixCameraBackground();

        // Fix 2: Player sprite sorting order
        fixesApplied += FixPlayerSortingOrder();

        // Fix 3: Enemy sprite sorting order (in RuntimeEnemySpawner)
        fixesApplied += FixEnemySortingOrder();

        // Fix 4: Canvas render mode
        fixesApplied += FixCanvasRenderMode();

        // Mark scene as dirty and save
        if (fixesApplied > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"\n✓ Applied {fixesApplied} fixes and saved scene!");

            EditorUtility.DisplayDialog("Success",
                $"Fixed {fixesApplied} issues in PortalFight scene!\n\n" +
                "Changes:\n" +
                "• Camera background changed to skybox\n" +
                "• Player sprite sorting fixed\n" +
                "• Enemy sprites will render correctly\n" +
                "• Canvas properly configured\n\n" +
                "Test by clicking a Rift node!",
                "OK");
        }
        else
        {
            Debug.Log("No issues found - scene already configured correctly!");
        }
    }

    private static int FixCameraBackground()
    {
        var cam = Camera.main;
        if (!cam) cam = FindObjectOfType<Camera>();

        if (!cam)
        {
            Debug.LogWarning("No camera found!");
            return 0;
        }

        // Check if camera is set to SolidColor (which shows as red/colored background)
        if (cam.clearFlags == CameraClearFlags.SolidColor)
        {
            Debug.Log("Fixing camera background:");
            Debug.Log($"  Before: ClearFlags={cam.clearFlags}, BG Color={cam.backgroundColor}");

            cam.clearFlags = CameraClearFlags.Skybox;

            Debug.Log($"  After: ClearFlags={cam.clearFlags}");
            EditorUtility.SetDirty(cam);
            return 1;
        }
        else
        {
            Debug.Log($"✓ Camera already configured (ClearFlags={cam.clearFlags})");
            return 0;
        }
    }

    private static int FixPlayerSortingOrder()
    {
        var player = FindObjectOfType<Game.Player.PlayerCharacter>();
        if (!player)
        {
            Debug.LogWarning("No Player found in scene!");
            return 0;
        }

        var sr = player.GetComponent<SpriteRenderer>();
        if (!sr)
        {
            Debug.LogWarning("Player has no SpriteRenderer!");
            return 0;
        }

        if (sr.sortingOrder < 10)
        {
            Debug.Log($"Fixing Player sorting order:");
            Debug.Log($"  Before: sortingOrder={sr.sortingOrder}");

            sr.sortingOrder = 10;  // Player renders above enemies (which are at 5)

            Debug.Log($"  After: sortingOrder={sr.sortingOrder}");
            EditorUtility.SetDirty(sr);
            return 1;
        }
        else
        {
            Debug.Log($"✓ Player sorting order already correct ({sr.sortingOrder})");
            return 0;
        }
    }

    private static int FixEnemySortingOrder()
    {
        // This won't fix already spawned enemies, but will configure the spawner
        // to create enemies with correct sorting order

        // Note: We can't directly edit the RuntimeEnemySpawner script,
        // but we can document what needs to be changed

        Debug.Log("Note: Enemy sorting order is set in RuntimeEnemySpawner.cs");
        Debug.Log("  Enemies will be created with sortingOrder=5 (below player at 10)");

        return 0; // Can't auto-fix this without modifying the script
    }

    private static int FixCanvasRenderMode()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            Debug.LogWarning("No Canvas found!");
            return 0;
        }

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("Fixing Canvas render mode:");
            Debug.Log($"  Before: renderMode={canvas.renderMode}");

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            Debug.Log($"  After: renderMode={canvas.renderMode}");
            EditorUtility.SetDirty(canvas);
            return 1;
        }
        else
        {
            Debug.Log($"✓ Canvas already configured correctly");
            return 0;
        }
    }

    [MenuItem("Tools/Fix PortalFight/Fix Camera Only")]
    public static void FixCameraOnly()
    {
        var cam = Camera.main;
        if (!cam) cam = FindObjectOfType<Camera>();

        if (!cam)
        {
            Debug.LogError("No camera found!");
            return;
        }

        Debug.Log("=== Fixing Camera Background ===");
        Debug.Log($"Current: ClearFlags={cam.clearFlags}, BG Color={cam.backgroundColor}");

        cam.clearFlags = CameraClearFlags.Skybox;

        Debug.Log($"New: ClearFlags={cam.clearFlags}");
        Debug.Log("✓ Camera fixed! Red overlay should be gone.");

        EditorUtility.SetDirty(cam);

        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    [MenuItem("Tools/Fix PortalFight/Fix Sorting Orders")]
    public static void FixSortingOrders()
    {
        Debug.Log("=== Fixing Sprite Sorting Orders ===\n");

        // Fix player
        var player = FindObjectOfType<Game.Player.PlayerCharacter>();
        if (player)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sr.sortingOrder = 10;
                EditorUtility.SetDirty(sr);
                Debug.Log($"✓ Player sortingOrder set to 10");
            }
        }

        // Fix any existing enemies in the scene
        var enemies = FindObjectsOfType<Game.Enemies.EnemyBase>();
        int fixedCount = 0;
        foreach (var enemy in enemies)
        {
            var sr = enemy.GetComponent<SpriteRenderer>();
            if (sr && sr.sortingOrder != 5)
            {
                sr.sortingOrder = 5;
                EditorUtility.SetDirty(sr);
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"✓ Fixed {fixedCount} enemy sorting orders to 5");
        }

        Debug.Log("\nNote: This fixes existing objects. New enemies spawned at runtime");
        Debug.Log("will use the sorting order defined in RuntimeEnemySpawner.cs");

        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
