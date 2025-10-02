using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Player;
using Game.Abilities;

namespace Game.UI
{
    public class AbilityBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private AbilityButton[] slots; // you already assigned these
        [SerializeField] private float slotWidth  = 160f;
        [SerializeField] private float slotHeight = 48f;

        void Awake()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<AbilityButton>(true)
                        .OrderBy(b => b.transform.GetSiblingIndex()).ToArray();

            // ensure each slot has size hints so HLG won’t collapse them
            foreach (var b in slots)
            {
                if (!b) continue;
                var le = b.GetComponent<LayoutElement>() ?? b.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth  = slotWidth;
                le.preferredHeight = slotHeight;
            }

            // sane defaults on the parent layout
            var hlg = GetComponent<HorizontalLayoutGroup>();
            if (hlg)
            {
                hlg.childControlWidth  = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth  = false;
                hlg.childForceExpandHeight = false;
            }

            if (player != null) player.AbilitiesChanged += Refresh;
        }

        void OnEnable()  { StartCoroutine(RefreshNextFrame()); }
        void OnDestroy() { if (player != null) player.AbilitiesChanged -= Refresh; }

        private IEnumerator RefreshNextFrame()
        {
            // wait one frame so PlayerCharacter.Start() can grant defaults
            yield return null;
            Refresh();
        }

        public void Refresh()
        {

            var loadout = player?.AbilityLoadout;
            int count = loadout?.Count ?? 0;

            for (int i = 0; i < slots.Length; i++)
            {

                var btn = slots[i];
                if (!btn) continue;

                // keep slot visible; bind will disable it if empty
                btn.gameObject.SetActive(true);

                AbilityDef def = (loadout != null && i < count) ? loadout[i] : null;
                btn.Bind(player, def);

            }
        }
    }
}
