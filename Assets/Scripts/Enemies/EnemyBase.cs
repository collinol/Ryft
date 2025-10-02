using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Abilities;
using Game.Abilities.Enemy;
using Game.UI;

namespace Game.Enemies
{
    public abstract class EnemyBase : MonoBehaviour, IActor
    {
        [Header("Identity")]
        [SerializeField] protected string displayName = "Enemy";

        [Header("Stats")]
        [SerializeField] protected Stats baseStats = new Stats { maxHealth = 10, strength = 2, defense = 0 };

        public string DisplayName => displayName;
        public Stats   BaseStats  => baseStats;
        public Stats   TotalStats => baseStats;
        public int     Health     { get; private set; }
        public bool    IsAlive    => Health > 0;

        private HealthBarView hpBar; // <—

        [Header("Enemy Ability IDs (from EnemyAbilityDatabase)")]
        [Tooltip("IDs must match AbilityDef.id values in EnemyAbilityDatabase (e.g., \"EnemyStrike\").")]
        public string[] abilityIds;

        private List<AbilityRuntime> _abilityRuntimes;

        protected virtual void Awake()
        {
            // If a subclass changes stats, do that BEFORE base.Awake()
            Health = Mathf.Max(1, baseStats.maxHealth);

            // Create and snap the bar
            hpBar = HealthBarView.Attach(transform, new Vector3(0f, 1.5f, 0f));
            hpBar.Set(Health, TotalStats.maxHealth);
        }

        // IActor
        public void ApplyDamage(int amount)
        {
            var mitigated = Mathf.Max(0, amount - TotalStats.defense);
            Health = Mathf.Max(0, Health - mitigated);
            hpBar?.Set(Health, TotalStats.maxHealth);
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(TotalStats.maxHealth, Health + Mathf.Max(0, amount));
            hpBar?.Set(Health, TotalStats.maxHealth);
        }

        // Enemy abilities only
        public List<AbilityRuntime> EnsureEnemyAbilityRuntimes(EnemyAbilityDatabase db)
        {
            if (_abilityRuntimes != null) return _abilityRuntimes;
            _abilityRuntimes = new List<AbilityRuntime>();
            if (db == null || abilityIds == null || abilityIds.Length == 0) return _abilityRuntimes;

            foreach (var id in abilityIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                var rt = db.CreateRuntime(id, this);
                if (rt != null) _abilityRuntimes.Add(rt);
            }
            return _abilityRuntimes;
        }

        public virtual AbilityRuntime PickRandomEnemyAbilityRuntime()
        {
            if (_abilityRuntimes == null || _abilityRuntimes.Count == 0) return null;
            return _abilityRuntimes[Random.Range(0, _abilityRuntimes.Count)];
        }
    }
}
