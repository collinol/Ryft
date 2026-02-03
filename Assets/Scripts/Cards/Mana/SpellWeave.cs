using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Spell Weave - Every third spell costs 0 Mana.
    /// Combined with draw engines → pseudo-infinite casting.
    /// </summary>
    public class SpellWeave : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Spell Weave! Every third spell costs 0.");
            var tracker = CombatEventTracker.Instance;
            ctx.Log($"{Owner.DisplayName} weaves a spell pattern! Every 3rd spell will be free.");
            // NOTE: This needs to be checked BEFORE spell cost is paid, so it requires integration at card play time
        }
    }
}
