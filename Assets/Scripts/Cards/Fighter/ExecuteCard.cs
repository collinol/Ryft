using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class ExecuteCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int threshold = Mathf.RoundToInt(target.TotalStats.maxHealth * 0.2f);
            bool killed = false;

            if (target.Health < threshold)
            {
                killed = DealDamageTracked(target, 99999);
                ctx.Log($"{Owner.DisplayName} executes {target.DisplayName}!");
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} attempts Execute but {target.DisplayName} is above 20% HP.");
            }

            HandleMomentum(killed);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
