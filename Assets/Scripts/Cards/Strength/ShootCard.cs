using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    public class ShootCard : DamageSingleCard
    {
        protected override StatField CostField => StatField.Strength;
        protected override int GetBaseCostAmount(StatField field) => 1;
        protected override int GetBasePower()  => 4;
        protected override int GetScaling()    => 1;
    }
}
