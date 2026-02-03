using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// Energy Leech - Deal 3 damage and gain +1 Engineering.
    /// </summary>
    public class EnergyLeech : CardRuntime
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

            var player = Owner as PlayerCharacter;
            if (player != null)
            {
                player.Gain(new Stats { engineering = 1 }, allowExceedCap: true);
            }
            ctx.Log($"{Owner.DisplayName} leeches energy for {dmg} damage and gains +1 Engineering this turn!");
        }
    }
}
