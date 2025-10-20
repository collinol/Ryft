using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "Game/Card Database", fileName = "CardDatabase")]
    public class CardDatabase : ScriptableObject
    {
        // ————— Data model ———————————————————————————————————————————————

        [Serializable]
        public class Availability
        {
            public CardDef card;            // reference into the catalog
            [Min(0)] public int available;  // how many copies the player currently owns/unlocked
            [Min(0)] public int maxCopies = 99; // optional cap (e.g., 4 for CCG-like)
            public bool rewardEligible = true;  // if false, never offered as reward
        }

        [Header("Universe of cards (ALL that exist in your game)")]
        [SerializeField] private List<CardDef> catalog = new();

        [Header("What the player currently owns/unlocked")]
        [SerializeField] private List<Availability> availability = new();

        // quick lookups
        private Dictionary<string, CardDef> byId;            // all cards by id
        private Dictionary<string, Availability> availById;  // owned by id

        // common resource paths used by Load()
        private const string PathNoSpace   = "Cards/CardDatabase";
        private const string PathWithSpace = "Cards/Card Database";

        // ————— Lifecycle ————————————————————————————————————————————————

        public static CardDatabase Load()
        {
            var db = Resources.Load<CardDatabase>(PathNoSpace) ?? Resources.Load<CardDatabase>(PathWithSpace);
            if (!db)
            {
                Debug.LogError($"CardDatabase not found at Resources/{PathNoSpace} or Resources/{PathWithSpace}");
                return null;
            }
            db.Build();
            return db;
        }

        void OnEnable()   => Build();
        void OnValidate() => Build();

        private void Build()
        {
            byId = new Dictionary<string, CardDef>(StringComparer.OrdinalIgnoreCase);
            availById = new Dictionary<string, Availability>(StringComparer.OrdinalIgnoreCase);

            // index catalog
            foreach (var c in catalog)
            {
                if (!c || string.IsNullOrWhiteSpace(c.id)) continue;
                if (!byId.ContainsKey(c.id)) byId.Add(c.id, c);
            }

            // index availability
            foreach (var a in availability)
            {
                if (a?.card == null || string.IsNullOrWhiteSpace(a.card.id)) continue;
                if (!availById.ContainsKey(a.card.id)) availById.Add(a.card.id, a);
            }
        }

        // ————— Queries ————————————————————————————————————————————————

        public IReadOnlyList<CardDef> Catalog => catalog;
        public IReadOnlyList<Availability> AvailabilityList => availability;

        public CardDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return byId != null && byId.TryGetValue(id.Trim(), out var def) ? def : null;
        }

        public int GetAvailableCopies(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return 0;
            return (availById != null && availById.TryGetValue(id.Trim(), out var a)) ? Mathf.Max(0, a.available) : 0;
        }

        // ————— Mutation helpers (grant/spend) ————————————————————————

        public void Grant(string id, int copies = 1)
        {
            if (copies <= 0) return;
            var def = Get(id);
            if (!def) return;

            if (!availById.TryGetValue(id, out var entry))
            {
                entry = new Availability { card = def, available = 0, maxCopies = 99, rewardEligible = true };
                availability.Add(entry);
                availById[id] = entry;
            }

            entry.available = Mathf.Clamp(entry.available + copies, 0, Mathf.Max(1, entry.maxCopies));
        }

        public bool TryConsumeCopyForDeckBuild(string id)
        {
            if (!availById.TryGetValue(id, out var entry)) return false;
            if (entry.available <= 0) return false;
            entry.available -= 1;
            return true;
        }

        // ————— Deck building —————————————————————————————————————————————

        /// <summary>
        /// Builds a deck list expanded by available copy counts.
        /// NOTE: This returns a fresh list and does NOT mutate availability.
        /// </summary>
        public List<CardDef> BuildPlayerDeck()
        {
            var deck = new List<CardDef>();
            foreach (var a in availability)
            {
                if (a?.card == null || a.available <= 0) continue;
                for (int i = 0; i < a.available; i++)
                    deck.Add(a.card);
            }
            return deck;
        }

        // ————— Rewards (rarity-weighted) ——————————————————————————————

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
                common = 60,
                uncommon = 25,
                rare = 10,
                epic = 4,
                legendary = 1
            };

            public int For(CardRarity r) => r switch
            {
                CardRarity.Common    => common,
                CardRarity.Uncommon  => uncommon,
                CardRarity.Rare      => rare,
                CardRarity.Epic      => epic,
                CardRarity.Legendary => legendary,
                _ => 0
            };
        }

        [Header("Reward Settings")]
        public RarityWeights rewardWeights = RarityWeights.Default;

        /// <summary>
        /// Returns candidates that can be offered as rewards (e.g., not disabled, optionally not at copy cap).
        /// </summary>
        public IEnumerable<CardDef> GetRewardPool(bool excludeAtMaxCopies = true)
        {
            foreach (var c in catalog)
            {
                if (!c) continue;
                if (availById.TryGetValue(c.id, out var a))
                {
                    if (!a.rewardEligible) continue;
                    if (excludeAtMaxCopies && a.available >= a.maxCopies) continue;
                    yield return c;
                }
                else
                {
                    // Not owned yet → eligible by default unless you decide otherwise
                    yield return c;
                }
            }
        }

        /// <summary>
        /// Weighted random reward pick by rarity. Returns null if pool empty.
        /// minRarity lets you gate offers (e.g., boss chest).
        /// </summary>
        public CardDef RollReward(System.Random rng = null, CardRarity? minRarity = null)
        {
            var pool = GetRewardPool(excludeAtMaxCopies: true)
                .Where(c => !minRarity.HasValue || c.rarity >= minRarity.Value)
                .ToList();

            if (pool.Count == 0) return null;

            rng ??= new System.Random();

            int total = 0;
            var cumulative = new List<(CardDef card, int sum)>(pool.Count);

            foreach (var c in pool)
            {
                int w = Mathf.Max(0, rewardWeights.For(c.rarity));
                if (w <= 0) continue;
                total += w;
                cumulative.Add((c, total));
            }

            if (total <= 0) return null;

            int roll = rng.Next(1, total + 1);
            foreach (var (card, sum) in cumulative)
                if (roll <= sum) return card;

            return cumulative[cumulative.Count - 1].card; // fallback
        }
    }
}
