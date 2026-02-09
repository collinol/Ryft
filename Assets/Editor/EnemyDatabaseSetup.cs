using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Game.Enemies;
using Game.Core;
using Game.Abilities;

/// <summary>
/// Editor tool to create and manage the EnemyDatabase asset and EnemyDef assets.
/// Use via menu: Tools > Ryft > Enemies
/// </summary>
public class EnemyDatabaseSetup : EditorWindow
{
    private const string DatabasePath = "Assets/Resources/databases/EnemyDatabase.asset";
    private const string EnemyFolder  = "Assets/Resources/Enemies";

    // ───────────── Database management ─────────────

    [MenuItem("Tools/Ryft/Enemies/Create Enemy Database")]
    public static void CreateEnemyDatabase()
    {
        var existing = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(DatabasePath);
        if (existing != null)
        {
            Debug.Log($"[EnemySetup] EnemyDatabase already exists at {DatabasePath}");
            Selection.activeObject = existing;
            return;
        }

        var db = ScriptableObject.CreateInstance<EnemyDatabase>();
        System.IO.Directory.CreateDirectory(EnemyFolder);
        AssetDatabase.CreateAsset(db, DatabasePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EnemySetup] Created EnemyDatabase at {DatabasePath}");

        // Auto-create seed enemy defs if none exist
        EnsureSeedEnemyDefs();

        // Populate database
        PopulateDatabase(db);

        Selection.activeObject = db;
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("Tools/Ryft/Enemies/Refresh Enemy Database")]
    public static void RefreshEnemyDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError($"[EnemySetup] EnemyDatabase not found at {DatabasePath}. Create it first.");
            return;
        }

        PopulateDatabase(db);
    }

    private static void PopulateDatabase(EnemyDatabase db)
    {
        var guids = AssetDatabase.FindAssets("t:EnemyDef", new[] { EnemyFolder });
        var items = new List<EnemyDef>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<EnemyDef>(path);
            if (def != null)
            {
                items.Add(def);
                Debug.Log($"[EnemySetup] Found: {def.id} ({def.displayName})");
            }
        }

        items = items.OrderBy(e => e.tier).ThenBy(e => e.displayName).ToList();
        db.SetEnemies(items);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EnemySetup] Populated database with {items.Count} enemies");
    }

    // ───────────── Seed EnemyDef creation ─────────────

    [MenuItem("Tools/Ryft/Enemies/Create Seed Enemy Defs")]
    public static void EnsureSeedEnemyDefs()
    {
        System.IO.Directory.CreateDirectory(EnemyFolder);

        // Regular enemies
        CreateDefIfMissing("Enemy_Goblin", "Goblin", EnemyTier.Regular, 20, 2,
            new[] { "EnemyStrikeAbility" }, "", Color.green);

        CreateDefIfMissing("Enemy_Skeleton", "Skeleton", EnemyTier.Regular, 15, 3,
            new[] { "EnemyStrikeAbility" }, "", Color.gray);

        CreateDefIfMissing("Enemy_Slime", "Slime", EnemyTier.Regular, 25, 1,
            new[] { "SlimeAttackAbility" }, "", new Color(0.2f, 0.8f, 0.2f));

        CreateDefIfMissing("Enemy_Bandit", "Bandit", EnemyTier.Regular, 18, 4,
            new[] { "DaggerStrikeAbility", "StealGoldAbility" },
            "Game.Enemies.BanditEnemy", new Color(0.6f, 0.3f, 0.1f));

        CreateDefIfMissing("Enemy_Cultist", "Cultist", EnemyTier.Regular, 20, 2,
            new[] { "CurseAbility", "DarkRitualAbility", "EnemyStrikeAbility" },
            "Game.Enemies.CultistEnemy", new Color(0.5f, 0.1f, 0.5f));

        // Elite enemies
        CreateDefIfMissing("Enemy_OrcChieftain", "Orc Chieftain", EnemyTier.Elite, 80, 6,
            new[] { "WarCryAbility", "HeavySmashAbility", "RallyAbility", "EnemyStrikeAbility" },
            "Game.Enemies.Elite.OrcChieftainEnemy", new Color(0.3f, 0.5f, 0.1f));

        CreateDefIfMissing("Enemy_DarkKnight", "Dark Knight", EnemyTier.Elite, 100, 5,
            new[] { "ShieldBashAbility", "DarkSlashAbility", "FortifyAbility" },
            "Game.Enemies.Elite.DarkKnightEnemy", new Color(0.2f, 0.2f, 0.3f));

        CreateDefIfMissing("Enemy_Golem", "Stone Golem", EnemyTier.Elite, 120, 4,
            new[] { "SlamAbility", "RockArmorAbility", "EarthquakeAbility" },
            "Game.Enemies.Elite.GolemEnemy", new Color(0.5f, 0.5f, 0.5f));

        CreateDefIfMissing("Enemy_Necromancer", "Necromancer", EnemyTier.Elite, 60, 3,
            new[] { "SummonAbility", "DrainLifeAbility", "CurseAbility" },
            "Game.Enemies.Elite.NecromancerEnemy", new Color(0.3f, 0.1f, 0.3f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Wire up minion references (must happen after all defs exist)
        WireMinionReferences();
    }

    private static void CreateDefIfMissing(string assetName, string displayName,
        EnemyTier tier, int hp, int str, string[] abilities,
        string runtimeType, Color proceduralColor)
    {
        string path = $"{EnemyFolder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<EnemyDef>(path) != null)
        {
            Debug.Log($"[EnemySetup] {assetName} already exists, skipping");
            return;
        }

        var def = ScriptableObject.CreateInstance<EnemyDef>();
        def.id = assetName;
        def.displayName = displayName;
        def.tier = tier;
        def.baseStats = new Stats { maxHealth = hp, strength = str };
        def.abilityIds = abilities;
        def.runtimeTypeName = runtimeType;
        def.proceduralColor = proceduralColor;

        AssetDatabase.CreateAsset(def, path);
        Debug.Log($"[EnemySetup] Created {assetName} at {path}");
    }

    private static void WireMinionReferences()
    {
        // OrcChieftain -> 1 Goblin @ 50%
        var orcDef = AssetDatabase.LoadAssetAtPath<EnemyDef>($"{EnemyFolder}/Enemy_OrcChieftain.asset");
        var goblinDef = AssetDatabase.LoadAssetAtPath<EnemyDef>($"{EnemyFolder}/Enemy_Goblin.asset");
        if (orcDef != null && goblinDef != null && (orcDef.minions == null || orcDef.minions.Length == 0))
        {
            orcDef.minions = new[]
            {
                new MinionEntry { minionDef = goblinDef, count = 1, spawnChance = 0.5f }
            };
            EditorUtility.SetDirty(orcDef);
        }

        // Necromancer -> 2 Skeleton @ 100%
        var necroDef = AssetDatabase.LoadAssetAtPath<EnemyDef>($"{EnemyFolder}/Enemy_Necromancer.asset");
        var skelDef = AssetDatabase.LoadAssetAtPath<EnemyDef>($"{EnemyFolder}/Enemy_Skeleton.asset");
        if (necroDef != null && skelDef != null && (necroDef.minions == null || necroDef.minions.Length == 0))
        {
            necroDef.minions = new[]
            {
                new MinionEntry { minionDef = skelDef, count = 2, spawnChance = 1f }
            };
            EditorUtility.SetDirty(necroDef);
        }

        AssetDatabase.SaveAssets();
    }

    // ───────────── Enemy Ability Defs ─────────────

    private const string AbilityFolder = "Assets/Scripts/Resources/EnemyAbilities";
    private const string AbilityDbPath = "Assets/Scripts/Resources/EnemyAbilities/EnemyAbilityDatabase.asset";

    [MenuItem("Tools/Ryft/Enemies/Create Missing Enemy Ability Defs")]
    public static void CreateMissingAbilityDefs()
    {
        System.IO.Directory.CreateDirectory(AbilityFolder);

        // (id, displayName, targeting, power, scaling, cooldown, runtimeTypeName)
        var abilities = new[]
        {
            ("SlimeAttackAbility",    "Slime Attack",  TargetingType.SingleEnemy,  8,  1, 1, "Game.Abilities.EnemyAbilities.SlimeAttackAbility"),
            ("DaggerStrikeAbility",   "Dagger Strike", TargetingType.SingleEnemy, 10,  1, 1, "Game.Abilities.EnemyAbilities.DaggerStrikeAbility"),
            ("StealGoldAbility",      "Steal Gold",    TargetingType.SingleEnemy, 10,  1, 2, "Game.Abilities.EnemyAbilities.StealGoldAbility"),
            ("CurseAbility",          "Curse",         TargetingType.SingleEnemy, 10,  1, 2, "Game.Abilities.EnemyAbilities.CurseAbility"),
            ("DarkRitualAbility",     "Dark Ritual",   TargetingType.Self,        15,  1, 3, "Game.Abilities.EnemyAbilities.DarkRitualAbility"),
            ("WarCryAbility",         "War Cry",       TargetingType.Self,        10,  1, 3, "Game.Abilities.EnemyAbilities.WarCryAbility"),
            ("HeavySmashAbility",     "Heavy Smash",   TargetingType.SingleEnemy, 12,  1, 2, "Game.Abilities.EnemyAbilities.HeavySmashAbility"),
            ("RallyAbility",          "Rally",         TargetingType.Self,        10,  1, 3, "Game.Abilities.EnemyAbilities.RallyAbility"),
            ("ShieldBashAbility",     "Shield Bash",   TargetingType.SingleEnemy, 10,  1, 1, "Game.Abilities.EnemyAbilities.ShieldBashAbility"),
            ("DarkSlashAbility",      "Dark Slash",    TargetingType.SingleEnemy, 12,  1, 1, "Game.Abilities.EnemyAbilities.DarkSlashAbility"),
            ("FortifyAbility",        "Fortify",       TargetingType.Self,        10,  1, 3, "Game.Abilities.EnemyAbilities.FortifyAbility"),
            ("SlamAbility",           "Slam",          TargetingType.SingleEnemy, 15,  1, 1, "Game.Abilities.EnemyAbilities.SlamAbility"),
            ("RockArmorAbility",      "Rock Armor",    TargetingType.Self,        10,  1, 3, "Game.Abilities.EnemyAbilities.RockArmorAbility"),
            ("EarthquakeAbility",     "Earthquake",    TargetingType.AllEnemies,  12,  1, 3, "Game.Abilities.EnemyAbilities.EarthquakeAbility"),
            ("SummonAbility",         "Summon",        TargetingType.None,        10,  1, 3, "Game.Abilities.EnemyAbilities.SummonAbility"),
            ("DrainLifeAbility",      "Drain Life",    TargetingType.SingleEnemy, 10,  1, 2, "Game.Abilities.EnemyAbilities.DrainLifeAbility"),
        };

        int created = 0;
        foreach (var (id, displayName, targeting, power, scaling, cooldown, runtimeType) in abilities)
        {
            string path = $"{AbilityFolder}/{id.Replace("Ability", "")}.asset";
            if (AssetDatabase.LoadAssetAtPath<AbilityDef>(path) != null)
            {
                Debug.Log($"[EnemySetup] Ability {id} already exists at {path}, skipping");
                continue;
            }

            var def = ScriptableObject.CreateInstance<AbilityDef>();
            def.id = id;
            def.displayName = displayName;
            def.targeting = targeting;
            def.power = power;
            def.scaling = scaling;
            def.baseCooldown = cooldown;
            def.runtimeTypeName = runtimeType;

            AssetDatabase.CreateAsset(def, path);
            Debug.Log($"[EnemySetup] Created ability def: {id} at {path}");
            created++;
        }

        AssetDatabase.SaveAssets();

        // Now refresh the EnemyAbilityDatabase
        RefreshAbilityDatabase();

        Debug.Log($"[EnemySetup] Created {created} new ability defs");
    }

    private static void RefreshAbilityDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<Game.Abilities.Enemy.EnemyAbilityDatabase>(AbilityDbPath);
        if (db == null)
        {
            Debug.LogError($"[EnemySetup] EnemyAbilityDatabase not found at {AbilityDbPath}");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:AbilityDef", new[] { AbilityFolder });
        var items = new List<AbilityDef>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<AbilityDef>(path);
            if (def != null)
            {
                items.Add(def);
                Debug.Log($"[EnemySetup] Found ability: {def.id} ({def.displayName})");
            }
        }

        items = items.OrderBy(a => a.id).ToList();
        db.SetAbilities(items);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EnemySetup] EnemyAbilityDatabase now has {items.Count} abilities");
    }

    // ───────────── Info ─────────────

    [MenuItem("Tools/Ryft/Enemies/List All Enemies")]
    public static void ListAllEnemies()
    {
        var db = EnemyDatabase.Load();
        if (db == null)
        {
            Debug.LogError("EnemyDatabase not found! Create it first with Tools > Ryft > Enemies > Create Enemy Database");
            return;
        }

        Debug.Log($"=== Enemy Database ({db.Count} enemies) ===");
        foreach (var e in db.All)
        {
            if (e == null) continue;
            string minionInfo = "";
            if (e.minions != null && e.minions.Length > 0)
            {
                var parts = new List<string>();
                foreach (var m in e.minions)
                    if (m.minionDef != null)
                        parts.Add($"{m.count}x {m.minionDef.displayName} @{m.spawnChance:P0}");
                if (parts.Count > 0) minionInfo = $" Minions: [{string.Join(", ", parts)}]";
            }
            Debug.Log($"  [{e.tier}] {e.id}: {e.displayName} HP={e.baseStats.maxHealth} STR={e.baseStats.strength}{minionInfo}");
        }
    }
}
