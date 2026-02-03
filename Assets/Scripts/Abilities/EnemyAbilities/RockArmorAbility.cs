using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// <summary>
    /// Golem encases itself in rock, gaining significant damage reduction.
    /// </summary>
    public class RockArmorAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            int duration = 3;
            int reduction = Mathf.Max(5, Def.power);

            Owner.StatusEffects?.AddEffect(StatusEffectType.DamageReduction, duration, 1, reduction);

            ctx.Log($"{Owner.DisplayName} encases itself in rock armor, gaining {reduction} damage reduction for {duration} turns!");

            PutOnCooldown();
        }
    }
}
