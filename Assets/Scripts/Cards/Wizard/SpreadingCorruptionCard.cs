using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class SpreadingCorruptionCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Agony, 2, Owner);

            ctx.Log($"{Owner.DisplayName} uses Spreading Corruption, applying 2 Agony to {target.DisplayName}.");
        }
    }
}
