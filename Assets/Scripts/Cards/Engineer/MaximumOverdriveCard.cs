using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class MaximumOverdriveCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            OverclockManager.Instance?.DoubleStacks();
            ctx.Log($"{Owner.DisplayName} doubles Overclock stacks!");
        }
    }
}
