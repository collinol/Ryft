using UnityEngine;
using Game.Core; using Game.Combat;

namespace Game.Cards
{
    public abstract class DamageAllCard : CardRuntime
    {

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg  = Mathf.Max(1, GetBasePower() + stat * GetScaling());

            int hits = 0;
            foreach (var e in ctx.AllAliveEnemies()) { DealDamage(e, dmg, ScalingStat); hits++; }
            if (hits > 0)
                ctx.Log($"{Owner.DisplayName} uses {Def.displayName}, dealing {dmg} to all enemies ({hits}).");
        }

        public override TargetingType Targeting => TargetingType.AllEnemies;
    }
}
