using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Injection - Restore 5 HP and gain +1 Engineering.
    /// </summary>
    public class Injection : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.Heal(5);
            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { engineering = 1 }, allowExceedCap: true);
            }
            ctx.Log($"{Owner.DisplayName} uses Injection! Heals 5 HP and gains +1 Engineering this turn.");
        }
    }
}
