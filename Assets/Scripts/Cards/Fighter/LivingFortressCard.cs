using Game.Core;
using Game.Combat;

namespace Game.Cards.Fighter
{
    public class LivingFortressCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var fsc = FightSceneController.Instance;
            int cardsInHand = fsc != null ? fsc.CurrentHand.Count : 0;
            int protPerCard = 3 + GetStatValue();
            int totalProt = protPerCard * cardsInHand;
            GainProtection(totalProt);
            ctx.Log($"{Owner.DisplayName} uses Living Fortress, gains {totalProt} protection ({protPerCard} x {cardsInHand} cards in hand).");
        }
    }
}
