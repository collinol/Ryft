using Game.Core;

namespace Game.Cards
{
    public class ReflectDamage : ReflectCard
    {
        protected override int GetBasePower()  => 50;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Mana;
    }
}