using Game.Core;
using Game.Combat;

namespace Game.Cards.Engineer
{
    public class CombatStimulantsCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;
            FightSceneController.Instance?.DrawCards(3);
            FightSceneController.Instance?.GainEnergy(1);
            Owner.StatusEffects?.AddEffect(StatusEffectType.CombatStimDamage, 1, 1);
            ctx.Log($"{Owner.DisplayName} uses Combat Stimulants: draw 3, +1 energy, 3 dmg at end of turn.");
        }
    }
}
