// Game/Ryfts/OrangeClosedOnKillResetRandomCD.cs
using UnityEngine;
using System.Linq;

namespace Game.Ryfts
{
    // On enemy defeated: chance to reset a random player ability cooldown.
    public class OrangeClosedOnKillResetRandomCD : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnEnemyDefeated) return;
            if (!ShouldProc()) return;

            // TODO (nice): add mgr.TryResetRandomCooldown();
            // Minimal implementation: reduce all by 1 as a friendly approximation.
            mgr.ReduceAllCooldownsBy(1);
            StartInternalCooldown();
        }
    }
}
