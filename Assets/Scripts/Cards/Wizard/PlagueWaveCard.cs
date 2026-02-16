using Game.Core;
using Game.Combat;
using System.Linq;

namespace Game.Cards.Wizard
{
    public class PlagueWaveCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int dmg = GetFinalDamage();
            int kills = DealDamageToAll(ctx, dmg);

            foreach (var e in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(e, StatusEffectType.Cursed, 2, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Plague Wave for {dmg} damage to all enemies and applies 2 Cursed. {kills} killed.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
