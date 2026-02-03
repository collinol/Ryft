using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Taunt - Force all enemies to attack you next turn.
    /// </summary>
    public class Taunt : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} taunts all enemies to attack them next turn!");
            Owner.StatusEffects.AddEffect(StatusEffectType.Taunt, duration: 1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
