using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Mass Production - Duplicate a random device.
    /// </summary>
    public class MassProductionCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            var copy = dm.DuplicateRandom();
            if (copy != null)
                ctx.Log($"{Owner.DisplayName} uses Mass Production, duplicated {copy.Name}.");
            else
                ctx.Log($"{Owner.DisplayName} uses Mass Production but has no devices to duplicate!");
        }
    }
}
