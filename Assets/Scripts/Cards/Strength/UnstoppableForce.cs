using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Unstoppable Force - For each Strength spent this turn, refund 1 on kill.
    /// Enables self-sustaining chain kills.
    /// </summary>
    public class UnstoppableForce : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} becomes an unstoppable force! Kills refund Strength spent.");
            // TODO: Track Strength spent and refund on kills
        }
    }
}
