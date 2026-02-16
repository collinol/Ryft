using Game.Core;
using Game.Combat;
using System.Linq;

namespace Game.Cards.Wizard
{
    public class FrostNovaCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stacks = GetScaledPower();

            foreach (var e in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(e, StatusEffectType.Frozen, stacks, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Frost Nova, applying {stacks} Frozen to all enemies.");
        }
    }
}
