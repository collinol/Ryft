using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Abilities;
using Game.Player;
using Game.Combat;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class AbilityButton : MonoBehaviour
    {
        public Image icon;
        public TMP_Text label;
        public TMP_Text cooldownText;

        [SerializeField] private string abilityId;

        private Button btn;
        private Image targetGraphic;
        private Color normalColor;
        private Color grayColor;

        void Awake()
        {
            btn = GetComponent<Button>();
            targetGraphic = GetComponent<Image>();
            normalColor = targetGraphic ? targetGraphic.color : Color.white;
            grayColor = new Color(normalColor.r * 0.6f, normalColor.g * 0.6f, normalColor.b * 0.6f, normalColor.a);
            btn.onClick.AddListener(OnClick);
        }

        public void Bind(PlayerCharacter p, AbilityDef def)
        {
            if (!def)
            {
                abilityId = null;
                if (icon) icon.sprite = null;
                if (label) label.text = "";
                SetState(ready:false, cd:0, showText:false);
                return;
            }

            abilityId = def.id;
            if (icon)  icon.sprite = def.icon;
            if (label) label.text  = string.IsNullOrWhiteSpace(def.displayName) ? def.name : def.displayName;

            RefreshFromController(FindObjectOfType<FightSceneController>());
        }

        // Called by controller after casts and end turn
        public void RefreshFromController(FightSceneController fsc)
        {
            if (string.IsNullOrEmpty(abilityId) || fsc == null)
            {
                SetState(ready:true, cd:0, showText:false);
                return;
            }

            int cd   = fsc.GetCooldownRemaining(abilityId);
            bool ready = cd <= 0;                 // <<< use cooldown, not runtime.IsReady

            SetState(ready, cd, showText:!ready); // gray + show CD while cooling
        }


        private void SetState(bool ready, int cd, bool showText)
        {
            if (btn) btn.interactable = ready;
            if (targetGraphic) targetGraphic.color = ready ? normalColor : grayColor;
            if (cooldownText) cooldownText.text = showText ? $"{cd}" : "";
        }

        private void OnClick()
        {
            if (string.IsNullOrEmpty(abilityId)) return;
            var fsc = FindObjectOfType<FightSceneController>();
            fsc?.UsePlayerAbility(abilityId);
        }
    }
}
