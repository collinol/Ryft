using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Types of status effects that can be applied to actors
    /// </summary>
    public enum StatusEffectType
    {
        Stun,              // Cannot act
        Slow,              // Reduced actions
        DefenseUp,         // Increased defense
        DamageReduction,   // Reduce incoming damage by %
        Reflect,           // Reflect damage back
        ReflectMagic,      // Reflect magic damage
        ReflectRanged,     // Reflect ranged damage
        Taunt,             // Force enemies to target this actor
        BlockRanged,       // Block ranged attacks
        ShieldDrone,       // Block next attack completely
        DecoyRedirect,     // Redirect next attack to decoy
        Countering,        // Counter next attack
        DoubleNextGadget,  // Next gadget triggers twice
        FreeNextEngCard,   // Next Engineering card is free
    }

    /// <summary>
    /// A status effect instance on an actor
    /// </summary>
    [System.Serializable]
    public class StatusEffect
    {
        public StatusEffectType Type;
        public int Duration;        // -1 = permanent, 0 = expired, >0 = turns remaining
        public int Stacks;          // How many stacks of this effect
        public float Value;         // Numeric value (e.g., damage reduction %, defense amount)
        public string SourceId;     // What card/ability caused this

        public StatusEffect(StatusEffectType type, int duration, int stacks = 1, float value = 0f, string sourceId = "")
        {
            Type = type;
            Duration = duration;
            Stacks = stacks;
            Value = value;
            SourceId = sourceId;
        }

        /// <summary>Tick down the duration by 1 turn. Returns true if still active.</summary>
        public bool TickDuration()
        {
            if (Duration > 0) Duration--;
            return Duration != 0;
        }
    }

    /// <summary>
    /// Manager for status effects on an actor
    /// </summary>
    public class StatusEffectManager
    {
        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();
        private readonly IActor owner;

        public StatusEffectManager(IActor owner)
        {
            this.owner = owner;
        }

        /// <summary>Add a new status effect</summary>
        public void AddEffect(StatusEffectType type, int duration, int stacks = 1, float value = 0f, string sourceId = "")
        {
            // Check if we already have this effect type
            var existing = activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                // Refresh duration and add stacks
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

        /// <summary>Remove an effect by type</summary>
        public void RemoveEffect(StatusEffectType type)
        {
            activeEffects.RemoveAll(e => e.Type == type);
        }

        /// <summary>Check if actor has a specific effect</summary>
        public bool HasEffect(StatusEffectType type)
        {
            return activeEffects.Exists(e => e.Type == type);
        }

        /// <summary>Get effect by type (null if not found)</summary>
        public StatusEffect GetEffect(StatusEffectType type)
        {
            return activeEffects.Find(e => e.Type == type);
        }

        /// <summary>Get all active effects</summary>
        public IReadOnlyList<StatusEffect> GetActiveEffects() => activeEffects;

        /// <summary>Count total number of active buffs</summary>
        public int CountBuffs()
        {
            int count = 0;
            foreach (var effect in activeEffects)
            {
                if (IsBuff(effect.Type))
                    count += effect.Stacks;
            }
            return count;
        }

        /// <summary>Tick all effects at end of turn</summary>
        public void TickAllEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (!activeEffects[i].TickDuration())
                {
                    Debug.Log($"[StatusEffect] {owner.DisplayName} lost {activeEffects[i].Type}");
                    activeEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>Clear all effects</summary>
        public void ClearAll()
        {
            activeEffects.Clear();
        }

        /// <summary>Check if an effect type is a buff (vs debuff)</summary>
        private bool IsBuff(StatusEffectType type)
        {
            return type != StatusEffectType.Stun && type != StatusEffectType.Slow;
        }

        /// <summary>
        /// Apply outgoing damage modifiers based on active effects
        /// </summary>
        public int ApplyOutgoingDamageModifiers(int baseDamage)
        {
            float damage = baseDamage;

            // Slow reduces damage output
            if (HasEffect(StatusEffectType.Slow))
            {
                var slow = GetEffect(StatusEffectType.Slow);
                damage *= 0.75f; // 25% damage reduction when slowed
                Debug.Log($"[StatusEffect] {owner.DisplayName} slowed: damage reduced to {damage}");
            }

            return Mathf.RoundToInt(damage);
        }

        /// <summary>
        /// Apply incoming damage modifiers based on active effects (defense, reduction, shields)
        /// Returns the modified damage and whether the attack was blocked/reflected
        /// </summary>
        public (int finalDamage, bool blocked, bool reflected) ApplyIncomingDamageModifiers(int baseDamage, IActor attacker)
        {
            float damage = baseDamage;
            bool blocked = false;
            bool reflected = false;

            // Shield Drone - block next attack completely
            if (TryConsumeEffect(StatusEffectType.ShieldDrone))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName}'s shield drone blocks the attack!");
                return (0, true, false);
            }

            // Decoy Redirect - redirect to decoy
            if (TryConsumeEffect(StatusEffectType.DecoyRedirect))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName}'s decoy absorbs the attack!");
                return (0, true, false);
            }

            // Damage Reduction
            if (HasEffect(StatusEffectType.DamageReduction))
            {
                var reduction = GetEffect(StatusEffectType.DamageReduction);
                float reductionPercent = reduction.Value / 100f;
                damage *= (1f - reductionPercent);
                Debug.Log($"[StatusEffect] {owner.DisplayName} damage reduction: {baseDamage} -> {damage}");
            }

            // Defense Up (flat reduction per stack)
            if (HasEffect(StatusEffectType.DefenseUp))
            {
                var defense = GetEffect(StatusEffectType.DefenseUp);
                damage = Mathf.Max(1, damage - (defense.Stacks * defense.Value));
                Debug.Log($"[StatusEffect] {owner.DisplayName} defense up: reduced by {defense.Stacks * defense.Value}");
            }

            // Reflect effects
            if (HasEffect(StatusEffectType.Reflect))
            {
                reflected = true;
                Debug.Log($"[StatusEffect] {owner.DisplayName} reflects damage back to attacker!");
                if (attacker != null)
                {
                    attacker.ApplyDamage(Mathf.RoundToInt(damage * 0.5f)); // Reflect 50% of damage
                }
            }

            // Counter - damage attacker and consume effect
            if (TryConsumeEffect(StatusEffectType.Countering))
            {
                Debug.Log($"[StatusEffect] {owner.DisplayName} counters the attack!");
                if (attacker != null)
                {
                    attacker.ApplyDamage(baseDamage); // Counter for full damage
                }
            }

            return (Mathf.RoundToInt(damage), blocked, reflected);
        }

        /// <summary>
        /// Check if this actor should be prioritized as a target (Taunt)
        /// </summary>
        public bool ShouldBeTaunted()
        {
            return HasEffect(StatusEffectType.Taunt);
        }

        /// <summary>Consume a one-time effect (like shield or counter)</summary>
        public bool TryConsumeEffect(StatusEffectType type)
        {
            var effect = GetEffect(type);
            if (effect != null)
            {
                effect.Stacks--;
                if (effect.Stacks <= 0)
                {
                    RemoveEffect(type);
                }
                Debug.Log($"[StatusEffect] {owner.DisplayName} consumed {type}");
                return true;
            }
            return false;
        }
    }
}
