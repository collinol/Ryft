using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class GuardCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} uses Guard and gains {prot} protection.");
        }
    }
}
