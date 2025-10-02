// Assets/Scripts/Abilities/AbilityDatabase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Abilities
{
    [CreateAssetMenu(menuName = "Game/Ability Database", fileName = "AbilityDatabase")]
    public class AbilityDatabase : ScriptableObject
    {
        [SerializeField] private List<AbilityDef> abilities = new();

        // Lookups by explicit def.id and by asset name (case-insensitive)
        private Dictionary<string, AbilityDef> byId;
        private Dictionary<string, AbilityDef> byName;

        private const string ResourcesPathNoSpace = "Abilities/AbilityDatabase";
        private const string ResourcesPathWithSpace = "Abilities/Ability Database";

        public static AbilityDatabase Load()
        {
            // Try both common filenames
            var db = Resources.Load<AbilityDatabase>(ResourcesPathNoSpace)
                  ?? Resources.Load<AbilityDatabase>(ResourcesPathWithSpace);

            if (!db)
            {
                Debug.LogError(
                    $"AbilityDatabase not found at Resources/{ResourcesPathNoSpace} or Resources/{ResourcesPathWithSpace}");
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

                // Index by explicit ID if it exists
                if (!string.IsNullOrWhiteSpace(a.id))
                {
                    if (!byId.ContainsKey(a.id))
                        byId.Add(a.id, a);
                    else
                        Debug.LogWarning($"[AbilityDatabase] Duplicate id '{a.id}' — keeping first, ignoring {a.name}.");
                }

                // Always index by asset name as a fallback
                if (!string.IsNullOrWhiteSpace(a.name))
                {
                    if (!byName.ContainsKey(a.name))
                        byName.Add(a.name, a);
                    else
                        Debug.LogWarning($"[AbilityDatabase] Duplicate asset name '{a.name}' — keeping first.");
                }
            }
        }

        public IReadOnlyList<AbilityDef> All => abilities;

        /// <summary>
        /// Returns an AbilityDef by id (preferred) or asset name (fallback).
        /// Comparison is case-insensitive and trims whitespace.
        /// </summary>
        public AbilityDef Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            key = key.Trim();

            if (byId != null && byId.TryGetValue(key, out var byIdHit))
                return byIdHit;

            if (byName != null && byName.TryGetValue(key, out var byNameHit))
                return byNameHit;

            return null;
        }

        public AbilityRuntime CreateRuntime(string id, Core.IActor owner)
        {
            var def = Get(id);
            if (!def) return null;

            // Expecting def.runtimeTypeName to be an assembly-qualified or resolvable type name of a subclass of AbilityRuntime
            var type = Type.GetType(def.runtimeTypeName);
            if (type == null)
            {
                Debug.LogError($"Ability runtime type not found: {def.runtimeTypeName}");
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

        // Helper so you can drag/drop abilities from Project into the list
        public void SetAbilities(List<AbilityDef> list)
        {
            abilities = list ?? new List<AbilityDef>();
            Build();
        }
    }
}
