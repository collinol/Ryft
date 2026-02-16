using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class CreepingColdCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects?.AddEffect(StatusEffectType.CreepingCold, 3);

            ctx.Log($"{Owner.DisplayName} uses Creeping Cold. All enemies will gain 2 Frozen at the start of each turn for 3 turns.");
        }
    }
}
