using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Phoenix Form - Apply PhoenixForm status (duration 1) to owner.
    /// If would die, heal to 10 HP + deal 15 to all.
    /// </summary>
    public class PhoenixFormCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            Owner.StatusEffects.AddEffect(StatusEffectType.PhoenixForm, 1, 1);
            PlayBuffEffect(Owner, StatField.Intellect);
            ctx.Log($"{Owner.DisplayName} uses Phoenix Form! Death is not the end.");
        }
    }
}
