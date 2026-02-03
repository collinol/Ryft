using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Earthquake - Deal 6 damage to all grounded enemies.
    /// </summary>
    public class Earthquake : DamageAllCard
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 6;
        protected override int GetScaling() => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();

            var victims = ctx.AllAliveEnemies();
            int hitCount = 0;
            foreach (var enemy in victims)
            {
                int finalDmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, enemy);
                DealDamage(enemy, finalDmg, ScalingStat);
                hitCount++;
            }

            ctx.Log($"{Owner.DisplayName} causes an earthquake, dealing {dmg} to {hitCount} enemies!");
        }
    }
}
