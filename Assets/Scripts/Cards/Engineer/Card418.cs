using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class Card418 : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;
            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Burning, GetScaledPower(), Owner);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} uses 418: 5 Burning applied to {target.DisplayName}.");
        }
    }
}
