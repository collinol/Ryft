using Game.Core;
using Game.Combat;
using Game.Ryfts;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class BloodPactCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.ApplyDamage(5);
            RyftEffectManager.Ensure().AddTempStrength(2);

            ctx.Log($"{Owner.DisplayName} uses Blood Pact. Loses 5 HP, gains +2 Strength this turn.");
        }
    }
}
