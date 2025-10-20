// Game/Ryfts/OrangeClosedOpeningVolley.cs
using UnityEngine;
using System.Linq;

namespace Game.Ryfts
{
    // At battle start: deal flat damage to a random enemy.
    public class OrangeClosedOpeningVolley : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnBattleStart) return;
            if (ctx.fight == null) return;

            int dmg = Mathf.Max(1, Def.intMagnitude);
            var enemies = ctx.fight.Enemies.Where(e => e && e.IsAlive).ToList();
            if (enemies.Count == 0) return;

            var t = enemies[Random.Range(0, enemies.Count)];
            int before = t.Health;
            t.ApplyDamage(dmg);
            int dealt = Mathf.Max(0, before - t.Health);
            if (dealt > 0) RyftCombatEvents.RaiseDamageDealt(null, t, dealt);
            if (!t.IsAlive) RyftCombatEvents.RaiseEnemyDefeated(t);
        }
    }
}
