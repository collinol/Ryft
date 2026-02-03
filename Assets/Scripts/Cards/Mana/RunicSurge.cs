using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Runic Surge - Each time you spend Mana, next spell's cost -1.
    /// Combine with cost-reset buffs → infinite.
    /// </summary>
    public class RunicSurge : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Runic Surge! Each Mana spent reduces next spell cost.");
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                int manaSpent = tracker.GetTotalManaSpentThisTurn();
                ctx.Log($"{Owner.DisplayName} channels runic energy! {manaSpent} Mana spent this turn will reduce costs.");
                // TODO: Implement cost reduction mechanism - needs deeper integration
            }
        }
    }
}
