using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Fighter
{
    public class EndlessFuryCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            var target = explicitTarget ?? ctx.FirstAliveEnemy();
            if (target == null) return;

            int dmg = GetFinalDamage();
            bool killed = DealDamageTracked(target, dmg);

            // Second hit (if target is still alive, hit again; otherwise pick next alive enemy)
            if (killed)
            {
                HandleMomentum(killed);
                var next = ctx.FirstAliveEnemy();
                if (next != null)
                {
                    int dmg2 = GetFinalDamage();
                    bool killed2 = DealDamageTracked(next, dmg2);
                    HandleMomentum(killed2);
                    ctx.Log($"{Owner.DisplayName} uses Endless Fury for {dmg}+{dmg2} damage (redirected second hit).");
                }
                else
                {
                    ctx.Log($"{Owner.DisplayName} uses Endless Fury for {dmg} damage (target killed, no enemies remain).");
                }
            }
            else
            {
                int dmg2 = GetFinalDamage();
                killed = DealDamageTracked(target, dmg2);
                ctx.Log($"{Owner.DisplayName} uses Endless Fury for {dmg}+{dmg2} damage.");
                HandleMomentum(killed);
            }
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
