using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Shock Trap - Deal 3 damage; enemy loses next turn.
    /// </summary>
    public class ShockTrap : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Engineering;
        protected override int GetBasePower() => 3;
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

            ctx.Log($"{Owner.DisplayName} deploys Shock Trap! {target.DisplayName} takes {dmg} damage and loses next turn.");
            target.StatusEffects.AddEffect(StatusEffectType.Stun, duration: 1, stacks: 1, value: 0f, sourceId: Def.id);
        }
    }
}
