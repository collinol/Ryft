// Game/Ryfts/OrangeClosedOnKillResetRandomCD.cs
using UnityEngine;

namespace Game.Ryfts
{
    // On enemy defeated: chance to refresh player stats.
    // - Heals player to full
    // - Clears negative temporary modifiers (MaxHp/Strength/Defense) back to zero
    public class OrangeClosedOnKillResetRandomCD : RyftEffectRuntime
    {
        public override void HandleTrigger(RyftEffectManager mgr, RyftEffectContext ctx)
        {
            if (ctx.trigger != RyftTrigger.OnEnemyDefeated) return;
            if (!ShouldProc()) return;

            var player = mgr.PlayerActor;
            if (player != null && player.IsAlive)
            {

                int fixedMax   = 0;
                int fixedStr   = 0;
                int fixedDef   = 0;

                if (mgr.TempMaxHp < 0)   { fixedMax = -mgr.TempMaxHp; mgr.AddTempMaxHp(+fixedMax); }
                if (mgr.TempStrength < 0){ fixedStr = -mgr.TempStrength; mgr.AddTempStrength(+fixedStr); }
                if (mgr.TempDefense < 0) { fixedDef = -mgr.TempDefense; mgr.AddTempDefense(+fixedDef); }

                // If max HP increased due to removing a negative temp, top off to new max.
                int newMax = Mathf.Max(1, player.TotalStats.maxHealth);
                int topOff = Mathf.Max(0, newMax - player.Health);
                if (topOff > 0) player.Heal(topOff);

                mgr.DebugLogEffectAction(
                    "RefreshStatsOnKill",
                    $"{Def?.id} healed to full and cleared negatives: +MaxHp {fixedMax}, +Str {fixedStr}, +Def {fixedDef}"
                );
            }

            StartInternalCooldown();
        }
    }
}
