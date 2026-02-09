using System.Collections.Generic;
using UnityEngine;
using Game.Equipment;

namespace Game.Player
{
    /// <summary>
    /// Defines and grants starting equipment for a new run.
    /// Attach to a GameObject in your initial scene or call Initialize() when starting a new game.
    /// </summary>
    public class PlayerStartingEquipment : MonoBehaviour
    {
        [Header("Starting Equipment (by ID)")]
        [Tooltip("List of equipment IDs that the player starts with in their inventory")]
        [SerializeField] private List<string> startingEquipmentIds = new();

        [Header("Auto-Equip")]
        [Tooltip("Automatically equip the first valid item for each slot")]
        [SerializeField] private bool autoEquipOnStart = false;

        [Header("Settings")]
        [Tooltip("Only grant starting equipment if inventory is empty")]
        [SerializeField] private bool onlyIfInventoryEmpty = true;

        private static bool hasGrantedStartingEquipment = false;

        void Start()
        {
            // Only grant once per game session
            if (hasGrantedStartingEquipment && onlyIfInventoryEmpty)
            {
                Debug.Log("[StartingEquipment] Already granted this session, skipping");
                return;
            }

            GrantStartingEquipment();
        }

        /// <summary>
        /// Call this to grant starting equipment (e.g., when starting a new run).
        /// </summary>
        public void GrantStartingEquipment()
        {
            var mgr = EquipmentManager.Instance;
            if (mgr == null)
            {
                Debug.LogError("[StartingEquipment] EquipmentManager not found!");
                return;
            }

            // Check if inventory already has items
            if (onlyIfInventoryEmpty && mgr.Inventory.Count > 0)
            {
                Debug.Log("[StartingEquipment] Inventory not empty, skipping");
                return;
            }

            var db = EquipmentDatabase.Load();
            if (db == null)
            {
                Debug.LogError("[StartingEquipment] EquipmentDatabase not found!");
                return;
            }

            int added = 0;
            foreach (var id in startingEquipmentIds)
            {
                if (string.IsNullOrEmpty(id)) continue;

                var def = db.Get(id);
                if (def == null)
                {
                    Debug.LogWarning($"[StartingEquipment] Equipment not found: {id}");
                    continue;
                }

                var instance = new EquipmentInstance(def);
                mgr.AddToInventory(instance);
                added++;
                Debug.Log($"[StartingEquipment] Added: {def.displayName}");
            }

            Debug.Log($"[StartingEquipment] Granted {added} starting items");

            // Auto-equip if enabled
            if (autoEquipOnStart)
            {
                AutoEquipBestItems(mgr);
            }

            hasGrantedStartingEquipment = true;
        }

        /// <summary>
        /// Automatically equip the first valid item for each empty slot.
        /// </summary>
        private void AutoEquipBestItems(EquipmentManager mgr)
        {
            var inventory = new List<EquipmentInstance>(mgr.Inventory);

            foreach (var item in inventory)
            {
                if (item?.def == null) continue;
                if (item.def.slot == EquipmentSlot.None) continue;

                // Check if slot is empty
                var currentlyEquipped = mgr.GetEquipped(item.def.slot);
                if (currentlyEquipped == null)
                {
                    mgr.Equip(item);
                    Debug.Log($"[StartingEquipment] Auto-equipped: {item.def.displayName} -> {item.def.slot}");
                }
            }
        }

        /// <summary>
        /// Reset the static flag (call when starting a completely new run).
        /// </summary>
        public static void ResetForNewRun()
        {
            hasGrantedStartingEquipment = false;
        }

        /// <summary>
        /// Clear all equipment and grant starting equipment fresh.
        /// </summary>
        public void ResetAndGrantFresh()
        {
            var mgr = EquipmentManager.Instance;
            if (mgr == null) return;

            // Unequip everything
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;
                mgr.Unequip(slot);
            }

            // Clear inventory (need to use reflection or add a method to EquipmentManager)
            // For now, just remove items one by one
            var toRemove = new List<EquipmentInstance>(mgr.Inventory);
            foreach (var item in toRemove)
            {
                mgr.RemoveFromInventory(item);
            }

            hasGrantedStartingEquipment = false;
            GrantStartingEquipment();
        }
    }
}
