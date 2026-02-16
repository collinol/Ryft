using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Game.Cards;

/// <summary>
/// Editor tool to create and manage the CardDatabase asset and CardDef assets.
/// Use via menu: Tools > Ryft > Cards
/// </summary>
public class CardDatabaseSetup : EditorWindow
{
    private const string DatabasePath = "Assets/Resources/databases/CardDatabase.asset";
    private const string CardFolder   = "Assets/Resources/Cards";

    // ───────────── Database management ─────────────

    [MenuItem("Tools/Ryft/Cards/Create Card Database")]
    public static void CreateCardDatabase()
    {
        var existing = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath);
        if (existing != null)
        {
            Debug.Log($"[CardSetup] CardDatabase already exists at {DatabasePath}");
            Selection.activeObject = existing;
            return;
        }

        System.IO.Directory.CreateDirectory("Assets/Resources/databases");
        var db = ScriptableObject.CreateInstance<CardDatabase>();
        AssetDatabase.CreateAsset(db, DatabasePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CardSetup] Created CardDatabase at {DatabasePath}");
        EnsureSeedCardDefs();
        PopulateDatabase(db);
        Selection.activeObject = db;
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("Tools/Ryft/Cards/Refresh Card Database")]
    public static void RefreshCardDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError($"[CardSetup] CardDatabase not found at {DatabasePath}. Create it first.");
            return;
        }
        PopulateDatabase(db);
    }

    private static void PopulateDatabase(CardDatabase db)
    {
        var guids = AssetDatabase.FindAssets("t:CardDef", new[] { CardFolder });
        var catalog = new List<CardDef>();
        var availability = new List<CardDatabase.Availability>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<CardDef>(path);
            if (def != null)
            {
                catalog.Add(def);
                Debug.Log($"[CardSetup] Found: {def.id} ({def.displayName})");
            }
        }

        catalog = catalog.OrderBy(c => c.displayName).ToList();

        // Use reflection to set private fields
        var catalogField = typeof(CardDatabase).GetField("catalog",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var availField = typeof(CardDatabase).GetField("availability",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (catalogField != null) catalogField.SetValue(db, catalog);

        // Create availability entries: basic cards start with 2 copies, others with 1
        foreach (var c in catalog)
        {
            bool isBasic = c.id.Contains("HeavyStrike") || c.id.Contains("Guard") || c.id.Contains("Bandage")
                || c.id.Contains("ArcaneBolt") || c.id.Contains("ManaShield") || c.id.Contains("HealingLight")
                || c.id.Contains("PlasmaShot") || c.id.Contains("DeployCover") || c.id.Contains("Repair");
            availability.Add(new CardDatabase.Availability
            {
                card = c,
                available = isBasic ? 2 : 1,
                maxCopies = 99,
                rewardEligible = true
            });
        }

        if (availField != null) availField.SetValue(db, availability);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CardSetup] Populated database with {catalog.Count} cards");
    }

    // ───────────── Card Def creation ─────────────

    [MenuItem("Tools/Ryft/Cards/Create All Card Defs")]
    public static void EnsureSeedCardDefs()
    {
        System.IO.Directory.CreateDirectory(CardFolder);

        // ═══════════ FIGHTER CLASS ═══════════

        // Basic
        CreateCard("HeavyStrike", "Heavy Strike", CardType.Attack, CardStatType.Strength,
            1, 6, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.HeavyStrikeCard",
            "Deal 6 damage.", null);
        CreateCard("Guard", "Guard", CardType.Defend, CardStatType.Strength,
            1, 5, 1, TargetingType.Self, "Game.Cards.Fighter.GuardCard",
            "Gain 5 protection.", null);
        CreateCard("Bandage", "Bandage", CardType.Heal, CardStatType.Strength,
            1, 4, 1, TargetingType.Self, "Game.Cards.Fighter.BandageCard",
            "Heal 4 HP. Rage: heal 6. Defensive: gain 3 protection.", null);

        // Rage Berserker - Momentum
        CreateCard("RelentlessAssault", "Relentless Assault", CardType.Attack, CardStatType.Strength,
            2, 8, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.RelentlessAssaultCard",
            "Deal 8 damage.", new[] { CardKeyword.Momentum });
        CreateCard("CrushingBlow", "Crushing Blow", CardType.Attack, CardStatType.Strength,
            3, 12, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.CrushingBlowCard",
            "Deal 12 damage.", new[] { CardKeyword.Momentum });
        CreateCard("Bloodthirst", "Bloodthirst", CardType.Attack, CardStatType.Strength,
            2, 3, 1, TargetingType.Self, "Game.Cards.Fighter.BloodthirstCard",
            "Your next attack this turn deals +3 damage.", new[] { CardKeyword.Momentum });
        CreateCard("Rampage", "Rampage", CardType.Attack, CardStatType.Strength,
            1, 4, 1, TargetingType.AllEnemies, "Game.Cards.Fighter.RampageCard",
            "Deal 4 damage to all enemies.", new[] { CardKeyword.Momentum });
        CreateCard("EndlessFury", "Endless Fury", CardType.Attack, CardStatType.Strength,
            4, 10, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.EndlessFuryCard",
            "Deal 10 damage twice.", new[] { CardKeyword.Momentum });

        // Rage Berserker - Rage
        CreateCard("EnterTheFrenzy", "Enter the Frenzy", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Fighter.EnterTheFrenzyCard",
            "Enter Rage Stance for 3 turns.", null);
        CreateCard("BerserkersRoar", "Berserker's Roar", CardType.Skill, CardStatType.None,
            0, 0, 0, TargetingType.Self, "Game.Cards.Fighter.BerserkersRoarCard",
            "Enter Rage Stance. Draw 2 cards. Take 5 damage.", null);
        CreateCard("ControlledFury", "Controlled Fury", CardType.Skill, CardStatType.None,
            2, 8, 0, TargetingType.Self, "Game.Cards.Fighter.ControlledFuryCard",
            "Enter Rage Stance. Gain 8 protection.", null);
        CreateCard("CalmTheStorm", "Calm the Storm", CardType.Skill, CardStatType.None,
            1, 10, 0, TargetingType.Self, "Game.Cards.Fighter.CalmTheStormCard",
            "Exit Rage Stance. Heal 10 HP.", null);

        // Rage Berserker - Lifesteal
        CreateCard("VampiricStrike", "Vampiric Strike", CardType.Attack, CardStatType.Strength,
            2, 7, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.VampiricStrikeCard",
            "Deal 7 damage. Heal HP equal to damage dealt.", null);
        CreateCard("Execute", "Execute", CardType.Attack, CardStatType.None,
            2, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Fighter.ExecuteCard",
            "Kill target if below 20% max HP.", new[] { CardKeyword.Momentum });
        CreateCard("BloodPact", "Blood Pact", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Fighter.BloodPactCard",
            "Lose 5 HP. Gain +2 Strength this turn.", null);
        CreateCard("SecondWind", "Second Wind", CardType.Heal, CardStatType.Strength,
            2, 8, 1, TargetingType.Self, "Game.Cards.Fighter.SecondWindCard",
            "Heal 8 HP. Below 50%: heal 16 instead.", null);
        CreateCard("FrenziedRegeneration", "Frenzied Regeneration", CardType.Heal, CardStatType.Strength,
            2, 3, 1, TargetingType.Self, "Game.Cards.Fighter.FrenziedRegenerationCard",
            "Heal 3 HP per attack card played this turn.", null);

        // Rage Utility
        CreateCard("RecklessSwing", "Reckless Swing", CardType.Attack, CardStatType.Strength,
            1, 10, 1, TargetingType.SingleEnemy, "Game.Cards.Fighter.RecklessSwingCard",
            "Deal 10 damage. Take 3 damage.", null);
        CreateCard("SeeingRed", "Seeing Red", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Fighter.SeeingRedCard",
            "Rage: draw 3. Otherwise: draw 1.", null);

        // Defensive Tank - Defense
        CreateCard("RetaliatingGuard", "Retaliating Guard", CardType.Defend, CardStatType.Strength,
            2, 8, 1, TargetingType.Self, "Game.Cards.Fighter.RetaliatingGuardCard",
            "Gain 8 protection. Enter Defensive Stance.", null);
        CreateCard("EnduringWall", "Enduring Wall", CardType.Defend, CardStatType.Strength,
            3, 15, 1, TargetingType.Self, "Game.Cards.Fighter.EnduringWallCard",
            "Gain 15 protection. Enter Defensive Stance.", null);
        CreateCard("ReflectiveShield", "Reflective Shield", CardType.Defend, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Fighter.ReflectiveShieldCard",
            "Reflect all damage for 1 turn. Enter Defensive Stance.", new[] { CardKeyword.Momentum });
        CreateCard("StalwartDefense", "Stalwart Defense", CardType.Defend, CardStatType.Strength,
            1, 4, 1, TargetingType.Self, "Game.Cards.Fighter.StalwartDefenseCard",
            "Gain 4 protection. Enter Defensive Stance.", null);
        CreateCard("UnyieldingFortress", "Unyielding Fortress", CardType.Defend, CardStatType.Strength,
            4, 20, 1, TargetingType.Self, "Game.Cards.Fighter.UnyieldingFortressCard",
            "Gain 20 protection. Enter Defensive Stance.", null);

        // Defensive Tank - Stance
        CreateCard("TurtleStance", "Turtle Stance", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Fighter.TurtleStanceCard",
            "Enter Defensive Stance.", null);
        CreateCard("Bunker", "Bunker", CardType.Skill, CardStatType.None,
            0, 10, 0, TargetingType.Self, "Game.Cards.Fighter.BunkerCard",
            "Enter Defensive Stance. Gain 10 protection. Cannot attack this turn.", null);
        CreateCard("ArmoredShell", "Armored Shell", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Fighter.ArmoredShellCard",
            "Enter Defensive Stance. Gain protection = 25% max HP.", null);
        CreateCard("BreakStance", "Break Stance", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Fighter.BreakStanceCard",
            "Exit Defensive Stance. Next attack deals double.", null);

        // Defensive Tank - Reflect
        CreateCard("Revenge", "Revenge", CardType.Attack, CardStatType.None,
            1, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Fighter.RevengeCard",
            "Deal damage equal to protection lost last turn.", null);
        CreateCard("DelayedJustice", "Delayed Justice", CardType.Defend, CardStatType.None,
            2, 10, 0, TargetingType.Self, "Game.Cards.Fighter.DelayedJusticeCard",
            "Gain 10 protection. Store blocked damage. Play again to deal it to all.", null);

        // Defensive Tank - Utility
        CreateCard("Taunt", "Taunt", CardType.Skill, CardStatType.Strength,
            1, 6, 1, TargetingType.Self, "Game.Cards.Fighter.TauntCard",
            "All enemies target you. Gain 6 protection.", null);
        CreateCard("LivingFortress", "Living Fortress", CardType.Skill, CardStatType.Strength,
            2, 3, 1, TargetingType.Self, "Game.Cards.Fighter.LivingFortressCard",
            "Gain 3 protection per card in hand.", null);
        CreateCard("ImmovableObject", "Immovable Object", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.Self, "Game.Cards.Fighter.ImmovableObjectCard",
            "Prevent all damage this turn.", null);
        CreateCard("RegenerativePlating", "Regenerative Plating", CardType.Heal, CardStatType.Strength,
            2, 2, 1, TargetingType.Self, "Game.Cards.Fighter.RegenerativePlatingCard",
            "Heal 2 HP per 5 protection you have.", null);

        // ═══════════ WIZARD CLASS ═══════════

        // Basic
        CreateCard("ArcaneBolt", "Arcane Bolt", CardType.Attack, CardStatType.Intellect,
            1, 4, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.ArcaneBoltCard",
            "Deal 4 damage.", null);
        CreateCard("ManaShield", "Mana Shield", CardType.Defend, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Wizard.ManaShieldCard",
            "Gain protection equal to your Intellect.", null);
        CreateCard("HealingLight", "Healing Light", CardType.Heal, CardStatType.Intellect,
            1, 5, 1, TargetingType.Self, "Game.Cards.Wizard.HealingLightCard",
            "Heal 5 HP.", null);

        // Shared Utility
        CreateCard("ArcaneIntellect", "Arcane Intellect", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Wizard.ArcaneIntellectCard",
            "Draw 3 cards. +1 Intellect this combat.", null);
        CreateCard("TimeStop", "Time Stop", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.Self, "Game.Cards.Wizard.TimeStopCard",
            "Take an extra turn.", null, CardRarity.Rare);
        CreateCard("ElementalMastery", "Elemental Mastery", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Wizard.ElementalMasteryCard",
            "Next debuff applies +2 stacks.", null);

        // Fire - Burning Application
        CreateCard("Ignite", "Ignite", CardType.Skill, CardStatType.Intellect,
            1, 3, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.IgniteCard",
            "Apply 3 Burning to target.", null);
        CreateCard("HeatWave", "Heat Wave", CardType.Skill, CardStatType.Intellect,
            2, 2, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.HeatWaveCard",
            "Apply 2 Burning to all enemies.", null);
        CreateCard("Combustion", "Combustion", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.CombustionCard",
            "Spread target's Burning to all other enemies.", null);
        CreateCard("LivingBomb", "Living Bomb", CardType.Skill, CardStatType.Intellect,
            2, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.LivingBombCard",
            "Apply 5 Burning. On death: 3 Burning to others.", null);
        CreateCard("Meteor", "Meteor", CardType.Attack, CardStatType.Intellect,
            3, 15, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.MeteorCard",
            "Deal 15 damage to all. Apply 3 Burning.", null, CardRarity.Rare);

        // Fire - Burning Detonation
        CreateCard("Inferno", "Inferno", CardType.Attack, CardStatType.Intellect,
            2, 4, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.InfernoCard",
            "Consume Burning. Deal 4 per stack.", new[] { CardKeyword.Extract });
        CreateCard("Backdraft", "Backdraft", CardType.Attack, CardStatType.None,
            1, 0, 0, TargetingType.AllEnemies, "Game.Cards.Wizard.BackdraftCard",
            "Consume all Burning. Deal 2x total to all.", new[] { CardKeyword.Extract });
        CreateCard("Meltdown", "Meltdown", CardType.Attack, CardStatType.Intellect,
            3, 10, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.MeltdownCard",
            "Deal 10 + 2x total Burning. No consume.", new[] { CardKeyword.Extract });

        // Fire - Utility
        CreateCard("FlameBarrier", "Flame Barrier", CardType.Defend, CardStatType.Intellect,
            2, 6, 1, TargetingType.Self, "Game.Cards.Wizard.FlameBarrierCard",
            "Gain 6 protection. Attackers gain 2 Burning.", null);
        CreateCard("Pyroblast", "Pyroblast", CardType.Attack, CardStatType.Intellect,
            5, 25, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.PyroblastCard",
            "Deal 25 damage. Apply 5 Burning.", null, CardRarity.Epic);
        CreateCard("PhoenixForm", "Phoenix Form", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.Self, "Game.Cards.Wizard.PhoenixFormCard",
            "If you would die: set HP to 10, deal 15 to all.", null, CardRarity.Rare);

        // Ice - Frozen Application
        CreateCard("FrostNova", "Frost Nova", CardType.Skill, CardStatType.Intellect,
            2, 5, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.FrostNovaCard",
            "Apply 5 Frozen to all enemies.", null);
        CreateCard("Permafrost", "Permafrost", CardType.Skill, CardStatType.Intellect,
            1, 3, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.PermafrostCard",
            "Apply 3 Frozen to target.", null);
        CreateCard("CreepingCold", "Creeping Cold", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Wizard.CreepingColdCard",
            "For 3 turns, apply 2 Frozen to all enemies.", null);
        CreateCard("Hypothermia", "Hypothermia", CardType.Skill, CardStatType.Intellect,
            2, 0, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.HypothermiaCard",
            "Double target's Frozen stacks.", null);
        CreateCard("Frostbolt", "Frostbolt", CardType.Attack, CardStatType.Intellect,
            2, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.FrostboltCard",
            "Deal 5 damage. Apply 2 Frozen.", null);
        CreateCard("Blizzard", "Blizzard", CardType.Attack, CardStatType.Intellect,
            3, 3, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.BlizzardCard",
            "Deal 3 damage to all. Apply 2 Frozen.", null);
        CreateCard("Avalanche", "Avalanche", CardType.Attack, CardStatType.Intellect,
            4, 8, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.AvalancheCard",
            "Deal 8 damage to all. Apply 3 Frozen.", null, CardRarity.Uncommon);

        // Ice - Frozen Detonation
        CreateCard("Shatter", "Shatter", CardType.Attack, CardStatType.Intellect,
            2, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.ShatterCard",
            "Consume Frozen. Deal 5 per stack.", new[] { CardKeyword.Extract });
        CreateCard("IceLance", "Ice Lance", CardType.Attack, CardStatType.Intellect,
            1, 3, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.IceLanceCard",
            "Deal 3 + 3 per Frozen stack. No consume.", new[] { CardKeyword.Extract });
        CreateCard("FrozenTomb", "Frozen Tomb", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.FrozenTombCard",
            "Kill if 10+ Frozen. Bosses: 30 damage.", new[] { CardKeyword.Extract }, CardRarity.Rare);
        CreateCard("Crystallize", "Crystallize", CardType.Attack, CardStatType.Intellect,
            2, 6, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.CrystallizeCard",
            "Deal 6 damage. 3+ Frozen: stun 1 turn.", new[] { CardKeyword.Extract });

        // Ice - Utility
        CreateCard("IceArmor", "Ice Armor", CardType.Defend, CardStatType.Intellect,
            2, 8, 1, TargetingType.Self, "Game.Cards.Wizard.IceArmorCard",
            "Gain 8 protection. Attackers gain 1 Frozen.", null);
        CreateCard("FlashFreeze", "Flash Freeze", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.FlashFreezeCard",
            "Apply 5 Frozen. Target immune this turn.", null);
        CreateCard("AbsoluteZero", "Absolute Zero", CardType.Skill, CardStatType.None,
            4, 0, 0, TargetingType.AllEnemies, "Game.Cards.Wizard.AbsoluteZeroCard",
            "4 Frozen to all. Frozen can never decay.", null, CardRarity.Epic);

        // Undead - Curse Application
        CreateCard("SoulDrain", "Soul Drain", CardType.Attack, CardStatType.Intellect,
            2, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.SoulDrainCard",
            "Deal 5 damage. Apply 2 Cursed.", null);
        CreateCard("PlagueWave", "Plague Wave", CardType.Attack, CardStatType.Intellect,
            3, 4, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.PlagueWaveCard",
            "Deal 4 damage to all. Apply 2 Cursed.", null);
        CreateCard("DeathsTouch", "Death's Touch", CardType.Attack, CardStatType.Intellect,
            1, 3, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.DeathsTouchCard",
            "Deal 3 damage. Apply 2 Cursed.", null);
        CreateCard("MassGrave", "Mass Grave", CardType.Attack, CardStatType.Intellect,
            4, 0, 1, TargetingType.AllEnemies, "Game.Cards.Wizard.MassGraveCard",
            "Deal 2x total Cursed to all enemies.", null, CardRarity.Rare);
        CreateCard("CurseOfWeakness", "Curse of Weakness", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.CurseOfWeaknessCard",
            "Apply 2 Weakness to target.", null);
        CreateCard("SpreadingCorruption", "Spreading Corruption", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.SpreadingCorruptionCard",
            "Apply 2 Agony to target.", null);
        CreateCard("Doom", "Doom", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.DoomCard",
            "Apply Doom (10 turns to death).", null, CardRarity.Rare);

        // Undead - Curse Detonation
        CreateCard("ConsumeSoul", "Consume Soul", CardType.Attack, CardStatType.Intellect,
            2, 4, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.ConsumeSoulCard",
            "Consume Cursed. 4 dmg + 2 heal per stack.", new[] { CardKeyword.Extract });
        CreateCard("NecroticBlast", "Necrotic Blast", CardType.Attack, CardStatType.Intellect,
            1, 0, 1, TargetingType.SingleEnemy, "Game.Cards.Wizard.NecroticBlastCard",
            "Consume Cursed. Deal 2x stacks damage.", new[] { CardKeyword.Extract });
        CreateCard("MassSuffering", "Mass Suffering", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.MassSufferingCard",
            "On target death, transfer Agony to random enemy.", null);
        CreateCard("Reaper", "Reaper", CardType.Attack, CardStatType.None,
            3, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Wizard.ReaperCard",
            "Kill if 20+ Cursed AND below 20% HP.", new[] { CardKeyword.Extract }, CardRarity.Rare);

        // ═══════════ ENGINEER CLASS ═══════════

        // Basic
        CreateCard("PlasmaShot", "Plasma Shot", CardType.Attack, CardStatType.Engineering,
            1, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.PlasmaShotCard",
            "Deal 5 damage.", null);
        CreateCard("DeployCover", "Deploy Cover", CardType.Defend, CardStatType.Engineering,
            1, 5, 1, TargetingType.Self, "Game.Cards.Engineer.DeployCoverCard",
            "Gain 5 protection.", null);
        CreateCard("Repair", "Repair", CardType.Heal, CardStatType.Engineering,
            1, 4, 1, TargetingType.Self, "Game.Cards.Engineer.RepairCard",
            "Heal 4 HP. Heal 1 device 4 HP.", null);

        // Shared Utility
        CreateCard("AnalyzeWeakness", "Analyze Weakness", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Engineer.AnalyzeWeaknessCard",
            "Target takes 25% more damage. Draw 1.", null);
        CreateCard("EmergencyProtocol", "Emergency Protocol", CardType.Skill, CardStatType.None,
            0, 0, 0, TargetingType.Self, "Game.Cards.Engineer.EmergencyProtocolCard",
            "Draw 2 cards.", null);
        CreateCard("Overclock_Eng", "Overclock", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Engineer.OverclockCard",
            "Gain 2 energy. Take 4 damage.", null);

        // Device Cards
        CreateCard("ChronoTurret", "Chrono Turret", CardType.Device, CardStatType.Engineering,
            2, 5, 1, TargetingType.Self, "Game.Cards.Engineer.ChronoTurretCard",
            "Deploy turret (5 HP). Start of turn: 3 damage to random enemy.", null);
        CreateCard("TemporalShield", "Temporal Shield", CardType.Device, CardStatType.Engineering,
            2, 8, 1, TargetingType.Self, "Game.Cards.Engineer.TemporalShieldCard",
            "Deploy shield (8 HP). Start of turn: 3 protection.", null);
        CreateCard("UnstableDrone", "Unstable Drone", CardType.Device, CardStatType.Engineering,
            1, 3, 1, TargetingType.Self, "Game.Cards.Engineer.UnstableDroneCard",
            "Deploy drone (3 HP). Start of turn: 2 damage to random enemy.", null);
        CreateCard("ParadoxMine", "Paradox Mine", CardType.Device, CardStatType.Engineering,
            2, 1, 0, TargetingType.Self, "Game.Cards.Engineer.ParadoxMineCard",
            "Deploy mine. When enemy attacks: 10 damage to all, self-destructs.", null);
        CreateCard("SentryGun", "Sentry Gun", CardType.Device, CardStatType.Engineering,
            3, 8, 1, TargetingType.Self, "Game.Cards.Engineer.SentryGunCard",
            "Deploy sentry (8 HP). Start of turn: 5 damage to random enemy.", null);
        CreateCard("HealingStation", "Healing Station", CardType.Device, CardStatType.Engineering,
            2, 6, 1, TargetingType.Self, "Game.Cards.Engineer.HealingStationCard",
            "Deploy station (6 HP). Start of turn: heal 3 HP.", null);
        CreateCard("DecoyHologram", "Decoy Hologram", CardType.Device, CardStatType.Engineering,
            1, 4, 1, TargetingType.Self, "Game.Cards.Engineer.DecoyHologramCard",
            "Deploy decoy (4 HP). Enemies attack it first.", null);
        CreateCard("OverchargedCapacitor", "Overcharged Capacitor", CardType.Device, CardStatType.Engineering,
            2, 3, 1, TargetingType.Self, "Game.Cards.Engineer.OverchargedCapacitorCard",
            "Deploy capacitor (3 HP). All attacks deal +1 damage.", null);
        CreateCard("MissileBattery", "Missile Battery", CardType.Device, CardStatType.Engineering,
            4, 10, 1, TargetingType.Self, "Game.Cards.Engineer.MissileBatteryCard",
            "Deploy battery (10 HP). Start of turn: 8 damage to random enemy.", null, CardRarity.Uncommon);
        CreateCard("TemporalAnchor", "Temporal Anchor", CardType.Device, CardStatType.Engineering,
            3, 5, 1, TargetingType.Self, "Game.Cards.Engineer.TemporalAnchorCard",
            "Deploy anchor (5 HP). Start of turn: draw 1 card.", null);

        // Detonation
        CreateCard("ControlledDemolition", "Controlled Demolition", CardType.Skill, CardStatType.Engineering,
            1, 8, 1, TargetingType.Self, "Game.Cards.Engineer.ControlledDemolitionCard",
            "Destroy 1 device. Deal 8 damage to all.", null);
        CreateCard("ChainReaction", "Chain Reaction", CardType.Skill, CardStatType.Engineering,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.ChainReactionCard",
            "Destroy all devices. Deal 3^N to all.", new[] { CardKeyword.Rewind }, CardRarity.Rare);
        CreateCard("Overload", "Overload", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Engineer.OverloadCard",
            "Destroy 1 device. Draw 2.", new[] { CardKeyword.Rewind });
        CreateCard("SalvageProtocol", "Salvage Protocol", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Engineer.SalvageProtocolCard",
            "Destroy 1 device. Gain cost+1 energy.", null);

        // Device Synergy
        CreateCard("EmergencyRepairs", "Emergency Repairs", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.EmergencyRepairsCard",
            "Heal all devices to full HP.", null);
        CreateCard("MassProduction", "Mass Production", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.Self, "Game.Cards.Engineer.MassProductionCard",
            "Copy a random device.", null);
        CreateCard("FortifiedPlating", "Fortified Plating", CardType.Skill, CardStatType.Engineering,
            1, 4, 1, TargetingType.Self, "Game.Cards.Engineer.FortifiedPlatingCard",
            "All devices gain +4 max HP and heal.", null);
        CreateCard("NetworkLink", "Network Link", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.NetworkLinkCard",
            "Trigger all device start-of-turn effects.", new[] { CardKeyword.Rewind });

        // Cyborg Attack
        CreateCard("ArmCannon", "Arm Cannon", CardType.Attack, CardStatType.Engineering,
            2, 7, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.ArmCannonCard",
            "Deal 7 damage.", null);
        CreateCard("RocketBarrage", "Rocket Barrage", CardType.Attack, CardStatType.Engineering,
            3, 4, 1, TargetingType.AllEnemies, "Game.Cards.Engineer.RocketBarrageCard",
            "Deal 4 damage to all enemies.", null);
        CreateCard("AlphaBeam", "Alpha Beam", CardType.Attack, CardStatType.Engineering,
            1, 3, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.AlphaBeamCard",
            "Deal 3 damage.", null);
        CreateCard("BetaBeam", "Beta Beam", CardType.Attack, CardStatType.Engineering,
            2, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.BetaBeamCard",
            "Deal 5 damage.", null);
        CreateCard("GammaBeam", "Gamma Beam", CardType.Attack, CardStatType.Engineering,
            4, 20, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.GammaBeamCard",
            "Deal 20 damage.", new[] { CardKeyword.Rewind }, CardRarity.Rare);

        // Weapon Installation
        CreateCard("InstallMinigun", "Install: Minigun", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.InstallMinigunCard",
            "Attacks hit 1 extra time at 50% damage.", null);
        CreateCard("InstallRailgun", "Install: Railgun", CardType.Skill, CardStatType.None,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.InstallRailgunCard",
            "Single-target attacks splash 50% to others.", null);
        CreateCard("InstallFlamethrower", "Install: Flamethrower", CardType.Skill, CardStatType.Engineering,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.InstallFlamethrowerCard",
            "Attacks apply 2 Burning.", null);
        CreateCard("InstallCryoCannon", "Install: Cryo Cannon", CardType.Skill, CardStatType.Engineering,
            2, 0, 0, TargetingType.Self, "Game.Cards.Engineer.InstallCryoCannonCard",
            "Attacks apply 2 Frozen.", null);

        // Overclock
        CreateCard("OverclockSystems", "Overclock Systems", CardType.Skill, CardStatType.Engineering,
            2, 3, 1, TargetingType.Self, "Game.Cards.Engineer.OverclockSystemsCard",
            "Gain 3 Overclock stacks.", null);
        CreateCard("MaximumOverdrive", "Maximum Overdrive", CardType.Skill, CardStatType.None,
            3, 0, 0, TargetingType.Self, "Game.Cards.Engineer.MaximumOverdriveCard",
            "Double Overclock stacks.", null, CardRarity.Rare);
        CreateCard("AdrenalineInjector", "Adrenaline Injector", CardType.Skill, CardStatType.Engineering,
            1, 5, 1, TargetingType.Self, "Game.Cards.Engineer.AdrenalineInjectorCard",
            "Gain 5 Overclock stacks. Draw 2.", null);
        CreateCard("TimeDilationField", "Time Dilation Field", CardType.Skill, CardStatType.Engineering,
            4, 30, 1, TargetingType.Self, "Game.Cards.Engineer.TimeDilationFieldCard",
            "Gain 30 Overclock stacks.", null, CardRarity.Epic);

        // Cyborg Enhancement
        CreateCard("ReactiveArmor", "Reactive Armor", CardType.Defend, CardStatType.None,
            2, 8, 0, TargetingType.Self, "Game.Cards.Engineer.ReactiveArmorCard",
            "Gain 8 protection. +4 if weapon installed.", null);
        CreateCard("CombatStimulants", "Combat Stimulants", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Engineer.CombatStimulantsCard",
            "Draw 3. Gain 1 energy. Take 3 at end of turn.", null);
        CreateCard("HaveYouTriedTurningItOffAndTurningItBackOn", "Have You Tried Turning it Off and Turning it Back on?", CardType.Skill, CardStatType.None,
            1, 0, 0, TargetingType.Self, "Game.Cards.Engineer.HaveYouTriedTurningItOffAndTurningItBackOn",
            "Remove Overclock. Heal + protect = stacks.", null);

        // Special Attacks
        CreateCard("Card418", "418", CardType.Attack, CardStatType.Engineering,
            1, 5, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.Card418",
            "Apply 5 Burning to target.", null);
        CreateCard("Card402", "402", CardType.Attack, CardStatType.Engineering,
            2, 6, 1, TargetingType.SingleEnemy, "Game.Cards.Engineer.Card402",
            "Deal 6 damage. Kill = +20 gold.", null);
        CreateCard("Card403", "403", CardType.Attack, CardStatType.None,
            2, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Engineer.Card403",
            "Kill if target HP < your HP.", null, CardRarity.Rare);
        CreateCard("Card503", "503", CardType.Attack, CardStatType.None,
            2, 0, 0, TargetingType.SingleEnemy, "Game.Cards.Engineer.Card503",
            "Target cannot attack next turn.", null);
        CreateCard("Card500", "500", CardType.Attack, CardStatType.Engineering,
            6, 40, 1, TargetingType.AllEnemies, "Game.Cards.Engineer.Card500",
            "Deal 40 to all. Take 15 damage.", new[] { CardKeyword.Rewind }, CardRarity.Legendary);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CardSetup] All card defs created!");
    }

    private static void CreateCard(string id, string displayName, CardType cardType, CardStatType statType,
        int cost, int power, int scaling, TargetingType targeting, string runtimeType,
        string description, CardKeyword[] keywords, CardRarity rarity = CardRarity.Common)
    {
        string path = $"{CardFolder}/Card_{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<CardDef>(path);
        if (existing != null)
        {
            // Update existing asset in place so name/stat changes propagate
            existing.id = id;
            existing.displayName = displayName;
            existing.description = description ?? "";
            existing.cardType = cardType;
            existing.statType = statType;
            existing.energyCost = cost;
            existing.power = power;
            existing.scaling = scaling;
            existing.targeting = targeting;
            existing.runtimeTypeName = runtimeType;
            existing.keywords = keywords;
            existing.rarity = rarity;
            EditorUtility.SetDirty(existing);
            Debug.Log($"[CardSetup] Updated {id}");
            return;
        }

        var def = ScriptableObject.CreateInstance<CardDef>();
        def.id = id;
        def.displayName = displayName;
        def.description = description ?? "";
        def.cardType = cardType;
        def.statType = statType;
        def.energyCost = cost;
        def.power = power;
        def.scaling = scaling;
        def.targeting = targeting;
        def.runtimeTypeName = runtimeType;
        def.keywords = keywords;
        def.rarity = rarity;

        AssetDatabase.CreateAsset(def, path);
        Debug.Log($"[CardSetup] Created {id}");
    }

    // ───────────── Info ─────────────

    [MenuItem("Tools/Ryft/Cards/List All Cards")]
    public static void ListAllCards()
    {
        var db = CardDatabase.Load();
        if (db == null)
        {
            Debug.LogError("CardDatabase not found!");
            return;
        }

        Debug.Log($"=== Card Database ({db.Catalog.Count} cards) ===");
        foreach (var c in db.Catalog)
        {
            if (c == null) continue;
            Debug.Log($"  [{c.cardType}] {c.id}: {c.displayName} (Cost:{c.energyCost} Pow:{c.power} {c.statType})");
        }
    }
}
