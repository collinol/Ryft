using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Auto-Assembler - Creates a copy of a random Engineering card in your hand.
    /// </summary>
    public class AutoAssembler : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            // Get a random Engineering card from the deck
            var fightController = FightSceneController.Instance;
            if (fightController != null)
            {
                var randomEngCard = fightController.GetRandomCardByType(StatField.Engineering);
                if (randomEngCard != null)
                {
                    fightController.AddCardToHand(randomEngCard);
                    ctx.Log($"{Owner.DisplayName} activates Auto-Assembler! Created {randomEngCard.displayName}!");
                }
                else
                {
                    ctx.Log($"{Owner.DisplayName} activates Auto-Assembler, but no Engineering cards found!");
                }
            }
        }
    }
}
