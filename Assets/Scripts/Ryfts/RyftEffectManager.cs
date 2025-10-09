using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Abilities;
using Game.Player;
using Game.Combat;

namespace Game.Ryfts
{
    public class RyftEffectManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] public bool verboseRyftLogs = true;
        private float nextOutgoingDamageMult = 1f;
        private string nextOutgoingDamageTag = null;
        private RyftEffectRuntime nextOutgoingDamageSource = null;

        public static RyftEffectManager Instance { get; private set; }

        [SerializeField] private PlayerCharacter player;             // assign at runtime in fights if null
        [SerializeField] private List<RyftEffectRuntime> active = new();

        // Persisted additive bonuses
        private int bonusMaxHp, bonusStr, bonusDef;
        public int BonusMaxHp => bonusMaxHp;
        public int BonusStrength => bonusStr;
        public int BonusDefense => bonusDef;

        // temp flags for per-turn hooks
        private bool nextAttackIgnoresDefense;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Subscribe();
        }

        void OnDestroy()
        {
            if (Instance == this) Unsubscribe();
        }

        private void Subscribe()
        {
            RyftCombatEvents.OnBattleStart      += OnBattleStart;
            RyftCombatEvents.OnBattleEnd        += OnBattleEnd;
            RyftCombatEvents.OnTurnStart        += OnTurnStart;
            RyftCombatEvents.OnTurnEnd          += OnTurnEnd;
            RyftCombatEvents.OnAbilityUsed      += OnAbilityUsed;
            RyftCombatEvents.OnAbilityResolved  += OnAbilityResolved;
            RyftCombatEvents.OnDamageDealt      += OnDamageDealt;
            RyftCombatEvents.OnDamageTaken      += OnDamageTaken;
            RyftCombatEvents.OnEnemyDefeated    += OnEnemyDefeated;
        }

        private void Unsubscribe()
        {
            RyftCombatEvents.OnBattleStart      -= OnBattleStart;
            RyftCombatEvents.OnBattleEnd        -= OnBattleEnd;
            RyftCombatEvents.OnTurnStart        -= OnTurnStart;
            RyftCombatEvents.OnTurnEnd          -= OnTurnEnd;
            RyftCombatEvents.OnAbilityUsed      -= OnAbilityUsed;
            RyftCombatEvents.OnAbilityResolved  -= OnAbilityResolved;
            RyftCombatEvents.OnDamageDealt      -= OnDamageDealt;
            RyftCombatEvents.OnDamageTaken      -= OnDamageTaken;
            RyftCombatEvents.OnEnemyDefeated    -= OnEnemyDefeated;
        }

        // --- Public API ----------------------------------------------------

        public IActor PlayerActor
        {
            get { EnsurePlayerRef(); return player; }
        }

        public bool IsPlayer(IActor actor)
        {
            EnsurePlayerRef();
            return actor != null && ReferenceEquals(actor, player);
        }

        public void EnsurePlayerRef()
        {
            if (!player) player = FindObjectOfType<PlayerCharacter>();
        }

        public static RyftEffectManager Ensure()
        {
            if (Instance) return Instance;
            var go = new GameObject("RyftEffectManager");
            return go.AddComponent<RyftEffectManager>();
        }

        public void PlayerPermanentStatsDelta(int maxHp = 0, int strength = 0, int defense = 0)
        {
            bonusMaxHp += maxHp;
            bonusStr   += strength;
            bonusDef   += defense;

            EnsurePlayerRef();
            if (player != null)
            {
                // If you later expose TotalStats with modifiers, apply them there.
                // For now, reflect max HP bumps as immediate healing so the benefit is felt.
                if (maxHp > 0) player.Heal(maxHp);
            }
        }

        public void AddEffect(RyftEffectDef def)
        {
            if (!def) return;

            // find existing effect with same ID
            var existing = active.FirstOrDefault(r => r != null && r.Def != null && r.Def.id == def.id);
            if (existing != null)
            {
                existing.AddStack(1, refreshDuration: true);
                Debug.Log($"[Ryft] Stacked {def.id} -> stacks={existing.stacks}, chance={existing.CurrentProcPercent:0.#}%");
                return;
            }

            var rt = def.CreateRuntime(); // ensure this calls runtime.Bind(def)
            active.Add(rt);
            rt.OnAdded(this);
            Debug.Log($"[Ryft] Added {def.id} -> stacks={rt.stacks}, chance={rt.CurrentProcPercent:0.#}%");
        }

        public void RemoveEffect(RyftEffectRuntime rt)
        {
            if (rt == null) return;
            rt.OnRemoved(this);
            active.Remove(rt);
        }

        public void ClearBattleScoped()
        {
            // remove UntilBattleEnd and DurationNTurns effects between scenes
            var toRemove = active.Where(a => a.Def.lifetime == EffectLifetime.UntilBattleEnd
                                          || a.Def.lifetime == EffectLifetime.DurationNTurns).ToList();
            foreach (var r in toRemove) RemoveEffect(r);
        }

        // Map interactions (call when ryft is closed/exploded)
        public void OnRyftOutcome(RyftEffectDef def)
        {
            // polarity is encoded in the def (you’ll have distinct POS/NEG defs)
            AddEffect(def);
        }

        // --- Utilities invoked by built-in/custom effects ------------------
        public void TryDoubleCast(RyftEffectContext ctx)
        {
            if (ctx?.fight == null || ctx.abilityDef == null) return;
            var fsc = Object.FindObjectOfType<FightSceneController>();
            if (!fsc) return;

            fsc.UsePlayerAbility(ctx.abilityDef.id, freeCast: true);
        }

        public void TryResetCooldownOf(AbilityDef def)
        {
            if (!def) return;
            var fsc = Object.FindObjectOfType<FightSceneController>();
            //fsc?.ResetCooldown(def.id);
        }

        public void ReduceAllCooldownsBy(int v)
        {
            var fsc = Object.FindObjectOfType<FightSceneController>();
            //fsc?.ReduceAllCooldowns(v);
        }

        public void ApplyBarrierPercentToPlayer(float percent)
        {
            EnsurePlayerRef();
            if (!player) return;
            int amount = Mathf.RoundToInt(Mathf.Max(0, player.TotalStats.maxHealth) * Mathf.Clamp01(percent));
            // If you have a barrier system, hook into it; fallback is a temporary heal.
            player.Heal(amount);
        }

        public void HealPlayer(int amount)
        {
            EnsurePlayerRef();
            player?.Heal(Mathf.Max(0, amount));
        }

        public void FlagNextPlayerAttackIgnoreDefense() => nextAttackIgnoresDefense = true;
        public bool ConsumeIgnoreDefenseFlagIfAny()
        {
            if (!nextAttackIgnoresDefense) return false;
            nextAttackIgnoresDefense = false;
            return true;
        }

        public void TryExecute(IActor target, IActor source = null)
        {
            if (target == null || !target.IsAlive) return;

            // Nuke via ApplyDamage so all standard clamping runs
            int before = target.Health;
            target.ApplyDamage(int.MaxValue / 2); // big number, ensures kill through defense
            int dealt = Mathf.Max(0, before - target.Health);

            // Let listeners know what happened (if we know who caused it)
            if (source != null) RyftCombatEvents.RaiseDamageDealt(source, target, dealt);
            if (!target.IsAlive) RyftCombatEvents.RaiseEnemyDefeated(target);
        }

        public void TryAddCooldown(AbilityDef def, int delta)
        {
            if (!def) return;
            var fsc = Object.FindObjectOfType<FightSceneController>();
            // Implement a method on FightSceneController like: public void AddCooldown(string id, int delta)
            fsc?.AddCooldown(def.id, delta);
        }
        /*
        to modify outgoing damage, in the ability itself use mgr.AppslyOutgoingDamageModifiers(dmg);
        and in the ryft effect script, set the modifier amount with SetNextOutgoingDamageMultiplier
        */
        public void SetNextOutgoingDamageMultiplier(float mult, string tag = null, RyftEffectRuntime src = null)
        {
            nextOutgoingDamageMult = Mathf.Clamp(nextOutgoingDamageMult * mult, 0f, 2f);
            nextOutgoingDamageTag  = string.IsNullOrEmpty(tag) && src?.Def != null ? src.Def.id : tag;
            nextOutgoingDamageSource = src;

            if (verboseRyftLogs)
                Debug.Log($"[Ryft][ARM] NextHit mult={nextOutgoingDamageMult} from {(nextOutgoingDamageTag ?? "(unknown)")}");
        }

        public int ApplyOutgoingDamageModifiers(
            int baseDamage,
            AbilityDef ability = null,
            IActor attacker = null,
            IActor target = null)
        {
            int result = Mathf.Max(0, Mathf.RoundToInt(baseDamage * nextOutgoingDamageMult));

            if (verboseRyftLogs)
            {
                string abil = ability ? ability.id : "(n/a)";
                string atk  = attacker != null ? attacker.DisplayName : "(n/a)";
                string tgt  = target   != null ? target.DisplayName   : "(n/a)";
                float mult  = nextOutgoingDamageMult;
                string tag  = nextOutgoingDamageTag ?? "(none)";

                Debug.Log($"[Ryft][DMG] ability={abil} attacker={atk} target={tgt} base={baseDamage} mult={mult:0.###} src={tag} => final={result}");
            }

            // consume once
            nextOutgoingDamageMult = 1f;
            nextOutgoingDamageTag  = null;
            nextOutgoingDamageSource = null;

            return result;
        }



        // --- Event fan-out to active effects -------------------------------
        private void Broadcast(RyftTrigger t, FightContext fight = null, IActor src = null, IActor tgt = null, AbilityDef a = null, int amt = 0)
        {
            if (active.Count == 0) return;
            var ctx = new RyftEffectContext { fight = fight, source = src, target = tgt, abilityDef = a, amount = amt, trigger = t };
            var snapshot = active.ToArray(); // avoid modification during iteration
            foreach (var e in snapshot) e?.HandleTrigger(this, ctx);
        }

        private void OnBattleStart(FightContext c)     { EnsurePlayerRef(); Broadcast(RyftTrigger.OnBattleStart, fight:c); }
        private void OnBattleEnd()                     { Broadcast(RyftTrigger.OnBattleEnd); ClearBattleScoped(); }
        private void OnTurnStart()                     { Broadcast(RyftTrigger.OnTurnStart); }
        private void OnTurnEnd()                       { Broadcast(RyftTrigger.OnTurnEnd); }
        private void OnAbilityUsed(Game.Core.IActor s, AbilityDef a, FightContext c)     { Broadcast(RyftTrigger.OnAbilityUsed, fight:c, src:s, a:a); }
        private void OnAbilityResolved(Game.Core.IActor s, AbilityDef a, FightContext c) { Broadcast(RyftTrigger.OnAbilityResolved, fight:c, src:s, a:a); }
        private void OnDamageDealt(Game.Core.IActor s, Game.Core.IActor t, int dmg) { Broadcast(RyftTrigger.OnDamageDealt, src:s, tgt:t, amt:dmg); }
        private void OnDamageTaken(Game.Core.IActor t, int dmg)                     { Broadcast(RyftTrigger.OnDamageTaken, tgt:t, amt:dmg); }
        private void OnEnemyDefeated(Game.Core.IActor e)                            { Broadcast(RyftTrigger.OnEnemyDefeated, tgt:e); }

        /* ---- Logging / Debug ---- */
        public IReadOnlyList<RyftEffectRuntime> ActiveEffects => active;

        public void DebugLogActiveEffects(string tag = "[Ryft]")
        {
            if (active == null || active.Count == 0)
            {
                Debug.Log($"{tag} Active effects: (none)");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{tag} Active effects ({active.Count}):");
            foreach (var r in active)
            {
                if (r == null || r.Def == null) continue;
                sb.Append(" • ")
                  .Append(r.Def.id).Append(" — ").Append(r.Def.displayName)
                  .Append("  [").Append(r.Def.color).Append(" / ").Append(r.Def.polarity).Append("]")
                  .Append("  Chance: ").Append(r.CurrentProcPercent.ToString("0.#")).Append("%")
                  .Append("  ICD: ").Append(r.internalCdRemaining).Append("/").Append(r.Def.internalCooldownTurns)
                  .Append("  Stacks: ").Append(r.stacks)
                  .Append("  Lifetime: ").Append(r.Def.lifetime);
                if (r.Def.intMagnitude != 0) sb.Append("  int=").Append(r.Def.intMagnitude);
                if (Mathf.Abs(r.Def.floatMagnitude) > 0.0001f) sb.Append("  float=").Append(r.Def.floatMagnitude.ToString("0.###"));
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }
        public void DebugLogEffectAction(string action, string detail)
        {
            if (verboseRyftLogs)
                Debug.Log($"[Ryft][{action}] {detail}");
        }
    }
}
