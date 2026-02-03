using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Ethereal Reflection - Summon a clone that replays your last spell.
    /// Clone re-casts itself if not prevented → recursion.
    /// </summary>
    public class EtherealReflection : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} creates an Ethereal Reflection! Clone replays last spell.");
            var tracker = CombatEventTracker.Instance;
            if (tracker != null && tracker.LastSpellCast != null)
            {
                ctx.Log($"{Owner.DisplayName} summons an ethereal clone to replay {tracker.LastSpellCast.displayName}!");
                // TODO: Implement spell replay mechanism - needs card runtime system access
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} summons an ethereal clone, but there's no spell to replay.");
            }
        }
    }
}
