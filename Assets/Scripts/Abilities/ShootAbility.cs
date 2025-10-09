using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Abilities
{
    // Simple: damage first alive enemy or the passed explicit target
    public class ShootAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            var attacker = Owner;

            var strength = attacker.TotalStats.strength;
            var dmg = Mathf.Max(1, Def.power + strength * Def.scaling);
            var mgr = RyftEffectManager.Instance;
            if (mgr) dmg = mgr.ApplyOutgoingDamageModifiers(dmg);

            // capture pre-HP to compute *actual* damage dealt
            int before = target.Health;

            target.ApplyDamage(dmg);
            int dealt = Mathf.Max(0, before - target.Health);

            // raise “damage dealt” with the mitigated/clamped amount
            RyftCombatEvents.RaiseDamageDealt(attacker, target, dealt);

            // if this ability always targets enemies, also signal defeats here
            if (!target.IsAlive)
                RyftCombatEvents.RaiseEnemyDefeated(target);

            ctx.Log($"{attacker.DisplayName} uses {Def.displayName} for {dmg} damage on {target.DisplayName}.");

            if (!FightSceneController.Instance || !FightSceneController.Instance.IsFreeCast)
                PutOnCooldown();
        }
    }
}
