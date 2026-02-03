using Game.Core;

namespace Game.Cards
{
    public class MedKit : HealCard
    {
        protected override int GetBasePower()  => 5;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Strength;
    }
}