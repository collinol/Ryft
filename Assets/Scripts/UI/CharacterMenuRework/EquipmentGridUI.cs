// Assets/Scripts/UI/CharacterMenuRework/EquipmentGridUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Game.UI.Inventory
{
    public class EquipmentGridUI : MonoBehaviour
    {
        [Header("Header (optional)")]
        [SerializeField] TMP_Text headerText;

        [Header("Layout")]
        [SerializeField] GridLayoutGroup grid;      // assign CharacterGrid / InventoryGrid
        [SerializeField, Min(1)] int rows = 3;
        [SerializeField, Min(1)] int cols = 4;

        [Header("Cell Prefab / Look")]
        [SerializeField] EquipmentCellUI cellPrefab;
        [SerializeField] Sprite cellBackground;

        [Header("Sizing")]
        [SerializeField] int cellSize = 96;
        [SerializeField] int spacing = 8;

        [Header("Character Grid Mapping")]
        [SerializeField] bool isCharacterGrid = false;
        [SerializeField] List<Game.Equipment.EquipmentSlot> characterSlots = new();

        public bool Owns(EquipmentCellUI c) => c && _cells.Contains(c);
        public int IndexOf(EquipmentCellUI c) => c ? _cells.IndexOf(c) : -1;
        public string DebugName => isCharacterGrid ? "CHAR" : "INV";

        public IReadOnlyList<EquipmentCellUI> Cells => _cells;
        public int Capacity => rows * cols;

        private readonly List<EquipmentCellUI> _cells = new();

        public void SetHeader(string text)
        {
            if (headerText) headerText.text = text;
        }

        public void Build(System.Action<EquipmentCellUI> onClicked)
        {
            if (!grid || !cellPrefab) return;

            // 1) configure grid sizing
            grid.spacing     = new Vector2(spacing, spacing);
            grid.constraint  = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            grid.cellSize    = new Vector2(cellSize, cellSize);
            grid.childAlignment = TextAnchor.UpperLeft;

            // 2) size the grid Rect via LayoutElement so it won’t collapse
            var le = grid.GetComponent<LayoutElement>() ?? grid.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth   = 1f;
            var totalW = cols * cellSize + (cols - 1) * spacing;
            var totalH = rows * cellSize + (rows - 1) * spacing;
            le.preferredWidth  = totalW;
            le.preferredHeight = totalH;

            // 3) rebuild cells
            for (int i = grid.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(grid.transform.GetChild(i).gameObject);

            _cells.Clear();

            for (int i = 0; i < Capacity; i++)
            {
                var cell = Instantiate(cellPrefab, grid.transform);
                cell.name = $"Cell_{i}";
                cell.SetBackground(cellBackground);
                cell.index = i;
                cell.isCharacterCell = isCharacterGrid;

                // slot mapping only for character grid
                if (isCharacterGrid)
                    cell.characterSlot = (i < characterSlots.Count) ? characterSlots[i] : Game.Equipment.EquipmentSlot.None;

                cell.onClicked = onClicked;
                _cells.Add(cell);
            }
        }

        public EquipmentCellUI GetCell(int i) => (i >= 0 && i < _cells.Count) ? _cells[i] : null;
    }
}
