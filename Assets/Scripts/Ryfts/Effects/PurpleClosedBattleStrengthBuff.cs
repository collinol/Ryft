using UnityEngine;

namespace Game.Ryfts
{
    // Temporary Strength buff. We apply on added and remove on removed.
    public class PurpleClosedBattleStrengthBuff : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
            int amt = Mathf.Max(1, Def.intMagnitude);
            mgr.PlayerPermanentStatsDelta(strength: amt);
        }

        public override void OnRemoved(RyftEffectManager mgr)
        {
            int amt = Mathf.Max(1, Def.intMagnitude);
            mgr.PlayerPermanentStatsDelta(strength: -amt);
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx) { /* passive */ }
    }
}