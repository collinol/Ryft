using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Equipment
{
    public class EquipmentManager
    {
        private readonly Dictionary<EquipmentSlot, EquipmentInstance> equipped = new();

        public Stats CombinedModifiers
        {
            get
            {
                var sum = Stats.Zero;
                foreach (var kv in equipped)
                    sum += kv.Value.ActiveModifiers;
                return sum;
            }
        }

        public bool Equip(EquipmentInstance item)
        {
            if (item == null || item.Def == null) return false;
            equipped[item.Def.slot] = item;
            return true;
        }

        public EquipmentInstance Get(EquipmentSlot slot) =>
            equipped.TryGetValue(slot, out var inst) ? inst : null;

        public void DamageAll(int amount)
        {
            foreach (var kv in equipped) kv.Value.Damage(amount);
        }
    }
}
