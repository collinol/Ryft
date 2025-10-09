// Assets/Scripts/Combat/FightSceneController.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;
using Game.Player;
using Game.Enemies;
using Game.Abilities;
using Game.Abilities.Enemy; // <-- new: enemy DB
using Game.UI;
using Game.Ryfts;

namespace Game.Combat
{
    public class FightSceneController : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private EnemyBase[] enemies;

        [Header("Databases")]
        [SerializeField] private AbilityDatabase       playerAbilityDb;   // player abilities
        [SerializeField] private EnemyAbilityDatabase  enemyAbilityDb;    // enemy abilities

        [Header("UI (optional)")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private TurnBannerUI turnBanner;

        [Header("Turn Timing")]
        [SerializeField] private float enemyActionDelay = 0.6f;

        private FightContext ctx;
        private Camera cam;

        // Player ability runtimes (id -> runtime)
        private readonly Dictionary<string, AbilityRuntime> runtimeById = new();

        private AbilityRuntime pendingTargetedAbility;
        public static FightSceneController Instance { get; private set; }
        public bool IsFreeCast => isFreeCast;
        private bool pendingFreeCast;

        void Awake()
        {
             Instance = this;
            if (!player)  player  = FindObjectOfType<PlayerCharacter>();
            if (enemies == null || enemies.Length == 0)
                enemies = FindObjectsOfType<EnemyBase>();

            if (!playerAbilityDb) playerAbilityDb = AbilityDatabase.Load();
            if (!enemyAbilityDb)  enemyAbilityDb  = EnemyAbilityDatabase.Load();

            if (!uiCanvas)  uiCanvas  = FindObjectOfType<Canvas>();
            cam = Camera.main;
            if (!turnBanner && uiCanvas) turnBanner = TurnBannerUI.Ensure(uiCanvas);
        }

        void Start()
        {
            if (turnBanner) turnBanner.ShowInstant("Player Turn");


            var enemyList = (enemies != null) ? enemies.Where(e => e != null).ToList()
                                              : new List<EnemyBase>();

            ctx = new FightContext(player, enemyList, msg => Debug.Log(msg));

            // Build player runtimes from PLAYER db
            foreach (var def in player.AbilityLoadout)
            {
                if (!def) continue;
                if (!runtimeById.ContainsKey(def.id))
                {
                    var rt = playerAbilityDb.CreateRuntime(def.id, player);
                    if (rt != null) runtimeById[def.id] = rt;
                }
            }

            // Build enemy runtimes from ENEMY db
            foreach (var e in enemyList) e?.EnsureEnemyAbilityRuntimes(enemyAbilityDb);

            RyftCombatEvents.RaiseBattleStart(ctx);
            RyftCombatEvents.RaiseTurnStart();
            RefreshAbilityButtons();
            var ryftMgr = RyftEffectManager.Ensure();
            ryftMgr.EnsurePlayerRef();
            ryftMgr.DebugLogActiveEffects("[Fight]");
        }

        public void AddCooldown(string abilityId, int delta)
        {
            if (string.IsNullOrEmpty(abilityId) || delta == 0) return;
            if (!runtimeById.TryGetValue(abilityId, out var runtime))
            {
                runtime = playerAbilityDb.CreateRuntime(abilityId, player);
                if (runtime == null) return;
                runtimeById[abilityId] = runtime;
            }
            int before = runtime.CooldownRemaining;
            runtime.AdjustCooldown(delta);
            if (RyftEffectManager.Instance?.verboseRyftLogs == true)
                Debug.Log($"[Ryft][CD] {abilityId}: {before} {(delta>0?"+":"")}{delta} => {runtime.CooldownRemaining}");
            RefreshAbilityButtons();
        }



        // ------------------- UI hooks -------------------
        private bool isFreeCast;
        public void UsePlayerAbility(string abilityId, bool freeCast = false)
        {
            isFreeCast = freeCast;
            if (string.IsNullOrEmpty(abilityId)) return;

            if (!runtimeById.TryGetValue(abilityId, out var runtime))
            {
                runtime = playerAbilityDb.CreateRuntime(abilityId, player);
                if (runtime == null) return;
                runtimeById[abilityId] = runtime;
            }

            switch (runtime.Def.targeting)
            {
                case TargetingType.None:
                case TargetingType.Self:
                    RyftCombatEvents.RaiseAbilityUsed(player, runtime.Def, ctx);
                    runtime.Execute(ctx, player);
                    RyftCombatEvents.RaiseAbilityResolved(player, runtime.Def, ctx);
                    RefreshAbilityButtons();
                    break;

                case TargetingType.SingleEnemy:
                    pendingTargetedAbility = runtime;
                    pendingFreeCast = freeCast;
                    ctx.Log("Click an enemy to target.");
                    return;

                case TargetingType.AllEnemies:
                    RyftCombatEvents.RaiseAbilityUsed(player, runtime.Def, ctx);
                    foreach (var e in ctx.Enemies.Where(e => e && e.IsAlive))
                        runtime.Execute(ctx, e);
                    RyftCombatEvents.RaiseAbilityResolved(player, runtime.Def, ctx);
                    RefreshAbilityButtons();
                    break;
            }
            isFreeCast = false;
        }

        public void OnEnemyClicked(EnemyBase enemy)
        {
            if (!enemy || !enemy.IsAlive) return;
            if (pendingTargetedAbility != null)
            {
                isFreeCast = pendingFreeCast;
                RyftCombatEvents.RaiseAbilityUsed(player, pendingTargetedAbility.Def, ctx);
                pendingTargetedAbility.Execute(ctx, enemy);
                RyftCombatEvents.RaiseAbilityResolved(player, pendingTargetedAbility.Def, ctx);
                isFreeCast = false;
                pendingTargetedAbility = null;
                RefreshAbilityButtons();
            }
        }

        // ------------------- Turn flow -------------------

        public void EndPlayerTurnButton()
        {
            foreach (var rt in runtimeById.Values) rt.TickCooldown();
            RefreshAbilityButtons();
            RyftCombatEvents.RaiseTurnEnd();
            if (turnBanner) turnBanner.Show("Enemy Turn");
            StartCoroutine(EnemyTurnThenBackToPlayer());
        }

        private IEnumerator EnemyTurnThenBackToPlayer()

        {
            RyftCombatEvents.RaiseTurnStart();
            foreach (var enemy in ctx.Enemies.Where(e => e && e.IsAlive))
            {
                yield return new WaitForSeconds(enemyActionDelay);

                var ability = enemy.PickRandomEnemyAbilityRuntime();
                if (ability == null)
                {
                    ctx.Log($"{enemy.DisplayName} has no abilities configured.");
                    continue;
                }

                if (player && player.IsAlive && ability.CanUse(ctx))
                {
                    ability.Execute(ctx, ctx.Player);
                }
            }

            // Tick enemy cooldowns
            foreach (var e in ctx.Enemies.Where(e => e))
                foreach (var rt in e.EnsureEnemyAbilityRuntimes(enemyAbilityDb))
                    rt?.TickCooldown();

            RefreshAbilityButtons();
            RyftCombatEvents.RaiseTurnEnd();
            if (turnBanner) turnBanner.Show("Player Turn");
            RyftCombatEvents.RaiseTurnStart();
        }

        // ------------------- helpers for UI buttons ----------------
        public bool IsAbilityReady(string id) =>
            runtimeById.TryGetValue(id, out var rt) && rt.IsReady;

        public int GetCooldownRemaining(string id) =>
            runtimeById.TryGetValue(id, out var rt) ? rt.CooldownRemaining : 0;

        private void RefreshAbilityButtons()
        {
            var buttons = FindObjectsOfType<AbilityButton>(true);
            foreach (var b in buttons) b.RefreshFromController(this);
        }
    }
}
