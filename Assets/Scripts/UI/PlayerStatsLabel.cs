using UnityEngine;
using Game.Player;
using Game.Core;
using TMPro;
using Game.Ryfts;
using Game.Combat;

namespace Game.UI
{
    public class PlayerStatsLabel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private TMP_Text text;

        [Header("Format")]
        [SerializeField] private bool showNames = false;
        [SerializeField] private bool showEnergy = true;
        [SerializeField] private bool showEnergyCredits = true;

        private FightSceneController fsc;

        void Awake()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            if (!text)   text   = GetComponent<TMP_Text>();
            // do NOT subscribe here—controller might not be alive yet
        }

        void OnEnable()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            if (player)  player.OnTurnStatsChanged += HandleTurnStatsChanged;

            BindController();
            RefreshNow();
        }

        void OnDisable()
        {
            if (player) player.OnTurnStatsChanged -= HandleTurnStatsChanged;
            if (fsc)    fsc.OnEnergyChanged       -= HandleEnergyChanged;
        }

        private void BindController()
        {
            // (Re)grab instance and subscribe if available
            var inst = FightSceneController.Instance;
            if (inst != fsc)
            {
                if (fsc) fsc.OnEnergyChanged -= HandleEnergyChanged;
                fsc = inst;
                if (fsc) fsc.OnEnergyChanged += HandleEnergyChanged;
            }
        }

        private void HandleTurnStatsChanged(Stats s)           => Render(s);
        private void HandleEnergyChanged(int current, int max) => RefreshNow();

        private void RefreshNow()
        {
            if (!player) return;
            if (!fsc) BindController();     // late-binding in case controller spawned after us
            Render(player.CurrentTurnStats);
        }

        private void Render(Stats s)
        {
            if (!text) return;

            string statsPart = showNames
                ? $"STR {s.strength} / MANA {s.mana} / ENG {s.engineering}"
                : $"{s.strength}/{s.mana}/{s.engineering}";

            string energyPart = "";
            if (showEnergy && fsc != null)
            {
                int cur = fsc.CurrentEnergy;
                int max = fsc.MaxEnergy;

                string credit = "";
                if (showEnergyCredits)
                {
                    int credits = RyftEffectManager.Ensure().PeekCredits();
                    if (credits > 0) credit = $" (+{credits})";
                }

                energyPart = showNames ? $"EN {cur}/{max}{credit}" : $"{cur}/{max}{credit}";

            }

            text.text = showEnergy && fsc != null ? $"{energyPart} | {statsPart}" : statsPart;
        }
    }
}
