// Game.Cards.OverclockRelayCard
using Game.Core; using Game.Combat; using Game.Ryfts;

namespace Game.Cards
{
    public abstract class OverclockRelayCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            RyftEffectManager.Ensure().RegisterRefundChance(StatField.Engineering, chancePct: Def.power); // e.g., 25 = 25%
            ctx.Log($"{Owner.DisplayName} deploys {Def.displayName}: {Def.power}% chance to refund Engineering when spent.");
        }
    }
}
