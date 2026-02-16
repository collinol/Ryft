using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Inferno - Consume all Burning on target. Deal 4*stacks+stat damage. Extract keyword.
    /// </summary>
    public class InfernoCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int consumedStacks = target.StatusEffects.ConsumeAllStacks(StatusEffectType.Burning);
            int baseDmg = 4 * consumedStacks + GetStatValue();
            int dmg = GetFinalDamage(baseDmg);

            bool killed = DealDamageTracked(target, dmg);
            HandleExtract(killed, consumedStacks);

            ctx.Log($"{Owner.DisplayName} uses Inferno, consuming {consumedStacks} Burning for {dmg} damage.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
