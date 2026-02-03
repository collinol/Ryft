using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Abilities;
using Game.Abilities.Enemy;
using Game.UI;
using Game.Combat;
namespace Game.Enemies
{
    public abstract class EnemyBase : MonoBehaviour, IActor
    {
        [Header("Identity")]
        [SerializeField] protected string displayName = "Enemy";

        [Header("Stats")]
        [SerializeField] protected Stats baseStats = new Stats { maxHealth = 10, strength = 2 };

        public string DisplayName => displayName;
        public Stats   BaseStats  => baseStats;
        public Stats   TotalStats => baseStats;
        public int     Health     { get; private set; }
        public bool    IsAlive    => Health > 0;
        public StatusEffectManager StatusEffects { get; private set; }

        private HealthBarView hpBar;

        [Header("Enemy Ability IDs (from EnemyAbilityDatabase)")]
        [Tooltip("IDs must match AbilityDef.id values in EnemyAbilityDatabase (e.g., \"EnemyStrike\").")]
        public string[] abilityIds;

        private List<AbilityRuntime> _abilityRuntimes;

        protected virtual void Awake()
        {
            // If a subclass changes stats, do that BEFORE base.Awake()
            Health = Mathf.Max(1, baseStats.maxHealth);

            // Initialize status effects
            StatusEffects = new StatusEffectManager(this);

            // Create and snap the bar (positioned below the enemy sprite, sized to fit within sprite width)
            hpBar = HealthBarView.Attach(transform, new Vector3(0f, -1.2f, 0f), new Vector2(0.5f, 0.08f));
            hpBar.Set(Health, TotalStats.maxHealth);
        }

        // IActor
        public void ApplyDamage(int amount)
        {
            ApplyDamage(amount, null);
        }

        public void ApplyDamage(int amount, IActor attacker)
        {
            // Apply status effect modifiers
            var (finalDamage, blocked, reflected) = StatusEffects.ApplyIncomingDamageModifiers(amount, attacker);

            if (blocked)
            {
                Debug.Log($"[{DisplayName}] Attack blocked!");
                return;
            }

            var mitigated = Mathf.Max(0, finalDamage);
            bool wasAlive = IsAlive;
            Health = Mathf.Max(0, Health - mitigated);
            hpBar?.Set(Health, TotalStats.maxHealth);

            // Track kill if this damage killed the enemy
            if (wasAlive && !IsAlive)
            {
                var tracker = CombatEventTracker.Instance;
                // We don't know who dealt the damage here, so we'll need to track it differently
                // This will be handled at the card level

                // Hide sprite and health bar when enemy dies
                OnDeath();
            }
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(TotalStats.maxHealth, Health + Mathf.Max(0, amount));
            hpBar?.Set(Health, TotalStats.maxHealth);
        }

        // Enemy abilities only
        public List<AbilityRuntime> EnsureEnemyAbilityRuntimes(EnemyAbilityDatabase db)
        {
            if (_abilityRuntimes != null) return _abilityRuntimes;
            _abilityRuntimes = new List<AbilityRuntime>();
            if (db == null || abilityIds == null || abilityIds.Length == 0) return _abilityRuntimes;

            foreach (var id in abilityIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                var rt = db.CreateRuntime(id, this);
                if (rt != null) _abilityRuntimes.Add(rt);
            }
            return _abilityRuntimes;
        }

        public virtual AbilityRuntime PickRandomEnemyAbilityRuntime()
        {
            if (_abilityRuntimes == null || _abilityRuntimes.Count == 0) return null;
            return _abilityRuntimes[Random.Range(0, _abilityRuntimes.Count)];
        }
        public virtual void PerformEnemyAction(FightContext ctx, EnemyAbilityDatabase db)
        {
            if (!IsAlive) return;
            if (db == null)
            {
                Debug.LogWarning($"[{DisplayName}] EnemyAbilityDatabase missing.");
                return;
            }

            // Check for Stun - cannot act
            if (StatusEffects.HasEffect(StatusEffectType.Stun))
            {
                ctx.Log($"{DisplayName} is stunned and cannot act!");
                Debug.Log($"[{DisplayName}] is stunned, skipping action");
                return;
            }

            // Check for Slow - reduced actions (for now, skip every other turn when slowed)
            if (StatusEffects.HasEffect(StatusEffectType.Slow))
            {
                var slowEffect = StatusEffects.GetEffect(StatusEffectType.Slow);
                if (slowEffect != null && slowEffect.Value > 0)
                {
                    // Slow reduces action count - for now, we'll make them skip this turn
                    ctx.Log($"{DisplayName} is slowed and moves sluggishly...");
                    Debug.Log($"[{DisplayName}] is slowed, reduced effectiveness");
                    // Still let them act, but we could reduce damage in the ability itself
                }
            }

            // Choose which ability to use this turn
            var id = PickEnemyAbilityId();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[{DisplayName}] has no ability IDs.");
                return;
            }

            // Create a NEW runtime instance from DB each time
            var rt = db.CreateRuntime(id, this);
            if (rt == null)
            {
                Debug.LogWarning($"[{DisplayName}] couldn't create runtime for '{id}'.");
                return;
            }

            // Target the rift portal if present, otherwise the player
            var target = ctx.GetEnemyPrimaryTarget();
            rt.Execute(ctx, target);
        }
        protected virtual string PickEnemyAbilityId()
        {
            if (abilityIds == null || abilityIds.Length == 0) return null;
            // Randomize if you want:
            // return abilityIds[UnityEngine.Random.Range(0, abilityIds.Length)];
            return abilityIds[0];
        }

        protected virtual void OnDeath()
        {
            Debug.Log($"[{DisplayName}] Died - hiding sprite and health bar");

            // Hide sprite
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.enabled = false;
            }

            // Hide health bar
            if (hpBar)
            {
                hpBar.gameObject.SetActive(false);
            }

            // Notify FightSceneController to check if all enemies are dead
            var fightController = FindObjectOfType<Game.Combat.FightSceneController>();
            if (fightController)
            {
                fightController.CheckVictoryCondition();
            }
        }
    }
}
