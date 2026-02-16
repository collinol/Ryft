using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class EnterTheFrenzyCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var stance = StanceManager.Instance;
            if (stance != null)
            {
                if (stance.IsInStance(StanceType.Defensive))
                    stance.ExitStance();

                stance.EnterStance(StanceType.Rage, 3);
            }

            ctx.Log($"{Owner.DisplayName} enters the frenzy! Rage Stance for 3 turns.");
        }
    }
}
