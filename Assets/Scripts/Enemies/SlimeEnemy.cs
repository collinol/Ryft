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
            if (sourceDef == null)
            {
                displayName = "Slime";
                baseStats.maxHealth = 25;
                baseStats.strength = 1;
                abilityIds = new[] { "SlimeAttackAbility" };
            }
            base.Awake();
        }
    }
}
