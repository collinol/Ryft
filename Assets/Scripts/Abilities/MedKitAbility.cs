using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities
{
    public class MedKitAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            var healTarget = Owner;
            var heal = Mathf.Max(1, Def.power + Owner.TotalStats.defense * Def.scaling);
            healTarget.Heal(heal);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and heals {heal}.");

            PutOnCooldown();
        }
    }
}
