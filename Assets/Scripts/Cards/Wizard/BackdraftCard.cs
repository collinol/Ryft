using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Backdraft - Consume all Burning on ALL enemies. Deal 2*totalStacks to all enemies.
    /// Extract keyword. No stat scaling.
    /// </summary>
    public class BackdraftCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int totalStacks = 0;
            foreach (var enemy in ctx.AllAliveEnemies().ToList())
            {
                totalStacks += enemy.StatusEffects.ConsumeAllStacks(StatusEffectType.Burning);
            }

            int baseDmg = 2 * totalStacks;
            int dmg = GetFinalDamage(baseDmg);
            int kills = DealDamageToAll(ctx, dmg);

            HandleExtract(kills > 0, totalStacks);

            ctx.Log($"{Owner.DisplayName} uses Backdraft, consuming {totalStacks} total Burning for {dmg} damage to all enemies.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
