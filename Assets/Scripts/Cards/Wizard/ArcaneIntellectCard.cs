using Game.Core;
using Game.Combat;
using Game.Ryfts;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Arcane Intellect - Draw 3 cards and gain +1 Intellect permanently for combat.
    /// </summary>
    public class ArcaneIntellectCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            FightSceneController.Instance?.DrawCards(3);

            RyftEffectManager.Ensure().PlayerPermanentStatsDelta(intellect: 1);

            ctx.Log($"{Owner.DisplayName} uses Arcane Intellect: draws 3 cards and gains +1 Intellect.");
        }
    }
}
