// Assets/Scripts/UI/CharacterMenuRework/EquipmentCellUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Equipment;

namespace Game.UI.Inventory
{
    public class EquipmentCellUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visuals")]
        [SerializeField] private Image background;                 // Grey.png
        [SerializeField] private EquipmentItemUI itemViewPrefab;
        [SerializeField] private RectTransform itemAnchor;         // child RectTransform
        [SerializeField] private TMP_Text slotLabel;               // optional for Character grid

        [Header("Identity")]
        public int index;                                          // index in its grid
        public bool isCharacterCell;                               // true for left grid
        public EquipmentSlot characterSlot = EquipmentSlot.None;   // which slot if Character

        public EquipmentItemUI ItemView { get; private set; }
        private EquipmentInstance current;                         // <— declare this

        public System.Action<EquipmentCellUI> onClicked;           // set by controller
        public EquipmentInstance GetItem() => ItemView ? ItemView.Instance : null;

        public RectTransform ItemAnchor => itemAnchor;
        public bool IsCharacterCell => isCharacterCell;

        public void SetTint(Color c)
        {
            if (background) background.color = c;
        }
        private void HighlightCell(EquipmentCellUI cell, bool on)
        {
            cell?.SetTint(on ? new Color(1f,1f,1f,0.75f) : Color.white);
        }
        void Awake()
        {
            // Ensure we have an item view child that fills the cell
            if (!ItemView && itemViewPrefab && itemAnchor)
            {
                ItemView = Instantiate(itemViewPrefab, itemAnchor);

                var rt = ItemView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin     = Vector2.zero;
                    rt.anchorMax     = Vector2.one;
                    rt.offsetMin     = Vector2.zero;
                    rt.offsetMax     = Vector2.zero;
                    rt.pivot         = new Vector2(0.5f, 0.5f);
                    rt.localScale    = Vector3.one;
                    rt.localPosition = Vector3.zero;
                }
            }
        }

        public void SetBackground(Sprite sprite)
        {
            if (background) background.sprite = sprite;
        }

        public void SetSlotLabel(string text)
        {
            if (slotLabel) slotLabel.text = text ?? string.Empty;
        }

        public void BindItem(EquipmentInstance inst)
        {
            current = inst;

            // Ensure (again) we have a view (covers runtime-created cells)
            if (!ItemView && itemViewPrefab && itemAnchor)
            {
                ItemView = Instantiate(itemViewPrefab, itemAnchor);

                var rt = ItemView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin     = Vector2.zero;
                    rt.anchorMax     = Vector2.one;
                    rt.offsetMin     = Vector2.zero;
                    rt.offsetMax     = Vector2.zero;
                    rt.pivot         = new Vector2(0.5f, 0.5f);
                    rt.localScale    = Vector3.one;
                    rt.localPosition = Vector3.zero;
                }
            }

            // Debug: what are we binding?
            var id   = inst?.def?.id ?? "NULL";
            var icon = (inst?.def?.icon ? inst.def.icon.name : "NULL");

            // Forward to the item view (it will toggle its Image visibility)
            if (ItemView) ItemView.Bind(inst);
        }

        public void ClearItem()
        {
            if (ItemView) ItemView.Bind(null);
            current = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(this);
        }


        public void EnsureView()
        {
            if (ItemView || !itemViewPrefab || !itemAnchor) return;

            ItemView = Instantiate(itemViewPrefab, itemAnchor);
            var rt = ItemView.GetComponent<RectTransform>();
            if (rt)
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
            }
        }

        // lets the controller inject a freshly-instantiated view
        public void ForceAssignView(EquipmentItemUI view)
        {
            ItemView = view;
        }
    }
}
