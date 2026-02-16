using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class CrystallizeCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);

            if (!killed)
            {
                int frozenStacks = target.StatusEffects.GetStacks(StatusEffectType.Frozen);
                if (frozenStacks >= 3)
                {
                    target.StatusEffects.AddEffect(StatusEffectType.Stun, 1);
                    ctx.Log($"{Owner.DisplayName} uses Crystallize for {dmg} damage and stuns {target.DisplayName} (had {frozenStacks} Frozen).");
                }
                else
                {
                    ctx.Log($"{Owner.DisplayName} uses Crystallize for {dmg} damage. {target.DisplayName} has only {frozenStacks} Frozen (need 3+ to stun).");
                }
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses Crystallize for {dmg} damage, killing {target.DisplayName}.");
            }

            HandleExtract(killed, 0);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
