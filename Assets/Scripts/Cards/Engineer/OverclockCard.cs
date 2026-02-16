using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Engineer
{
    /// <summary>
    /// Overclock - Gain 2 energy. Take 4 damage.
    /// </summary>
    public class OverclockCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            FightSceneController.Instance?.GainEnergy(2);
            Owner.ApplyDamage(4);
            ctx.Log($"{Owner.DisplayName} uses Overclock, gains 2 energy and takes 4 damage.");
        }
    }
}
