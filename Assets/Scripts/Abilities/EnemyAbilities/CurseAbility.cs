using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Applies a debuff to the player that reduces their damage output.
    /// </summary>
    public class CurseAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = ctx.Player;
            if (target == null || !target.IsAlive) return;

            int curseDuration = Mathf.Max(2, Def.power / 5);
            int damageReduction = Mathf.Max(1, Def.power / 3);

            // Apply a debuff using Slow (reduces outgoing damage)
            target.StatusEffects?.AddEffect(StatusEffectType.Slow, curseDuration, 1, damageReduction);

            ctx.Log($"{Owner.DisplayName} curses {target.DisplayName}! Damage reduced by {damageReduction} for {curseDuration} turns.");

            PutOnCooldown();
        }
    }
}
