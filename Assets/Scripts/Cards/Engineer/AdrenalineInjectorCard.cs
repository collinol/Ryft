using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class AdrenalineInjectorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int stacks = 5 + GetStatValue();
            OverclockManager.Instance?.AddStacks(stacks);
            FightSceneController.Instance?.DrawCards(2);
            ctx.Log($"{Owner.DisplayName} injects adrenaline: +{stacks} Overclock, draw 2.");
        }
    }
}
