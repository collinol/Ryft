// Assets/Scripts/Enemies/GoblinEnemy.cs
using UnityEngine;

namespace Game.Enemies
{
    /// Goblin: can ONLY use EnemyStrike via AbilityDatabase.
    public class GoblinEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            displayName = "Goblin";
            baseStats.maxHealth = 18;
            baseStats.strength = 2;
            baseStats.defense = 0;
            base.Awake();

            abilityIds = new[] { "EnemyStrikeAbility" };
        }
    }
}
