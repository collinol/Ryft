using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Berserk - Lose 5 HP, gain +5 Strength.
    /// </summary>
    public class Berserk : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            DealDamage(Owner, 5, ScalingStat);
            var mgr = RyftEffectManager.Ensure();
            mgr.PlayerPermanentStatsDelta(strength: 5);
            ctx.Log($"{Owner.DisplayName} goes berserk! Loses 5 HP, gains +5 Strength!");
        }
    }
}
