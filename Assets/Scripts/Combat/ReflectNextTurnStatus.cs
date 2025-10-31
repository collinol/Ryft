using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    /// <summary>
    /// Keeps track of reflect that activates on the next enemy turn.
    /// - Grant(owner, pct, ctx) to schedule the reflect.
    /// - TryReflect(defender, attacker, incomingDamage, ctx) should be called
    ///   right after attacker damages defender during the next enemy turn.
    /// The reflect is one-shot and then consumed.
    /// </summary>
    public static class ReflectNextTurnStatus
    {
        private class Entry
        {
            public float percent;     // 0..1
            public int activateTurn;  // the enemy-turn index at which this becomes active
            public bool consumed;
        }

        // Track per-actor reflect entries
        private static readonly Dictionary<IActor, Entry> _pending = new();

        /// <summary>
        /// Schedules reflect to become active on the NEXT enemy turn.
        /// </summary>
        public static void Grant(IActor owner, float percent, FightContext ctx)
        {
            if (owner == null) return;
            if (!_pending.TryGetValue(owner, out var e))
                e = _pending[owner] = new Entry();

            e.percent  = Mathf.Clamp01(percent);
            e.consumed = false;

            int cur = FightSceneController.Instance ? FightSceneController.Instance.EnemyTurnIndex : 0;
            e.activateTurn = cur + 1;
        }

        /// <summary>
        /// Call right after an enemy damages defender. If a reflect is ready, it fires once.
        /// </summary>
        public static void TryReflect(IActor defender, IActor attacker, int incomingDamage, FightContext ctx)
        {
            if (defender == null || attacker == null) return;
            if (!_pending.TryGetValue(defender, out var e)) return;
            if (e.consumed) return;

            int cur = FightSceneController.Instance ? FightSceneController.Instance.EnemyTurnIndex : 0;
            if (cur < e.activateTurn) return; // not active yet

            int reflectDmg = Mathf.RoundToInt(incomingDamage * e.percent);
            if (reflectDmg > 0 && attacker.IsAlive)
            {
                attacker.ApplyDamage(reflectDmg);
                ctx.Log($"{defender.DisplayName} reflects {reflectDmg} damage back to {attacker.DisplayName}!");
            }
            e.consumed = true;
        }

        /// <summary>
        /// Optional: clear at end of battle.
        /// </summary>
        public static void ClearAll() => _pending.Clear();
    }
}
