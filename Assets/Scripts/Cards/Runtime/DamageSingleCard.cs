using UnityEngine;
using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public class DamageSingleCard : CardRuntime
    {
        protected override StatField CostField => StatField.Strength;
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerMaxFor(CostField);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            target.ApplyDamage(dmg);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage on {target.DisplayName}.");
        }
        public override TargetingType Targeting => TargetingType.SingleEnemy;
    }
}
