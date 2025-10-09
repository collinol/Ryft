namespace Game.Ryfts
{
    // When an enemy dies: reduce all ability cooldowns by N.
    public class CooldownReduceAllEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnEnemyDefeated) return;
            if (!ShouldProc()) return;
            mgr.ReduceAllCooldownsBy(Def.intMagnitude <= 0 ? 1 : Def.intMagnitude);
            StartInternalCooldown();
        }
    }
}
