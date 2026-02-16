using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Salvage Protocol - Destroy a random device. Gain energy = device.OriginalCost + 1.
    /// </summary>
    public class SalvageProtocolCard : CardRuntime
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
                ctx.Log($"{Owner.DisplayName} uses Salvage Protocol but has no devices!");
                return;
            }

            string deviceName = device.Name;
            int energyGain = device.OriginalCost + 1;
            dm.DestroyDevice(device);

            FightSceneController.Instance?.GainEnergy(energyGain);
            ctx.Log($"{Owner.DisplayName} salvages {deviceName} and gains {energyGain} energy.");
        }
    }
}
