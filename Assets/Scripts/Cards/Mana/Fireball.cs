using Game.Core;

namespace Game.Cards
{
    /// <summary>
    /// Fireball - Deal 5 magic damage.
    /// </summary>
    public class Fireball : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 5;
        protected override int GetScaling() => 1;
    }
}
