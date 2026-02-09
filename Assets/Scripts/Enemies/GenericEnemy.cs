using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Data-only enemy — reads all stats from an EnemyDef.
    /// Used for enemies that don't need custom AI logic.
    /// </summary>
    public class GenericEnemy : EnemyBase
    {
        protected override void Awake()
        {
            // Stats already set by InitFromDef before activation
            base.Awake();
        }

        protected override string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;
            return abilityIds[Random.Range(0, abilityIds.Length)];
        }
    }
}
