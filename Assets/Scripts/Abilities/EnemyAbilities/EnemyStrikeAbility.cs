// Assets/Scripts/Abilities/EnemyAbilities/EnemyStrikeAbility.cs
using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Abilities.EnemyAbilities
{
    /// Deal Def.power (e.g., 5) to the player.
    public class EnemyStrikeAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = explicitTarget ?? ctx.Player;
            if (target == null || !target.IsAlive) return;

            var dmg = Mathf.Max(1, Def.power);
            target.ApplyDamage(dmg);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage on {target.DisplayName}.");

            PutOnCooldown();
        }
    }
}
