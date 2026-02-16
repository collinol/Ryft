using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class TimeDilationFieldCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int stacks = 30 + GetStatValue();
            OverclockManager.Instance?.AddStacks(stacks);
            ctx.Log($"{Owner.DisplayName} activates Time Dilation Field: +{stacks} Overclock stacks.");
        }
    }
}
