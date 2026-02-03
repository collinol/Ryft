using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Reactive Armor - Reflect 2 damage when hit by ranged attack.
    /// </summary>
    public class ReactiveArmor : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Reactive Armor! Reflects 2 damage from ranged attacks.");
            Owner.StatusEffects.AddEffect(StatusEffectType.ReflectRanged, duration: -1, stacks: 1, value: 2f, sourceId: Def.id);
        }
    }
}
