using Game.Core;
using Game.Combat;
using System.Linq;

namespace Game.Cards.Engineer
{
    public class Card500 : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            int dmg = GetFinalDamage();
            int kills = DealDamageToAll(ctx, dmg);
            Owner.ApplyDamage(15);
            HandleRewind(kills > 0);
            FightSceneController.Instance?.TrackAttackCardPlayed();
            ctx.Log($"{Owner.DisplayName} uses 500: {dmg} to all enemies, takes 15 self-damage.");
        }
    }
}
