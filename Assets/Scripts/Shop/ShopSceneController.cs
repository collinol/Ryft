using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Equipment;

namespace Game.Shop
{
    /// <summary>
    /// Controls the shop scene - displays items for sale and handles purchases.
    /// </summary>
    public class ShopSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button repairButton;
        [SerializeField] private TextMeshProUGUI repairCostText;

        [Header("Shop Settings")]
        [SerializeField] private int cardCount = 4;
        [SerializeField] private int equipmentCount = 2;

        private List<ShopItem> shopItems = new();
        private CardDatabase cardDb;
        private EquipmentDatabase equipDb;

        void Start()
        {
            cardDb = CardDatabase.Load();
            equipDb = EquipmentDatabase.Load();

            GenerateShopInventory();
            CreateShopUI();
            UpdateGoldDisplay();
            UpdateRepairCost();

            if (leaveButton)
            {
                leaveButton.onClick.AddListener(OnLeaveClicked);
            }

            if (repairButton)
            {
                repairButton.onClick.AddListener(OnRepairClicked);
            }
        }

        private void GenerateShopInventory()
        {
            shopItems.Clear();
            var rng = new System.Random();

            // Generate card items
            if (cardDb != null)
            {
                for (int i = 0; i < cardCount; i++)
                {
                    var card = cardDb.RollReward(rng);
                    if (card != null)
                    {
                        int price = ShopItem.GetCardPrice(card.rarity);
                        shopItems.Add(ShopItem.ForCard(card, price));
                    }
                }
            }

            // Generate equipment items
            if (equipDb != null)
            {
                var equips = equipDb.RollRewards(equipmentCount, rng);
                foreach (var equip in equips)
                {
                    int price = ShopItem.GetEquipmentPrice(equip.rarity);
                    shopItems.Add(ShopItem.ForEquipment(equip, price));
                }
            }

            Debug.Log($"[Shop] Generated {shopItems.Count} items for sale");
        }

        private void CreateShopUI()
        {
            if (itemContainer == null)
            {
                CreateFallbackUI();
                return;
            }

            if (shopItemPrefab == null)
            {
                CreateFallbackUI();
                return;
            }

            foreach (var item in shopItems)
            {
                var go = Instantiate(shopItemPrefab, itemContainer);
                var ui = go.GetComponent<ShopItemUI>();
                if (ui != null)
                {
                    ui.Setup(item, OnItemPurchased);
                }
            }
        }

        private void CreateFallbackUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var canvasGo = new GameObject("ShopCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "SHOP";
            title.fontSize = 48;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.9f);
            titleRt.anchorMax = new Vector2(0.5f, 0.95f);
            titleRt.sizeDelta = new Vector2(400, 60);

            // Gold display
            var goldGo = new GameObject("Gold");
            goldGo.transform.SetParent(canvas.transform, false);
            goldText = goldGo.AddComponent<TextMeshProUGUI>();
            goldText.fontSize = 32;
            goldText.alignment = TextAlignmentOptions.Right;
            goldText.color = new Color(1f, 0.85f, 0.1f);
            var goldRt = goldGo.GetComponent<RectTransform>();
            goldRt.anchorMin = new Vector2(0.7f, 0.9f);
            goldRt.anchorMax = new Vector2(0.95f, 0.95f);
            goldRt.sizeDelta = Vector2.zero;

            // Item container
            var container = new GameObject("ItemContainer");
            container.transform.SetParent(canvas.transform, false);
            var glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(180, 250);
            glg.spacing = new Vector2(20, 20);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            var containerRt = container.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.1f, 0.25f);
            containerRt.anchorMax = new Vector2(0.9f, 0.85f);
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;

            // Create item buttons
            for (int i = 0; i < shopItems.Count; i++)
            {
                CreateFallbackItemButton(container.transform, shopItems[i], i);
            }

            // Repair button
            var repairGo = new GameObject("RepairButton");
            repairGo.transform.SetParent(canvas.transform, false);
            var repairRt = repairGo.AddComponent<RectTransform>();
            repairRt.anchorMin = new Vector2(0.3f, 0.1f);
            repairRt.anchorMax = new Vector2(0.5f, 0.18f);
            repairRt.sizeDelta = Vector2.zero;
            repairRt.offsetMin = Vector2.zero;
            repairRt.offsetMax = Vector2.zero;
            var repairImg = repairGo.AddComponent<Image>();
            repairImg.color = new Color(0.3f, 0.5f, 0.3f);
            repairButton = repairGo.AddComponent<Button>();
            repairButton.onClick.AddListener(OnRepairClicked);

            var repairTextGo = new GameObject("Text");
            repairTextGo.transform.SetParent(repairGo.transform, false);
            repairCostText = repairTextGo.AddComponent<TextMeshProUGUI>();
            repairCostText.text = "Repair All";
            repairCostText.fontSize = 20;
            repairCostText.alignment = TextAlignmentOptions.Center;
            repairCostText.color = Color.white;
            var repairTextRt = repairTextGo.GetComponent<RectTransform>();
            repairTextRt.anchorMin = Vector2.zero;
            repairTextRt.anchorMax = Vector2.one;

            // Leave button
            var leaveGo = new GameObject("LeaveButton");
            leaveGo.transform.SetParent(canvas.transform, false);
            var leaveRt = leaveGo.AddComponent<RectTransform>();
            leaveRt.anchorMin = new Vector2(0.5f, 0.1f);
            leaveRt.anchorMax = new Vector2(0.7f, 0.18f);
            leaveRt.sizeDelta = Vector2.zero;
            leaveRt.offsetMin = Vector2.zero;
            leaveRt.offsetMax = Vector2.zero;
            var leaveImg = leaveGo.AddComponent<Image>();
            leaveImg.color = new Color(0.5f, 0.3f, 0.3f);
            leaveButton = leaveGo.AddComponent<Button>();
            leaveButton.onClick.AddListener(OnLeaveClicked);

            var leaveTextGo = new GameObject("Text");
            leaveTextGo.transform.SetParent(leaveGo.transform, false);
            var leaveText = leaveTextGo.AddComponent<TextMeshProUGUI>();
            leaveText.text = "Leave Shop";
            leaveText.fontSize = 24;
            leaveText.alignment = TextAlignmentOptions.Center;
            leaveText.color = Color.white;
            var leaveTextRt = leaveTextGo.GetComponent<RectTransform>();
            leaveTextRt.anchorMin = Vector2.zero;
            leaveTextRt.anchorMax = Vector2.one;

            UpdateGoldDisplay();
            UpdateRepairCost();
        }

        private void CreateFallbackItemButton(Transform parent, ShopItem item, int index)
        {
            var go = new GameObject($"ShopItem_{index}");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 250);

            var img = go.AddComponent<Image>();
            img.color = item.Type == ShopItem.ItemType.Card
                ? GetCardRarityColor(item.Card?.rarity ?? CardRarity.Common)
                : GetEquipRarityColor(item.Equipment?.rarity ?? EquipmentRarity.Common);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnItemPurchased(item));

            // Name
            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(go.transform, false);
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = item.DisplayName;
            nameText.fontSize = 16;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.7f);
            nameRt.anchorMax = new Vector2(1, 0.95f);
            nameRt.offsetMin = new Vector2(5, 0);
            nameRt.offsetMax = new Vector2(-5, 0);

            // Type badge
            var typeGo = new GameObject("Type");
            typeGo.transform.SetParent(go.transform, false);
            var typeText = typeGo.AddComponent<TextMeshProUGUI>();
            typeText.text = item.Type.ToString().ToUpper();
            typeText.fontSize = 12;
            typeText.alignment = TextAlignmentOptions.Center;
            typeText.color = new Color(1f, 1f, 1f, 0.7f);
            var typeRt = typeGo.GetComponent<RectTransform>();
            typeRt.anchorMin = new Vector2(0, 0.55f);
            typeRt.anchorMax = new Vector2(1, 0.7f);
            typeRt.offsetMin = new Vector2(5, 0);
            typeRt.offsetMax = new Vector2(-5, 0);

            // Description
            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(go.transform, false);
            var descText = descGo.AddComponent<TextMeshProUGUI>();
            descText.text = item.Description;
            descText.fontSize = 11;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.9f, 0.9f, 0.9f);
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.2f);
            descRt.anchorMax = new Vector2(1, 0.55f);
            descRt.offsetMin = new Vector2(5, 0);
            descRt.offsetMax = new Vector2(-5, 0);

            // Price
            var priceGo = new GameObject("Price");
            priceGo.transform.SetParent(go.transform, false);
            var priceText = priceGo.AddComponent<TextMeshProUGUI>();
            priceText.text = $"{item.Price} Gold";
            priceText.fontSize = 18;
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.color = new Color(1f, 0.85f, 0.1f);
            var priceRt = priceGo.GetComponent<RectTransform>();
            priceRt.anchorMin = new Vector2(0, 0.02f);
            priceRt.anchorMax = new Vector2(1, 0.18f);
            priceRt.offsetMin = new Vector2(5, 0);
            priceRt.offsetMax = new Vector2(-5, 0);

            // Store reference for updating sold state
            var itemUI = go.AddComponent<ShopItemUI>();
            itemUI.Setup(item, OnItemPurchased);
        }

        private void OnItemPurchased(ShopItem item)
        {
            if (item == null || item.IsSold) return;

            if (MapSession.I == null || MapSession.I.Gold < item.Price)
            {
                Debug.Log($"[Shop] Not enough gold! Have: {MapSession.I?.Gold ?? 0}, Need: {item.Price}");
                return;
            }

            // Spend gold
            MapSession.I.SpendGold(item.Price);

            // Grant item
            switch (item.Type)
            {
                case ShopItem.ItemType.Card:
                    if (cardDb != null && item.Card != null)
                    {
                        cardDb.Grant(item.Card.id, 1);
                        Debug.Log($"[Shop] Purchased card: {item.Card.displayName}");
                    }
                    break;

                case ShopItem.ItemType.Equipment:
                    var equipMgr = EquipmentManager.Instance;
                    if (equipMgr != null && item.Equipment != null)
                    {
                        var instance = new EquipmentInstance(item.Equipment);
                        equipMgr.AddToInventory(instance);
                        Debug.Log($"[Shop] Purchased equipment: {item.Equipment.displayName}");
                    }
                    break;
            }

            item.IsSold = true;
            UpdateGoldDisplay();
            RefreshShopUI();
        }

        private void OnRepairClicked()
        {
            var equipMgr = EquipmentManager.Instance;
            if (equipMgr == null) return;

            int cost = RepairService.GetTotalRepairCost(equipMgr);
            if (cost <= 0)
            {
                Debug.Log("[Shop] No items need repair");
                return;
            }

            if (MapSession.I == null || MapSession.I.Gold < cost)
            {
                Debug.Log($"[Shop] Not enough gold for repairs! Have: {MapSession.I?.Gold ?? 0}, Need: {cost}");
                return;
            }

            MapSession.I.SpendGold(cost);
            RepairService.RepairAllEquipped(equipMgr);

            UpdateGoldDisplay();
            UpdateRepairCost();
            Debug.Log($"[Shop] Repaired all equipment for {cost} gold");
        }

        private void OnLeaveClicked()
        {
            Debug.Log("[Shop] Leaving shop, returning to map");
            SceneManager.LoadScene("MapScene");
        }

        private void UpdateGoldDisplay()
        {
            if (goldText != null && MapSession.I != null)
            {
                goldText.text = $"Gold: {MapSession.I.Gold}";
            }
        }

        private void UpdateRepairCost()
        {
            if (repairCostText == null) return;

            var equipMgr = EquipmentManager.Instance;
            int cost = equipMgr != null ? RepairService.GetTotalRepairCost(equipMgr) : 0;

            repairCostText.text = cost > 0 ? $"Repair All ({cost}g)" : "Repair (No damage)";

            if (repairButton != null)
            {
                repairButton.interactable = cost > 0 && MapSession.I != null && MapSession.I.Gold >= cost;
            }
        }

        private void RefreshShopUI()
        {
            // Update all shop item UIs to reflect sold status
            var itemUIs = FindObjectsOfType<ShopItemUI>();
            foreach (var ui in itemUIs)
            {
                ui.RefreshSoldState();
            }
        }

        private Color GetCardRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => new Color(0.4f, 0.4f, 0.4f),
                CardRarity.Uncommon => new Color(0.2f, 0.5f, 0.2f),
                CardRarity.Rare => new Color(0.2f, 0.3f, 0.7f),
                CardRarity.Epic => new Color(0.5f, 0.2f, 0.7f),
                CardRarity.Legendary => new Color(0.8f, 0.6f, 0.1f),
                _ => Color.gray
            };
        }

        private Color GetEquipRarityColor(EquipmentRarity rarity)
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
    }
}
