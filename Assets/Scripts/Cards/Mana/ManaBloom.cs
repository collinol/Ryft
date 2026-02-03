using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Mana Bloom - Whenever you cast a spell, gain +1 Mana for each active buff.
    /// </summary>
    public class ManaBloom : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Mana Bloom! Spells grant +1 Mana per buff.");
            void OnSpellCast(CardDef spell, IActor caster)
            {
                if (caster == Owner)
                {
                    var player = Owner as Game.Player.PlayerCharacter;
                    if (player != null)
                    {
                        int buffCount = player.StatusEffects.CountBuffs();
                        if (buffCount > 0)
                        {
                            player.Gain(new Stats { mana = buffCount }, allowExceedCap: true);
                        }
                    }
                }
            }
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                tracker.OnSpellCast += OnSpellCast;
            }
            ctx.Log($"{Owner.DisplayName} activates Mana Bloom! Will gain +1 Mana per buff when casting spells.");
        }
    }
}
