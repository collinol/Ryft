using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities
{
    // Simple: damage first alive enemy or the passed explicit target
    public class ShootAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            var strength = Owner.TotalStats.strength;
            var dmg = Mathf.Max(1, Def.power + strength * Def.scaling);
            target.ApplyDamage(dmg);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage on {target.DisplayName}.");

            PutOnCooldown();
        }
    }
}
