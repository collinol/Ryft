using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    public class MeteorCard : DamageAllCard
    {
        protected override int GetEnergyCost() => 2;
        protected override int GetBasePower()  => 8;
        protected override int GetScaling()    => 1;
        protected override StatField ScalingStat => StatField.Mana;
    }
}
