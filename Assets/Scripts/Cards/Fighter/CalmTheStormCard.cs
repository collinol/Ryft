using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class CalmTheStormCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var stance = StanceManager.Instance;
            if (stance != null && stance.IsInStance(StanceType.Rage))
                stance.ExitStance();
            Owner.Heal(10);

            ctx.Log($"{Owner.DisplayName} calms the storm. Exits Rage Stance and heals 10 HP.");
        }
    }
}
