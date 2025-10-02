using UnityEngine;
using Game.Core;

namespace Game.Equipment
{
    [CreateAssetMenu(menuName = "Game/Equipment", fileName = "Equip_")]
    public class EquipmentDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public EquipmentSlot slot;
        public Sprite icon;

        [Header("Stats")]
        public Stats modifiers;

        [Header("Durability")]
        public int maxDurability = 20;
        public bool breaksWhenZero = true;
    }
}
