using UnityEngine;

namespace Game.Enemies.Elite
{
    /// <summary>
    /// Necromancer: Low HP but summons minions and drains life.
    /// </summary>
    public class NecromancerEnemy : EnemyBase
    {
        private int turnCount = 0;
        private int summonCount = 0;
        private const int MaxSummons = 3;

        protected override void Awake()
        {
            displayName = "Necromancer";
            baseStats.maxHealth = 60;
            baseStats.strength = 3;
            base.Awake();
            abilityIds = new[] { "SummonAbility", "DrainLifeAbility", "CurseAbility" };
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;

            turnCount++;

            // First turn: Always summon
            if (turnCount == 1 && summonCount < MaxSummons)
            {
                summonCount++;
                return abilityIds[0];
            }

            // If low on health, prioritize drain life
            float healthPercent = (float)Health / TotalStats.maxHealth;
            if (healthPercent < 0.5f && abilityIds.Length > 1)
            {
                return abilityIds[1]; // Drain Life
            }

            // Every 3 turns: Summon if under limit
            if (turnCount % 3 == 0 && summonCount < MaxSummons)
            {
                summonCount++;
                return abilityIds[0];
            }

            // Every 4 turns: Curse
            if (turnCount % 4 == 0 && abilityIds.Length > 2)
            {
                return abilityIds[2];
            }

            // Default: Drain Life
            return abilityIds.Length > 1 ? abilityIds[1] : abilityIds[0];
        }
    }
}
