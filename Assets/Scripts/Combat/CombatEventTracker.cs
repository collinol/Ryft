using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Cards;

namespace Game.Combat
{
    /// <summary>
    /// Tracks combat events for cards that trigger on kills, damage, etc.
    /// </summary>
    public class CombatEventTracker : MonoBehaviour
    {
        public static CombatEventTracker Instance { get; private set; }

        // Kill tracking
        private int killsThisTurn = 0;
        private readonly List<KillInfo> recentKills = new List<KillInfo>();

        // Event listeners
        public event Action<IActor, IActor, int> OnKill;              // (killer, victim, finalDamage)
        public event Action<IActor, int> OnDamageTaken;                // (victim, amount)
        public event Action<CardDef, IActor> OnSpellCast;              // (cardDef, caster)
        public event Action<CardDef, IActor> OnEngineeringCardPlayed;  // (cardDef, player)
        public event Action<IActor> OnGadgetActivated;                 // (player)
        public event Action OnGadgetDestroyed;                         // ()

        // Last spell tracking for EtherealReflection
        public CardDef LastSpellCast { get; private set; }
        public IActor LastSpellCaster { get; private set; }

        // Spell cast counter for SpellWeave
        private int spellsCastThisTurn = 0;

        // Mana spending tracker for RunicSurge
        private int totalManaSpentThisTurn = 0;

        private struct KillInfo
        {
            public IActor Killer;
            public IActor Victim;
            public DamageType Type;
            public int Damage;
        }

        private enum DamageType
        {
            Physical,
            Magic,
            Engineering
        }

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
        /// Call this when an enemy dies from damage
        /// </summary>
        public void RegisterKill(IActor killer, IActor victim, int damage, StatField damageSource)
        {
            killsThisTurn++;

            DamageType type = damageSource switch
            {
                StatField.Mana => DamageType.Magic,
                StatField.Engineering => DamageType.Engineering,
                _ => DamageType.Physical
            };

            recentKills.Add(new KillInfo
            {
                Killer = killer,
                Victim = victim,
                Type = type,
                Damage = damage
            });

            Debug.Log($"[CombatTracker] {killer?.DisplayName} killed {victim?.DisplayName} with {type} damage ({damage})");

            // Trigger listeners
            OnKill?.Invoke(killer, victim, damage);
        }

        /// <summary>
        /// Get kill count this turn
        /// </summary>
        public int GetKillsThisTurn() => killsThisTurn;

        /// <summary>
        /// Get kills by damage type
        /// </summary>
        public int GetMagicKillsThisTurn()
        {
            int count = 0;
            foreach (var kill in recentKills)
            {
                if (kill.Type == DamageType.Magic) count++;
            }
            return count;
        }

        /// <summary>
        /// Get Engineering kills this turn
        /// </summary>
        public int GetEngineeringKillsThisTurn()
        {
            int count = 0;
            foreach (var kill in recentKills)
            {
                if (kill.Type == DamageType.Engineering) count++;
            }
            return count;
        }

        /// <summary>
        /// Reset turn counters (call at start of player turn)
        /// </summary>
        public void ResetTurnCounters()
        {
            killsThisTurn = 0;
            recentKills.Clear();
            spellsCastThisTurn = 0;
            totalManaSpentThisTurn = 0;
        }

        /// <summary>
        /// Track spell cast
        /// </summary>
        public void RecordSpellCast(CardDef spell, IActor caster)
        {
            LastSpellCast = spell;
            LastSpellCaster = caster;
            spellsCastThisTurn++;

            OnSpellCast?.Invoke(spell, caster);
            Debug.Log($"[CombatTracker] Spell cast: {spell?.displayName} (total this turn: {spellsCastThisTurn})");
        }

        /// <summary>
        /// Check if every Nth spell should be free
        /// </summary>
        public bool IsNthSpell(int n)
        {
            return spellsCastThisTurn > 0 && spellsCastThisTurn % n == 0;
        }

        /// <summary>
        /// Track Engineering card played
        /// </summary>
        public void RecordEngineeringCard(CardDef card, IActor player)
        {
            OnEngineeringCardPlayed?.Invoke(card, player);
            Debug.Log($"[CombatTracker] Engineering card played: {card?.displayName}");
        }

        /// <summary>
        /// Track Mana spending for RunicSurge
        /// </summary>
        public void RecordManaSpent(int amount)
        {
            totalManaSpentThisTurn += amount;
        }

        public int GetTotalManaSpentThisTurn() => totalManaSpentThisTurn;

        /// <summary>
        /// Track damage taken
        /// </summary>
        public void RecordDamageTaken(IActor victim, int amount)
        {
            OnDamageTaken?.Invoke(victim, amount);
        }

        /// <summary>
        /// Track gadget activation
        /// </summary>
        public void RecordGadgetActivation(IActor player)
        {
            OnGadgetActivated?.Invoke(player);
        }

        /// <summary>
        /// Track gadget destruction
        /// </summary>
        public void RecordGadgetDestruction()
        {
            OnGadgetDestroyed?.Invoke();
        }
    }
}
