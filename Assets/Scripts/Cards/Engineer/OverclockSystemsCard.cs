using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class OverclockSystemsCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int stacks = 3 + GetStatValue();
            OverclockManager.Instance?.AddStacks(stacks);
            ctx.Log($"{Owner.DisplayName} gains {stacks} Overclock stacks.");
        }
    }
}
