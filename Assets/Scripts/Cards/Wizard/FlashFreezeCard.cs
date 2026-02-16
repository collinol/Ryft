using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class FlashFreezeCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Frozen, 5, Owner);
            target.StatusEffects?.AddEffect(StatusEffectType.DamageImmune, 1);

            ctx.Log($"{Owner.DisplayName} uses Flash Freeze on {target.DisplayName}, applying 5 Frozen and DamageImmune for 1 turn.");
        }
    }
}
