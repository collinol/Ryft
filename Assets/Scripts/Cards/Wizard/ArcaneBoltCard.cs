using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Arcane Bolt - Deal 4+stat damage to a single enemy.
    /// </summary>
    public class ArcaneBoltCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            ctx.Log($"{Owner.DisplayName} uses Arcane Bolt for {dmg} damage.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
