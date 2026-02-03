using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Arcane Focus - Gain +1 Mana regeneration for 2 turns.
    /// </summary>
    public class ArcaneFocus : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var mgr = RyftEffectManager.Ensure();
            mgr.AddTempMana(1);
            ctx.Log($"{Owner.DisplayName} focuses arcane energy, gaining +1 Mana!");
        }
    }
}
