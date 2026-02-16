using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class RegenerativePlatingCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int currentProt = CombatBuffManager.GetProtection(Owner);
            int heal = (currentProt / 5) * GetScaledPower();
            if (heal > 0) Owner.Heal(heal);
            ctx.Log($"{Owner.DisplayName} uses Regenerative Plating, healing {heal} HP (2 per 5 protection, had {currentProt} protection).");
        }
    }
}
