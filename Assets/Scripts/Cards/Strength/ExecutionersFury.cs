using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Executioner's Fury - Kill an enemy → reset all Strength cooldowns.
    /// Pair with self-damage targets or summons.
    /// </summary>
    public class ExecutionersFury : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates {Def.displayName}! Next kill resets all Strength cooldowns.");
            // TODO: Register kill listener that resets cooldowns
        }
    }
}
