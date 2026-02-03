using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Finisher - Deal +50% damage to enemies below 30% HP.
    /// </summary>
    public class Finisher : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 5;
        protected override int GetScaling() => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());

            // Check if target is below 30% HP
            float hpPercent = (float)target.Health / target.TotalStats.maxHealth;
            if (hpPercent < 0.3f)
            {
                dmg = Mathf.RoundToInt(dmg * 1.5f);
                ctx.Log($"{Owner.DisplayName} uses {Def.displayName} on a weakened foe!");
            }

            var mgr = RyftEffectManager.Ensure();
            dmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, target);
            DealDamage(target, dmg, ScalingStat);
            ctx.Log($"{Owner.DisplayName} deals {dmg} damage to {target.DisplayName}!");
        }
    }
}
