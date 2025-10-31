using UnityEngine;
using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Reusable runtime that grants "reflect next enemy turn".
    /// The reflect percent is computed from Def.power and Def.scaling * ScalingStat value.
    ///
    /// Example: Def.power = 50, Def.scaling = 0  =>  50% reflect next enemy turn.
    /// If you want scaling, set Def.scaling and override ScalingStat in the concrete card.
    /// </summary>
    public abstract class ReflectCard : CardRuntime
    {
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var owner = Owner;

            // Percent = (power + scaling * statValue) / 100
            int statVal = GetOwnerCurrentFor(ScalingStat);
            float pct = Mathf.Clamp01((GetBasePower() + GetScaling() * statVal) / 100f);

            ReflectNextTurnStatus.Grant(owner, pct, ctx);

            ctx.Log($"{owner.DisplayName} prepares to reflect {Mathf.RoundToInt(pct * 100f)}% of incoming damage next enemy turn.");


        }
    }
}
