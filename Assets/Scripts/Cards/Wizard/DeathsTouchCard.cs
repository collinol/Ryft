using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class DeathsTouchCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);

            if (!killed)
            {
                CombatBuffManager.ApplyDebuff(target, StatusEffectType.Cursed, 2, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Death's Touch for {dmg} damage and applies 2 Cursed to {target.DisplayName}.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
