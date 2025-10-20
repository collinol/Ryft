using Game.Core;

namespace Game.Cards
{
    public class MedKitCard : HealCard
    {
        protected override StatField CostField => StatField.Strength;
        protected override int GetBaseCostAmount(StatField field) => 1;
        protected override int GetBasePower()  => 5;
        protected override int GetScaling()    => 1;
    }
}