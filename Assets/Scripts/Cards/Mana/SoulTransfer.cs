using Game.Core;
using Game.Combat;

namespace Game.Cards
{
    /// <summary>
    /// Soul Transfer - Heal 2 HP per magic kill.
    /// </summary>
    public class SoulTransfer : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        public override TargetingType Targeting => TargetingType.Self;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            ctx.Log($"{Owner.DisplayName} activates Soul Transfer! +2 HP per magic kill.");
            var tracker = CombatEventTracker.Instance;
            if (tracker != null)
            {
                int magicKills = tracker.GetMagicKillsThisTurn();
                int healAmount = magicKills * 2;
                if (healAmount > 0)
                {
                    Owner.Heal(healAmount);
                    ctx.Log($"{Owner.DisplayName} heals {healAmount} HP from {magicKills} magic kills!");
                }
            }
        }
    }
}
