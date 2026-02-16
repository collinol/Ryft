using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Overcharged Capacitor - Deploy "Overcharged Capacitor" with 3+stat HP, cost 2, set BonusFlatDamage=1.
    /// StartOfTurn: no active effect (passive damage bonus).
    /// </summary>
    public class OverchargedCapacitorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 3 + GetStatValue();
            var device = dm.DeployDevice("Overcharged Capacitor", hp, 2, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                // No active effect - provides passive BonusFlatDamage
            });
            device.BonusFlatDamage = 1;

            ctx.Log($"{Owner.DisplayName} deploys Overcharged Capacitor (HP: {hp}).");
        }
    }
}
