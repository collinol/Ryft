using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;
using System.Linq;

namespace Game.Cards
{
    /// <summary>
    /// Cluster Charge - Split 6 damage among all enemies.
    /// </summary>
    public class ClusterCharge : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        protected override int GetBasePower() => 6;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int totalDmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());

            var victims = ctx.AllAliveEnemies();
            int count = victims.Count();
            if (count == 0) return;

            int dmgPerEnemy = totalDmg / count;
            foreach (var enemy in victims)
            {
                DealDamage(enemy, dmgPerEnemy, ScalingStat);
            }

            ctx.Log($"{Owner.DisplayName} deploys Cluster Charge! {totalDmg} damage split among {count} enemies ({dmgPerEnemy} each).");
        }
    }
}
