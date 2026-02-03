using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Orc Chieftain's war cry - buffs self and intimidates the player.
    /// </summary>
    public class WarCryAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            // Buff self
            int buffDuration = 3;
            int damageBoost = Mathf.Max(3, Def.power / 2);

            Owner.StatusEffects?.AddEffect(StatusEffectType.DefenseUp, buffDuration, 1, damageBoost);

            // Intimidate player (apply damage reduction debuff)
            var player = ctx.Player;
            if (player != null && player.IsAlive)
            {
                player.StatusEffects?.AddEffect(StatusEffectType.Slow, 2, 1, 2);
            }

            ctx.Log($"{Owner.DisplayName} lets out a terrifying WAR CRY! Gains +{damageBoost} damage, player is intimidated!");

            PutOnCooldown();
        }
    }
}
