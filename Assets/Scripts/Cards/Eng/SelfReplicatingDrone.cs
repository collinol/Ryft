using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Self-Replicating Drone - Summons a drone that duplicates itself if you still have Engineering left.
    /// With refunds, leads to exponential scaling.
    /// </summary>
    public class SelfReplicatingDrone : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var player = Owner as PlayerCharacter;
            if (player == null) return;

            int engineeringPower = player.CurrentTurnStats.engineering;

            // Deploy the initial drone
            var gadgetManager = GadgetManager.Instance;
            if (gadgetManager != null)
            {
                gadgetManager.DeployGadget(GadgetType.Drone, Owner, engineeringPower, -1, Def.id);
                ctx.Log($"{Owner.DisplayName} deploys a Self-Replicating Drone with power {engineeringPower}!");

                // Check if we still have Engineering to duplicate
                if (player.CurrentTurnStats.engineering > 0)
                {
                    // Deploy another drone (duplication)
                    gadgetManager.DeployGadget(GadgetType.Drone, Owner, engineeringPower, -1, Def.id);
                    ctx.Log($"The drone replicates itself! Another drone deployed!");
                }
            }
        }
    }
}
