using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Disintegrate - Instantly kill a weakened foe (below 20% HP).
    /// </summary>
    public class Disintegrate : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
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
                ctx.Log($"{Owner.DisplayName} disintegrates {target.DisplayName}!");
            }
            else
            {
                int dmg = Mathf.Max(1, 5 + GetOwnerCurrentFor(ScalingStat));
                DealDamage(target, dmg, ScalingStat);
                ctx.Log($"{Owner.DisplayName} attacks {target.DisplayName} for {dmg} damage (not weak enough to disintegrate).");
            }
        }
    }
}
