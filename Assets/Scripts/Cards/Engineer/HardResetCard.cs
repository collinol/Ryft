using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class HaveYouTriedTurningItOffAndTurningItBackOn : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int stacks = OverclockManager.Instance?.RemoveAllStacks() ?? 0;
            if (stacks > 0)
            {
                Owner.Heal(stacks);
                GainProtection(stacks);
            }
            ctx.Log($"{Owner.DisplayName} resets: removed {stacks} Overclock, healed {stacks}, gained {stacks} protection.");
        }
    }
}
