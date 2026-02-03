using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Adrenal Surge - Gain +1 Strength after every attack that kills an enemy.
    /// Pairs with cheap damage cards to self-scale infinitely.
    /// </summary>
    public class AdrenalSurge : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates {Def.displayName}! +1 Strength per kill!");
            // TODO: Implement kill listener that adds +1 Strength
            // This effect would need to be implemented in RyftEffectManager or as a battle-scoped effect
        }
    }
}
