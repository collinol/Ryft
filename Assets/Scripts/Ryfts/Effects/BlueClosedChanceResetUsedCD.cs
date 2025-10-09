using Game.Abilities;
using UnityEngine;

namespace Game.Ryfts
{
    public class BlueClosedChanceResetUsedCD : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityUsed || ctx.abilityDef == null) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            mgr.TryResetCooldownOf(ctx.abilityDef);
            StartInternalCooldown();
        }
    }
}