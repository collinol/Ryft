namespace Game.Ryfts
{
    /// When an enemy dies: grant call credits (which auto-reduce future call costs) by N.
    /// Uses Def.intMagnitude (defaults to 1).
    public class CooldownReduceAllEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnEnemyDefeated) return;
            if (!ShouldProc()) return;

            int credits = (Def?.intMagnitude ?? 0) <= 0 ? 1 : Def.intMagnitude;
            mgr.AddCallCredits(credits);
            mgr.DebugLogEffectAction("CreditGain",
                $"{Def?.id} granted +{credits} call credits");

            StartInternalCooldown();
        }
    }
}
