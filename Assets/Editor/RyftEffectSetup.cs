#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Game.Ryfts;

public static class RyftEffectSetup
{
    private struct EffectEntry
    {
        public string id, displayName, description;
        public RyftColor color;
        public EffectPolarity polarity;
        public RyftRarity rarity;
        public BuiltInOp builtIn;
        public int intMag;
        public float floatMag;
        public string runtimeType;
    }

    [MenuItem("Tools/Ryft/Create All 53 Ryft Effects")]
    public static void CreateAll()
    {
        const string folder = "Assets/Resources/Ryfts";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "Ryfts");
        }

        var entries = BuildAllEntries();
        var defs = new List<RyftEffectDef>();

        foreach (var e in entries)
        {
            string path = $"{folder}/RyftEffect_{e.id}.asset";

            var def = AssetDatabase.LoadAssetAtPath<RyftEffectDef>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<RyftEffectDef>();
                AssetDatabase.CreateAsset(def, path);
            }

            def.id = e.id;
            def.displayName = e.displayName;
            def.description = e.description;
            def.color = e.color;
            def.polarity = e.polarity;
            def.rarity = e.rarity;
            def.lifetime = EffectLifetime.Permanent;
            def.maxStacks = 5;
            def.chancePercent = 100f;
            def.builtIn = e.builtIn;
            def.intMagnitude = e.intMag;
            def.floatMagnitude = e.floatMag;
            def.runtimeTypeName = e.runtimeType ?? "";

            EditorUtility.SetDirty(def);
            defs.Add(def);
        }

        // Populate database
        string dbPath = $"databases/RyftEffectDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<RyftEffectDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<RyftEffectDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        db.SetEffects(defs);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RyftEffectSetup] Created {defs.Count} effect assets + updated database.");
    }

    private static List<EffectEntry> BuildAllEntries()
    {
        var list = new List<EffectEntry>();

        // ══════ ORANGE CLOSED (13) ══════
        list.Add(E("OC_EnergyPerTurn",     "+1 Energy/Turn",            "Gain 1 extra energy at the start of each turn.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.GainEnergyOnTurnStart, intMag: 1));
        list.Add(E("OC_DrawPerTurn",        "+1 Card Draw/Turn",         "Draw 1 extra card at the start of each turn.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.DrawOnTurnStart, intMag: 1));
        list.Add(E("OC_StartProtection",    "Starting Protection",       "Start each battle with 2 Protection.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.ProtectionOnBattleStart, intMag: 2));
        list.Add(E("OC_BonusAttack",        "+2 Attack Damage",          "All attacks deal 2 bonus damage.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.BonusAttackDamage, intMag: 2));
        list.Add(E("OC_HealEndTurn",        "Heal 2 HP/Turn",           "Heal 2 HP at the end of each turn.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.HealOnTurnEnd, intMag: 2));
        list.Add(E("OC_CostReduction",      "Cards Cost 1 Less",        "All cards cost 1 less energy (min 0).",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.EnergyCostReduction, intMag: 1));
        list.Add(E("OC_FirstAttackDouble",  "First Attack Doubles",      "The first attack each turn deals double damage.",
            RyftColor.Orange, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.FirstAttackDoubleDamageEffect"));
        list.Add(E("OC_DefendBonus",        "Defend Grants +5 Prot",     "Playing a Defend card grants +5 bonus Protection.",
            RyftColor.Orange, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.DefendGrantsProtectionEffect", intMag: 5));
        list.Add(E("OC_WeaknessEnemies",    "Enemies Start Weak",        "All enemies start combat with 2 Weakness.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.WeaknessOnEnemies, intMag: 2));
        list.Add(E("OC_ExtraStartCards",    "+3 Starting Cards",         "Draw 3 extra cards at the start of combat.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.ExtraCardsOnBattleStart, intMag: 3));
        list.Add(E("OC_KeywordEnergy",      "Keyword +1 Energy",         "Keyword triggers (Momentum, Extract) grant 1 extra energy.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.KeywordBonusEnergy, intMag: 1));
        list.Add(E("OC_HealOnKill",         "Kill Heals 5 HP",          "Killing an enemy heals 5 HP.",
            RyftColor.Orange, EffectPolarity.Positive, BuiltInOp.HealOnKill, intMag: 5));
        list.Add(E("OC_ProtectionCarry",    "Protection Carries Over",   "10% of Protection persists between turns.",
            RyftColor.Orange, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.ProtectionCarryOverEffect", floatMag: 0.10f));

        // ══════ ORANGE EXPLODED (10) ══════
        list.Add(E("OX_LoseEnergy",         "-1 Energy/Turn",            "Lose 1 energy at the start of each turn.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.LoseEnergyOnTurnStart, intMag: 1));
        list.Add(E("OX_DrawReduction",      "-1 Card Draw/Turn",         "Draw 1 fewer card per turn (min 1).",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.DrawReduction, intMag: 1));
        list.Add(E("OX_DamageEndTurn",      "Take 1 Dmg/Turn",          "Take 1 damage at the end of each turn.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.DamageOnTurnEnd, intMag: 1));
        list.Add(E("OX_CostIncrease",       "Cards Cost +1",            "All cards cost 1 more energy.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.EnergyCostIncrease, intMag: 1));
        list.Add(E("OX_EnemyDmgBonus",      "Enemies +5% Damage",       "All enemies deal 5% more damage.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.EnemyDamageBonusPercent, floatMag: 0.05f));
        list.Add(E("OX_PlayerWeakness",     "Start with Weakness",       "Start each combat with 1 Weakness.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.WeaknessOnPlayer, intMag: 1));
        list.Add(E("OX_EnemyProtection",    "Enemies Start Protected",   "All enemies start combat with 5 Protection.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.ProtectionOnEnemies, intMag: 5));
        list.Add(E("OX_InitialDrawReduce",  "-2 Starting Cards",         "Draw 2 fewer cards at the start of combat.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.InitialDrawReduction, intMag: 2));
        list.Add(E("OX_DmgDiscardsCard",    "Damage Discards Card",      "Taking damage discards a random card from your hand.",
            RyftColor.Orange, EffectPolarity.Negative, runtime: "Game.Ryfts.Effects.DamageDiscardsCardEffect"));
        list.Add(E("OX_ProtReduction",      "Protection -10%",           "Your Protection is 10% less effective.",
            RyftColor.Orange, EffectPolarity.Negative, BuiltInOp.ProtectionReductionPercent, floatMag: 0.10f));

        // ══════ PURPLE CLOSED (6) ══════
        list.Add(E("PC_EquipStatBonus",     "Equipment +10% Stats",      "Equipment provides 10% more stats.",
            RyftColor.Purple, EffectPolarity.Positive, BuiltInOp.EquipmentStatBonusPercent, floatMag: 0.10f));
        list.Add(E("PC_EquipSaleDouble",    "Double Equip Sale",         "Sell equipment for double gold.",
            RyftColor.Purple, EffectPolarity.Positive, BuiltInOp.EquipmentSaleMultiplier, floatMag: 2.0f));
        list.Add(E("PC_EquipCostDown",      "Equipment -10% Cost",       "Equipment costs 10% less in shops.",
            RyftColor.Purple, EffectPolarity.Positive, BuiltInOp.EquipmentCostPercent, floatMag: -0.10f));
        list.Add(E("PC_EquipMaxHP",         "+5 Max HP/Piece",           "Gain +5 max HP for each equipped piece.",
            RyftColor.Purple, EffectPolarity.Positive, BuiltInOp.EquipmentMaxHpPerPiece, intMag: 5));
        list.Add(E("PC_EquipHealTurn",      "Equip Heals 2/Turn",       "Heal 2 HP per equipped piece at the start of each turn.",
            RyftColor.Purple, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.EquipmentHealPerTurnEffect", intMag: 2));
        list.Add(E("PC_EquipEnergyTurn",    "+1 Energy/Piece/Turn",      "Gain 1 energy per equipped piece each turn.",
            RyftColor.Purple, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.EquipmentEnergyPerTurnEffect", intMag: 1));

        // ══════ PURPLE EXPLODED (5) ══════
        list.Add(E("PX_EquipStatDown",      "Equipment -10% Stats",      "Equipment provides 10% fewer stats.",
            RyftColor.Purple, EffectPolarity.Negative, BuiltInOp.EquipmentStatBonusPercent, floatMag: -0.10f));
        list.Add(E("PX_EquipBreakChance",   "1% Break Chance",           "1% chance each equipped piece breaks after combat.",
            RyftColor.Purple, EffectPolarity.Negative, runtime: "Game.Ryfts.Effects.EquipmentBreakChanceEffect", floatMag: 0.01f));
        list.Add(E("PX_EquipSellPenalty",   "Sell Equip -5%",            "Equipment sells for 5% less.",
            RyftColor.Purple, EffectPolarity.Negative, BuiltInOp.EquipmentSellPenaltyPercent, floatMag: -0.05f));
        list.Add(E("PX_EquipCostUp",        "Equipment +5% Cost",        "Equipment costs 5% more in shops.",
            RyftColor.Purple, EffectPolarity.Negative, BuiltInOp.EquipmentCostIncreasePercent, floatMag: 0.05f));
        list.Add(E("PX_EquipMaxHPDown",     "-1 Max HP/Piece",           "Lose 1 max HP for each equipped piece.",
            RyftColor.Purple, EffectPolarity.Negative, BuiltInOp.EquipmentReduceMaxHpPerPiece, intMag: 1));

        // ══════ BLUE CLOSED (7) ══════
        list.Add(E("BC_CopyHighCost",       "Copy Highest Card",         "Copy the highest cost card to your hand at battle start.",
            RyftColor.Blue, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.HighestCostCardCopyEffect"));
        list.Add(E("BC_KeywordCostDown",    "Keyword Cards -1 Cost",     "Cards with keywords cost 1 less energy.",
            RyftColor.Blue, EffectPolarity.Positive, BuiltInOp.KeywordCostReduction, intMag: 1));
        list.Add(E("BC_HealBonus",          "Heal Cards +10%",           "Heal cards restore 10% more HP.",
            RyftColor.Blue, EffectPolarity.Positive, BuiltInOp.HealCardBonusPercent, floatMag: 0.10f));
        list.Add(E("BC_DoubleStrike",       "5% Double Strike",          "Attack cards have a 5% chance to strike twice.",
            RyftColor.Blue, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.AttackDoubleStrikeEffect", floatMag: 0.05f));
        list.Add(E("BC_DefendBonus",        "Defend +3 Protection",      "Defend cards grant 3 extra Protection.",
            RyftColor.Blue, EffectPolarity.Positive, BuiltInOp.DefendCardBonusProtection, intMag: 3));
        list.Add(E("BC_DrawEvery3",         "Draw Every 3 Plays",        "Every 3 cards played, draw 1 card.",
            RyftColor.Blue, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.DrawEveryNPlaysEffect", intMag: 3));
        list.Add(E("BC_ZeroCostDraw",       "0-Cost Cards Draw",         "Playing a 0-cost card draws 1 card.",
            RyftColor.Blue, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.ZeroCostDrawEffect"));

        // ══════ BLUE EXPLODED (3) ══════
        list.Add(E("BX_HealPenalty",        "Heal Cards -5%",            "Heal cards restore 5% less HP.",
            RyftColor.Blue, EffectPolarity.Negative, BuiltInOp.HealCardPenaltyPercent, floatMag: -0.05f));
        list.Add(E("BX_MissChance",         "2% Attack Miss",            "Attack cards have a 2% chance to deal 0 damage.",
            RyftColor.Blue, EffectPolarity.Negative, runtime: "Game.Ryfts.Effects.AttackMissChanceEffect", floatMag: 0.02f));
        list.Add(E("BX_HandSizeDown",       "Hand Size -1",              "Maximum hand size is reduced by 1.",
            RyftColor.Blue, EffectPolarity.Negative, BuiltInOp.HandSizeModifier, intMag: -1));

        // ══════ GREEN CLOSED (5) ══════
        list.Add(E("GC_GoldGain",           "+5% Gold",                  "Gain 5% more gold from all sources.",
            RyftColor.Green, EffectPolarity.Positive, BuiltInOp.GoldGainPercent, floatMag: 0.05f));
        list.Add(E("GC_ShopDiscount",       "Shop -2% Prices",           "Shop prices are 2% lower.",
            RyftColor.Green, EffectPolarity.Positive, BuiltInOp.ShopPricePercent, floatMag: -0.02f));
        list.Add(E("GC_GoldInterest",       "1% Gold Interest",          "Earn 1% interest on your gold after each combat.",
            RyftColor.Green, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.GoldInterestEffect", floatMag: 0.01f));
        list.Add(E("GC_GoldPerDamage",      "1 Gold/Damage",             "Earn 1 gold for each point of damage dealt in combat.",
            RyftColor.Green, EffectPolarity.Positive, runtime: "Game.Ryfts.Effects.GoldPerDamageEffect", floatMag: 1.0f));
        list.Add(E("GC_SellBonus",          "Sell +2%",                  "Items sell for 2% more.",
            RyftColor.Green, EffectPolarity.Positive, BuiltInOp.SellValuePercent, floatMag: 0.02f));

        // ══════ GREEN EXPLODED (4) ══════
        list.Add(E("GX_GoldLoss",           "-5% Gold",                  "Gain 5% less gold from all sources.",
            RyftColor.Green, EffectPolarity.Negative, BuiltInOp.GoldGainPercent, floatMag: -0.05f));
        list.Add(E("GX_ShopIncrease",       "Shop +2% Prices",           "Shop prices are 2% higher.",
            RyftColor.Green, EffectPolarity.Negative, BuiltInOp.ShopPriceIncreasePercent, floatMag: 0.02f));
        list.Add(E("GX_GoldLossAfterCombat","Lose 1% Gold/Combat",       "Lose 1% of your gold after each combat.",
            RyftColor.Green, EffectPolarity.Negative, runtime: "Game.Ryfts.Effects.GoldLossAfterCombatEffect", floatMag: 0.01f));
        list.Add(E("GX_GoldLossPerCard",    "Lose 2 Gold/Card",          "Lose 2 gold each time you play a card.",
            RyftColor.Green, EffectPolarity.Negative, runtime: "Game.Ryfts.Effects.GoldLossPerCardEffect", intMag: 2));

        return list;
    }

    private static EffectEntry E(string id, string name, string desc,
        RyftColor color, EffectPolarity polarity,
        BuiltInOp builtIn = BuiltInOp.None,
        int intMag = 0, float floatMag = 0f,
        string runtime = null,
        RyftRarity rarity = RyftRarity.Common)
    {
        return new EffectEntry
        {
            id = id, displayName = name, description = desc,
            color = color, polarity = polarity, rarity = rarity,
            builtIn = builtIn, intMag = intMag, floatMag = floatMag,
            runtimeType = runtime
        };
    }
}
#endif
