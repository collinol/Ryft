using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Spell Rebirth - If a spell kills an enemy, return it to your hand at 0 cost.
    /// </summary>
    public class SpellRebirth : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Spell Rebirth! Killing spells return at 0 cost.");

            // Register kill listener for magic kills
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                tracker.OnKill += (killer, victim, damage) =>
                {
                    // Check if the killer is the owner and the last spell was a magic spell
                    if (ReferenceEquals(killer, Owner))
                    {
                        var lastSpell = tracker.LastSpellCast;
                        if (lastSpell != null)
                        {
                            var fightController = FightSceneController.Instance;
                            if (fightController != null)
                            {
                                fightController.AddCardToHand(lastSpell, 0);
                                ctx.Log($"Spell Rebirth triggered! {lastSpell.displayName} returns at 0 cost!");
                            }
                        }
                    }
                };
            }
        }
    }
}
