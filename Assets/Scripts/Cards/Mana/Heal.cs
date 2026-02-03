using Game.Core;

namespace Game.Cards
{
    /// <summary>
    /// Heal - Restore 5 HP.
    /// </summary>
    public class Heal : HealCard
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 5;
        protected override int GetScaling() => 1;
    }
}
