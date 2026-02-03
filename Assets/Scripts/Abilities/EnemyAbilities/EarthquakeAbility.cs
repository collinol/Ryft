using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.RyftEntities;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// AoE attack that damages the player and potentially knocks them off balance (applies slow).
    /// </summary>
    public class EarthquakeAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var baseDmg = Mathf.Max(1, Def.power);

            var finalDmg = baseDmg;
            if (Owner != null && Owner.StatusEffects != null)
            {
                finalDmg = Owner.StatusEffects.ApplyOutgoingDamageModifiers(baseDmg);
            }

            // Damage player
            var player = ctx.Player;
            if (player != null && player.IsAlive)
            {
                player.ApplyDamage(finalDmg, Owner);
                ctx.OnPlayerDamagedBy(attacker: Owner, damage: finalDmg);

                // 50% chance to slow the player
                if (Random.value < 0.5f)
                {
                    player.StatusEffects?.AddEffect(StatusEffectType.Slow, 2, 1, 2);
                    ctx.Log($"{player.DisplayName} is knocked off balance!");
                }
            }

            // Also damage portal if present
            var portal = ctx.RyftPortal;
            if (portal != null && portal.IsAlive)
            {
                portal.ApplyDamage(finalDmg / 2);
            }

            ctx.Log($"{Owner.DisplayName} causes an EARTHQUAKE dealing {finalDmg} damage!");

            PutOnCooldown();
        }
    }
}
