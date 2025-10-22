using Game.Core; using Game.Combat; using Game.Ryfts;

namespace Game.Cards
{
    public abstract class ManaEchoCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            // Register a temporary hook: next N (power) Mana costs are refunded immediately.
            var mgr = RyftEffectManager.Ensure();
            mgr.RegisterTemporaryRefund(StatField.Mana, count: Def.power); // implement as you already do for refunds
            ctx.Log($"{Owner.DisplayName} invokes {Def.displayName}: next {Def.power} Mana costs are refunded.");
        }
    }
}
