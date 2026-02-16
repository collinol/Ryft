using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class RocketBarrageCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int dmg = GetFinalDamage();
            DealDamageToAll(ctx, dmg);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} fires Rocket Barrage for {dmg} damage to all.");
        }
    }
}
