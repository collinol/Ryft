using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// Power Slam - Deal 6 damage and apply Stun (1 turn).
    public class PowerSlam : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 6;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.SingleEnemy;

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

            // Apply Stun for 1 turn
            target.StatusEffects.AddEffect(StatusEffectType.Stun, duration: 1, stacks: 1, value: 0f, sourceId: Def.id);
            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} for {dmg} damage and stuns {target.DisplayName}!");
        }
    }
}
