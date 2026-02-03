using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Types of gadgets/drones that can be deployed
    /// </summary>
    public enum GadgetType
    {
        Drone,
        Turret,
        Mine,
        Shield,
        Cover,
    }

    /// <summary>
    /// A deployed gadget instance
    /// </summary>
    public class Gadget
    {
        public GadgetType Type;
        public IActor Owner;
        public int Power;
        public int Duration; // -1 = permanent until destroyed, 0 = expired, >0 = turns remaining
        public string SourceCardId;

        public Gadget(GadgetType type, IActor owner, int power, int duration, string sourceCardId)
        {
            Type = type;
            Owner = owner;
            Power = power;
            Duration = duration;
            SourceCardId = sourceCardId;
        }

        public bool TickDuration()
        {
            if (Duration > 0) Duration--;
            return Duration != 0;
        }
    }

    /// <summary>
    /// Manages deployed gadgets and drones
    /// </summary>
    public class GadgetManager : MonoBehaviour
    {
        public static GadgetManager Instance { get; private set; }

        private readonly List<Gadget> deployedGadgets = new List<Gadget>();

        public event Action<Gadget> OnGadgetDeployed;
        public event Action<Gadget> OnGadgetDestroyed;
        public event Action<Gadget> OnGadgetActivated;

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
        /// Deploy a new gadget
        /// </summary>
        public void DeployGadget(GadgetType type, IActor owner, int power, int duration = -1, string sourceCardId = "")
        {
            var gadget = new Gadget(type, owner, power, duration, sourceCardId);
            deployedGadgets.Add(gadget);

            Debug.Log($"[Gadget] {owner?.DisplayName} deployed {type} (power: {power}, duration: {duration})");

            OnGadgetDeployed?.Invoke(gadget);

            // Notify combat tracker
            var tracker = CombatEventTracker.Instance;
            tracker?.RecordGadgetActivation(owner);
        }

        /// <summary>
        /// Destroy a specific gadget
        /// </summary>
        public void DestroyGadget(Gadget gadget)
        {
            if (gadget == null) return;

            if (deployedGadgets.Remove(gadget))
            {
                Debug.Log($"[Gadget] Destroyed {gadget.Type} from {gadget.Owner?.DisplayName}");

                OnGadgetDestroyed?.Invoke(gadget);

                // Notify combat tracker
                var tracker = CombatEventTracker.Instance;
                tracker?.RecordGadgetDestruction();
            }
        }

        /// <summary>
        /// Destroy all gadgets owned by an actor
        /// </summary>
        public void DestroyAllGadgetsOwnedBy(IActor owner)
        {
            var toDestroy = new List<Gadget>();

            foreach (var gadget in deployedGadgets)
            {
                if (gadget.Owner == owner)
                {
                    toDestroy.Add(gadget);
                }
            }

            foreach (var gadget in toDestroy)
            {
                DestroyGadget(gadget);
            }
        }

        /// <summary>
        /// Destroy all gadgets of a specific type
        /// </summary>
        public void DestroyAllGadgetsOfType(GadgetType type)
        {
            var toDestroy = new List<Gadget>();

            foreach (var gadget in deployedGadgets)
            {
                if (gadget.Type == type)
                {
                    toDestroy.Add(gadget);
                }
            }

            foreach (var gadget in toDestroy)
            {
                DestroyGadget(gadget);
            }
        }

        /// <summary>
        /// Get all gadgets owned by an actor
        /// </summary>
        public List<Gadget> GetGadgetsOwnedBy(IActor owner)
        {
            var result = new List<Gadget>();
            foreach (var gadget in deployedGadgets)
            {
                if (gadget.Owner == owner)
                {
                    result.Add(gadget);
                }
            }
            return result;
        }

        /// <summary>
        /// Get all gadgets of a specific type
        /// </summary>
        public List<Gadget> GetGadgetsOfType(GadgetType type)
        {
            var result = new List<Gadget>();
            foreach (var gadget in deployedGadgets)
            {
                if (gadget.Type == type)
                {
                    result.Add(gadget);
                }
            }
            return result;
        }

        /// <summary>
        /// Count gadgets owned by an actor
        /// </summary>
        public int CountGadgetsOwnedBy(IActor owner)
        {
            int count = 0;
            foreach (var gadget in deployedGadgets)
            {
                if (gadget.Owner == owner) count++;
            }
            return count;
        }

        /// <summary>
        /// Tick all gadget durations
        /// </summary>
        public void TickAllGadgets()
        {
            var toDestroy = new List<Gadget>();

            foreach (var gadget in deployedGadgets)
            {
                if (!gadget.TickDuration())
                {
                    toDestroy.Add(gadget);
                }
            }

            foreach (var gadget in toDestroy)
            {
                DestroyGadget(gadget);
            }
        }

        /// <summary>
        /// Clear all gadgets
        /// </summary>
        public void ClearAll()
        {
            deployedGadgets.Clear();
        }

        /// <summary>
        /// Get all deployed gadgets
        /// </summary>
        public IReadOnlyList<Gadget> GetAllGadgets() => deployedGadgets;
    }
}
