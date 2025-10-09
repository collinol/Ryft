// Game/Ryfts/OrangeClosedLifeSteal.cs
using UnityEngine;

namespace Game.Ryfts
{
    // Heal the player for a fraction of damage dealt.
    public class OrangeClosedLifeSteal : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnDamageDealt) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (ctx.amount <= 0) return;

            float pct = Mathf.Max(0f, Def.floatMagnitude);
            int heal = Mathf.RoundToInt(ctx.amount * pct);
            if (heal > 0)
            {
                 mgr.HealPlayer(heal);
                 mgr.DebugLogEffectAction("LIFESTEAL", $"{Def?.id} dealt={ctx.amount} heal={heal}");
            }
        }
    }
}
