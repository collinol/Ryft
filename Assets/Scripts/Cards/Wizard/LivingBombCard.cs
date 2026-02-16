using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Living Bomb - Apply 5+stat Burning to target. Apply LivingBomb status to target
    /// (on death, 3 Burning to all other enemies).
    /// </summary>
    public class LivingBombCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stacks = GetScaledPower();
            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Burning, stacks, Owner);
            target.StatusEffects.AddEffect(StatusEffectType.LivingBomb, -1, 1);
            ctx.Log($"{Owner.DisplayName} uses Living Bomb on {target.DisplayName}: {stacks} Burning + LivingBomb.");
        }
    }
}
