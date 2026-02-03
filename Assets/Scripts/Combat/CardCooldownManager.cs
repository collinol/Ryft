using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Manages cooldowns for cards
    /// </summary>
    public class CardCooldownManager : MonoBehaviour
    {
        public static CardCooldownManager Instance { get; private set; }

        // cardId -> turns remaining
        private readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Set a cooldown for a card
        /// </summary>
        public void SetCooldown(string cardId, int turns)
        {
            if (string.IsNullOrEmpty(cardId) || turns <= 0) return;

            cooldowns[cardId] = turns;
            Debug.Log($"[Cooldown] {cardId} on cooldown for {turns} turns");
        }

        /// <summary>
        /// Reduce cooldown for a specific card
        /// </summary>
        public void ReduceCooldown(string cardId, int amount)
        {
            if (string.IsNullOrEmpty(cardId)) return;

            if (cooldowns.ContainsKey(cardId))
            {
                cooldowns[cardId] = Mathf.Max(0, cooldowns[cardId] - amount);
                Debug.Log($"[Cooldown] {cardId} reduced by {amount}, now {cooldowns[cardId]} turns remaining");

                if (cooldowns[cardId] <= 0)
                {
                    cooldowns.Remove(cardId);
                }
            }
        }

        /// <summary>
        /// Reset a specific card's cooldown
        /// </summary>
        public void ResetCooldown(string cardId)
        {
            if (cooldowns.Remove(cardId))
            {
                Debug.Log($"[Cooldown] {cardId} cooldown reset");
            }
        }

        /// <summary>
        /// Reset all cooldowns
        /// </summary>
        public void ResetAllCooldowns()
        {
            int count = cooldowns.Count;
            cooldowns.Clear();
            Debug.Log($"[Cooldown] All cooldowns reset ({count} cards)");
        }

        /// <summary>
        /// Reset all cooldowns of a specific stat type
        /// </summary>
        public void ResetCooldownsByType(string typePrefix)
        {
            var toRemove = new List<string>();
            foreach (var key in cooldowns.Keys)
            {
                if (key.Contains(typePrefix))
                {
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                cooldowns.Remove(key);
            }

            if (toRemove.Count > 0)
            {
                Debug.Log($"[Cooldown] Reset {toRemove.Count} {typePrefix} cooldowns");
            }
        }

        /// <summary>
        /// Check if a card is on cooldown
        /// </summary>
        public bool IsOnCooldown(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return false;
            return cooldowns.ContainsKey(cardId) && cooldowns[cardId] > 0;
        }

        /// <summary>
        /// Get remaining cooldown turns for a card
        /// </summary>
        public int GetCooldownTurns(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return 0;
            return cooldowns.ContainsKey(cardId) ? cooldowns[cardId] : 0;
        }

        /// <summary>
        /// Tick all cooldowns at the start of turn
        /// </summary>
        public void TickAllCooldowns()
        {
            var toRemove = new List<string>();

            foreach (var key in cooldowns.Keys)
            {
                cooldowns[key]--;
                if (cooldowns[key] <= 0)
                {
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                cooldowns.Remove(key);
                Debug.Log($"[Cooldown] {key} ready!");
            }
        }

        /// <summary>
        /// Clear all cooldowns
        /// </summary>
        public void ClearAll()
        {
            cooldowns.Clear();
        }
    }
}
