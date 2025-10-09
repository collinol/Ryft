using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Abilities;

namespace Game.Ryfts
{
    /// Parametric implementations of common ryft effects (no custom class needed)
    public class BuiltInRyftEffect : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
            // Immediate “Permanent” apply for stat increases
            if (Def.lifetime == EffectLifetime.Permanent)
            {
                switch (Def.builtIn)
                {
                    case BuiltInOp.AddMaxHealth:
                        mgr.PlayerPermanentStatsDelta(maxHp: Def.intMagnitude);
                        break;
                    case BuiltInOp.AddStrength:
                        mgr.PlayerPermanentStatsDelta(strength: Def.intMagnitude);
                        break;
                    case BuiltInOp.AddDefense:
                        mgr.PlayerPermanentStatsDelta(defense: Def.intMagnitude);
                        break;
                }
            }
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            // Turn-based housekeeping
            if (ctx.trigger == RyftTrigger.OnTurnEnd) TickTurn(mgr);

            // Proc checks for chance-based ops
            switch (Def.builtIn)
            {
                case BuiltInOp.ChanceDoubleCast:
                    if (ctx.trigger == RyftTrigger.OnAbilityUsed && ShouldProc())
                    {
                        mgr.TryDoubleCast(ctx);
                        StartInternalCooldown();
                    }
                    break;

                case BuiltInOp.ChanceCooldownResetSelf:
                    if (ctx.trigger == RyftTrigger.OnAbilityResolved && ShouldProc())
                    {
                        mgr.TryResetCooldownOf(ctx.abilityDef);
                        StartInternalCooldown();
                    }
                    break;

                case BuiltInOp.ChanceReduceAllCooldowns:
                    if (ctx.trigger == RyftTrigger.OnEnemyDefeated && ShouldProc())
                    {
                        mgr.ReduceAllCooldownsBy(1);
                        StartInternalCooldown();
                    }
                    break;

                case BuiltInOp.ChanceShieldOnBattleStart:
                    if (ctx.trigger == RyftTrigger.OnBattleStart && ShouldProc())
                    {
                        mgr.ApplyBarrierPercentToPlayer(Def.floatMagnitude); // e.g., 0.10f = 10%
                        StartInternalCooldown();
                    }
                    break;

                case BuiltInOp.ChanceHealOnHit:
                    if (ctx.trigger == RyftTrigger.OnDamageDealt && ShouldProc())
                    {
                        mgr.HealPlayer(Def.intMagnitude);
                        StartInternalCooldown();
                    }
                    break;

                case BuiltInOp.ChanceIgnoreDefense:
                    if (ctx.trigger == RyftTrigger.OnAbilityUsed && ShouldProc())
                    {
                        mgr.FlagNextPlayerAttackIgnoreDefense();
                        StartInternalCooldown();
                    }
                    break;
            }
        }
    }
}
