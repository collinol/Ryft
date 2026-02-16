using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class AlphaBeamCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;
            int dmg = GetFinalDamage();
            DealDamageTracked(target, dmg);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} fires Alpha Beam for {dmg} damage.");
        }
    }
}
