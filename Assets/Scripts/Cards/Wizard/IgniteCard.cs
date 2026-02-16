using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Ignite - Apply 3+stat Burning to a single enemy.
    /// </summary>
    public class IgniteCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stacks = GetScaledPower();
            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Burning, stacks, Owner);
            ctx.Log($"{Owner.DisplayName} uses Ignite, applying {stacks} Burning to {target.DisplayName}.");
        }
    }
}
