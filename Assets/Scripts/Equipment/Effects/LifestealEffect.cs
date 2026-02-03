using UnityEngine;
using Game.Combat;
using Game.Core;

namespace Game.Equipment.Effects
{
    /// <summary>
    /// Heals the owner for a percentage of damage dealt.
    /// </summary>
    public class LifestealEffect : IEquipmentEffect
    {
        private IActor owner;
        private float lifestealPercent = 0.15f; // 15% lifesteal

        public void Bind(IActor owner)
        {
            this.owner = owner;
        }

        public void OnOwnerDealtDamage(FightContext ctx, IActor target, int damage)
        {
            if (owner == null || damage <= 0) return;

            int healAmount = Mathf.Max(1, Mathf.RoundToInt(damage * lifestealPercent));
            owner.Heal(healAmount);

            ctx?.Log($"Lifesteal: Healed {healAmount} from dealing {damage} damage.");
            Debug.Log($"[LifestealEffect] Healed {owner.DisplayName} for {healAmount}");
        }
    }
}
