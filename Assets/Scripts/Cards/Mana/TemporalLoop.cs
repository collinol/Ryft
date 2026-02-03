using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Temporal Loop - Reset all cooldowns of your spells.
    /// Combine with itself for full-turn recursion.
    /// </summary>
    public class TemporalLoop : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            // Reset all cooldowns
            var cooldownManager = CardCooldownManager.Instance;
            if (cooldownManager != null)
            {
                cooldownManager.ResetAllCooldowns();
                ctx.Log($"{Owner.DisplayName} casts Temporal Loop! All spell cooldowns reset!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} casts Temporal Loop, but cooldown system not found!");
            }
        }
    }
}
