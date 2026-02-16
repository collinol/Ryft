using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Cards;
using Game.Combat;
using Game.Core;
using Game.Player;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class AbilityButton : MonoBehaviour, IPointerClickHandler
    {
        public Image icon;
        public TMP_Text label;
        public TMP_Text cooldownText; // shows energy cost top-left

        [SerializeField] private string cardId;

        private TMP_Text descriptionText;
        private TMP_Text statText;

        private Button btn;
        private Image  targetGraphic;
        private Color  normalColor;
        private Color  grayColor;

        // Stat type colors
        private static readonly Color StrColor = new Color32(0xFF, 0x66, 0x44, 0xFF); // #FF6644
        private static readonly Color IntColor = new Color32(0x44, 0x99, 0xFF, 0xFF); // #4499FF
        private static readonly Color EngColor = new Color32(0x44, 0xCC, 0x66, 0xFF); // #44CC66

        void Awake()
        {
            btn = GetComponent<Button>();
            targetGraphic = GetComponent<Image>();
            normalColor = targetGraphic ? targetGraphic.color : Color.white;
            grayColor   = new Color(normalColor.r * 0.6f, normalColor.g * 0.6f, normalColor.b * 0.6f, normalColor.a);

            // Clip all children to card bounds
            if (!GetComponent<RectMask2D>())
                gameObject.AddComponent<RectMask2D>();

            // Remove Unity's onClick listener - we'll use IPointerClickHandler instead
            btn.onClick.RemoveAllListeners();

            // Hide icon if present — card is text-only
            if (icon) icon.gameObject.SetActive(false);

            // Configure label (card name) — top row, small bold
            if (label)
            {
                label.enableWordWrapping = true;
                label.overflowMode = TextOverflowModes.Truncate;
                label.alignment = TextAlignmentOptions.Center;
                label.enableAutoSizing = true;
                label.fontSizeMin = 6;
                label.fontSizeMax = 11;
                label.fontStyle = FontStyles.Bold;

                // Position: top center band (20%-80% wide, top 82%-98%)
                var lr = label.GetComponent<RectTransform>();
                lr.anchorMin = new Vector2(0.18f, 0.82f);
                lr.anchorMax = new Vector2(0.82f, 0.98f);
                lr.offsetMin = Vector2.zero;
                lr.offsetMax = Vector2.zero;
            }

            // Position cooldown/cost text: top-left (0%-20% wide, top 82%-98%)
            if (cooldownText)
            {
                var cr = cooldownText.GetComponent<RectTransform>();
                cr.anchorMin = new Vector2(0.02f, 0.82f);
                cr.anchorMax = new Vector2(0.2f, 0.98f);
                cr.offsetMin = Vector2.zero;
                cr.offsetMax = Vector2.zero;
                cooldownText.alignment = TextAlignmentOptions.Center;
                cooldownText.fontSize = 14;
                cooldownText.fontStyle = FontStyles.Bold;
                cooldownText.overflowMode = TextOverflowModes.Truncate;
            }

            // Create or find DescriptionText
            var descTf = transform.Find("DescriptionText");
            if (descTf != null)
            {
                descriptionText = descTf.GetComponent<TMP_Text>();
            }
            else
            {
                var go = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);
                descriptionText = go.GetComponent<TMP_Text>();
            }
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.enableWordWrapping = true;
            descriptionText.overflowMode = TextOverflowModes.Truncate;
            descriptionText.enableAutoSizing = true;
            descriptionText.fontSizeMin = 6;
            descriptionText.fontSizeMax = 10;
            descriptionText.raycastTarget = false;
            descriptionText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            // Position: card body (5%-95% wide, 4%-80% tall)
            var dr = descriptionText.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0.05f, 0.04f);
            dr.anchorMax = new Vector2(0.95f, 0.80f);
            dr.offsetMin = Vector2.zero;
            dr.offsetMax = Vector2.zero;

            // Create or find StatText
            var statTf = transform.Find("StatText");
            if (statTf != null)
            {
                statText = statTf.GetComponent<TMP_Text>();
            }
            else
            {
                var go = new GameObject("StatText", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);
                statText = go.GetComponent<TMP_Text>();
            }
            statText.alignment = TextAlignmentOptions.Center;
            statText.enableWordWrapping = false;
            statText.overflowMode = TextOverflowModes.Truncate;
            statText.fontSize = 11;
            statText.fontStyle = FontStyles.Bold;
            statText.raycastTarget = false;
            // Position: top-right (75%-98% wide, top 82%-98%)
            var sr = statText.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.75f, 0.82f);
            sr.anchorMax = new Vector2(0.98f, 0.98f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;

            // Ensure the button's image is raycast target
            if (targetGraphic)
            {
                targetGraphic.raycastTarget = true;
            }

            // Ensure button has proper rect transform
            var rt = GetComponent<RectTransform>();
            if (rt)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
            }
        }

        public void BindCard(CardDef def)
        {
            if (!def)
            {
                cardId = null;
                if (icon) icon.sprite = null;
                if (label) label.text = "";
                if (descriptionText) descriptionText.text = "";
                if (statText) statText.text = "";
                if (cooldownText) cooldownText.text = "";
                SetState(ready:false, showBlocked:false);

                // Disable raycast when no card
                if (targetGraphic) targetGraphic.raycastTarget = false;
                return;
            }

            cardId = def.id;
            if (icon)
            {
                icon.sprite = def.icon;
                icon.raycastTarget = false;
            }

            if (label)
            {
                label.text = string.IsNullOrWhiteSpace(def.displayName) ? def.name : def.displayName;
                if (label is Graphic graphic)
                    graphic.raycastTarget = false;
            }

            // Energy cost (top-left)
            if (cooldownText)
                cooldownText.text = def.energyCost.ToString();

            // Stat abbreviation (top-right)
            if (statText)
            {
                switch (def.statType)
                {
                    case CardStatType.Strength:
                        statText.text = "STR";
                        statText.color = StrColor;
                        statText.gameObject.SetActive(true);
                        break;
                    case CardStatType.Intellect:
                        statText.text = "INT";
                        statText.color = IntColor;
                        statText.gameObject.SetActive(true);
                        break;
                    case CardStatType.Engineering:
                        statText.text = "ENG";
                        statText.color = EngColor;
                        statText.gameObject.SetActive(true);
                        break;
                    default:
                        statText.text = "";
                        statText.gameObject.SetActive(false);
                        break;
                }
            }

            // Dynamic description with fully-modified power (stats + ryft effects + stance + etc.)
            if (descriptionText)
            {
                string desc = def.description ?? "";
                if (!string.IsNullOrEmpty(desc) && def.power > 0)
                {
                    int displayPower = GetDisplayPowerForCard(def);
                    string basePowerStr = def.power.ToString();
                    int idx = desc.IndexOf(basePowerStr);
                    if (idx >= 0)
                        desc = desc.Substring(0, idx) + displayPower.ToString() + desc.Substring(idx + basePowerStr.Length);
                }
                descriptionText.text = desc;
            }

            // Enable button raycast
            if (targetGraphic) targetGraphic.raycastTarget = true;

            RefreshFromController(FindObjectOfType<FightSceneController>());
        }

        /// <summary>
        /// Gets the fully-modified display power for a card, using the CardRuntime
        /// which accounts for stats, ryft effects, stance, overclock, etc.
        /// Falls back to simple stat scaling if runtime is unavailable.
        /// </summary>
        private int GetDisplayPowerForCard(CardDef def)
        {
            var fsc = FightSceneController.Instance;
            if (fsc != null)
            {
                var rt = fsc.GetCardRuntime(def.id);
                if (rt != null)
                    return rt.GetDisplayPower();
            }

            // Fallback: simple stat scaling (no ryft/stance/overclock modifiers)
            int statValue = 0;
            if (fsc != null)
            {
                var player = fsc.GetPlayer() as PlayerCharacter;
                if (player != null)
                {
                    var stats = player.TotalStats;
                    statValue = def.statType switch
                    {
                        CardStatType.Strength    => stats.strength,
                        CardStatType.Intellect   => stats.intellect,
                        CardStatType.Engineering => stats.engineering,
                        _ => 0
                    };
                }
            }
            return def.power + statValue * def.scaling;
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
            // Keep cooldownText showing energy cost — don't clear it

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

        // Tooltip removed — card face now shows fully-modified values directly
    }
}
