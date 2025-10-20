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
        [SerializeField] private float slotWidth  = 160f;
        [SerializeField] private float slotHeight = 48f;

        FightSceneController ctrl;

        void Awake()
        {
            ctrl = FindObjectOfType<FightSceneController>();
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<AbilityButton>(true)
                        .OrderBy(b => b.transform.GetSiblingIndex()).ToArray();

            foreach (var b in slots)
            {
                if (!b) continue;
                var le = b.GetComponent<LayoutElement>() ?? b.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth  = slotWidth;
                le.preferredHeight = slotHeight;
            }

            var hlg = GetComponent<HorizontalLayoutGroup>();
            if (hlg)
            {
                hlg.childControlWidth  = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth  = false;
                hlg.childForceExpandHeight = false;
            }
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
