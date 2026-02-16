using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Ryfts
{
    /// Parametric implementations of common ryft effects (no custom class needed).
    /// Handles both immediate-apply (OnAdded) and triggered (HandleTrigger) ops.
    /// Passive Query ops (B-tier) need no handler here — consuming systems poll via SumInt/SumFloat.
    public class BuiltInRyftEffect : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
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
                    case BuiltInOp.AddIntellect:
                        mgr.PlayerPermanentStatsDelta(intellect: Def.intMagnitude);
                        break;
                    case BuiltInOp.AddEngineering:
                        mgr.PlayerPermanentStatsDelta(eng: Def.intMagnitude);
                        break;
                }
            }
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            switch (Def.builtIn)
            {
                // ── Legacy chance-based ──
                case BuiltInOp.ChanceDoubleCast:
                    if (ctx.trigger == RyftTrigger.OnAbilityUsed && ShouldProc())
                    { mgr.TryDoubleCast(ctx); StartInternalCooldown(); }
                    break;

                case BuiltInOp.ChanceShieldOnBattleStart:
                    if (ctx.trigger == RyftTrigger.OnBattleStart && ShouldProc())
                    { mgr.ApplyBarrierPercentToPlayer(Def.floatMagnitude); StartInternalCooldown(); }
                    break;

                case BuiltInOp.ChanceHealOnHit:
                    if (ctx.trigger == RyftTrigger.OnDamageDealt && ShouldProc())
                    { mgr.HealPlayer(Def.intMagnitude); StartInternalCooldown(); }
                    break;

                case BuiltInOp.ChanceIgnoreDefense:
                    if (ctx.trigger == RyftTrigger.OnAbilityUsed && ShouldProc())
                    { mgr.FlagNextPlayerAttackIgnoreDefense(); StartInternalCooldown(); }
                    break;

                // ── New Triggered (A) — Orange Combat ──

                case BuiltInOp.GainEnergyOnTurnStart:
                    if (ctx.trigger == RyftTrigger.OnTurnStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null) fsc.GainEnergy(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("ENERGY", $"+{Def.intMagnitude * stacks} energy on turn start");
                    }
                    break;

                case BuiltInOp.LoseEnergyOnTurnStart:
                    if (ctx.trigger == RyftTrigger.OnTurnStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null) fsc.LoseEnergy(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("ENERGY", $"-{Def.intMagnitude * stacks} energy on turn start");
                    }
                    break;

                case BuiltInOp.DrawOnTurnStart:
                    if (ctx.trigger == RyftTrigger.OnTurnStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null) fsc.DrawCards(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("DRAW", $"+{Def.intMagnitude * stacks} cards on turn start");
                    }
                    break;

                case BuiltInOp.ProtectionOnBattleStart:
                    if (ctx.trigger == RyftTrigger.OnBattleStart)
                    {
                        var player = mgr.PlayerActor;
                        if (player != null)
                            CombatBuffManager.GainProtection(player, Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("PROT", $"+{Def.intMagnitude * stacks} protection on battle start");
                    }
                    break;

                case BuiltInOp.HealOnTurnEnd:
                    if (ctx.trigger == RyftTrigger.OnTurnEnd)
                    {
                        mgr.HealPlayer(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("HEAL", $"+{Def.intMagnitude * stacks} HP on turn end");
                    }
                    break;

                case BuiltInOp.DamageOnTurnEnd:
                    if (ctx.trigger == RyftTrigger.OnTurnEnd)
                    {
                        mgr.EnsurePlayerRef();
                        var player = mgr.PlayerActor;
                        if (player != null) player.ApplyDamage(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("DMG", $"Take {Def.intMagnitude * stacks} damage on turn end");
                    }
                    break;

                case BuiltInOp.WeaknessOnEnemies:
                    if (ctx.trigger == RyftTrigger.OnBattleStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null)
                        {
                            foreach (var enemy in fsc.AllAliveEnemies())
                                CombatBuffManager.ApplyDebuff(enemy, StatusEffectType.Weakness, Def.intMagnitude * stacks);
                        }
                        mgr.DebugLogEffectAction("DEBUFF", $"Applied {Def.intMagnitude * stacks} Weakness to all enemies");
                    }
                    break;

                case BuiltInOp.WeaknessOnPlayer:
                    if (ctx.trigger == RyftTrigger.OnBattleStart)
                    {
                        var player = mgr.PlayerActor;
                        if (player != null)
                            CombatBuffManager.ApplyDebuff(player, StatusEffectType.Weakness, Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("DEBUFF", $"Applied {Def.intMagnitude * stacks} Weakness to player");
                    }
                    break;

                case BuiltInOp.ProtectionOnEnemies:
                    if (ctx.trigger == RyftTrigger.OnBattleStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null)
                        {
                            foreach (var enemy in fsc.AllAliveEnemies())
                                CombatBuffManager.GainProtection(enemy, Def.intMagnitude * stacks);
                        }
                        mgr.DebugLogEffectAction("PROT", $"Enemies start with {Def.intMagnitude * stacks} protection");
                    }
                    break;

                case BuiltInOp.ExtraCardsOnBattleStart:
                    if (ctx.trigger == RyftTrigger.OnBattleStart)
                    {
                        var fsc = FightSceneController.Instance;
                        if (fsc != null) fsc.DrawCards(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("DRAW", $"+{Def.intMagnitude * stacks} extra starting cards");
                    }
                    break;

                case BuiltInOp.HealOnKill:
                    if (ctx.trigger == RyftTrigger.OnEnemyDefeated)
                    {
                        mgr.HealPlayer(Def.intMagnitude * stacks);
                        mgr.DebugLogEffectAction("HEAL", $"+{Def.intMagnitude * stacks} HP on kill");
                    }
                    break;

                // Passive Query ops need no handler — they're polled by consuming systems
                default:
                    break;
            }
        }
    }
}
