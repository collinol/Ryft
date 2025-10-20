using UnityEngine;
using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public class DamageAllCard : CardRuntime
    {
        protected override StatField CostField => StatField.Engineering;
        protected override int GetBaseCostAmount(StatField field) => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            int stat = GetOwnerMaxFor(CostField);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());

            int hits = 0;
            foreach (var e in ctx.AllAliveEnemies()) { e.ApplyDamage(dmg); hits++; }
            if (hits > 0)
                ctx.Log($"{Owner.DisplayName} uses {Def.displayName}, dealing {dmg} to all enemies ({hits}).");
        }

        public override TargetingType Targeting => TargetingType.AllEnemies;
    }
}
