using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class ArmoredShellCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            StanceManager.Instance?.EnterStance(StanceType.Defensive);
            int prot = Mathf.RoundToInt(Owner.TotalStats.maxHealth * 0.25f);
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} uses Armored Shell, enters Defensive Stance and gains {prot} protection (25% of max HP).");
        }
    }
}
