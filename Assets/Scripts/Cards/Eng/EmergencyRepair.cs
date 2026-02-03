using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Emergency Repair - Auto-heal 10 HP when below 25%.
    /// </summary>
    public class EmergencyRepair : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            float hpPercent = (float)Owner.Health / Owner.TotalStats.maxHealth;
            if (hpPercent < 0.25f)
            {
                Owner.Heal(10);
                ctx.Log($"{Owner.DisplayName} triggers Emergency Repair! Heals 10 HP!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} activates Emergency Repair protocol (will trigger below 25% HP).");
            }
        }
    }
}
