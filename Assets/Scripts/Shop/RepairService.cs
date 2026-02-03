using System.Collections.Generic;
using UnityEngine;
using Game.Equipment;

namespace Game.Shop
{
    /// <summary>
    /// Handles equipment repair logic.
    /// </summary>
    public static class RepairService
    {
        private const int BaseCostPerDurabilityPoint = 1;

        /// <summary>
        /// Calculate the cost to repair a single piece of equipment.
        /// </summary>
        public static int GetRepairCost(EquipmentInstance item)
        {
            if (item == null || item.def == null) return 0;
            if (item.def.maxDurability <= 0) return 0; // Unbreakable

            int missing = item.def.maxDurability - item.currentDurability;
            if (missing <= 0) return 0;

            return missing * BaseCostPerDurabilityPoint;
        }

        /// <summary>
        /// Calculate total cost to repair all equipped items.
        /// </summary>
        public static int GetTotalRepairCost(EquipmentManager manager)
        {
            if (manager == null) return 0;

            int total = 0;

            // Check all equipment slots
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;

                var equipped = manager.GetEquipped(slot);
                if (equipped != null)
                {
                    total += GetRepairCost(equipped);
                }
            }

            return total;
        }

        /// <summary>
        /// Repair a single piece of equipment.
        /// </summary>
        public static void RepairItem(EquipmentInstance item)
        {
            if (item == null || item.def == null) return;
            if (item.def.maxDurability <= 0) return;

            int missing = item.def.maxDurability - item.currentDurability;
            item.Repair(missing);
            Debug.Log($"[RepairService] Repaired {item.def.displayName} to full durability");
        }

        /// <summary>
        /// Repair all equipped items.
        /// </summary>
        public static void RepairAllEquipped(EquipmentManager manager)
        {
            if (manager == null) return;

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;

                var equipped = manager.GetEquipped(slot);
                if (equipped != null)
                {
                    RepairItem(equipped);
                }
            }

            Debug.Log("[RepairService] Repaired all equipped items");
        }

        /// <summary>
        /// Get list of damaged items that need repair.
        /// </summary>
        public static List<EquipmentInstance> GetDamagedItems(EquipmentManager manager)
        {
            var damaged = new List<EquipmentInstance>();
            if (manager == null) return damaged;

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;

                var equipped = manager.GetEquipped(slot);
                if (equipped != null && equipped.def != null && equipped.def.maxDurability > 0)
                {
                    if (equipped.currentDurability < equipped.def.maxDurability)
                    {
                        damaged.Add(equipped);
                    }
                }
            }

            return damaged;
        }
    }
}
