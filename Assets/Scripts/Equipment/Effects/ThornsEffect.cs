using UnityEngine;
using Game.Combat;
using Game.Core;

namespace Game.Equipment.Effects
{
    /// <summary>
    /// Reflects a portion of damage taken back to attacker.
    /// </summary>
    public class ThornsEffect : IEquipmentEffect
    {
        private IActor owner;
        private int thornsDamage = 2;

        public void Bind(IActor owner)
        {
            this.owner = owner;
        }

        public void OnOwnerDamaged(FightContext ctx, IActor attacker, int damage)
        {
            if (owner == null || attacker == null || damage <= 0) return;
            if (attacker == owner) return; // Don't reflect self-damage

            attacker.ApplyDamage(thornsDamage);
            ctx?.Log($"Thorns: Reflected {thornsDamage} damage back to {attacker.DisplayName}!");
            Debug.Log($"[ThornsEffect] Reflected {thornsDamage} to {attacker.DisplayName}");
        }
    }
}
