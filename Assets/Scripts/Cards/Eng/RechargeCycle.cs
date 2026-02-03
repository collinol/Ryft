using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Recharge Cycle - Whenever you play an Engineering card, gain +1 Energy.
    /// With energy → cost conversions, loops forever.
    /// </summary>
    public class RechargeCycle : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} initiates Recharge Cycle! Engineering cards grant +1 Energy.");
            void OnEngCardPlayed(CardDef card, IActor player)
            {
                if (player == Owner)
                {
                    var fsc = FightSceneController.Instance;
                    if (fsc != null)
                    {
                        // Gain +1 energy (up to max)
                        int current = fsc.CurrentEnergy;
                        int max = fsc.MaxEnergy;
                        if (current < max)
                        {
                            // Use reflection to access SetEnergy
                            var method = fsc.GetType().GetMethod("SetEnergy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            method?.Invoke(fsc, new object[] { current + 1 });
                        }
                    }
                }
            }
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                tracker.OnEngineeringCardPlayed += OnEngCardPlayed;
            }
            ctx.Log($"{Owner.DisplayName} activates Recharge Cycle! Will gain +1 Energy each Engineering card played.");
        }
    }
}
