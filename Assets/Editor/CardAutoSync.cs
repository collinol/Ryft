// Assets/Editor/CardAutoSync.cs
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Cards; // CardDef, CardDatabase

public static class CardAutoSync
{
    private const string ResourcesCardsFolder = "Assets/Resources/Cards";
    private static readonly string[] DbCandidateNames = { "CardDatabase.asset", "Card Database.asset" };

    [MenuItem("Tools/Cards/Sync Card Assets + Database")]
    public static void Sync()
    {
        EnsureFolder(ResourcesCardsFolder);

        // 1) Find/ensure CardDatabase asset
        var db = FindOrCreateDatabase();
        if (!db)
        {
            Debug.LogError("Failed to locate or create CardDatabase.");
            return;
        }

        // 2) Find all runtime card types in Game.Cards without relying on a name suffix
        var cardTypes = FindRuntimeCardTypes();

        // 3) Ensure a CardDef asset exists for each type
        var ensuredDefs = new List<CardDef>();
        foreach (var t in cardTypes)
        {
            var def = FindExistingDefAsset(t) ?? CreateDefAsset(t);
            if (def) ensuredDefs.Add(def);
        }

        // 4) Add new defs to CardDatabase (catalog + availability)
        AddToDatabase(db, ensuredDefs);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CardAutoSync] Sync complete. Types scanned: {cardTypes.Count}, defs ensured: {ensuredDefs.Count}");
    }

    // ---------------- helpers ----------------

    private static void EnsureFolder(string folder)
    {
        var parts = folder.Split('/');
        var path = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = $"{path}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(path, parts[i]);
            path = next;
        }
    }

    private static CardDatabase FindOrCreateDatabase()
    {
        foreach (var name in DbCandidateNames)
        {
            var p = $"{ResourcesCardsFolder}/{name}";
            var db = AssetDatabase.LoadAssetAtPath<CardDatabase>(p);
            if (db) return db;
        }

        var created = ScriptableObject.CreateInstance<CardDatabase>();
        var createdPath = $"{ResourcesCardsFolder}/{DbCandidateNames[0]}";
        AssetDatabase.CreateAsset(created, createdPath);
        Debug.Log($"[CardAutoSync] Created CardDatabase at {createdPath}");
        return created;
    }

    /// <summary>
    /// Finds concrete runtime card classes by:
    /// - Namespace == "Game.Cards"
    /// - Not a ScriptableObject or MonoBehaviour
    /// - Not abstract
    /// - Has a method Execute(FightContext, IActor)   <-- avoids name-suffix heuristics
    /// </summary>
    private static List<Type> FindRuntimeCardTypes()
    {
        var asms = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("Assembly-CSharp", StringComparison.Ordinal))
            .ToArray();

        var list = new List<Type>();
        foreach (var asm in asms)
        {
            Type fightCtx = null;
            Type iActor   = null;

            try
            {
                // Resolve parameter types by name to avoid hard references if namespaces change
                fightCtx = asm.GetTypes().FirstOrDefault(t => t.Name == "FightContext");
                iActor   = asm.GetTypes().FirstOrDefault(t => t.Name == "IActor");
            }
            catch { /* ignore */ }

            foreach (var t in asm.GetTypes())
            {
                if (t == null || !t.IsClass || t.IsAbstract) continue;
                if (t.Namespace != "Game.Cards") continue;
                if (typeof(ScriptableObject).IsAssignableFrom(t)) continue;
                if (typeof(MonoBehaviour).IsAssignableFrom(t)) continue;
                if (t == typeof(CardDef) || t == typeof(CardDatabase)) continue;

                // Must expose Execute(FightContext, IActor) like your runtime cards
                if (!HasExecuteSignature(t, fightCtx, iActor)) continue;

                list.Add(t);
            }
        }

        return list.Distinct().OrderBy(t => t.FullName).ToList();
    }

    private static bool HasExecuteSignature(Type type, Type fightCtx, Type iActor)
    {
        // if we couldn't resolve types, fall back to name-based matching on parameters
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var m in methods)
        {
            if (m.Name != "Execute") continue;
            var ps = m.GetParameters();
            if (ps.Length != 2) continue;

            // Prefer strong match when we have the types
            if (fightCtx != null && iActor != null)
            {
                if (ps[0].ParameterType == fightCtx && ps[1].ParameterType == iActor)
                    return true;
            }
            else
            {
                // Fallback: compare by simple names to remain resilient
                if (ps[0].ParameterType.Name == "FightContext" && ps[1].ParameterType.Name == "IActor")
                    return true;
            }
        }
        return false;
    }

    private static CardDef FindExistingDefAsset(Type cardType)
    {
        var path = $"{ResourcesCardsFolder}/{cardType.Name}.asset";
        var def = AssetDatabase.LoadAssetAtPath<CardDef>(path);
        if (def)
        {
            var expected = cardType.FullName; // e.g., "Game.Cards.Meteor"
            if (def.runtimeTypeName != expected)
            {
                def.runtimeTypeName = expected;
                EditorUtility.SetDirty(def);
            }
        }
        return def;
    }

    private static CardDef CreateDefAsset(Type cardType)
    {
        var def = ScriptableObject.CreateInstance<CardDef>();

        def.name            = cardType.Name;
        def.id              = cardType.Name;                // keep stable id == class name
        def.displayName     = SplitPascalCase(cardType.Name); // no "Card" stripping anymore
        def.runtimeTypeName = cardType.FullName;              // e.g., "Game.Cards.Meteor"

        // sensible defaults; change as desired
        def.targeting  = TargetingType.SingleEnemy;
        def.energyCost = 1;
        def.power      = 5;
        def.scaling    = 1;
        def.rarity     = CardRarity.Common;

        var path = $"{ResourcesCardsFolder}/{cardType.Name}.asset";
        AssetDatabase.CreateAsset(def, path);
        Debug.Log($"[CardAutoSync] Created CardDef: {path}");
        return def;
    }

    private static string SplitPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var chars = new List<char>(s.Length * 2);
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(s[i - 1]) || (i + 1 < s.Length && char.IsLower(s[i + 1]))))
                chars.Add(' ');
            chars.Add(c);
        }
        return new string(chars.ToArray());
    }

    private static void AddToDatabase(CardDatabase db, List<CardDef> defs)
    {
        if (defs == null || defs.Count == 0) return;

        var so = new SerializedObject(db);
        var catalogProp      = so.FindProperty("catalog");
        var availabilityProp = so.FindProperty("availability");

        var existingCatalog = new HashSet<CardDef>();
        for (int i = 0; i < catalogProp.arraySize; i++)
        {
            var elem = catalogProp.GetArrayElementAtIndex(i);
            var refObj = elem.objectReferenceValue as CardDef;
            if (refObj) existingCatalog.Add(refObj);
        }

        var existingAvail = new HashSet<CardDef>();
        for (int i = 0; i < availabilityProp.arraySize; i++)
        {
            var elem = availabilityProp.GetArrayElementAtIndex(i);
            var cardRef = elem.FindPropertyRelative("card").objectReferenceValue as CardDef;
            if (cardRef) existingAvail.Add(cardRef);
        }

        foreach (var def in defs)
        {
            if (!def) continue;

            if (!existingCatalog.Contains(def))
            {
                catalogProp.InsertArrayElementAtIndex(catalogProp.arraySize);
                catalogProp.GetArrayElementAtIndex(catalogProp.arraySize - 1).objectReferenceValue = def;
                existingCatalog.Add(def);
            }

            if (!existingAvail.Contains(def))
            {
                availabilityProp.InsertArrayElementAtIndex(availabilityProp.arraySize);
                var newElem = availabilityProp.GetArrayElementAtIndex(availabilityProp.arraySize - 1);
                newElem.FindPropertyRelative("card").objectReferenceValue = def;
                newElem.FindPropertyRelative("available").intValue = 0;     // start locked
                newElem.FindPropertyRelative("maxCopies").intValue = 99;    // or your cap
                newElem.FindPropertyRelative("rewardEligible").boolValue = true;
                existingAvail.Add(def);
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
    }
}
#endif
