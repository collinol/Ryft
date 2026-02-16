using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class BreakStanceCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var stance = StanceManager.Instance;
            if (stance != null && stance.IsInStance(StanceType.Defensive))
                stance.ExitStance();
            Owner.StatusEffects?.AddEffect(StatusEffectType.NextAttackDouble, 1, 1);
            ctx.Log($"{Owner.DisplayName} uses Break Stance, exits Defensive Stance and empowers next attack to deal double damage.");
        }
    }
}
