using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Battle Rhythm - Draw a card every third attack.
    /// With zero-cost strikes, becomes an infinite draw loop.
    /// </summary>
    public class BattleRhythm : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates {Def.displayName}! Draw a card every 3 attacks.");
            var mgr = RyftEffectManager.Ensure();
            mgr.RegisterDrawEveryNCards(3, 1);
        }
    }
}
