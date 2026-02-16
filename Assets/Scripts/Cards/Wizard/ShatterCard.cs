using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class ShatterCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int consumed = target.StatusEffects.ConsumeAllStacks(StatusEffectType.Frozen);
            int dmg = GetFinalDamage(5 * consumed + GetStatValue());
            bool killed = DealDamageTracked(target, dmg);

            ctx.Log($"{Owner.DisplayName} uses Shatter, consuming {consumed} Frozen for {dmg} damage.");

            HandleExtract(killed, consumed);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
