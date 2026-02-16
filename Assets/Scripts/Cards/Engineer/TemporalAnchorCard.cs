using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Temporal Anchor - Deploy "Temporal Anchor" with 5+stat HP, cost 3, StartOfTurn: draw 1 card.
    /// </summary>
    public class TemporalAnchorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 5 + GetStatValue();
            dm.DeployDevice("Temporal Anchor", hp, 3, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                FightSceneController.Instance?.DrawCards(1);
                Debug.Log("[Device] Temporal Anchor draws 1 card.");
            });

            ctx.Log($"{Owner.DisplayName} deploys Temporal Anchor (HP: {hp}).");
        }
    }
}
