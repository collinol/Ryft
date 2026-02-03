// Assets/Scripts/Cards/CardDef.cs
using UnityEngine;

namespace Game.Cards
{
    public enum TargetingType { None, Self, SingleEnemy, AllEnemies }
    public enum CardRarity   { Common, Uncommon, Rare, Epic, Legendary }

    [CreateAssetMenu(menuName = "Game/Card", fileName = "Card_")]
    public class CardDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Gameplay")]
        public TargetingType targeting = TargetingType.SingleEnemy;

        [Header("Numbers")]
        public int energyCost = 1;
        public int power   = 5;
        public int scaling = 1;

        [Header("Runtime Class")]
        public string runtimeTypeName; // e.g., "Game.Cards.StrikeCard"

        [Header("Meta")]
        public CardRarity rarity = CardRarity.Common;
    }
}
