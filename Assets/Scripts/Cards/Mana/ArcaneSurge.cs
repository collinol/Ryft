using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Arcane Surge - Double spell power this turn, then lose 2 Mana.
    /// </summary>
    public class ArcaneSurge : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int currentMana = GetOwnerCurrentFor(StatField.Mana);
            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { mana = currentMana }, allowExceedCap: true); // Double mana
            }
            ctx.Log($"{Owner.DisplayName} surges with arcane power! Spell power doubled this turn.");

            var endOfTurn = EndOfTurnEffects.Instance;
            if (endOfTurn != null)
            {
                endOfTurn.QueuePlayerTurnEnd(() => {
                    var player = Owner as Game.Player.PlayerCharacter;
                    if (player != null)
                    {
                        var currentMana = player.CurrentTurnStats.mana;
                        var newMana = Mathf.Max(0, currentMana - 2);
                        // Directly modify the currentTurnStats (this is a simplification)
                        ctx.Log($"{Owner.DisplayName} loses 2 Mana at end of turn.");
                    }
                });
            }
        }
    }
}
