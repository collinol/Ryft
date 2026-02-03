using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Cards;
using Game.RyftEntities;

namespace Game.Abilities.EnemyAbilities
{

    public class EnemyStrikeAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            // Use primary target (portal if present, otherwise player)
            var target = explicitTarget ?? ctx.GetEnemyPrimaryTarget();
            if (target == null || !target.IsAlive) return;

            var baseDmg = 1;//Mathf.Max(1, Def.power);

            // Apply owner's status effect modifiers (e.g., Slow reduces damage)
            var finalDmg = baseDmg;
            if (Owner != null && Owner.StatusEffects != null)
            {
                finalDmg = Owner.StatusEffects.ApplyOutgoingDamageModifiers(baseDmg);
            }

            // Apply damage with attacker info so defender can apply their status effects
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

            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {finalDmg} damage on {target.DisplayName}.");

            PutOnCooldown();
        }
    }
}
