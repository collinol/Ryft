using UnityEngine;

namespace Game.Ryfts
{
    /// On battle start, apply a one-shot SURCHARGE to the first call cost.
    /// Uses Def.intMagnitude (>=1). Negative values would act as a discount, but
    /// exploded blue generally skews negative, so we clamp to >=1 here.
    public class BlueExplodedCDsPlus1OnStart : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;

            int surcharge = Mathf.Max(1, Def?.intMagnitude ?? 1);
            mgr.AddNextCallCostDelta(+surcharge);
            mgr.DebugLogEffectAction("SurchargeNext",
                $"{Def?.id} will add +{surcharge} cost to the next call");
        }
    }
}
