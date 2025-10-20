using UnityEngine;

namespace Game.Ryfts
{
    // On card used: small chance to immediately recast for free.
    public class OrangeClosedDoubleCastEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityUsed || ctx.cardDef == null) return;
            if (!ShouldProc()) return;

            Debug.Log("DOUBLE CAST TRIGGERED");
            mgr.TryDoubleCast(ctx);
            StartInternalCooldown();
        }
    }
}
