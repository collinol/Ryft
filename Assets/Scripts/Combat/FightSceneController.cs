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
using System;
using Game.Abilities;
using Game.Abilities.Enemy;

namespace Game.Combat
{
    public class FightSceneController : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private EnemyBase[] enemies;

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

        private CardRuntime pendingTargetedCard;

        public static FightSceneController Instance { get; private set; }

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
        }

        void Start()
        {
            if (turnBanner) turnBanner.ShowInstant("Player Turn");

            var enemyList = (enemies != null) ? enemies.Where(e => e != null).ToList()
                                              : new List<EnemyBase>();
            ctx = new FightContext(player, enemyList, msg => Debug.Log(msg));

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

        public void EndPlayerTurnButton()
        {
            // DO NOT discard hand. Just go enemy turn.
            if (turnBanner) turnBanner.Show("Enemy Turn");
            StartCoroutine(EnemyTurnThenBackToPlayer());
        }

        private IEnumerator EnemyTurnThenBackToPlayer()
        {
            var enemyDb = EnemyAbilityDatabase.Load();
            foreach (var enemy in ctx.Enemies.Where(e => e && e.IsAlive))
            {
                yield return new WaitForSeconds(enemyActionDelay);

                // --- Minimal default enemy action: deal flat damage to the player ---
                enemy.PerformEnemyAction(ctx,enemyDb);
            }

            // Back to player
            if (turnBanner) turnBanner.Show("Player Turn");
            StartPlayerTurn();
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
                    RyftCombatEvents.RaiseAbilityUsed(owner, null, ctx);
                    rt.Execute(ctx, player);
                    RyftCombatEvents.RaiseAbilityResolved(owner, null, ctx);
                    DiscardFromHand(index);
                    break;
                }

                case Game.Cards.TargetingType.SingleEnemy:
                    pendingTargetedCard = rt;
                    break;

                case Game.Cards.TargetingType.AllEnemies:
                {
                    var owner = player as IActor;
                    RyftCombatEvents.RaiseAbilityUsed(owner, null, ctx);
                    rt.Execute(ctx, null);
                    RyftCombatEvents.RaiseAbilityResolved(owner, null, ctx);
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
                RyftCombatEvents.RaiseAbilityUsed(owner, null, ctx);
                pendingTargetedCard.Execute(ctx, enemy);
                RyftCombatEvents.RaiseAbilityResolved(owner, null, ctx);

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

    }
}


