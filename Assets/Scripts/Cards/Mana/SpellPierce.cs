using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Spell Pierce - Ignore magic resistance.
    /// </summary>
    public class SpellPierce : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 4;
        protected override int GetScaling() => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());

            // Apply damage directly without resistance
            DealDamage(target, dmg, ScalingStat);
            ctx.Log($"{Owner.DisplayName} pierces through magic resistance for {dmg} damage!");
        }
    }
}
