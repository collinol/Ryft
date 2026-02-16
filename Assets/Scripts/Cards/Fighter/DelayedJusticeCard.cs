using Game.Core;
using Game.Combat;
using UnityEngine;
using System.Linq;

namespace Game.Cards.Fighter
{
    public class DelayedJusticeCard : CardRuntime
    {
        public override void Execute(FightContext ctx, IActor explicitTarget = null)
        {
            if (!CanUse(ctx)) return;
            if (!TryPayEnergy()) return;

            GainProtection(10);

            var fsc = FightSceneController.Instance;
            if (fsc == null) return;

            if (fsc.DelayedJusticeDamage > 0)
            {
                int storedDmg = fsc.DelayedJusticeDamage;
                fsc.DelayedJusticeDamage = 0;
                int kills = DealDamageToAll(ctx, storedDmg);
                ctx.Log($"{Owner.DisplayName} uses Delayed Justice, gains 10 protection and deals {storedDmg} stored damage to all enemies ({kills} killed).");
            }
            else
            {
                int currentProt = CombatBuffManager.GetProtection(Owner);
                fsc.DelayedJusticeDamage = currentProt;
                ctx.Log($"{Owner.DisplayName} uses Delayed Justice, gains 10 protection and stores {currentProt} protection as pending damage.");
            }
        }
    }
}
