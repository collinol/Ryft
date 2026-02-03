using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Mirror Shield - Reflect next spell.
    /// </summary>
    public class MirrorShield : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} raises a Mirror Shield! Next spell will be reflected.");
            Owner.StatusEffects.AddEffect(StatusEffectType.Reflect, duration: -1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
