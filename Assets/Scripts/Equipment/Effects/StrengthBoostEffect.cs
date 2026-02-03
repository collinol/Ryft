using UnityEngine;
using Game.Combat;
using Game.Core;
using Game.Player;

namespace Game.Equipment.Effects
{
    /// <summary>
    /// Grants bonus strength at the start of each turn.
    /// </summary>
    public class StrengthBoostEffect : IEquipmentEffect
    {
        private IActor owner;
        private int strengthPerTurn = 1;

        public void Bind(IActor owner)
        {
            this.owner = owner;
        }

        public void OnTurnStarted(FightContext ctx, IActor whoseTurn)
        {
            if (owner == null || whoseTurn != owner) return;

            if (owner is PlayerCharacter player)
            {
                var gain = new Stats { strength = strengthPerTurn };
                player.Gain(gain, allowExceedCap: true);
                ctx?.Log($"Strength Boost: Gained {strengthPerTurn} bonus strength.");
                Debug.Log($"[StrengthBoostEffect] Granted {strengthPerTurn} strength to {owner.DisplayName}");
            }
        }
    }
}
