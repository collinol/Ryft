using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Ice Barrier - Gain +3 Defense for 2 turns.
    /// </summary>
    public class IceBarrier : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} creates an Ice Barrier! +3 Defense for 2 turns.");
            Owner.StatusEffects.AddEffect(StatusEffectType.DefenseUp, duration: 2, stacks: 1, value: 3f, sourceId: Def.id);
        }
    }
}
