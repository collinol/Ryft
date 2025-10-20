using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Combat;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class AbilityButton : MonoBehaviour
    {
        public Image icon;
        public TMP_Text label;
        public TMP_Text cooldownText; // repurpose to show affordability (empty if playable)

        [SerializeField] private string cardId;

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
            btn.onClick.AddListener(OnClick);
        }

        public void BindCard(CardDef def)
        {
            if (!def)
            {
                cardId = null;
                if (icon) icon.sprite = null;
                if (label) label.text = "";
                SetState(ready:false, showBlocked:false);
                return;
            }

            cardId = def.id;
            if (icon)  icon.sprite = def.icon;
            if (label) label.text  = string.IsNullOrWhiteSpace(def.displayName) ? def.name : def.displayName;

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
        }

        private void OnClick()
        {
            if (string.IsNullOrEmpty(cardId)) return;
            var fsc = FindObjectOfType<FightSceneController>();
            fsc?.UsePlayerCard(cardId);
        }
    }
}
