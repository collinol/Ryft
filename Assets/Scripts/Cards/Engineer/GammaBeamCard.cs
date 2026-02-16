using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class GammaBeamCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;
            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);
            HandleRewind(killed);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} fires Gamma Beam for {dmg} damage.");
        }
    }
}
