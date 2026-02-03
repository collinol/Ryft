using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Spell Ward - Reflect 2 damage when hit by magic.
    /// </summary>
    public class SpellWard : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Spell Ward! Reflects 2 damage when hit by magic.");
            Owner.StatusEffects.AddEffect(StatusEffectType.ReflectMagic, duration: -1, stacks: 1, value: 2f, sourceId: Def.id);
        }
    }
}
