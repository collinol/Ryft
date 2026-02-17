using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Equipment;
using Game.TimePortal;

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
        private bool isObligationElite;
        private string obligationEquipId;       // The borrowed item ID player must select
        private bool selectedObligationItem;    // Did the player select the borrowed item?
        private CardDatabase cardDb;
        private EquipmentDatabase equipDb;

        void Start()
        {
            cardDb = CardDatabase.Load();
            equipDb = EquipmentDatabase.Load();

            // Determine reward type
            isEliteReward = MapSession.I != null && MapSession.I.IsEliteFight;
            isObligationElite = MapSession.I != null && MapSession.I.IsObligationEliteFight;

            // Find the borrowed item if this is an obligation elite
            if (isObligationElite && MapSession.I?.TimePortal != null)
            {
                // The obligation elite on WorldLevel N is for the borrow from WorldLevel N-1
                int prevWorld = MapSession.I.WorldLevel - 1;
                var borrowed = MapSession.I.TimePortal.GetBorrowedGearForWorld(prevWorld);
                if (borrowed != null)
                {
                    obligationEquipId = borrowed.equipmentId;
                    Debug.Log($"[RewardScene] Obligation elite: player must select {obligationEquipId} to close the loop");
                }
            }

            // Update title
            if (titleText)
            {
                titleText.text = isObligationElite
                    ? "Choose wisely..."
                    : isEliteReward
                        ? "Elite Defeated! Choose Equipment:"
                        : "Victory! Choose a Card:";
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

            if (isObligationElite && !string.IsNullOrEmpty(obligationEquipId))
            {
                // Obligation elite: one reward MUST be the borrowed item
                var borrowedDef = equipDb.Get(obligationEquipId);
                if (borrowedDef != null)
                {
                    // Roll the other rewards first (excluding the borrowed item)
                    equipmentRewards = equipDb.RollRewards(equipmentChoiceCount - 1, rng, EquipmentRarity.Uncommon);
                    equipmentRewards.Remove(borrowedDef); // Ensure no duplicate

                    // Insert the borrowed item at a random position
                    int insertIdx = rng.Next(0, equipmentRewards.Count + 1);
                    equipmentRewards.Insert(insertIdx, borrowedDef);
                    Debug.Log($"[RewardScene] Obligation elite rewards: inserted {borrowedDef.displayName} at index {insertIdx}");
                }
                else
                {
                    Debug.LogWarning($"[RewardScene] Borrowed item {obligationEquipId} not found in database, using normal rewards");
                    equipmentRewards = equipDb.RollRewards(equipmentChoiceCount, rng, EquipmentRarity.Uncommon);
                }
            }
            else
            {
                equipmentRewards = equipDb.RollRewards(equipmentChoiceCount, rng, EquipmentRarity.Uncommon);
            }

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
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = isObligationElite
                ? "ELITE DEFEATED!"
                : isEliteReward
                    ? "ELITE DEFEATED!\nChoose Equipment:"
                    : "VICTORY!\nChoose a Card:";
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = isEliteReward ? new Color(1f, 0.8f, 0.2f) : new Color(0.5f, 1f, 0.5f);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.2f, 0.8f);
            titleRt.anchorMax = new Vector2(0.8f, 0.95f);
            titleRt.sizeDelta = Vector2.zero;
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            // Gold display
            var goldGo = new GameObject("Gold");
            goldGo.transform.SetParent(canvas.transform, false);
            goldText = goldGo.AddComponent<TextMeshProUGUI>();
            goldText.fontSize = 28;
            goldText.alignment = TextAlignmentOptions.Right;
            goldText.color = new Color(1f, 0.85f, 0.1f);
            var goldRt = goldGo.GetComponent<RectTransform>();
            goldRt.anchorMin = new Vector2(0.7f, 0.9f);
            goldRt.anchorMax = new Vector2(0.95f, 0.98f);
            goldRt.sizeDelta = Vector2.zero;
            goldRt.offsetMin = Vector2.zero;
            goldRt.offsetMax = Vector2.zero;
            UpdateGoldDisplay();

            // Create container for reward cards
            var container = new GameObject("RewardContainer");
            container.transform.SetParent(canvas.transform, false);
            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var containerRt = container.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.1f, 0.25f);
            containerRt.anchorMax = new Vector2(0.9f, 0.75f);
            containerRt.sizeDelta = Vector2.zero;
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;

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
            skipRt.anchorMin = new Vector2(0.4f, 0.08f);
            skipRt.anchorMax = new Vector2(0.6f, 0.16f);
            skipRt.sizeDelta = Vector2.zero;
            skipRt.offsetMin = Vector2.zero;
            skipRt.offsetMax = Vector2.zero;
            var skipImg = skipGo.AddComponent<Image>();
            skipImg.color = new Color(0.4f, 0.35f, 0.35f);
            var skipBtn = skipGo.AddComponent<Button>();
            skipBtn.onClick.AddListener(OnSkipClicked);

            var skipTextGo = new GameObject("Text");
            skipTextGo.transform.SetParent(skipGo.transform, false);
            var skipText = skipTextGo.AddComponent<TextMeshProUGUI>();
            skipText.text = "Skip Reward";
            skipText.fontSize = 28;
            skipText.alignment = TextAlignmentOptions.Center;
            skipText.color = Color.white;
            var skipTextRt = skipTextGo.GetComponent<RectTransform>();
            skipTextRt.anchorMin = Vector2.zero;
            skipTextRt.anchorMax = Vector2.one;
            skipTextRt.sizeDelta = Vector2.zero;
            skipTextRt.offsetMin = Vector2.zero;
            skipTextRt.offsetMax = Vector2.zero;
        }

        private void CreateFallbackCardButton(Transform parent, CardDef card, int index)
        {
            var go = new GameObject($"CardReward_{index}");
            go.transform.SetParent(parent, false);

            // Set fixed size for the card
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 320);

            // Add LayoutElement to enforce size in layout group
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 220;
            le.preferredHeight = 320;
            le.minWidth = 220;
            le.minHeight = 320;

            var img = go.AddComponent<Image>();
            img.color = GetRarityColor(card.rarity);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardSelected(card));

            // Rarity label at top
            var rarityGo = new GameObject("Rarity");
            rarityGo.transform.SetParent(go.transform, false);
            var rarityText = rarityGo.AddComponent<TextMeshProUGUI>();
            rarityText.text = card.rarity.ToString().ToUpper();
            rarityText.fontSize = 16;
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.fontStyle = TMPro.FontStyles.Bold;
            rarityText.color = new Color(1f, 1f, 1f, 0.8f);
            var rarityRt = rarityGo.GetComponent<RectTransform>();
            rarityRt.anchorMin = new Vector2(0, 0.9f);
            rarityRt.anchorMax = new Vector2(1, 1f);
            rarityRt.offsetMin = new Vector2(5, 5);
            rarityRt.offsetMax = new Vector2(-5, -5);

            // Card name
            var textGo = new GameObject("Name");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = card.displayName;
            text.fontSize = 22;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableWordWrapping = true;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0.7f);
            textRt.anchorMax = new Vector2(1, 0.9f);
            textRt.offsetMin = new Vector2(10, 0);
            textRt.offsetMax = new Vector2(-10, 0);

            // Description
            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(go.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = card.description ?? "";
            desc.fontSize = 16;
            desc.alignment = TextAlignmentOptions.Center;
            desc.color = new Color(0.95f, 0.95f, 0.95f);
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Ellipsis;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.15f);
            descRt.anchorMax = new Vector2(1, 0.7f);
            descRt.offsetMin = new Vector2(10, 0);
            descRt.offsetMax = new Vector2(-10, 0);

            // Energy cost at bottom
            var costGo = new GameObject("Cost");
            costGo.transform.SetParent(go.transform, false);
            var costText = costGo.AddComponent<TextMeshProUGUI>();
            costText.text = card.energyCost > 0 ? $"Cost: {card.energyCost} Energy" : "Free";
            costText.fontSize = 18;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = new Color(0.9f, 0.8f, 0.3f);
            var costRt = costGo.GetComponent<RectTransform>();
            costRt.anchorMin = new Vector2(0, 0.02f);
            costRt.anchorMax = new Vector2(1, 0.12f);
            costRt.offsetMin = new Vector2(5, 0);
            costRt.offsetMax = new Vector2(-5, 0);
        }

        private void CreateFallbackEquipmentButton(Transform parent, EquipmentDef equip, int index)
        {
            bool isObligationItem = isObligationElite && equip.id == obligationEquipId;

            var go = new GameObject($"EquipReward_{index}");
            go.transform.SetParent(parent, false);

            // Set fixed size for the card
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 320);

            // Add LayoutElement to enforce size in layout group
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 220;
            le.preferredHeight = 320;
            le.minWidth = 220;
            le.minHeight = 320;

            var img = go.AddComponent<Image>();
            // Highlight the obligation item with a golden border/tint
            img.color = isObligationItem
                ? new Color(0.8f, 0.65f, 0.1f) // Gold for the time loop item
                : GetEquipRarityColor(equip.rarity);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnEquipmentSelected(equip));

            // Rarity / time loop label at top
            var rarityGo = new GameObject("Rarity");
            rarityGo.transform.SetParent(go.transform, false);
            var rarityText = rarityGo.AddComponent<TextMeshProUGUI>();
            rarityText.text = isObligationItem ? "TIME LOOP" : equip.rarity.ToString().ToUpper();
            rarityText.fontSize = 16;
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.fontStyle = TMPro.FontStyles.Bold;
            rarityText.color = isObligationItem ? new Color(1f, 0.9f, 0.3f) : new Color(1f, 1f, 1f, 0.8f);
            var rarityRt = rarityGo.GetComponent<RectTransform>();
            rarityRt.anchorMin = new Vector2(0, 0.9f);
            rarityRt.anchorMax = new Vector2(1, 1f);
            rarityRt.offsetMin = new Vector2(5, 5);
            rarityRt.offsetMax = new Vector2(-5, -5);

            // Slot type
            var slotGo = new GameObject("Slot");
            slotGo.transform.SetParent(go.transform, false);
            var slotText = slotGo.AddComponent<TextMeshProUGUI>();
            slotText.text = equip.slot.ToString();
            slotText.fontSize = 14;
            slotText.alignment = TextAlignmentOptions.Center;
            slotText.color = new Color(0.8f, 0.8f, 0.8f);
            var slotRt = slotGo.GetComponent<RectTransform>();
            slotRt.anchorMin = new Vector2(0, 0.82f);
            slotRt.anchorMax = new Vector2(1, 0.9f);
            slotRt.offsetMin = new Vector2(5, 0);
            slotRt.offsetMax = new Vector2(-5, 0);

            // Name
            var textGo = new GameObject("Name");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = equip.displayName;
            text.fontSize = 22;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableWordWrapping = true;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0.65f);
            textRt.anchorMax = new Vector2(1, 0.82f);
            textRt.offsetMin = new Vector2(10, 0);
            textRt.offsetMax = new Vector2(-10, 0);

            // Stats
            var descGo = new GameObject("Stats");
            descGo.transform.SetParent(go.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = FormatEquipStats(equip);
            desc.fontSize = 18;
            desc.alignment = TextAlignmentOptions.Center;
            desc.color = new Color(0.5f, 1f, 0.5f);
            desc.enableWordWrapping = true;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.25f);
            descRt.anchorMax = new Vector2(1, 0.65f);
            descRt.offsetMin = new Vector2(10, 0);
            descRt.offsetMax = new Vector2(-10, 0);

            // Description
            var descTextGo = new GameObject("Description");
            descTextGo.transform.SetParent(go.transform, false);
            var descText = descTextGo.AddComponent<TextMeshProUGUI>();
            descText.text = equip.description ?? "";
            descText.fontSize = 14;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.9f, 0.9f, 0.9f);
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            var descTextRt = descTextGo.GetComponent<RectTransform>();
            descTextRt.anchorMin = new Vector2(0, 0.05f);
            descTextRt.anchorMax = new Vector2(1, 0.25f);
            descTextRt.offsetMin = new Vector2(10, 0);
            descTextRt.offsetMax = new Vector2(-10, 0);
        }

        private string FormatEquipStats(EquipmentDef equip)
        {
            var parts = new List<string>();
            if (equip.bonusStats.maxHealth != 0) parts.Add($"HP: {equip.bonusStats.maxHealth:+#;-#;0}");
            if (equip.bonusStats.strength != 0) parts.Add($"STR: {equip.bonusStats.strength:+#;-#;0}");
            if (equip.bonusStats.intellect != 0) parts.Add($"INT: {equip.bonusStats.intellect:+#;-#;0}");
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

            // Check if this is the obligation item
            if (isObligationElite && !string.IsNullOrEmpty(obligationEquipId))
            {
                if (equip.id == obligationEquipId)
                {
                    // Player selected the borrowed item — loop closes!
                    // They already have it from the Envelope, so don't add a duplicate
                    selectedObligationItem = true;
                    Debug.Log($"[RewardScene] Player selected the obligation item {equip.id} — time loop closed!");
                }
                else
                {
                    // Player selected a different item — loop fails
                    selectedObligationItem = false;
                    Debug.Log($"[RewardScene] Player selected {equip.id} instead of obligation item {obligationEquipId} — loop will fail!");

                    // Add the selected (non-obligation) item to inventory
                    var equipMgr = EquipmentManager.Instance;
                    if (equipMgr != null)
                    {
                        var instance = new EquipmentInstance(equip);
                        equipMgr.AddToInventory(instance);
                        Debug.Log($"[RewardScene] Added {equip.id} to inventory");
                    }
                }
            }
            else
            {
                // Normal elite: just add to inventory
                var equipMgr = EquipmentManager.Instance;
                if (equipMgr != null)
                {
                    var instance = new EquipmentInstance(equip);
                    equipMgr.AddToInventory(instance);
                    Debug.Log($"[RewardScene] Added {equip.id} to inventory");
                }
            }

            ReturnToMap();
        }

        private void OnSkipClicked()
        {
            Debug.Log("[RewardScene] Player skipped reward");
            if (isObligationElite)
            {
                selectedObligationItem = false;
                Debug.Log("[RewardScene] Skipped obligation elite reward — loop will fail!");
            }
            ReturnToMap();
        }

        private void ReturnToMap()
        {
            // Clear pending reward flag
            if (MapSession.I != null)
            {
                // Handle obligation elite outcome
                if (isObligationElite)
                {
                    int prevWorld = MapSession.I.WorldLevel - 1;

                    if (selectedObligationItem)
                    {
                        // Loop closed successfully — mark elite defeated, portal will appear
                        MapSession.I.ObligationEliteDefeated = true;
                        Debug.Log("[RewardScene] Time loop closed! Obligation elite defeated, TimePortal will appear.");
                    }
                    else
                    {
                        // Loop failed — remove the borrowed gear from player
                        if (MapSession.I.TimePortal != null)
                        {
                            string removedId = MapSession.I.TimePortal.FailLoop(prevWorld);
                            if (!string.IsNullOrEmpty(removedId))
                            {
                                RemoveBorrowedGearFromPlayer(removedId);
                                Debug.Log($"[RewardScene] Time loop FAILED! Removed borrowed gear: {removedId}");
                            }
                        }
                        // Still mark elite defeated so the map doesn't keep showing it
                        MapSession.I.ObligationEliteDefeated = true;
                    }

                    MapSession.I.IsObligationEliteFight = false;
                }

                MapSession.I.PendingReward = false;
                MapSession.I.IsEliteFight = false;
            }

            SceneManager.LoadScene("MapScene");
        }

        /// <summary>
        /// Remove borrowed gear from player's equipment slots and inventory.
        /// </summary>
        private void RemoveBorrowedGearFromPlayer(string equipId)
        {
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null) return;

            // Check all equipped slots
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;
                var equipped = equipMgr.GetEquipped(slot);
                if (equipped != null && equipped.def != null && equipped.def.id == equipId)
                {
                    equipMgr.Unequip(slot);
                    equipMgr.RemoveFromInventory(equipped);
                    Debug.Log($"[RewardScene] Removed borrowed gear from slot {slot}: {equipId}");
                    return;
                }
            }

            // Check inventory
            var inventory = equipMgr.Inventory;
            for (int i = inventory.Count - 1; i >= 0; i--)
            {
                if (inventory[i]?.def?.id == equipId)
                {
                    equipMgr.RemoveFromInventory(inventory[i]);
                    Debug.Log($"[RewardScene] Removed borrowed gear from inventory: {equipId}");
                    return;
                }
            }
        }
    }
}
