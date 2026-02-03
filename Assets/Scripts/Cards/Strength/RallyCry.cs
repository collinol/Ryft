using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Rally Cry - Buff allies +2 Strength this turn.
    /// </summary>
    public class RallyCry : CardRuntime
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
                player.Gain(new Stats { strength = 2 }, allowExceedCap: true);
            }
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and gains +2 Strength this turn!");
        }
    }
}
