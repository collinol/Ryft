using UnityEngine;

namespace Game.Combat
{
    public enum StanceType { None, Rage, Defensive }

    /// <summary>
    /// Manages player stances (mutually exclusive).
    /// Rage: 2x outgoing damage, 2x incoming damage.
    /// Defensive: 0.5x outgoing damage, 0.5x incoming damage, +5 protection at turn start.
    /// </summary>
    public class StanceManager : MonoBehaviour
    {
        public static StanceManager Instance { get; private set; }

        public StanceType CurrentStance { get; private set; } = StanceType.None;
        public int RemainingTurns { get; private set; } = 0;

        public float OutgoingDamageMultiplier => CurrentStance switch
        {
            StanceType.Rage => 2.0f,
            StanceType.Defensive => 0.5f,
            _ => 1.0f
        };

        public float IncomingDamageMultiplier => CurrentStance switch
        {
            StanceType.Rage => 2.0f,
            StanceType.Defensive => 0.5f,
            _ => 1.0f
        };

        void Awake() => Instance = this;

        public void EnterStance(StanceType stance, int turns = -1)
        {
            if (CurrentStance != StanceType.None && CurrentStance != stance)
                Debug.Log($"[Stance] Exiting {CurrentStance}, entering {stance}");

            CurrentStance = stance;
            RemainingTurns = turns; // -1 = permanent until explicitly exited
            Debug.Log($"[Stance] Entered {stance} (turns={turns})");
        }

        public void ExitStance()
        {
            if (CurrentStance == StanceType.None) return;
            Debug.Log($"[Stance] Exited {CurrentStance}");
            CurrentStance = StanceType.None;
            RemainingTurns = 0;
        }

        public bool IsInStance(StanceType stance) => CurrentStance == stance;

        /// <summary>Called at start of player turn.</summary>
        public void TickTurnStart()
        {
            // Defensive stance grants +5 protection at turn start
            if (CurrentStance == StanceType.Defensive)
            {
                var player = FightSceneController.Instance?.GetPlayer();
                if (player != null)
                {
                    CombatBuffManager.GainProtection(player, 5);
                    Debug.Log("[Stance] Defensive Stance: +5 protection");
                }
            }

            // Tick down duration
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
                if (RemainingTurns <= 0)
                {
                    Debug.Log($"[Stance] {CurrentStance} expired");
                    CurrentStance = StanceType.None;
                }
            }
        }
    }
}
