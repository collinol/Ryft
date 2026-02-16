using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Decoy Hologram - Deploy "Decoy Hologram" with 4+stat HP, cost 1, set IsDecoy=true.
    /// StartOfTurn: no effect (it's a decoy target).
    /// </summary>
    public class DecoyHologramCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 4 + GetStatValue();
            var device = dm.DeployDevice("Decoy Hologram", hp, 1, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                // No active effect - decoy exists to absorb enemy attacks
            });
            device.IsDecoy = true;

            ctx.Log($"{Owner.DisplayName} deploys Decoy Hologram (HP: {hp}).");
        }
    }
}
