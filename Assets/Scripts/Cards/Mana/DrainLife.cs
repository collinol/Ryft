using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Drain Life - Deal 3 damage and heal for same.
    /// </summary>
    public class DrainLife : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 3;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.SingleEnemy;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();
            dmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, target);
            DealDamage(target, dmg, ScalingStat);

            Owner.Heal(dmg);
            ctx.Log($"{Owner.DisplayName} drains {dmg} life from {target.DisplayName}!");
        }
    }
}
