using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;
using System.Linq;

namespace Game.Cards
{
    /// <summary>
    /// Lightning Storm - Random 3x 4-damage hits.
    /// </summary>
    public class LightningStorm : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 4;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();

            var enemies = ctx.AllAliveEnemies().ToList();
            if (enemies.Count == 0) return;

            // Play area effect at center of enemies
            if (enemies.Count > 0 && enemies[0] is MonoBehaviour mono)
            {
                PlayAreaEffect(mono.transform.position, 3f, ScalingStat);
            }

            for (int i = 0; i < 3; i++)
            {
                var target = enemies[Random.Range(0, enemies.Count)];
                int finalDmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, target);
                DealDamage(target, finalDmg, ScalingStat);
                ctx.Log($"Lightning strikes {target.DisplayName} for {finalDmg} damage!");
            }
        }
    }
}
