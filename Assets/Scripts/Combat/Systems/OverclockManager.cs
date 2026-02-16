using UnityEngine;
using Game.Player;

namespace Game.Combat
{
    /// <summary>
    /// Manages Overclock stacks for the Cyborg Engineer subclass.
    /// +5% damage per stack. At 20 stacks → extra turn + reset to 0.
    /// </summary>
    public class OverclockManager : MonoBehaviour
    {
        public static OverclockManager Instance { get; private set; }

        public int Stacks { get; private set; } = 0;
        public float DamageBonusPercent => Stacks * 0.05f;

        void Awake() => Instance = this;

        public void AddStacks(int amount)
        {
            if (amount <= 0) return;
            Stacks += amount;
            Debug.Log($"[Overclock] +{amount} stacks → {Stacks} total ({DamageBonusPercent:P0} bonus)");

            if (Stacks >= 20)
            {
                Debug.Log("[Overclock] 20 stacks reached! Extra turn + reset.");
                Stacks = 0;
                SyncToStatusEffects();
                FightSceneController.Instance?.QueueExtraTurn();
                return;
            }

            SyncToStatusEffects();
        }

        public void DoubleStacks()
        {
            int prev = Stacks;
            Stacks *= 2;
            Debug.Log($"[Overclock] Doubled: {prev} → {Stacks}");

            if (Stacks >= 20)
            {
                Debug.Log("[Overclock] 20 stacks reached! Extra turn + reset.");
                Stacks = 0;
                SyncToStatusEffects();
                FightSceneController.Instance?.QueueExtraTurn();
                return;
            }

            SyncToStatusEffects();
        }

        public int RemoveAllStacks()
        {
            int removed = Stacks;
            Stacks = 0;
            SyncToStatusEffects();
            Debug.Log($"[Overclock] Removed all {removed} stacks.");
            return removed;
        }

        public void SetStacks(int value)
        {
            Stacks = Mathf.Max(0, value);
            SyncToStatusEffects();
        }

        private void SyncToStatusEffects()
        {
            var player = FindObjectOfType<PlayerCharacter>();
            if (player == null) return;
            player.StatusEffects.SetStacks(StatusEffectType.Overclock, Stacks);
        }
    }
}
