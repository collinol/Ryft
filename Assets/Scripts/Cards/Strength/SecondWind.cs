using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Second Wind - Heal 50% HP if below 25%.
    /// </summary>
    public class SecondWind : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            float hpPercent = (float)Owner.Health / Owner.TotalStats.maxHealth;
            if (hpPercent < 0.25f)
            {
                int healAmount = Mathf.RoundToInt(Owner.TotalStats.maxHealth * 0.5f);
                Owner.Heal(healAmount);
                ctx.Log($"{Owner.DisplayName} gets a second wind and heals {healAmount} HP!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} tries to use Second Wind, but isn't wounded enough (needs <25% HP).");
            }
        }
    }
}
