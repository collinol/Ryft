using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Player;
using Game.Ryfts;
using Game.VFX;

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

        /// <summary>
        /// Apply damage and track if it killed the target
        /// </summary>
        protected void DealDamage(IActor target, int damage, StatField damageSource)
        {
            if (target == null) return;

            // Play VFX
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                var targetTransform = GetActorTransform(target);
                if (targetTransform != null)
                {
                    Color damageColor = GetDamageColor(damageSource);
                    vfx.PlayDamageEffect(targetTransform, damageColor, damage);
                }
            }

            bool wasAlive = target.IsAlive;
            target.ApplyDamage(damage);

            // Track kill
            if (wasAlive && !target.IsAlive)
            {
                var tracker = CombatEventTracker.Instance;
                tracker?.RegisterKill(Owner, target, damage, damageSource);
            }
        }

        /// <summary>
        /// Play a projectile effect from owner to target
        /// </summary>
        protected void PlayProjectile(IActor target, StatField damageSource, System.Action onHit = null)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;

            var sourceTransform = GetActorTransform(Owner);
            var targetTransform = GetActorTransform(target);

            if (sourceTransform != null && targetTransform != null)
            {
                Color color = GetDamageColor(damageSource);
                vfx.PlayProjectileEffect(sourceTransform, targetTransform, color, onHit);
            }
        }

        /// <summary>
        /// Play a buff effect on a target
        /// </summary>
        protected void PlayBuffEffect(IActor target, StatField statType)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;

            var targetTransform = GetActorTransform(target);
            if (targetTransform != null)
            {
                Color color = GetDamageColor(statType);
                vfx.PlayBuffEffect(targetTransform, color);
            }
        }

        /// <summary>
        /// Play a heal effect on a target
        /// </summary>
        protected void PlayHealEffect(IActor target, int amount)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;

            var targetTransform = GetActorTransform(target);
            if (targetTransform != null)
            {
                vfx.PlayHealEffect(targetTransform, amount);
            }
        }

        /// <summary>
        /// Play an area effect at a position
        /// </summary>
        protected void PlayAreaEffect(Vector3 position, float radius, StatField damageSource)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;

            Color color = GetDamageColor(damageSource);
            vfx.PlayAreaEffect(position, radius, color);
        }

        // Helper methods
        private Transform GetActorTransform(IActor actor)
        {
            if (actor == null) return null;

            // Try casting to MonoBehaviour types
            if (actor is MonoBehaviour mono)
                return mono.transform;

            return null;
        }

        private Color GetDamageColor(StatField damageSource)
        {
            return damageSource switch
            {
                StatField.Strength => VFXColors.Physical,
                StatField.Mana => VFXColors.Magic,
                StatField.Engineering => VFXColors.Engineering,
                StatField.Energy => VFXColors.Energy,
                _ => Color.white
            };
        }
    }
}
