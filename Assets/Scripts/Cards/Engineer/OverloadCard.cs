using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Overload - Destroy a random device. Draw 2 cards. Rewind keyword.
    /// </summary>
    public class OverloadCard : CardRuntime
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
                ctx.Log($"{Owner.DisplayName} uses Overload but has no devices!");
                return;
            }

            string deviceName = device.Name;
            dm.DestroyDevice(device);

            FightSceneController.Instance?.DrawCards(2);
            ctx.Log($"{Owner.DisplayName} overloads {deviceName} and draws 2 cards.");
        }
    }
}
