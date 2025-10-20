using UnityEngine;
using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public class HealCard : CardRuntime
    {
        protected override StatField CostField => StatField.Mana;
        protected override int GetBaseCostAmount(StatField field) => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            var target = explicitTarget ?? ctx.PlayerActor;
            int stat = GetOwnerMaxFor(CostField);
            int heal  = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            target.Heal(heal);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and heals {heal} HP.");
        }
        public override TargetingType Targeting => TargetingType.Self;
    }
}