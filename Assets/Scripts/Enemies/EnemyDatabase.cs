using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(menuName = "Game/Enemy Database", fileName = "EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private List<EnemyDef> enemies = new();
        public IReadOnlyList<EnemyDef> All => enemies;
        public int Count => enemies?.Count ?? 0;

        private Dictionary<string, EnemyDef> byId;

        private const string ResPath = "databases/EnemyDatabase";

        public static EnemyDatabase Load()
        {
            var db = Resources.Load<EnemyDatabase>(ResPath);
            if (!db)
            {
                Debug.LogError($"EnemyDatabase not found at Resources/{ResPath} or Resources/{ResPath}");
                return null;
            }
            db.Build();
            return db;
        }

        void OnEnable()   => Build();
        void OnValidate() => Build();

        private void Build()
        {
            byId = new(StringComparer.OrdinalIgnoreCase);
            foreach (var e in enemies)
            {
                if (!e || string.IsNullOrWhiteSpace(e.id)) continue;
                if (!byId.ContainsKey(e.id)) byId.Add(e.id, e);
            }
        }

        public EnemyDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || byId == null) return null;
            return byId.TryGetValue(id.Trim(), out var v) ? v : null;
        }

        public List<EnemyDef> GetByTierAndLevel(EnemyTier tier, int level)
        {
            return enemies.Where(e => e != null && e.tier == tier && e.minLevel <= level).ToList();
        }

        public EnemyDef GetRandom(EnemyTier tier, int level)
        {
            var pool = GetByTierAndLevel(tier, level);
            if (pool.Count == 0) return null;
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        public List<EnemyDef> GetRandomDistinct(EnemyTier tier, int level, int count)
        {
            var pool = GetByTierAndLevel(tier, level);
            if (pool.Count == 0) return new List<EnemyDef>();

            // Shuffle and take up to count
            var shuffled = pool.OrderBy(_ => UnityEngine.Random.value).ToList();
            var results = new List<EnemyDef>();
            for (int i = 0; i < count; i++)
            {
                // Allow duplicates if pool is smaller than count
                results.Add(shuffled[i % shuffled.Count]);
            }
            return results;
        }

        public void SetEnemies(List<EnemyDef> list)
        {
            enemies = list ?? new();
            Build();
        }
    }
}
