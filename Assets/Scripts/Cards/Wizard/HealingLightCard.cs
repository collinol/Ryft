using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Healing Light - Heal 5+stat HP.
    /// </summary>
    public class HealingLightCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int heal = GetScaledHeal();
            Owner.Heal(heal);
            PlayHealEffect(Owner, heal);
            ctx.Log($"{Owner.DisplayName} uses Healing Light and heals {heal} HP.");
        }
    }
}
