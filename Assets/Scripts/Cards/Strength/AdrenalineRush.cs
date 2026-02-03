using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Adrenaline Rush - Gain +1 Strength this turn.
    /// </summary>
    public class AdrenalineRush : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { strength = 1 }, allowExceedCap: true);

                // Play buff effect
                PlayBuffEffect(Owner, StatField.Strength);
            }
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and gains +1 Strength this turn!");
        }
    }
}
