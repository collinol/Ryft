using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class DoomCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Doom, 10, Owner);

            ctx.Log($"{Owner.DisplayName} uses Doom on {target.DisplayName}. Death in 10 turns.");
        }
    }
}
