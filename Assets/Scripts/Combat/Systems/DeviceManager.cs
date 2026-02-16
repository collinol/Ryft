using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public enum DeviceTrigger
    {
        StartOfTurn,   // Triggers at start of player turn
        OnEnemyAttack, // Triggers when an enemy attacks
    }

    /// <summary>
    /// Represents a deployed device on the battlefield.
    /// </summary>
    public class Device
    {
        public string Name;
        public int CurrentHP;
        public int MaxHP;
        public int OriginalCost;
        public DeviceTrigger Trigger;
        public System.Action<FightContext> OnTrigger;
        public bool IsAlive => CurrentHP > 0;

        // For Overcharged Capacitor: provides flat bonus damage
        public int BonusFlatDamage;
        // For Decoy Hologram: acts as taunt
        public bool IsDecoy;

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            if (CurrentHP <= 0)
                Debug.Log($"[Device] {Name} destroyed!");
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }

        public void HealToFull()
        {
            CurrentHP = MaxHP;
        }
    }

    /// <summary>
    /// Manages deployed devices for the Engineer class.
    /// Devices persist across turns, have HP, and trigger effects.
    /// </summary>
    public class DeviceManager : MonoBehaviour
    {
        public static DeviceManager Instance { get; private set; }

        private readonly List<Device> devices = new();

        public IReadOnlyList<Device> Devices => devices;
        public int DeviceCount => devices.Count(d => d.IsAlive);

        void Awake() => Instance = this;

        public Device DeployDevice(string name, int hp, int cost, DeviceTrigger trigger,
            System.Action<FightContext> onTrigger)
        {
            var device = new Device
            {
                Name = name,
                CurrentHP = hp,
                MaxHP = hp,
                OriginalCost = cost,
                Trigger = trigger,
                OnTrigger = onTrigger
            };
            devices.Add(device);
            Debug.Log($"[Device] Deployed {name} (HP: {hp}, Trigger: {trigger})");
            return device;
        }

        /// <summary>Called at start of player turn.</summary>
        public void TickTurnStart(FightContext ctx)
        {
            // Clean up dead devices first
            devices.RemoveAll(d => !d.IsAlive);

            foreach (var device in devices.ToList())
            {
                if (device.IsAlive && device.Trigger == DeviceTrigger.StartOfTurn)
                {
                    Debug.Log($"[Device] {device.Name} triggers start-of-turn effect");
                    device.OnTrigger?.Invoke(ctx);
                }
            }
        }

        /// <summary>Called when an enemy attacks.</summary>
        public void OnEnemyAttack(FightContext ctx)
        {
            foreach (var device in devices.ToList())
            {
                if (device.IsAlive && device.Trigger == DeviceTrigger.OnEnemyAttack)
                {
                    Debug.Log($"[Device] {device.Name} triggers on-enemy-attack effect");
                    device.OnTrigger?.Invoke(ctx);
                }
            }
        }

        /// <summary>Trigger all start-of-turn devices immediately (Network Link).</summary>
        public void TriggerAllStartOfTurn(FightContext ctx)
        {
            foreach (var device in devices.ToList())
            {
                if (device.IsAlive && device.Trigger == DeviceTrigger.StartOfTurn)
                {
                    device.OnTrigger?.Invoke(ctx);
                }
            }
        }

        public void DestroyDevice(Device device)
        {
            device.CurrentHP = 0;
            devices.Remove(device);
            Debug.Log($"[Device] {device.Name} destroyed");
        }

        public void DestroyAllDevices()
        {
            int count = devices.Count;
            devices.Clear();
            Debug.Log($"[Device] All {count} devices destroyed");
        }

        /// <summary>Get total flat bonus damage from all alive devices.</summary>
        public int GetTotalBonusDamage()
        {
            int total = 0;
            foreach (var d in devices)
                if (d.IsAlive) total += d.BonusFlatDamage;
            return total;
        }

        /// <summary>Check if there's a decoy device alive.</summary>
        public bool HasDecoy()
        {
            return devices.Any(d => d.IsAlive && d.IsDecoy);
        }

        /// <summary>Get a random alive device.</summary>
        public Device GetRandomDevice()
        {
            var alive = devices.Where(d => d.IsAlive).ToList();
            if (alive.Count == 0) return null;
            return alive[Random.Range(0, alive.Count)];
        }

        /// <summary>Heal all devices to full HP.</summary>
        public void HealAllDevices()
        {
            foreach (var d in devices)
                if (d.IsAlive) d.HealToFull();
        }

        /// <summary>Add max HP and heal all devices.</summary>
        public void FortifyDevices(int bonusHP)
        {
            foreach (var d in devices)
            {
                if (d.IsAlive)
                {
                    d.MaxHP += bonusHP;
                    d.Heal(bonusHP);
                }
            }
        }

        /// <summary>Duplicate a random alive device.</summary>
        public Device DuplicateRandom()
        {
            var source = GetRandomDevice();
            if (source == null) return null;

            var copy = new Device
            {
                Name = source.Name + " (Copy)",
                CurrentHP = source.MaxHP,
                MaxHP = source.MaxHP,
                OriginalCost = source.OriginalCost,
                Trigger = source.Trigger,
                OnTrigger = source.OnTrigger,
                BonusFlatDamage = source.BonusFlatDamage,
                IsDecoy = source.IsDecoy
            };
            devices.Add(copy);
            Debug.Log($"[Device] Duplicated {source.Name}");
            return copy;
        }

        /// <summary>Count alive devices and return as int (for Chain Reaction).</summary>
        public int DestroyAllAndCount()
        {
            int count = devices.Count(d => d.IsAlive);
            devices.Clear();
            return count;
        }
    }
}
