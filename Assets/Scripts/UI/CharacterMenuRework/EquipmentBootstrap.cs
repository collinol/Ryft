// Assets/Scripts/UI/CharacterMenuRework/EquipmentBootstrap.cs
using UnityEngine;
using Game.Equipment;

public class EquipmentBootstrap : MonoBehaviour
{
    [Header("Load/Equip Options")]
    public bool clearBeforeSeeding = true;
    public bool autoEquipFirstPerSlot = true; // one per slot if available

    void Start()
    {
        var mgr = EquipmentManager.Instance ?? FindObjectOfType<EquipmentManager>();
        if (!mgr) { Debug.LogError("EquipmentBootstrap: EquipmentManager not found."); return; }

        var db = EquipmentDatabase.Load(); // uses Resources path
        if (!db) { Debug.LogError("EquipmentBootstrap: EquipmentDatabase not found in Resources."); return; }
        foreach (var def in db.All)
        {
            Debug.Log($"DB check: def='{def.name}', id={def.id}, icon={(def.icon ? def.icon.name : "NULL")}");
        }

        Debug.Log($"Bootstrap: DB='{db.name}', raw count={db.All.Count}");
        // 1) Seed inventory with one instance per def
        mgr.SeedFromDatabase(db, clearBeforeSeeding);
        Debug.Log($"After SeedFromDatabase: mgr.Inventory.Count={mgr.Inventory.Count}");
        // 2) Optionally auto-equip the first item per slot
        if (autoEquipFirstPerSlot)
        {
            foreach (var def in db.All)
            {
                if (!def || def.slot == EquipmentSlot.None) continue;
                if (mgr.GetEquipped(def.slot) != null) continue;

                EquipmentInstance inst = null;
                foreach (var it in mgr.Inventory) { if (it != null && it.def == def) { inst = it; break; } }
                if (inst != null) mgr.Equip(inst);
            }
        }

        // 3) Refresh the UI
        Debug.Log($"Seeded items: {mgr.Inventory.Count}");
        for (int i = 0; i < mgr.Inventory.Count; i++)
        {
            var ei = mgr.Inventory[i];
            var iconName = (ei?.def?.icon ? ei.def.icon.name : "NULL");
            Debug.Log($"POST-SEED inv[{i}] def={ei?.def?.name ?? "NULL"} icon={iconName} defID={ei?.def?.GetInstanceID()}");
        }
        var ui = FindObjectOfType<Game.UI.Inventory.EquipmentUIController>();
        ui?.RefreshFromManager(); // add this wrapper in the controller (below)
    }
}
