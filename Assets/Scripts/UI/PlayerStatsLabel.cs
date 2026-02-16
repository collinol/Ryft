using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private bool showEnergy = true;
        [SerializeField] private bool showEnergyCredits = true;

        private FightSceneController fsc;
        private TMP_Text panelText;

        void Awake()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            BuildPanel();
        }

        private void BuildPanel()
        {
            // Find a screen-space overlay canvas, skip world-space health bar canvases
            Canvas rootCanvas = null;
            foreach (var c in FindObjectsOfType<Canvas>())
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    rootCanvas = c;
                    break;
                }
            }

            if (rootCanvas == null)
            {
                var canvasGo = new GameObject("StatsUICanvas");
                rootCanvas = canvasGo.AddComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                rootCanvas.sortingOrder = 100;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Create a fixed panel anchored to top-left
            var panelGO = new GameObject("StatsPanel", typeof(RectTransform));
            panelGO.transform.SetParent(rootCanvas.transform, false);

            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 1f);
            panelRT.anchorMax = new Vector2(0f, 1f);
            panelRT.pivot = new Vector2(0f, 1f);
            panelRT.anchoredPosition = new Vector2(8f, -8f);
            panelRT.sizeDelta = new Vector2(280f, 36f);

            // Semi-transparent background
            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.5f);
            bg.raycastTarget = false;

            // Text inside the panel
            var textGO = new GameObject("StatsText", typeof(RectTransform));
            textGO.transform.SetParent(panelGO.transform, false);

            panelText = textGO.AddComponent<TextMeshProUGUI>();
            panelText.fontSize = 16f;
            panelText.enableAutoSizing = false;
            panelText.color = Color.white;
            panelText.alignment = TextAlignmentOptions.MidlineLeft;
            panelText.overflowMode = TextOverflowModes.Overflow;
            panelText.raycastTarget = false;

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(8f, 2f);
            textRT.offsetMax = new Vector2(-4f, -2f);

            // Hide the original text if it exists
            if (text != null) text.enabled = false;
        }

        void OnEnable()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
            if (player) player.OnTurnStatsChanged += HandleTurnStatsChanged;

            BindController();
            RefreshNow();
        }

        void OnDisable()
        {
            if (player) player.OnTurnStatsChanged -= HandleTurnStatsChanged;
            if (fsc) fsc.OnEnergyChanged -= HandleEnergyChanged;
        }

        private void BindController()
        {
            var inst = FightSceneController.Instance;
            if (inst != fsc)
            {
                if (fsc) fsc.OnEnergyChanged -= HandleEnergyChanged;
                fsc = inst;
                if (fsc) fsc.OnEnergyChanged += HandleEnergyChanged;
            }
        }

        private void HandleTurnStatsChanged(Stats s) => Render(s);
        private void HandleEnergyChanged(int current, int max) => RefreshNow();

        private void RefreshNow()
        {
            if (!player) return;
            if (!fsc) BindController();
            Render(player.CurrentTurnStats);
        }

        private void Render(Stats s)
        {
            var target = panelText != null ? panelText : text;
            if (target == null) return;

            string energyPart = "";
            if (showEnergy && fsc != null)
            {
                int cur = fsc.CurrentEnergy;
                int max = fsc.MaxEnergy;
                string credit = "";
                if (showEnergyCredits)
                {
                    int credits = RyftEffectManager.Ensure().PeekCredits();
                    if (credits > 0) credit = $"+{credits}";
                }
                energyPart = credit.Length > 0
                    ? $"EN {cur}/{max}({credit})"
                    : $"EN {cur}/{max}";
            }

            string stats = $"STR {s.strength}  INT {s.intellect}  ENG {s.engineering}";
            target.text = showEnergy && fsc != null ? $"{energyPart}  |  {stats}" : stats;
        }
    }
}
