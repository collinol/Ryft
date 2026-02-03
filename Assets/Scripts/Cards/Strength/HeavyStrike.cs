using Game.Core;

namespace Game.Cards
{
    /// <summary>
    /// Heavy Strike - Deal 10 damage.
    /// </summary>
    public class HeavyStrike : DamageSingleCard
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 10;
        protected override int GetScaling() => 1;
    }
}
