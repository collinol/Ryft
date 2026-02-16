using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Emergency Protocol - Draw 2 cards. Cost 0.
    /// </summary>
    public class EmergencyProtocolCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            FightSceneController.Instance?.DrawCards(2);
            ctx.Log($"{Owner.DisplayName} uses Emergency Protocol and draws 2 cards.");
        }
    }
}
