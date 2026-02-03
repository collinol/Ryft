using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Cultist: Support enemy that curses and performs dark rituals.
    /// </summary>
    public class CultistEnemy : EnemyBase
    {
        protected override void Awake()
        {
            displayName = "Cultist";
            baseStats.maxHealth = 20;
            baseStats.strength = 2;
            base.Awake();
            abilityIds = new[] { "CurseAbility", "DarkRitualAbility", "EnemyStrikeAbility" };
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;

            float roll = Random.value;
            if (roll < 0.3f) return abilityIds[0]; // Curse
            if (roll < 0.5f && abilityIds.Length > 1) return abilityIds[1]; // Dark Ritual
            return abilityIds.Length > 2 ? abilityIds[2] : abilityIds[0]; // Basic attack
        }
    }
}
