using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Equipment;

namespace Game.Rewards
{
    /// <summary>
    /// UI component for displaying a single reward choice (card or equipment).
    /// </summary>
    public class RewardItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Button selectButton;

        private CardDef cardDef;
        private EquipmentDef equipDef;
        private System.Action<CardDef> onCardSelected;
        private System.Action<EquipmentDef> onEquipSelected;

        public void SetupCard(CardDef card, System.Action<CardDef> callback)
        {
            cardDef = card;
            onCardSelected = callback;

            if (nameText) nameText.text = card.displayName;
            if (descriptionText) descriptionText.text = card.description ?? "";
            if (iconImage && card.icon) iconImage.sprite = card.icon;
            if (backgroundImage) backgroundImage.color = GetCardRarityColor(card.rarity);
            if (rarityText) rarityText.text = card.rarity.ToString();
            if (statsText) statsText.text = FormatCardCost(card);

            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClicked);
            }
        }

        public void SetupEquipment(EquipmentDef equip, System.Action<EquipmentDef> callback)
        {
            equipDef = equip;
            onEquipSelected = callback;

            if (nameText) nameText.text = equip.displayName;
            if (descriptionText) descriptionText.text = equip.description ?? "";
            if (iconImage && equip.icon) iconImage.sprite = equip.icon;
            if (backgroundImage) backgroundImage.color = GetEquipRarityColor(equip.rarity);
            if (rarityText) rarityText.text = equip.rarity.ToString();
            if (statsText) statsText.text = FormatEquipStats(equip);

            if (selectButton)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            if (cardDef != null)
            {
                onCardSelected?.Invoke(cardDef);
            }
            else if (equipDef != null)
            {
                onEquipSelected?.Invoke(equipDef);
            }
        }

        private string FormatCardCost(CardDef card)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (card.cost.strength > 0) parts.Add($"STR: {card.cost.strength}");
            if (card.cost.mana > 0) parts.Add($"MANA: {card.cost.mana}");
            if (card.cost.engineering > 0) parts.Add($"ENG: {card.cost.engineering}");
            return parts.Count > 0 ? string.Join(" | ", parts) : "Free";
        }

        private string FormatEquipStats(EquipmentDef equip)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (equip.bonusStats.maxHealth != 0) parts.Add($"HP {equip.bonusStats.maxHealth:+#;-#;0}");
            if (equip.bonusStats.strength != 0) parts.Add($"STR {equip.bonusStats.strength:+#;-#;0}");
            if (equip.bonusStats.mana != 0) parts.Add($"MANA {equip.bonusStats.mana:+#;-#;0}");
            if (equip.bonusStats.engineering != 0) parts.Add($"ENG {equip.bonusStats.engineering:+#;-#;0}");
            return string.Join(" | ", parts);
        }

        private Color GetCardRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => new Color(0.5f, 0.5f, 0.5f, 0.8f),
                CardRarity.Uncommon => new Color(0.2f, 0.7f, 0.2f, 0.8f),
                CardRarity.Rare => new Color(0.2f, 0.4f, 0.9f, 0.8f),
                CardRarity.Epic => new Color(0.7f, 0.2f, 0.9f, 0.8f),
                CardRarity.Legendary => new Color(1f, 0.8f, 0.1f, 0.8f),
                _ => new Color(0.4f, 0.4f, 0.4f, 0.8f)
            };
        }

        private Color GetEquipRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => new Color(0.5f, 0.5f, 0.5f, 0.8f),
                EquipmentRarity.Uncommon => new Color(0.2f, 0.7f, 0.2f, 0.8f),
                EquipmentRarity.Rare => new Color(0.2f, 0.4f, 0.9f, 0.8f),
                EquipmentRarity.Epic => new Color(0.7f, 0.2f, 0.9f, 0.8f),
                EquipmentRarity.Legendary => new Color(1f, 0.8f, 0.1f, 0.8f),
                _ => new Color(0.4f, 0.4f, 0.4f, 0.8f)
            };
        }
    }
}
