// Game.Cards.DefenseTechCard
using UnityEngine; using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public class DefenseTechCard : CardRuntime
    {
        protected override StatField CostField => StatField.Engineering; // or Mana for magic barrier
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayCost()) return;

            // Minimal placeholder effects that you can expand with your status system:
            int shield = Mathf.Max(1, Def.power + Owner.TotalStats.defense);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName}, generating a shield of {shield} (defense boosted).");
            // TODO: attach a temporary status to Player to mitigate or reflect damage.
        }
    }
}
