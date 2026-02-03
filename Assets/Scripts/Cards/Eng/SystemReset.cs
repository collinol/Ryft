using Game.Core;
using Game.Combat;
using Game.Ryfts;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// System Reset - If a gadget kills an enemy, restore all Engineering.
    /// Infinite explosion loops.
    /// </summary>
    public class SystemReset : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} primes System Reset! Gadget kills restore all Engineering.");

            // Register kill listener for gadget kills (Engineering damage type)
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                tracker.OnKill += (killer, victim, damage) =>
                {
                    // Check if this was an engineering kill by the owner
                    if (ReferenceEquals(killer, Owner))
                    {
                        var engineeringKills = tracker.GetEngineeringKillsThisTurn();
                        if (engineeringKills > 0)
                        {
                            // Restore all Engineering
                            var player = Owner as PlayerCharacter;
                            if (player != null)
                            {
                                var totalEng = player.TotalStats.engineering;
                                player.Gain(new Stats { engineering = totalEng }, allowExceedCap: false);
                                ctx.Log($"System Reset triggered! All Engineering restored!");
                            }
                        }
                    }
                };
            }
        }
    }
}
