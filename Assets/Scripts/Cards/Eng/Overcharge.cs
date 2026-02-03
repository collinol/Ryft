using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Overcharge - Lose 3 HP, gain +5 Engineering.
    /// </summary>
    public class Overcharge : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            DealDamage(Owner, 3, ScalingStat);
            var mgr = RyftEffectManager.Ensure();
            mgr.PlayerPermanentStatsDelta(eng: 5);
            ctx.Log($"{Owner.DisplayName} overcharges! Loses 3 HP, gains +5 Engineering!");
        }
    }
}
