using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    public class Card402 : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;
            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            if (killed)
            {
                MapSession.I?.AddGold(20);
                ctx.Log($"{Owner.DisplayName} uses 402: {dmg} damage, kill! +20 gold.");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses 402: {dmg} damage.");
            }
        }
    }
}
