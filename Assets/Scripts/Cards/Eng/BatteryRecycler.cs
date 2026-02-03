using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Battery Recycler - When a gadget is destroyed, refund its full cost.
    /// </summary>
    public class BatteryRecycler : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Battery Recycler! Gadgets refund full cost when destroyed.");

            // Register gadget destruction listener
            var gadgetManager = GadgetManager.Instance;
            if (gadgetManager != null)
            {
                gadgetManager.OnGadgetDestroyed += (gadget) =>
                {
                    // Check if gadget was owned by the player
                    if (ReferenceEquals(gadget.Owner, Owner))
                    {
                        // Refund Engineering based on gadget power
                        var player = Owner as PlayerCharacter;
                        if (player != null)
                        {
                            int refundAmount = gadget.Power;
                            player.Gain(new Stats { engineering = refundAmount }, allowExceedCap: false);
                            ctx.Log($"Battery Recycler triggered! Refunded {refundAmount} Engineering!");
                        }
                    }
                };
            }
        }
    }
}
