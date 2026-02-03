using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.RyftEntities;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Deals damage and has a chance to stun the target.
    /// </summary>
    public class ShieldBashAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = explicitTarget ?? ctx.GetEnemyPrimaryTarget();
            if (target == null || !target.IsAlive) return;

            var baseDmg = Mathf.Max(1, Def.power);

            var finalDmg = baseDmg;
            if (Owner != null && Owner.StatusEffects != null)
            {
                finalDmg = Owner.StatusEffects.ApplyOutgoingDamageModifiers(baseDmg);
            }

            // Apply damage
            if (target is Game.Player.PlayerCharacter player)
            {
                player.ApplyDamage(finalDmg, Owner);
                ctx.OnPlayerDamagedBy(attacker: Owner, damage: finalDmg);

                // 30% chance to stun player
                if (Random.value < 0.3f)
                {
                    player.StatusEffects?.AddEffect(StatusEffectType.Stun, 1, 1, 0);
                    ctx.Log($"{target.DisplayName} is stunned!");
                }
            }
            else if (target is Game.Enemies.EnemyBase enemy)
            {
                enemy.ApplyDamage(finalDmg, Owner);
            }
            else if (target is RyftPortalEntity portal)
            {
                portal.ApplyDamage(finalDmg);
            }
            else
            {
                target.ApplyDamage(finalDmg);
            }

            ctx.Log($"{Owner.DisplayName} shield bashes {target.DisplayName} for {finalDmg} damage!");

            PutOnCooldown();
        }
    }
}
