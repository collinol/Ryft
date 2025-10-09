using UnityEngine;

namespace Game.Ryfts
{
    // At battle start: lose % of max HP.
    public class GreenExplodedStartWounded : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;

            var p = mgr.PlayerActor;
            if (p == null || !p.IsAlive) return;

            int amount = Mathf.RoundToInt(Mathf.Max(0, p.TotalStats.maxHealth) * Mathf.Clamp01(Def.floatMagnitude));
            if (amount <= 0) return;

            int before = p.Health;
            p.ApplyDamage(amount);
            int taken = Mathf.Max(0, before - p.Health);
            if (taken > 0) RyftCombatEvents.RaiseDamageTaken(p, taken);
        }
    }
}