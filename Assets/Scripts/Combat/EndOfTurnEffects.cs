using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Queues effects to trigger at the end of turn
    /// </summary>
    public class EndOfTurnEffects : MonoBehaviour
    {
        public static EndOfTurnEffects Instance { get; private set; }

        private readonly List<Action> playerTurnEndEffects = new List<Action>();
        private readonly List<Action> enemyTurnEndEffects = new List<Action>();

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
        /// Queue an effect to trigger at the end of the current player turn
        /// </summary>
        public void QueuePlayerTurnEnd(Action effect)
        {
            if (effect != null)
            {
                playerTurnEndEffects.Add(effect);
                Debug.Log($"[EndOfTurn] Queued player turn end effect (total: {playerTurnEndEffects.Count})");
            }
        }

        /// <summary>
        /// Queue an effect to trigger at the end of the current enemy turn
        /// </summary>
        public void QueueEnemyTurnEnd(Action effect)
        {
            if (effect != null)
            {
                enemyTurnEndEffects.Add(effect);
                Debug.Log($"[EndOfTurn] Queued enemy turn end effect (total: {enemyTurnEndEffects.Count})");
            }
        }

        /// <summary>
        /// Trigger all player turn end effects and clear the queue
        /// </summary>
        public void TriggerPlayerTurnEnd()
        {
            if (playerTurnEndEffects.Count > 0)
            {
                Debug.Log($"[EndOfTurn] Triggering {playerTurnEndEffects.Count} player turn end effects");
                foreach (var effect in playerTurnEndEffects)
                {
                    try
                    {
                        effect?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EndOfTurn] Error executing player turn end effect: {e.Message}");
                    }
                }
                playerTurnEndEffects.Clear();
            }
        }

        /// <summary>
        /// Trigger all enemy turn end effects and clear the queue
        /// </summary>
        public void TriggerEnemyTurnEnd()
        {
            if (enemyTurnEndEffects.Count > 0)
            {
                Debug.Log($"[EndOfTurn] Triggering {enemyTurnEndEffects.Count} enemy turn end effects");
                foreach (var effect in enemyTurnEndEffects)
                {
                    try
                    {
                        effect?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EndOfTurn] Error executing enemy turn end effect: {e.Message}");
                    }
                }
                enemyTurnEndEffects.Clear();
            }
        }

        /// <summary>
        /// Clear all queued effects
        /// </summary>
        public void ClearAll()
        {
            playerTurnEndEffects.Clear();
            enemyTurnEndEffects.Clear();
        }
    }
}
