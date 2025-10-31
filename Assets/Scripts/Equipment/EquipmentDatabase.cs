using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Equipment
{
    [CreateAssetMenu(menuName = "Game/Equipment Database", fileName = "EquipmentDatabase")]
    public class EquipmentDatabase : ScriptableObject
    {
        [SerializeField] private List<EquipmentDef> items = new();
        public IReadOnlyList<EquipmentDef> Items => items;
        public int Count => items?.Count ?? 0;
        private Dictionary<string, EquipmentDef> byId;

        private const string ResNoSpace = "Equipment/EquipmentDatabase";
        private const string ResWithSpace = "Equipment/Equipment Database";
        public EquipmentDef Get(int index)
        {
            if (items == null || index < 0 || index >= items.Count) return null;
            return items[index];
        }
        public static EquipmentDatabase Load()
        {
            var db = Resources.Load<EquipmentDatabase>(ResNoSpace)
                  ?? Resources.Load<EquipmentDatabase>(ResWithSpace);
            if (!db)
            {
                Debug.LogError($"EquipmentDatabase not found at Resources/{ResNoSpace} or Resources/{ResWithSpace}");
                return null;
            }
            db.Build();
            return db;
        }

        void OnEnable()   => Build();
        void OnValidate() => Build();

        private void Build()
        {
            byId = new(StringComparer.OrdinalIgnoreCase);
            foreach (var it in items)
            {
                if (!it || string.IsNullOrWhiteSpace(it.id)) continue;
                if (!byId.ContainsKey(it.id)) byId.Add(it.id, it);
            }
        }

        public IReadOnlyList<EquipmentDef> All => items;
        public EquipmentDef Get(string id) => (string.IsNullOrWhiteSpace(id) || byId == null) ? null
                                                 : (byId.TryGetValue(id.Trim(), out var v) ? v : null);

        public void SetItems(List<EquipmentDef> list) { items = list ?? new(); Build(); }
    }
}
