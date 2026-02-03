using UnityEngine;

namespace Game.Enemies.Elite
{
    /// <summary>
    /// Dark Knight: Tanky elite with shields and dark slashes.
    /// </summary>
    public class DarkKnightEnemy : EnemyBase
    {
        private int turnCount = 0;

        protected override void Awake()
        {
            displayName = "Dark Knight";
            baseStats.maxHealth = 100;
            baseStats.strength = 5;
            base.Awake();
            abilityIds = new[] { "ShieldBashAbility", "DarkSlashAbility", "FortifyAbility" };
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;

            turnCount++;

            // First turn and every 5 turns: Fortify
            if (turnCount == 1 || turnCount % 5 == 0)
                return abilityIds.Length > 2 ? abilityIds[2] : abilityIds[0];

            // Every 3 turns: Shield Bash (stun chance)
            if (turnCount % 3 == 0) return abilityIds[0];

            // Default: Dark Slash
            return abilityIds.Length > 1 ? abilityIds[1] : abilityIds[0];
        }
    }
}
