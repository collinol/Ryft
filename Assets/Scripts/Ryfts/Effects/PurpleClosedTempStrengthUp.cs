using UnityEngine;
using System;

namespace Game.Ryfts
{
    /// +Strength for DurationNTurns. Stacks additively.
    public class PurpleClosedTempStrengthUp : RyftEffectRuntime
    {
        private int amt;

        public override void OnAdded(RyftEffectManager mgr)
        {
            amt = Mathf.Max(1, Def.intMagnitude);
            mgr.AddTempStrength(amt); // grant first stack
        }

        public override void OnRemoved(RyftEffectManager mgr)
        {
            // remove all stacks’ worth when the effect expires or is purged
            mgr.AddTempStrength(-(amt * Mathf.Max(1, stacks)));
        }

        public override void OnStacksChanged(RyftEffectManager mgr, int delta)
        {
            if (delta != 0)
                mgr.AddTempStrength(amt * delta); // + for gains, - for losses
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx) { /* passive */ }
    }
}