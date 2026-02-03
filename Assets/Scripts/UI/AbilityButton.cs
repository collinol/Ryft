using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Cards;
using Game.Combat;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class AbilityButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image icon;
        public TMP_Text label;
        public TMP_Text cooldownText; // repurpose to show affordability (empty if playable)

        [SerializeField] private string cardId;
        private CardDef currentCard;

        private Button btn;
        private Image  targetGraphic;
        private Color  normalColor;
        private Color  grayColor;

        void Awake()
        {
            btn = GetComponent<Button>();
            targetGraphic = GetComponent<Image>();
            normalColor = targetGraphic ? targetGraphic.color : Color.white;
            grayColor   = new Color(normalColor.r * 0.6f, normalColor.g * 0.6f, normalColor.b * 0.6f, normalColor.a);

            // Remove Unity's onClick listener - we'll use IPointerClickHandler instead
            btn.onClick.RemoveAllListeners();

            // Configure text overflow handling for vertical card layout
            if (label)
            {
                label.enableWordWrapping = true;  // Allow wrapping for vertical cards
                label.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.fontSize = Mathf.Min(label.fontSize, 14); // Cap font size
                label.fontSizeMin = 8;
                label.fontSizeMax = 14;
                label.enableAutoSizing = true;
            }

            // Ensure the button's image is raycast target
            if (targetGraphic)
            {
                targetGraphic.raycastTarget = true;
            }

            // Ensure button has proper rect transform
            var rt = GetComponent<RectTransform>();
            if (rt)
            {
                // Don't stretch to parent
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
            }
        }

        public void BindCard(CardDef def)
        {
            currentCard = def;

            if (!def)
            {
                cardId = null;
                if (icon) icon.sprite = null;
                if (label) label.text = "";
                SetState(ready:false, showBlocked:false);

                // Disable raycast when no card
                if (targetGraphic) targetGraphic.raycastTarget = false;
                return;
            }

            cardId = def.id;
            if (icon)
            {
                icon.sprite = def.icon;
                // Icon should NOT block raycasts
                icon.raycastTarget = false;
            }

            if (label)
            {
                label.text = string.IsNullOrWhiteSpace(def.displayName) ? def.name : def.displayName;
                // Text should NOT block raycasts
                if (label is Graphic graphic)
                    graphic.raycastTarget = false;
            }

            // Enable button raycast
            if (targetGraphic) targetGraphic.raycastTarget = true;

            RefreshFromController(FindObjectOfType<FightSceneController>());
        }

        // Called by controller after plays / turns
        public void RefreshFromController(FightSceneController fsc)
        {
            if (string.IsNullOrEmpty(cardId) || fsc == null)
            {
                SetState(ready:true, showBlocked:false);
                return;
            }

            bool canPlay = fsc.CanPlayCard(cardId);
            SetState(canPlay, showBlocked:!canPlay);
        }

        private void SetState(bool ready, bool showBlocked)
        {
            if (btn) btn.interactable = ready;
            if (targetGraphic) targetGraphic.color = ready ? normalColor : grayColor;
            if (cooldownText) cooldownText.text = "";

            // Ensure raycast is always enabled for hover/tooltip
            if (targetGraphic)
            {
                targetGraphic.raycastTarget = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only respond to left clicks
            if (eventData.button != PointerEventData.InputButton.Left) return;

            // Check if button is interactable
            if (btn && !btn.interactable)
            {
                Debug.Log($"[AbilityButton] Card '{cardId}' is not playable right now");
                return;
            }

            if (string.IsNullOrEmpty(cardId))
            {
                Debug.Log("[AbilityButton] No card bound to this button");
                return;
            }

            var fsc = FindObjectOfType<FightSceneController>();
            if (fsc)
            {
                Debug.Log($"[AbilityButton] Attempting to play card: {cardId}");
                fsc.UsePlayerCard(cardId);
            }
            else
            {
                Debug.LogWarning("[AbilityButton] FightSceneController not found!");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentCard == null)
            {
                Debug.Log("[AbilityButton] OnPointerEnter - no current card");
                return;
            }

            var tooltip = CardTooltip.Instance;
            if (tooltip)
            {
                string title = string.IsNullOrWhiteSpace(currentCard.displayName) ? currentCard.name : currentCard.displayName;
                string description = string.IsNullOrWhiteSpace(currentCard.description) ? "No description." : currentCard.description;
                tooltip.Show(title, description, currentCard.energyCost, currentCard.power, currentCard.scaling);
            }
            else
            {
                Debug.LogWarning("[AbilityButton] CardTooltip instance not found!");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var tooltip = CardTooltip.Instance;
            if (tooltip)
            {
                tooltip.Hide();
            }
        }
    }
}
