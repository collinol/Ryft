// Assets/Scripts/Abilities/EnemyAbilityDatabase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Abilities.Enemy
{
    /// Separate database for ENEMY abilities only.
    [CreateAssetMenu(menuName = "Game/Enemy Ability Database", fileName = "EnemyAbilityDatabase")]
    public class EnemyAbilityDatabase : ScriptableObject
    {
        [SerializeField] private List<AbilityDef> abilities = new();

        private Dictionary<string, AbilityDef> byId;
        private Dictionary<string, AbilityDef> byName;

        // Resources paths for auto-load
        private const string PathNoSpace  = "EnemyAbilities/EnemyAbilityDatabase";
        private const string PathWithSpace = "EnemyAbilities/Enemy Ability Database";

        public static EnemyAbilityDatabase Load()
        {
            var db = Resources.Load<EnemyAbilityDatabase>(PathNoSpace)
                  ?? Resources.Load<EnemyAbilityDatabase>(PathWithSpace);

            if (!db)
            {
                Debug.LogError(
                    $"EnemyAbilityDatabase not found at Resources/{PathNoSpace} or Resources/{PathWithSpace}");
                return null;
            }

            db.Build();
            return db;
        }

        private void OnEnable()   => Build();
        private void OnValidate() => Build();

        private void Build()
        {
            if (abilities == null) abilities = new();

            byId   = new Dictionary<string, AbilityDef>(StringComparer.OrdinalIgnoreCase);
            byName = new Dictionary<string, AbilityDef>(StringComparer.OrdinalIgnoreCase);

            foreach (var a in abilities)
            {
                if (!a) continue;

                if (!string.IsNullOrWhiteSpace(a.id) && !byId.ContainsKey(a.id))
                    byId.Add(a.id, a);

                if (!string.IsNullOrWhiteSpace(a.name) && !byName.ContainsKey(a.name))
                    byName.Add(a.name, a);
            }
        }

        public AbilityDef Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            key = key.Trim();

            if (byId != null && byId.TryGetValue(key, out var hit)) return hit;
            if (byName != null && byName.TryGetValue(key, out var hitName)) return hitName;

            return null;
        }

        public AbilityRuntime CreateRuntime(string id, Core.IActor owner)
        {
            var def = Get(id);
            if (!def) return null;

            var type = Type.GetType(def.runtimeTypeName);
            if (type == null)
            {
                Debug.LogError($"Enemy ability runtime type not found: {def.runtimeTypeName}");
                return null;
            }

            var runtime = Activator.CreateInstance(type) as AbilityRuntime;
            if (runtime == null)
            {
                Debug.LogError($"Type '{def.runtimeTypeName}' is not an AbilityRuntime.");
                return null;
            }

            runtime.Bind(def, owner);
            return runtime;
        }

        public void SetAbilities(List<AbilityDef> list)
        {
            abilities = list ?? new List<AbilityDef>();
            Build();
        }
    }
}
