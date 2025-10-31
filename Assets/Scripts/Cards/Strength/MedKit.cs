using Game.Core;

namespace Game.Cards
{
    public class MedKit : HealCard
    {
        protected override int GetEnergyCost() => 2;
        protected override int GetBasePower()  => 5;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Strength;
    }
}