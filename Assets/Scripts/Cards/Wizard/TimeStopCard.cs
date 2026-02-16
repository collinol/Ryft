using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Time Stop - Queue an extra turn.
    /// </summary>
    public class TimeStopCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            FightSceneController.Instance?.QueueExtraTurn();
            ctx.Log($"{Owner.DisplayName} uses Time Stop! Extra turn queued.");
        }
    }
}
