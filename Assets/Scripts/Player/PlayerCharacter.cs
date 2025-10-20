// Assets/Scripts/Player/PlayerCharacter.cs
using System;
using UnityEngine;
using Game.Core;        // IActor, Stats
using Game.UI;
using Game.Ryfts;

namespace Game.Player
{
    public class PlayerCharacter : MonoBehaviour, IActor
    {
        [Header("Identity & Stats")]
        [SerializeField] private string displayName = "Player";
        private Stats baseStats;
        public string DisplayName => displayName;
        public Stats BaseStats => baseStats;
        public event Action<Stats> OnTurnStatsChanged;

        /// <summary>
        /// Base + Ryft (permanent + temporary) — used for health clamping and for
        /// refreshing the per-turn spendable stat pool.
        /// </summary>
        public Stats TotalStats
        {
            get
            {
                var s = new Stats
                {
                    maxHealth = BaseStats.maxHealth,
                    strength  = BaseStats.strength,
                    mana        = BaseStats.mana,
                    defense   = BaseStats.defense,
                    engineering = BaseStats.engineering
                };

                var mgr = RyftEffectManager.Instance;
                if (mgr)
                {
                    // permanent
                    s.maxHealth += mgr.BonusMaxHp;
                    s.strength  += mgr.BonusStrength;
                    s.mana  += mgr.BonusMana;
                    s.defense   += mgr.BonusDefense;
                    s.engineering += mgr.BonusEngineering;

                    // temporary (battle-scoped)
                    s.maxHealth += mgr.TempMaxHp;
                    s.strength  += mgr.TempStrength;
                    s.mana  += mgr.TempMana;
                    s.defense   += mgr.TempDefense;
                    s.engineering   += mgr.TempEngineering;
                }

                return s;
            }
        }

        public int  Health  { get; private set; }
        public bool IsAlive => Health > 0;

        private HealthBarView hpBar;

        [Header("Per-turn Spendable Pool (refreshed at the start of player turn)")]
        private Stats currentTurnStats;
        public Stats CurrentTurnStats => currentTurnStats;

        void Awake()
        {
            baseStats = new Stats { maxHealth = 30, strength = 5, mana = 5, engineering = 5, defense = 5 };
            currentTurnStats = new Stats { maxHealth = 30,  strength = 5, mana = 5, engineering = 5, defense = 5 };


            Health = Mathf.Max(1, TotalStats.maxHealth);
            hpBar = HealthBarView.Attach(transform, new Vector3(0f, 1.5f, 0f));
            hpBar.Set(Health, TotalStats.maxHealth);
            RefreshTurnStats();
        }
        void OnEnable()  { RyftCombatEvents.OnResourceRefund += HandleResourceRefund; }
        void OnDisable() { RyftCombatEvents.OnResourceRefund -= HandleResourceRefund; }

        // ---- IActor --------------------------------------------------------

        public void Gain(Stats gain)
        {
            var cap = TotalStats; // don’t exceed the per-turn max derived from total stats
            currentTurnStats = new Stats
            {
                maxHealth = currentTurnStats.maxHealth, // not spendable
                strength  = Mathf.Min(cap.strength, currentTurnStats.strength + Math.Max(0, gain.strength)),
                mana        = Mathf.Min(cap.mana,        currentTurnStats.mana        + Math.Max(0, gain.mana)),
                engineering = Mathf.Min(cap.engineering, currentTurnStats.engineering + Math.Max(0, gain.engineering)),
                defense     = currentTurnStats.defense
            };
            OnTurnStatsChanged?.Invoke(currentTurnStats);
        }
        private void HandleResourceRefund(IActor who, StatField field, int amount)
        {
            if (!ReferenceEquals(who, this) || amount <= 0) return;

            var cap = TotalStats; // clamp to this turn’s max pool derived from total stats
            StatsUtil.AddClamped(ref currentTurnStats, field, amount, cap);
            OnTurnStatsChanged?.Invoke(currentTurnStats);
        }

        public void ApplyDamage(int amount)
        {
            var mitigated = Mathf.Max(0, amount - TotalStats.defense);
            Health = Mathf.Max(0, Health - mitigated);
            hpBar?.Set(Health, TotalStats.maxHealth);
            RyftCombatEvents.RaiseDamageTaken(this, mitigated);
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(TotalStats.maxHealth, Health + Mathf.Max(0, amount));
            hpBar?.Set(Health, TotalStats.maxHealth);
        }

        // ---- Card-cost interface (used by CardRuntime) ---------------------
        /// <summary>
        /// Called at the beginning of the player's turn to restore the spendable pool.
        /// Uses current TotalStats so Ryft effects (including temporary battle buffs) are honored each turn.
        /// </summary>
        public void RefreshTurnStats()
        {
            var t = TotalStats;
            currentTurnStats = new Stats
            {
                maxHealth = t.maxHealth, // not spendable, but kept for completeness
                strength  = t.strength,
                mana  = t.mana,
                defense   = t.defense,
                engineering   = t.engineering
            };
            OnTurnStatsChanged?.Invoke(currentTurnStats);
        }

        /// <summary>Checks if the player can afford a card's cost from this turn's pool.</summary>
        public bool CanPay(Stats cost)
        {
            // ignore maxHealth in costs (not a spendable resource)
            return currentTurnStats.strength    >= Mathf.Max(0, cost.strength)
            && currentTurnStats.mana        >= Mathf.Max(0, cost.mana)
            && currentTurnStats.engineering >= Mathf.Max(0, cost.engineering);
        }

        public void Pay(Stats cost)
        {
            Debug.Log($"[PC.Pay] BEFORE  STR={currentTurnStats.strength}, DEF={currentTurnStats.defense}, ENG={currentTurnStats.engineering} | COST S={cost.strength} D={cost.defense} E={cost.engineering}");

            currentTurnStats = new Stats
            {
                maxHealth   = currentTurnStats.maxHealth,
                strength    = Mathf.Max(0, currentTurnStats.strength    - Mathf.Max(0, cost.strength)),
                mana        = Mathf.Max(0, currentTurnStats.mana        - Mathf.Max(0, cost.mana)),        // NEW
                engineering = Mathf.Max(0, currentTurnStats.engineering - Mathf.Max(0, cost.engineering)),
                defense     = currentTurnStats.defense
            };

            Debug.Log($"[PC.Pay]  AFTER  STR={currentTurnStats.strength}, DEF={currentTurnStats.defense}, ENG={currentTurnStats.engineering}");
            OnTurnStatsChanged?.Invoke(currentTurnStats);
        }

        // Kept for compatibility with existing calls; does nothing special in the card system.
        public void EndTurn() => Debug.Log("PlayerCharacter.EndTurn()");
    }
}
