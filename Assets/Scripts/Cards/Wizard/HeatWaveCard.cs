using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Heat Wave - Apply 2+stat Burning to all enemies.
    /// </summary>
    public class HeatWaveCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stacks = GetScaledPower();
            foreach (var enemy in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(enemy, StatusEffectType.Burning, stacks, Owner);
            }
            ctx.Log($"{Owner.DisplayName} uses Heat Wave, applying {stacks} Burning to all enemies.");
        }
    }
}
