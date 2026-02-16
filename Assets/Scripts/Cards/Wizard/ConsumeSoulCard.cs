using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class ConsumeSoulCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int consumed = target.StatusEffects.ConsumeAllStacks(StatusEffectType.Cursed);
            int baseDmg = 4 * consumed + GetStatValue();
            int dmg = GetFinalDamage(Mathf.Max(1, baseDmg));
            bool killed = DealDamageTracked(target, dmg);

            int healAmount = 2 * consumed;
            if (healAmount > 0)
            {
                Owner.Heal(healAmount);
            }

            ctx.Log($"{Owner.DisplayName} uses Consume Soul, consuming {consumed} Cursed for {dmg} damage and {healAmount} healing.");

            HandleExtract(killed, consumed);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
