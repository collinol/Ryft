using UnityEngine;

namespace Game.Ryfts
{
    public class GreenClosedBarrierOnBattleStart : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;
            mgr.ApplyBarrierPercentToPlayer(Mathf.Clamp01(Def.floatMagnitude));
        }
    }
}