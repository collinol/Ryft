using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Skeleton: Basic undead enemy with low HP but persistent.
    /// </summary>
    public class SkeletonEnemy : EnemyBase
    {
        protected override void Awake()
        {
            if (sourceDef == null)
            {
                displayName = "Skeleton";
                baseStats.maxHealth = 15;
                baseStats.strength = 3;
                abilityIds = new[] { "EnemyStrikeAbility" };
            }
            base.Awake();
        }
    }
}
