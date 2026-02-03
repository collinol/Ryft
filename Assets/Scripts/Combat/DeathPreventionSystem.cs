using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Types of death prevention effects
    /// </summary>
    public enum DeathPreventionType
    {
        LastStand,          // Restore to 1 HP and double Strength
        Rebirth,            // Restore to 50% HP
        PhoenixForm,        // Restore to full HP and double spell power
        FailsafeProtocol,   // Engineering - restore to full HP and restore drones
    }

    /// <summary>
    /// A registered death prevention effect
    /// </summary>
    public class DeathPrevention
    {
        public DeathPreventionType Type;
        public bool Used;
        public string SourceId;
        public Action<IActor> OnTrigger;

        public DeathPrevention(DeathPreventionType type, string sourceId, Action<IActor> onTrigger = null)
        {
            Type = type;
            SourceId = sourceId;
            OnTrigger = onTrigger;
            Used = false;
        }
    }

    /// <summary>
    /// Manages death prevention effects (like Rebirth, Phoenix Form, Last Stand)
    /// </summary>
    public class DeathPreventionSystem : MonoBehaviour
    {
        public static DeathPreventionSystem Instance { get; private set; }

        private readonly Dictionary<IActor, List<DeathPrevention>> actorPreventions = new Dictionary<IActor, List<DeathPrevention>>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Register a death prevention effect for an actor
        /// </summary>
        public void RegisterPrevention(IActor actor, DeathPreventionType type, string sourceId, Action<IActor> onTrigger = null)
        {
            if (actor == null) return;

            if (!actorPreventions.ContainsKey(actor))
            {
                actorPreventions[actor] = new List<DeathPrevention>();
            }

            var prevention = new DeathPrevention(type, sourceId, onTrigger);
            actorPreventions[actor].Add(prevention);

            Debug.Log($"[DeathPrevention] Registered {type} for {actor.DisplayName} (source: {sourceId})");
        }

        /// <summary>
        /// Check if actor has an active prevention and trigger it if dying
        /// Returns the new health value, or -1 if no prevention was triggered
        /// </summary>
        public int TryPreventDeath(IActor actor, int currentHealth)
        {
            if (actor == null || !actorPreventions.ContainsKey(actor)) return -1;

            var preventions = actorPreventions[actor];

            // Find first unused prevention
            foreach (var prev in preventions)
            {
                if (!prev.Used)
                {
                    prev.Used = true;
                    return TriggerPrevention(actor, prev, currentHealth);
                }
            }

            return -1;
        }

        private int TriggerPrevention(IActor actor, DeathPrevention prevention, int currentHealth)
        {
            Debug.Log($"[DeathPrevention] {prevention.Type} triggered for {actor.DisplayName}!");

            int newHealth = currentHealth;

            switch (prevention.Type)
            {
                case DeathPreventionType.LastStand:
                    newHealth = 1;
                    // Double Strength handled by the card's OnTrigger callback
                    break;

                case DeathPreventionType.Rebirth:
                    newHealth = Mathf.Max(1, actor.TotalStats.maxHealth / 2);
                    break;

                case DeathPreventionType.PhoenixForm:
                    newHealth = actor.TotalStats.maxHealth;
                    // Double spell power handled by the card's OnTrigger callback
                    break;

                case DeathPreventionType.FailsafeProtocol:
                    newHealth = actor.TotalStats.maxHealth;
                    // Restore drones handled by the card's OnTrigger callback
                    break;
            }

            // Execute custom trigger logic
            prevention.OnTrigger?.Invoke(actor);

            return newHealth;
        }

        /// <summary>
        /// Clear all preventions for an actor (e.g., when battle ends)
        /// </summary>
        public void ClearActor(IActor actor)
        {
            if (actor != null)
            {
                actorPreventions.Remove(actor);
            }
        }

        /// <summary>
        /// Clear all preventions
        /// </summary>
        public void ClearAll()
        {
            actorPreventions.Clear();
        }

        /// <summary>
        /// Check if actor has an active (unused) death prevention
        /// </summary>
        public bool HasActivePrevention(IActor actor)
        {
            if (actor == null || !actorPreventions.ContainsKey(actor)) return false;

            foreach (var prev in actorPreventions[actor])
            {
                if (!prev.Used) return true;
            }
            return false;
        }
    }
}
