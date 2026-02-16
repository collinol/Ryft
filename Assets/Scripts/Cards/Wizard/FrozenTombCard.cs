using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class FrozenTombCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int frozenStacks = target.StatusEffects.GetStacks(StatusEffectType.Frozen);
            bool killed = false;

            if (frozenStacks >= 10)
            {
                bool isBoss = (target as Game.Enemies.EnemyBase)?.SourceDef?.tier == Game.Enemies.EnemyTier.Elite;

                if (isBoss)
                {
                    int dmg = 30;
                    killed = DealDamageTracked(target, dmg);
                    ctx.Log($"{Owner.DisplayName} uses Frozen Tomb on boss {target.DisplayName} for {dmg} damage.");
                }
                else
                {
                    target.ApplyDamage(99999);
                    killed = !target.IsAlive;
                    ctx.Log($"{Owner.DisplayName} uses Frozen Tomb, instantly killing {target.DisplayName}!");
                }
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses Frozen Tomb but {target.DisplayName} only has {frozenStacks} Frozen (need 10+).");
            }

            HandleExtract(killed, frozenStacks);
        }
    }
}
