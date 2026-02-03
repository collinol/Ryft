using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Buffs all allied enemies with increased damage for a duration.
    /// </summary>
    public class RallyAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            // Apply damage buff to all alive enemies
            int buffDuration = Mathf.Max(2, Def.power / 5);
            int damageBoost = Mathf.Max(1, Def.power / 2);

            int buffedCount = 0;
            foreach (var enemy in ctx.Enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    // Use DefenseUp as a damage boost (we'll check for it in outgoing modifiers)
                    enemy.StatusEffects?.AddEffect(StatusEffectType.DefenseUp, buffDuration, 1, damageBoost);
                    buffedCount++;
                }
            }

            ctx.Log($"{Owner.DisplayName} rallies allies! {buffedCount} enemies gain +{damageBoost} damage for {buffDuration} turns.");

            PutOnCooldown();
        }
    }
}
