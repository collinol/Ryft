using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Chain Strike - After you play this, play it again for +1 Strength cost (repeatable).
    /// Becomes a soft infinite with Strength cost reduction effects.
    /// </summary>
    public class ChainStrike : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 3;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.SingleEnemy;

        private int chainCount = 0;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();
            dmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, target);
            DealDamage(target, dmg, ScalingStat);

            chainCount++;
            ctx.Log($"{Owner.DisplayName} chain strikes for {dmg} damage! (Chain #{chainCount})");

            // Add this card back to hand with +1 cost
            var fightController = FightSceneController.Instance;
            if (fightController != null)
            {
                // Note: Cost override not fully implemented yet, but this adds the card back
                fightController.AddCardToHand(Def, GetEnergyCost() + 1);
                ctx.Log($"Chain Strike returns to hand with +1 Energy cost!");
            }
        }
    }
}
