using UnityEngine;
using Game.Core;     // StatField

namespace Game.Ryfts
{
    public class BlueClosedChanceResetUsedCD : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            int lastPaid = mgr.PeekLastPaidCostSafe();
            if (lastPaid <= 0) { StartInternalCooldown(); return; }

            float pct = (Def && Def.floatMagnitude > 0f) ? Mathf.Clamp01(Def.floatMagnitude) : 1f;
            int refund = (Def && Def.floatMagnitude > 0f)
                ? Mathf.RoundToInt(lastPaid * pct)
                : Mathf.Max(1, Def?.intMagnitude ?? lastPaid);

            var field = mgr.PeekLastPaidField();

            mgr.DebugLogEffectAction("RefundNow",
                $"{Def?.id} refund {refund} to {field} (lastPaid={lastPaid})");

            // Immediate refund to the SAME field that was paid
            RyftCombatEvents.RaiseResourceRefund(ctx.source, field, refund);

            StartInternalCooldown();
        }
    }
}
