using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Recursive AI - Your next gadget's effect triggers again automatically.
    /// Stack → double every turn → runaway growth.
    /// </summary>
    public class RecursiveAI : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Recursive AI! Next gadget triggers twice.");
            Owner.StatusEffects.AddEffect(StatusEffectType.DoubleNextGadget, duration: -1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
