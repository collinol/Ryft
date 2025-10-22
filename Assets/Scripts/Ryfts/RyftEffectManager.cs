using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Player;
using Game.Combat;
using Game.Cards;

namespace Game.Ryfts
{
    /// Central place to track active ryft effects and a lightweight "call cost" economy
    /// used by Blue effects (refunds/surcharges). There is deliberately no concept of
    /// ability cooldowns here anymore.
    public class RyftEffectManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] public bool verboseRyftLogs = true;

        // --- One-hit damage modifier plumbing (unchanged) ---
        private float  nextOutgoingDamageMult = 1f;
        private string nextOutgoingDamageTag  = null;
        private RyftEffectRuntime nextOutgoingDamageSource = null;

        public static RyftEffectManager Instance { get; private set; }

        [SerializeField] private PlayerCharacter player;             // assign at runtime in fights if null
        [SerializeField] private List<RyftEffectRuntime> active = new();

        // Persisted additive bonuses (meta buffs)
        private int bonusMaxHp, bonusStr, bonusDef, bonusEng, bonusMana;
        public int BonusMaxHp   => bonusMaxHp;
        public int BonusStrength=> bonusStr;
        public int BonusMana=> bonusMana;
        public int BonusDefense => bonusDef;
        public int BonusEngineering => bonusEng;

        // Battle-scoped temporary deltas
        private int tempMaxHp, tempStr, tempDef, tempMana, tempEng;
        public int TempMaxHp    => tempMaxHp;
        public int TempStrength => tempStr;
        public int TempDefense  => tempDef;
        public int TempEngineering  => tempEng;
        public int TempMana  => tempMana;

        // --------- Call-cost economy ----------
        private int callCredits;
        private int nextCallCostDelta;

        // Last payment tracking (field + amount) for refunds
        private Game.Core.StatField lastPaidField = Game.Core.StatField.Strength;
        private int  lastPaidCost;
        private bool hasLastPaid;
        private CardDef lastPlayedCard;
        public CardDef PeekLastPlayedCardDef() => lastPlayedCard;
        public bool TryGetLastPaymentField(out Game.Core.StatField f) { f = lastPaidField; return hasLastPaid; }

        // Card Draw
        private int drawEveryN_cardsThreshold = 0;
        private int drawEveryN_drawAmount     = 0;
        private int drawEveryN_counter        = 0;

        public void RecordLastPayment(Game.Core.StatField field, int amount)
        {
            lastPaidField = field;
            lastPaidCost  = Mathf.Max(0, amount);
            hasLastPaid   = true;
            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Last payment: field={lastPaidField} amount={lastPaidCost}");
        }

        public int       PeekLastPaidCostSafe() => hasLastPaid ? lastPaidCost : 0;
        public Game.Core.StatField PeekLastPaidField() => lastPaidField;
        public void AddCallCredits(int amount)
        {
            callCredits = Mathf.Max(0, callCredits + amount);
            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Credits now = {callCredits} (Δ {amount})");
        }

        public void RefundPercentOfLastCost(float percent01)
        {
            if (!hasLastPaid || lastPaidCost <= 0) return;
            var pct = Mathf.Clamp01(percent01);
            int refund = Mathf.Max(0, Mathf.RoundToInt(lastPaidCost * pct));
            if (refund > 0) AddCallCredits(refund);
            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Refunded {refund} ({pct:P0}) of last cost {lastPaidCost}");
        }

        public void AddNextCallCostDelta(int delta)
        {
            nextCallCostDelta += delta;
            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Next call cost delta now {nextCallCostDelta} (Δ {delta})");
        }

        public int ApplyCallCost(int baseCost, bool autoUseCredits = true)
        {
            int cost = Mathf.Max(0, baseCost + nextCallCostDelta);
            nextCallCostDelta = 0;

            if (autoUseCredits && callCredits > 0 && cost > 0)
            {
                int use = Mathf.Min(callCredits, cost);
                cost -= use;
                callCredits -= use;
                if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Used {use} credits; remaining credits={callCredits}, cost after credits={cost}");
            }

            // Do NOT record here; CardRuntime records with the actual field via RecordLastPayment(...)
            return cost;
        }

        public int PeekCredits() => callCredits;

        // ---------------------------------------------

        public void AddTempMaxHp(int v)    { tempMaxHp += v; }
        public void AddTempMana(int v)    { tempMana += v; }
        public void AddTempEngineering(int v)    { tempEng += v; }
        public void AddTempStrength(int v) { tempStr   += v; }
        public void AddTempDefense(int v)  { tempDef   += v; }

        public void ClearTemps()
        {
            tempMaxHp = tempStr = tempEng = tempMana = tempDef = 0;
            nextCallCostDelta = 0;
            hasLastPaid = false;
            lastPaidCost = 0;
        }

        private void TickAllEffectsOneTurn()
        {
            if (active == null || active.Count == 0) return;
            var snapshot = active.ToArray();
            foreach (var r in snapshot) r?.TickTurn(this);
        }

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

        public IActor PlayerActor { get { EnsurePlayerRef(); return player; } }

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

        public void PlayerPermanentStatsDelta(int maxHp = 0, int strength = 0, int eng = 0, int mana = 0)
        {
            bonusMaxHp += maxHp;
            bonusStr   += strength;
            bonusMana   += mana;
            bonusEng   += eng;

            EnsurePlayerRef();
            if (player != null)
            {
                if (maxHp > 0) player.Heal(maxHp);
            }
        }

        public void AddEffect(RyftEffectDef def)
        {
            if (!def) return;

            var existing = active.FirstOrDefault(r => r != null && r.Def != null && r.Def.id == def.id);
            if (existing != null)
            {
                existing.AddStack(1, refreshDuration: true, mgr: this);
                Debug.Log($"[Ryft] Stacked {def.id} -> stacks={existing.stacks}, chance={existing.CurrentProcPercent:0.#}%");
                return;
            }

            var rt = def.CreateRuntime();
            active.Add(rt);
            rt.OnAdded(this);
            Debug.Log($"[Ryft] Added {def.id} — {def.displayName}  [{def.color}/{def.polarity}]");
        }

        public void RemoveEffect(RyftEffectRuntime rt)
        {
            if (rt == null) return;
            rt.OnRemoved(this);
            active.Remove(rt);
        }

        public void ClearBattleScoped()
        {
            var toRemove = active.Where(a => a.Def.lifetime == EffectLifetime.UntilBattleEnd
                                          || a.Def.lifetime == EffectLifetime.DurationNTurns).ToList();
            foreach (var r in toRemove) RemoveEffect(r);
        }

        public void OnRyftOutcome(RyftEffectDef def) => AddEffect(def);

        public void TryDoubleCast(RyftEffectContext ctx)
        {
            if (verboseRyftLogs) Debug.Log("[Ryft] TryDoubleCast is a no-op for now.");
        }

        public void ApplyBarrierPercentToPlayer(float percent)
        {
            EnsurePlayerRef();
            if (!player) return;
            int amount = Mathf.RoundToInt(Mathf.Max(0, player.TotalStats.maxHealth) * Mathf.Clamp01(percent));
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

            int before = target.Health;
            target.ApplyDamage(int.MaxValue / 2);
            int dealt = Mathf.Max(0, before - target.Health);

            if (source != null) RyftCombatEvents.RaiseDamageDealt(source, target, dealt);
            if (!target.IsAlive) RyftCombatEvents.RaiseEnemyDefeated(target);
        }

        /*
         * To modify outgoing damage for a *single* upcoming hit, call SetNextOutgoingDamageMultiplier
         * from an effect, and have the card runtime call ApplyOutgoingDamageModifiers on its damage.
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
            CardDef card = null,
            IActor attacker = null,
            IActor target   = null)
        {
            int result = Mathf.Max(0, Mathf.RoundToInt(baseDamage * nextOutgoingDamageMult));

            if (verboseRyftLogs)
            {
                string cardId = card ? card.id : "(n/a)";
                string atk  = attacker != null ? attacker.DisplayName : "(n/a)";
                string tgt  = target   != null ? target.DisplayName   : "(n/a)";
                float mult  = nextOutgoingDamageMult;
                string tag  = nextOutgoingDamageTag ?? "(none)";

                Debug.Log($"[Ryft][DMG] card={cardId} attacker={atk} target={tgt} base={baseDamage} mult={mult:0.###} src={tag} => final={result}");
            }

            // consume once
            nextOutgoingDamageMult = 1f;
            nextOutgoingDamageTag  = null;
            nextOutgoingDamageSource = null;

            return result;
        }

        /// <summary>
        /// For the rest of THIS TURN, reduce the cost of the given resource by `reduceBy`,
        /// but never below `minCost` (e.g., 0). Multiple calls stack additively.
        /// </summary>
        public void RegisterCostReducer(Game.Core.StatField field, int reduceBy, int minCost = 0)
        {
            if (reduceBy <= 0) return;
            if (turnCostReduceBy.TryGetValue(field, out var r)) turnCostReduceBy[field] = r + reduceBy;
            else turnCostReduceBy[field] = reduceBy;

            minCost = Mathf.Max(0, minCost);
            if (turnCostFloor.TryGetValue(field, out var curFloor))
                turnCostFloor[field] = Mathf.Min(curFloor, minCost);   // keep the lowest floor (e.g., 0 beats 1)
            else
                turnCostFloor[field] = minCost;

            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Reducer registered: {field} -{reduceBy} (min {turnCostFloor[field]}) this turn.");
        }

        /// <summary>
        /// Register a persistent (battle-scoped) chance [0..100] that spending the given resource
        /// refunds that cost back as credits. Multiple calls stack and clamp to 100.
        /// </summary>
        public void RegisterRefundChance(Game.Core.StatField field, int chancePct)
        {
            chancePct = Mathf.Clamp(chancePct, 0, 100);
            if (refundChancePct.TryGetValue(field, out var cur)) refundChancePct[field] = Mathf.Clamp(cur + chancePct, 0, 100);
            else refundChancePct[field] = chancePct;

            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Refund chance registered: {field} +{chancePct}% (now {refundChancePct[field]}%).");
        }

        public void RegisterDrawEveryNCards(int n, int drawAmount)
        {
            drawEveryN_cardsThreshold = Mathf.Max(1, n);
            drawEveryN_drawAmount     += Mathf.Max(1, drawAmount);
            drawEveryN_counter        = 0;
            if (verboseRyftLogs)
                Debug.Log($"[Ryft][DRAW] Registered: every {drawEveryN_cardsThreshold} plays → draw {drawEveryN_drawAmount}.");
        }


        // --- Field-scoped temporary refunds ---------------------------------
        private readonly System.Collections.Generic.Dictionary<Game.Core.StatField, int> tempRefunds
            = new System.Collections.Generic.Dictionary<Game.Core.StatField, int>();

        public void RegisterTemporaryRefund(Game.Core.StatField field, int count)
        {
            if (count <= 0) return;
            tempRefunds.TryGetValue(field, out var cur);
            tempRefunds[field] = cur + count;
            if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Registered {count} temp refunds for {field}; now {tempRefunds[field]}");
        }

        /// <summary>
        /// Apply call cost for a specific resource field. If there is a pending temporary refund
        /// for this field, consume one and make the cost 0. Otherwise fall back to normal ApplyCallCost.
        /// </summary>
        public int ApplyCallCostForField(int baseCost, Game.Core.StatField field, bool autoUseCredits = true)
        {
            // 1) Field-scoped "next N are free"
            if (tempRefunds.TryGetValue(field, out var remaining) && remaining > 0)
            {
                tempRefunds[field] = remaining - 1;
                nextCallCostDelta = 0; // consume any pending delta
                if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Consumed temp refund for {field}; remaining={tempRefunds[field]}");
                return 0;
            }

            // 2) Start from base
            int cost = Mathf.Max(0, baseCost);

            // 3) This-turn reducers & floors (e.g., Momentum)
            if (turnCostReduceBy.TryGetValue(field, out var reduceBy)) cost = Mathf.Max(0, cost - reduceBy);
            if (turnCostFloor.TryGetValue(field, out var floor))       cost = Mathf.Max(floor, cost);

            // 4) Global "next call delta" (one-shot) then clamp
            cost = Mathf.Max(0, cost + nextCallCostDelta);
            nextCallCostDelta = 0;

            // 5) Spend credits automatically (from previous refunds etc.)
            if (autoUseCredits && callCredits > 0 && cost > 0)
            {
                int use = Mathf.Min(callCredits, cost);
                cost -= use;
                callCredits -= use;
                if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Used {use} credits; remaining credits={callCredits}, cost after credits={cost}");
            }

            // 6) Roll refund chance AFTER computing final cost (credit back for future plays)
            if (cost > 0 && refundChancePct.TryGetValue(field, out var pct) && pct > 0)
            {
                // UnityEngine.Random.value ∈ [0,1)
                if (UnityEngine.Random.Range(0, 100) < pct)
                {
                    AddCallCredits(cost);
                    if (verboseRyftLogs) Debug.Log($"[Ryft][COST] Refund chance hit for {field}: credited {cost} back.");
                }
            }

            return Mathf.Max(0, cost);
        }
        private void ClearTurnCostModifiers()
        {
            turnCostReduceBy.Clear();
            turnCostFloor.Clear();
        }

        /// <summary>
        /// Returns a NEW runtime bound to the player for the last card the player resolved,
        /// optionally only if the last paid resource matched <paramref name="onlyIfCostField"/>.
        /// This does NOT pay its cost; caller's Execute will do that.
        /// </summary>
        public CardRuntime GetLastPlayedCardRuntime(Game.Core.StatField? onlyIfCostField = null, IActor ownerOverride = null)
        {
            if (lastPlayedCard == null) return null;

            // If caller cares about the last resource used for that play, gate by it.
            if (onlyIfCostField.HasValue && hasLastPaid && lastPaidField != onlyIfCostField.Value)
                return null;

            var type = System.Type.GetType(lastPlayedCard.runtimeTypeName);
            if (type == null)
            {
                if (verboseRyftLogs) Debug.LogWarning($"[Ryft] Runtime type not found: {lastPlayedCard.runtimeTypeName}");
                return null;
            }

            var rt = System.Activator.CreateInstance(type) as CardRuntime;
            if (rt == null)
            {
                if (verboseRyftLogs) Debug.LogWarning($"[Ryft] Type is not a CardRuntime: {lastPlayedCard.runtimeTypeName}");
                return null;
            }

            var owner = ownerOverride ?? PlayerActor;
            rt.Bind(lastPlayedCard, owner);
            return rt;
        }



        // --- Per-field cost modifiers (this turn) ------------------------------
        // Sum of all "reduce cost by X" effects until end of turn.
        private readonly Dictionary<Game.Core.StatField, int> turnCostReduceBy
            = new Dictionary<Game.Core.StatField, int>();
        // Floor (minimum cost) enforced this turn (e.g., min 0).
        private readonly Dictionary<Game.Core.StatField, int> turnCostFloor
            = new Dictionary<Game.Core.StatField, int>();

        // --- Per-field refund chance (battle-scoped unless you clear it) -------
        private readonly Dictionary<Game.Core.StatField, int> refundChancePct
            = new Dictionary<Game.Core.StatField, int>();


        // --- Event fan-out to active effects -------------------------------
        private void Broadcast(
            RyftTrigger t,
            FightContext fight = null,
            IActor src = null,
            IActor tgt = null,
            CardDef card = null,
            int amt = 0)
        {
            if (active.Count == 0) return;

            var ctx = new RyftEffectContext
            {
                fight   = fight,
                source  = src,
                target  = tgt,
                cardDef = card,
                amount  = amt,
                trigger = t
            };

            var snapshot = active.ToArray();
            foreach (var e in snapshot) e?.HandleTrigger(this, ctx);
        }

        private void OnBattleStart(FightContext c)     { EnsurePlayerRef(); Broadcast(RyftTrigger.OnBattleStart, fight:c); }
        private void OnBattleEnd()
        {
            Broadcast(RyftTrigger.OnBattleEnd);
            ClearBattleScoped();
            ClearTemps();
            ClearTurnCostModifiers();
            drawEveryN_cardsThreshold = 0;
            drawEveryN_drawAmount     = 0;
            drawEveryN_counter        = 0;
        }
        private void OnTurnStart()
        {
            TickAllEffectsOneTurn();
            Broadcast(RyftTrigger.OnTurnStart);
        }
        private void OnTurnEnd()
        {
            Broadcast(RyftTrigger.OnTurnEnd);
            ClearTurnCostModifiers();
        }


        private void OnAbilityUsed(IActor s, CardDef a, FightContext c)
            => Broadcast(RyftTrigger.OnAbilityUsed, fight:c, src:s, card:a);

        private void OnAbilityResolved(IActor s, CardDef a, FightContext c)
        {
            Broadcast(RyftTrigger.OnAbilityResolved, fight:c, src:s, card:a);

            if (a != null && IsPlayer(s))
                lastPlayedCard = a;

            if (IsPlayer(s) && a != null && drawEveryN_cardsThreshold > 0 && drawEveryN_drawAmount > 0)
            {
                drawEveryN_counter++;
                if (drawEveryN_counter % drawEveryN_cardsThreshold == 0)
                {
                    // ask fight to draw
                    var fsc = Game.Combat.FightSceneController.Instance;
                    fsc?.DrawCards(drawEveryN_drawAmount);
                    if (verboseRyftLogs)
                        Debug.Log($"[Ryft][DRAW] Threshold hit ({drawEveryN_counter}). Drew {drawEveryN_drawAmount}.");
                }
            }
        }

        private void OnDamageDealt(IActor s, IActor t, int dmg) { Broadcast(RyftTrigger.OnDamageDealt, src:s, tgt:t, amt:dmg); }
        private void OnDamageTaken(IActor t, int dmg)           { Broadcast(RyftTrigger.OnDamageTaken, tgt:t, amt:dmg); }
        private void OnEnemyDefeated(IActor e)                  { Broadcast(RyftTrigger.OnEnemyDefeated, tgt:e); }

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
