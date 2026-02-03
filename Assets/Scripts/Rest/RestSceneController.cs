using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Player;

namespace Game.Rest
{
    /// <summary>
    /// Controls the rest/health station scene.
    /// Allows player to heal 30% of max HP.
    /// </summary>
    public class RestSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI healAmountText;
        [SerializeField] private Button healButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Image healthFillImage;

        [Header("Settings")]
        [SerializeField, Range(0.1f, 0.5f)] private float healPercent = 0.3f; // 30%

        private PlayerCharacter player;
        private bool hasHealed = false;

        void Start()
        {
            // Find or create player reference
            player = FindObjectOfType<PlayerCharacter>();

            if (player == null)
            {
                Debug.LogWarning("[RestScene] No PlayerCharacter found, creating fallback");
            }

            CreateUI();
            UpdateHealthDisplay();
        }

        private void CreateUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var canvasGo = new GameObject("RestCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.15f, 0.2f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "Rest Station";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.8f, 0.9f, 1f);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.2f, 0.75f);
            titleRt.anchorMax = new Vector2(0.8f, 0.9f);

            // Health bar background
            var healthBgGo = new GameObject("HealthBarBg");
            healthBgGo.transform.SetParent(canvas.transform, false);
            var healthBgImg = healthBgGo.AddComponent<Image>();
            healthBgImg.color = new Color(0.2f, 0.1f, 0.1f);
            var healthBgRt = healthBgGo.GetComponent<RectTransform>();
            healthBgRt.anchorMin = new Vector2(0.25f, 0.55f);
            healthBgRt.anchorMax = new Vector2(0.75f, 0.62f);

            // Health bar fill
            var healthFillGo = new GameObject("HealthBarFill");
            healthFillGo.transform.SetParent(healthBgGo.transform, false);
            healthFillImage = healthFillGo.AddComponent<Image>();
            healthFillImage.color = new Color(0.2f, 0.8f, 0.2f);
            var healthFillRt = healthFillGo.GetComponent<RectTransform>();
            healthFillRt.anchorMin = Vector2.zero;
            healthFillRt.anchorMax = new Vector2(0.5f, 1f); // Will be updated
            healthFillRt.offsetMin = Vector2.zero;
            healthFillRt.offsetMax = Vector2.zero;

            // Health text
            var healthTextGo = new GameObject("HealthText");
            healthTextGo.transform.SetParent(canvas.transform, false);
            healthText = healthTextGo.AddComponent<TextMeshProUGUI>();
            healthText.text = "HP: 0 / 0";
            healthText.fontSize = 28;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.color = Color.white;
            var healthTextRt = healthTextGo.GetComponent<RectTransform>();
            healthTextRt.anchorMin = new Vector2(0.25f, 0.45f);
            healthTextRt.anchorMax = new Vector2(0.75f, 0.55f);

            // Heal amount text
            var healAmountGo = new GameObject("HealAmount");
            healAmountGo.transform.SetParent(canvas.transform, false);
            healAmountText = healAmountGo.AddComponent<TextMeshProUGUI>();
            healAmountText.fontSize = 24;
            healAmountText.alignment = TextAlignmentOptions.Center;
            healAmountText.color = new Color(0.5f, 1f, 0.5f);
            var healAmountRt = healAmountGo.GetComponent<RectTransform>();
            healAmountRt.anchorMin = new Vector2(0.25f, 0.38f);
            healAmountRt.anchorMax = new Vector2(0.75f, 0.45f);

            // Heal button
            var healBtnGo = new GameObject("HealButton");
            healBtnGo.transform.SetParent(canvas.transform, false);
            var healBtnRt = healBtnGo.AddComponent<RectTransform>();
            healBtnRt.anchorMin = new Vector2(0.3f, 0.2f);
            healBtnRt.anchorMax = new Vector2(0.5f, 0.32f);
            var healBtnImg = healBtnGo.AddComponent<Image>();
            healBtnImg.color = new Color(0.2f, 0.6f, 0.2f);
            healButton = healBtnGo.AddComponent<Button>();
            healButton.onClick.AddListener(OnHealClicked);

            var healTextGo = new GameObject("Text");
            healTextGo.transform.SetParent(healBtnGo.transform, false);
            var healBtnText = healTextGo.AddComponent<TextMeshProUGUI>();
            healBtnText.text = "Rest & Heal";
            healBtnText.fontSize = 24;
            healBtnText.alignment = TextAlignmentOptions.Center;
            healBtnText.color = Color.white;
            var healBtnTextRt = healTextGo.GetComponent<RectTransform>();
            healBtnTextRt.anchorMin = Vector2.zero;
            healBtnTextRt.anchorMax = Vector2.one;

            // Leave button
            var leaveBtnGo = new GameObject("LeaveButton");
            leaveBtnGo.transform.SetParent(canvas.transform, false);
            var leaveBtnRt = leaveBtnGo.AddComponent<RectTransform>();
            leaveBtnRt.anchorMin = new Vector2(0.5f, 0.2f);
            leaveBtnRt.anchorMax = new Vector2(0.7f, 0.32f);
            var leaveBtnImg = leaveBtnGo.AddComponent<Image>();
            leaveBtnImg.color = new Color(0.4f, 0.3f, 0.3f);
            leaveButton = leaveBtnGo.AddComponent<Button>();
            leaveButton.onClick.AddListener(OnLeaveClicked);

            var leaveTextGo = new GameObject("Text");
            leaveTextGo.transform.SetParent(leaveBtnGo.transform, false);
            var leaveBtnText = leaveTextGo.AddComponent<TextMeshProUGUI>();
            leaveBtnText.text = "Continue";
            leaveBtnText.fontSize = 24;
            leaveBtnText.alignment = TextAlignmentOptions.Center;
            leaveBtnText.color = Color.white;
            var leaveBtnTextRt = leaveTextGo.GetComponent<RectTransform>();
            leaveBtnTextRt.anchorMin = Vector2.zero;
            leaveBtnTextRt.anchorMax = Vector2.one;
        }

        private void UpdateHealthDisplay()
        {
            if (player == null)
            {
                if (healthText) healthText.text = "HP: -- / --";
                if (healAmountText) healAmountText.text = "";
                return;
            }

            int current = player.Health;
            int max = player.TotalStats.maxHealth;
            int healAmount = CalculateHealAmount();

            if (healthText)
            {
                healthText.text = $"HP: {current} / {max}";
            }

            if (healthFillImage)
            {
                float fillPercent = max > 0 ? (float)current / max : 0;
                var rt = healthFillImage.GetComponent<RectTransform>();
                rt.anchorMax = new Vector2(fillPercent, 1f);
            }

            if (healAmountText)
            {
                if (hasHealed)
                {
                    healAmountText.text = "You have rested.";
                    healAmountText.color = Color.gray;
                }
                else if (current >= max)
                {
                    healAmountText.text = "Already at full health!";
                    healAmountText.color = new Color(0.5f, 0.8f, 0.5f);
                }
                else
                {
                    healAmountText.text = $"Heal for {healAmount} HP ({Mathf.RoundToInt(healPercent * 100)}% of max)";
                    healAmountText.color = new Color(0.5f, 1f, 0.5f);
                }
            }

            if (healButton)
            {
                healButton.interactable = !hasHealed && current < max;
            }
        }

        private int CalculateHealAmount()
        {
            if (player == null) return 0;
            int max = player.TotalStats.maxHealth;
            return Mathf.Max(1, Mathf.RoundToInt(max * healPercent));
        }

        private void OnHealClicked()
        {
            if (player == null || hasHealed) return;

            int healAmount = CalculateHealAmount();
            player.Heal(healAmount);
            hasHealed = true;

            Debug.Log($"[RestScene] Player healed for {healAmount} HP. Current: {player.Health}/{player.TotalStats.maxHealth}");

            UpdateHealthDisplay();
        }

        private void OnLeaveClicked()
        {
            Debug.Log("[RestScene] Leaving rest station, returning to map");
            SceneManager.LoadScene("MapScene");
        }
    }
}
