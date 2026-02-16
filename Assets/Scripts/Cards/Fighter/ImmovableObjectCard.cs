using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class ImmovableObjectCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects?.AddEffect(StatusEffectType.DamageImmune, 1);
            ctx.Log($"{Owner.DisplayName} uses Immovable Object, becoming immune to all damage for 1 turn.");
        }
    }
}
