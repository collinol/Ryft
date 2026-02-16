using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Deploy Cover - Gain 5+stat protection.
    /// </summary>
    public class DeployCoverCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetScaledProtection();
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} uses Deploy Cover and gains {prot} protection.");
        }
    }
}
