using Game.Core;
using Game.Combat;
using Game.Ryfts;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Mana Shield - Gain protection equal to the owner's Intellect stat.
    /// </summary>
    public class ManaShieldCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int prot = GetOwnerCurrentFor(StatField.Intellect);
            var mgr = RyftEffectManager.Ensure();
            prot += mgr.SumInt(BuiltInOp.DefendCardBonusProtection);
            GainProtection(prot);
            ctx.Log($"{Owner.DisplayName} uses Mana Shield and gains {prot} protection.");
        }
    }
}
