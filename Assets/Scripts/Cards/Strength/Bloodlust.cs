using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Bloodlust - Heal 3 HP for each enemy defeated this turn.
    /// </summary>
    public class Bloodlust : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates {Def.displayName}! Will heal 3 HP per kill this turn.");
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                int kills = tracker.GetKillsThisTurn();
                int healAmount = kills * 3;
                if (healAmount > 0)
                {
                    Owner.Heal(healAmount);
                    ctx.Log($"{Owner.DisplayName} heals {healAmount} HP from {kills} kills this turn!");
                }
            }
        }
    }
}
