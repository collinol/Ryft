using Game.Core;

namespace Game.Cards
{
    public class HealSpellCard : HealCard
    {
        protected override StatField CostField => StatField.Mana;
        protected override int GetBaseCostAmount(StatField field) => 1;
    }
}