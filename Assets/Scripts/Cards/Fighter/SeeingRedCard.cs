using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class SeeingRedCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var stance = StanceManager.Instance;
            if (stance != null && stance.IsInStance(StanceType.Rage))
            {
                FightSceneController.Instance?.DrawCards(3);
                ctx.Log($"{Owner.DisplayName} is Seeing Red! Draws 3 cards.");
            }
            else
            {
                FightSceneController.Instance?.DrawCards(1);
                ctx.Log($"{Owner.DisplayName} uses Seeing Red. Draws 1 card.");
            }
        }
    }
}
