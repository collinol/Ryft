// Game/Ryfts/BlueExplodedChanceAddCooldown.cs
using Game.Abilities;
using UnityEngine;

namespace Game.Ryfts
{
    // Chance on ability resolved: add +1 cooldown to THAT ability.
    public class BlueExplodedChanceAddCooldown : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved || ctx.abilityDef == null) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (!ShouldProc()) return;

            int delta = Mathf.Max(1, Def.intMagnitude);
            mgr.TryAddCooldown(ctx.abilityDef, delta);
            mgr.DebugLogEffectAction("CD+",
                $"{Def?.id} ability={ctx.abilityDef.id} +{delta}");
            StartInternalCooldown();
        }
    }

}