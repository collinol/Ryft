using Game.Core;
using Game.Combat;
using System.Linq;
using UnityEngine;

namespace Game.Cards.Wizard
{
    /// <summary>
    /// Meteor - Deal 15+stat damage to all enemies. Apply 3 Burning to all enemies.
    /// </summary>
    public class MeteorCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int dmg = GetFinalDamage();
            DealDamageToAll(ctx, dmg);

            foreach (var enemy in ctx.AllAliveEnemies().ToList())
            {
                CombatBuffManager.ApplyDebuff(enemy, StatusEffectType.Burning, 3, Owner);
            }

            ctx.Log($"{Owner.DisplayName} uses Meteor for {dmg} damage to all enemies + 3 Burning each.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
