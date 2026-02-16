using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class RecklessSwingCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            Owner.ApplyDamage(3);

            ctx.Log($"{Owner.DisplayName} uses Reckless Swing for {dmg} damage and takes 3 self-damage.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
