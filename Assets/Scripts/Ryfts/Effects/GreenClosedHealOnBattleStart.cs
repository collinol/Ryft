using UnityEngine;

namespace Game.Ryfts
{
    public class GreenClosedHealOnBattleStart : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;

            var p = mgr.PlayerActor;
            if (p == null) return;

            int amount = Mathf.RoundToInt(Mathf.Max(0, p.TotalStats.maxHealth) * Mathf.Clamp01(Def.floatMagnitude));
            if (amount > 0) mgr.HealPlayer(amount);
        }
    }
}
