using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class PermafrostCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stacks = GetScaledPower();
            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Frozen, stacks, Owner);

            ctx.Log($"{Owner.DisplayName} uses Permafrost, applying {stacks} Frozen to {target.DisplayName}.");
        }
    }
}
