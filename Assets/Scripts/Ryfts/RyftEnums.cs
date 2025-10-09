namespace Game.Ryfts
{
    public enum RyftColor { Orange, Green, Blue, Yellow, Purple }

    public enum EffectPolarity { Positive, Negative }
    public enum EffectLifetime { Permanent, UntilBattleEnd, DurationNTurns }
    public enum RyftTrigger
    {
        OnRyftClosed, OnRyftExploded,
        OnBattleStart, OnTurnStart, OnTurnEnd, OnBattleEnd,
        OnAbilityUsed, OnAbilityResolved,
        OnDamageDealt, OnDamageTaken, OnEnemyDefeated
    }
}
