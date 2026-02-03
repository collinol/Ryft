using UnityEngine;

namespace Game.Enemies.Elite
{
    /// <summary>
    /// Golem: Very high HP, slow but devastating attacks.
    /// </summary>
    public class GolemEnemy : EnemyBase
    {
        private int turnCount = 0;

        protected override void Awake()
        {
            displayName = "Stone Golem";
            baseStats.maxHealth = 120;
            baseStats.strength = 4;
            base.Awake();
            abilityIds = new[] { "SlamAbility", "RockArmorAbility", "EarthquakeAbility" };
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;

            turnCount++;

            // First turn: Rock Armor
            if (turnCount == 1)
                return abilityIds.Length > 1 ? abilityIds[1] : abilityIds[0];

            // Every 4 turns: Earthquake (AoE)
            if (turnCount % 4 == 0 && abilityIds.Length > 2)
                return abilityIds[2];

            // Every 6 turns: Refresh Rock Armor
            if (turnCount % 6 == 0 && abilityIds.Length > 1)
                return abilityIds[1];

            // Default: Slam
            return abilityIds[0];
        }
    }
}
