namespace Game.Ryfts
{
    // Applies permanent additive stat bonuses immediately on add.
    public class PermanentStatModEffect : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
            mgr.PlayerPermanentStatsDelta(
                maxHp:   Def.builtIn == BuiltInOp.AddMaxHealth ? Def.intMagnitude : 0,
                strength:Def.builtIn == BuiltInOp.AddStrength  ? Def.intMagnitude : 0,
                defense: Def.builtIn == BuiltInOp.AddDefense   ? Def.intMagnitude : 0
            );
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx) { /* nothing */ }
    }
}
