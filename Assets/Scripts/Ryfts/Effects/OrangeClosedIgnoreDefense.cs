// Game/Ryfts/OrangeClosedIgnoreDefense.cs
using UnityEngine;

namespace Game.Ryfts
{
    // Arms a one-shot "ignore defense" flag. Your damaging abilities
    // should consult mgr.ConsumeIgnoreDefenseFlagIfAny() during damage calc.
    public class OrangeClosedIgnoreDefense : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityUsed) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            mgr.FlagNextPlayerAttackIgnoreDefense();
            StartInternalCooldown();
        }
    }
}
