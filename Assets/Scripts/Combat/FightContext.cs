// Assets/Scripts/Combat/FightContext.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;      // IActor
using Game.Player;    // PlayerCharacter : IActor
using Game.Enemies;   // EnemyBase : IActor
using Game.RyftEntities; // RyftPortalEntity

namespace Game.Combat
{
    /// Lightweight context abilities/enemies can use to find targets & log.
    public class FightContext
    {
        public PlayerCharacter Player { get; }
        public List<EnemyBase> Enemies { get; }
        public RyftPortalEntity RyftPortal { get; private set; }

        public IActor PlayerActor => Player;
        public IEnumerable<IActor> EnemyActors      => Enemies.Where(e => e).Cast<IActor>();
        public IEnumerable<IActor> AliveEnemyActors => Enemies.Where(e => e && e.IsAlive).Cast<IActor>();

        private readonly Action<string> _logger;

        public FightContext(PlayerCharacter player, List<EnemyBase> enemies, Action<string> logger = null)
        {
            Player  = player;
            Enemies = enemies ?? new List<EnemyBase>();
            _logger = logger ?? (msg => Debug.Log($"[Combat] {msg}"));
        }

        public FightContext(PlayerCharacter player, List<EnemyBase> enemies, RyftPortalEntity portal, Action<string> logger = null)
            : this(player, enemies, logger)
        {
            RyftPortal = portal;
        }

        /// <summary>
        /// Sets the rift portal for this fight context.
        /// </summary>
        public void SetRyftPortal(RyftPortalEntity portal)
        {
            RyftPortal = portal;
        }

        /// <summary>
        /// Gets the primary target for enemies in portal fight mode.
        /// Returns the portal if it exists and is alive, otherwise falls back to player.
        /// </summary>
        public IActor GetEnemyPrimaryTarget()
        {
            if (RyftPortal != null && RyftPortal.IsAlive)
                return RyftPortal;
            return Player;
        }

        public void Log(string msg) => _logger?.Invoke(msg);

        public IActor FirstAliveEnemy() => Enemies.FirstOrDefault(e => e && e.IsAlive);
        public bool AllEnemiesDead()    => Enemies.All(e => e == null || !e.IsAlive);

        public void RegisterEnemy(EnemyBase e)
        {
            if (e != null && !Enemies.Contains(e)) Enemies.Add(e);
        }

        /// <summary>
        /// Add a newly spawned enemy to the combat context.
        /// </summary>
        public void AddEnemy(EnemyBase e)
        {
            RegisterEnemy(e);
        }

        public void CleanupNulls()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
                if (Enemies[i] == null) Enemies.RemoveAt(i);
        }
        public IEnumerable<IActor> AllAliveEnemies() => FightSceneController.Instance.AllAliveEnemies();

        public void OnPlayerDamagedBy(IActor attacker, int damage)
        {
            var fsc = FightSceneController.Instance;
            if (fsc != null) fsc.OnPlayerDamagedBy(attacker, damage, this);
        }
    }
}
