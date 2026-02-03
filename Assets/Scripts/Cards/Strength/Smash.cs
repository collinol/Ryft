using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Smash - Deal 8 damage and knock enemy back (skip next turn).
    /// </summary>
    public class Smash : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 8;
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

            ctx.Log($"{Owner.DisplayName} smashes {target.DisplayName} for {dmg} damage and knocks them back!");
            target.StatusEffects.AddEffect(StatusEffectType.Stun, duration: 1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
