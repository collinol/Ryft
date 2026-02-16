using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Controlled Demolition - Destroy a random device you control. Deal 8+stat damage to all enemies.
    /// </summary>
    public class ControlledDemolitionCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            var device = dm.GetRandomDevice();
            if (device == null)
            {
                ctx.Log($"{Owner.DisplayName} uses Controlled Demolition but has no devices!");
                return;
            }

            string deviceName = device.Name;
            dm.DestroyDevice(device);

            int dmg = GetFinalDamage();
            int kills = DealDamageToAll(ctx, dmg);
            ctx.Log($"{Owner.DisplayName} detonates {deviceName} for {dmg} damage to all enemies. ({kills} kills)");
        }
    }
}
