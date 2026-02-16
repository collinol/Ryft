using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Network Link - All devices trigger their start-of-turn effects immediately. Rewind keyword.
    /// </summary>
    public class NetworkLinkCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            dm.TriggerAllStartOfTurn(ctx);
            ctx.Log($"{Owner.DisplayName} uses Network Link, all devices trigger their effects!");

            // Check if any enemies were killed by triggered effects
            bool anyKilled = ctx.AllEnemiesDead();
            HandleRewind(anyKilled);
        }
    }
}
