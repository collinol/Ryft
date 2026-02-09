using System;
using UnityEngine;
using Game.Core;

namespace Game.Enemies
{
    public enum EnemyTier
    {
        Regular = 0,
        Elite = 1
    }

    [Serializable]
    public struct MinionEntry
    {
        public EnemyDef minionDef;
        public int count;
        [Range(0f, 1f)] public float spawnChance;
    }

    [CreateAssetMenu(menuName = "Game/Enemy", fileName = "Enemy_")]
    public class EnemyDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite sprite;

        [Header("Classification")]
        public EnemyTier tier = EnemyTier.Regular;
        [Min(0)] public int minLevel = 0;

        [Header("Stats")]
        public Stats baseStats = new Stats { maxHealth = 20, strength = 2 };

        [Header("Abilities")]
        [Tooltip("IDs must match AbilityDef.id values in EnemyAbilityDatabase.")]
        public string[] abilityIds;

        [Header("Custom Behavior")]
        [Tooltip("Fully-qualified C# type for enemies with custom AI (e.g. Game.Enemies.BanditEnemy). Leave empty for GenericEnemy.")]
        public string runtimeTypeName;

        [Header("Minions (Elite encounters)")]
        public MinionEntry[] minions;

        [Header("Fallback Visuals")]
        [Tooltip("Used to generate a procedural sprite if no sprite is assigned.")]
        public Color proceduralColor = Color.green;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name;
        }
#endif
    }
}
