using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public enum StatusEffectType
    {
        // Original effects
        Stun,
        Slow,
        DefenseUp,
        DamageReduction,
        Reflect,
        ReflectMagic,
        ReflectRanged,
        Taunt,
        BlockRanged,
        ShieldDrone,
        DecoyRedirect,
        Countering,
        DoubleNextGadget,
        FreeNextEngCard,

        // New debuffs
        Burning,          // 2 damage per stack at start of turn. No decay.
        Frozen,           // +2% damage taken per stack. Decay 1 at end of turn.
        Cursed,           // Stackable, consumed by spells. No decay.
        Agony,            // 1% max HP damage per stack at start of turn. No decay.
        Weakness,         // Deal 25% less damage. Remove 1 stack at end of turn.
        Doom,             // Counter from 10. -1 per turn end. At 0, instant death.

        // New buffs
        Overclock,        // +5% damage per stack. At 20 stacks, extra turn + reset.
        Protection,       // Flat damage absorption. Decays at end of turn.
        ReflectAll,       // Reflect all damage back for N turns.
        DamageImmune,     // Prevent all damage this turn.
        NextAttackBonus,  // Next attack deals +N damage.
        NextAttackDouble, // Next attack deals double damage.
        CannotAttack,     // Cannot play attack cards this turn.
        FlameBarrier,     // When attacked, attacker gets Burning.
        IceArmor,         // When attacked, attacker gets Frozen.
        PhoenixForm,      // If would die, heal to 10 HP + deal 15 to all.
        AnalyzeWeakness,  // Target takes 25% more damage this turn.
        LivingBomb,       // On death, apply 3 Burning to all other enemies.
        MassSuffering,    // On death, transfer Agony stacks to random enemy.
        FrozenImmune,     // Frozen stacks cannot decay (Absolute Zero).
        CreepingCold,     // Apply 2 Frozen to all enemies at start of turn for N turns.
        DelayedJustice,   // Stores blocked damage, next play deals it to all.
        CombatStimDamage, // Take 3 damage at end of turn.
        EnemySkipAttack,  // Enemy skips their attack.
        ElementalMastery, // Next debuff application gets +2 stacks.
        DecoyTarget,      // Enemies target this entity first.
    }

    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType Type;
        public int Duration;    // -1 = permanent, 0 = expired, >0 = turns remaining
        public int Stacks;
        public float Value;
        public string SourceId;

        public StatusEffect(StatusEffectType type, int duration, int stacks = 1, float value = 0f, string sourceId = "")
        {
            Type = type;
            Duration = duration;
            Stacks = stacks;
            Value = value;
            SourceId = sourceId;
        }

        public bool TickDuration()
        {
            if (Duration > 0) Duration--;
            return Duration != 0;
        }
    }

    public class StatusEffectManager
    {
        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();
        private readonly IActor owner;

        public StatusEffectManager(IActor owner)
        {
            this.owner = owner;
        }

        public void AddEffect(StatusEffectType type, int duration, int stacks = 1, float value = 0f, string sourceId = "")
        {
            var existing = activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.Duration = Mathf.Max(existing.Duration, duration);
                existing.Stacks += stacks;
                existing.Value = Mathf.Max(existing.Value, value);
            }
            else
            {
                activeEffects.Add(new StatusEffect(type, duration, stacks, value, sourceId));
            }

            Debug.Log($"[StatusEffect] {owner.DisplayName} gained {type} (duration={duration}, stacks={stacks}, value={value})");
        }

        /// <summary>Set stacks to exact value (don't add).</summary>
        public void SetStacks(StatusEffectType type, int stacks, int duration = -1)
        {
            var existing = activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.Stacks = stacks;
                if (stacks <= 0)
                    activeEffects.Remove(existing);
            }
            else if (stacks > 0)
            {
                activeEffects.Add(new StatusEffect(type, duration, stacks));
            }
        }

        public void RemoveEffect(StatusEffectType type)
        {
            activeEffects.RemoveAll(e => e.Type == type);
        }

        public bool HasEffect(StatusEffectType type)
        {
            return activeEffects.Exists(e => e.Type == type);
        }

        public StatusEffect GetEffect(StatusEffectType type)
        {
            return activeEffects.Find(e => e.Type == type);
        }

        public int GetStacks(StatusEffectType type)
        {
            var e = GetEffect(type);
            return e?.Stacks ?? 0;
        }

        /// <summary>Consume all stacks. Returns number consumed.</summary>
        public int ConsumeAllStacks(StatusEffectType type)
        {
            var e = GetEffect(type);
            if (e == null) return 0;
            int stacks = e.Stacks;
            RemoveEffect(type);
            return stacks;
        }

        public IReadOnlyList<StatusEffect> GetActiveEffects() => activeEffects;

        public int CountBuffs()
        {
            int count = 0;
            foreach (var effect in activeEffects)
                if (IsBuff(effect.Type)) count += effect.Stacks;
            return count;
        }

        /// <summary>Tick effects at start of turn (burning, agony, creeping cold damage).</summary>
        public void TickStartOfTurn()
        {
            // Burning: deal 2 damage per stack
            var burning = GetEffect(StatusEffectType.Burning);
            if (burning != null && burning.Stacks > 0)
            {
                int dmg = burning.Stacks * 2;
                Debug.Log($"[Burning] {owner.DisplayName} takes {dmg} burn damage ({burning.Stacks} stacks)");
                owner.ApplyDamage(dmg);
            }

            // Agony: deal 1% max HP per stack
            var agony = GetEffect(StatusEffectType.Agony);
            if (agony != null && agony.Stacks > 0)
            {
                int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.TotalStats.maxHealth * 0.01f * agony.Stacks));
                Debug.Log($"[Agony] {owner.DisplayName} takes {dmg} agony damage ({agony.Stacks} stacks)");
                owner.ApplyDamage(dmg);
            }
        }

        /// <summary>Tick effects at end of turn (frozen decay, weakness decay, doom countdown).</summary>
        public void TickEndOfTurn()
        {
            // Frozen: decay by 1 (unless immune)
            if (!HasEffect(StatusEffectType.FrozenImmune))
            {
                var frozen = GetEffect(StatusEffectType.Frozen);
                if (frozen != null)
                {
                    frozen.Stacks = Mathf.Max(0, frozen.Stacks - 1);
                    if (frozen.Stacks <= 0) RemoveEffect(StatusEffectType.Frozen);
                }
            }

            // Weakness: remove 1 stack
            var weakness = GetEffect(StatusEffectType.Weakness);
            if (weakness != null)
            {
                weakness.Stacks--;
                if (weakness.Stacks <= 0) RemoveEffect(StatusEffectType.Weakness);
            }

            // Doom: countdown
            var doom = GetEffect(StatusEffectType.Doom);
            if (doom != null)
            {
                doom.Stacks--;
                Debug.Log($"[Doom] {owner.DisplayName} doom counter: {doom.Stacks}");
                if (doom.Stacks <= 0)
                {
                    Debug.Log($"[Doom] {owner.DisplayName} dies from Doom!");
                    owner.ApplyDamage(99999);
                }
            }
        }

        /// <summary>Old-style tick for backward compat - ticks durations on all effects.</summary>
        public void TickAllEffects()
        {
            // Start-of-turn damage ticks
            TickStartOfTurn();

            // Duration-based removal
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (!activeEffects[i].TickDuration())
                {
                    Debug.Log($"[StatusEffect] {owner.DisplayName} lost {activeEffects[i].Type}");
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            activeEffects.Clear();
        }

        private bool IsBuff(StatusEffectType type)
        {
            return type != StatusEffectType.Stun && type != StatusEffectType.Slow
                && type != StatusEffectType.Burning && type != StatusEffectType.Frozen
                && type != StatusEffectType.Cursed && type != StatusEffectType.Agony
                && type != StatusEffectType.Weakness && type != StatusEffectType.Doom;
        }

        public int ApplyOutgoingDamageModifiers(int baseDamage)
        {
            float damage = baseDamage;

            if (HasEffect(StatusEffectType.Slow))
                damage *= 0.75f;

            if (HasEffect(StatusEffectType.Weakness))
                damage *= 0.75f;

            // Ryft passive: enemies deal bonus % damage (only for non-player actors)
            if (owner is not Game.Player.PlayerCharacter)
            {
                var mgr = Game.Ryfts.RyftEffectManager.Instance;
                if (mgr != null)
                {
                    float bonusPct = mgr.SumFloat(Game.Ryfts.BuiltInOp.EnemyDamageBonusPercent);
                    if (bonusPct > 0f) damage *= (1f + bonusPct);
                }
            }

            return Mathf.RoundToInt(damage);
        }

        public (int finalDamage, bool blocked, bool reflected) ApplyIncomingDamageModifiers(int baseDamage, IActor attacker)
        {
            float damage = baseDamage;
            bool blocked = false;
            bool reflected = false;

            // Damage immune
            if (HasEffect(StatusEffectType.DamageImmune))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName} is damage immune!");
                return (0, true, false);
            }

            // Shield Drone
            if (TryConsumeEffect(StatusEffectType.ShieldDrone))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName}'s shield drone blocks the attack!");
                return (0, true, false);
            }

            // Decoy Redirect
            if (TryConsumeEffect(StatusEffectType.DecoyRedirect))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName}'s decoy absorbs the attack!");
                return (0, true, false);
            }

            // Protection (flat absorption) — may be reduced by ryft effect
            var protection = GetEffect(StatusEffectType.Protection);
            if (protection != null && protection.Stacks > 0)
            {
                int effectiveProt = protection.Stacks;

                // Ryft passive: protection reduction % (only affects player)
                if (owner is Game.Player.PlayerCharacter)
                {
                    var ryftMgr = Game.Ryfts.RyftEffectManager.Instance;
                    if (ryftMgr != null)
                    {
                        float reductionPct = ryftMgr.SumFloat(Game.Ryfts.BuiltInOp.ProtectionReductionPercent);
                        if (reductionPct > 0f)
                            effectiveProt = Mathf.RoundToInt(effectiveProt * (1f - reductionPct));
                    }
                }

                int absorbed = Mathf.Min(effectiveProt, Mathf.RoundToInt(damage));
                protection.Stacks -= absorbed;
                damage -= absorbed;
                Debug.Log($"[Protection] {owner.DisplayName} absorbed {absorbed} damage ({protection.Stacks} remaining)");
                if (protection.Stacks <= 0) RemoveEffect(StatusEffectType.Protection);
            }

            // Stance damage reduction
            var stance = StanceManager.Instance;
            if (stance != null && owner is Game.Player.PlayerCharacter)
            {
                damage = Mathf.RoundToInt(damage * stance.IncomingDamageMultiplier);
            }

            // Damage Reduction (percentage)
            if (HasEffect(StatusEffectType.DamageReduction))
            {
                var reduction = GetEffect(StatusEffectType.DamageReduction);
                float reductionPercent = reduction.Value / 100f;
                damage *= (1f - reductionPercent);
            }

            // Defense Up
            if (HasEffect(StatusEffectType.DefenseUp))
            {
                var defense = GetEffect(StatusEffectType.DefenseUp);
                damage = Mathf.Max(0, damage - (defense.Stacks * defense.Value));
            }

            // Reflect
            if (HasEffect(StatusEffectType.Reflect))
            {
                reflected = true;
                if (attacker != null) attacker.ApplyDamage(Mathf.RoundToInt(damage * 0.5f));
            }

            // Reflect All
            if (HasEffect(StatusEffectType.ReflectAll))
            {
                reflected = true;
                if (attacker != null) attacker.ApplyDamage(Mathf.RoundToInt(damage));
            }

            // Flame Barrier
            if (HasEffect(StatusEffectType.FlameBarrier) && attacker != null)
            {
                CombatBuffManager.ApplyDebuff(attacker, StatusEffectType.Burning, 2);
            }

            // Ice Armor
            if (HasEffect(StatusEffectType.IceArmor) && attacker != null)
            {
                CombatBuffManager.ApplyDebuff(attacker, StatusEffectType.Frozen, 1);
            }

            // Counter
            if (TryConsumeEffect(StatusEffectType.Countering))
            {
                if (attacker != null) attacker.ApplyDamage(baseDamage);
            }

            return (Mathf.Max(0, Mathf.RoundToInt(damage)), blocked, reflected);
        }

        public bool ShouldBeTaunted()
        {
            return HasEffect(StatusEffectType.Taunt);
        }

        public bool TryConsumeEffect(StatusEffectType type)
        {
            var effect = GetEffect(type);
            if (effect != null)
            {
                effect.Stacks--;
                if (effect.Stacks <= 0) RemoveEffect(type);
                Debug.Log($"[StatusEffect] {owner.DisplayName} consumed {type}");
                return true;
            }
            return false;
        }
    }
}
