using UnityEngine;
using Game.Combat;
using Game.Cards;

namespace Game.Ryfts.Effects
{
    /// <summary>First attack each turn deals double damage.</summary>
    public class FirstAttackDoubleDamageEffect : RyftEffectRuntime
    {
        private bool usedThisTurn;

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger == RyftTrigger.OnTurnStart)
            {
                usedThisTurn = false;
            }
            else if (ctx.trigger == RyftTrigger.OnAbilityUsed && !usedThisTurn)
            {
                if (ctx.cardDef != null && ctx.cardDef.cardType == CardType.Attack && mgr.IsPlayer(ctx.source))
                {
                    usedThisTurn = true;
                    mgr.SetNextOutgoingDamageMultiplier(2f, "FirstAttackDouble", this);
                    mgr.DebugLogEffectAction("DOUBLE", "First attack this turn deals double damage!");
                }
            }
        }
    }

    /// <summary>Playing a Defend card grants bonus protection.</summary>
    public class DefendGrantsProtectionEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (ctx.cardDef == null || ctx.cardDef.cardType != CardType.Defend) return;

            int bonus = Def.intMagnitude * stacks;
            var player = mgr.PlayerActor;
            if (player != null)
            {
                CombatBuffManager.GainProtection(player, bonus);
                mgr.DebugLogEffectAction("PROT", $"+{bonus} protection from Defend card");
            }
        }
    }

    /// <summary>10% of protection carries over between turns.</summary>
    public class ProtectionCarryOverEffect : RyftEffectRuntime
    {
        private int savedProtection;

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger == RyftTrigger.OnTurnEnd)
            {
                var player = mgr.PlayerActor;
                if (player != null)
                {
                    int currentProt = CombatBuffManager.GetProtection(player);
                    float pct = Def.floatMagnitude * stacks;
                    savedProtection = Mathf.RoundToInt(currentProt * pct);
                    if (savedProtection > 0)
                        mgr.DebugLogEffectAction("CARRY", $"Saving {savedProtection} protection ({pct:P0})");
                }
            }
            else if (ctx.trigger == RyftTrigger.OnTurnStart)
            {
                if (savedProtection > 0)
                {
                    var player = mgr.PlayerActor;
                    if (player != null)
                    {
                        CombatBuffManager.GainProtection(player, savedProtection);
                        mgr.DebugLogEffectAction("CARRY", $"Restored {savedProtection} protection from last turn");
                    }
                    savedProtection = 0;
                }
            }
        }
    }

    /// <summary>Taking damage discards a random card from hand.</summary>
    public class DamageDiscardsCardEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnDamageTaken) return;
            if (!mgr.IsPlayer(ctx.target)) return;
            if (ctx.amount <= 0) return;

            var fsc = FightSceneController.Instance;
            if (fsc != null)
            {
                fsc.DiscardRandomCard();
                mgr.DebugLogEffectAction("DISCARD", "Discarded a random card from damage");
            }
        }
    }
}
