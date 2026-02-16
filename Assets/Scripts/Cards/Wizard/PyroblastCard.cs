using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Pyroblast - Deal 25+stat damage to a single enemy. Apply 5 Burning.
    /// </summary>
    public class PyroblastCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);

            if (target.IsAlive)
            {
                CombatBuffManager.ApplyDebuff(target, StatusEffectType.Burning, 5, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Pyroblast for {dmg} damage + 5 Burning on {target.DisplayName}.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
