using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Player;
using Game.Ryfts;

namespace Game.Cards
{
    public abstract class CardRuntime
    {
        // Which resource this card spends (override in each card)
        protected virtual StatField CostField => StatField.Strength;
        protected virtual int GetBaseCostAmount(StatField field) => 0;
        protected virtual int GetBasePower()  => Def ? Def.power   : 0;
        protected virtual int GetScaling()    => Def ? Def.scaling : 0;
        public virtual TargetingType Targeting => Def ? Def.targeting : TargetingType.None;


        public CardDef Def { get; private set; }
        public IActor  Owner { get; private set; }
        public void Bind(CardDef def, IActor owner) { Def = def; Owner = owner; }



        public virtual bool CanUse(FightContext ctx)
        {
            if (Owner == null || !Owner.IsAlive) return false;
            if (Owner is PlayerCharacter pc)
            {
                int need = Mathf.Max(0, GetBaseCostAmount(CostField));
                var single = default(Stats);
                StatsUtil.Set(ref single, CostField, need);
                return pc.CanPay(single);
            }
            return true;
        }

        public abstract void Execute(FightContext ctx, IActor explicitTarget = null);

        protected virtual bool TryPayCost() => TryPayCostFor(CostField);

        protected bool TryPayCostFor(StatField field)
        {
            if (Owner is not PlayerCharacter pc) return true;

            var mgr = RyftEffectManager.Ensure();

            int baseCallCost = Mathf.Max(0, GetBaseCostAmount(field));
            int finalCost    = mgr.ApplyCallCostForField(baseCallCost, field, autoUseCredits: true);

            var toPay = default(Stats);
            StatsUtil.Set(ref toPay, field, finalCost);

            mgr.RecordLastPayment(field, finalCost);

            if (!pc.CanPay(toPay)) return false;
            pc.Pay(toPay);
            return true;
        }
        protected int GetOwnerMaxFor(StatField f)
        {
            var t = Owner.TotalStats;
            return f switch
            {
                StatField.Strength    => t.strength,
                StatField.Mana        => t.mana,
                StatField.Engineering => t.engineering,
                _ => 0
            };
        }
    }
}
