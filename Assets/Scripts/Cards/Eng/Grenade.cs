using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    public class Grenade : DamageAllCard
    {
        // Explicit: pays with engineering
        protected override int GetEnergyCost() => 1;
        protected override int GetBasePower()  => 6;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Engineering;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            var attacker = Owner;
            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var victims = ctx.AllAliveEnemies();
            int hitCount = 0;
            foreach (var enemy in victims)
            {
                enemy.ApplyDamage(dmg);
                hitCount++;
            }
            attacker.ApplyDamage(2);

            if (hitCount > 0)
                ctx.Log($"{attacker.DisplayName} throws {Def.displayName}, dealing {dmg} to all enemies ({hitCount}).");
        }
    }
}
