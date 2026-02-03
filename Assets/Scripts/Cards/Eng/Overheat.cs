using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Overheat - Double Engineering power this turn, but take 3 self-damage.
    /// </summary>
    public class Overheat : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int currentEng = GetOwnerCurrentFor(StatField.Engineering);
            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { engineering = currentEng }, allowExceedCap: true); // Double engineering
            }
            DealDamage(Owner, 3, ScalingStat);

            ctx.Log($"{Owner.DisplayName} overheats! Engineering doubled but takes 3 damage.");
        }
    }
}
