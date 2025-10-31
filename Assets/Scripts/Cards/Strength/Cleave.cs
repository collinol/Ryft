using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    public class Cleave : DamageAllCard
    {
        protected override int GetEnergyCost() => 1;
        protected override int GetBasePower()  => 4;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Strength;
    }
}
