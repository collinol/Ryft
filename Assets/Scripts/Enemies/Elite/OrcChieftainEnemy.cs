using UnityEngine;

namespace Game.Enemies.Elite
{
    /// <summary>
    /// Orc Chieftain: Powerful elite with war cry and heavy attacks.
    /// </summary>
    public class OrcChieftainEnemy : EnemyBase
    {
        private int turnCount = 0;

        protected override void Awake()
        {
            displayName = "Orc Chieftain";
            baseStats.maxHealth = 80;
            baseStats.strength = 6;
            base.Awake();
            abilityIds = new[] { "WarCryAbility", "HeavySmashAbility", "RallyAbility", "EnemyStrikeAbility" };
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;

            turnCount++;

            // First turn: War Cry
            if (turnCount == 1) return abilityIds[0];

            // Every 4 turns: Rally
            if (turnCount % 4 == 0 && abilityIds.Length > 2) return abilityIds[2];

            // Every 3 turns: Heavy Smash
            if (turnCount % 3 == 0 && abilityIds.Length > 1) return abilityIds[1];

            // Default: Basic strike
            return abilityIds.Length > 3 ? abilityIds[3] : abilityIds[0];
        }
    }
}
