using UnityEngine;
using Game.Core;

namespace Game.Equipment
{
    public class EquipmentInstance
    {
        public EquipmentDef Def { get; private set; }
        public int Durability { get; private set; }

        public EquipmentInstance(EquipmentDef def)
        {
            Def = def;
            Durability = def.maxDurability;
        }

        public bool IsBroken => Def.breaksWhenZero && Durability <= 0;

        public Stats ActiveModifiers => IsBroken ? Stats.Zero : Def.modifiers;

        public void Damage(int amount)
        {
            Durability = Mathf.Max(0, Durability - amount);
        }
    }
}
