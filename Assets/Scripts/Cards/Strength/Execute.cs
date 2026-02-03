using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Execute - Instantly kill an enemy under 15% HP.
    /// </summary>
    public class ExecuteCard : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.SingleEnemy;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            float hpPercent = (float)target.Health / target.TotalStats.maxHealth;
            if (hpPercent < 0.15f)
            {
                target.ApplyDamage(target.Health); // Kill instantly
                ctx.Log($"{Owner.DisplayName} executes {target.DisplayName}!");
            }
            else
            {
                // Deal normal damage if not below threshold
                int dmg = Mathf.Max(1, 5 + GetOwnerCurrentFor(ScalingStat));
                DealDamage(target, dmg, ScalingStat);
                ctx.Log($"{Owner.DisplayName} attacks {target.DisplayName} for {dmg} damage (target not low enough to execute).");
            }
        }
    }
}
