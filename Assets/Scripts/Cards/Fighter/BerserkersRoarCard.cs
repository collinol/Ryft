using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class BerserkersRoarCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        protected override int GetEnergyCost() => 0;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            StanceManager.Instance?.EnterStance(StanceType.Rage);
            FightSceneController.Instance?.DrawCards(2);
            Owner.ApplyDamage(5);

            ctx.Log($"{Owner.DisplayName} lets out a Berserker's Roar! Rage Stance, draws 2, takes 5 damage.");
        }
    }
}
