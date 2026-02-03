using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    public abstract class ArcaneCopyCard : CardRuntime
    {

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var mgr = RyftEffectManager.Ensure();

            var clone = mgr.GetLastPlayedCardRuntime(onlyIfCostField: StatField.Energy);
            if (clone != null)
            {
                clone.Execute(ctx, explicitTarget);
                ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and replays the last card.");
            }
        }
    }
}
