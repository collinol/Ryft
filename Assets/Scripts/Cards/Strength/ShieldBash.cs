using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Shield Bash - Deal 3 damage and gain +2 Defense for 1 turn.
    /// </summary>
    public class ShieldBash : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
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

            Owner.StatusEffects.AddEffect(StatusEffectType.DefenseUp, duration: 1, stacks: 1, value: 2f, sourceId: Def.id);
            ctx.Log($"{Owner.DisplayName} shield bashes for {dmg} damage and gains +2 Defense!");
        }
    }
}
