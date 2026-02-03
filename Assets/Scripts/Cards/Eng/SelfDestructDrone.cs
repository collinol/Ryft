using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Self-Destruct Drone - Sacrifice drone to kill a low HP enemy.
    /// </summary>
    public class SelfDestructDrone : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.SingleEnemy;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            float hpPercent = (float)target.Health / target.TotalStats.maxHealth;
            if (hpPercent < 0.20f)
            {
                target.ApplyDamage(target.Health);
                ctx.Log($"{Owner.DisplayName} self-destructs a drone to kill {target.DisplayName}!");
            }
            else
            {
                int dmg = Mathf.Max(1, 5 + GetOwnerCurrentFor(ScalingStat));
                DealDamage(target, dmg, ScalingStat);
                ctx.Log($"{Owner.DisplayName} attacks {target.DisplayName} for {dmg} damage (not weak enough for execution).");
            }
        }
    }
}
