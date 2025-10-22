using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Player;
using Game.Ryfts;

namespace Game.Cards
{
    public abstract class CardRuntime
    {
        // Which stat this card *scales* from (not a resource to spend)
        protected abstract StatField ScalingStat { get; }

        protected virtual int GetBasePower()  => Def ? Def.power   : 0;
        protected virtual int GetScaling()    => Def ? Def.scaling : 0;
        public virtual TargetingType Targeting => Def ? Def.targeting : TargetingType.None;

        public CardDef Def { get; private set; }
        public IActor  Owner { get; private set; }
        public void Bind(CardDef def, IActor owner) { Def = def; Owner = owner; }

        protected virtual int GetEnergyCost() => Def ? Mathf.Max(0, Def.energyCost) : 0;

        public virtual bool CanUse(FightContext ctx)
        {
            if (Owner == null || !Owner.IsAlive || Def == null) return false;
            var fsc = FightSceneController.Instance;
            if (!fsc) return false;
            return fsc.CanAffordEnergy(GetEnergyCost());
        }

        protected bool TryPayEnergy(int? overrideCost = null)
        {
            var fsc = FightSceneController.Instance;
            if (!fsc) return true;
            return fsc.TrySpendEnergy(Mathf.Max(0, overrideCost ?? GetEnergyCost()));
        }

        public abstract void Execute(FightContext ctx, IActor explicitTarget = null);


        /// <summary>
        /// Current (buffed) value of the scaling stat: base + persistent bonuses + temporary battle deltas.
        /// </summary>
        protected int GetOwnerCurrentFor(StatField f)
        {
            if (Owner == null) return 0;

            int baseVal = f switch
            {
                StatField.Strength    => Owner.TotalStats.strength,
                StatField.Mana        => Owner.TotalStats.mana,
                StatField.Engineering => Owner.TotalStats.engineering,
                _ => 0
            };

            var mgr = RyftEffectManager.Ensure();
            int bonus = f switch
            {
                StatField.Strength    => mgr.BonusStrength + mgr.TempStrength,
                StatField.Mana        => mgr.BonusMana     + mgr.TempMana,
                StatField.Engineering => mgr.BonusEngineering + mgr.TempEngineering,
                _ => 0
            };

            return Mathf.Max(0, baseVal + bonus);
        }
    }
}
