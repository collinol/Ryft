using Game.Core;
using Game.Combat;
using System.Linq;

namespace Game.Cards.Wizard
{
    public class AbsoluteZeroCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            foreach (var e in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(e, StatusEffectType.Frozen, 4, Owner);
                e.StatusEffects?.AddEffect(StatusEffectType.FrozenImmune, -1);
            }

            ctx.Log($"{Owner.DisplayName} uses Absolute Zero, applying 4 Frozen to all enemies. Frozen can no longer decay.");
        }
    }
}
