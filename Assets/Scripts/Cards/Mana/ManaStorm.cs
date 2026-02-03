using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Mana Storm - Casting a spell draws another.
    /// With 0-cost cards, you get infinite casting.
    /// </summary>
    public class ManaStorm : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} conjures a Mana Storm! Each spell draws a card.");
            void OnSpellCast(CardDef spell, IActor caster)
            {
                if (caster == Owner)
                {
                    FightSceneController.Instance?.DrawCards(1);
                }
            }
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                tracker.OnSpellCast += OnSpellCast;
            }
            ctx.Log($"{Owner.DisplayName} activates Mana Storm! Will draw a card each time a spell is cast.");
        }
    }
}
