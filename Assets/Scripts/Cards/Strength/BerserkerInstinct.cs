using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Berserker Instinct - Whenever you take damage, play a random 0-cost Strength card.
    /// Combine with reflect or thorns → runaway trigger storm.
    /// </summary>
    public class BerserkerInstinct : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} gains Berserker Instinct! Takes damage → free Strength card.");
            // TODO: Register damage listener that triggers random Strength card
        }
    }
}
