using UnityEngine;

namespace Game.Cards
{
    public enum TargetingType { None, Self, SingleEnemy, AllEnemies }
    public enum CardRarity   { Common, Uncommon, Rare, Epic, Legendary }
    public enum CardType     { Attack, Defend, Heal, Skill, Device }
    public enum CardStatType { None, Strength, Intellect, Engineering }
    public enum CardKeyword  { Momentum, Extract, Rewind }

    [CreateAssetMenu(menuName = "Game/Card", fileName = "Card_")]
    public class CardDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Classification")]
        public CardType cardType = CardType.Attack;
        public CardStatType statType = CardStatType.None;
        public CardKeyword[] keywords;

        [Header("Gameplay")]
        public TargetingType targeting = TargetingType.SingleEnemy;

        [Header("Numbers")]
        public int energyCost = 1;
        public int power   = 5;
        public int scaling = 1;

        [Header("Runtime Class")]
        public string runtimeTypeName; // e.g., "Game.Cards.Fighter.HeavyStrikeCard"

        [Header("Meta")]
        public CardRarity rarity = CardRarity.Common;

        public bool HasKeyword(CardKeyword kw)
        {
            if (keywords == null) return false;
            for (int i = 0; i < keywords.Length; i++)
                if (keywords[i] == kw) return true;
            return false;
        }
    }
}
