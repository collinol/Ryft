using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Grants the user damage reduction for several turns.
    /// </summary>
    public class FortifyAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            int duration = Mathf.Max(2, Def.power / 5);
            int reduction = Mathf.Max(2, Def.power / 2);

            Owner.StatusEffects?.AddEffect(StatusEffectType.DamageReduction, duration, 1, reduction);

            ctx.Log($"{Owner.DisplayName} fortifies, gaining {reduction} damage reduction for {duration} turns!");

            PutOnCooldown();
        }
    }
}
