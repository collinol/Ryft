using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class SecondWindCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetStatValue();
            int threshold = Mathf.RoundToInt(Owner.TotalStats.maxHealth * 0.5f);
            int heal;

            if (Owner.Health < threshold)
            {
                heal = 16 + stat;
                ctx.Log($"{Owner.DisplayName} uses Second Wind (below 50% HP) and heals {heal} HP.");
            }
            else
            {
                heal = 8 + stat;
                ctx.Log($"{Owner.DisplayName} uses Second Wind and heals {heal} HP.");
            }

            Owner.Heal(heal);
        }
    }
}
