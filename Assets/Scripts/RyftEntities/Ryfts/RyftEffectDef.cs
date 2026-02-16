using UnityEngine;

namespace Game.Ryfts
{
    public enum BuiltInOp
    {
        None,
        // Legacy permanent stat mods
        AddMaxHealth, AddStrength, AddIntellect, AddEngineering,
        // Legacy chance-based triggers
        ChanceDoubleCast, ChanceShieldOnBattleStart, ChanceHealOnHit, ChanceIgnoreDefense,

        // ── Triggered (A) — Orange Combat ──
        GainEnergyOnTurnStart, DrawOnTurnStart, ProtectionOnBattleStart, HealOnTurnEnd,
        WeaknessOnEnemies, ExtraCardsOnBattleStart, HealOnKill,
        LoseEnergyOnTurnStart, DamageOnTurnEnd, WeaknessOnPlayer, ProtectionOnEnemies,

        // ── Passive Query (B) — Orange Combat ──
        BonusAttackDamage, EnergyCostReduction, KeywordBonusEnergy,
        EnergyCostIncrease, DrawReduction, InitialDrawReduction,
        EnemyDamageBonusPercent, ProtectionReductionPercent,

        // ── Passive Query (B) — Purple Equipment ──
        EquipmentStatBonusPercent, EquipmentSaleMultiplier, EquipmentCostPercent,
        EquipmentMaxHpPerPiece, EquipmentBreakChancePercent, EquipmentSellPenaltyPercent,
        EquipmentCostIncreasePercent, EquipmentReduceMaxHpPerPiece,

        // ── Passive Query (B) — Blue Cards ──
        KeywordCostReduction, HealCardBonusPercent, DefendCardBonusProtection,
        HandSizeModifier, HealCardPenaltyPercent,

        // ── Passive Query (B) — Green Gold ──
        GoldGainPercent, ShopPricePercent, SellValuePercent,
        ShopPriceIncreasePercent,
    }

    [CreateAssetMenu(menuName="Game/Ryfts/Ryft Effect", fileName="RyftEffect_")]
    public class RyftEffectDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;              // unique key
        public string displayName;
        [TextArea] public string description;

        [Header("Classification")]
        public RyftColor color;
        public EffectPolarity polarity;
        public RyftRarity rarity = RyftRarity.Common;

        [Header("Lifetime")]
        public EffectLifetime lifetime = EffectLifetime.Permanent;
        public int durationTurns = 0;     // used if DurationNTurns
        public int delayTurns = 0;        // wait N trigger passes before evaluating
        public int maxStacks = 1;

        [Header("Proc Chance & Cooldown")]
        [Range(0,100)] public float chancePercent = 100f; // % roll at trigger time
        public int internalCooldownTurns = 0;             // self-ICD between procs

        [Header("Numbers")]
        public int intMagnitude = 0;      // e.g., +5 health, +1 strength
        public float floatMagnitude = 0f; // e.g., +0.10f for 10%

        [Header("Execution")]
        public BuiltInOp builtIn = BuiltInOp.None;
        // If set, instantiate this runtime (class must inherit RyftEffectRuntime)
        public string runtimeTypeName;

        public RyftEffectRuntime CreateRuntime()
        {
            if (!string.IsNullOrEmpty(runtimeTypeName))
            {
                var t = System.Type.GetType(runtimeTypeName);
                if (t != null)
                {
                    var inst = System.Activator.CreateInstance(t) as RyftEffectRuntime;
                    if (inst != null) { inst.Bind(this); return inst; }
                }
                Debug.LogError($"RyftEffect runtime type not found or invalid: {runtimeTypeName}");
            }

            // Fallback to parametric built-in runtime
            var builtin = new BuiltInRyftEffect();
            builtin.Bind(this);
            return builtin;
        }
    }
}
