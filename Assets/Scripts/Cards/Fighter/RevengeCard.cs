using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class RevengeCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = Mathf.Max(0, FightSceneController.Instance != null
                ? FightSceneController.Instance.ProtectionLostLastTurn
                : 0);
            bool killed = DealDamageTracked(target, dmg);
            ctx.Log($"{Owner.DisplayName} uses Revenge for {dmg} damage (protection lost last turn).");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
