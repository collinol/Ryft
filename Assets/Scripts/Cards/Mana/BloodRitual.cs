using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Blood Ritual - Lose 3 HP, gain +5 Mana.
    /// </summary>
    public class BloodRitual : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            DealDamage(Owner, 3, ScalingStat);
            var mgr = RyftEffectManager.Ensure();
            mgr.PlayerPermanentStatsDelta(mana: 5);
            ctx.Log($"{Owner.DisplayName} performs a Blood Ritual! Loses 3 HP, gains +5 Mana!");
        }
    }
}
