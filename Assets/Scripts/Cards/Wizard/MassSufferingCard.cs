using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class MassSufferingCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            target.StatusEffects?.AddEffect(StatusEffectType.MassSuffering, -1);

            ctx.Log($"{Owner.DisplayName} uses Mass Suffering on {target.DisplayName}. On death, Agony transfers to a random enemy.");
        }
    }
}
