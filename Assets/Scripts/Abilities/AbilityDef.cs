using UnityEngine;

namespace Game.Abilities
{
    public enum TargetingType { None, Self, SingleEnemy, AllEnemies }

    [CreateAssetMenu(menuName = "Game/Ability", fileName = "Ability_")]
    public class AbilityDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;          // unique key
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Gameplay")]
        public TargetingType targeting = TargetingType.SingleEnemy;
        public int baseCooldown = 1;

        [Header("Numbers (optional, used by generic abilities)")]
        public int power = 5;      // e.g., damage/heal base amount
        public int scaling = 1;    // applied with strength, etc.

        [Header("Runtime Class")]
        // Fully-qualified type name that inherits from AbilityRuntime (e.g., "Game.Abilities.ShootAbility")
        public string runtimeTypeName;
    }
}
