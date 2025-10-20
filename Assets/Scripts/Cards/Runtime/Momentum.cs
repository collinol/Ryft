// Game.Cards.MomentumCard
using Game.Core; using Game.Combat; using Game.Ryfts;

namespace Game.Cards
{
    public class MomentumCard : CardRuntime
    {
        protected override StatField CostField => StatField.Strength;
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            var mgr = RyftEffectManager.Ensure();
            mgr.RegisterCostReducer(StatField.Strength, reduceBy: Def.power, minCost: 0); // implement reducer in manager
            ctx.Log($"{Owner.DisplayName} builds {Def.displayName}: Strength cards cost -{Def.power} this turn (min 0).");
        }
    }
}
