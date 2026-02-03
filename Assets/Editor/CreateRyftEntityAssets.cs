using UnityEngine;
using UnityEditor;
using Game.RyftEntities;
using Game.Ryfts;

/// <summary>
/// Editor utility to create RyftEntityDef assets and prefabs for each Ryft color.
/// Use: Menu -> Ryft -> Create Ryft Entity Assets
/// </summary>
public class CreateRyftEntityAssets : Editor
{
    [MenuItem("Ryft/Create Ryft Entity Assets")]
    public static void CreateAssets()
    {
        // Ensure directories exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder("Assets/Resources/RyftEntities"))
            AssetDatabase.CreateFolder("Assets/Resources", "RyftEntities");

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/RyftEntities"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "RyftEntities");

        // Create assets for each color
        CreateRyftAsset(RyftColor.Orange, typeof(OrangeRyft));
        CreateRyftAsset(RyftColor.Green, typeof(GreenRyft));
        CreateRyftAsset(RyftColor.Blue, typeof(BlueRyft));
        CreateRyftAsset(RyftColor.Purple, typeof(PurpleRyft));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CreateRyftEntityAssets] Created all Ryft entity assets and prefabs!");
    }

    private static void CreateRyftAsset(RyftColor color, System.Type ryftComponentType)
    {
        string colorName = color.ToString();

        // Create the prefab first
        string prefabPath = $"Assets/Prefabs/RyftEntities/{colorName}Ryft.prefab";

        GameObject prefabGO;
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (existingPrefab != null)
        {
            Debug.Log($"[CreateRyftEntityAssets] Prefab already exists: {prefabPath}");
            prefabGO = existingPrefab;
        }
        else
        {
            // Create a new GameObject for the prefab
            var tempGO = new GameObject($"{colorName}Ryft");

            // Add SpriteRenderer
            var sr = tempGO.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            // Add the correct Ryft component
            tempGO.AddComponent(ryftComponentType);

            // Save as prefab
            prefabGO = PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);
            DestroyImmediate(tempGO);

            Debug.Log($"[CreateRyftEntityAssets] Created prefab: {prefabPath}");
        }

        // Create the ScriptableObject asset
        string assetPath = $"Assets/Resources/RyftEntities/{colorName}Ryft.asset";

        RyftEntityDef existingDef = AssetDatabase.LoadAssetAtPath<RyftEntityDef>(assetPath);

        if (existingDef != null)
        {
            Debug.Log($"[CreateRyftEntityAssets] Asset already exists: {assetPath}");
            // Update the prefab reference if needed
            if (existingDef.prefab == null && prefabGO != null)
            {
                existingDef.prefab = prefabGO;
                EditorUtility.SetDirty(existingDef);
            }
            return;
        }

        // Create new ScriptableObject
        var def = ScriptableObject.CreateInstance<RyftEntityDef>();
        def.ryftColor = color;
        def.displayName = $"{colorName} Rift Portal";
        def.maxHealth = 50;
        def.prefab = prefabGO;

        AssetDatabase.CreateAsset(def, assetPath);
        Debug.Log($"[CreateRyftEntityAssets] Created asset: {assetPath}");
    }

    [MenuItem("Ryft/Create Ryft Entity Assets", true)]
    private static bool ValidateCreateAssets()
    {
        return !Application.isPlaying;
    }
}
