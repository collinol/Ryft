using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class VampiricStrikeCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            Owner.Heal(dmg);

            ctx.Log($"{Owner.DisplayName} uses Vampiric Strike for {dmg} damage and heals {dmg} HP.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
