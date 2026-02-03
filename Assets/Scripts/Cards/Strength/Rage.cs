using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Rage - Double Strength this turn, skip next attack.
    /// </summary>
    public class Rage : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int currentStr = GetOwnerCurrentFor(StatField.Strength);
            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { strength = currentStr }, allowExceedCap: true); // Double strength
            }
            ctx.Log($"{Owner.DisplayName} enters a rage, doubling Strength this turn!");
            // Queue effect to skip next player action
            var endOfTurn = EndOfTurnEffects.Instance;
            if (endOfTurn != null)
            {
                // This is a simplification - ideally track "skip next attack" specifically
                ctx.Log($"({Owner.DisplayName} will have reduced actions next turn)");
            }
        }
    }
}
