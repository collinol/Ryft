using UnityEngine;
using Game.Combat;
using Game.Cards;

namespace Game.Ryfts.Effects
{
    /// <summary>Earn 1% interest on gold after combat.</summary>
    public class GoldInterestEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleEnd) return;

            if (MapSession.I == null) return;
            float pct = Def.floatMagnitude * stacks;
            int interest = Mathf.RoundToInt(MapSession.I.Gold * pct);
            if (interest > 0)
            {
                MapSession.I.Gold += interest; // bypass AddGold to avoid double-counting goldGainPercent
                mgr.DebugLogEffectAction("GOLD", $"+{interest} gold interest ({pct:P1} of {MapSession.I.Gold - interest})");
            }
        }
    }

    /// <summary>Earn 1 gold per damage dealt in combat.</summary>
    public class GoldPerDamageEffect : RyftEffectRuntime
    {
        private int accumulatedGold;

        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger == RyftTrigger.OnBattleStart)
            {
                accumulatedGold = 0;
            }
            else if (ctx.trigger == RyftTrigger.OnDamageDealt && mgr.IsPlayer(ctx.source))
            {
                int goldPerDmg = Mathf.RoundToInt(Def.floatMagnitude * stacks);
                accumulatedGold += ctx.amount * goldPerDmg;
            }
            else if (ctx.trigger == RyftTrigger.OnBattleEnd)
            {
                if (accumulatedGold > 0 && MapSession.I != null)
                {
                    MapSession.I.AddGold(accumulatedGold);
                    mgr.DebugLogEffectAction("GOLD", $"+{accumulatedGold} gold from damage dealt");
                }
                accumulatedGold = 0;
            }
        }
    }

    /// <summary>Lose 1% gold after each combat.</summary>
    public class GoldLossAfterCombatEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleEnd) return;

            if (MapSession.I == null) return;
            float pct = Def.floatMagnitude * stacks;
            int loss = Mathf.RoundToInt(MapSession.I.Gold * pct);
            if (loss > 0)
            {
                MapSession.I.Gold = Mathf.Max(0, MapSession.I.Gold - loss);
                mgr.DebugLogEffectAction("GOLD", $"-{loss} gold ({pct:P1} loss after combat)");
            }
        }
    }

    /// <summary>Lose 2 gold per card played.</summary>
    public class GoldLossPerCardEffect : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnAbilityResolved) return;
            if (!mgr.IsPlayer(ctx.source)) return;

            if (MapSession.I == null) return;
            int loss = Def.intMagnitude * stacks;
            MapSession.I.Gold = Mathf.Max(0, MapSession.I.Gold - loss);
            mgr.DebugLogEffectAction("GOLD", $"-{loss} gold (card played cost)");
        }
    }
}
