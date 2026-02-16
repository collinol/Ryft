using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class FrenziedRegenerationCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int attacksPlayed = FightSceneController.Instance != null
                ? FightSceneController.Instance.AttackCardsPlayedThisTurn
                : 0;

            int heal = GetScaledPower() * attacksPlayed;
            Owner.Heal(heal);

            ctx.Log($"{Owner.DisplayName} uses Frenzied Regeneration. Heals {heal} HP ({attacksPlayed} attacks played this turn).");
        }
    }
}
