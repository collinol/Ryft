using UnityEngine;

namespace Game.Ryfts
{
    public class BlueExplodedCDsPlus1OnStart : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;
            // Negative "increase by 1" == reduce by -1
            mgr.ReduceAllCooldownsBy(-Mathf.Max(1, Def.intMagnitude));
        }
    }
}