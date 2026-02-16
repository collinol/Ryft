using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Analyze Weakness - Apply AnalyzeWeakness status (duration 1) to target. Draw 1 card.
    /// </summary>
    public class AnalyzeWeaknessCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            target.StatusEffects?.AddEffect(StatusEffectType.AnalyzeWeakness, 1);
            ctx.Log($"{Owner.DisplayName} uses Analyze Weakness on {target.DisplayName}.");

            FightSceneController.Instance?.DrawCards(1);
        }
    }
}
