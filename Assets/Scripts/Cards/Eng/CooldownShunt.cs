using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Cooldown Shunt - Resets a gadget's cooldown when another is activated.
    /// Enables infinite trigger loops.
    /// </summary>
    public class CooldownShunt : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} installs Cooldown Shunt! Gadget activation resets other cooldowns.");

            // Register gadget deployment listener
            var gadgetManager = GadgetManager.Instance;
            if (gadgetManager != null)
            {
                gadgetManager.OnGadgetDeployed += (gadget) =>
                {
                    // Check if gadget was deployed by the owner
                    if (ReferenceEquals(gadget.Owner, Owner))
                    {
                        // Reset a random cooldown
                        var cooldownManager = CardCooldownManager.Instance;
                        if (cooldownManager != null)
                        {
                            // For now, reset all cooldowns as we don't have random selection implemented
                            cooldownManager.ResetAllCooldowns();
                            ctx.Log($"Cooldown Shunt triggered! Cooldowns reset!");
                        }
                    }
                };
            }
        }
    }
}
