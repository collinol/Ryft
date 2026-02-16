using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class ControlledFuryCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            StanceManager.Instance?.EnterStance(StanceType.Rage);
            GainProtection(8);

            ctx.Log($"{Owner.DisplayName} uses Controlled Fury. Rage Stance + 8 protection.");
        }
    }
}
