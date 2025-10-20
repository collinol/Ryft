using UnityEngine;
using Game.Player;
using Game.Core;
using TMPro;

namespace Game.UI
{
    /// Attach this to a World-Space canvas Text under the Player, or give it a TMP_Text explicitly.
    public class PlayerStatsLabel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private TMP_Text text;     // Or use UnityEngine.UI.Text and change type/signature

        [Header("Format")]
        [Tooltip("If true, shows labels like STR/DEF, otherwise just '5/0'.")]
        [SerializeField] private bool showNames = false;

        void Awake()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            if (!text)   text   = GetComponent<TMP_Text>();

            if (player != null)
                player.OnTurnStatsChanged += HandleTurnStatsChanged;

            // seed the label with current values
            RefreshNow();
        }

        void OnDestroy()
        {
            if (player != null)
                player.OnTurnStatsChanged -= HandleTurnStatsChanged;
        }

        private void HandleTurnStatsChanged(Stats s) => SetLabel(s);

        private void RefreshNow()
        {
            if (!player) return;
            SetLabel(player.CurrentTurnStats);
        }

        private void SetLabel(Stats s)
        {
            if (!text) return;

            // exactly as requested: "strength/defense" like "5/0"
            if (!showNames)
            {
                text.text = $"{s.strength}/{s.mana}/{s.engineering}";
            }
            else
            {
                text.text = $"STR {s.strength} / MANA {s.mana} / ENG {s.engineering}";
            }
        }
    }
}
