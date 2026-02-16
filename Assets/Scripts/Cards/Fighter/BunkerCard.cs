using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class BunkerCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            GainProtection(10);
            Owner.StatusEffects?.AddEffect(StatusEffectType.CannotAttack, 1);
            ctx.Log($"{Owner.DisplayName} uses Bunker, enters Defensive Stance, gains 10 protection, but cannot attack this turn.");
        }
    }
}
