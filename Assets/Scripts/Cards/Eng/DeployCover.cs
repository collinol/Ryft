using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Deploy Cover - Gain +2 Defense and block ranged attacks for 1 turn.
    /// </summary>
    public class DeployCover : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} deploys cover! +2 Defense and blocks ranged attacks.");
            Owner.StatusEffects.AddEffect(StatusEffectType.DefenseUp, duration: 1, stacks: 1, value: 2f, sourceId: Def.id);
            Owner.StatusEffects.AddEffect(StatusEffectType.BlockRanged, duration: 1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
