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

        void Awake()
        {
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


            RefreshAbilityButtons();
        }


        // ------------------- UI hooks -------------------

        public void UsePlayerAbility(string abilityId)
        {
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
                    runtime.Execute(ctx, player);
                    RefreshAbilityButtons();
                    break;

                case TargetingType.SingleEnemy:
                    pendingTargetedAbility = runtime;
                    ctx.Log("Click an enemy to target.");
                    break;

                case TargetingType.AllEnemies:
                    foreach (var e in ctx.Enemies.Where(e => e && e.IsAlive))
                        runtime.Execute(ctx, e);
                    RefreshAbilityButtons();
                    break;
            }
        }

        public void OnEnemyClicked(EnemyBase enemy)
        {
            if (!enemy || !enemy.IsAlive) return;
            if (pendingTargetedAbility != null)
            {
                pendingTargetedAbility.Execute(ctx, enemy);
                pendingTargetedAbility = null;
                RefreshAbilityButtons();
            }
        }

        // ------------------- Turn flow -------------------

        public void EndPlayerTurnButton()
        {
            foreach (var rt in runtimeById.Values) rt.TickCooldown();
            RefreshAbilityButtons();

            if (turnBanner) turnBanner.Show("Enemy Turn");
            StartCoroutine(EnemyTurnThenBackToPlayer());
        }

        private IEnumerator EnemyTurnThenBackToPlayer()
        {
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
            if (turnBanner) turnBanner.Show("Player Turn");
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
