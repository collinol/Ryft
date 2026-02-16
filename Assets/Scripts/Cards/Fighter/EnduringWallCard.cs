using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class EnduringWallCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);
            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            ctx.Log($"{Owner.DisplayName} uses Enduring Wall, gains {prot} protection and enters Defensive Stance.");
        }
    }
}
