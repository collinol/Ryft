using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Temporal Shield - Deploy "Temporal Shield" with 8+stat HP, cost 2, StartOfTurn: grant 3 protection to player.
    /// </summary>
    public class TemporalShieldCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 8 + GetStatValue();
            dm.DeployDevice("Temporal Shield", hp, 2, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                CombatBuffManager.GainProtection(triggerCtx.PlayerActor, 3);
                Debug.Log("[Device] Temporal Shield grants 3 protection.");
            });

            ctx.Log($"{Owner.DisplayName} deploys Temporal Shield (HP: {hp}).");
        }
    }
}
