using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Elemental Mastery - Apply ElementalMastery status to owner.
    /// Next debuff application gets +2 stacks.
    /// </summary>
    public class ElementalMasteryCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects.AddEffect(StatusEffectType.ElementalMastery, -1, 1);
            PlayBuffEffect(Owner, StatField.Intellect);
            ctx.Log($"{Owner.DisplayName} uses Elemental Mastery. Next debuff gets +2 stacks.");
        }
    }
}
