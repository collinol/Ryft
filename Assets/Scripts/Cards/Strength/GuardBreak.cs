using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Guard Break - Ignore enemy Defense this attack.
    /// </summary>
    public class GuardBreak : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Strength;
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

            // Apply damage directly without defense mitigation
            DealDamage(target, dmg, ScalingStat);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage, ignoring defense!");
        }
    }
}
