using System;
using UnityEngine;

namespace Game.Ryfts
{
    /// Permanent +Strength. Stacks additively.
    public class PurpleClosedPermStrengthUp : RyftEffectRuntime
    {
        private int amt;

        public override void OnAdded(RyftEffectManager mgr)
        {
            amt = Mathf.Max(1, Def.intMagnitude);
            // First stack
            mgr.PlayerPermanentStatsDelta(strength: amt);
        }

        public override void OnRemoved(RyftEffectManager mgr)
        {
            // Remove all granted stacks (in case this effect ever gets purged)
            mgr.PlayerPermanentStatsDelta(strength: -(amt * Mathf.Max(1, stacks)));
        }

        // Called when Manager stacks an existing effect (your AddEffect uses AddStack)
        public override void OnStacksChanged(RyftEffectManager mgr, int delta)
        {
            if (delta != 0)
                mgr.PlayerPermanentStatsDelta(strength: amt * delta);
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx) { /* passive */ }
    }
}