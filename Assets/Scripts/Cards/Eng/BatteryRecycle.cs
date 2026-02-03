using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Battery Recycle - Heal 2 HP per gadget destroyed.
    /// </summary>
    public class BatteryRecycle : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Battery Recycle! +2 HP per gadget destroyed.");
            // TODO: Track gadget destruction and heal
        }
    }
}
