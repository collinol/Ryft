using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;
using System.Linq;

namespace Game.Cards
{
    /// <summary>
    /// Chain Lightning - Hit up to 3 enemies for 3 damage each.
    /// </summary>
    public class ChainLightning : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Mana;
        protected override int GetBasePower() => 3;
        protected override int GetScaling() => 1;
        public override TargetingType Targeting => TargetingType.AllEnemies;

        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int stat = GetOwnerCurrentFor(ScalingStat);
            int dmg = Mathf.Max(1, GetBasePower() + stat * GetScaling());
            var mgr = RyftEffectManager.Ensure();

            var victims = ctx.AllAliveEnemies().Take(3).ToList();
            foreach (var enemy in victims)
            {
                int finalDmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, enemy);
                DealDamage(enemy, finalDmg, ScalingStat);
            }

            ctx.Log($"{Owner.DisplayName} casts Chain Lightning, hitting {victims.Count} enemies for {dmg} each!");
        }
    }
}
