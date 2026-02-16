using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Sentry Gun - Deploy "Sentry Gun" with 8+stat HP, cost 3, StartOfTurn: deal 5 damage to random enemy.
    /// </summary>
    public class SentryGunCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 8 + GetStatValue();
            dm.DeployDevice("Sentry Gun", hp, 3, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                var alive = triggerCtx.AllAliveEnemies().ToList();
                if (alive.Count == 0) return;
                var enemy = alive[Random.Range(0, alive.Count)];
                enemy.ApplyDamage(5);
                Debug.Log("[Device] Sentry Gun deals 5 damage to " + enemy.DisplayName);
            });

            ctx.Log($"{Owner.DisplayName} deploys Sentry Gun (HP: {hp}).");
        }
    }
}
