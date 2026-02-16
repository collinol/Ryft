using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    public class Card403 : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            if (target.Health < Owner.Health)
            {
                DealDamageTracked(target, 99999);
                ctx.Log($"{Owner.DisplayName} uses 403: target HP ({target.Health}) < player HP ({Owner.Health}). Instant kill!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses 403: target HP too high. No effect.");
            }
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
