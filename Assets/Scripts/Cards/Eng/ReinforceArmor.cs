using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Reinforce Armor - Deploy shield drone that blocks next attack.
    /// </summary>
    public class ReinforceArmor : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} deploys a shield drone! Next attack blocked.");
            Owner.StatusEffects.AddEffect(StatusEffectType.ShieldDrone, duration: -1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
