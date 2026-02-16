using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class TurtleStanceCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            ctx.Log($"{Owner.DisplayName} uses Turtle Stance and enters Defensive Stance.");
        }
    }
}
