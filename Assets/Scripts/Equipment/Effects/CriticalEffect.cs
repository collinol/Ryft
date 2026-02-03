using UnityEngine;
using Game.Combat;
using Game.Core;

namespace Game.Equipment.Effects
{
    /// <summary>
    /// Chance to deal double damage on attacks.
    /// Note: This effect is tracked via a static flag that card abilities check.
    /// </summary>
    public class CriticalEffect : IEquipmentEffect
    {
        private IActor owner;
        private float critChance = 0.15f; // 15% crit chance

        public static bool CriticalHitTriggered { get; private set; }

        public void Bind(IActor owner)
        {
            this.owner = owner;
        }

        public void OnTurnStarted(FightContext ctx, IActor whoseTurn)
        {
            if (owner == null || whoseTurn != owner) return;

            // Roll for crit at start of turn
            CriticalHitTriggered = Random.value < critChance;

            if (CriticalHitTriggered)
            {
                ctx?.Log("Critical Hit ready! Next attack deals double damage!");
                Debug.Log($"[CriticalEffect] Critical hit ready for {owner.DisplayName}");
            }
        }

        public void OnOwnerDealtDamage(FightContext ctx, IActor target, int damage)
        {
            // Reset crit flag after dealing damage
            if (CriticalHitTriggered)
            {
                CriticalHitTriggered = false;
            }
        }
    }
}
