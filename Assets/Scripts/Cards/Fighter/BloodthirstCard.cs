using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class BloodthirstCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects?.AddEffect(StatusEffectType.NextAttackBonus, 1, 1, (float)GetScaledPower());
            ctx.Log($"{Owner.DisplayName} uses Bloodthirst. Next attack this turn deals +3 damage.");

            // Momentum: no damage dealt directly, so no kill check needed here.
            // The bonus applies to the next attack card played.
        }
    }
}
