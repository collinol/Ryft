#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Ryfts;

[Serializable]
public class RyftEffectsJsonRoot { public List<RyftEffectJsonDef> effects = new(); }

[Serializable]
public class RyftEffectJsonDef
{
    public string id;
    public string displayName;
    public string description;

    public string color;     // Orange, Green, Blue, Yellow, Purple
    public string polarity;  // Positive, Negative

    public string lifetime;  // Permanent, UntilBattleEnd, DurationNTurns
    public int durationTurns;
    public int delayTurns;
    public int maxStacks;

    public float chancePercent;
    public int internalCooldownTurns;

    public int intMagnitude;
    public float floatMagnitude;

    public string builtIn;         // None, AddMaxHealth, ...
    public string runtimeTypeName; // optional; if empty => BuiltIn path
}

public static class RyftEffectsJsonImporter
{
    private const string DefaultJsonPath   = "Assets/Config/ryft_effects.json";
    private const string EffectsFolder     = "Assets/Resources/Ryfts";
    private const string DatabaseAssetPath = EffectsFolder + "/RyftEffectDatabase.asset";

    [MenuItem("Tools/Ryfts/Import Effects from JSON...")]
    public static void ImportFromJsonMenu()
    {
        string path = EditorUtility.OpenFilePanel("Select ryft_effects.json", "Assets", "json");
        if (string.IsNullOrEmpty(path)) return;
        ImportFromJson(path);
    }

    [MenuItem("Tools/Ryfts/Import Effects from Default JSON")]
    public static void ImportFromDefault()
    {
        ImportFromJson(DefaultJsonPath);
    }

    public static void ImportFromJson(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[RyftImporter] JSON not found: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var root = JsonUtility.FromJson<RyftEffectsJsonRoot>(json);
            if (root == null || root.effects == null)
            {
                Debug.LogError($"[RyftImporter] Bad JSON format: {path}");
                return;
            }

            EnsureFolders();

            // Create or load database
            var db = AssetDatabase.LoadAssetAtPath<RyftEffectDatabase>(DatabaseAssetPath);
            if (!db)
            {
                db = ScriptableObject.CreateInstance<RyftEffectDatabase>();
                AssetDatabase.CreateAsset(db, DatabaseAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[RyftImporter] Created database at {DatabaseAssetPath}");
            }

            var existingEffects = new List<RyftEffectDef>();
            // Import/update each effect as a ScriptableObject
            foreach (var j in root.effects)
            {
                if (string.IsNullOrWhiteSpace(j.id))
                {
                    Debug.LogWarning("[RyftImporter] Skipping effect with empty id.");
                    continue;
                }

                string assetPath = $"{EffectsFolder}/RyftEffect_{SanitizeFileName(j.id)}.asset";
                var def = AssetDatabase.LoadAssetAtPath<RyftEffectDef>(assetPath);
                if (!def)
                {
                    def = ScriptableObject.CreateInstance<RyftEffectDef>();
                    AssetDatabase.CreateAsset(def, assetPath);
                    bool created = AssetDatabase.GetAssetPath(def) == assetPath && AssetDatabase.Contains(def);
                    Debug.Log(created
                        ? $"[RyftImporter] Created effect asset: {assetPath}"
                        : $"[RyftImporter] Updated effect asset: {assetPath}");
                }

                ApplyJsonToDef(def, j);

                EditorUtility.SetDirty(def);
                existingEffects.Add(def);
            }

            // Update DB list (sorted by id for consistency)
            existingEffects = existingEffects.Where(x => x).OrderBy(x => x.id, StringComparer.OrdinalIgnoreCase).ToList();
            db.SetEffects(existingEffects);
            Debug.Log($"[RyftImporter] DB '{db.name}' now has {existingEffects.Count} effect(s):");
            for (int i = 0; i < existingEffects.Count; i++)
            {
                var e = existingEffects[i];
                Debug.Log($"  [{i}] id='{e.id}', name='{e.displayName}', color={e.color}, pol={e.polarity}, " +
                          $"chance={e.chancePercent}%, icd={e.internalCooldownTurns}, lifetime={e.lifetime}, " +
                          $"builtIn={e.builtIn}, runtime={(string.IsNullOrEmpty(e.runtimeTypeName) ? "(built-in)" : e.runtimeTypeName)}");
            }

            EditorUtility.SetDirty(db);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RyftImporter] Imported {existingEffects.Count} effect(s) from {path}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RyftImporter] Exception while importing {path}:\n{ex}");
        }
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(EffectsFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Ryfts");
    }

    private static string SanitizeFileName(string id)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            id = id.Replace(c, '_');
        return id;
    }

    private static void ApplyJsonToDef(RyftEffectDef def, RyftEffectJsonDef j)
    {
        def.id = j.id?.Trim();
        def.displayName = j.displayName ?? j.id;
        def.description = j.description ?? "";

        def.color    = ParseEnum<RyftColor>(j.color, RyftColor.Orange);
        def.polarity = ParseEnum<EffectPolarity>(j.polarity, EffectPolarity.Positive);

        def.lifetime      = ParseEnum<EffectLifetime>(j.lifetime, EffectLifetime.Permanent);
        def.durationTurns = Mathf.Max(0, j.durationTurns);
        def.delayTurns    = Mathf.Max(0, j.delayTurns);
        def.maxStacks     = Mathf.Max(1, j.maxStacks);

        def.chancePercent         = Mathf.Clamp(j.chancePercent, 0f, 100f);
        def.internalCooldownTurns = Mathf.Max(0, j.internalCooldownTurns);

        def.intMagnitude   = j.intMagnitude;
        def.floatMagnitude = j.floatMagnitude;

        def.builtIn = ParseEnum<BuiltInOp>(j.builtIn, BuiltInOp.None);

        // runtimeTypeName: keep as-is; CreateRuntime will try to instantiate later
        def.runtimeTypeName = string.IsNullOrWhiteSpace(j.runtimeTypeName) ? null : j.runtimeTypeName.Trim();
    }

    private static T ParseEnum<T>(string s, T fallback) where T : struct
    {
        if (!string.IsNullOrEmpty(s) && Enum.TryParse<T>(s, true, out var val))
            return val;
        return fallback;
    }
}
#endif
