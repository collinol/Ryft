using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Missile Battery - Deploy "Missile Battery" with 10+stat HP, cost 4, StartOfTurn: deal 8 damage to random enemy.
    /// </summary>
    public class MissileBatteryCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 10 + GetStatValue();
            dm.DeployDevice("Missile Battery", hp, 4, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                var alive = triggerCtx.AllAliveEnemies().ToList();
                if (alive.Count == 0) return;
                var enemy = alive[Random.Range(0, alive.Count)];
                enemy.ApplyDamage(8);
                Debug.Log("[Device] Missile Battery deals 8 damage to " + enemy.DisplayName);
            });

            ctx.Log($"{Owner.DisplayName} deploys Missile Battery (HP: {hp}).");
        }
    }
}
