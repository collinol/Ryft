using UnityEngine;
using System.Linq;
using Game.Combat;
using Game.Cards;

namespace Game.Ryfts.Effects
{
    /// <summary>Copy the highest cost card into hand at battle start.</summary>
    public class HighestCostCardCopyEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;

            var fsc = FightSceneController.Instance;
            if (fsc == null) return;

            var hand = fsc.CurrentHand;
            if (hand == null || hand.Count == 0) return;

            CardDef highest = null;
            int maxCost = -1;
            foreach (var card in hand)
            {
                if (card != null && card.energyCost > maxCost)
                {
                    maxCost = card.energyCost;
                    highest = card;
                }
            }

            if (highest != null)
            {
                fsc.AddCardToHand(highest, 0);
                mgr.DebugLogEffectAction("COPY", $"Copied {highest.displayName} (cost {maxCost}) to hand");
            }
        }
    }

    /// <summary>5% chance for attack cards to strike twice.</summary>
    public class AttackDoubleStrikeEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (ctx.cardDef == null || ctx.cardDef.cardType != CardType.Attack) return;

            float chance = Def.floatMagnitude * stacks;
            if (Random.value >= chance) return;

            // Re-execute the card via the last played card system
            var rt = mgr.GetLastPlayedCardRuntime();
            if (rt != null)
            {
                var fightCtx = FightSceneController.Instance?.GetContext();
                if (fightCtx != null)
                {
                    rt.Execute(fightCtx, null);
                    mgr.DebugLogEffectAction("DOUBLE", $"Double strike triggered! ({chance:P1})");
                }
            }
        }
    }

    /// <summary>Every N card plays, draw 1 card.</summary>
    public class DrawEveryNPlaysEffect : RyftEffectRuntime
    {
        public override void OnAdded(RyftEffectManager mgr)
        {
            mgr.RegisterDrawEveryNCards(Def.intMagnitude, 1);
        }

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            // The draw logic is handled by RyftEffectManager's drawEveryN system
        }
    }

    /// <summary>0-cost cards draw a card when played.</summary>
    public class ZeroCostDrawEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (ctx.cardDef == null || ctx.cardDef.energyCost > 0) return;

            var fsc = FightSceneController.Instance;
            if (fsc != null)
            {
                fsc.DrawCards(1);
                mgr.DebugLogEffectAction("DRAW", $"0-cost card {ctx.cardDef.displayName} drew a card");
            }
        }
    }

    /// <summary>2% chance for attacks to miss (deal 0 damage).</summary>
    public class AttackMissChanceEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityUsed) return;
            if (!mgr.IsPlayer(ctx.source)) return;
            if (ctx.cardDef == null || ctx.cardDef.cardType != CardType.Attack) return;

            float chance = Def.floatMagnitude * stacks;
            if (Random.value < chance)
            {
                mgr.SetNextOutgoingDamageMultiplier(0f, "AttackMiss", this);
                mgr.DebugLogEffectAction("MISS", $"Attack missed! ({chance:P1} chance)");
            }
        }
    }
}
