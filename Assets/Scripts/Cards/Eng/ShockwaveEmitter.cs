using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Shockwave Emitter - Knock back all enemies slightly.
    /// </summary>
    public class ShockwaveEmitter : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} emits a shockwave! All enemies knocked back.");
            // TODO: Apply knockback effect
        }
    }
}
