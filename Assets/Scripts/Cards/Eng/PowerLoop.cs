using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Power Loop - Whenever you refund Engineering, next Engineering card is free.
    /// </summary>
    public class PowerLoop : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} establishes a Power Loop! Refunds make next card free.");
            Owner.StatusEffects.AddEffect(StatusEffectType.FreeNextEngCard, duration: -1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
