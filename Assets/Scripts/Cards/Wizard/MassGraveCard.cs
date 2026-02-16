using Game.Core;
using Game.Combat;
using UnityEngine;

namespace Game.Cards.Wizard
{
    public class MassGraveCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            int totalCursed = CombatBuffManager.GetTotalDebuffStacksOnAllEnemies(ctx, StatusEffectType.Cursed);
            int baseDmg = 2 * totalCursed + GetStatValue();
            int dmg = GetFinalDamage(Mathf.Max(1, baseDmg));
            int kills = DealDamageToAll(ctx, dmg);

            ctx.Log($"{Owner.DisplayName} uses Mass Grave for {dmg} damage to all enemies ({totalCursed} total Cursed stacks). {kills} killed.");
            FightSceneController.Instance?.TrackAttackCardPlayed();
        }
    }
}
