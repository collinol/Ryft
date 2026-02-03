using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Equipment;

namespace Game.Rewards
{
    /// <summary>
    /// Orchestrates the post-combat reward selection screen.
    /// Regular fights offer 3 cards, elite fights offer 3 equipment.
    /// </summary>
    public class RewardSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private GameObject cardRewardPrefab;
        [SerializeField] private GameObject equipmentRewardPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button skipButton;

        [Header("Settings")]
        [SerializeField] private int cardChoiceCount = 3;
        [SerializeField] private int equipmentChoiceCount = 3;

        private List<CardDef> cardRewards = new();
        private List<EquipmentDef> equipmentRewards = new();
        private bool isEliteReward;
        private CardDatabase cardDb;
        private EquipmentDatabase equipDb;

        void Start()
        {
            cardDb = CardDatabase.Load();
            equipDb = EquipmentDatabase.Load();

            // Determine reward type
            isEliteReward = MapSession.I != null && MapSession.I.IsEliteFight;

            // Update title
            if (titleText)
            {
                titleText.text = isEliteReward ? "Elite Defeated! Choose Equipment:" : "Victory! Choose a Card:";
            }

            // Show gold
            UpdateGoldDisplay();

            // Generate rewards
            if (isEliteReward)
            {
                GenerateEquipmentRewards();
            }
            else
            {
                GenerateCardRewards();
            }

            // Setup skip button
            if (skipButton)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        private void UpdateGoldDisplay()
        {
            if (goldText && MapSession.I != null)
            {
                goldText.text = $"Gold: {MapSession.I.Gold}";
            }
        }

        private void GenerateCardRewards()
        {
            if (cardDb == null)
            {
                Debug.LogError("[RewardScene] CardDatabase not found!");
                ReturnToMap();
                return;
            }

            cardRewards.Clear();
            var rng = new System.Random();

            for (int i = 0; i < cardChoiceCount; i++)
            {
                var card = cardDb.RollReward(rng);
                if (card != null && !cardRewards.Contains(card))
                {
                    cardRewards.Add(card);
                }
            }

            // Fill with any cards if we don't have enough unique ones
            while (cardRewards.Count < cardChoiceCount)
            {
                var card = cardDb.RollReward(rng);
                if (card != null)
                {
                    cardRewards.Add(card);
                    break; // Don't infinite loop
                }
                else break;
            }

            DisplayCardRewards();
        }

        private void GenerateEquipmentRewards()
        {
            if (equipDb == null)
            {
                Debug.LogError("[RewardScene] EquipmentDatabase not found!");
                ReturnToMap();
                return;
            }

            var rng = new System.Random();
            equipmentRewards = equipDb.RollRewards(equipmentChoiceCount, rng, EquipmentRarity.Uncommon);

            DisplayEquipmentRewards();
        }

        private void DisplayCardRewards()
        {
            if (rewardContainer == null || cardRewardPrefab == null)
            {
                Debug.LogWarning("[RewardScene] Missing UI references, creating fallback UI");
                CreateFallbackUI();
                return;
            }

            foreach (var card in cardRewards)
            {
                var go = Instantiate(cardRewardPrefab, rewardContainer);
                var rewardUI = go.GetComponent<RewardItemUI>();
                if (rewardUI)
                {
                    rewardUI.SetupCard(card, OnCardSelected);
                }
            }
        }

        private void DisplayEquipmentRewards()
        {
            if (rewardContainer == null || equipmentRewardPrefab == null)
            {
                Debug.LogWarning("[RewardScene] Missing UI references, creating fallback UI");
                CreateFallbackUI();
                return;
            }

            foreach (var equip in equipmentRewards)
            {
                var go = Instantiate(equipmentRewardPrefab, rewardContainer);
                var rewardUI = go.GetComponent<RewardItemUI>();
                if (rewardUI)
                {
                    rewardUI.SetupEquipment(equip, OnEquipmentSelected);
                }
            }
        }

        private void CreateFallbackUI()
        {
            // Create simple fallback UI if prefabs aren't assigned
            var canvas = FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var canvasGo = new GameObject("FallbackCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Create container
            var container = new GameObject("RewardContainer");
            container.transform.SetParent(canvas.transform, false);
            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            if (isEliteReward)
            {
                for (int i = 0; i < equipmentRewards.Count; i++)
                {
                    CreateFallbackEquipmentButton(container.transform, equipmentRewards[i], i);
                }
            }
            else
            {
                for (int i = 0; i < cardRewards.Count; i++)
                {
                    CreateFallbackCardButton(container.transform, cardRewards[i], i);
                }
            }

            // Skip button
            var skipGo = new GameObject("SkipButton");
            skipGo.transform.SetParent(canvas.transform, false);
            var skipRt = skipGo.AddComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.5f, 0.1f);
            skipRt.anchorMax = new Vector2(0.5f, 0.1f);
            skipRt.sizeDelta = new Vector2(200, 50);
            var skipImg = skipGo.AddComponent<Image>();
            skipImg.color = new Color(0.3f, 0.3f, 0.3f);
            var skipBtn = skipGo.AddComponent<Button>();
            skipBtn.onClick.AddListener(OnSkipClicked);

            var skipTextGo = new GameObject("Text");
            skipTextGo.transform.SetParent(skipGo.transform, false);
            var skipText = skipTextGo.AddComponent<TextMeshProUGUI>();
            skipText.text = "Skip Reward";
            skipText.fontSize = 24;
            skipText.alignment = TextAlignmentOptions.Center;
            skipText.color = Color.white;
            var skipTextRt = skipTextGo.GetComponent<RectTransform>();
            skipTextRt.anchorMin = Vector2.zero;
            skipTextRt.anchorMax = Vector2.one;
            skipTextRt.sizeDelta = Vector2.zero;
        }

        private void CreateFallbackCardButton(Transform parent, CardDef card, int index)
        {
            var go = new GameObject($"CardReward_{index}");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 200);

            var img = go.AddComponent<Image>();
            img.color = GetRarityColor(card.rarity);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardSelected(card));

            // Card name
            var textGo = new GameObject("Name");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = card.displayName;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0.6f);
            textRt.anchorMax = new Vector2(1, 0.9f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            // Description
            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(go.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = card.description ?? "";
            desc.fontSize = 12;
            desc.alignment = TextAlignmentOptions.Center;
            desc.color = Color.white;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.1f);
            descRt.anchorMax = new Vector2(1, 0.6f);
            descRt.offsetMin = new Vector2(5, 0);
            descRt.offsetMax = new Vector2(-5, 0);
        }

        private void CreateFallbackEquipmentButton(Transform parent, EquipmentDef equip, int index)
        {
            var go = new GameObject($"EquipReward_{index}");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 200);

            var img = go.AddComponent<Image>();
            img.color = GetEquipRarityColor(equip.rarity);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnEquipmentSelected(equip));

            // Name
            var textGo = new GameObject("Name");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = equip.displayName;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0.6f);
            textRt.anchorMax = new Vector2(1, 0.9f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            // Stats
            var descGo = new GameObject("Stats");
            descGo.transform.SetParent(go.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = FormatEquipStats(equip);
            desc.fontSize = 12;
            desc.alignment = TextAlignmentOptions.Center;
            desc.color = Color.white;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.1f);
            descRt.anchorMax = new Vector2(1, 0.6f);
            descRt.offsetMin = new Vector2(5, 0);
            descRt.offsetMax = new Vector2(-5, 0);
        }

        private string FormatEquipStats(EquipmentDef equip)
        {
            var parts = new List<string>();
            if (equip.bonusStats.maxHealth != 0) parts.Add($"HP: {equip.bonusStats.maxHealth:+#;-#;0}");
            if (equip.bonusStats.strength != 0) parts.Add($"STR: {equip.bonusStats.strength:+#;-#;0}");
            if (equip.bonusStats.mana != 0) parts.Add($"MANA: {equip.bonusStats.mana:+#;-#;0}");
            if (equip.bonusStats.engineering != 0) parts.Add($"ENG: {equip.bonusStats.engineering:+#;-#;0}");
            return string.Join("\n", parts);
        }

        private Color GetRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => new Color(0.4f, 0.4f, 0.4f),
                CardRarity.Uncommon => new Color(0.2f, 0.6f, 0.2f),
                CardRarity.Rare => new Color(0.2f, 0.4f, 0.8f),
                CardRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                CardRarity.Legendary => new Color(0.9f, 0.7f, 0.1f),
                _ => Color.gray
            };
        }

        private Color GetEquipRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => new Color(0.4f, 0.4f, 0.4f),
                EquipmentRarity.Uncommon => new Color(0.2f, 0.6f, 0.2f),
                EquipmentRarity.Rare => new Color(0.2f, 0.4f, 0.8f),
                EquipmentRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                EquipmentRarity.Legendary => new Color(0.9f, 0.7f, 0.1f),
                _ => Color.gray
            };
        }

        private void OnCardSelected(CardDef card)
        {
            if (card == null) return;

            Debug.Log($"[RewardScene] Player selected card: {card.displayName}");

            // Grant the card to the player
            if (cardDb != null)
            {
                cardDb.Grant(card.id, 1);
                Debug.Log($"[RewardScene] Granted 1 copy of {card.id} to player");
            }

            ReturnToMap();
        }

        private void OnEquipmentSelected(EquipmentDef equip)
        {
            if (equip == null) return;

            Debug.Log($"[RewardScene] Player selected equipment: {equip.displayName}");

            // Add to player's inventory
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr != null)
            {
                var instance = new EquipmentInstance(equip);
                equipMgr.AddToInventory(instance);
                Debug.Log($"[RewardScene] Added {equip.id} to inventory");
            }

            ReturnToMap();
        }

        private void OnSkipClicked()
        {
            Debug.Log("[RewardScene] Player skipped reward");
            ReturnToMap();
        }

        private void ReturnToMap()
        {
            // Clear pending reward flag
            if (MapSession.I != null)
            {
                MapSession.I.PendingReward = false;
                MapSession.I.IsEliteFight = false;
            }

            SceneManager.LoadScene("MapScene");
        }
    }
}
