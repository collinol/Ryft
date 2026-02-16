using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Chrono Turret - Deploy "Chrono Turret" with 5+stat HP, cost 2, StartOfTurn: deal 3 damage to random alive enemy.
    /// </summary>
    public class ChronoTurretCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            int hp = 5 + GetStatValue();
            dm.DeployDevice("Chrono Turret", hp, 2, DeviceTrigger.StartOfTurn, triggerCtx =>
            {
                var alive = triggerCtx.AllAliveEnemies().ToList();
                if (alive.Count == 0) return;
                var enemy = alive[Random.Range(0, alive.Count)];
                enemy.ApplyDamage(3);
                Debug.Log("[Device] Chrono Turret deals 3 damage to " + enemy.DisplayName);
            });

            ctx.Log($"{Owner.DisplayName} deploys Chrono Turret (HP: {hp}).");
        }
    }
}
