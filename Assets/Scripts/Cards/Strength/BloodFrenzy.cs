using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Ryfts;

namespace Game.Cards
{
    /// <summary>
    /// Blood Frenzy - Deal 2 damage and heal 2. If you overheal, draw 1 card.
    /// With enough damage/heal buffs, this sustains endless draws.
    /// </summary>
    public class BloodFrenzy : CardRuntime
    {
        protected override StatField ScalingStat => StatField.Strength;
        protected override int GetBasePower() => 2;
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

            // Deal damage with VFX
            DealDamage(target, dmg, ScalingStat);

            int hpBefore = Owner.Health;
            Owner.Heal(2);

            // Play heal effect
            PlayHealEffect(Owner, 2);

            int hpAfter = Owner.Health;

            bool overhealed = (hpBefore == Owner.TotalStats.maxHealth);
            if (overhealed)
            {
                ctx.Log($"{Owner.DisplayName} uses Blood Frenzy and overheals! Drawing 1 card.");
                FightSceneController.Instance?.DrawCards(1);
            }
            else
            {
                ctx.Log($"{Owner.DisplayName} uses Blood Frenzy for {dmg} damage and heals 2 HP.");
            }
        }
    }
}
