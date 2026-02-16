using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class RampageCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int dmg = GetFinalDamage();
            int kills = DealDamageToAll(ctx, dmg);
            ctx.Log($"{Owner.DisplayName} uses Rampage for {dmg} damage to all enemies. ({kills} killed)");

            HandleMomentum(kills > 0);
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
