using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Meltdown - Deal 10+stat damage to target. Add bonus = 2 * total Burning on ALL enemies.
    /// Does NOT consume stacks. Extract keyword.
    /// </summary>
    public class MeltdownCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int totalBurning = CombatBuffManager.GetTotalDebuffStacksOnAllEnemies(ctx, StatusEffectType.Burning);
            int baseDmg = 10 + GetStatValue() + 2 * totalBurning;
            int dmg = GetFinalDamage(baseDmg);

            bool killed = DealDamageTracked(target, dmg);
            HandleExtract(killed, totalBurning);

            ctx.Log($"{Owner.DisplayName} uses Meltdown for {dmg} damage (bonus from {totalBurning} total Burning).");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
