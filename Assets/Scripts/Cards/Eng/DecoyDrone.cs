using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Decoy Drone - Redirect first incoming attack to drone.
    /// </summary>
    public class DecoyDrone : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} deploys a Decoy Drone! Next attack redirected.");
            Owner.StatusEffects.AddEffect(StatusEffectType.DecoyRedirect, duration: -1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
