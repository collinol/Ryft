using Game.Core;

namespace Game.Cards
{
    /// <summary>
    /// Strike - Deal 5 damage.
    /// </summary>
    public class Strike : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 5;
        protected override int GetScaling() => 1;
    }
}
