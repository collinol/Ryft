using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Rebirth - Restore to 50% HP on death (once).
    /// </summary>
    public class Rebirth : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        private bool used = false;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var deathPrevention = DeathPreventionSystem.Instance;
            if (deathPrevention != null)
            {
                deathPrevention.RegisterPrevention(Owner, DeathPreventionType.Rebirth, Def.id);
                ctx.Log($"{Owner.DisplayName} is reborn! Will restore to 50% HP if slain.");
            }
            used = false;
        }
    }
}
