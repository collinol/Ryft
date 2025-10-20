// Assets/Scripts/Ryfts/RyftCombatEvents.cs
using System;
using Game.Core;      // StatField
using Game.Combat;
using Game.Cards;

namespace Game.Ryfts
{
    public static class RyftCombatEvents
    {
        // Battle flow
        public static event Action<FightContext> OnBattleStart;
        public static event Action OnBattleEnd;
        public static event Action OnTurnStart;
        public static event Action OnTurnEnd;

        // Card flow
        public static event Action<IActor, CardDef, FightContext> OnAbilityUsed;
        public static event Action<IActor, CardDef, FightContext> OnAbilityResolved;

        // Combat updates
        public static event Action<IActor, IActor, int> OnDamageDealt;
        public static event Action<IActor, int> OnDamageTaken;
        public static event Action<IActor> OnEnemyDefeated;

        // Immediate resource refund (single definition)
        public static event Action<IActor, StatField, int> OnResourceRefund;

        // Raisers
        public static void RaiseBattleStart(FightContext ctx) => OnBattleStart?.Invoke(ctx);
        public static void RaiseBattleEnd()                   => OnBattleEnd?.Invoke();
        public static void RaiseTurnStart()                   => OnTurnStart?.Invoke();
        public static void RaiseTurnEnd()                     => OnTurnEnd?.Invoke();

        public static void RaiseAbilityUsed(IActor who, CardDef def, FightContext ctx)
            => OnAbilityUsed?.Invoke(who, def, ctx);

        public static void RaiseAbilityResolved(IActor who, CardDef def, FightContext ctx)
            => OnAbilityResolved?.Invoke(who, def, ctx);

        public static void RaiseDamageDealt(IActor src, IActor tgt, int amount)
            => OnDamageDealt?.Invoke(src, tgt, amount);

        public static void RaiseDamageTaken(IActor tgt, int amount)
            => OnDamageTaken?.Invoke(tgt, amount);

        public static void RaiseEnemyDefeated(IActor enemy)
            => OnEnemyDefeated?.Invoke(enemy);

        public static void RaiseResourceRefund(IActor who, StatField field, int amount)
            => OnResourceRefund?.Invoke(who, field, Math.Max(0, amount));
    }
}
