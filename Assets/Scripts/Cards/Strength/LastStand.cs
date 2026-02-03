using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Last Stand - If you would die, restore to 1 HP and double Strength.
    /// Combine with lifesteal → permanent survival loop.
    /// </summary>
    public class LastStand : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        private bool triggered = false;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var deathPrevention = DeathPreventionSystem.Instance;
            if (deathPrevention != null)
            {
                deathPrevention.RegisterPrevention(Owner, DeathPreventionType.LastStand, Def.id, (actor) => {
                    // Double Strength
                    var player = actor as Game.Player.PlayerCharacter;
                    if (player != null)
                    {
                        player.Gain(new Stats { strength = player.CurrentTurnStats.strength }, allowExceedCap: true);
                    }
                });
                ctx.Log($"{Owner.DisplayName} activates Last Stand! Will survive a fatal blow with doubled Strength.");
            }
            triggered = false;
        }
    }
}
