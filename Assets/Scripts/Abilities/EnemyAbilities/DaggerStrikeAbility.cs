using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.RyftEntities;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Quick dagger strike - moderate damage with chance to attack twice.
    /// </summary>
    public class DaggerStrikeAbility : AbilityRuntime
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

            // First strike
            ApplyDamageToTarget(target, finalDmg, ctx);

            // 40% chance for a second strike
            if (Random.value < 0.4f && target.IsAlive)
            {
                ApplyDamageToTarget(target, finalDmg, ctx);
                ctx.Log($"{Owner.DisplayName} strikes again!");
            }

            PutOnCooldown();
        }

        private void ApplyDamageToTarget(IActor target, int damage, FightContext ctx)
        {
            if (target is Game.Player.PlayerCharacter player)
            {
                player.ApplyDamage(damage, Owner);
                ctx.OnPlayerDamagedBy(attacker: Owner, damage: damage);
            }
            else if (target is Game.Enemies.EnemyBase enemy)
            {
                enemy.ApplyDamage(damage, Owner);
            }
            else if (target is RyftPortalEntity portal)
            {
                portal.ApplyDamage(damage);
            }
            else
            {
                target.ApplyDamage(damage);
            }

            ctx.Log($"{Owner.DisplayName} stabs {target.DisplayName} for {damage} damage!");
        }
    }
}
