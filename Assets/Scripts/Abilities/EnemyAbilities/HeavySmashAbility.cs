using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.RyftEntities;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// High damage attack used by elite enemies.
    /// </summary>
    public class HeavySmashAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = explicitTarget ?? ctx.GetEnemyPrimaryTarget();
            if (target == null || !target.IsAlive) return;

            // Heavy smash deals 2x power damage
            var baseDmg = Mathf.Max(1, Def.power) * 2;

            var finalDmg = baseDmg;
            if (Owner != null && Owner.StatusEffects != null)
            {
                finalDmg = Owner.StatusEffects.ApplyOutgoingDamageModifiers(baseDmg);
            }

            if (target is Game.Player.PlayerCharacter player)
            {
                player.ApplyDamage(finalDmg, Owner);
                ctx.OnPlayerDamagedBy(attacker: Owner, damage: finalDmg);
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

            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {finalDmg} HEAVY damage on {target.DisplayName}!");

            PutOnCooldown();
        }
    }
}
