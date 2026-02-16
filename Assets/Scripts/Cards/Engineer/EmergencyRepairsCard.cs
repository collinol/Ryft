using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Emergency Repairs - Heal all devices to full HP.
    /// </summary>
    public class EmergencyRepairsCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            dm.HealAllDevices();
            ctx.Log($"{Owner.DisplayName} uses Emergency Repairs, all devices healed to full HP.");
        }
    }
}
