using UnityEngine;

namespace Game.Ryfts
{
    public class BlueClosedCDsMinus1OnStart : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;
            mgr.ReduceAllCooldownsBy(Mathf.Max(1, Def.intMagnitude));
        }
    }
}