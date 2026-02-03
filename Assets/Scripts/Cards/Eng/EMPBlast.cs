using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// EMP Blast - Disable enemy shields/robots for 1 turn.
    /// </summary>
    public class EMPBlast : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} triggers an EMP Blast! All enemy shields disabled.");
            // TODO: Disable shields/buffs on enemies
        }
    }
}
