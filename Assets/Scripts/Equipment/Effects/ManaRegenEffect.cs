using UnityEngine;
using Game.Combat;
using Game.Core;
using Game.Player;

namespace Game.Equipment.Effects
{
    /// <summary>
    /// Grants bonus intellect at the start of each turn.
    /// </summary>
    public class ManaRegenEffect : IEquipmentEffect
    {
        private IActor owner;
        private int intellectPerTurn = 1;

        public void Bind(IActor owner)
        {
            this.owner = owner;
        }

        public void OnTurnStarted(FightContext ctx, IActor whoseTurn)
        {
            if (owner == null || whoseTurn != owner) return;

            if (owner is PlayerCharacter player)
            {
                var gain = new Stats { intellect = intellectPerTurn };
                player.Gain(gain, allowExceedCap: true);
                ctx?.Log($"Mana Regen: Gained {intellectPerTurn} bonus intellect.");
                Debug.Log($"[ManaRegenEffect] Granted {intellectPerTurn} intellect to {owner.DisplayName}");
            }
        }
    }
}
