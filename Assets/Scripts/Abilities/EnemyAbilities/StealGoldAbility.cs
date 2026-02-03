using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Bandit ability - deals minor damage and steals gold from the player.
    /// </summary>
    public class StealGoldAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = ctx.Player;
            if (target == null || !target.IsAlive) return;

            // Minor damage
            var baseDmg = Mathf.Max(1, Def.power / 2);
            var finalDmg = baseDmg;
            if (Owner != null && Owner.StatusEffects != null)
            {
                finalDmg = Owner.StatusEffects.ApplyOutgoingDamageModifiers(baseDmg);
            }

            target.ApplyDamage(finalDmg, Owner);
            ctx.OnPlayerDamagedBy(attacker: Owner, damage: finalDmg);

            // Steal gold
            int stolenGold = Mathf.Min(Def.power, MapSession.I?.Gold ?? 0);
            if (stolenGold > 0 && MapSession.I != null)
            {
                MapSession.I.Gold -= stolenGold;
                ctx.Log($"{Owner.DisplayName} steals {stolenGold} gold from {target.DisplayName}!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} attacks {target.DisplayName} for {finalDmg} damage but finds no gold to steal.");
            }

            PutOnCooldown();
        }
    }
}
