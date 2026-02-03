using Game.Cards;
using Game.Equipment;

namespace Game.Shop
{
    /// <summary>
    /// Represents an item for sale in the shop.
    /// </summary>
    public class ShopItem
    {
        public enum ItemType { Card, Equipment, Repair }

        public ItemType Type { get; }
        public CardDef Card { get; }
        public EquipmentDef Equipment { get; }
        public int Price { get; }
        public bool IsSold { get; set; }

        public string DisplayName => Type switch
        {
            ItemType.Card => Card?.displayName ?? "Unknown Card",
            ItemType.Equipment => Equipment?.displayName ?? "Unknown Equipment",
            ItemType.Repair => "Repair All Equipment",
            _ => "Unknown"
        };

        public string Description => Type switch
        {
            ItemType.Card => Card?.description ?? "",
            ItemType.Equipment => Equipment?.description ?? "",
            ItemType.Repair => "Fully repair all equipped items.",
            _ => ""
        };

        private ShopItem(ItemType type, int price)
        {
            Type = type;
            Price = price;
            IsSold = false;
        }

        public static ShopItem ForCard(CardDef card, int price)
        {
            return new ShopItem(ItemType.Card, price) { Card = card };
        }

        public static ShopItem ForEquipment(EquipmentDef equip, int price)
        {
            return new ShopItem(ItemType.Equipment, price) { Equipment = equip };
        }

        public static ShopItem ForRepair(int price)
        {
            return new ShopItem(ItemType.Repair, price);
        }

        // Price calculation helpers
        public static int GetCardPrice(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => 50,
                CardRarity.Uncommon => 100,
                CardRarity.Rare => 150,
                CardRarity.Epic => 250,
                CardRarity.Legendary => 400,
                _ => 75
            };
        }

        public static int GetEquipmentPrice(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => 75,
                EquipmentRarity.Uncommon => 125,
                EquipmentRarity.Rare => 200,
                EquipmentRarity.Epic => 325,
                EquipmentRarity.Legendary => 500,
                _ => 100
            };
        }
    }
}
