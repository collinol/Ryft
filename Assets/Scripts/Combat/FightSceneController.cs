using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Player;
using Game.Enemies;
using Game.Cards;
using Game.UI;
using Game.Ryfts;
using Game.RyftEntities;
using System;
using Game.Abilities;
using Game.Abilities.Enemy;
using Game.VFX;

namespace Game.Combat
{
    [DefaultExecutionOrder(-100)]
    public class FightSceneController : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private EnemyBase[] enemies;
        private RyftPortalEntity ryftPortal;

        [Header("Databases")]
        [SerializeField] private CardDatabase cardDb;

        [Header("UI (optional)")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private TurnBannerUI turnBanner;

        [Header("Turn Timing")]
        [SerializeField] private float enemyActionDelay = 0.6f;

        [SerializeField] private Game.UI.AbilityBarUI handUI;
        [SerializeField, Min(1)] private int maxHandSize = 0;

        private FightContext ctx;

        // deck state
        private readonly List<CardDef> drawPile    = new();
        private readonly List<CardDef> discardPile = new();
        private readonly List<CardDef> hand        = new();

        // card runtimes (id -> runtime bound to player)
        private readonly Dictionary<string, CardRuntime> runtimeById = new();
        private bool dealtOpeningHand = false;

        [SerializeField, Min(0)] private int maxEnergyPerTurn = 3;
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy => Mathf.Max(0, maxEnergyPerTurn);
        public int EnemyTurnIndex { get; private set; } = 0;
        private CardRuntime pendingTargetedCard;

        public static FightSceneController Instance { get; private set; }
        public event Action<int,int> OnEnergyChanged;
        private CombatEventTracker combatTracker;
        private DeathPreventionSystem deathPrevention;
        private EndOfTurnEffects endOfTurnEffects;
        private CardCooldownManager cooldownManager;
        private GadgetManager gadgetManager;

        void Awake()
        {
            Instance = this;
            if (!handUI) handUI = FindObjectOfType<Game.UI.AbilityBarUI>(true);
            if (!player)  player  = FindObjectOfType<PlayerCharacter>();
            if (enemies == null || enemies.Length == 0)
                enemies = FindObjectsOfType<EnemyBase>();

            if (!cardDb) cardDb = CardDatabase.Load();
            if (!uiCanvas) uiCanvas = FindObjectOfType<Canvas>();
            if (!turnBanner && uiCanvas) turnBanner = TurnBannerUI.Ensure(uiCanvas);

            // Initialize card tooltip
            if (uiCanvas) Game.UI.CardTooltip.Ensure(uiCanvas);

            // Initialize combat systems
            combatTracker = CombatEventTracker.Instance;
            if (!combatTracker)
            {
                var go = new GameObject("CombatEventTracker");
                combatTracker = go.AddComponent<CombatEventTracker>();
            }

            deathPrevention = DeathPreventionSystem.Instance;
            if (!deathPrevention)
            {
                var go = new GameObject("DeathPreventionSystem");
                deathPrevention = go.AddComponent<DeathPreventionSystem>();
            }

            endOfTurnEffects = EndOfTurnEffects.Instance;
            if (!endOfTurnEffects)
            {
                var go = new GameObject("EndOfTurnEffects");
                endOfTurnEffects = go.AddComponent<EndOfTurnEffects>();
            }

            cooldownManager = CardCooldownManager.Instance;
            if (!cooldownManager)
            {
                var go = new GameObject("CardCooldownManager");
                cooldownManager = go.AddComponent<CardCooldownManager>();
            }

            gadgetManager = GadgetManager.Instance;
            if (!gadgetManager)
            {
                var go = new GameObject("GadgetManager");
                gadgetManager = go.AddComponent<GadgetManager>();
            }

            // Initialize VFX manager
            var vfxManager = CardVFXManager.Instance;
            if (!vfxManager)
            {
                var go = new GameObject("CardVFXManager");
                go.AddComponent<CardVFXManager>();
            }
        }

        void Start()
        {
            if (turnBanner) turnBanner.ShowInstant("Player Turn");

            // Position player at y=3 to avoid overlap with card hand
            if (player != null)
            {
                var pos = player.transform.position;
                player.transform.position = new Vector3(pos.x, 3.0f, pos.z);
                Debug.Log($"[FightSceneController] Set player position to y=3.0");
            }

            // ALWAYS re-find enemies in Start() to ensure RuntimeEnemySpawner has finished
            enemies = FindObjectsOfType<EnemyBase>();
            Debug.Log($"[FightSceneController] Found {enemies?.Length ?? 0} enemies in Start()");

            if (enemies != null && enemies.Length > 0)
            {
                // Position all enemies at y=3 to avoid overlap with card hand
                foreach (var e in enemies)
                {
                    if (e != null)
                    {
                        var pos = e.transform.position;
                        e.transform.position = new Vector3(pos.x, 3.0f, pos.z);
                        Debug.Log($"  - Found enemy: {e.name} HP:{e.Health}/{e.TotalStats.maxHealth} Alive:{e.IsAlive} at y=3.0");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[FightSceneController] NO ENEMIES FOUND! Check if RuntimeEnemySpawner is in the scene.");
            }

            var enemyList = (enemies != null) ? enemies.Where(e => e != null).ToList()
                                              : new List<EnemyBase>();

            Debug.Log($"[FightSceneController] Creating FightContext with {enemyList.Count} enemies");

            ctx = new FightContext(player, enemyList, msg => Debug.Log(msg));

            // Find and register the rift portal if this is a portal fight
            ryftPortal = FindObjectOfType<RyftPortalEntity>();
            if (ryftPortal != null)
            {
                ctx.SetRyftPortal(ryftPortal);
                ryftPortal.OnPortalDestroyed += OnPortalDestroyed;
                Debug.Log($"[FightSceneController] Found RyftPortal ({ryftPortal.RyftColor}) with {ryftPortal.Health} HP - enemies will target it");
            }

            // Build deck
            drawPile.Clear();
            discardPile.Clear();
            hand.Clear();
            if (cardDb != null)
            {
                drawPile.AddRange(cardDb.BuildPlayerDeck());
                Shuffle(drawPile);
            }

            // Pre-bind runtimes for all unique cards (optional)
            if (cardDb != null)
            {
                foreach (var c in cardDb.BuildPlayerDeck().Distinct())
                    EnsureRuntime(c?.id);
            }

            // Start first player turn
            StartPlayerTurn();
        }

        // ========== Turn flow ==========

        private void StartPlayerTurn()
        {
            // Reset combat trackers
            if (combatTracker) combatTracker.ResetTurnCounters();

            // Tick status effects
            if (player) player.StatusEffects?.TickAllEffects();
            if (enemies != null)
            {
                foreach (var enemy in enemies.Where(e => e && e.IsAlive))
                {
                    enemy.StatusEffects?.TickAllEffects();
                }
            }

            // Tick cooldowns
            if (cooldownManager) cooldownManager.TickAllCooldowns();

            // Tick gadgets
            if (gadgetManager) gadgetManager.TickAllGadgets();

            SetEnergy(MaxEnergy);
            player.RefreshTurnStats();

            int cap = EffectiveMaxHandSize();

            if (!dealtOpeningHand)
            {
                DrawToHandSize(cap);      // opening hand: fill to max slots
                dealtOpeningHand = true;
            }
            else
            {
                if (hand.Count >= cap)
                {
                    ctx.Log("Hand is full — no card drawn.");
                    RefreshHandUI();
                    return;
                }
                Draw(1); // normal turn: draw 1 if there’s space
            }

            RefreshHandUI();
        }
        private void DrawToHandSize(int targetSize)
        {
            int need = Mathf.Max(0, targetSize - hand.Count);
            if (need > 0) Draw(need);
        }
        public void DrawCards(int n)
        {
            if (n <= 0) return;
            Draw(n);
            RefreshHandUI();
            ctx?.Log($"You draw {n} card{(n==1?"":"s")}.");
        }

        /// <summary>
        /// Add a specific card to hand with optional cost override
        /// </summary>
        public void AddCardToHand(CardDef card, int energyCostOverride = -1)
        {
            if (card == null) return;

            // Add to hand
            hand.Add(card);

            // TODO: If energyCostOverride is provided, need to track modified costs
            // For now, this just adds the card - cost modification needs additional tracking

            RefreshHandUI();
            ctx?.Log($"You gained {card.displayName} to your hand!");
        }

        /// <summary>
        /// Get a random card from the current deck by stat type
        /// </summary>
        public CardDef GetRandomCardByType(StatField type)
        {
            if (cardDb == null) return null;

            var allCards = cardDb.BuildPlayerDeck();
            var matchingCards = new List<CardDef>();

            foreach (var card in allCards)
            {
                if (card == null) continue;

                // Check the card's runtime type to determine its stat
                var rt = EnsureRuntime(card.id);
                if (rt != null)
                {
                    // Use reflection to get ScalingStat
                    var scalingStatProp = rt.GetType().GetProperty("ScalingStat",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (scalingStatProp != null)
                    {
                        var scalingStat = (StatField)scalingStatProp.GetValue(rt);
                        if (scalingStat == type)
                        {
                            matchingCards.Add(card);
                        }
                    }
                }
            }

            if (matchingCards.Count == 0) return null;

            int randomIndex = UnityEngine.Random.Range(0, matchingCards.Count);
            return matchingCards[randomIndex];
        }

        public void EndPlayerTurnButton()
        {
            Debug.Log("[FightSceneController] EndPlayerTurnButton called");

            // Trigger end-of-turn effects
            endOfTurnEffects?.TriggerPlayerTurnEnd();

            // DO NOT discard hand. Just go enemy turn.
            if (turnBanner) turnBanner.Show("Enemy Turn");
            StartCoroutine(EnemyTurnThenBackToPlayer());
        }

        private IEnumerator EnemyTurnThenBackToPlayer()
        {
            Debug.Log($"[FightSceneController] Starting enemy turn {EnemyTurnIndex + 1}");
            Debug.Log($"[FightSceneController] ctx.Enemies count: {ctx?.Enemies?.Count ?? 0}");

            EnemyTurnIndex++;
            var enemyDb = EnemyAbilityDatabase.Load();

            if (ctx?.Enemies == null)
            {
                Debug.LogError("[FightSceneController] ctx.Enemies is NULL!");
                yield break;
            }

            var aliveEnemies = ctx.Enemies.Where(e => e && e.IsAlive).ToList();
            Debug.Log($"[FightSceneController] Found {aliveEnemies.Count} alive enemies");

            foreach (var enemy in aliveEnemies)
            {
                Debug.Log($"[FightSceneController] Enemy {enemy.name} taking turn...");
                yield return new WaitForSeconds(enemyActionDelay);

                // --- Minimal default enemy action: deal flat damage to the player ---
                enemy.PerformEnemyAction(ctx, enemyDb);
            }

            Debug.Log("[FightSceneController] Enemy turn complete, returning to player turn");

            // Back to player
            if (turnBanner) turnBanner.Show("Player Turn");
            StartPlayerTurn();
        }

        private void SetEnergy(int value)
        {
            CurrentEnergy = Mathf.Clamp(value, 0, MaxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
        }

        void OnEnable()
        {
            RyftCombatEvents.OnResourceRefund += HandleResourceRefund;
        }

        void OnDisable()
        {
            RyftCombatEvents.OnResourceRefund -= HandleResourceRefund;
        }

        private void HandleResourceRefund(IActor who, StatField field, int amount)
        {
            // Only player refunds, only Energy
            if (amount <= 0) return;
            if (!ReferenceEquals(who, player)) return;
            if (field != StatField.Energy) return;

            SetEnergy(CurrentEnergy + amount);               // instantly add refunded energy
            Debug.Log($"[Energy Refund] +{amount} → {CurrentEnergy}/{MaxEnergy}");
        }

        // ========== Cards ==========

        public IReadOnlyList<CardDef> CurrentHand => hand;

        public bool CanPlayCard(string id)
        {
            var card = hand.FirstOrDefault(h => h && h.id == id);
            if (!card) return false;
            var rt = EnsureRuntime(id);
            return rt != null && rt.CanUse(ctx);
        }

        public void UsePlayerCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return;

            // find a copy in hand
            var index = hand.FindIndex(c => c && c.id == cardId);
            if (index < 0) return;

            var card = hand[index];
            var rt   = EnsureRuntime(card.id);
            if (rt == null) return;

            switch (rt.Targeting)
            {
                case Game.Cards.TargetingType.None:
                case Game.Cards.TargetingType.Self:
                {
                    var owner = player as IActor;
                    RyftCombatEvents.RaiseAbilityUsed(owner, rt.Def, ctx);
                    rt.Execute(ctx, player);
                    RyftCombatEvents.RaiseAbilityResolved(owner, rt.Def, ctx);
                    DiscardFromHand(index);
                    break;
                }

                case Game.Cards.TargetingType.SingleEnemy:
                    pendingTargetedCard = rt;
                    break;

                case Game.Cards.TargetingType.AllEnemies:
                {
                    var owner = player as IActor;
                    RyftCombatEvents.RaiseAbilityUsed(owner, rt.Def, ctx);
                    rt.Execute(ctx, null);
                    RyftCombatEvents.RaiseAbilityResolved(owner, rt.Def, ctx);
                    DiscardFromHand(index);
                    break;
                }
            }


            RefreshHandUI();
        }

        public void OnEnemyClicked(EnemyBase enemy)
        {
            if (!enemy || !enemy.IsAlive) return;
            if (pendingTargetedCard == null) return;

            var id = pendingTargetedCard.Def.id;
            var handIndex = hand.FindIndex(c => c && c.id == id);
            if (handIndex >= 0)
            {
                var owner = player as IActor;
                RyftCombatEvents.RaiseAbilityUsed(owner, pendingTargetedCard.Def, ctx);
                pendingTargetedCard.Execute(ctx, enemy);
                RyftCombatEvents.RaiseAbilityResolved(owner, pendingTargetedCard.Def, ctx);

                DiscardFromHand(handIndex);
                RefreshHandUI();
            }

            pendingTargetedCard = null;
        }


        private void DiscardFromHand(int handIndex)
        {
            if (handIndex < 0 || handIndex >= hand.Count) return;
            var card = hand[handIndex];
            if (card) discardPile.Add(card);
            hand.RemoveAt(handIndex);
        }

        private void Draw(int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (drawPile.Count == 0)
                {
                    // reshuffle discard into draw
                    if (discardPile.Count == 0) return; // nothing to draw
                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    Shuffle(drawPile);
                }

                var top = drawPile[drawPile.Count - 1];
                drawPile.RemoveAt(drawPile.Count - 1);
                hand.Add(top);
            }
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private CardRuntime EnsureRuntime(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (runtimeById.TryGetValue(id, out var rt)) return rt;

            var def = cardDb?.Get(id);
            if (!def)
            {
                Debug.LogError($"Card '{id}' not found in CardDatabase.");
                return null;
            }

            var type = System.Type.GetType(def.runtimeTypeName);
            if (type == null)
            {
                Debug.LogError($"Card runtime type not found: {def.runtimeTypeName}");
                return null;
            }

            rt = System.Activator.CreateInstance(type) as CardRuntime;
            if (rt == null)
            {
                Debug.LogError($"Type '{def.runtimeTypeName}' is not a CardRuntime.");
                return null;
            }

            rt.Bind(def, player);
            runtimeById[id] = rt;
            return rt;
        }

        private void RefreshHandUI()
        {
            if (!handUI) handUI = FindObjectOfType<Game.UI.AbilityBarUI>(true);
            handUI?.Refresh();
        }
        private int EffectiveMaxHandSize()
        {
            if (maxHandSize > 0) return maxHandSize;
            if (handUI)
            {
                var slots = handUI.GetComponentsInChildren<Game.UI.AbilityButton>(true);
                if (slots != null && slots.Length > 0) return slots.Length;
            }
            return 5;
        }
        public IEnumerable<IActor> AllAliveEnemies()
        {
            var list = enemies ?? Array.Empty<EnemyBase>();
            foreach (var e in list)
            {
                if (e && e.IsAlive) yield return e;  // EnemyBase implements IActor
            }
        }
        public bool CanAffordEnergy(int cost) => cost <= Mathf.Max(0, CurrentEnergy + Game.Ryfts.RyftEffectManager.Ensure().PeekCredits());

        public bool TrySpendEnergy(int baseCost)
        {
            var mgr  = Game.Ryfts.RyftEffectManager.Ensure();
            int cost = mgr.ApplyCallCostForField(Mathf.Max(0, baseCost), Game.Core.StatField.Energy, autoUseCredits: true);

            // Record the energy payment so refunds hit Energy
            mgr.RecordLastPayment(Game.Core.StatField.Energy, cost);

            if (cost == 0)
            {
                OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
                return true;
            }

            if (CurrentEnergy < cost) return false;

            SetEnergy(CurrentEnergy - cost);
            return true;
        }
        public void OnPlayerDamagedBy(IActor attacker, int damage, FightContext ctx)
        {
            if (damage <= 0 || attacker == null || player == null) return;

            // Trigger reflect if it is armed for the player
            ReflectNextTurnStatus.TryReflect(defender: player, attacker: attacker, incomingDamage: damage, ctx);
        }

        public void CheckVictoryCondition()
        {
            // Check if all enemies are dead
            if (enemies == null || enemies.Length == 0)
            {
                Debug.Log("[FightSceneController] No enemies to check");
                return;
            }

            bool allDead = true;
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                // Check if this is a portal fight
                if (ryftPortal != null)
                {
                    // Portal fight victory - portal survived and all enemies dead
                    if (ryftPortal.IsAlive)
                    {
                        Debug.Log("[FightSceneController] PORTAL FIGHT VICTORY - All enemies defeated, portal survived!");
                        if (MapSession.I != null)
                        {
                            MapSession.I.PortalFightVictory = true;
                        }
                        StartCoroutine(ReturnToMapAfterDelay());
                    }
                    // If portal is dead, OnPortalDestroyed will handle it
                }
                else
                {
                    // Regular fight - grant gold and go to reward scene
                    Debug.Log("[FightSceneController] All enemies defeated! Going to RewardScene...");
                    GrantVictoryRewards();
                    StartCoroutine(GoToRewardSceneAfterDelay());
                }
            }
        }

        /// <summary>
        /// Called when the rift portal is destroyed by enemies.
        /// </summary>
        private void OnPortalDestroyed()
        {
            Debug.Log("[FightSceneController] PORTAL FIGHT DEFEAT - Portal was destroyed!");

            if (MapSession.I != null)
            {
                MapSession.I.PortalFightVictory = false;
            }

            StartCoroutine(ReturnToMapAfterDelay());
        }

        private System.Collections.IEnumerator ReturnToMapAfterDelay()
        {
            // Wait a moment so the player can see the outcome
            yield return new WaitForSeconds(1.0f);

            // Unsubscribe from portal events before leaving
            if (ryftPortal != null)
            {
                ryftPortal.OnPortalDestroyed -= OnPortalDestroyed;
            }

            // Load MapScene; MapController will detect MapSession.I.Saved and resolve the outcome
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void GrantVictoryRewards()
        {
            if (MapSession.I == null) return;

            // Calculate gold based on enemy count and elite status
            int baseGold = 10;
            int enemyCount = enemies?.Length ?? 1;
            int goldReward = baseGold + (enemyCount * 5);

            if (MapSession.I.IsEliteFight)
            {
                goldReward *= 2; // Elites give double gold

                // Track elite defeat for time portal obligations
                TrackEliteDefeat();
            }

            MapSession.I.AddGold(goldReward);
            MapSession.I.PendingReward = true;

            Debug.Log($"[FightSceneController] Granted {goldReward} gold. Total: {MapSession.I.Gold}");
        }

        private void TrackEliteDefeat()
        {
            if (enemies == null || enemies.Length == 0) return;

            // Find the elite enemy that was defeated
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                string typeName = enemy.GetType().Name;

                // Check if it's an elite type
                if (typeName.Contains("Chieftain") || typeName.Contains("Knight") ||
                    typeName.Contains("Golem") || typeName.Contains("Necromancer"))
                {
                    // Simplify the name for matching
                    string eliteType = typeName.Replace("Enemy", "");
                    MapSession.I.LastDefeatedEliteType = eliteType;

                    // Notify time portal system
                    if (MapSession.I.TimePortal != null)
                    {
                        int currentLevel = MapSession.I.CurrentMapLevel;
                        MapSession.I.TimePortal.OnEliteDefeated(eliteType, currentLevel);
                    }

                    Debug.Log($"[FightSceneController] Tracked elite defeat: {eliteType}");
                    break;
                }
            }
        }

        private System.Collections.IEnumerator GoToRewardSceneAfterDelay()
        {
            yield return new WaitForSeconds(1.0f);

            // Unsubscribe from portal events before leaving
            if (ryftPortal != null)
            {
                ryftPortal.OnPortalDestroyed -= OnPortalDestroyed;
            }

            // Go to reward scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("RewardScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            // Clean up portal subscription
            if (ryftPortal != null)
            {
                ryftPortal.OnPortalDestroyed -= OnPortalDestroyed;
            }
        }

    }
}


