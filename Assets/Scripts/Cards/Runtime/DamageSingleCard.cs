using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    public abstract class DamageSingleCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();
            dmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, target);
            target.ApplyDamage(dmg);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage on {target.DisplayName}.");
        }
        public override TargetingType Targeting => TargetingType.SingleEnemy;
    }
}
