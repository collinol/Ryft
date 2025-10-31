using UnityEngine;
using Game.Core;

namespace Game.Equipment
{
    [CreateAssetMenu(menuName = "Game/Equipment", fileName = "Equip_")]
    public class EquipmentDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;                 // unique key (string)
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Slot & Durability")]
        public EquipmentSlot slot = EquipmentSlot.None;
        [Min(0)] public int maxDurability = 0; // 0 = unbreakable

        [Header("Stat Bonuses")]
        public Stats bonusStats; // adds to TotalStats while equipped

        [Header("Optional Effect")]
        // Fully-qualified class name that implements IEquipmentEffect
        public string runtimeEffectTypeName;

        #if UNITY_EDITOR
        void OnValidate()
        {
            // Auto-fill id from asset name if empty
            if (string.IsNullOrWhiteSpace(id))
                id = name;

            // Gentle sanity warnings in editor
            if (slot == EquipmentSlot.None)
                Debug.LogWarning($"{name}: Equipment slot is None — it cannot be equipped into Character grid.");
            if (!icon)
                Debug.LogWarning($"{name}: No icon assigned — it will appear blank in the UI.");
        }
        #endif

    }
}
