// Assets/Scripts/Player/PlayerCharacter.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;        // IActor, Stats
using Game.Abilities;   // AbilityDatabase, AbilityDef
using Game.Talent;      // TalentTreeDef (kept for compatibility)
using Game.UI;

namespace Game.Player
{
    public class PlayerCharacter : MonoBehaviour, IActor
    {
        [Header("Identity & Stats")]
        [SerializeField] private string displayName = "Player";
        [SerializeField] private Stats baseStats = new Stats { maxHealth = 20, strength = 3, defense = 0 };
        public string DisplayName => displayName;
        public Stats BaseStats => baseStats;
        public Stats TotalStats => baseStats;
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;
        private HealthBarView hpBar;

        [Header("Ability Slots")]
        [SerializeField] private int maxSlots = 5;

        [SerializeField] private string[] defaultStarterIds = { "Shoot", "MedKit" };
        [SerializeField] private List<AbilityDef> learned = new();
        [SerializeField] private AbilityDef[] loadout;

        public event Action AbilitiesChanged;

        public IReadOnlyList<AbilityDef> AbilityLoadout
        {
            get { EnsureArrays(); return loadout; }
        }

        void Awake()
        {
            EnsureArrays();
            Health = Mathf.Max(1, TotalStats.maxHealth);

            hpBar = HealthBarView.Attach(transform, new Vector3(0f, 1.5f, 0f));
            hpBar.Set(Health, TotalStats.maxHealth);
        }

        void Start()
        {
            if (learned == null || learned.Count == 0)
                GrantDefaults();
        }

        // ---- IActor --------------------------------------------------------
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

        // ---- Abilities -----------------------------------------------------
        public void InitAbilitiesFromTalents(AbilityDatabase db, TalentTreeDef tree, IEnumerable<string> starterAbilityIds)
        {
            if (!db) { Debug.LogError("PlayerCharacter.InitAbilitiesFromTalents: AbilityDatabase is NULL."); return; }
            EnsureArrays();

            foreach (var id in (starterAbilityIds ?? Array.Empty<string>()))
                Learn(db.Get(id));

            AutoFillLoadoutFromLearned();
            AbilitiesChanged?.Invoke();

            Debug.Log($"Player init complete. Learned: {learned.Count}, Loadout filled: {loadout.Count(a => a != null)}");
        }

        public bool TryUseAbilityById(string abilityId)
        {
            EnsureArrays();
            if (string.IsNullOrWhiteSpace(abilityId)) return false;
            var def = learned.FirstOrDefault(a => a && a.id == abilityId);
            if (!def) return false;
            Debug.Log($"Player wants to use {def.displayName}");
            return true; // controller actually executes
        }

        public void EndTurn() => Debug.Log("PlayerCharacter.EndTurn()");

        // ---- Internals -----------------------------------------------------
        private void GrantDefaults()
        {
            var db = AbilityDatabase.Load();
            if (!db) return;

            foreach (var id in defaultStarterIds ?? Array.Empty<string>())
                Learn(db.Get(id));

            AutoFillLoadoutFromLearned();
            AbilitiesChanged?.Invoke();

            Debug.Log($"Player defaults granted. Learned: {learned.Count}, Loadout filled: {loadout.Count(a => a != null)}");
        }

        private void Learn(AbilityDef def)
        {
            if (!def) return;
            learned ??= new List<AbilityDef>();
            if (!learned.Contains(def)) learned.Add(def);
        }

        private void AutoFillLoadoutFromLearned()
        {
            EnsureArrays();
            Array.Clear(loadout, 0, loadout.Length);
            int s = 0;
            foreach (var a in learned)
            {
                if (!a) continue;
                if (s >= maxSlots) break;
                loadout[s++] = a;
            }
        }

        private void EnsureArrays()
        {
            if (maxSlots <= 0) maxSlots = 5;
            learned ??= new List<AbilityDef>();
            if (loadout == null || loadout.Length != maxSlots)
                loadout = new AbilityDef[maxSlots];
        }
    }
}
