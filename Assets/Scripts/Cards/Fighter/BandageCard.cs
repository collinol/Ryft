using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class BandageCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var stance = StanceManager.Instance;
            if (stance != null && stance.IsInStance(StanceType.Rage))
            {
                int heal = 6 + GetStatValue();
                Owner.Heal(heal);
                ctx.Log($"{Owner.DisplayName} uses Bandage (Rage) and heals {heal} HP.");
            }
            else if (stance != null && stance.IsInStance(StanceType.Defensive))
            {
                int prot = 3 + GetStatValue();
                GainProtection(prot);
                ctx.Log($"{Owner.DisplayName} uses Bandage (Defensive) and gains {prot} protection.");
            }
            else
            {
                int heal = 4 + GetStatValue();
                Owner.Heal(heal);
                ctx.Log($"{Owner.DisplayName} uses Bandage and heals {heal} HP.");
            }
        }
    }
}
