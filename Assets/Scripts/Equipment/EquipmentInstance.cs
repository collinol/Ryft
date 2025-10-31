using System;
using UnityEngine;

namespace Game.Equipment
{
    [Serializable]
    public class EquipmentInstance
    {
        public EquipmentDef def;
        public int currentDurability;

        public EquipmentInstance(EquipmentDef d)
        {
            def = d;
            currentDurability = d ? Mathf.Max(0, d.maxDurability) : 0;
        }

        public bool IsBroken => def && def.maxDurability > 0 && currentDurability <= 0;

        public void Damage(int amount)
        {
            if (!def || def.maxDurability <= 0) return;
            currentDurability = Mathf.Max(0, currentDurability - Mathf.Max(0, amount));
        }

        public void Repair(int amount)
        {
            if (!def || def.maxDurability <= 0) return;
            currentDurability = Mathf.Min(def.maxDurability, currentDurability + Mathf.Max(0, amount));
        }
    }
}
