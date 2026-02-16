using Game.Core;
using Game.Combat;
using System.Linq;

namespace Game.Cards.Wizard
{
    public class AvalancheCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int dmg = GetFinalDamage();
            int kills = DealDamageToAll(ctx, dmg);

            foreach (var e in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(e, StatusEffectType.Frozen, 3, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Avalanche for {dmg} damage to all enemies and applies 3 Frozen. {kills} killed.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
