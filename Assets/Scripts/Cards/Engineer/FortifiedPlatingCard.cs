using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Fortified Plating - All devices gain +4 max HP and heal 4 HP. Stat (eng) adds to fortify amount.
    /// </summary>
    public class FortifiedPlatingCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int fortifyAmount = 4 + GetStatValue();
            dm.FortifyDevices(fortifyAmount);
            ctx.Log($"{Owner.DisplayName} uses Fortified Plating, all devices gain +{fortifyAmount} max HP.");
        }
    }
}
