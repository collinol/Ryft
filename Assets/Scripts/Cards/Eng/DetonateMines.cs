using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Detonate Mines - Trigger all traps for massive AOE damage.
    /// </summary>
    public class DetonateMines : DamageAllCard
    {
        protected override StatField ScalingStat => StatField.Engineering;
        protected override int GetBasePower() => 8;
        protected override int GetScaling() => 2;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();

            var victims = ctx.AllAliveEnemies();
            foreach (var enemy in victims)
            {
                int finalDmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, enemy);
                DealDamage(enemy, finalDmg, ScalingStat);
            }

            ctx.Log($"{Owner.DisplayName} detonates all mines for {dmg} damage to all enemies!");
        }
    }
}
