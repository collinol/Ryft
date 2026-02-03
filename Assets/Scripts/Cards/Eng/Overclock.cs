using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Overclock - Gain +1 Engineering for 2 turns.
    /// </summary>
    public class Overclock : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var mgr = RyftEffectManager.Ensure();
            mgr.AddTempEngineering(1);
            ctx.Log($"{Owner.DisplayName} overclocks their systems! +1 Engineering.");
        }
    }
}
