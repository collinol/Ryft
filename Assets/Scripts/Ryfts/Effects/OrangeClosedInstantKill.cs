using UnityEngine;

namespace Game.Ryfts
{
    public class OrangeClosedInstantKill : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnDamageDealt) return;
            if (ctx.cardDef == null) return;
            if (!mgr.IsPlayer(ctx.source) || ctx.target == null) return;
            if (!ShouldProc()) return;

            int hp = ctx.target.Health;
            mgr.DebugLogEffectAction("EXECUTE", $"{Def?.id} target={ctx.target.DisplayName} refunding {hp}");
            if (hp > 0)
            {
                ctx.target.ApplyDamage(hp);
                RyftCombatEvents.RaiseDamageDealt(ctx.source, ctx.target, hp);
                if (!ctx.target.IsAlive) RyftCombatEvents.RaiseEnemyDefeated(ctx.target);
            }
            StartInternalCooldown();
        }
    }
}
