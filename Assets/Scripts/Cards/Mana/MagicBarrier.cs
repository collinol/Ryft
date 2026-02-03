using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Magic Barrier - Allies take 50% less damage this turn.
    /// </summary>
    public class MagicBarrier : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} creates a Magic Barrier! Damage reduced by 50% this turn.");
            Owner.StatusEffects.AddEffect(StatusEffectType.DamageReduction, duration: 1, stacks: 1, value: 0.5f, sourceId: Def.id);
        }
    }
}
