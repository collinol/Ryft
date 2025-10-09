using Game.Abilities;
using UnityEngine;

namespace Game.Ryfts
{
    public class OrangeExplodedDamageReduced : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityUsed) return;
            if (ctx.source == null || !mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            float mult = (Def.floatMagnitude > 0f) ? Mathf.Clamp01(Def.floatMagnitude) : 0.5f;
            mgr.SetNextOutgoingDamageMultiplier(mult, Def?.id, this);
            StartInternalCooldown();
        }
    }
}