using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    public class CullTheWeak : DamageSingleCard
    {
        protected override int GetEnergyCost() => 1;
        protected override int GetBasePower()  => 6;
        protected override int GetScaling()    => 1;
        public override TargetingType Targeting => TargetingType.SingleEnemy;
        protected override StatField ScalingStat => StatField.Strength;
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            float fullDamage = 0;
            if (target.Health * 1f < .3*target.TotalStats.maxHealth )
            {
                fullDamage = dmg*1.5f;
            }
            else
            {
                fullDamage = dmg * 1f;
            }
            target.ApplyDamage(Mathf.RoundToInt(fullDamage));
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {fullDamage} damage on {target.DisplayName}.");
        }

    }

}
