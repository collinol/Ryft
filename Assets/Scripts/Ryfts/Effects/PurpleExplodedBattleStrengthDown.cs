using UnityEngine;

namespace Game.Ryfts
{
    // Temporary Strength debuff.
    public class PurpleExplodedBattleStrengthDown : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
            int amt = Mathf.Max(1, Mathf.Abs(Def.intMagnitude));
            mgr.PlayerPermanentStatsDelta(strength: -amt);
        }

        public override void OnRemoved(RyftEffectManager mgr)
        {
            int amt = Mathf.Max(1, Mathf.Abs(Def.intMagnitude));
            mgr.PlayerPermanentStatsDelta(strength: +amt);
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx) { /* passive */ }
    }
}