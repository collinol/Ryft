using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Bandit: Medium stats, can steal gold and attack quickly.
    /// </summary>
    public class BanditEnemy : EnemyBase
    {
        protected override void Awake()
        {
            if (sourceDef == null)
            {
                displayName = "Bandit";
                baseStats.maxHealth = 18;
                baseStats.strength = 4;
                abilityIds = new[] { "DaggerStrikeAbility", "StealGoldAbility" };
            }
            base.Awake();
        }

        protected override string PickEnemyAbilityId()
        {
            // 30% chance to try stealing gold, otherwise dagger strike
            if (abilityIds == null || abilityIds.Length == 0) return null;
            return Random.value < 0.3f && abilityIds.Length > 1 ? abilityIds[1] : abilityIds[0];
        }
    }
}
