using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Abilities
{
    public class MedKitAbility : AbilityRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;

            var healTarget = Owner;
            int baseHeal = Mathf.Max(1, Def.power + Owner.TotalStats.defense * Def.scaling);

            // (If you later add incoming-heal multipliers, apply them here)
            int before  = healTarget.Health;
            healTarget.Heal(baseHeal);
            int applied = healTarget.Health - before;

            ctx.Log($"{Owner.DisplayName} uses {Def.displayName} and heals {applied}.");

            if (RyftEffectManager.Instance && RyftEffectManager.Instance.verboseRyftLogs)
            {
                Debug.Log($"[Ryft][HEAL] target={healTarget.DisplayName} before={before} +{applied} => {healTarget.Health}");
            }

            if (!FightSceneController.Instance || !FightSceneController.Instance.IsFreeCast)
                PutOnCooldown();
        }
    }
}