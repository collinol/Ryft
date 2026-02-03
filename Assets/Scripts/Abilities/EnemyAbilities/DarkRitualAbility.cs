using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Cultist sacrifices some HP to buff all enemies.
    /// </summary>
    public class DarkRitualAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            // Sacrifice HP
            int sacrifice = Mathf.Max(1, Owner.TotalStats.maxHealth / 10);
            Owner.ApplyDamage(sacrifice);

            if (!Owner.IsAlive)
            {
                ctx.Log($"{Owner.DisplayName} sacrifices too much and perishes!");
                PutOnCooldown();
                return;
            }

            // Buff all allies
            int buffDuration = 3;
            int damageBoost = Mathf.Max(2, Def.power / 3);

            foreach (var enemy in ctx.Enemies)
            {
                if (enemy != null && enemy.IsAlive && enemy != Owner)
                {
                    enemy.StatusEffects?.AddEffect(StatusEffectType.DefenseUp, buffDuration, 1, damageBoost);
                }
            }

            ctx.Log($"{Owner.DisplayName} performs a dark ritual, sacrificing {sacrifice} HP to empower allies!");

            PutOnCooldown();
        }
    }
}
