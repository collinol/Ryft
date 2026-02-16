using Game.Core;
using Game.Combat;

namespace Game.Cards.Wizard
{
    public class CurseOfWeaknessCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            CombatBuffManager.ApplyDebuff(target, StatusEffectType.Weakness, 2, Owner);

            ctx.Log($"{Owner.DisplayName} uses Curse of Weakness, applying 2 Weakness to {target.DisplayName}.");
        }
    }
}
