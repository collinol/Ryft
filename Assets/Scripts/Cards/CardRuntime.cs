using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Player;
using Game.Ryfts;
using Game.VFX;
using System.Linq;

namespace Game.Cards
{
    public abstract class CardRuntime
    {
        public CardDef Def { get; private set; }
        public IActor  Owner { get; private set; }
        public void Bind(CardDef def, IActor owner) { Def = def; Owner = owner; }

        public virtual TargetingType Targeting => Def ? Def.targeting : TargetingType.None;

        protected virtual int GetEnergyCost() => Def ? Mathf.Max(0, Def.energyCost) : 0;

        /// <summary>
        /// Returns the StatField this card scales from, derived from CardDef.statType.
        /// </summary>
        protected virtual StatField ScalingStat => Def == null ? StatField.Strength : Def.statType switch
        {
            CardStatType.Strength    => StatField.Strength,
            CardStatType.Intellect   => StatField.Intellect,
            CardStatType.Engineering => StatField.Engineering,
            _ => StatField.Strength
        };

        protected virtual int GetBasePower()  => Def ? Def.power   : 0;
        protected virtual int GetScaling()    => Def ? Def.scaling : 0;

        /// <summary>
        /// Returns the fully-modified power value for display on the card,
        /// accounting for stats, ryft effects, stance, overclock, etc.
        /// </summary>
        public virtual int GetDisplayPower()
        {
            if (Def == null) return 0;
            return Def.cardType switch
            {
                CardType.Attack => GetFinalDamage(),
                CardType.Defend => GetScaledProtection(),
                CardType.Heal   => GetScaledHeal(),
                _               => GetScaledPower()
            };
        }

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

        // ─── Stat helpers ───

        /// <summary>
        /// Gets the stat value this card scales from. Returns 0 if statType is None.
        /// </summary>
        protected int GetStatValue()
        {
            if (Def == null || Def.statType == CardStatType.None) return 0;
            return GetOwnerCurrentFor(ScalingStat);
        }

        /// <summary>
        /// Calculates scaled power: basePower + statValue * scaling.
        /// </summary>
        protected int GetScaledPower()
        {
            int stat = GetStatValue();
            return Mathf.Max(1, GetBasePower() + stat * GetScaling());
        }

        /// <summary>
        /// Calculates damage with stance/overclock/weapon modifiers applied.
        /// </summary>
        protected int GetFinalDamage(int baseDmg = -1)
        {
            int dmg = baseDmg >= 0 ? baseDmg : GetScaledPower();

            // Ryft passive: bonus attack damage for attack cards
            var mgr = RyftEffectManager.Ensure();
            if (Def != null && Def.cardType == CardType.Attack)
                dmg += mgr.SumInt(BuiltInOp.BonusAttackDamage);

            // Stance damage multiplier
            var stance = StanceManager.Instance;
            if (stance != null)
                dmg = Mathf.RoundToInt(dmg * stance.OutgoingDamageMultiplier);

            // Overclock damage bonus
            var oc = OverclockManager.Instance;
            if (oc != null)
                dmg = Mathf.RoundToInt(dmg * (1f + oc.DamageBonusPercent));

            // Weakness debuff on player
            if (Owner?.StatusEffects != null && Owner.StatusEffects.HasEffect(StatusEffectType.Weakness))
                dmg = Mathf.RoundToInt(dmg * 0.75f);

            // Ryft effect modifiers (one-hit multiplier)
            dmg = mgr.ApplyOutgoingDamageModifiers(dmg, Def, Owner, null);

            return Mathf.Max(1, dmg);
        }

        /// <summary>
        /// Calculates protection with stat scaling applied.
        /// </summary>
        protected int GetScaledProtection()
        {
            int prot = GetScaledPower();

            // Ryft passive: defend card bonus protection
            if (Def != null && Def.cardType == CardType.Defend)
            {
                var mgr = RyftEffectManager.Ensure();
                prot += mgr.SumInt(BuiltInOp.DefendCardBonusProtection);
            }

            return Mathf.Max(0, prot);
        }

        /// <summary>
        /// Calculates heal with stat scaling applied.
        /// </summary>
        protected int GetScaledHeal()
        {
            int heal = GetScaledPower();

            // Ryft passive: heal card bonus/penalty percent
            var mgr = RyftEffectManager.Ensure();
            float bonusPct = mgr.SumFloat(BuiltInOp.HealCardBonusPercent)
                           + mgr.SumFloat(BuiltInOp.HealCardPenaltyPercent);
            if (Mathf.Abs(bonusPct) > 0.001f)
                heal = Mathf.RoundToInt(heal * (1f + bonusPct));

            return Mathf.Max(0, heal);
        }

        protected int GetOwnerCurrentFor(StatField f)
        {
            if (Owner == null) return 0;

            int baseVal = f switch
            {
                StatField.Strength    => Owner.TotalStats.strength,
                StatField.Intellect   => Owner.TotalStats.intellect,
                StatField.Engineering => Owner.TotalStats.engineering,
                _ => 0
            };

            var mgr = RyftEffectManager.Ensure();
            int bonus = f switch
            {
                StatField.Strength    => mgr.BonusStrength + mgr.TempStrength,
                StatField.Intellect   => mgr.BonusIntellect + mgr.TempIntellect,
                StatField.Engineering => mgr.BonusEngineering + mgr.TempEngineering,
                _ => 0
            };

            return Mathf.Max(0, baseVal + bonus);
        }

        // ─── Damage / VFX helpers ───

        /// <summary>
        /// Apply damage and track if it killed the target. Returns true if target was killed.
        /// </summary>
        protected bool DealDamageTracked(IActor target, int damage)
        {
            if (target == null) return false;

            // Weapon installation bonus damage
            var weapon = WeaponInstallationManager.Instance;
            if (weapon != null && Def != null && Def.cardType == CardType.Attack)
            {
                damage += weapon.BonusFlatDamage;
                if (weapon.CurrentWeapon == WeaponType.Flamethrower)
                    CombatBuffManager.ApplyDebuff(target, StatusEffectType.Burning, 2);
                if (weapon.CurrentWeapon == WeaponType.CryoCannon)
                    CombatBuffManager.ApplyDebuff(target, StatusEffectType.Frozen, 2);
            }

            // Frozen bonus damage
            var frozenEffect = target.StatusEffects?.GetEffect(StatusEffectType.Frozen);
            if (frozenEffect != null && frozenEffect.Stacks > 0)
            {
                float bonusPercent = frozenEffect.Stacks * 0.02f;
                damage = Mathf.RoundToInt(damage * (1f + bonusPercent));
            }

            // Analyze Weakness bonus
            var analyzeEffect = target.StatusEffects?.GetEffect(StatusEffectType.AnalyzeWeakness);
            if (analyzeEffect != null)
            {
                damage = Mathf.RoundToInt(damage * 1.25f);
            }

            // VFX
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                var targetTransform = GetActorTransform(target);
                if (targetTransform != null)
                {
                    Color damageColor = GetDamageColor(ScalingStat);
                    vfx.PlayDamageEffect(targetTransform, damageColor, damage);
                }
            }

            bool wasAlive = target.IsAlive;
            target.ApplyDamage(damage, Owner);

            bool killed = wasAlive && !target.IsAlive;
            if (killed)
            {
                var tracker = CombatEventTracker.Instance;
                tracker?.RegisterKill(Owner, target, damage, ScalingStat);
            }

            return killed;
        }

        /// <summary>
        /// Deal damage to a single target (backward compat wrapper).
        /// </summary>
        protected void DealDamage(IActor target, int damage, StatField damageSource)
        {
            DealDamageTracked(target, damage);
        }

        /// <summary>
        /// Deal damage to all alive enemies. Returns number of kills.
        /// </summary>
        protected int DealDamageToAll(FightContext ctx, int damage)
        {
            int kills = 0;
            foreach (var e in ctx.AllAliveEnemies().ToList())
            {
                if (DealDamageTracked(e, damage)) kills++;
            }
            return kills;
        }

        /// <summary>
        /// Handles Momentum keyword: on kill, gain 1 energy + draw 1 card.
        /// </summary>
        protected void HandleMomentum(bool killed)
        {
            if (!killed || Def == null || !Def.HasKeyword(CardKeyword.Momentum)) return;
            var fsc = FightSceneController.Instance;
            if (fsc == null) return;
            int bonusEnergy = RyftEffectManager.Ensure().SumInt(BuiltInOp.KeywordBonusEnergy);
            fsc.GainEnergy(1 + bonusEnergy);
            fsc.DrawCards(1);
            Debug.Log($"[Momentum] Kill! +{1 + bonusEnergy} energy, +1 card.");
        }

        /// <summary>
        /// Handles Extract keyword: on kill, refund energy cost + draw cards = consumed stacks.
        /// </summary>
        protected void HandleExtract(bool killed, int consumedStacks)
        {
            if (!killed || Def == null || !Def.HasKeyword(CardKeyword.Extract)) return;
            var fsc = FightSceneController.Instance;
            if (fsc == null) return;
            int bonusEnergy = RyftEffectManager.Ensure().SumInt(BuiltInOp.KeywordBonusEnergy);
            fsc.GainEnergy(Def.energyCost + bonusEnergy);
            if (consumedStacks > 0) fsc.DrawCards(consumedStacks);
            Debug.Log($"[Extract] Kill! Refund {Def.energyCost + bonusEnergy} energy, draw {consumedStacks} cards.");
        }

        /// <summary>
        /// Handles Rewind keyword: on kill, take extra turn.
        /// </summary>
        protected void HandleRewind(bool killed)
        {
            if (!killed || Def == null || !Def.HasKeyword(CardKeyword.Rewind)) return;
            var fsc = FightSceneController.Instance;
            if (fsc == null) return;
            fsc.QueueExtraTurn();
            Debug.Log("[Rewind] Kill! Extra turn queued.");
        }

        /// <summary>
        /// Gain protection for the player.
        /// </summary>
        protected void GainProtection(int amount)
        {
            if (Owner == null) return;
            CombatBuffManager.GainProtection(Owner, amount);
        }

        /// <summary>
        /// Play projectile VFX from owner to target.
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

        protected void PlayHealEffect(IActor target, int amount)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;
            var targetTransform = GetActorTransform(target);
            if (targetTransform != null) vfx.PlayHealEffect(targetTransform, amount);
        }

        protected void PlayAreaEffect(Vector3 position, float radius, StatField damageSource)
        {
            var vfx = CardVFXManager.Instance;
            if (vfx == null) return;
            Color color = GetDamageColor(damageSource);
            vfx.PlayAreaEffect(position, radius, color);
        }

        // ─── Utility ───

        private Transform GetActorTransform(IActor actor)
        {
            if (actor == null) return null;
            if (actor is MonoBehaviour mono) return mono.transform;
            return null;
        }

        private Color GetDamageColor(StatField damageSource)
        {
            return damageSource switch
            {
                StatField.Strength => VFXColors.Physical,
                StatField.Intellect => VFXColors.Magic,
                StatField.Engineering => VFXColors.Engineering,
                StatField.Energy => VFXColors.Energy,
                _ => Color.white
            };
        }
    }
}
