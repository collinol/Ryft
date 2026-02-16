using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Repair - Heal 4+stat HP. If DeviceManager has devices, heal 1 device for 4 HP too.
    /// If no devices, heal self 2 extra.
    /// </summary>
    public class RepairCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int heal = GetScaledHeal();
            Owner.Heal(heal);

            var dm = DeviceManager.Instance;
            if (dm != null && dm.DeviceCount > 0)
            {
                var device = dm.GetRandomDevice();
                if (device != null)
                {
                    device.Heal(4);
                    ctx.Log($"{Owner.DisplayName} uses Repair, heals {heal} HP and repairs {device.Name} for 4 HP.");
                }
            }
            else
            {
                Owner.Heal(2);
                ctx.Log($"{Owner.DisplayName} uses Repair, heals {heal + 2} HP (no devices bonus).");
            }
        }
    }
}
