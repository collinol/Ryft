using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Equipment;
using Game.Combat;

namespace Game.TimePortal
{
    /// <summary>
    /// Controls the Time Portal encounter scene.
    /// Allows player to borrow equipment from their future self.
    /// </summary>
    public class TimePortalSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI obligationsText;
        [SerializeField] private Transform gearContainer;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        [Header("Settings")]
        [SerializeField] private int futureLevelOffset = 3;

        private EquipmentDef offeredGear;  // Single gear piece offered
        private EquipmentDatabase equipDb;
        private int currentLevel;
        private string requiredEliteType;

        void Start()
        {
            equipDb = EquipmentDatabase.Load();
            currentLevel = MapSession.I?.CurrentMapLevel ?? 0;

            // Check for pending obligations first
            CheckPendingObligations();

            // Generate gear offers
            GenerateGearOffers();
            CreateUI();
        }

        private void CheckPendingObligations()
        {
            var state = GetOrCreateTimePortalState();

            // Mark this portal visit
            state.OnTimePortalVisited(currentLevel);

            // Check for expired gear
            var expired = state.CheckExpiredGear(currentLevel);
            foreach (var equipId in expired)
            {
                RemoveExpiredGearFromPlayer(equipId);
            }
        }

        private void RemoveExpiredGearFromPlayer(string equipId)
        {
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null) return;

            // Check all slots for the equipment
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None) continue;

                var equipped = equipMgr.GetEquipped(slot);
                if (equipped != null && equipped.def != null && equipped.def.id == equipId)
                {
                    equipMgr.Unequip(slot);
                    equipMgr.RemoveFromInventory(equipped);
                    Debug.Log($"[TimePortal] Removed expired gear: {equipId}");
                    return;
                }
            }

            // Also check inventory
            var inventory = equipMgr.Inventory;
            for (int i = inventory.Count - 1; i >= 0; i--)
            {
                if (inventory[i]?.def?.id == equipId)
                {
                    equipMgr.RemoveFromInventory(inventory[i]);
                    Debug.Log($"[TimePortal] Removed expired gear from inventory: {equipId}");
                    return;
                }
            }
        }

        private void GenerateGearOffers()
        {
            offeredGear = null;
            if (equipDb == null)
            {
                Debug.LogError("[TimePortal] EquipmentDatabase is null!");
                return;
            }

            var rng = new System.Random();

            // Always offer level 2 gear (gear from the "future")
            int targetLevel = 2;

            Debug.Log($"[TimePortal] CurrentMapLevel={currentLevel}, looking for level {targetLevel} gear");
            Debug.Log($"[TimePortal] Equipment database has {equipDb.Count} items total");

            // Debug: list all equipment and their levels
            foreach (var item in equipDb.All)
            {
                if (item != null)
                    Debug.Log($"[TimePortal] Equipment: {item.displayName}, level={item.level}");
            }

            // Count how many level 2 items exist
            var level2Items = equipDb.All.Where(e => e != null && e.level == 2).ToList();
            Debug.Log($"[TimePortal] Found {level2Items.Count} items at level 2");

            offeredGear = equipDb.RollRewardForLevel(targetLevel, rng);

            // Pick a random elite type for the obligation
            var enemyDb = Game.Enemies.EnemyDatabase.Load();
            if (enemyDb != null)
            {
                var eliteDef = enemyDb.GetRandom(Game.Enemies.EnemyTier.Elite, 0);
                requiredEliteType = eliteDef != null ? eliteDef.id : "DarkKnight";
            }
            else
            {
                requiredEliteType = "DarkKnight";
            }

            if (offeredGear != null)
            {
                Debug.Log($"[TimePortal] Offering: {offeredGear.displayName} (level={offeredGear.level}). Required elite: {requiredEliteType}");
            }
            else
            {
                Debug.LogWarning($"[TimePortal] No level {targetLevel} gear found! Check that equipment assets have level field set to 2.");
            }
        }

        private void CreateUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var canvasGo = new GameObject("TimePortalCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Background with mystical appearance
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.05f, 0.2f); // Dark purple
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "TIME PORTAL";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.7f, 0.5f, 1f);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.2f, 0.85f);
            titleRt.anchorMax = new Vector2(0.8f, 0.95f);

            // Description
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(canvas.transform, false);
            descriptionText = descGo.AddComponent<TextMeshProUGUI>();

            if (offeredGear != null)
            {
                descriptionText.text = "Your future self reaches through time...\n\"I bring you this gear from level " + (currentLevel + 1) + ". Accept it, but you will need to make the same choices as I have if you wish to keep it.\"";
            }
            else
            {
                descriptionText.text = "Your future self reaches through time...\n\"I have nothing to offer you yet. Come back when the path grows deeper.\"";
            }
            descriptionText.fontSize = 20;
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.color = new Color(0.8f, 0.8f, 1f);
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.15f, 0.7f);
            descRt.anchorMax = new Vector2(0.85f, 0.85f);

            // Gear container (center single item)
            var containerGo = new GameObject("GearContainer");
            containerGo.transform.SetParent(canvas.transform, false);
            var containerRt = containerGo.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.35f, 0.35f);
            containerRt.anchorMax = new Vector2(0.65f, 0.65f);
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;
            gearContainer = containerGo.transform;

            // Create single gear display (not a choice, just showing what's offered)
            if (offeredGear != null)
            {
                CreateGearDisplay(gearContainer, offeredGear);
            }
            else
            {
                // No gear available - show empty state
                var emptyGo = new GameObject("NoGear");
                emptyGo.transform.SetParent(gearContainer, false);
                var emptyRt = emptyGo.AddComponent<RectTransform>();
                emptyRt.anchorMin = Vector2.zero;
                emptyRt.anchorMax = Vector2.one;
                var emptyText = emptyGo.AddComponent<TextMeshProUGUI>();
                emptyText.text = "No gear available\nat this level";
                emptyText.fontSize = 24;
                emptyText.alignment = TextAlignmentOptions.Center;
                emptyText.color = new Color(0.5f, 0.5f, 0.6f);
            }

            // Obligations text
            var oblGo = new GameObject("Obligations");
            oblGo.transform.SetParent(canvas.transform, false);
            obligationsText = oblGo.AddComponent<TextMeshProUGUI>();
            UpdateObligationsText();
            obligationsText.fontSize = 16;
            obligationsText.alignment = TextAlignmentOptions.Center;
            obligationsText.color = new Color(1f, 0.8f, 0.5f);
            var oblRt = oblGo.GetComponent<RectTransform>();
            oblRt.anchorMin = new Vector2(0.2f, 0.2f);
            oblRt.anchorMax = new Vector2(0.8f, 0.32f);

            // Accept button
            var acceptGo = new GameObject("AcceptButton");
            acceptGo.transform.SetParent(canvas.transform, false);
            var acceptRt = acceptGo.AddComponent<RectTransform>();
            acceptRt.anchorMin = new Vector2(0.3f, 0.08f);
            acceptRt.anchorMax = new Vector2(0.5f, 0.18f);
            var acceptImg = acceptGo.AddComponent<Image>();
            acceptImg.color = new Color(0.3f, 0.2f, 0.5f);
            acceptButton = acceptGo.AddComponent<Button>();
            acceptButton.onClick.AddListener(OnAcceptClicked);
            acceptButton.interactable = offeredGear != null;  // Enable if gear is available

            var acceptTextGo = new GameObject("Text");
            acceptTextGo.transform.SetParent(acceptGo.transform, false);
            var acceptText = acceptTextGo.AddComponent<TextMeshProUGUI>();
            acceptText.text = "Accept Bargain";
            acceptText.fontSize = 20;
            acceptText.alignment = TextAlignmentOptions.Center;
            acceptText.color = Color.white;
            var acceptTextRt = acceptTextGo.GetComponent<RectTransform>();
            acceptTextRt.anchorMin = Vector2.zero;
            acceptTextRt.anchorMax = Vector2.one;

            // Decline button
            var declineGo = new GameObject("DeclineButton");
            declineGo.transform.SetParent(canvas.transform, false);
            var declineRt = declineGo.AddComponent<RectTransform>();
            declineRt.anchorMin = new Vector2(0.5f, 0.08f);
            declineRt.anchorMax = new Vector2(0.7f, 0.18f);
            var declineImg = declineGo.AddComponent<Image>();
            declineImg.color = new Color(0.4f, 0.3f, 0.3f);
            declineButton = declineGo.AddComponent<Button>();
            declineButton.onClick.AddListener(OnDeclineClicked);

            var declineTextGo = new GameObject("Text");
            declineTextGo.transform.SetParent(declineGo.transform, false);
            var declineText = declineTextGo.AddComponent<TextMeshProUGUI>();
            declineText.text = "Walk Away";
            declineText.fontSize = 20;
            declineText.alignment = TextAlignmentOptions.Center;
            declineText.color = Color.white;
            var declineTextRt = declineTextGo.GetComponent<RectTransform>();
            declineTextRt.anchorMin = Vector2.zero;
            declineTextRt.anchorMax = Vector2.one;
        }

        private void CreateGearDisplay(Transform parent, EquipmentDef equip)
        {
            var go = new GameObject("OfferedGear");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            var img = go.AddComponent<Image>();
            img.color = GetRarityColor(equip.rarity);

            // Name
            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(go.transform, false);
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = equip.displayName;
            nameText.fontSize = 22;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.75f);
            nameRt.anchorMax = new Vector2(1, 0.95f);
            nameRt.offsetMin = new Vector2(10, 0);
            nameRt.offsetMax = new Vector2(-10, 0);

            // Level indicator
            var levelGo = new GameObject("Level");
            levelGo.transform.SetParent(go.transform, false);
            var levelText = levelGo.AddComponent<TextMeshProUGUI>();
            levelText.text = $"Level {equip.level} Gear";
            levelText.fontSize = 14;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = new Color(1f, 0.9f, 0.5f);
            var levelRt = levelGo.GetComponent<RectTransform>();
            levelRt.anchorMin = new Vector2(0, 0.65f);
            levelRt.anchorMax = new Vector2(1, 0.75f);
            levelRt.offsetMin = new Vector2(10, 0);
            levelRt.offsetMax = new Vector2(-10, 0);

            // Slot
            var slotGo = new GameObject("Slot");
            slotGo.transform.SetParent(go.transform, false);
            var slotText = slotGo.AddComponent<TextMeshProUGUI>();
            slotText.text = $"[{equip.slot}]";
            slotText.fontSize = 14;
            slotText.alignment = TextAlignmentOptions.Center;
            slotText.color = new Color(0.7f, 0.7f, 0.8f);
            var slotRt = slotGo.GetComponent<RectTransform>();
            slotRt.anchorMin = new Vector2(0, 0.55f);
            slotRt.anchorMax = new Vector2(1, 0.65f);
            slotRt.offsetMin = new Vector2(10, 0);
            slotRt.offsetMax = new Vector2(-10, 0);

            // Stats
            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(go.transform, false);
            var statsText = statsGo.AddComponent<TextMeshProUGUI>();
            statsText.text = FormatStats(equip);
            statsText.fontSize = 16;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.color = new Color(0.9f, 0.9f, 0.9f);
            var statsRt = statsGo.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0, 0.2f);
            statsRt.anchorMax = new Vector2(1, 0.55f);
            statsRt.offsetMin = new Vector2(10, 0);
            statsRt.offsetMax = new Vector2(-10, 0);

            // Rarity
            var rarityGo = new GameObject("Rarity");
            rarityGo.transform.SetParent(go.transform, false);
            var rarityText = rarityGo.AddComponent<TextMeshProUGUI>();
            rarityText.text = equip.rarity.ToString();
            rarityText.fontSize = 16;
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.color = GetRarityTextColor(equip.rarity);
            rarityText.fontStyle = FontStyles.Italic;
            var rarityRt = rarityGo.GetComponent<RectTransform>();
            rarityRt.anchorMin = new Vector2(0, 0.05f);
            rarityRt.anchorMax = new Vector2(1, 0.18f);
            rarityRt.offsetMin = new Vector2(10, 0);
            rarityRt.offsetMax = new Vector2(-10, 0);
        }

        private void UpdateObligationsText()
        {
            if (obligationsText == null) return;

            if (offeredGear == null)
            {
                obligationsText.text = "";
            }
            else
            {
                // Defeat level matches the gear level (e.g., level 2 gear requires defeating elite at level 2)
                int defeatLevel = offeredGear.level;
                int returnLevel = offeredGear.level + 1;

                obligationsText.text = $"OBLIGATIONS:\n" +
                    $"1. Defeat {requiredEliteType} at level {defeatLevel}\n" +
                    $"2. Return to a Time Portal at level {returnLevel}\n" +
                    $"\nFail these, and the gear vanishes!";
            }
        }

        private void OnAcceptClicked()
        {
            if (offeredGear == null) return;

            // Add gear to player inventory
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr != null)
            {
                var instance = new EquipmentInstance(offeredGear);
                equipMgr.AddToInventory(instance);
                Debug.Log($"[TimePortal] Added borrowed gear: {offeredGear.displayName}");
            }

            // Track borrowed gear and obligations
            var state = GetOrCreateTimePortalState();
            state.BorrowGear(offeredGear.id, currentLevel, requiredEliteType);

            Debug.Log($"[TimePortal] Accepted bargain for {offeredGear.displayName}");

            ReturnToMap();
        }

        private void OnDeclineClicked()
        {
            Debug.Log("[TimePortal] Declined the bargain");
            ReturnToMap();
        }

        private void ReturnToMap()
        {
            SceneManager.LoadScene("MapScene");
        }

        private TimePortalState GetOrCreateTimePortalState()
        {
            if (MapSession.I == null)
            {
                new GameObject("MapSession (auto)").AddComponent<MapSession>();
            }

            if (MapSession.I.TimePortal == null)
            {
                MapSession.I.TimePortal = new TimePortalState();
            }

            return MapSession.I.TimePortal;
        }

        private string FormatStats(EquipmentDef equip)
        {
            var parts = new List<string>();
            if (equip.bonusStats.maxHealth != 0) parts.Add($"HP: {equip.bonusStats.maxHealth:+#;-#;0}");
            if (equip.bonusStats.strength != 0) parts.Add($"STR: {equip.bonusStats.strength:+#;-#;0}");
            if (equip.bonusStats.mana != 0) parts.Add($"MANA: {equip.bonusStats.mana:+#;-#;0}");
            if (equip.bonusStats.engineering != 0) parts.Add($"ENG: {equip.bonusStats.engineering:+#;-#;0}");
            return string.Join("\n", parts);
        }

        private Color GetRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => new Color(0.4f, 0.4f, 0.4f),
                EquipmentRarity.Uncommon => new Color(0.2f, 0.5f, 0.2f),
                EquipmentRarity.Rare => new Color(0.2f, 0.3f, 0.7f),
                EquipmentRarity.Epic => new Color(0.5f, 0.2f, 0.7f),
                EquipmentRarity.Legendary => new Color(0.8f, 0.6f, 0.1f),
                _ => Color.gray
            };
        }

        private Color GetRarityTextColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => Color.gray,
                EquipmentRarity.Uncommon => Color.green,
                EquipmentRarity.Rare => Color.blue,
                EquipmentRarity.Epic => new Color(0.8f, 0.3f, 1f),
                EquipmentRarity.Legendary => new Color(1f, 0.8f, 0.1f),
                _ => Color.white
            };
        }
    }
}
