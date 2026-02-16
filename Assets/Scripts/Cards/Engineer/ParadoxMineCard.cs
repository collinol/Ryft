using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Paradox Mine - Deploy "Paradox Mine" with 1 HP, cost 2, OnEnemyAttack: deal 10 damage to ALL enemies, then destroy self.
    /// </summary>
    public class ParadoxMineCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var dm = DeviceManager.Instance;
            if (dm == null) return;

            Device mine = dm.DeployDevice("Paradox Mine", 1, 2, DeviceTrigger.OnEnemyAttack, triggerCtx =>
            {
                foreach (var enemy in triggerCtx.AllAliveEnemies().ToList())
                {
                    enemy.ApplyDamage(10);
                }
                Debug.Log("[Device] Paradox Mine explodes for 10 damage to all enemies!");

                // Destroy self after detonation
                var self = DeviceManager.Instance?.Devices;
                if (self != null)
                {
                    foreach (var d in self)
                    {
                        if (d.Name == "Paradox Mine" && d.IsAlive)
                        {
                            DeviceManager.Instance.DestroyDevice(d);
                            break;
                        }
                    }
                }
            });

            ctx.Log($"{Owner.DisplayName} deploys Paradox Mine.");
        }
    }
}
