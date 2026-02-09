using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Game.Equipment;

/// <summary>
/// Editor tool to create and manage the EquipmentDatabase asset.
/// Use via menu: Tools > Ryft > Equipment
/// </summary>
public class EquipmentDatabaseSetup : EditorWindow
{
    private const string DatabasePath = "Assets/Resources/databases/EquipmentDatabase.asset";
    private const string EquipmentFolder = "Assets/Resources/Equipment";

    [MenuItem("Tools/Ryft/Equipment/Create Equipment Database")]
    public static void CreateEquipmentDatabase()
    {
        // Check if it already exists
        var existing = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(DatabasePath);
        if (existing != null)
        {
            Debug.Log($"[EquipmentSetup] EquipmentDatabase already exists at {DatabasePath}");
            Selection.activeObject = existing;
            return;
        }

        // Create new database
        var db = ScriptableObject.CreateInstance<EquipmentDatabase>();

        // Ensure directory exists
        System.IO.Directory.CreateDirectory(EquipmentFolder);

        AssetDatabase.CreateAsset(db, DatabasePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EquipmentSetup] Created EquipmentDatabase at {DatabasePath}");

        // Auto-populate with existing equipment
        PopulateDatabase(db);

        Selection.activeObject = db;
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("Tools/Ryft/Equipment/Refresh Equipment Database")]
    public static void RefreshEquipmentDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError($"[EquipmentSetup] EquipmentDatabase not found at {DatabasePath}. Create it first.");
            return;
        }

        PopulateDatabase(db);
    }

    private static void PopulateDatabase(EquipmentDatabase db)
    {
        // Find all EquipmentDef assets in the Equipment folder
        var guids = AssetDatabase.FindAssets("t:EquipmentDef", new[] { EquipmentFolder });
        var items = new List<EquipmentDef>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<EquipmentDef>(path);
            if (def != null)
            {
                items.Add(def);
                Debug.Log($"[EquipmentSetup] Found: {def.id} ({def.displayName})");
            }
        }

        // Sort by slot, then by name
        items = items.OrderBy(e => e.slot).ThenBy(e => e.displayName).ToList();

        // Use SetItems method to update the database
        db.SetItems(items);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EquipmentSetup] Populated database with {items.Count} equipment items");
    }

    [MenuItem("Tools/Ryft/Equipment/Create Equipment Manager Prefab")]
    public static void CreateEquipmentManagerPrefab()
    {
        string prefabPath = "Assets/Prefabs/EquipmentManager.prefab";

        // Check if prefab already exists
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null)
        {
            Debug.Log($"[EquipmentSetup] EquipmentManager prefab already exists at {prefabPath}");
            Selection.activeObject = existing;
            return;
        }

        // Create new GameObject with EquipmentManager
        var go = new GameObject("EquipmentManager");
        go.AddComponent<EquipmentManager>();

        // Ensure Prefabs folder exists
        System.IO.Directory.CreateDirectory("Assets/Prefabs");

        // Save as prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log($"[EquipmentSetup] Created EquipmentManager prefab at {prefabPath}");
        Selection.activeObject = prefab;
    }

    [MenuItem("Tools/Ryft/Equipment/Add Equipment Manager to Scene")]
    public static void AddEquipmentManagerToScene()
    {
        // Check if already in scene
        var existing = Object.FindObjectOfType<EquipmentManager>();
        if (existing != null)
        {
            Debug.Log("[EquipmentSetup] EquipmentManager already exists in scene");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create new GameObject
        var go = new GameObject("EquipmentManager");
        go.AddComponent<EquipmentManager>();

        Debug.Log("[EquipmentSetup] Added EquipmentManager to scene");
        Selection.activeGameObject = go;
    }

    [MenuItem("Tools/Ryft/Equipment/Setup All (Database + Manager)")]
    public static void SetupAll()
    {
        CreateEquipmentDatabase();
        AddEquipmentManagerToScene();

        EditorUtility.DisplayDialog("Equipment Setup Complete",
            "Created/refreshed:\n" +
            "1. EquipmentDatabase asset (Resources/Equipment/)\n" +
            "2. EquipmentManager in current scene\n\n" +
            "The EquipmentManager uses DontDestroyOnLoad, so it persists across scenes.\n\n" +
            "To add equipment to player inventory for testing:\n" +
            "- Select EquipmentManager in Hierarchy\n" +
            "- Expand 'Inventory' in Inspector\n" +
            "- Add items directly to the list",
            "OK");
    }

    [MenuItem("Tools/Ryft/Equipment/List All Equipment")]
    public static void ListAllEquipment()
    {
        var db = EquipmentDatabase.Load();
        if (db == null)
        {
            Debug.LogError("EquipmentDatabase not found! Create it first with Tools > Ryft > Equipment > Create Equipment Database");
            return;
        }

        Debug.Log($"=== Equipment Database ({db.Count} items) ===");
        foreach (var item in db.All)
        {
            if (item == null) continue;
            Debug.Log($"  [{item.slot}] {item.id}: {item.displayName} (Level {item.level}, {item.rarity})");
        }
    }
}
