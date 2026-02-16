using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class TauntCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects?.AddEffect(StatusEffectType.Taunt, 1);
            int prot = 6 + GetStatValue();
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} uses Taunt, forcing all enemies to target them and gaining {prot} protection.");
        }
    }
}
