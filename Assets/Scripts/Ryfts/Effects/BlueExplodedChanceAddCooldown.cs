using UnityEngine;

namespace Game.Ryfts
{
    /// On your call resolving: % chance to add a SURCHARGE to the *next* call.
    /// Uses Def.intMagnitude (>=1).
    public class BlueExplodedChanceAddCooldown : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            int delta = Mathf.Max(1, Def?.intMagnitude ?? 1);
            mgr.AddNextCallCostDelta(+delta);
            mgr.DebugLogEffectAction("SurchargeNext",
                $"{Def?.id} next call cost +{delta}");

            StartInternalCooldown();
        }
    }
}
