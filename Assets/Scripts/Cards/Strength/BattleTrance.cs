// Assets/Scripts/Cards/Runtime/PlaymakerCard.cs
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    public class BattleTrance : CardRuntime
    {
        protected override int GetEnergyCost() => 1;
        public override TargetingType Targeting => TargetingType.Self;
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 3;
        protected override int GetScaling()   => 1;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            RyftEffectManager.Ensure().RegisterDrawEveryNCards(n: GetBasePower(), drawAmount: GetScaling());
            ctx.Log($"{Owner.DisplayName} plays {Def.displayName}: Every 3 cards you play, draw 1.");
        }


    }
}
