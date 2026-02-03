using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Equipment
{
    public enum EquipmentRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    [CreateAssetMenu(menuName = "Game/Equipment Database", fileName = "EquipmentDatabase")]
    public class EquipmentDatabase : ScriptableObject
    {
        [SerializeField] private List<EquipmentDef> items = new();
        public IReadOnlyList<EquipmentDef> Items => items;
        public int Count => items?.Count ?? 0;
        private Dictionary<string, EquipmentDef> byId;

        [Serializable]
        public struct RarityWeights
        {
            public int common;
            public int uncommon;
            public int rare;
            public int epic;
            public int legendary;

            public static RarityWeights Default => new RarityWeights
            {
                common = 50,
                uncommon = 30,
                rare = 15,
                epic = 4,
                legendary = 1
            };

            public int For(EquipmentRarity r) => r switch
            {
                EquipmentRarity.Common => common,
                EquipmentRarity.Uncommon => uncommon,
                EquipmentRarity.Rare => rare,
                EquipmentRarity.Epic => epic,
                EquipmentRarity.Legendary => legendary,
                _ => 0
            };
        }

        [Header("Reward Settings")]
        public RarityWeights rewardWeights = RarityWeights.Default;

        private const string ResNoSpace = "Equipment/EquipmentDatabase";
        private const string ResWithSpace = "Equipment/Equipment Database";
        public EquipmentDef Get(int index)
        {
            if (items == null || index < 0 || index >= items.Count) return null;
            return items[index];
        }
        public static EquipmentDatabase Load()
        {
            var db = Resources.Load<EquipmentDatabase>(ResNoSpace)
                  ?? Resources.Load<EquipmentDatabase>(ResWithSpace);
            if (!db)
            {
                Debug.LogError($"EquipmentDatabase not found at Resources/{ResNoSpace} or Resources/{ResWithSpace}");
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
            foreach (var it in items)
            {
                if (!it || string.IsNullOrWhiteSpace(it.id)) continue;
                if (!byId.ContainsKey(it.id)) byId.Add(it.id, it);
            }
        }

        public IReadOnlyList<EquipmentDef> All => items;
        public EquipmentDef Get(string id) => (string.IsNullOrWhiteSpace(id) || byId == null) ? null
                                                 : (byId.TryGetValue(id.Trim(), out var v) ? v : null);

        public void SetItems(List<EquipmentDef> list) { items = list ?? new(); Build(); }

        /// <summary>
        /// Roll a random equipment reward, weighted by rarity.
        /// </summary>
        public EquipmentDef RollReward(System.Random rng = null, EquipmentRarity? minRarity = null)
        {
            var pool = items.Where(e => e != null && (!minRarity.HasValue || e.rarity >= minRarity.Value)).ToList();
            if (pool.Count == 0) return null;

            rng ??= new System.Random();

            int total = 0;
            var cumulative = new List<(EquipmentDef equip, int sum)>(pool.Count);

            foreach (var e in pool)
            {
                int w = Mathf.Max(1, rewardWeights.For(e.rarity));
                total += w;
                cumulative.Add((e, total));
            }

            if (total <= 0) return pool[rng.Next(pool.Count)];

            int roll = rng.Next(1, total + 1);
            foreach (var (equip, sum) in cumulative)
                if (roll <= sum) return equip;

            return cumulative[cumulative.Count - 1].equip;
        }

        /// <summary>
        /// Get multiple unique equipment rewards.
        /// </summary>
        public List<EquipmentDef> RollRewards(int count, System.Random rng = null, EquipmentRarity? minRarity = null)
        {
            var results = new List<EquipmentDef>();
            var available = items.Where(e => e != null && (!minRarity.HasValue || e.rarity >= minRarity.Value)).ToList();
            rng ??= new System.Random();

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int total = 0;
                var cumulative = new List<(EquipmentDef equip, int sum)>();

                foreach (var e in available)
                {
                    int w = Mathf.Max(1, rewardWeights.For(e.rarity));
                    total += w;
                    cumulative.Add((e, total));
                }

                if (total <= 0) break;

                int roll = rng.Next(1, total + 1);
                EquipmentDef picked = null;
                foreach (var (equip, sum) in cumulative)
                {
                    if (roll <= sum) { picked = equip; break; }
                }

                if (picked != null)
                {
                    results.Add(picked);
                    available.Remove(picked);
                }
            }

            return results;
        }
    }
}
