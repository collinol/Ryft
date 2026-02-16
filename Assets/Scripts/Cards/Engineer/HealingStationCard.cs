using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Healing Station - Deploy "Healing Station" with 6+stat HP, cost 2, StartOfTurn: heal player 3 HP.
    /// </summary>
    public class HealingStationCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 6 + GetStatValue();
            dm.DeployDevice("Healing Station", hp, 2, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                triggerCtx.Player.Heal(3);
                Debug.Log("[Device] Healing Station heals player for 3 HP.");
            });

            ctx.Log($"{Owner.DisplayName} deploys Healing Station (HP: {hp}).");
        }
    }
}
