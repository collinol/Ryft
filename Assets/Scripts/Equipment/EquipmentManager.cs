using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Equipment;
using Game.Combat;

namespace Game.Equipment
{
    /// Attach to PlayerCharacter. Manages inventory, equipped items, stat bonuses, and effects.
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }
        [Header("Seeding (optional)")]
        [SerializeField] private EquipmentDatabase defaultDatabase;
        [SerializeField] private bool seedInventoryFromDatabaseOnAwake = false;
        [SerializeField] private bool clearInventoryBeforeSeeding = true;

        [Header("Inventory")]
        [SerializeField] private List<EquipmentInstance> inventory = new();

        [Header("Equipped (by slot)")]
        [SerializeField] private List<EquippedEntry> equipped = new(); // inspector view
        private Dictionary<EquipmentSlot, EquipmentInstance> map = new();

        [Serializable]
        public class EquippedEntry
        {
            public EquipmentSlot slot;
            public EquipmentInstance item;
        }

        private IActor owner;

        private List<IEquipmentEffect> effects = new();
        void Awake()
        {
            owner = GetComponent<IActor>();
            if (equipped == null) equipped = new();
            RebuildEquippedMap();
            RebindEffects();
             if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (seedInventoryFromDatabaseOnAwake && defaultDatabase)
            SeedFromDatabase(defaultDatabase, clearInventoryBeforeSeeding);
            DontDestroyOnLoad(gameObject);
        }
        public void SeedFromDatabase(EquipmentDatabase db, bool clear)
        {
            if (!db) return;
            if (clear) inventory = new List<EquipmentInstance>();
            foreach (var def in db.Items)
            {
                if (!def) continue;
                inventory.Add(new EquipmentInstance(def));
            }
        }
        public IReadOnlyList<EquipmentDef> GetInventoryDefs()
        {
            var list = new List<EquipmentDef>();
            foreach (var it in inventory)
                if (it != null && it.def) list.Add(it.def);
            return list;
        }

        public IReadOnlyList<EquipmentInstance> Inventory => inventory;
        public EquipmentInstance GetEquipped(EquipmentSlot slot)
        {
            return (map != null && map.TryGetValue(slot, out var inst)) ? inst : null;
        }

        public void AddToInventoryAt(int index, EquipmentInstance inst)
        {
            if (inst == null) return;
            inventory ??= new();
            index = Mathf.Clamp(index, 0, inventory.Count);
            inventory.Insert(index, inst);
        }
        public void AddToInventory(EquipmentInstance inst)
        {
            if (inst == null) return;
            inventory ??= new();
            inventory.Add(inst);
        }
        public int IndexOf(EquipmentInstance inst) =>  inventory != null ? inventory.IndexOf(inst) : -1;

        public bool RemoveFromInventory(EquipmentInstance inst)
        {
            if (inventory == null || inst == null) return false;
            return inventory.Remove(inst);
        }

        public void Equip(EquipmentInstance inst)
        {
            if (inst == null || inst.def == null) return;

            var slot = inst.def.slot;
            if (slot == EquipmentSlot.None)
            {
                Debug.LogWarning($"[MGR] Tried to equip item with no slot: {inst.def.id}");
                return;
            }

            // Remove from inventory
            inventory?.Remove(inst);

            // If slot already has something, unequip it (put back in inventory)
            if (map.TryGetValue(slot, out var old) && old != null)
            {
                Debug.Log($"[MGR] Slot {slot} occupied by {old.def.id}, moving to inventory");
                AddToInventory(old);
            }

            SetEquipped(slot, inst);
            Debug.Log($"[MGR] EQUIPPED {inst.def.id} -> slot {slot}");
        }

        public bool Unequip(EquipmentSlot slot)
        {
            var current = GetEquipped(slot);
            if (current == null) return false;
            SetEquipped(slot, null);
            inventory.Add(current);
            RebindEffects();
            return true;
        }

        public Stats GetEquipmentStatBonus()
        {
            if (map == null || map.Count == 0) return Stats.Zero;
            var sum = Stats.Zero;
            foreach (var kv in map)
            {
                var d = kv.Value?.def;
                if (!d) continue;
                sum = sum + d.bonusStats;
            }
            return sum;
        }

        // --- Combat notifications (call these from your fight controller) ---
        public void NotifyBattleStarted(FightContext ctx) { foreach (var e in effects) e.OnBattleStarted(ctx); }
        public void NotifyBattleEnded(FightContext ctx)   { foreach (var e in effects) e.OnBattleEnded(ctx); }
        public void NotifyTurnStarted(FightContext ctx, IActor whose) { foreach (var e in effects) e.OnTurnStarted(ctx, whose); }
        public void NotifyTurnEnded(FightContext ctx, IActor whose)   { foreach (var e in effects) e.OnTurnEnded(ctx, whose); }
        public void NotifyOwnerDamaged(FightContext ctx, IActor attacker, int damage) { foreach (var e in effects) e.OnOwnerDamaged(ctx, attacker, damage); }
        public void NotifyOwnerDealtDamage(FightContext ctx, IActor target, int damage) { foreach (var e in effects) e.OnOwnerDealtDamage(ctx, target, damage); }

        // --- internals ---
        private void RebuildEquippedMap()
        {
            map = new();
            foreach (var e in equipped ?? Enumerable.Empty<EquippedEntry>())
            {
                if (e == null || e.slot == EquipmentSlot.None) continue;
                if (e.item != null) map[e.slot] = e.item;
            }
        }

        private void SetEquipped(EquipmentSlot slot, EquipmentInstance item)
        {
            if (equipped == null) equipped = new();
            var found = equipped.FirstOrDefault(x => x.slot == slot);
            if (found == null)
            {
                found = new EquippedEntry { slot = slot, item = item };
                equipped.Add(found);
            }
            else found.item = item;
            RebuildEquippedMap();
        }

        private void RebindEffects()
        {
            effects.Clear();
            foreach (var kv in map)
            {
                var def = kv.Value?.def;
                if (!def || string.IsNullOrWhiteSpace(def.runtimeEffectTypeName)) continue;
                var t = Type.GetType(def.runtimeEffectTypeName);
                if (t == null) { Debug.LogError($"Equipment effect type not found: {def.runtimeEffectTypeName}"); continue; }
                if (Activator.CreateInstance(t) is IEquipmentEffect effect)
                {
                    effect.Bind(owner);
                    effects.Add(effect);
                }
                else Debug.LogError($"Type '{def.runtimeEffectTypeName}' does not implement IEquipmentEffect.");
            }
        }
        /**  DEBUGGING **/
        public int IndexOfInInventory(EquipmentInstance inst)
        {
            return (inventory != null) ? inventory.IndexOf(inst) : -1;
        }

        public void InsertIntoInventoryAt(EquipmentInstance inst, int index)
        {
            if (inst == null) return;
            inventory ??= new();
            index = Mathf.Clamp(index, 0, inventory.Count);
            Debug.Log($"[EM] Insert {inst.def?.id} @ invIndex={index}");
            inventory.Insert(index, inst);
        }

        public void MoveInventoryItem(EquipmentInstance inst, int newIndex)
        {
            if (inst == null || inventory == null) return;
            int old = inventory.IndexOf(inst);
            if (old < 0) { Debug.Log($"[EM] Move: item not in inventory, inserting @ {newIndex}"); InsertIntoInventoryAt(inst, newIndex); return; }

            // account for removal when moving forward
            if (newIndex > old) newIndex--;
            newIndex = Mathf.Clamp(newIndex, 0, inventory.Count - 1);

            Debug.Log($"[EM] Move {inst.def?.id} inv {old} -> {newIndex}");
            inventory.RemoveAt(old);
            inventory.Insert(newIndex, inst);
        }
    }
}
