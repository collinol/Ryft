using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Failsafe Protocol - Upon death, restore all drones and full HP.
    /// With any self-damage trigger, creates infinite resets.
    /// </summary>
    public class FailsafeProtocol : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        private bool used = false;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var deathPrevention = DeathPreventionSystem.Instance;
            if (deathPrevention != null)
            {
                deathPrevention.RegisterPrevention(Owner, DeathPreventionType.FailsafeProtocol, Def.id, (actor) => {
                    // Restore all drones (TODO: implement drone restoration when drone system exists)
                    ctx.Log($"{Owner.DisplayName}'s failsafe activated! All drones restored.");
                });
                ctx.Log($"{Owner.DisplayName} activates Failsafe Protocol! Will restore to full HP and restore drones upon death.");
            }
            used = false;
        }
    }
}
