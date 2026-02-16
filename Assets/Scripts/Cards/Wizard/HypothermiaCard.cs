using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class HypothermiaCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int current = target.StatusEffects.GetStacks(StatusEffectType.Frozen);
            int bonus = current;

            if (bonus > 0)
            {
                CombatBuffManager.ApplyDebuff(target, StatusEffectType.Frozen, bonus, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Hypothermia on {target.DisplayName}. Doubled Frozen from {current} to {current + bonus}.");
        }
    }
}
