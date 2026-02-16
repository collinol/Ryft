using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class UnyieldingFortressCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);
            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            ctx.Log($"{Owner.DisplayName} uses Unyielding Fortress, gains {prot} protection and enters Defensive Stance.");
        }
    }
}
