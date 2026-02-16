using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Flame Barrier - Gain 6+stat protection. Apply FlameBarrier status (duration 1) to owner.
    /// </summary>
    public class FlameBarrierCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);

            Owner.StatusEffects.AddEffect(StatusEffectType.FlameBarrier, 1, 1);

            ctx.Log($"{Owner.DisplayName} uses Flame Barrier: {prot} protection + FlameBarrier active.");
        }
    }
}
