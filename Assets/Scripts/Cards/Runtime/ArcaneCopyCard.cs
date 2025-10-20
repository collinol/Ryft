// Game.Cards.ArcaneCopyCard
using Game.Core; using Game.Combat; using Game.Ryfts;

namespace Game.Cards
{
    public class ArcaneCopyCard : CardRuntime
    {
        protected override StatField CostField => StatField.Mana;
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            var mgr = RyftEffectManager.Ensure();
            var clone = mgr.GetLastPlayedCardRuntime(onlyIfCostField: StatField.Mana);
            if (clone != null)
            {
                clone.Execute(ctx, explicitTarget); // re-executes last Mana spell (watch for reentrancy)
                ctx.Log($"{Owner.DisplayName} casts {Def.displayName} and replays the last spell.");
            }
        }
    }
}
