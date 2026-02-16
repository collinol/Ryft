using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Static helpers for applying debuffs, protection, and other combat effects.
    /// Centralizes Elemental Mastery interaction and common patterns.
    /// </summary>
    public static class CombatBuffManager
    {
        /// <summary>
        /// Apply a debuff to a target. Respects Elemental Mastery (+2 stacks on next debuff).
        /// </summary>
        public static void ApplyDebuff(IActor target, StatusEffectType debuffType, int stacks, IActor source = null)
        {
            if (target?.StatusEffects == null) return;

            // Check if source has Elemental Mastery
            if (source?.StatusEffects != null && source.StatusEffects.HasEffect(StatusEffectType.ElementalMastery))
            {
                stacks += 2;
                source.StatusEffects.RemoveEffect(StatusEffectType.ElementalMastery);
                Debug.Log("[ElementalMastery] +2 bonus debuff stacks consumed.");
            }

            // Debuffs use -1 duration (permanent, managed by their own decay logic)
            target.StatusEffects.AddEffect(debuffType, -1, stacks);
        }

        /// <summary>
        /// Gain protection (flat damage absorption).
        /// </summary>
        public static void GainProtection(IActor target, int amount)
        {
            if (target?.StatusEffects == null || amount <= 0) return;
            target.StatusEffects.AddEffect(StatusEffectType.Protection, -1, amount);
            Debug.Log($"[Protection] {target.DisplayName} gains {amount} protection");
        }

        /// <summary>
        /// Get current protection stacks on an actor.
        /// </summary>
        public static int GetProtection(IActor actor)
        {
            return actor?.StatusEffects?.GetStacks(StatusEffectType.Protection) ?? 0;
        }

        /// <summary>
        /// Remove all protection from an actor. Called at end of player turn.
        /// </summary>
        public static void ClearProtection(IActor actor)
        {
            actor?.StatusEffects?.RemoveEffect(StatusEffectType.Protection);
        }

        /// <summary>
        /// Get total stacks of a debuff across all alive enemies.
        /// </summary>
        public static int GetTotalDebuffStacksOnAllEnemies(FightContext ctx, StatusEffectType type)
        {
            int total = 0;
            foreach (var e in ctx.AllAliveEnemies())
            {
                total += e.StatusEffects?.GetStacks(type) ?? 0;
            }
            return total;
        }
    }
}
