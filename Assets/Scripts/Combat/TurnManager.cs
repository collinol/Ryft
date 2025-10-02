// Assets/Scripts/Combat/TurnManager.cs
using System;
using UnityEngine;

namespace Game.Combat
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public enum Phase { PlayerTurn, EnemyTurn }
        public Phase CurrentPhase { get; private set; } = Phase.PlayerTurn;

        public event Action OnPlayerTurnStarted;
        public event Action OnPlayerTurnEnded;
        public event Action OnEnemyTurnStarted;
        public event Action OnEnemyTurnEnded;

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartPlayerTurn()
        {
            CurrentPhase = Phase.PlayerTurn;
            OnPlayerTurnStarted?.Invoke();
        }

        public void EndPlayerTurn()
        {
            OnPlayerTurnEnded?.Invoke();
            CurrentPhase = Phase.EnemyTurn;
            OnEnemyTurnStarted?.Invoke();
        }

        public void EndEnemyTurn()
        {
            OnEnemyTurnEnded?.Invoke();
            StartPlayerTurn();
        }
    }
}
