using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class Card503 : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;
            target.StatusEffects?.AddEffect(StatusEffectType.EnemySkipAttack, 1, 1);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} uses 503: {target.DisplayName} cannot attack next turn.");
        }
    }
}
