using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class RelentlessAssaultCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            ctx.Log($"{Owner.DisplayName} uses Relentless Assault for {dmg} damage.");

            HandleMomentum(killed);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
