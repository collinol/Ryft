using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Equipment
{
    /// Reflects 1 damage to the attacker whenever the owner is damaged.
    public class ThornsOnHitEffect : IEquipmentEffect
    {
        private IActor owner;
        public void Bind(IActor o) { owner = o; }

        public void OnOwnerDamaged(FightContext ctx, IActor attacker, int damage)
        {
            if (attacker == null || damage <= 0) return;
            attacker.ApplyDamage(1);
            ctx?.Log($"{owner?.DisplayName} thorns reflect 1 damage to {attacker.DisplayName}.");
        }
    }
}
