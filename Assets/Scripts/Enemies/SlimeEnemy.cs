using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Slime: High HP, low damage but applies slow effects.
    /// </summary>
    public class SlimeEnemy : EnemyBase
    {
        protected override void Awake()
        {
            displayName = "Slime";
            baseStats.maxHealth = 25;
            baseStats.strength = 1;
            base.Awake();
            abilityIds = new[] { "SlimeAttackAbility" };
        }
    }
}
