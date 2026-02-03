using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Counterstrike - Deal 3 damage when attacked next turn.
    /// </summary>
    public class Counterstrike : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} prepares to counterstrike!");
            Owner.StatusEffects.AddEffect(StatusEffectType.Countering, duration: 1, stacks: 1, value: 3f, sourceId: Def.id);
        }
    }
}
