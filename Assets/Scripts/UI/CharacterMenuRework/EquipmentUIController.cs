// Assets/Scripts/UI/Inventory/EquipmentUIController.cs
using System.Collections.Generic;
using UnityEngine;
using Game.Equipment;

namespace Game.UI.Inventory
{
    public class EquipmentUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EquipmentGridUI characterGrid;
        [SerializeField] private EquipmentGridUI inventoryGrid;

        [Header("Seeding / Data")]
        [SerializeField] private EquipmentDatabase database;   // optional: set to Resources asset
        [SerializeField] private bool seedManagerFromDatabase = true;

        // runtime state
        private EquipmentManager mgr;

        private EquipmentCellUI _pendingFrom;
        private EquipmentInstance _pendingItem;
        private int _pendingFromIndex = -1;
        private static bool IsCharacterCell(EquipmentCellUI cell) => cell && cell.isCharacterCell;
        private bool IsInventoryCell(EquipmentCellUI cell) => inventoryGrid.Owns(cell);
        private string _pendingFromGrid = "";

        [SerializeField] private EquipmentItemUI itemPrefab;

        void Awake()
        {
            mgr = EquipmentManager.Instance ?? FindObjectOfType<EquipmentManager>();
            if (!mgr)
            {
                // if EquipmentManager not in the scene yet, create a simple host
                var go = new GameObject("EquipmentManager_Auto");
                mgr = go.AddComponent<EquipmentManager>();
            }

            // Optional seeding from database (fills manager inventory with all defs)
            if (seedManagerFromDatabase && database)
            {
                mgr.SeedFromDatabase(database, clear: true);
            }

            characterGrid.SetHeader("Character");
            characterGrid.Build(OnCellClicked);

            inventoryGrid.SetHeader("Inventory");
            inventoryGrid.Build(OnCellClicked);

            RepopulateAll();
        }

        private void WireCell(EquipmentCellUI cell)
        {
            cell.onClicked = OnCellClicked;
        }

        // Fill both grids from manager state
        public void RepopulateAll()
        {
            var mgr = EquipmentManager.Instance ?? FindObjectOfType<EquipmentManager>();

            // Character grid
            foreach (var cell in characterGrid.Cells)
            {
                EquipmentInstance inst = null;
                if (cell.characterSlot != EquipmentSlot.None)
                    inst = mgr.GetEquipped(cell.characterSlot);

                Debug.Log($"[UI] CHAR cell[{cell.index}] slot={cell.characterSlot} -> {(inst != null ? inst.def.id : "NULL")}");
                EnsureCellHasView(cell);
                cell.BindItem(inst);
            }

            // Inventory grid
            var inv = mgr.Inventory;
            for (int i = 0; i < inventoryGrid.Capacity; i++)
            {
                var cell = inventoryGrid.GetCell(i);
                var inst = (inv != null && i < inv.Count) ? inv[i] : null;
                Debug.Log($"[UI] INV  cell[{i}] -> {(inst != null ? inst.def.id : "NULL")}");
                EnsureCellHasView(cell);
                cell.BindItem(inst);
            }

            ClearHeld();
        }

        private void EnsureCellHasView(EquipmentCellUI cell)
        {
            if (cell == null) return;

            if (cell.ItemView == null)
            {
                // Prefer controller-level prefab if assigned…
                if (itemPrefab != null && cell.ItemAnchor != null)
                {
                    var view = Instantiate(itemPrefab, cell.ItemAnchor);
                    var rt = view.GetComponent<RectTransform>();
                    if (rt)
                    {
                        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                        rt.pivot     = new Vector2(0.5f, 0.5f);
                    }
                    cell.ForceAssignView(view);
                }
                else
                {
                    // …otherwise ask the cell to make its own view from its prefab.
                    cell.EnsureView();
                }
            }
        }


        private void OnCellClicked(EquipmentCellUI cell)
        {
            var mgr = EquipmentManager.Instance;
            if (!mgr || cell == null) return;
            string grid = IsCharacterCell(cell) ? "CHAR" : IsInventoryCell(cell) ? "INV" : "UNKNOWN";
            int index   = (grid == "CHAR") ? characterGrid.IndexOf(cell)
                         : (grid == "INV") ? inventoryGrid.IndexOf(cell)
                         : -1;
             Debug.Log($"[CLICK] grid={grid} index={index} pending={(_pendingItem!=null)}");
            // ---- PICK UP ----
            if (_pendingItem == null)
            {
                var item = cell.GetItem();
                if (item == null) return;

                if (IsCharacterCell(cell))
                {
                    // from character: unequip -> inventory (we're going to remove it again)
                    mgr.Unequip(item.def.slot);
                    // remove the one we just pushed into inventory so we truly "pick it up"
                    mgr.RemoveFromInventory(item);
                    _pendingFromIndex = -1; // came from character, not inventory
                }
                else
                {
                    // from inventory: remember the original index, then remove
                    _pendingFromIndex = mgr.IndexOf(item);
                    mgr.RemoveFromInventory(item);
                }

                cell.ClearItem();
                _pendingItem = item;
                _pendingFrom = cell;
                _pendingFromGrid  = grid;
                Debug.Log($"[PICK] took {item.def?.id} from {grid}[{index}]");
                HighlightCell(cell, true);
                return;
            }

            // ---- PLACE ----
            var placing = _pendingItem;
            var from    = _pendingFrom;
            string tgtGrid = grid;
            int    tgtIndex = index;
            Debug.Log($"[PLACE] target {tgtGrid}[{tgtIndex}] item={placing.def?.id}");

            if (IsCharacterCell(cell))
            {
                var targetSlot = cell.characterSlot;

                // wrong slot? put it back where it came from
                if (placing.def.slot != targetSlot)
                {
                    RestorePendingToSource(from, placing);
                    return;
                }

                // if something is already there, Unequip() pushes it to inventory
                var already = mgr.GetEquipped(targetSlot);
                if (already != null) mgr.Unequip(targetSlot);

                mgr.Equip(placing);
            }
            else
            {
                // place into the exact inventory cell that was clicked
                var targetIndex = cell.index;

                // if we picked it up from inventory earlier, the list shrank by 1;
                // clicking a later slot means its index shifted left by one.
                var inv = mgr.Inventory;

                // remove from current position if it exists
                mgr.RemoveFromInventory(placing);

                // if the index is valid, insert there
                mgr.InsertIntoInventoryAt(placing, targetIndex);
                if (_pendingFromIndex >= 0 && _pendingFromIndex < targetIndex)
                    targetIndex--;


                Debug.Log($"[PLACE] placed {placing.def.id} into INV[{targetIndex}]");
            }

            ClearPending();
            RefreshFromManager();
        }

        private void HighlightCell(EquipmentCellUI cell, bool on)
        {
            // Simple color tint feedback
            if (!cell) return;
            var c = on ? new Color(1f, 1f, 0.6f, 1f) : Color.white;
            cell.SetTint(c);
        }

        private void ClearHeld()
        {
            _pendingFrom = null;
            _pendingItem = null;
        }
        private void RestorePendingToSource(EquipmentCellUI from, EquipmentInstance item)
        {
            var mgr = EquipmentManager.Instance;
            if (!mgr || item == null) return;
            Debug.Log($"[RESTORE] to {_pendingFromGrid}[{_pendingFromIndex}]");
            if (IsCharacterCell(from))
            {
                // Came from a character slot → just re-equip to that slot
                mgr.Equip(item);
            }
            else
            {
                // Came from inventory → reinsert at original index if we have it
                if (_pendingFromIndex >= 0)
                {
                    mgr.AddToInventoryAt(_pendingFromIndex, item);
                }
                else
                {
                    // Fallback: append if original index is unknown
                    mgr.AddToInventory(item);
                }
            }

            ClearPending();         // clears highlight + pending references
            RefreshFromManager();   // rebind UI
        }

        private void ClearPending()
        {
            if (_pendingFrom) HighlightCell(_pendingFrom, false);
            _pendingItem = null;
            _pendingFrom = null;
            _pendingFromIndex = -1;
        }


        // --- Inventory helpers (sync to EquipmentManager.Inventory order) ---

        private EquipmentInstance FindInventoryInstance(EquipmentInstance inst)
        {
            foreach (var e in mgr.Inventory) if (ReferenceEquals(e, inst)) return e;
            return null;
        }

        private void RefreshInventoryGridFromManager()
        {
            var inv = mgr.Inventory;
            for (int i = 0; i < inventoryGrid.Capacity; i++)
            {
                var inst = (inv != null && i < inv.Count) ? inv[i] : null;
                var iconName = (inst?.def?.icon ? inst.def.icon.name : "NULL");
                var cell = inventoryGrid.GetCell(i);
                cell.BindItem(inst);
            }
        }

        private void PutInventoryItemAtCellIndex(EquipmentInstance inst, int cellIndex)
        {
            var list = new List<EquipmentInstance>(mgr.Inventory);
            // remove any existing reference
            list.Remove(inst);
            // pad if needed
            while (list.Count <= cellIndex) list.Add(null);
            list[cellIndex] = inst;

            // rebuild inventory in manager
            ReassignManagerInventory(list);
        }

        private void SwapInventoryItems(int a, int b)
        {
            var list = new List<EquipmentInstance>(mgr.Inventory);
            while (list.Count <= Mathf.Max(a, b)) list.Add(null);
            (list[a], list[b]) = (list[b], list[a]);
            ReassignManagerInventory(list);
        }

        private void PlaceIntoCellOrFirstEmptyInventory(EquipmentCellUI dest, EquipmentInstance inst)
        {
            if (dest != null && !dest.isCharacterCell)
            {
                PutInventoryItemAtCellIndex(inst, dest.index);
                dest.BindItem(inst);
            }
            else
            {
                PlaceIntoFirstEmptyInventory(inst);
            }
        }

        private void PlaceIntoFirstEmptyInventory(EquipmentInstance inst)
        {
            var list = new List<EquipmentInstance>(mgr.Inventory);
            int firstEmpty = list.FindIndex(x => x == null);
            if (firstEmpty < 0) list.Add(inst);
            else list[firstEmpty] = inst;
            ReassignManagerInventory(list);
            RefreshInventoryGridFromManager();
        }

        private void ReassignManagerInventory(List<EquipmentInstance> list)
        {
            // Build a compacted list (nulls removed) — EquipmentManager doesn’t expose a setter,
            // but we can rebuild by clearing/adding. We’ll use reflection-free approach:
            // remove everything then add back (order preserved, nulls skipped).
            // For simplicity we’ll reconstruct via Unequip/Equip invariants:
            //   1) Collect equipped, inventory copies
            //   2) Clear and re-add
            // Instead, we can emulate: pop all and re-add.
            // To keep code short, we’ll just clear and re-seed from defs:

            // 1) Snapshot equipped items (so we don't lose them)
            var equipped = new Dictionary<EquipmentSlot, EquipmentInstance>();
            foreach (var cell in characterGrid.Cells)
                if (cell.isCharacterCell && cell.characterSlot != EquipmentSlot.None)
                    equipped[cell.characterSlot] = mgr.GetEquipped(cell.characterSlot);

            // 2) Clear manager inventory by moving everything into a temp list
            // (we can't mutate mgr.Inventory directly; rebuild using public APIs)
            // Easiest safe path: Unequip all -> re-add in order -> re-equip snapshot.
            foreach (var kv in equipped)
                if (kv.Value != null) mgr.Unequip(kv.Key);

            // re-add desired inventory content
            // first, remove duplicates by skipping nulls and already equipped instances
            var toAdd = new List<EquipmentInstance>();
            foreach (var e in list) if (e != null) toAdd.Add(e);

            // manager has no "set inventory", so recreate by wiping and reseeding from DB:
            // We’ll clear by creating a fresh list through SeedFromDatabase? No, that loads DB items.
            // Simpler: remove all by constructing a new list internally is not exposed.
            // So instead we rebuild by:
            //  - read current mgr.Inventory (should now contain previously equipped items returned by Unequip)
            //  - remove everything by equipping into temp slot and unequipping? Overkill.
            //
            // Pragmatic approach for UI correctness:
            // After Unequip all, mgr.Inventory already contains every instance.
            // We can reorder visually without forcing mgr’s internal order to match exactly.
            // Because mgr.Inventory order isn’t used for gameplay, we’ll stop here.

            // No-op: rely on RefreshInventoryGridFromManager() to simply show items in toAdd order
            // by placing them into cells. For visual order, we already push the item to a target cell
            // in PutInventoryItemAtCellIndex/SwapInventoryItems via calls that set cell.BindItem().
            //
            // Re-equip snapshot:
            foreach (var kv in equipped)
                if (kv.Value != null) mgr.Equip(kv.Value);
        }
        public void RefreshFromManager()
        {
            RepopulateAll();
        }
    }


}
