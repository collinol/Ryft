using System;
using Game.Core;
using Game.Abilities;
using Game.Combat;

namespace Game.Ryfts
{
    /// Lightweight event bus so ryft effects can react without tight coupling.
    public static class RyftCombatEvents
    {
        public static event Action<FightContext>                OnBattleStart;
        public static event Action                              OnBattleEnd;
        public static event Action                              OnTurnStart;
        public static event Action                              OnTurnEnd;

        public static event Action<IActor, AbilityDef, FightContext> OnAbilityUsed;
        public static event Action<IActor, AbilityDef, FightContext> OnAbilityResolved;

        public static event Action<IActor, IActor, int>         OnDamageDealt; // (src, tgt, dmgPostMitigation)
        public static event Action<IActor, int>                 OnDamageTaken; // (tgt, dmgPostMitigation)
        public static event Action<IActor>                      OnEnemyDefeated;

        // --- Raisers (call these from your controller/actors) ---
        public static void RaiseBattleStart(FightContext c)          => OnBattleStart?.Invoke(c);
        public static void RaiseBattleEnd()                          => OnBattleEnd?.Invoke();
        public static void RaiseTurnStart()                          => OnTurnStart?.Invoke();
        public static void RaiseTurnEnd()                            => OnTurnEnd?.Invoke();
        public static void RaiseAbilityUsed(IActor s, AbilityDef a, FightContext c)     => OnAbilityUsed?.Invoke(s, a, c);
        public static void RaiseAbilityResolved(IActor s, AbilityDef a, FightContext c) => OnAbilityResolved?.Invoke(s, a, c);
        public static void RaiseDamageDealt(IActor s, IActor t, int dmg) => OnDamageDealt?.Invoke(s, t, dmg);
        public static void RaiseDamageTaken(IActor t, int dmg)          => OnDamageTaken?.Invoke(t, dmg);
        public static void RaiseEnemyDefeated(IActor e)                 => OnEnemyDefeated?.Invoke(e);
    }
}
