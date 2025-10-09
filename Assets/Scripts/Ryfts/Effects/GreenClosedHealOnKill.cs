using UnityEngine;

namespace Game.Ryfts
{
    public class GreenClosedHealOnKill : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnEnemyDefeated) return;
            int amount = Mathf.Max(0, Def.intMagnitude);
            if (amount > 0) mgr.HealPlayer(amount);
        }
    }
}