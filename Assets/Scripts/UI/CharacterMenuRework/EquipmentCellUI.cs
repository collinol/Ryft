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
            // Create label if it doesn't exist
            if (slotLabel == null && !string.IsNullOrEmpty(text))
            {
                var labelGo = new GameObject("SlotLabel");
                labelGo.transform.SetParent(transform, false);

                var rt = labelGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0.3f);
                rt.offsetMin = new Vector2(2, 2);
                rt.offsetMax = new Vector2(-2, 0);

                slotLabel = labelGo.AddComponent<TMP_Text>();
                // TMP_Text is abstract, we need TextMeshProUGUI
            }

            if (slotLabel != null)
            {
                slotLabel.text = text ?? string.Empty;
            }
        }

        /// <summary>
        /// Creates the slot label with proper styling. Call after instantiation.
        /// </summary>
        public void CreateSlotLabelIfNeeded(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Check if label already exists
            if (slotLabel != null)
            {
                slotLabel.text = text;
                return;
            }

            // Create new label
            var labelGo = new GameObject("SlotLabel");
            labelGo.transform.SetParent(transform, false);

            var rt = labelGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0.35f);
            rt.offsetMin = new Vector2(1, 1);
            rt.offsetMax = new Vector2(-1, 0);

            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 10;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Truncate;

            slotLabel = tmp;
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
