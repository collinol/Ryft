using Game.Core;

namespace Game.Cards
{
    public class HealthInjection : HealCard
    {
        protected override int GetBasePower()  => 8;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Engineering;
    }
}