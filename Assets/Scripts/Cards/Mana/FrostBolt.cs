using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Frost Bolt - Deal 4 damage and apply Slow (-1 enemy action).
    /// </summary>
    public class FrostBolt : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 4;
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

            // Play projectile effect, then deal damage when it hits
            PlayProjectile(target, ScalingStat, () => {
                DealDamage(target, dmg, ScalingStat);
            });

            ctx.Log($"{Owner.DisplayName} casts {Def.displayName} for {dmg} damage and slows {target.DisplayName}!");
            target.StatusEffects.AddEffect(StatusEffectType.Slow, duration: 1, stacks: 1, value: 1f, sourceId: Def.id);
        }
    }
}
