using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Cards;
using Game.Combat;

namespace Game.UI
{
    /// Displays the player's current hand.
    public class AbilityBarUI : MonoBehaviour
    {
        [SerializeField] private AbilityButton[] slots;
        [SerializeField] private float slotWidth  = 100f;  // Card-like proportions
        [SerializeField] private float slotHeight = 140f;  // Taller than wide
        [SerializeField] private float spacing = 12f;
        [SerializeField] private int padding = 15;

        FightSceneController ctrl;

        void Awake()
        {
            ctrl = FindObjectOfType<FightSceneController>();
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<AbilityButton>(true)
                        .OrderBy(b => b.transform.GetSiblingIndex()).ToArray();

            // Configure each slot
            foreach (var b in slots)
            {
                if (!b) continue;

                // Set layout element
                var le = b.GetComponent<LayoutElement>() ?? b.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth  = slotWidth;
                le.preferredHeight = slotHeight;
                le.minWidth = slotWidth;
                le.minHeight = slotHeight;

                // Ensure proper rect transform setup
                var rt = b.GetComponent<RectTransform>();
                if (rt)
                {
                    rt.sizeDelta = new Vector2(slotWidth, slotHeight);
                }
            }

            // Configure layout group
            var hlg = GetComponent<HorizontalLayoutGroup>();
            if (hlg == null)
            {
                hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            hlg.spacing = spacing;
            hlg.padding = new RectOffset(padding, padding, padding, padding);
            hlg.childControlWidth  = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            // Add ContentSizeFitter if not present
            var csf = GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Debug.Log($"[AbilityBarUI] Configured {slots.Length} slots with width={slotWidth}, height={slotHeight}, spacing={spacing}");
        }

        void OnEnable()  { StartCoroutine(RefreshNextFrame()); }

        private IEnumerator RefreshNextFrame() { yield return null; Refresh(); }

        public void Refresh()
        {
            ctrl ??= FindObjectOfType<FightSceneController>();
            var hand = ctrl?.CurrentHand;
            int count = hand?.Count ?? 0;

            for (int i = 0; i < slots.Length; i++)
            {
                var btn = slots[i];
                if (!btn) continue;
                btn.gameObject.SetActive(true);

                CardDef def = (hand != null && i < count) ? hand[i] : null;
                btn.BindCard(def);
            }
        }
    }
}
