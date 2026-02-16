using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class ReaperCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int cursedStacks = target.StatusEffects.GetStacks(StatusEffectType.Cursed);
            float hpPercent = (float)target.Health / Mathf.Max(1, target.TotalStats.maxHealth);
            bool killed = false;

            if (cursedStacks >= 20 && hpPercent < 0.2f)
            {
                bool isBoss = (target as Game.Enemies.EnemyBase)?.SourceDef?.tier == Game.Enemies.EnemyTier.Elite;

                if (isBoss)
                {
                    int dmg = 50;
                    killed = DealDamageTracked(target, dmg);
                    ctx.Log($"{Owner.DisplayName} uses Reaper on boss {target.DisplayName} for {dmg} damage. ({cursedStacks} Cursed, {hpPercent:P0} HP)");
                }
                else
                {
                    target.ApplyDamage(99999);
                    killed = !target.IsAlive;
                    ctx.Log($"{Owner.DisplayName} uses Reaper, executing {target.DisplayName}! ({cursedStacks} Cursed, {hpPercent:P0} HP)");
                }
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses Reaper but conditions not met on {target.DisplayName} ({cursedStacks} Cursed, {hpPercent:P0} HP). Need 20+ Cursed and below 20% HP.");
            }

            HandleExtract(killed, cursedStacks);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
