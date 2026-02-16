using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Combustion - Read target's Burning stacks. Apply that many to all OTHER enemies.
    /// </summary>
    public class CombustionCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stacks = target.StatusEffects.GetStacks(StatusEffectType.Burning);
            if (stacks <= 0)
            {
                ctx.Log($"{Owner.DisplayName} uses Combustion, but {target.DisplayName} has no Burning.");
                return;
            }

            foreach (var enemy in ctx.AllAliveEnemies().ToList())
            {
                if (enemy == target) continue;
                CombatBuffManager.ApplyDebuff(enemy, StatusEffectType.Burning, stacks, Owner);
            }
            ctx.Log($"{Owner.DisplayName} uses Combustion, spreading {stacks} Burning from {target.DisplayName} to all other enemies.");
        }
    }
}
