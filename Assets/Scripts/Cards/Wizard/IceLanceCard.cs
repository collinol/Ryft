using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class IceLanceCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int frozenStacks = target.StatusEffects.GetStacks(StatusEffectType.Frozen);
            int baseDmg = 3 + GetStatValue() + 3 * frozenStacks;
            int dmg = GetFinalDamage(baseDmg);
            bool killed = DealDamageTracked(target, dmg);

            ctx.Log($"{Owner.DisplayName} uses Ice Lance for {dmg} damage ({frozenStacks} Frozen stacks on target).");

            HandleExtract(killed, frozenStacks);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
