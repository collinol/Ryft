// Game.Cards.ManaEchoCard
using Game.Core; using Game.Combat; using Game.Ryfts;

namespace Game.Cards
{
    public class ManaEchoCard : CardRuntime
    {
        protected override StatField CostField => StatField.Mana;
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            // Register a temporary hook: next N (power) Mana costs are refunded immediately.
            var mgr = RyftEffectManager.Ensure();
            mgr.RegisterTemporaryRefund(StatField.Mana, count: Def.power); // implement as you already do for refunds
            ctx.Log($"{Owner.DisplayName} invokes {Def.displayName}: next {Def.power} Mana costs are refunded.");
        }
    }
}
