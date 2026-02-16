using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class ReflectiveShieldCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects?.AddEffect(StatusEffectType.ReflectAll, 1);
            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            ctx.Log($"{Owner.DisplayName} uses Reflective Shield, reflecting all damage for 1 turn and entering Defensive Stance.");
        }
    }
}
